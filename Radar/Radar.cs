using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Helpers;
using GameOffsets;
using GameOffsets.Native;
using ImGuiNET;
using Newtonsoft.Json;
using SharpDX;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Convolution;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace Radar;

public class Radar : BaseSettingsPlugin<RadarSettings> {
    private const string TextureName = "radar_minimap";

    private const int TileToGridConversion = 23;

    private const int TileToWorldConversion = 250;

    public const float GridToWorldMultiplier = 10.869565f;

    private const double CameraAngle = 0.6754424205218056;

    private static readonly float CameraAngleCos = (float)Math.Cos(0.6754424205218056);

    private static readonly float CameraAngleSin = (float)Math.Sin(0.6754424205218056);

    private double _mapScale;

    private ConcurrentDictionary<string, List<TargetDescription>> _targetDescriptions = new ConcurrentDictionary<string, List<TargetDescription>>();

    private Vector2i? _areaDimensions;

    private TerrainData _terrainMetadata;

    private float[][] _heightData;

    private int[][] _processedTerrainData;

    private Dictionary<string, TargetDescription> _targetDescriptionsInArea = new Dictionary<string, TargetDescription>();

    private HashSet<string> _currentZoneTargetEntityPaths = new HashSet<string>();

    private CancellationTokenSource _findPathsCts = new CancellationTokenSource();

    private ConcurrentDictionary<string, TargetLocations> _clusteredTargetLocations = new ConcurrentDictionary<string, TargetLocations>();

    private ConcurrentDictionary<string, List<Vector2i>> _allTargetLocations = new ConcurrentDictionary<string, List<Vector2i>>();

    private ConcurrentDictionary<System.Numerics.Vector2, RouteDescription> _routes = new ConcurrentDictionary<System.Numerics.Vector2, RouteDescription>();

    private static readonly List<SharpDX.Color> RainbowColors = new List<SharpDX.Color>
    {
        SharpDX.Color.Red,
        SharpDX.Color.Green,
        SharpDX.Color.Blue,
        SharpDX.Color.Yellow,
        SharpDX.Color.Violet,
        SharpDX.Color.Orange,
        SharpDX.Color.White,
        SharpDX.Color.LightBlue,
        SharpDX.Color.Indigo
    };

    private SharpDX.RectangleF _rect;

    private ImDrawListPtr _backGroundWindowPtr;

    public override void AreaChange(AreaInstance area) {
        StopPathFinding();
        if (base.GameController.Game.IsInGameState || base.GameController.Game.IsEscapeState) {
            _targetDescriptionsInArea = GetTargetDescriptionsInArea().ToDictionary((TargetDescription x) => x.Name);
            _currentZoneTargetEntityPaths = (from x in _targetDescriptionsInArea.Values
                                             where x.TargetType == TargetType.Entity
                                             select x.Name).ToHashSet();
            _terrainMetadata = base.GameController.IngameState.Data.DataStruct.Terrain;
            _heightData = base.GameController.IngameState.Data.RawTerrainHeightData;
            _allTargetLocations = GetTargets();
            _areaDimensions = base.GameController.IngameState.Data.AreaDimensions;
            _processedTerrainData = base.GameController.IngameState.Data.RawPathfindingData;
            GenerateMapTexture();
            _clusteredTargetLocations = ClusterTargets();
            StartPathFinding();
        }
    }

    public override void OnLoad() {
        LoadTargets();
        base.Settings.Reload.OnPressed = delegate {
            Core.MainRunner.Run(new Coroutine(delegate {
                LoadTargets();
                AreaChange(base.GameController.Area.CurrentArea);
            }, new WaitTime(0), this, "RestartPathfinding", infinity: false));
        };
        base.Settings.MaximumPathCount.OnValueChanged += delegate {
            Core.MainRunner.Run(new Coroutine(RestartPathFinding, new WaitTime(0), this, "RestartPathfinding", infinity: false));
        };
        base.Settings.TerrainColor.OnValueChanged += delegate {
            GenerateMapTexture();
        };
        base.Settings.Debug.DrawHeightMap.OnValueChanged += delegate {
            GenerateMapTexture();
        };
        base.Settings.Debug.SkipEdgeDetector.OnValueChanged += delegate {
            GenerateMapTexture();
        };
        base.Settings.Debug.SkipNeighborFill.OnValueChanged += delegate {
            GenerateMapTexture();
        };
        base.Settings.Debug.SkipRecoloring.OnValueChanged += delegate {
            GenerateMapTexture();
        };
        base.Settings.Debug.DisableHeightAdjust.OnValueChanged += delegate {
            GenerateMapTexture();
        };
        base.Settings.MaximumMapTextureDimension.OnValueChanged += delegate {
            GenerateMapTexture();
        };
    }

