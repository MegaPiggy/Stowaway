using HarmonyLib;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TheStowaways.Components;
using UnityEngine.PostProcessing;

namespace TheStowaways;

public class TheStowaways : ModBehaviour
{
	public static TheStowaways Instance;
	public static INewHorizons NewHorizonsAPI;
	private static readonly ISet<string> SpecialStones = new HashSet<string>()
	{
		"LoversCoveStone",
		"DazWorkshopStone",
		"BrambleIsle_Stone",
		"TimberHearthMines_Stone",
		"StatueIsle_Stone",
		"Cyard_Stone"
	};

	public bool IsGolemConnection { get; private set; }

	private void Awake()
	{
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		Instance = this;
	}

	private void Start()
	{
		var newHorizonsAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
		newHorizonsAPI.GetBodyLoadedEvent().AddListener(BodyLoaded);
		newHorizonsAPI.LoadConfigs(this);
		NewHorizonsAPI = newHorizonsAPI;

		LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
		{
			switch(loadScene)
            {
				case OWScene.SolarSystem:
					initSolarSystem();
					break;
			}
		};
		GlobalMessenger.AddListener("EnterNomaiGolemConnection", golemConnectionEntered);
		GlobalMessenger.AddListener("ExitNomaiGolemConnection", golemConnectionExited);
	}

    private void BodyLoaded(string body)
    {
		if (body == "ConstructionYardIsland")
        {
			//Only init the construction yard after New Horizon has initialized it
			initConstructionYard_Late();
        }
		if (body == "BrambleIsland")
		{
			initBrambleIsland_Late();
		}
		if (body == "StatueIsland")
		{
			initStatueIsland_Late();
		}
		if (body == "QuantumIsland")
		{
			initQuantumIsland_Late();
		}
		if (body == "Timber Hearth")
        {
			initTimberHearth_Late();
		}
	}

	private void golemConnectionEntered()
    {
		Write("GOLEM CONNECTION ENTER");
		IsGolemConnection = true;
    }

	private void golemConnectionExited()
	{
		Write("GOLEM CONNECTION EXIT");
		IsGolemConnection = false;
	}

	private void initSolarSystem()
    {
		IsGolemConnection = false;
		initBrambleIsland();
		initConstructionYard();
		initShip();
		initDensityComponents();

    }

	private void initBrambleIsland() 
	{
		var body = SearchUtilities.Find("BrambleIsland_Body");
		if (body != null)
        {
			body.gameObject.AddComponent<BrambleIslandComponent>();
        }
	}

	private void initBrambleIsland_Late()
    {
		var brambleIslandBody = SearchUtilities.Find("BrambleIsland_Body");
		initTractorBeams(brambleIslandBody);
		
		var solarPanel = SearchUtilities.Find("BrambleIsland_Body/Sector_BrambleIsland/BrambleIsle Solar Panels");
		if(solarPanel)
        {
			var solarPanelComponent = solarPanel.gameObject.AddComponent<SolarPanelCollisionComponent>();
			solarPanelComponent.SetIsland(brambleIslandBody?.GetComponent<IslandController>());
        }
		
		var chertDialogue = SearchUtilities.Find("BrambleIsland_Body/Sector_BrambleIsland/ChertRecording DIALOGUE_TO_BE_REPLACED").GetComponent<CharacterDialogueTree>();
		try
		{
			if (chertDialogue != null)
			{
				var xml = File.ReadAllText(Path.Combine(ModHelper.Manifest.ModFolderPath, "planets\\ExistingPlanets\\dialogue\\Chert SunkenIsland Notes.xml"));
				chertDialogue._xmlCharacterDialogueAsset = new UnityEngine.TextAsset(xml);
				chertDialogue.LateInitialize();
			}
		} 
		catch(NullReferenceException)
        {} //Ignore this error thrown by LateInitialize
	}

	private void initConstructionYard()
    {
		var constructionYardBody = SearchUtilities.Find("ConstructionYardIsland_Body");
		initTractorBeams(constructionYardBody);

		var solarPanel = SearchUtilities.Find("ConstructionYardIsland_Body/Sector_ConstructionYard/ConstructYard Solar Panels");
		if (solarPanel)
		{
			var solarPanelComponent = solarPanel.gameObject.AddComponent<SolarPanelCollisionComponent>();
			solarPanelComponent.SetIsland(constructionYardBody?.GetComponent<IslandController>());
		}

		var socket = SearchUtilities.Find("ConstructionYardIsland_Body/Sector_ConstructionYard/Interactables_ConstructionYard/LandingIsland/Prefab_NOM_Whiteboard/ArcSocket");
		if (socket)
		{
			socket.AddComponent<ScrollSocketBehaviour>();
		}
	}

