#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore.PoEMemory.FilesInMemory;
using ExileCore.PoEMemory.FilesInMemory.Archnemesis;
using ExileCore.PoEMemory.FilesInMemory.Atlas;
using ExileCore.PoEMemory.FilesInMemory.Metamorph;
using ExileCore.PoEMemory.FilesInMemory.Sanctum;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.PoEMemory.MemoryObjects.Heist;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Static;

namespace ExileCore.PoEMemory;

public class FilesContainer
{
	private readonly IMemory _memory;

	private BaseItemTypes _baseItemTypes;

	private UniversalFileWrapper<BetrayalChoiceAction> _betrayalChoiceActions;

	private UniversalFileWrapper<BetrayalChoice> _betrayalChoises;

	private UniversalFileWrapper<BetrayalDialogue> _betrayalDialogue;

	private UniversalFileWrapper<ArchnemesisRecipe> _archnemesisRecipes;

	private UniversalFileWrapper<BetrayalJob> _betrayalJobs;

	private UniversalFileWrapper<BetrayalRank> _betrayalRanks;

	private UniversalFileWrapper<BetrayalReward> _betrayalRewards;

	private UniversalFileWrapper<BetrayalTarget> _betrayalTargets;

	private UniversalFileWrapper<HeistJobRecord> _HeistJobs;

	private UniversalFileWrapper<HeistChestRewardTypeRecord> _HeistChestRewardTypes;

	private UniversalFileWrapper<HeistNpcRecord> _HeistNpcs;

	private ModsDat _mods;

	private StatsDat _stats;

	private TagsDat _tags;

	private ItemVisualIdentities _itemVisualIdentities;

	private UniqueItemDescriptions _uniqueItemDescriptions;

	private UniversalFileWrapper<WordEntry> _word;

	private UniversalFileWrapper<AtlasNode> atlasNodes;

	public FilesFromMemory FilesFromMemory;

	private LabyrinthTrials labyrinthTrials;

	private MonsterVarieties monsterVarieties;

	private PassiveSkills passiveSkills;

	private PropheciesDat prophecies;

	private Quests quests;

	private QuestStates questStates;

	private WorldAreas worldAreas;

	private UniversalFileWrapper<MetamorphMetaSkill> _metamorphMetaSkills;

	private UniversalFileWrapper<MetamorphMetaSkillType> _metamorphMetaSkillTypes;

	private UniversalFileWrapper<MetamorphMetaMonster> _metamorphMetaMonsters;

	private UniversalFileWrapper<MetamorphRewardType> _metamorphRewardTypes;

	private UniversalFileWrapper<MetamorphRewardTypeItemsClient> _metamorphRewardTypeItemsClient;

	private AtlasRegions _atlasRegions;

	private BestiaryCapturableMonsters bestiaryCapturableMonsters;

	private UniversalFileWrapper<BestiaryRecipe> bestiaryRecipes;

	private UniversalFileWrapper<BestiaryRecipeComponent> bestiaryRecipeComponents;

	private UniversalFileWrapper<BestiaryGroup> bestiaryGroups;

	private UniversalFileWrapper<BestiaryFamily> bestiaryFamilies;

	private UniversalFileWrapper<BestiaryGenus> bestiaryGenuses;

	private UniversalFileWrapper<ArchnemesisMod> _archnemesisMods;

	private UniversalFileWrapper<LakeRoom> _lakeRooms;

	private UniversalFileWrapper<StampChoice> _stampChoices;

	private UniversalFileWrapper<HeistChestRecord> _heistChests;

	private UniversalFileWrapper<ChestRecord> _chests;

	private UniversalFileWrapper<QuestReward> _questRewards;

	private UniversalFileWrapper<QuestRewardOffer> _questRewardOffers;

	private UniversalFileWrapper<Character> _characters;

	private UniversalFileWrapper<GrantedEffectPerLevel> _grantedEffectsPerLevel;

	private UniversalFileWrapper<GrantedEffect> _grantedEffects;

	private UniversalFileWrapper<SanctumRoom> _sanctumRooms;

	private UniversalFileWrapper<SanctumRoomType> _sanctumRoomTypes;

	private UniversalFileWrapper<SanctumPersistentEffect> _sanctumPersistentEffects;

	private UniversalFileWrapper<SanctumDeferredRewardDisplayCategory> _sanctumDeferredRewardDisplayCategories;

	public ItemClasses ItemClasses { get; }

	public BaseItemTypes BaseItemTypes => _baseItemTypes ?? (_baseItemTypes = new BaseItemTypes(_memory, () => FindFile("Data/BaseItemTypes.dat")));