    public override void EntityAdded(Entity entity) {
        Positioned positioned = entity.GetComponent<Positioned>();
        if (positioned == null) {
            return;
        }
        string path = entity.Path;
        if (!_currentZoneTargetEntityPaths.Contains(path)) {
            return;
        }
        bool alreadyContains = false;
        _allTargetLocations.AddOrUpdate(path, (string _) => new List<Vector2i> { positioned.GridPosNum.Truncate() }, (string _, List<Vector2i> l) => (!(alreadyContains = l.Contains(positioned.GridPosNum.Truncate()))) ? l.Append(positioned.GridPosNum.Truncate()).ToList() : l);
        if (!alreadyContains) {
            TargetLocations oldValue = _clusteredTargetLocations.GetValueOrDefault(path);
            TargetLocations newValue = _clusteredTargetLocations.AddOrUpdate(path, (string _) => ClusterTarget(_targetDescriptionsInArea[path]), (string _, TargetLocations _) => ClusterTarget(_targetDescriptionsInArea[path]));
            if (oldValue == null || !newValue.Locations.ToHashSet().SetEquals(oldValue.Locations)) {
                RestartPathFinding();
            }
        }
    }

    private System.Numerics.Vector2 GetPlayerPosition() {
        Positioned playerPositionComponent = base.GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Positioned>();
        if (playerPositionComponent == null) {
            return new System.Numerics.Vector2(0f, 0f);
        }
        return new System.Numerics.Vector2(playerPositionComponent.GridX, playerPositionComponent.GridY);
    }

