using HarmonyLib;
using NewHorizons.Utility;
using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Stowaway.Components;
using UnityEngine;
using NewHorizons.Components.SizeControllers;

namespace Stowaway;

public class Stowaway : ModBehaviour
{
	public static Stowaway Instance;
	public static INewHorizons NewHorizonsAPI;
	private static readonly ISet<string> SpecialStones = new HashSet<string>()
	{
		"LoversCoveStone",
		"DSS_Stone",
		"TimberHearthMines_Stone",
		"StatueIsle_Stone",
		"Cyard_Stone",


		"Solar tide lovestone",
		"DSS proj Stone",
		"Sanctuary Voyager Stone",
		"ScrollWorkshop_Stone",
 
		"DSS Cassava TH stone",
		"DSS dazshop stone",
		"DSS cyard stone",
		"DSS lovers stone",
 
	};

	public bool IsGolemConnection { get; private set; }

	private void Awake()
	{
		Instance = this;
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
	}

	private void Start()
	{
		var newHorizonsAPI = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
		newHorizonsAPI.GetBodyLoadedEvent().AddListener(body => BodyLoaded(body.Replace(" ","")));
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
		if (body == "AshTwin")
		{
			initAshTwin_Late();
		}
		if (body == "EmberTwin")
		{
			initEmberTwin_Late();
		}
		if (body == "BrittleHollow")
		{
			initBrittleHollow_Late();
		}
		if (body == "TimberHearth")
		{
			initTimberHearth_Late();
		}
		if (body == "GiantsDeep")
		{
			initGiantsDeep_Late();
		}
		if (body == "ConstructionYardIsland")
		{
			//Only init the construction yard after New Horizon has initialized it
			initConstructionYard_Late();
		}
		if (body == "StatueIsland")
		{
			initStatueIsland_Late();
		}
		if (body == "QuantumIsland")
		{
			initQuantumIsland_Late();
		}
		if (body.EndsWith("Island"))
		{
			initIsland(SearchUtilities.Find(body + "_Body"));
		}
		if (body.EndsWith("Inspired"))
		{
			initInspiredComet(NewHorizonsAPI.GetPlanet(body));
		}
	}

	private void initWaterColumn(AstroObject giantsDeep)
	{
		var info = NewHorizonsAPI.QueryBody<QuantumMoonWaterColumnInfo>("GiantsDeep", "extras.QuantumMoonWaterColumn");
		QuantumMoonWaterColumnController.multiplier = info.size;
		QuantumMoonWaterColumnController.height = info.height;
		QuantumMoonWaterColumnController.debug = info.debug;

		var rigidbody = giantsDeep.GetOWRigidbody();
		var sector = giantsDeep.GetRootSector();

		var column = new GameObject($"QuantumMoonWaterColumn_Body");
		column.SetActive(false);
		column.transform.parent = giantsDeep.transform;

		column.AddComponent<OWRigidbody>();

		var matchMotion = column.AddComponent<MatchInitialMotion>();
		matchMotion.SetBodyToMatch(rigidbody);

		column.AddComponent<CenterOfTheUniverseOffsetApplier>();
		column.AddComponent<KinematicRigidbody>();

		var detectorGO = new GameObject("Detector_Funnel");
		detectorGO.transform.parent = column.transform;
		var funnelDetector = detectorGO.AddComponent<ConstantForceDetector>();
		funnelDetector._inheritDetector = giantsDeep.GetComponentInChildren<ConstantForceDetector>();
		funnelDetector._detectableFields = new ForceVolume[0];

		detectorGO.AddComponent<ForceApplier>();

		var scaleRoot = new GameObject("ScaleRoot");
		scaleRoot.transform.parent = column.transform;
		scaleRoot.transform.rotation = Quaternion.identity;
		scaleRoot.transform.localPosition = Vector3.zero;
		scaleRoot.transform.localScale = new Vector3(1, 1, 1);

		var proxyGO = SearchUtilities.Find("SandFunnel_Body/ScaleRoot/Proxy_SandFunnel").Instantiate(scaleRoot.transform, "Proxy_Funnel");
		var geoGO = SearchUtilities.Find("SandFunnel_Body/ScaleRoot/Geo_SandFunnel").Instantiate(scaleRoot.transform, "Geo_Funnel");
		var volumesGO = SearchUtilities.Find("SandFunnel_Body/ScaleRoot/Volumes_SandFunnel").Instantiate(scaleRoot.transform, "Volumes_Funnel");

		var sfv = volumesGO.GetComponentInChildren<SimpleFluidVolume>();
		sfv._fluidType = FluidVolume.Type.WATER;

		GameObject.Destroy(geoGO.transform.Find("Effects_HT_SandColumn/SandColumn_Interior").gameObject);

		var waterMaterials = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Geometry_NorthPole/OtherComponentsGroup/Terrain_NorthPoleSurface/BatchedGroup/BatchedMeshRenderers_5").GetComponent<MeshRenderer>().sharedMaterials.CopyMaterials();

		// Proxy
		var proxyExterior = proxyGO.transform.Find("SandColumn_Exterior (1)");
		proxyExterior.name = "WaterColumn_Exterior";
		var proxyExteriorMR = proxyExterior.GetComponent<MeshRenderer>();
		proxyExteriorMR.sharedMaterials = waterMaterials;
		proxyExteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		// Geometry
		var geoExterior = geoGO.transform.Find("Effects_HT_SandColumn/SandColumn_Exterior");
		geoExterior.name = "WaterColumn_Exterior";
		var geoExteriorMR = geoExterior.GetComponent<MeshRenderer>();
		geoExteriorMR.sharedMaterials = waterMaterials;
		geoExteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		proxyGO.GetComponent<SectorProxy>().SetSector(sector);
		geoGO.GetComponent<SectorCullGroup>().SetSector(sector);
		volumesGO.GetComponent<SectorCollisionGroup>().SetSector(sector);

		column.transform.localPosition = Vector3.zero;

		column.AddComponent<QuantumMoonWaterColumnController>();

		ModHelper.Events.Unity.FireOnNextUpdate(() => column.SetActive(true));
	}