	public ModsDat Mods => _mods ?? (_mods = new ModsDat(_memory, () => FindFile("Data/Mods.dat"), Stats, Tags));

	public StatsDat Stats => _stats ?? (_stats = new StatsDat(_memory, () => FindFile("Data/Stats.dat")));

	public TagsDat Tags => _tags ?? (_tags = new TagsDat(_memory, () => FindFile("Data/Tags.dat")));

	public WorldAreas WorldAreas => worldAreas ?? (worldAreas = new WorldAreas(_memory, () => FindFile("Data/WorldAreas.dat")));

	public PassiveSkills PassiveSkills => passiveSkills ?? (passiveSkills = new PassiveSkills(_memory, () => FindFile("Data/PassiveSkills.dat")));

	public LabyrinthTrials LabyrinthTrials => labyrinthTrials ?? (labyrinthTrials = new LabyrinthTrials(_memory, () => FindFile("Data/LabyrinthTrials.dat")));

	public Quests Quests => quests ?? (quests = new Quests(_memory, () => FindFile("Data/Quest.dat")));

	public QuestStates QuestStates => questStates ?? (questStates = new QuestStates(_memory, () => FindFile("Data/QuestStates.dat")));

	public UniversalFileWrapper<QuestReward> QuestRewards => _questRewards ?? (_questRewards = new UniversalFileWrapper<QuestReward>(_memory, () => FindFile("Data/QuestRewards.dat")));

	public UniversalFileWrapper<QuestRewardOffer> QuestRewardOffers => _questRewardOffers ?? (_questRewardOffers = new UniversalFileWrapper<QuestRewardOffer>(_memory, () => FindFile("Data/QuestRewardOffers.dat")));

	public UniversalFileWrapper<Character> Characters => _characters ?? (_characters = new UniversalFileWrapper<Character>(_memory, () => FindFile("Data/Characters.dat")));

	public MonsterVarieties MonsterVarieties => monsterVarieties ?? (monsterVarieties = new MonsterVarieties(_memory, () => FindFile("Data/MonsterVarieties.dat")));

	public PropheciesDat Prophecies => prophecies ?? (prophecies = new PropheciesDat(_memory, () => FindFile("Data/Prophecies.dat")));

	public ItemVisualIdentities ItemVisualIdentities => _itemVisualIdentities ?? (_itemVisualIdentities = new ItemVisualIdentities(_memory, () => FindFile("Data/ItemVisualIdentity.dat")));

	public UniqueItemDescriptions UniqueItemDescriptions => _uniqueItemDescriptions ?? (_uniqueItemDescriptions = new UniqueItemDescriptions(_memory, () => FindFile("Data/UniqueStashLayout.dat")));

	public UniversalFileWrapper<WordEntry> Words => _word ?? (_word = new UniversalFileWrapper<WordEntry>(_memory, () => FindFile("Data/Words.dat")));

	public UniversalFileWrapper<AtlasNode> AtlasNodes => atlasNodes ?? (atlasNodes = new AtlasNodes(_memory, () => FindFile("Data/AtlasNode.dat")));

	public UniversalFileWrapper<BetrayalTarget> BetrayalTargets => _betrayalTargets ?? (_betrayalTargets = new UniversalFileWrapper<BetrayalTarget>(_memory, () => FindFile("Data/BetrayalTargets.dat")));

	public UniversalFileWrapper<BetrayalJob> BetrayalJobs => _betrayalJobs ?? (_betrayalJobs = new UniversalFileWrapper<BetrayalJob>(_memory, () => FindFile("Data/BetrayalJobs.dat")));

	public UniversalFileWrapper<BetrayalRank> BetrayalRanks => _betrayalRanks ?? (_betrayalRanks = new UniversalFileWrapper<BetrayalRank>(_memory, () => FindFile("Data/BetrayalRanks.dat")));

	public UniversalFileWrapper<BetrayalReward> BetrayalRewards => _betrayalRewards ?? (_betrayalRewards = new UniversalFileWrapper<BetrayalReward>(_memory, () => FindFile("Data/BetrayalTraitorRewards.dat")));

	public UniversalFileWrapper<BetrayalChoice> BetrayalChoises => _betrayalChoises ?? (_betrayalChoises = new UniversalFileWrapper<BetrayalChoice>(_memory, () => FindFile("Data/BetrayalChoices.dat")));