    public override void Render() {
        IngameUIElements ingameUi = base.GameController.Game.IngameState.IngameUi;
        if (!base.Settings.Debug.IgnoreFullscreenPanels && (ingameUi.DelveWindow.IsVisible || ingameUi.Atlas.IsVisible || ingameUi.SellWindow.IsVisible)) {
            return;
        }
        SharpDX.RectangleF windowRectangle = base.GameController.Window.GetWindowRectangle();
        windowRectangle.Location = System.Numerics.Vector2.Zero.ToSharpDx();
        _rect = windowRectangle;
        if (!base.Settings.Debug.DisableDrawRegionLimiting) {
            if (ingameUi.OpenRightPanel.IsVisible) {
                _rect.Right = ingameUi.OpenRightPanel.GetClientRectCache.Left;
            }
            if (ingameUi.OpenLeftPanel.IsVisible) {
                _rect.Left = ingameUi.OpenLeftPanel.GetClientRectCache.Right;
            }
        }
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(_rect.Width, _rect.Height));
        ImGui.SetNextWindowPos(new System.Numerics.Vector2(_rect.Left, _rect.Top));
        ImGui.Begin("radar_background", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus);
        _backGroundWindowPtr = ImGui.GetWindowDrawList();
        SubMap largeMap = ingameUi.Map.LargeMap.AsObject<SubMap>();
        if (largeMap.IsVisible) {
            System.Numerics.Vector2 mapCenter = largeMap.GetClientRect().TopLeft.ToVector2Num() + largeMap.ShiftNum + largeMap.DefaultShiftNum + new System.Numerics.Vector2((int)base.Settings.Debug.MapCenterOffsetX, (int)base.Settings.Debug.MapCenterOffsetY);
            _mapScale = (float)base.GameController.IngameState.Camera.Height / 677f * largeMap.Zoom * (float)base.Settings.CustomScale;
            DrawLargeMap(mapCenter);
            DrawTargets(mapCenter);
        }
        DrawWorldPaths(largeMap);
        ImGui.End();
    }

    private void DrawWorldPaths(SubMap largeMap) {
        if (!base.Settings.PathfindingSettings.WorldPathSettings.ShowPathsToTargets || (largeMap.IsVisible && (bool)base.Settings.PathfindingSettings.WorldPathSettings.ShowPathsToTargetsOnlyWithClosedMap)) {
            return;
        }
        Render playerRender = base.GameController.Game.IngameState.Data.LocalPlayer?.GetComponent<Render>();
        if (playerRender == null) {
            return;
        }
        Camera camera = base.GameController.IngameState.Camera;
        System.Numerics.Vector3 posNum = playerRender.PosNum;
        posNum.Z = playerRender.RenderStruct.Height;
        System.Numerics.Vector2 initPos = camera.WorldToScreen(posNum);
        foreach (var item in _routes.Values.GroupBy(delegate (RouteDescription x) {
            if (x.Path.Count < 2) {
                return 0.0;
            }
            Vector2i vector2i = x.Path[1] - x.Path[0];
            return Math.Atan2(vector2i.Y, vector2i.X);
        }).SelectMany((IGrouping<double, RouteDescription> group) => group.Select((RouteDescription route, int i) => (route, (float)i - (float)group.Count() / 2f + 0.5f)))) {
            RouteDescription route2 = item.Item1;
            float offsetAmount = item.Item2;
            System.Numerics.Vector2 p0 = initPos;
            System.Numerics.Vector2 p0WithOffset = p0;
            int j = 0;
            foreach (Vector2i elem in route2.Path) {
                System.Numerics.Vector2 p1 = base.GameController.IngameState.Camera.WorldToScreen(new System.Numerics.Vector3((float)elem.X * 10.869565f, (float)elem.Y * 10.869565f, _heightData[elem.Y][elem.X]));
                System.Numerics.Vector2 vector;
                if ((bool)base.Settings.PathfindingSettings.WorldPathSettings.OffsetPaths) {
                    System.Numerics.Vector2 s = p1 - p0;
                    vector = new System.Numerics.Vector2(s.Y, 0f - s.X) / s.Length();
                }
                else {
                    vector = System.Numerics.Vector2.Zero;
                }
                System.Numerics.Vector2 finalOffset = vector * offsetAmount * base.Settings.PathfindingSettings.WorldPathSettings.PathThickness;
                p0 = p1;
                p1 += finalOffset;
                if (++j % (int)base.Settings.PathfindingSettings.WorldPathSettings.DrawEveryNthSegment == 0) {
                    if (!_rect.Contains(p0WithOffset) && !_rect.Contains(p1)) {
                        break;
                    }
                    base.Graphics.DrawLine(p0WithOffset, p1, base.Settings.PathfindingSettings.WorldPathSettings.PathThickness, route2.WorldColor());
                }
                p0WithOffset = p1;
            }
        }
    }

    private void DrawBox(System.Numerics.Vector2 p0, System.Numerics.Vector2 p1, SharpDX.Color color) {
        _backGroundWindowPtr.AddRectFilled(p0, p1, color.ToImgui());
    }

    private void DrawText(string text, System.Numerics.Vector2 pos, SharpDX.Color color) {
        _backGroundWindowPtr.AddText(pos, color.ToImgui(), text);
    }

    private System.Numerics.Vector2 TranslateGridDeltaToMapDelta(System.Numerics.Vector2 delta, float deltaZ) {
        deltaZ /= 10.869565f;
        return (float)_mapScale * new System.Numerics.Vector2((delta.X - delta.Y) * CameraAngleCos, (deltaZ - (delta.X + delta.Y)) * CameraAngleSin);
    }

    private void DrawLargeMap(System.Numerics.Vector2 mapCenter) {
        if ((bool)base.Settings.DrawWalkableMap && base.Graphics.LowLevel.HasTexture("radar_minimap") && _areaDimensions.HasValue) {
            Render playerRender = base.GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Render>();
            if (playerRender != null) {
                SixLabors.ImageSharp.RectangleF rectangleF = new SixLabors.ImageSharp.RectangleF(0f - playerRender.GridPos().X, 0f - playerRender.GridPos().Y, _areaDimensions.Value.X, _areaDimensions.Value.Y);
                float playerHeight = 0f - playerRender.RenderStruct.Height;
                System.Numerics.Vector2 p1 = mapCenter + TranslateGridDeltaToMapDelta(new System.Numerics.Vector2(rectangleF.Left, rectangleF.Top), playerHeight);
                System.Numerics.Vector2 p2 = mapCenter + TranslateGridDeltaToMapDelta(new System.Numerics.Vector2(rectangleF.Right, rectangleF.Top), playerHeight);
                System.Numerics.Vector2 p3 = mapCenter + TranslateGridDeltaToMapDelta(new System.Numerics.Vector2(rectangleF.Right, rectangleF.Bottom), playerHeight);
                System.Numerics.Vector2 p4 = mapCenter + TranslateGridDeltaToMapDelta(new System.Numerics.Vector2(rectangleF.Left, rectangleF.Bottom), playerHeight);
                _backGroundWindowPtr.AddImageQuad(base.Graphics.LowLevel.GetTexture("radar_minimap"), p1, p2, p3, p4);
            }
        }
    }

    private void DrawTargets(System.Numerics.Vector2 mapCenter) {
        SharpDX.Color col = base.Settings.PathfindingSettings.TargetNameColor.Value;
        Render playerRender = base.GameController.Game.IngameState.Data.LocalPlayer.GetComponent<Render>();
        if (playerRender == null) {
            return;
        }
        System.Numerics.Vector2 playerPosition = new System.Numerics.Vector2(playerRender.GridPos().X, playerRender.GridPos().Y);
        float playerHeight = 0f - playerRender.RenderStruct.Height;
        int ithElement = 0;
        if ((bool)base.Settings.PathfindingSettings.ShowPathsToTargetsOnMap) {
            foreach (RouteDescription route in _routes.Values) {
                ithElement++;
                ithElement %= 5;
                foreach (Vector2i elem in route.Path.Skip(ithElement).GetEveryNth(5)) {
                    System.Numerics.Vector2 mapDelta3 = TranslateGridDeltaToMapDelta(new System.Numerics.Vector2(elem.X, elem.Y) - playerPosition, playerHeight + _heightData[elem.Y][elem.X]);
                    DrawBox(mapCenter + mapDelta3 - new System.Numerics.Vector2(2f, 2f), mapCenter + mapDelta3 + new System.Numerics.Vector2(2f, 2f), route.MapColor());
                }
            }
        }
        string key;
        if ((bool)base.Settings.PathfindingSettings.ShowAllTargets) {
            foreach (KeyValuePair<string, List<Vector2i>> allTargetLocation in _allTargetLocations) {
                allTargetLocation.Deconstruct(out key, out var value);
                string tileName = key;
                List<Vector2i> list = value;
                System.Numerics.Vector2 textOffset2 = (base.Graphics.MeasureText(tileName) / 2f).ToSdx();
                foreach (Vector2i vector in list) {
                    System.Numerics.Vector2 mapDelta2 = TranslateGridDeltaToMapDelta(vector.ToVector2Num() - playerPosition, playerHeight + _heightData[vector.Y][vector.X]);
                    if ((bool)base.Settings.PathfindingSettings.EnableTargetNameBackground) {
                        DrawBox(mapCenter + mapDelta2 - textOffset2, mapCenter + mapDelta2 + textOffset2, SharpDX.Color.Black);
                    }
                    DrawText(tileName, mapCenter + mapDelta2 - textOffset2, col);
                }
            }
            return;
        }
        if (!base.Settings.PathfindingSettings.ShowSelectedTargets) {
            return;
        }
        foreach (KeyValuePair<string, TargetLocations> clusteredTargetLocation in _clusteredTargetLocations) {
            clusteredTargetLocation.Deconstruct(out key, out var value2);
            string name = key;
            TargetLocations description = value2;
            System.Numerics.Vector2[] locations = description.Locations;
            for (int i = 0; i < locations.Length; i++) {
                System.Numerics.Vector2 clusterPosition = locations[i];
                float clusterHeight = 0f;
                if (clusterPosition.X < (float)_heightData[0].Length && clusterPosition.Y < (float)_heightData.Length) {
                    clusterHeight = _heightData[(int)clusterPosition.Y][(int)clusterPosition.X];
                }
                string text = (string.IsNullOrWhiteSpace(description.DisplayName) ? name : description.DisplayName);
                System.Numerics.Vector2 textOffset = (base.Graphics.MeasureText(text) / 2f).ToSdx();
                System.Numerics.Vector2 mapDelta = TranslateGridDeltaToMapDelta(clusterPosition - playerPosition, playerHeight + clusterHeight);
                if ((bool)base.Settings.PathfindingSettings.EnableTargetNameBackground) {
                    DrawBox(mapCenter + mapDelta - textOffset, mapCenter + mapDelta + textOffset, SharpDX.Color.Black);
                }
                DrawText(text, mapCenter + mapDelta - textOffset, col);
            }
        }
    }

    private void GenerateMapTexture() {
        float[][] gridHeightData = _heightData;
        int maxX = _areaDimensions.Value.X;
        int maxY = _areaDimensions.Value.Y;
        Configuration configuration = Configuration.Default.Clone();
        configuration.PreferContiguousImageBuffers = true;
        Image<Rgba32> image = new Image<Rgba32>(configuration, maxX, maxY);
        try {
            if ((bool)base.Settings.Debug.DrawHeightMap) {
                float minHeight = gridHeightData.Min((float[] x) => x.Min());
                float maxHeight = gridHeightData.Max((float[] x) => x.Max());
                image.Mutate(configuration, delegate (IImageProcessingContext c) {
                    c.ProcessPixelRowsAsVector4(delegate (Span<System.Numerics.Vector4> row, SixLabors.ImageSharp.Point i) {
                        for (int num11 = 0; num11 < row.Length - 1; num11 += 2) {
                            float num12 = gridHeightData[i.Y][num11];
                            for (int num13 = 0; num13 < 2; num13++) {
                                row[num11 + num13] = new System.Numerics.Vector4(0f, (num12 - minHeight) / (maxHeight - minHeight), 0f, 1f);
                            }
                        }
                    });
                });
            }
            else {
                System.Numerics.Vector4 unwalkableMask = System.Numerics.Vector4.UnitX + System.Numerics.Vector4.UnitW;
                System.Numerics.Vector4 walkableMask = System.Numerics.Vector4.UnitY + System.Numerics.Vector4.UnitW;
                if ((bool)base.Settings.Debug.DisableHeightAdjust) {
                    Parallel.For(0, maxY, delegate (int y) {
                        for (int num9 = 0; num9 < maxX; num9++) {
                            int num10 = _processedTerrainData[y][num9];
                            image[num9, y] = new Rgba32((num10 == 0) ? unwalkableMask : walkableMask);
                        }
                    });
                }
                else {
                    Parallel.For(0, maxY, delegate (int y) {
                        for (int n = 0; n < maxX; n++) {
                            int num5 = (int)(gridHeightData[y][n / 2 * 2] / 10.869565f / 2f);
                            int num6 = n - num5;
                            int num7 = y - num5;
                            int num8 = _processedTerrainData[y][n];
                            if (num6 >= 0 && num6 < maxX && num7 >= 0 && num7 < maxY) {
                                image[num6, num7] = new Rgba32((num8 == 0) ? unwalkableMask : walkableMask);
                            }
                        }
                    });
                }
                if (!base.Settings.Debug.SkipNeighborFill) {
                    Parallel.For(0, maxY, delegate (int y) {
                        for (int k = 0; k < maxX; k++) {
                            if (image[k, y].ToVector4() == System.Numerics.Vector4.Zero) {
                                int num = 0;
                                int num2 = 0;
                                for (int l = -1; l < 2; l++) {
                                    for (int m = -1; m < 2; m++) {
                                        int num3 = k + l;
                                        int num4 = y + m;
                                        if (num3 >= 0 && num3 < maxX && num4 >= 0 && num4 < maxY) {
                                            System.Numerics.Vector4 vector3 = image[k + l, y + m].ToVector4();
                                            if (vector3 == walkableMask) {
                                                num++;
                                            }
                                            else if (vector3 == unwalkableMask) {
                                                num2++;
                                            }
                                        }
                                    }
                                }
                                image[k, y] = new Rgba32((num > num2) ? walkableMask : unwalkableMask);
                            }
                        }
                    });
                }
                if (!base.Settings.Debug.SkipEdgeDetector) {
                    new EdgeDetectorProcessor(EdgeDetectorKernel.Laplacian5x5, grayscale: false).CreatePixelSpecificProcessor(configuration, image, image.Bounds()).Execute();
                }
                if (!base.Settings.Debug.SkipRecoloring) {
                    image.Mutate(configuration, delegate (IImageProcessingContext c) {
                        c.ProcessPixelRowsAsVector4(delegate (Span<System.Numerics.Vector4> row, SixLabors.ImageSharp.Point p) {
                            for (int j = 0; j < row.Length; j++) {
                                ref System.Numerics.Vector4 reference = ref row[j];
                                System.Numerics.Vector4 vector = row[j];
                                float x2 = vector.X;
                                System.Numerics.Vector4 vector2 = ((x2 == 1f) ? base.Settings.TerrainColor.Value.ToImguiVec4() : ((x2 != 0f) ? vector : System.Numerics.Vector4.Zero));
                                reference = vector2;
                            }
                        });
                    });
                }
            }
            if (Math.Max(image.Height, image.Width) > (int)base.Settings.MaximumMapTextureDimension) {
                int width = image.Width;
                int newHeight = image.Height;
                int newWidth = width;
                if (image.Height > image.Width) {
                    newWidth = newWidth * (int)base.Settings.MaximumMapTextureDimension / newHeight;
                    newHeight = base.Settings.MaximumMapTextureDimension;
                }
                else {
                    newHeight = newHeight * (int)base.Settings.MaximumMapTextureDimension / newWidth;
                    newWidth = base.Settings.MaximumMapTextureDimension;
                }
                Size targetSize = new Size(newWidth, newHeight);
                ResizeOptions resizeOptions = new ResizeOptions();
                resizeOptions.Size = targetSize;
                new ResizeProcessor(resizeOptions, image.Size()).CreatePixelSpecificCloningProcessor(configuration, image, image.Bounds()).Execute();
            }
            using Image<Rgba32> imageCopy = image.Clone(configuration);
            base.Graphics.LowLevel.AddOrUpdateTexture("radar_minimap", imageCopy);
        }
        finally {
            if (image != null) {
                ((IDisposable)image).Dispose();
            }
        }
    }

    private void LoadTargets() {
        string fileText = File.ReadAllText(Path.Combine(base.DirectoryFullName, "targets.json"));
        _targetDescriptions = JsonConvert.DeserializeObject<ConcurrentDictionary<string, List<TargetDescription>>>(fileText);
    }

    private void RestartPathFinding() {
        StopPathFinding();
        StartPathFinding();
    }

    private void StartPathFinding() {
        if ((bool)base.Settings.PathfindingSettings.ShowPathsToTargetsOnMap) {
            FindPaths(_clusteredTargetLocations, _routes, _findPathsCts.Token);
        }
    }

    private void StopPathFinding() {
        _findPathsCts.Cancel();
        _findPathsCts = new CancellationTokenSource();
        _routes = new ConcurrentDictionary<System.Numerics.Vector2, RouteDescription>();
    }

    private void FindPaths(IReadOnlyDictionary<string, TargetLocations> tiles, ConcurrentDictionary<System.Numerics.Vector2, RouteDescription> routes, CancellationToken cancellationToken) {
        List<System.Numerics.Vector2> targets = tiles.SelectMany((KeyValuePair<string, TargetLocations> x) => x.Value.Locations).Distinct().ToList();
        PathFinder pf = new PathFinder(_processedTerrainData, new int[5] { 1, 2, 3, 4, 5 });
        foreach (var item in targets.Take(base.Settings.MaximumPathCount).Zip(Enumerable.Repeat(RainbowColors, 100).SelectMany((List<SharpDX.Color> x) => x))) {
            var (point, color) = item;
            Task.Run(() => FindPath(pf, point, color, routes, cancellationToken));
        }
    }

    private async Task WaitUntilPluginEnabled(CancellationToken cancellationToken) {
        while (!base.Settings.Enable) {
            await Task.Delay(TimeSpan.FromSeconds(1.0), cancellationToken);
        }
    }

    private async Task FindPath(PathFinder pf, System.Numerics.Vector2 point, SharpDX.Color color, ConcurrentDictionary<System.Numerics.Vector2, RouteDescription> routes, CancellationToken cancellationToken) {
        System.Numerics.Vector2 playerPosition = GetPlayerPosition();
        IEnumerable<List<Vector2i>> pathI = pf.RunFirstScan(new Vector2i((int)playerPosition.X, (int)playerPosition.Y), new Vector2i((int)point.X, (int)point.Y));
        foreach (List<Vector2i> elem in pathI) {
            await WaitUntilPluginEnabled(cancellationToken);
            if (cancellationToken.IsCancellationRequested) {
                return;
            }
            if (elem.Any()) {
                RouteDescription rd2 = new RouteDescription {
                    Path = elem,
                    MapColor = GetMapColor,
                    WorldColor = GetWorldColor
                };
                routes.AddOrUpdate(point, rd2, (System.Numerics.Vector2 _, RouteDescription _) => rd2);
            }
        }
        while (true) {
            await WaitUntilPluginEnabled(cancellationToken);
            System.Numerics.Vector2 newPosition = GetPlayerPosition();
            if (playerPosition == newPosition) {
                await Task.Delay(100, cancellationToken);
                continue;
            }
            if (cancellationToken.IsCancellationRequested) {
                break;
            }
            playerPosition = newPosition;
            List<Vector2i> path = pf.FindPath(new Vector2i((int)playerPosition.X, (int)playerPosition.Y), new Vector2i((int)point.X, (int)point.Y));
            if (path != null) {
                RouteDescription rd = new RouteDescription {
                    Path = path,
                    MapColor = GetMapColor,
                    WorldColor = GetWorldColor
                };
                routes.AddOrUpdate(point, rd, (System.Numerics.Vector2 _, RouteDescription _) => rd);
            }
        }
        SharpDX.Color GetMapColor() {
            if (!base.Settings.PathfindingSettings.UseRainbowColorsForMapPaths) {
                return base.Settings.PathfindingSettings.DefaultMapPathColor;
            }
            return color;
        }
        SharpDX.Color GetWorldColor() {
            if (!base.Settings.PathfindingSettings.WorldPathSettings.UseRainbowColorsForPaths) {
                return base.Settings.PathfindingSettings.WorldPathSettings.DefaultPathColor;
            }
            return color;
        }
    }

    private ConcurrentDictionary<string, List<Vector2i>> GetTargets() {
        return new ConcurrentDictionary<string, List<Vector2i>>(GetTileTargets().Concat(GetEntityTargets()).ToLookup((KeyValuePair<string, List<Vector2i>> x) => x.Key, (KeyValuePair<string, List<Vector2i>> x) => x.Value).ToDictionary((IGrouping<string, List<Vector2i>> x) => x.Key, (IGrouping<string, List<Vector2i>> x) => x.SelectMany((List<Vector2i> v) => v).ToList()));
    }

    private Dictionary<string, List<Vector2i>> GetEntityTargets() {
        return (from x in base.GameController.Entities
                where x.HasComponent<Positioned>()
                where _currentZoneTargetEntityPaths.Contains(x.Path)
                select x).ToLookup((Entity x) => x.Path, (Entity x) => x.GetComponent<Positioned>().GridPosNum.Truncate()).ToDictionary((IGrouping<string, Vector2i> x) => x.Key, (IGrouping<string, Vector2i> x) => x.ToList());
    }

    private Dictionary<string, List<Vector2i>> GetTileTargets() {
        TileStructure[] tileData = base.GameController.Memory.ReadStdVector<TileStructure>(_terrainMetadata.TgtArray);
        ConcurrentDictionary<string, ConcurrentQueue<Vector2i>> ret = new ConcurrentDictionary<string, ConcurrentQueue<Vector2i>>();
        Parallel.For(0, tileData.Length, delegate (int tileNumber) {
            string text = base.GameController.Memory.Read<TgtDetailStruct>(base.GameController.Memory.Read<TgtTileStruct>(tileData[tileNumber].TgtFilePtr).TgtDetailPtr).name.ToString(base.GameController.Memory);
            if (!string.IsNullOrEmpty(text)) {
                Vector2i item = new Vector2i(tileNumber % (int)_terrainMetadata.NumCols * 23, tileNumber / (int)_terrainMetadata.NumCols * 23);
                ret.GetOrAdd(text, (string _) => new ConcurrentQueue<Vector2i>()).Enqueue(item);
            }
        });
        return ret.ToDictionary((KeyValuePair<string, ConcurrentQueue<Vector2i>> k) => k.Key, (KeyValuePair<string, ConcurrentQueue<Vector2i>> k) => k.Value.ToList());
    }

    private bool IsDescriptionInArea(string descriptionAreaPattern) {
        return base.GameController.Area.CurrentArea.Area.RawName.Like(descriptionAreaPattern);
    }

    private IEnumerable<TargetDescription> GetTargetDescriptionsInArea() {
        return _targetDescriptions.Where((KeyValuePair<string, List<TargetDescription>> x) => IsDescriptionInArea(x.Key)).SelectMany((KeyValuePair<string, List<TargetDescription>> x) => x.Value);
    }

    private ConcurrentDictionary<string, TargetLocations> ClusterTargets() {
        ConcurrentDictionary<string, TargetLocations> tileMap = new ConcurrentDictionary<string, TargetLocations>();
        Parallel.ForEach(_targetDescriptionsInArea.Values, new ParallelOptions {
            MaxDegreeOfParallelism = 1
        }, delegate (TargetDescription target) {
            TargetLocations targetLocations = ClusterTarget(target);
            if (targetLocations != null) {
                tileMap[target.Name] = targetLocations;
            }
        });
        return tileMap;
    }

    private TargetLocations ClusterTarget(TargetDescription target) {
        if (!_allTargetLocations.TryGetValue(target.Name, out var tileList)) {
            return null;
        }
        int[] clusterIndexes = KMeans.Cluster(tileList.Select((Vector2i x) => new Vector2d(x.X, x.Y)).ToArray(), target.ExpectedCount);
        List<System.Numerics.Vector2> resultList = new List<System.Numerics.Vector2>();
        foreach (IGrouping<int, (Vector2i, int)> tileGroup in from x in tileList.Zip(clusterIndexes)
                                                              group x by x.Second) {
            System.Numerics.Vector2 v = default(System.Numerics.Vector2);
            int count = 0;
            foreach (var item in tileGroup) {
                Vector2i vector = item.Item1;
                int mult = ((!IsGridWalkable(vector)) ? 1 : 100);
                v += mult * vector.ToVector2Num();
                count += mult;
            }
            v /= (float)count;
            Vector2i? replacement = ((IEnumerable<Vector2i>)(from x in tileGroup.Select<(Vector2i, int), Vector2i>(((Vector2i First, int Second) tile) => new Vector2i(tile.First.X, tile.First.Y)).Where(IsGridWalkable)
                                                             orderby (x.ToVector2Num() - v).LengthSquared()
                                                             select x)).Select((Func<Vector2i, Vector2i?>)((Vector2i x) => x)).FirstOrDefault();
            if (replacement.HasValue) {
                v = replacement.Value.ToVector2Num();
            }
            if (!IsGridWalkable(v.Truncate())) {
                v = GetAllNeighborTiles(v.Truncate()).First(IsGridWalkable).ToVector2Num();
            }
            resultList.Add(v);
        }
        return new TargetLocations {
            Locations = resultList.Distinct().ToArray(),
            DisplayName = target.DisplayName
        };
    }

    private bool IsGridWalkable(Vector2i tile) {
        int num = _processedTerrainData[tile.Y][tile.X];
        return num == 5 || num == 4;
    }

    private IEnumerable<Vector2i> GetAllNeighborTiles(Vector2i start) {
        foreach (int range in Enumerable.Range(1, 100000)) {
            int xStart = Math.Max(0, start.X - range);
            int yStart = Math.Max(0, start.Y - range);
            int xEnd = Math.Min(_areaDimensions.Value.X, start.X + range);
            int yEnd = Math.Min(_areaDimensions.Value.Y, start.Y + range);
            for (int x2 = xStart; x2 <= xEnd; x2++) {
                yield return new Vector2i(x2, yStart);
                yield return new Vector2i(x2, yEnd);
            }
            for (int x2 = yStart + 1; x2 <= yEnd - 1; x2++) {
                yield return new Vector2i(xStart, x2);
                yield return new Vector2i(xEnd, x2);
            }
            if (xStart == 0 && yStart == 0 && xEnd == _areaDimensions.Value.X && yEnd == _areaDimensions.Value.Y) {
                break;
            }
        }
    }
}