	private void initInspiredComet(GameObject inspired)
	{
		var cometTailController = inspired.GetComponentInChildren<CometTailController>(true);
		var cometTail = cometTailController.gameObject;
		var inspiredTailController = cometTail.AddComponent<InspiredTailController>();
		inspiredTailController.gasTail = cometTailController.gasTail; // GasTail
		inspiredTailController.dustTail = cometTailController.dustTail; // DustTail
		GameObject.DestroyImmediate(cometTailController);
		inspiredTailController.SetTarget(inspired.transform.Find("Sector/Target"));
	}

	private void initBrittleHollow_Late()
	{
		var brittleHollow = Locator.GetAstroObject(AstroObject.Name.BrittleHollow);

		var stormShelter = brittleHollow.transform.Find("Sector_BH/Prefab_NOM_SafetyTractorBeam");
		if (stormShelter)
		{
			stormShelter.gameObject.GetAddComponent<OverheadDetector>();
			var stormShelterComponent = stormShelter.gameObject.GetAddComponent<SolarTideStormShelter>();

			var solarPanel = brittleHollow.transform.Find("Sector_BH/BH workshop solarpanel");
			if (solarPanel)
			{
				var solarPanelComponent = solarPanel.gameObject.GetAddComponent<SolarPanelCollision>();
				solarPanelComponent.SetSolarTideStormShelter(stormShelterComponent);
			}
		}
	}

	private void initAshTwin_Late()
	{
		var ashTwin = Locator.GetAstroObject(AstroObject.Name.TowerTwin);
#if DEBUG
		var sand = ashTwin.GetComponentInChildren<SandLevelController>(true);
		sand._scaleCurve = AnimationCurve.Constant(0, TimeLoop.LOOP_DURATION_IN_MINUTES * 60, 0); // set scale to 0 at all times
		sand.gameObject.SetActive(true);
#endif
		foreach (var door in ashTwin.GetComponentsInChildren<NomaiMultiPartDoor>(true))
		{
			door.gameObject.GetAddComponent<OverheadDetector>();
			door.gameObject.GetAddComponent<NomaiDoorTugger>().SetCanOpenAndClose(true);
		}
	}