	public UniversalFileWrapper<BetrayalChoiceAction> BetrayalChoiceActions => _betrayalChoiceActions ?? (_betrayalChoiceActions = new UniversalFileWrapper<BetrayalChoiceAction>(_memory, () => FindFile("Data/BetrayalChoiceActions.dat")));

	public UniversalFileWrapper<BetrayalDialogue> BetrayalDialogue => _betrayalDialogue ?? (_betrayalDialogue = new UniversalFileWrapper<BetrayalDialogue>(_memory, () => FindFile("Data/BetrayalDialogue.dat")));

	public UniversalFileWrapper<ArchnemesisRecipe> ArchnemesisRecipes => _archnemesisRecipes ?? (_archnemesisRecipes = new UniversalFileWrapper<ArchnemesisRecipe>(_memory, () => FindFile("Data/ArchnemesisRecipes.dat")));

	public UniversalFileWrapper<ArchnemesisMod> ArchnemesisMods => _archnemesisMods ?? (_archnemesisMods = new UniversalFileWrapper<ArchnemesisMod>(_memory, () => FindFile("Data/ArchnemesisMods.dat")));

	public UniversalFileWrapper<LakeRoom> LakeRooms => _lakeRooms ?? (_lakeRooms = new UniversalFileWrapper<LakeRoom>(_memory, () => FindFile("Data/LakeRooms.dat")));

	public UniversalFileWrapper<StampChoice> StampChoices => _stampChoices ?? (_stampChoices = new UniversalFileWrapper<StampChoice>(_memory, () => FindFile("Data/StampChoice.dat")));

	public UniversalFileWrapper<GrantedEffectPerLevel> GrantedEffectsPerLevel => _grantedEffectsPerLevel ?? (_grantedEffectsPerLevel = new UniversalFileWrapper<GrantedEffectPerLevel>(_memory, () => FindFile("Data/GrantedEffectsPerLevel.dat")));

	public UniversalFileWrapper<GrantedEffect> GrantedEffects => _grantedEffects ?? (_grantedEffects = new UniversalFileWrapper<GrantedEffect>(_memory, () => FindFile("Data/GrantedEffects.dat")));

	public UniversalFileWrapper<HeistChestRecord> HeistChests => _heistChests ?? (_heistChests = new UniversalFileWrapper<HeistChestRecord>(_memory, () => FindFile("Data/HeistChests.dat")));

	public UniversalFileWrapper<ChestRecord> Chests => _chests ?? (_chests = new UniversalFileWrapper<ChestRecord>(_memory, () => FindFile("Data/Chests.dat")));

	public UniversalFileWrapper<HeistJobRecord> HeistJobs => _HeistJobs ?? (_HeistJobs = new UniversalFileWrapper<HeistJobRecord>(_memory, () => FindFile("Data/HeistJobs.dat")));

	public UniversalFileWrapper<HeistChestRewardTypeRecord> HeistChestRewardType => _HeistChestRewardTypes ?? (_HeistChestRewardTypes = new UniversalFileWrapper<HeistChestRewardTypeRecord>(_memory, () => FindFile("Data/HeistChestRewardTypes.dat")));

	public UniversalFileWrapper<HeistNpcRecord> HeistNpcs => _HeistNpcs ?? (_HeistNpcs = new UniversalFileWrapper<HeistNpcRecord>(_memory, () => FindFile("Data/HeistNPCs.dat")));

	public UniversalFileWrapper<MetamorphMetaSkill> MetamorphMetaSkills => _metamorphMetaSkills ?? (_metamorphMetaSkills = new UniversalFileWrapper<MetamorphMetaSkill>(_memory, () => FindFile("Data/MetamorphosisMetaSkills.dat")));

	public UniversalFileWrapper<MetamorphMetaSkillType> MetamorphMetaSkillTypes => _metamorphMetaSkillTypes ?? (_metamorphMetaSkillTypes = new UniversalFileWrapper<MetamorphMetaSkillType>(_memory, () => FindFile("Data/MetamorphosisMetaSkillTypes.dat")));

	public UniversalFileWrapper<MetamorphMetaMonster> MetamorphMetaMonsters => _metamorphMetaMonsters ?? (_metamorphMetaMonsters = new UniversalFileWrapper<MetamorphMetaMonster>(_memory, () => FindFile("Data/MetamorphosisMetaMonsters.dat")));

	public UniversalFileWrapper<MetamorphRewardType> MetamorphRewardTypes => _metamorphRewardTypes ?? (_metamorphRewardTypes = new UniversalFileWrapper<MetamorphRewardType>(_memory, () => FindFile("Data/MetamorphosisRewardTypes.dat")));