	private void initConstructionYard_Late()
    {
		var text = SearchUtilities.Find(ScrollSocketBehaviour.ScrollPath).GetComponent<NomaiWallText>();
		if (text)
		{
			text._showTextOnStart = false;
			text.HideImmediate();
		}
	}

	private void initTimberHearth_Late()
	{
		var sign = SearchUtilities.Find("TimberHearth_Body/Sector_TH/HearthBoard Enjoy/EnjoyText");
		if (sign != null)
		{
			sign.AddComponent<EnjoySignComponent>();
		}
	}

	private void initStatueIsland_Late()
    {
		var statueIslandBody = SearchUtilities.Find("StatueIsland_Body");
		initTractorBeams(statueIslandBody);

		var solarPanel = SearchUtilities.Find("StatueIsland_Body/Sector_StatueIsland/StatueIsle Solar Panels");
		if (solarPanel)
		{
			var solarPanelComponent = solarPanel.gameObject.AddComponent<SolarPanelCollisionComponent>();
			solarPanelComponent.SetIsland(statueIslandBody?.GetComponent<IslandController>());
		}
	}

	private void initDensityComponents()
	{
		var statueIslandBody = SearchUtilities.Find("StatueIsland_Body");
		if(statueIslandBody)
		{
			var comp = statueIslandBody.AddComponent<IslandDensityComponent>();
			comp.DensityModifierSun = 0.03f;
			comp.DensityModifierMoon = 0.03f;
			comp.DensityModifierBoth = 0.4f;
		}
		
        var cyardIslandBody = SearchUtilities.Find("ConstructionYardIsland_Body");
        if (cyardIslandBody)
        {
            var comp = cyardIslandBody.AddComponent<IslandDensityComponent>();
            comp.DensityModifierSun = 0.03f;
            comp.DensityModifierMoon = 0.03f;
            comp.DensityModifierBoth = 0.4f;
        }
        var brambleIslandBody = SearchUtilities.Find("BrambleIsland_Body");
        if (brambleIslandBody)
        {
            var comp = brambleIslandBody.AddComponent<IslandDensityComponent>();
            comp.DensityModifierSun = 0.03f;
            comp.DensityModifierMoon = 0.03f;
            comp.DensityModifierBoth = 0.4f;
        }
        var gabbroIslandBody = SearchUtilities.Find("GabbroIsland_Body");
        if (gabbroIslandBody)
        {
            var comp = gabbroIslandBody.AddComponent<IslandDensityComponent>();
            comp.DensityModifierSun = 0.03f;
            comp.DensityModifierMoon = 0.03f;
            comp.DensityModifierBoth = 0.4f;
        }
    }

	private void initShip()
	{
		SearchUtilities.Find("Ship_Body").AddComponent<ShipCollisionComponent>();
	}

	private void initQuantumIsland_Late()
	{
		var probeDisplay = SearchUtilities.Find("QuantumIsland_Body/Sector_QuantumIsland/Nomai Camera/VerticalPivot/Launcher/ProbeScreen (1)/ProbeDisplay");
		if(probeDisplay != null)
        {
			probeDisplay.transform.localPosition = new UnityEngine.Vector3(0.2623f, 0.3001f, 0.3557f);
			probeDisplay.transform.localRotation = UnityEngine.Quaternion.Euler(3.332f, 76.1487f, 356.5323f);
		}
		var camera = SearchUtilities.Find("QuantumIsland_Body/Sector_QuantumIsland/Nomai Camera/VerticalPivot/Launcher/preLaunchCamera")?.GetComponent<OWCamera>();
		if (camera != null)
		{
			camera.fieldOfView = 60f;
		}
		var blackAndWhite = SearchUtilities.Find("QuantumIsland_Body/Sector_QuantumIsland/Nomai Camera/VerticalPivot/Launcher/preLaunchCamera")?.GetComponent<PostProcessingBehaviour>();
		if(blackAndWhite != null)
        {
			Destroy(blackAndWhite);
        }
	}

	private void initTractorBeams(UnityEngine.GameObject islandObject)
    {
		var islandController = islandObject.GetComponent<IslandController>();
		if (islandController)
		{
			var tractorBeams = islandObject.GetComponentsInChildren<SafetyTractorBeamController>(true);
			islandController._safetyTractorBeams = tractorBeams;
			Write($"Initializing {tractorBeams.Length} tractor beams on {islandObject.name}");
		}
		else
		{
			WriteError($"No island controller found on {islandObject.name}");
		}
	}

	internal static bool IsSpecialStone(SharedStone stone)
    {
		return SpecialStones.Contains(stone.name);
	}

	public static void Write(string msg) => Instance.ModHelper.Console.WriteLine($"[TheStowaways] : {msg}", MessageType.Info);
	public static void WriteError(string msg) => Instance.ModHelper.Console.WriteLine($"[TheStowaways] : {msg}", MessageType.Error);
}