	private void initTimberHearth_Late()
	{
		var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth);
		foreach (var gateway in timberHearth.GetComponentsInChildren<NomaiGateway>(true))
		{
			gateway.gameObject.GetAddComponent<OverheadDetector>();
			gateway.gameObject.GetAddComponent<NomaiGatewayTugger>();
		}
	}

	private void initEmberTwin_Late()
	{
		var caveTwin = Locator.GetAstroObject(AstroObject.Name.CaveTwin);
		foreach (var gateway in caveTwin.GetComponentsInChildren<NomaiGateway>(true))
		{
			if (gateway.name == "WindowCoverInterface")
			{
				gateway.gameObject.GetAddComponent<OverheadDetector>();
				gateway.gameObject.GetAddComponent<NomaiGatewayTugger>();
			}
		}
		foreach (var door in caveTwin.GetComponentsInChildren<NomaiMultiPartDoor>(true))
		{
			door.gameObject.GetAddComponent<OverheadDetector>();
			door.gameObject.GetAddComponent<NomaiDoorTugger>();
		}
	}

	private const float sizeMultiplier = 1.25f;

	private void initIsland(GameObject body)
	{
		initTractorBeams(body);
		foreach (Campfire campfire in body.GetComponentsInChildren<Campfire>(true))
		{
			campfire.gameObject.AddComponent<TornadoIslandCampfireDetector>();
		}
		if (body.FindChild("RepellentVolume") == null)
		{
			var collider = body.GetComponentInChildren<ForceApplier>(true).GetComponent<Collider>();
			var localPos = collider.transform.localPosition;
			var localEuler = collider.transform.localEulerAngles;
			var repellent = new GameObject("RepellentVolume");
			repellent.transform.SetParent(body.transform, false);
			repellent.transform.localPosition = localPos;
			repellent.transform.localEulerAngles = localEuler;
			repellent.layer = LayerMask.NameToLayer("BasicEffectVolume");
			if (collider is SphereCollider sCollider)
			{
				var sphere = repellent.AddComponent<SphereCollider>();
				sphere.radius = sCollider.radius * sizeMultiplier;
				sphere.center = sCollider.center;
				sphere.isTrigger = true;
			}
			else if (collider is CapsuleCollider cCollider)
			{
				var capsule = repellent.AddComponent<CapsuleCollider>();
				capsule.radius = cCollider.radius * sizeMultiplier;
				capsule.height = cCollider.height * sizeMultiplier;
				capsule.direction = cCollider.direction;
				capsule.center = cCollider.center;
				capsule.isTrigger = true;
			}
			repellent.AddComponent<OWTriggerVolume>();
			repellent.AddComponent<OtherIslandRepelFluidVolume>();
			repellent.AddComponent<DebugVolume>();
		}
		foreach (var door in body.GetComponentsInChildren<NomaiMultiPartDoor>(true))
		{
			door.gameObject.GetAddComponent<OverheadDetector>();
			door.gameObject.GetAddComponent<NomaiDoorTugger>();
		}
		foreach (var tractorSwitch in body.GetComponentsInChildren<TractorBeamSwitch>(true))
		{
			tractorSwitch.gameObject.GetAddComponent<OverheadDetector>();
			tractorSwitch.gameObject.GetAddComponent<TractorBeamSwitchTugger>();
		}
	}

	private void initGiantsDeep_Late()
	{
		var giantsDeep = Locator.GetAstroObject(AstroObject.Name.GiantsDeep);
		giantsDeep.GetComponent<QuantumOrbit>()._orbitRadius = 2000;
		var qmChopZone = new GameObject("QuantumMoonChopZone");
		qmChopZone.transform.SetParent(giantsDeep.GetRootSector().transform, false);
		qmChopZone.AddComponent<QuantumMoonChopZone>();
		var chopInfo = NewHorizonsAPI.QueryBody<QuantumMoonChopZoneInfo>("GiantsDeep", "extras.QuantumMoonChopZone");
		QuantumMoonChopZone.qmChopRadius = chopInfo.radius;
		CreateInvertedOccluder(giantsDeep.GetRootSector().transform, "CloudsInvertedOccluder", 930);
		CreateInvertedOccluder(giantsDeep.GetRootSector().transform, "OceanInvertedOccluder", 500);
		initWaterColumn(giantsDeep);
	}

	private void CreateInvertedOccluder(Transform parent, string name, float radius)
	{
		var invertedOccluder = new GameObject(name);
		invertedOccluder.transform.SetParent(parent, false);
		var sphere = invertedOccluder.AddComponent<SphereShape>();
		sphere.radius = radius;
		sphere.SetCollisionMode(Shape.CollisionMode.Volume);
		invertedOccluder.AddComponent<InvertedVisibilityOccluder>();
	}

	private void golemConnectionEntered()
	{
		Write("GOLEM CONNECTION ENTER");
		IsGolemConnection = true;
		DialogueConditionManager.SharedInstance.SetConditionState("GolemConnection", conditionState: true);
	}

	private void golemConnectionExited()
	{
		Write("GOLEM CONNECTION EXIT");
		IsGolemConnection = false;
	}

	private void initSolarSystem()
	{
		IsGolemConnection = false;
		initShip();
		initDensityComponents();
		initJellyfish();
	}

	private void initJellyfish()
	{
		foreach (JellyfishController jellyfish in FindObjectsOfType<JellyfishController>())
		{
			jellyfish.gameObject.AddComponent<JellyfishQuantumMoonRiser>();
			jellyfish.GetComponentInChildren<FluidDetector>().gameObject.AddComponent<JellyfishBarrierIgnorer>();
		}
	}

	private void initConstructionYard_Late()
	{
		var constructionYardBody = SearchUtilities.Find("ConstructionYardIsland_Body");

		var solarPanel = SearchUtilities.Find("ConstructionYardIsland_Body/Sector_ConstructionYard/ConstructYard Solar Panels");
		if (solarPanel)
		{
			var solarPanelComponent = solarPanel.gameObject.GetAddComponent<SolarPanelCollision>();
			solarPanelComponent.SetIsland(constructionYardBody?.GetComponent<IslandController>());
		}

		var socket = SearchUtilities.Find("ConstructionYardIsland_Body/Sector_ConstructionYard/Interactables_ConstructionYard/LandingIsland/Prefab_NOM_Whiteboard/ArcSocket");
		if (socket)
		{
			socket.GetAddComponent<SchematicRearSecret>();
		}

		var text = SearchUtilities.Find(SchematicRearSecret.ScrollPath).GetComponent<NomaiWallText>();
		if (text)
		{
			text._showTextOnStart = false;
			text.HideImmediate();
		}
	}

	private void initStatueIsland_Late()
	{
		var statueIslandBody = SearchUtilities.Find("StatueIsland_Body");

		var solarPanel = SearchUtilities.Find("StatueIsland_Body/Sector_StatueIsland/StatueIsle Solar Panels");
		if (solarPanel)
		{
			var solarPanelComponent = solarPanel.gameObject.GetAddComponent<SolarPanelCollision>();
			solarPanelComponent.SetIsland(statueIslandBody?.GetComponent<IslandController>());
		}
	}

	private void initDensityComponents()
	{
		var statueIslandBody = SearchUtilities.Find("StatueIsland_Body");
		if(statueIslandBody)
		{
			statueIslandBody.AddComponent<OverheadDetector>();
			var comp = statueIslandBody.AddComponent<IslandDensityModifier>();
			comp.DensityModifierSun = 0.03f;
			comp.DensityModifierMoon = 0.03f;
			comp.DensityModifierBoth = 0.4f;
		}
		var cyardIslandBody = SearchUtilities.Find("ConstructionYardIsland_Body");
		if (cyardIslandBody)
		{
			cyardIslandBody.AddComponent<OverheadDetector>();
			var comp = cyardIslandBody.AddComponent<IslandDensityModifier>();
			comp.DensityModifierSun = 0.03f;
			comp.DensityModifierMoon = 0.03f;
			comp.DensityModifierBoth = 0.4f;
		}
		var brambleIslandBody = SearchUtilities.Find("BrambleIsland_Body");
		if (brambleIslandBody)
		{
			brambleIslandBody.AddComponent<OverheadDetector>();
			var comp = brambleIslandBody.AddComponent<IslandDensityModifier>();
			comp.DensityModifierSun = 0.03f;
			comp.DensityModifierMoon = 0.03f;
			comp.DensityModifierBoth = 0.4f;
		}
		var gabbroIslandBody = SearchUtilities.Find("GabbroIsland_Body");
		if (gabbroIslandBody)
		{
			gabbroIslandBody.AddComponent<OverheadDetector>();
			var comp = gabbroIslandBody.AddComponent<IslandDensityModifier>();
			comp.DensityModifierSun = 0.03f;
			comp.DensityModifierMoon = 0.03f;
			comp.DensityModifierBoth = 0.4f;
		}
		var quantumIslandBody = SearchUtilities.Find("QuantumIsland_Body");
		if (quantumIslandBody)
		{
			quantumIslandBody.AddComponent<OverheadDetector>();
		}
	}

	private void initShip()
	{
		SearchUtilities.Find("Ship_Body").AddComponent<ShipSolarPanelCollision>();
	}

	private void initQuantumIsland_Late()
	{
	
		var cerberusJellyfish = SearchUtilities.Find("QuantumIsland_Body/Sector_QuantumIsland/Cerberus Jelly");
		if (cerberusJellyfish != null)
		{
			cerberusJellyfish.GetAddComponent<CerberusJellyfish>();
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

	public static void Write(string msg) => Instance.ModHelper.Console.WriteLine($"[Stowaway] : {msg}", MessageType.Info);
	public static void WriteSuccess(string msg) => Instance.ModHelper.Console.WriteLine($"[Stowaway] : {msg}", MessageType.Success);
	public static void WriteWarning(string msg) => Instance.ModHelper.Console.WriteLine($"[Stowaway] : {msg}", MessageType.Warning);
	public static void WriteError(string msg) => Instance.ModHelper.Console.WriteLine($"[Stowaway] : {msg}", MessageType.Error);
	public static void WriteException(Exception ex) => Instance.ModHelper.Console.WriteLine($"[Stowaway] : {ex}", MessageType.Error);
}