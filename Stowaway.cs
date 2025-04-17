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
using Stowaway.Misc;
using NewHorizons.Utility.Files;
using System.Linq;
using NewHorizons.Utility.OWML;
using NewHorizons.Components.Orbital;
using Autodesk.Fbx;
using Epic.OnlineServices;
using NewHorizons.Utility.OuterWilds;
using NewHorizons.Handlers;

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
		newHorizonsAPI.GetStarSystemLoadedEvent().AddListener(system => SystemLoaded(system));
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

	private void SystemLoaded(string system)
	{
		WriteError($"System Loaded: \"{system}\"");
		if (system == "SolarSystem" || system == "EyeOfTheUniverse")
		{
			initSurfaceMaterials();
		}
		if (system == "SolarSystem")
		{
			initSandFunnel_Late();
		}
	}

	private void BodyLoaded(string body)
	{
		WriteError($"Body Loaded: \"{body}\"");
		if (body == "Sun")
		{
			initSun_Late();
		}
		if (body == "HourglassTwins")
		{
			initHourglassTwins_Late();
		}
		if (body == "TheHourglassTwins")
		{
			initNewFocalPoint_Late(NewHorizonsAPI.GetPlanet("The Hourglass Twins"));
		}
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
		if (body == "DeepStormStation")
		{
			initDeepStormStation(NewHorizonsAPI.GetPlanet("Deep Storm Station"));
		}
		if (body.EndsWith("Island"))
		{
			initIsland(SearchUtilities.Find(body + "_Body"));
		}
		if (body.EndsWith("Inspired"))
		{
			initInspiredComet(NewHorizonsAPI.GetPlanet("Inspired"));
		}
	}

	private void initHourglassTwins_Late()
	{
		var hgt = Locator.GetAstroObject(AstroObject.Name.HourglassTwins);
		var sector = hgt.GetRootSector();
	}

	private void initNewFocalPoint_Late(GameObject gameObject)
	{
		WriteError($"initNewFocalPoint_Late: \"{gameObject.name}\"");
		var hgt = gameObject.GetComponent<NHAstroObject>();
		hgt._name = AstroObject.Name.HourglassTwins;
		WriteError("hgtSector: " + ((hgt.GetRootSector() != null) ? "exists" : "noexists"));
		Delay.RunWhen(() => hgt.GetRootSector() != null, () =>
		{
			var emberAstroObject = Locator.GetAstroObject(AstroObject.Name.CaveTwin);
			var ashAstroObject = Locator.GetAstroObject(AstroObject.Name.TowerTwin);
			var emberObject = emberAstroObject.gameObject;
			var ashObject = ashAstroObject.gameObject;
			var ember = emberAstroObject.GetRootSector();
			var ash = ashAstroObject.GetRootSector();
			var tli = ash.gameObject.FindChild("Sector_TimeLoopInterior").GetComponent<Sector>();
			var sector = hgt.GetRootSector();
			sector.OnDestroy();
			sector._firstUpdate = true;
			sector._idString = "HOURGLASS_TWINS";
			sector._name = Sector.Name.HourglassTwins;
			sector.Awake();
			Delay.FireOnNextUpdate(() => {
				GameObject.DestroyImmediate(emberObject.gameObject.FindChild("Sector_HGT"));
				GameObject.DestroyImmediate(ashObject.gameObject.FindChild("Sector_HGT"));
				var emberSectorStreaming = emberObject.GetComponentInChildren<SectorStreaming>(true);
				var ashSectorStreaming = ashObject.GetComponentInChildren<SectorStreaming>(true);
				emberSectorStreaming.OnDestroy();
				ashSectorStreaming.OnDestroy();
				GameObject.DestroyImmediate(emberSectorStreaming.gameObject);
				GameObject.DestroyImmediate(ashSectorStreaming.gameObject);
				ember.SetParentSector(sector);
				ash.SetParentSector(sector);
			});
			tli.SetParentSector(sector);
			var resolutionScale = sector.gameObject.AddComponent<SectorResolutionScale>();
			resolutionScale._playstation4 = DynamicResolutionManager.TargetResolution._900;
			resolutionScale._playstation4Pro = DynamicResolutionManager.TargetResolution._900;
			resolutionScale._playstation5 = DynamicResolutionManager.TargetResolution._1872;
			resolutionScale._xboxOne = DynamicResolutionManager.TargetResolution._900;
			resolutionScale._xboxOneS = DynamicResolutionManager.TargetResolution._900;
			resolutionScale._xboxOneX = DynamicResolutionManager.TargetResolution._1440;
			resolutionScale._xboxSeriesS = DynamicResolutionManager.TargetResolution._1296;
			resolutionScale._xboxSeriesX = DynamicResolutionManager.TargetResolution._2088;
			resolutionScale._xboxSeriesX = DynamicResolutionManager.TargetResolution._2088;
			resolutionScale.SetSector(sector);
			var ER = sector.gameObject.AddComponent<EffectRuleset>();
			ER._type = EffectRuleset.BubbleType.None;
			ER._sandMaterial = SearchUtilities.Find("FocalBody/Sector_HGT").GetComponent<EffectRuleset>()._sandMaterial;

			var sectorStreamingObj = new GameObject("Sector_Streaming");
			sectorStreamingObj.transform.SetParent(gameObject.transform, false);
			var sectorStreaming = sectorStreamingObj.AddComponent<SectorStreaming>();
			sectorStreaming._softLoadRadius = 3000;
			sectorStreaming._streamingGroup = StreamingHandler.GetStreamingGroup(AstroObject.Name.CaveTwin);
			sectorStreaming.SetSector(sector);
		});
	}

	private void initSandFunnel_Late()
	{
		var nhgt = NewHorizonsAPI.GetPlanet("The Hourglass Twins").transform;
		var sandFunnel = SearchUtilities.Find("SandFunnel_Body");
		var body = sandFunnel.GetComponent<OWRigidbody>();
		body._origParent = nhgt;
		body._origParentBody = nhgt.GetComponent<OWRigidbody>();
		sandFunnel.transform.SetParent(nhgt, false);
		sandFunnel.SetActive(true);
		body.UnsuspendImmediate(false);
		sandFunnel.AddComponent<SandFunnelFixer>();
		WriteError("SandFunnelFixer");
		Delay.FireInNUpdates(() =>
		{
			sandFunnel.SetActive(true);
			body.UnsuspendImmediate(false);
			body.GetAttachedForceDetector().enabled = true;
		}, 3);
	}

	private void initSun_Late()
	{
		var sun = Locator.GetAstroObject(AstroObject.Name.Sun);
		var sector = sun.GetRootSector().transform;
		var extraGravity = sector.Find("sun gravitywell3 sun");
		if (extraGravity != null)
		{
			WriteError("Found object at path Sun_Body/Sector_SUN/sun gravitywell3 sun");
			var volume = extraGravity.GetComponent<GravityVolume>();
			volume._surfaceAcceleration = 37.5f; // 400 billion at 100 accel. So change to 37.5 for 150 billion.
			volume._isPlanetGravityVolume = false;
			volume._setMass = false; // Do not set actual mass
			volume.Awake(); // Run start up code that changes gravitationalMass
		}
		else
			WriteError("Cannot find object at path Sun_Body/Sector_SUN/sun gravitywell3 sun");
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

		var detectorGO = new GameObject("Detector_QuantumMoonWaterColumn");
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

		var proxyGO = SearchUtilities.Find("SandFunnel_Body/ScaleRoot/Proxy_SandFunnel").Instantiate(scaleRoot.transform, "Proxy_QuantumMoonWaterColumn");
		var geoGO = SearchUtilities.Find("SandFunnel_Body/ScaleRoot/Geo_SandFunnel").Instantiate(scaleRoot.transform, "Geo_QuantumMoonWaterColumn");
		var volumesGO = SearchUtilities.Find("SandFunnel_Body/ScaleRoot/Volumes_SandFunnel").Instantiate(scaleRoot.transform, "Volumes_QuantumMoonWaterColumn");

		var sfv = volumesGO.GetComponentInChildren<SimpleFluidVolume>();
		sfv.name = "FluidVolume_QuantumMoonWaterColumn";
		sfv._fluidType = FluidVolume.Type.WATER;

		var waterMaterials = SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Geometry_NorthPole/OtherComponentsGroup/Terrain_NorthPoleSurface/BatchedGroup/BatchedMeshRenderers_5").GetComponent<MeshRenderer>().sharedMaterials.CopyMaterials();
		foreach (var waterMaterial in waterMaterials)
		{
			if (info.tint != null)
			{
				waterMaterial.SetColor("_FogColor", info.tint.ToColor());
			}
		}

		// Proxy
		var proxyExterior = proxyGO.transform.Find("SandColumn_Exterior (1)");
		proxyExterior.name = "WaterColumn_Exterior_Proxy";
		var proxyExteriorMR = proxyExterior.GetComponent<MeshRenderer>();
		proxyExteriorMR.sharedMaterials = waterMaterials;
		proxyExteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		// Geometry
		var effects = geoGO.transform.Find("Effects_HT_SandColumn");
		effects.name = "Effects_QuantumMoonWaterColumn";

		var geoExterior = effects.transform.Find("SandColumn_Exterior");
		geoExterior.name = "WaterColumn_Exterior";
		var geoExteriorMR = geoExterior.GetComponent<MeshRenderer>();
		geoExteriorMR.sharedMaterials = waterMaterials;
		geoExteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		var geoInterior = effects.transform.Find("SandColumn_Interior");
		geoInterior.name = "WaterColumn_Interior";
		var geoInteriorMR = geoInterior.GetComponent<MeshRenderer>();
		geoInteriorMR.sharedMaterials = waterMaterials;
		geoInteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		proxyGO.GetComponent<SectorProxy>().SetSector(sector);
		geoGO.GetComponent<SectorCullGroup>().SetSector(sector);
		volumesGO.GetComponent<SectorCollisionGroup>().SetSector(sector);

		column.transform.localPosition = Vector3.zero;

		column.AddComponent<QuantumMoonWaterColumnController>();

		ModHelper.Events.Unity.FireOnNextUpdate(() => column.SetActive(true));
	}

	private void initDeepStormStation(GameObject deepStormStation)
	{
		var sector = deepStormStation.transform.Find("Sector");
		var rfVolume = deepStormStation.transform.Find("RFVolume");
		var volumes = deepStormStation.transform.Find("Volumes");

		var pos = Vector3.up * 130;
		sector.localPosition = pos;
		rfVolume.localPosition = pos;
		volumes.localPosition = pos;
	}

	private void initInspiredComet(GameObject inspired)
	{
		var sector = inspired.transform.Find("Sector");
		var rfVolume = inspired.transform.Find("RFVolume");
		var volumes = inspired.transform.Find("Volumes");
		var fieldDetector = inspired.transform.Find("FieldDetector");
		var gravityWell = inspired.transform.Find("GravityWell");

		var pos = Vector3.up * 2400;
		sector.localPosition = pos;
		rfVolume.localPosition = pos;
		volumes.localPosition = pos;
		fieldDetector.localPosition = pos;
		gravityWell.localPosition = pos;

		var rot = new Vector3(270, 180, 0);
		sector.localEulerAngles = rot;
		rfVolume.localEulerAngles = rot;
		volumes.localEulerAngles = rot;
		fieldDetector.localEulerAngles = rot;
		gravityWell.localEulerAngles = rot;
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

		initGhostMatter(brittleHollow, "workshop gm wisps replacement");
		initGhostMatter(brittleHollow, "Sector_QuantumFragment/QTOWER GM/QTOWER GM wisps");
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

		initGhostMatter(ashTwin, "SS tower GM wisps door");
		initGhostMatter(ashTwin, "SS tower GM wisps warppad");
	}

	private void initTimberHearth_Late()
	{
		var timberHearth = Locator.GetAstroObject(AstroObject.Name.TimberHearth);
		foreach (var gateway in timberHearth.GetComponentsInChildren<NomaiGateway>(true))
		{
			gateway.gameObject.GetAddComponent<OverheadDetector>();
			gateway.gameObject.GetAddComponent<NomaiGatewayTugger>();
		}

		initGhostMatter(timberHearth, "AuroraWisps village patch");
		initGhostMatter(timberHearth, "Forest GM wisps 1");
		initGhostMatter(timberHearth, "Forest GM wisps 2");
		initGhostMatter(timberHearth, "Stowaway GM wisps");
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

	private const float sizeMultiplier = 1;

	private void initIsland(GameObject body)
	{
		initTractorBeams(body);
		foreach (Campfire campfire in body.GetComponentsInChildren<Campfire>(true))
		{
			campfire.gameObject.GetAddComponent<TornadoIslandCampfireDetector>();
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

		/*var socket = SearchUtilities.Find("ConstructionYardIsland_Body/Sector_ConstructionYard/Interactables_ConstructionYard/LandingIsland/Prefab_NOM_Whiteboard/ArcSocket");
		if (socket)
		{
			socket.GetAddComponent<SchematicRearSecret>();
		}

		var text = SearchUtilities.Find(SchematicRearSecret.ScrollPath).GetComponent<NomaiWallText>();
		if (text)
		{
			text._showTextOnStart = false;
			text.HideImmediate();
		}*/
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
			WriteWarning($"No island controller found on {islandObject.name}");
		}
	}

	private void initGhostMatter(AstroObject astroObject, string path)
	{
		var key = astroObject._name == AstroObject.Name.CustomString ? astroObject.GetCustomName() : astroObject._name.ToString();
		var gmTransform = astroObject.GetRootSector().transform.Find(path);

		if (gmTransform != null)
		{
			WriteError("Found object at path " + key + "/Sector/" + path);
			var gmObject = gmTransform.gameObject;
			var overhead = gmObject.GetAddComponent<OverheadDetector>();
			var gmFloat = gmObject.GetAddComponent<GhostMatterFloatController>();
		}
		else
		{
			WriteError("Cannot find object at path " + key + "/Sector/" + path);
		}
	}

	private static readonly string BundleLocation = "planets/bundle";

	private void initSurfaceMaterials()
	{
		var completePath = Path.Combine(ModHelper.Manifest.ModFolderPath, BundleLocation);

		var files = Directory.GetFiles(completePath).Where(file => !file.EndsWith(".manifest"));

		foreach (var file in files)
		{
			string key = Path.GetFileName(file);
			var relative = Path.Combine(BundleLocation, key);
			if (AssetBundleUtilities.AssetBundles.TryGetValue(key, out var tuple))
			{
				var bundle = tuple.bundle;
				foreach (var prefab in bundle.GetAllAssetNames().Where(asset => asset.EndsWith(".prefab")))
				{
					SurfaceTypeHandler.HandleMaterials(AssetBundleUtilities.LoadPrefab(relative, prefab, this));
				}
			}
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