	public UniversalFileWrapper<MetamorphRewardTypeItemsClient> MetamorphRewardTypeItemsClient => _metamorphRewardTypeItemsClient ?? (_metamorphRewardTypeItemsClient = new UniversalFileWrapper<MetamorphRewardTypeItemsClient>(_memory, () => FindFile("Data/MetamorphosisRewardTypeItemsClient.dat")));

	public AtlasRegions AtlasRegions => _atlasRegions ?? (_atlasRegions = new AtlasRegions(_memory, () => FindFile("Data/AtlasRegions.dat")));

	public Dictionary<string, FileInformation> AllFiles { get; private set; }

	public Dictionary<string, FileInformation> Metadata { get; } = new Dictionary<string, FileInformation>();


	public Dictionary<string, FileInformation> Data { get; private set; } = new Dictionary<string, FileInformation>();


	public Dictionary<string, FileInformation> OtherFiles { get; } = new Dictionary<string, FileInformation>();


	public Dictionary<string, FileInformation> LoadedInThisArea { get; private set; } = new Dictionary<string, FileInformation>(1024);


	public Dictionary<int, List<KeyValuePair<string, FileInformation>>> GroupedByTest2 { get; set; }

	public Dictionary<int, List<KeyValuePair<string, FileInformation>>> GroupedByChangeAction { get; set; }

	public BestiaryCapturableMonsters BestiaryCapturableMonsters
	{
		get
		{
			if (bestiaryCapturableMonsters == null)
			{
				return bestiaryCapturableMonsters = new BestiaryCapturableMonsters(_memory, () => FindFile("Data/BestiaryCapturableMonsters.dat"));
			}
			return bestiaryCapturableMonsters;
		}
	}

	public UniversalFileWrapper<BestiaryRecipe> BestiaryRecipes
	{
		get
		{
			if (bestiaryRecipes == null)
			{
				return bestiaryRecipes = new UniversalFileWrapper<BestiaryRecipe>(_memory, () => FindFile("Data/BestiaryRecipes.dat"));
			}
			return bestiaryRecipes;
		}
	}

	public UniversalFileWrapper<BestiaryRecipeComponent> BestiaryRecipeComponents
	{
		get
		{
			if (bestiaryRecipeComponents == null)
			{
				return bestiaryRecipeComponents = new UniversalFileWrapper<BestiaryRecipeComponent>(_memory, () => FindFile("Data/BestiaryRecipeComponent.dat"));
			}
			return bestiaryRecipeComponents;
		}
	}

	public UniversalFileWrapper<BestiaryGroup> BestiaryGroups
	{
		get
		{
			if (bestiaryGroups == null)
			{
				return bestiaryGroups = new UniversalFileWrapper<BestiaryGroup>(_memory, () => FindFile("Data/BestiaryGroups.dat"));
			}
			return bestiaryGroups;
		}
	}

	public UniversalFileWrapper<BestiaryFamily> BestiaryFamilies
	{
		get
		{
			if (bestiaryFamilies == null)
			{
				return bestiaryFamilies = new UniversalFileWrapper<BestiaryFamily>(_memory, () => FindFile("Data/BestiaryFamilies.dat"));
			}
			return bestiaryFamilies;
		}
	}

	public UniversalFileWrapper<BestiaryGenus> BestiaryGenuses
	{
		get
		{
			if (bestiaryGenuses == null)
			{
				return bestiaryGenuses = new UniversalFileWrapper<BestiaryGenus>(_memory, () => FindFile("Data/BestiaryGenus.dat"));
			}
			return bestiaryGenuses;
		}
	}

	public UniversalFileWrapper<SanctumRoom> SanctumRooms
	{
		get
		{
			UniversalFileWrapper<SanctumRoom> universalFileWrapper = _sanctumRooms;
			if (universalFileWrapper == null)
			{
				UniversalFileWrapper<SanctumRoom> obj = new UniversalFileWrapper<SanctumRoom>(_memory, () => FindFile("Data/SanctumRooms.dat"))
				{
					ExcludeZeroAddresses = true
				};
				UniversalFileWrapper<SanctumRoom> universalFileWrapper2 = obj;
				_sanctumRooms = obj;
				universalFileWrapper = universalFileWrapper2;
			}
			return universalFileWrapper;
		}
	}

