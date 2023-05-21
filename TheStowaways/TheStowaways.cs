using HarmonyLib;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.IO;
using System.Reflection;
using TheStowaways.Components;

namespace TheStowaways;

public class TheStowaways : ModBehaviour
{
	public static TheStowaways Instance;
	public static INewHorizons NewHorizonsAPI;

	public bool IsGolemConnection { get; private set; }

	private void Awake()
	{
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
		Instance = this;
	}

	private void Start()
	{
		var menuFrameworkAPI = ModHelper.Interaction.GetModApi<IMenuAPI>("_nebula.MenuFramework");
		var newHorizonsAPI = ModHelper.Interaction.GetModApi<INewHorizons>("xen.NewHorizons");
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
		GlobalMessenger.AddListener("WakeUp", playerWakeUp);
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

	private void playerWakeUp()
	{
		//Init this sign at player wake up, because otherwise the text is overwritten somewhere else
		initTimberHearthEnjoySign();
	}

	private void initSolarSystem()
    {
		IsGolemConnection = false;
		initBrambleIsland();
		initConstructionYard();
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
		var chertDialogue = SearchUtilities.Find("BrambleIsland_Body/Sector_BrambleIsland/ChertRecording DIALOGUE_TO_BE_REPLACED").GetComponent<CharacterDialogueTree>();
		if(chertDialogue != null)
        {
			var xml = File.ReadAllText(Path.Combine(ModHelper.Manifest.ModFolderPath, "planets\\ExistingPlanets\\dialogue\\Chert SunkenIsland Notes.xml"));
			chertDialogue._xmlCharacterDialogueAsset = new UnityEngine.TextAsset(xml);
			chertDialogue.LateInitialize();
        }
	}

	private void initConstructionYard()
    {
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

	private void initTimberHearthEnjoySign()
	{
		var text = SearchUtilities.Find("TimberHearth_Body/Sector_TH/HearthBoard Enjoy/EnjoyText").GetComponent<UnityEngine.UI.Text>();
		if (text != null)
		{
			text.fontSize = 65;
			text.horizontalOverflow = UnityEngine.HorizontalWrapMode.Overflow;
			text.text = "ENJOY YOUR TRAVELS!";
		}
	}

	internal static bool IsSpecialStone(SharedStone stone)
    {
		return stone.name == "LoversCoveStone" || stone.name == "DazWorkshopStone";
	}

	public static void Write(string msg) => Instance.ModHelper.Console.WriteLine($"[TheStowaways] : {msg}", MessageType.Info);
	public static void WriteError(string msg) => Instance.ModHelper.Console.WriteLine($"[TheStowaways] : {msg}", MessageType.Error);
}

