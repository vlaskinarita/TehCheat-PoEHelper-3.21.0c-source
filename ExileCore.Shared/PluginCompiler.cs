using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ImGuiNET;
using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Locator;

namespace ExileCore.Shared;

public class PluginCompiler : IDisposable
{
	private static readonly SemaphoreSlim BuildSemaphore = new SemaphoreSlim(1, 1);

	private readonly BuildManager buildManager = new BuildManager("pluginCompiler");

	private PluginCompiler()
	{
	}//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	//IL_0010: Expected O, but got Unknown


	public static PluginCompiler Create()
	{
		if (!MSBuildLocator.IsRegistered)
		{
			return null;
		}
		return new PluginCompiler();
	}

	public static PluginCompiler CreateOrThrow()
	{
		return Create() ?? throw new Exception("Plugin compilation is disabled");
	}

	public void CompilePlugin(FileInfo csProj, string outputDirectory)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, string> globalProperties = new Dictionary<string, string>
		{
			["OutputPath"] = outputDirectory,
			["exapiPackage"] = AppDomain.CurrentDomain.BaseDirectory,
			["RuntimeIdentifier"] = "win-x64",
			["SelfContained"] = "False",
			["PathMap"] = ""
		};
		string text = csProj.Name.Replace(csProj.Extension, "");
		ProjectPropertyElement val = null;
		try
		{
			ProjectRootElement val2 = ProjectRootElement.Open(csProj.FullName);
			PatchProject(val2);
			if (val2.HasUnsavedChanges)
			{
				val2.Save();
			}
			val = ((IEnumerable<ProjectPropertyElement>)val2.Properties).FirstOrDefault((Func<ProjectPropertyElement, bool>)((ProjectPropertyElement x) => x.Name == "TargetFramework"));
			BuildSemaphore.Wait();
			try
			{
				ProjectCollection val3 = new ProjectCollection();
				try
				{
					MsBuildLogger msBuildLogger = new MsBuildLogger();
					BuildParameters val4 = new BuildParameters(val3);
					val4.DisableInProcNode = true;
					val4.EnableNodeReuse = true;
					val4.Loggers = (IEnumerable<ILogger>)(object)new MsBuildLogger[1] { msBuildLogger };
					BuildParameters val5 = val4;
					ProjectInstance val6 = ProjectInstance.FromProjectRootElement(val2, new ProjectOptions
					{
						GlobalProperties = globalProperties
					});
					BuildResult val7 = buildManager.Build(val5, new BuildRequestData(val6, new string[2] { "Restore", "Build" }, (HostServices)null));
					if ((int)val7.OverallResult != 0)
					{
						throw val7.Exception ?? new Exception("Build failed:\n" + string.Join("\n", msBuildLogger.Errors));
					}
					val3.UnloadAllProjects();
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				BuildSemaphore.Release();
			}
		}
		catch (Exception value)
		{
			if (val == null || val.Value == null)
			{
				DebugWindow.LogError(text + " -> CompilePlugin failed, but you can try running the fix_plugins.ps1 script", 10f);
			}
			else if (val.Value == "net4.8")
			{
				DebugWindow.LogError(text + " -> CompilePlugin failed, but you can try updating its TargetFramework to net6.0-windows", 10f);
			}
			else
			{
				DebugWindow.LogError(text + " -> CompilePlugin failed");
			}
			DebugWindow.LogError($"{text} -> {value}");
			throw;
		}
	}

	private static void PatchProject(ProjectRootElement pre)
	{
		ProjectItemElement val = ((IEnumerable<ProjectItemElement>)pre.Items).FirstOrDefault((Func<ProjectItemElement, bool>)((ProjectItemElement x) => x.ItemType == "PackageReference" && x.Include.Equals("ImGui.NET", StringComparison.OrdinalIgnoreCase)));
		if (val != null)
		{
			ProjectMetadataElement val2 = ((IEnumerable<ProjectMetadataElement>)val.Metadata).FirstOrDefault((Func<ProjectMetadataElement, bool>)((ProjectMetadataElement x) => x.Name == "Version"));
			if (val2 != null)
			{
				string version = Assembly.GetAssembly(typeof(ImGui)).GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
				if (version.Trim() != val2.Value.Trim())
				{
					val2.Value = version;
				}
			}
		}
		ProjectPropertyElement val3 = ((IEnumerable<ProjectPropertyElement>)pre.Properties).FirstOrDefault((Func<ProjectPropertyElement, bool>)((ProjectPropertyElement x) => x.Name == "TargetFramework"));
		string text = ((val3 != null) ? val3.Value : null);
		if (text != null && !text.StartsWith("net4") && text != "net6.0-windows")
		{
			val3.Value = "net6.0-windows";
		}
	}

	public void Dispose()
	{
		buildManager.ResetCaches();
		buildManager.CancelAllSubmissions();
		buildManager.ShutdownAllNodes();
		buildManager.Dispose();
	}
}