	public UniversalFileWrapper<SanctumRoomType> SanctumRoomTypes
	{
		get
		{
			UniversalFileWrapper<SanctumRoomType> universalFileWrapper = _sanctumRoomTypes;
			if (universalFileWrapper == null)
			{
				UniversalFileWrapper<SanctumRoomType> obj = new UniversalFileWrapper<SanctumRoomType>(_memory, () => FindFile("Data/SanctumRoomTypes.dat"))
				{
					ExcludeZeroAddresses = true
				};
				UniversalFileWrapper<SanctumRoomType> universalFileWrapper2 = obj;
				_sanctumRoomTypes = obj;
				universalFileWrapper = universalFileWrapper2;
			}
			return universalFileWrapper;
		}
	}

	public UniversalFileWrapper<SanctumDeferredRewardDisplayCategory> SanctumDeferredRewardDisplayCategories
	{
		get
		{
			UniversalFileWrapper<SanctumDeferredRewardDisplayCategory> universalFileWrapper = _sanctumDeferredRewardDisplayCategories;
			if (universalFileWrapper == null)
			{
				UniversalFileWrapper<SanctumDeferredRewardDisplayCategory> obj = new UniversalFileWrapper<SanctumDeferredRewardDisplayCategory>(_memory, () => FindFile("Data/SanctumDeferredRewardDisplayCategories.dat"))
				{
					ExcludeZeroAddresses = true
				};
				UniversalFileWrapper<SanctumDeferredRewardDisplayCategory> universalFileWrapper2 = obj;
				_sanctumDeferredRewardDisplayCategories = obj;
				universalFileWrapper = universalFileWrapper2;
			}
			return universalFileWrapper;
		}
	}

	public UniversalFileWrapper<SanctumPersistentEffect> SanctumPersistentEffects
	{
		get
		{
			UniversalFileWrapper<SanctumPersistentEffect> universalFileWrapper = _sanctumPersistentEffects;
			if (universalFileWrapper == null)
			{
				UniversalFileWrapper<SanctumPersistentEffect> obj = new UniversalFileWrapper<SanctumPersistentEffect>(_memory, () => FindFile("Data/SanctumPersistentEffects.dat"))
				{
					ExcludeZeroAddresses = true
				};
				UniversalFileWrapper<SanctumPersistentEffect> universalFileWrapper2 = obj;
				_sanctumPersistentEffects = obj;
				universalFileWrapper = universalFileWrapper2;
			}
			return universalFileWrapper;
		}
	}

	public event EventHandler<Dictionary<string, FileInformation>> LoadedFiles;

	public FilesContainer(IMemory memory)
	{
		_memory = memory;
		ItemClasses = new ItemClasses();
		FilesFromMemory = new FilesFromMemory(_memory);
		using (new PerformanceTimer("Load files from memory"))
		{
			AllFiles = FilesFromMemory.GetAllFiles();
			Trace.WriteLine($"Loaded {AllFiles.Count} files from memory {AllFiles.Values.Count((FileInformation x) => x.Ptr > 0)}/{AllFiles.Count} has pointers.");
		}
		Task.Run(delegate
		{
			using (new PerformanceTimer("Preload stats and mods"))
			{
				_ = Stats.records.Count;
				_ = Mods.records.Count;
				ParseFiles(AllFiles);
			}
		});
	}

	public void LoadFiles()
	{
		AllFiles = FilesFromMemory.GetAllFilesSync();
	}

	public void ParseFiles(Dictionary<string, FileInformation> files)
	{
		foreach (KeyValuePair<string, FileInformation> file in files)
		{
			if (!string.IsNullOrEmpty(file.Key))
			{
				if (file.Key.StartsWith("Metadata/", StringComparison.Ordinal))
				{
					Metadata[file.Key] = file.Value;
				}
				else if (file.Key.StartsWith("Data/", StringComparison.Ordinal) && file.Key.EndsWith(".dat", StringComparison.Ordinal))
				{
					Data[file.Key] = file.Value;
				}
				else
				{
					OtherFiles[file.Key] = file.Value;
				}
			}
		}
	}

	public void ParseFiles(int gameAreaChangeCount)
	{
		if (AllFiles != null)
		{
			LoadedInThisArea = new Dictionary<string, FileInformation>(1024);
			ParseFiles(AllFiles);
			this.LoadedFiles?.Invoke(this, LoadedInThisArea);
		}
	}

	public long FindFile(string name)
	{
		try
		{
			if (AllFiles.TryGetValue(name, out var value))
			{
				return value.Ptr;
			}
		}
		catch (KeyNotFoundException)
		{
			MessageBox.Show($"Couldn't find the file in memory: {name}\nTry to restart the game.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			Environment.Exit(1);
		}
		return 0L;
	}
}
