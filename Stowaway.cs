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
using static SandFunnelTriggerVolume;
using NewHorizons.External.Modules.VariableSize;

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
		newHorizonsAPI.GetBodyLoadedEvent().AddListener(body => BodyLoaded(body.Replace(" ", "").Replace("'", "")));
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
		Write($"System Loaded: \"{system}\"");
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
		Write($"Body Loaded: \"{body}\"");
		if (body == "Sun")
		{
			initSun_Late();
		}
		if (body == "SunStation")
		{
			initSunStation_Late();
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
		if (body == "HollowsLantern")
		{
			initHollowsLantern_Late();
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
		if (body == "GabbroIsland")
		{
			initGabbroIsland_Late();
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
		if (body.EndsWith("Island") || body == "GabbroShip")
		{
			initIsland(SearchUtilities.Find(body + "_Body"));
		}
		if (body.EndsWith("Inspired"))
		{
			initInspiredComet(NewHorizonsAPI.GetPlanet("Inspired"));
		}
		if (body == "HourglassObservatory")
		{
			initHourglassObservatory(NewHorizonsAPI.GetPlanet("Hourglass Observatory"));
		}
		if (body == "QuantumWeatherLab")
		{
			initQuantumWeatherLab(NewHorizonsAPI.GetPlanet("Quantum Weather Lab"));
		}
	}

	private void initHollowsLantern_Late()
	{
		var hollowsLantern = Locator.GetAstroObject(AstroObject.Name.VolcanicMoon);
		var sector = hollowsLantern.GetComponentInChildren<Sector>(true);
		hollowsLantern._rootSector = sector;
		var meteorLaunchers = hollowsLantern.GetComponentsInChildren<MeteorLauncher>(true);
		int launcherCount = meteorLaunchers.Length;

		float baseInterval = 10.5f;
		float maxInterval = 25.25f;
		float intervalStep = (maxInterval - baseInterval) / (launcherCount - 1);
		float baseSpeed = 100f;
		float maxSpeed = 200f;
		float speedStep = (maxSpeed - baseSpeed) / (launcherCount - 1);
		float baseDamage = 60f;
		float maxDamage = 80f;
		float damageStep = (maxDamage - baseDamage) / (launcherCount - 1);
		Stowaway.Write($"launcherCount: {launcherCount}, baseInterval: {baseInterval}, maxInterval: {maxInterval}, intervalStep: {intervalStep}, baseSpeed: {baseSpeed}, maxSpeed: {maxSpeed}, speedStep: {speedStep}, baseDamage: {baseDamage}, maxDamage: {maxDamage}, damageStep: {damageStep}");

		int count = 0;
		foreach (var meteorLauncher in meteorLaunchers)
		{
			Stowaway.Write($"count: {count}, fixedInterval: {baseInterval + count * intervalStep}, fixedLaunchSpeed: {baseSpeed + count * speedStep}");
			var meteorController = meteorLauncher._meteorPrefab.GetComponent<MeteorController>();
			var originalForceDetector = meteorController._constantForceDetector;
			var newForceDetector = originalForceDetector.gameObject.AddComponent<QuantumMoonAwareForceDetector>();
			newForceDetector._detectableFields = originalForceDetector._detectableFields;
			newForceDetector._inheritDetector = originalForceDetector._inheritDetector;
			newForceDetector._inheritElement0 = originalForceDetector._inheritElement0;
			newForceDetector._fieldMultiplier = originalForceDetector._fieldMultiplier;
			meteorController._constantForceDetector = newForceDetector;
			GameObject.DestroyImmediate(originalForceDetector);

			var damage = baseDamage + count * damageStep;//(meteorController._minDamage + meteorController._maxDamage) / 2;
			meteorController._minDamage = damage;
			meteorController._maxDamage = damage;

			meteorLauncher.gameObject.SetActive(false);
			var modifiedLauncher = meteorLauncher.gameObject.AddComponent<DeterministicMeteorLauncher>();
			modifiedLauncher._meteorPrefab = meteorLauncher._meteorPrefab;
			modifiedLauncher._dynamicMeteorPrefab = meteorLauncher._dynamicMeteorPrefab;
			modifiedLauncher._dynamicProbability = meteorLauncher._dynamicProbability;
			modifiedLauncher._audioSector = meteorLauncher._audioSector;
			modifiedLauncher._detectableField = meteorLauncher._detectableField;
			modifiedLauncher._detectableFluid = meteorLauncher._detectableFluid;
			modifiedLauncher._minLaunchSpeed = meteorLauncher._minLaunchSpeed;
			modifiedLauncher._maxLaunchSpeed = meteorLauncher._maxLaunchSpeed;
			modifiedLauncher._fixedLaunchSpeed = baseSpeed + count * speedStep; //(meteorLauncher._minLaunchSpeed + meteorLauncher._maxLaunchSpeed) / 2;
			modifiedLauncher._minInterval = meteorLauncher._minInterval;
			modifiedLauncher._maxInterval = meteorLauncher._maxInterval;
			modifiedLauncher._fixedInterval = baseInterval + count * intervalStep; //(meteorLauncher._minInterval + meteorLauncher._maxInterval + count) / 2;
			modifiedLauncher._launchParticles = meteorLauncher._launchParticles;
			modifiedLauncher._launchSource = meteorLauncher._launchSource;
			modifiedLauncher._launchDirection = meteorLauncher._launchDirection;
			GameObject.DestroyImmediate(meteorLauncher);
			modifiedLauncher.gameObject.SetActive(true);
			count++;
		}

		hollowsLantern.transform.Find("MoltenCore_VM/MoltenCore_Proxy/LavaSphere (1)").gameObject.AddComponent<ProxyShadowCaster>();
		sector.transform.Find("VolcanicMoonVolume_Sector_Ruleset").GetComponent<SphereShape>().radius = 1000;
	}

	private void initHourglassObservatory(GameObject hourglassObservatory)
	{
		var sector = hourglassObservatory.transform.Find("Sector");
		var rfVolume = hourglassObservatory.transform.Find("RFVolume");
		var volumes = hourglassObservatory.transform.Find("Volumes");
		var fieldDetector = hourglassObservatory.transform.Find("FieldDetector");

		var pos = Vector3.down * 900;
		sector.localPosition = pos;

		var centeredPos = Vector3.down * 935;
		rfVolume.localPosition = centeredPos;
		volumes.localPosition = centeredPos;
		fieldDetector.localPosition = centeredPos;

		var rot = new Vector3(0, 180, 180);
		sector.localEulerAngles = rot;
		rfVolume.localEulerAngles = rot;
		volumes.localEulerAngles = rot;
		fieldDetector.localEulerAngles = rot;

		var sectorSphere = sector.GetComponent<SphereShape>();
		//sectorSphere.radius = 200;
		sectorSphere.center = pos - centeredPos;

		var rfv = rfVolume.GetComponent<ReferenceFrameVolume>();
		rfv._minColliderRadius = 32.5f;
		rfv._maxColliderRadius = 65;
		rfv._referenceFrame._minSuitTargetDistance = 0;
		rfv._referenceFrame._maxTargetDistance = 200;
		rfv._referenceFrame._autopilotArrivalDistance = 0;
		rfv._referenceFrame._autoAlignmentDistance = 0;
		rfv._referenceFrame._matchAngularVelocity = false;
		rfv._referenceFrame._minMatchAngularVelocityDistance = 0;
		rfv._referenceFrame._maxMatchAngularVelocityDistance = 0;
		rfv._referenceFrame._bracketsRadius = 50;
		rfv._referenceFrame._useCenterOfMass = false;
		rfv._referenceFrame._localPosition = centeredPos;

		Delay.RunWhen(() => ProxyHandler.GetProxy("Hourglass Observatory") != null, () =>
		{
			var proxy = ProxyHandler.GetProxy("Hourglass Observatory");
			proxy.root.transform.localPosition = pos;
			proxy.root.transform.localEulerAngles = rot;
		});
	}

	private void initQuantumWeatherLab(GameObject quantumWeatherLab)
	{
		var sector = quantumWeatherLab.transform.Find("Sector");
		var rfVolume = quantumWeatherLab.transform.Find("RFVolume");
		var volumes = quantumWeatherLab.transform.Find("Volumes");
		var fieldDetector = quantumWeatherLab.transform.Find("FieldDetector");

		var pos = Vector3.up * 400;
		sector.localPosition = pos;
		rfVolume.localPosition = pos;
		volumes.localPosition = pos;
		fieldDetector.localPosition = pos;

		var rot = Vector3.zero;
		sector.localEulerAngles = rot;
		rfVolume.localEulerAngles = rot;
		volumes.localEulerAngles = rot;
		fieldDetector.localEulerAngles = rot;

		var rfv = rfVolume.GetComponent<ReferenceFrameVolume>();
		rfv._referenceFrame._minSuitTargetDistance = 0;
		rfv._referenceFrame._autopilotArrivalDistance = 0;
		rfv._referenceFrame._autoAlignmentDistance = 0;
		rfv._referenceFrame._matchAngularVelocity = false;
		rfv._referenceFrame._minMatchAngularVelocityDistance = 0;
		rfv._referenceFrame._maxMatchAngularVelocityDistance = 0;
		rfv._referenceFrame._useCenterOfMass = false;
		rfv._referenceFrame._localPosition = pos;

		Delay.RunWhen(() => ProxyHandler.GetProxy("Quantum Weather Lab") != null, () =>
		{
			var proxy = ProxyHandler.GetProxy("Quantum Weather Lab");
			proxy.root.transform.localPosition = pos;
			proxy.root.transform.localEulerAngles = rot;
		});

		sector.transform.Find("QWeatherLab main sect/Interactibles/QWL_QuantumMeteorWatcher_Body_Geo/QWL peephole/Peephole/PeepholeCamera").GetComponent<OWCamera>().farClipPlane = 1500;
	}

	private void initSunStation_Late()
	{
		var ss = Locator.GetAstroObject(AstroObject.Name.SunStation);
		var sector = ss.GetComponentInChildren<Sector>(true);
		ss._rootSector = sector;

		AddSingularityController(sector, "BlackHole/BlackHoleRenderer", SingularityModule.SingularityType.BlackHole, false);
	}

	private void initHourglassTwins_Late()
	{
		var hgt = Locator.GetAstroObject(AstroObject.Name.HourglassTwins);
		var sector = hgt.GetRootSector();
	}

	private void initNewFocalPoint_Late(GameObject gameObject)
	{
		var hgt = gameObject.GetComponent<NHAstroObject>();
		hgt._name = AstroObject.Name.HourglassTwins;
		hgt.isVanilla = true;
		var mapMarker = gameObject.GetComponent<MapMarker>();
		mapMarker._labelID = UITextType.LocationHGT_Cap;
		var emberAstroObject = Locator.GetAstroObject(AstroObject.Name.CaveTwin);
		var ashAstroObject = Locator.GetAstroObject(AstroObject.Name.TowerTwin);
		var emberObject = emberAstroObject.gameObject;
		var ashObject = ashAstroObject.gameObject;
		var ember = emberAstroObject.GetRootSector();
		var ash = ashAstroObject.GetRootSector();
		var tli = ash.gameObject.FindChild("Sector_TimeLoopInterior").GetComponent<Sector>();
		var hgo = NewHorizonsAPI.GetPlanet("Hourglass Observatory").GetComponent<AstroObject>().GetRootSector();
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
		hgo.SetParentSector(sector);
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
		Delay.FireInNUpdates(() =>
		{
			sandFunnel.SetActive(true);
			body.UnsuspendImmediate(false);
			body.GetAttachedForceDetector().enabled = true;
		}, 3);
		var sector = nhgt.GetComponent<AstroObject>().GetRootSector();
		sandFunnel.GetComponentInChildren<SectorProxy>(true).SetSector(sector);
		sandFunnel.GetComponentInChildren<SectorCullGroup>(true).SetSector(sector);
		sandFunnel.GetComponentInChildren<SectorCollisionGroup>(true).SetSector(sector);
		sandFunnel.transform.Find("ScaleRoot/Geo_SandFunnel/Effects_HT_SandColumn/SandColumn_Exterior").gameObject.AddComponent<ProxyShadowCaster>();
	}

	private void initSun_Late()
	{
		var sun = Locator.GetAstroObject(AstroObject.Name.Sun);
		var sector = sun.GetRootSector();
		//AddSingularityController(sector, "WhiteHole/WhiteHoleRenderer", false);
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

		var sftv = volumesGO.GetComponentInChildren<SandFunnelTriggerVolume>();
		sftv._alignmentOverrideVolumes = giantsDeep.GetComponentsInChildren<DirectionalForceVolume>(true);
		sftv._listObjByExposure = new List<SandFunnelObj>(8);
		sftv._objectsReadyToTrack = new Queue<GameObject>(8);

		var sfv = volumesGO.GetComponentInChildren<SimpleFluidVolume>();
		sfv.name = "FluidVolume_QuantumMoonWaterColumn";
		sfv._fluidType = FluidVolume.Type.WATER;
		sfv._flowSpeed = info.flowSpeed;
		sfv._layer = 6;

		var waterMaterial = new Material(SearchUtilities.Find("BrittleHollow_Body/Sector_BH/Sector_NorthHemisphere/Sector_NorthPole/Geometry_NorthPole/OtherComponentsGroup/Terrain_NorthPoleSurface/BatchedGroup/BatchedMeshRenderers_5").GetComponent<MeshRenderer>().sharedMaterial);
		if (info.tint != null)
		{
			waterMaterial.SetColor("_FogColor", info.tint.ToColor());
		}

		// Reverse so it goes towards QM
		waterMaterial.SetVector("_WaveMovement", waterMaterial.GetVector("_WaveMovement").ReverseY());
		waterMaterial.SetVector("_WaveMovement2", waterMaterial.GetVector("_WaveMovement2").ReverseY());
		waterMaterial.SetVector("_WaveMovement3", waterMaterial.GetVector("_WaveMovement3").ReverseY());

		// Proxy
		var proxyExterior = proxyGO.transform.Find("SandColumn_Exterior (1)");
		proxyExterior.name = "WaterColumn_Exterior_Proxy";
		var proxyExteriorMR = proxyExterior.GetComponent<MeshRenderer>();
		proxyExteriorMR.sharedMaterial = waterMaterial;
		proxyExteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		// Geometry
		var effects = geoGO.transform.Find("Effects_HT_SandColumn");
		effects.name = "Effects_QuantumMoonWaterColumn";

		var geoExterior = effects.transform.Find("SandColumn_Exterior");
		geoExterior.name = "WaterColumn_Exterior";
		var geoExteriorMR = geoExterior.GetComponent<MeshRenderer>();
		geoExteriorMR.sharedMaterial = waterMaterial;
		geoExteriorMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

		var geoInterior = effects.transform.Find("SandColumn_Interior");
		geoInterior.name = "WaterColumn_Interior";
		var geoInteriorMR = geoInterior.GetComponent<MeshRenderer>();
		geoInteriorMR.sharedMaterial = waterMaterial;
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

		var giantsDeep = Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector();
		var dss = deepStormStation.GetComponent<AstroObject>().GetRootSector();
		dss.SetParentSector(giantsDeep);

		var rfv = rfVolume.GetComponent<ReferenceFrameVolume>();
		rfv._referenceFrame._useCenterOfMass = false;
		rfv._referenceFrame._localPosition = pos;

		Delay.RunWhen(() => ProxyHandler.GetProxy("Deep Storm Station") != null, () =>
		{
			var proxy = ProxyHandler.GetProxy("Deep Storm Station");
			proxy.root.transform.localPosition = pos;
		});

		AddSingularityController(sector.GetComponent<Sector>(), "DSS whitewarpcore socket/Props_NOM_WarpCoreWhite (1)/DSS white hole/WhiteHoleRenderer", SingularityModule.SingularityType.WhiteHole, true);
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

		var rot = new Vector3(270, 167.5f, 0);
		sector.localEulerAngles = rot;
		rfVolume.localEulerAngles = rot;
		volumes.localEulerAngles = rot;
		fieldDetector.localEulerAngles = rot;
		gravityWell.localEulerAngles = rot;

		var rfv = rfVolume.GetComponent<ReferenceFrameVolume>();
		rfv._minColliderRadius = 200;
		rfv._maxColliderRadius = 400;
		rfv._referenceFrame._minSuitTargetDistance = 0;
		rfv._referenceFrame._maxTargetDistance = 600;
		rfv._referenceFrame._autopilotArrivalDistance = 0;
		rfv._referenceFrame._autoAlignmentDistance = 0;
		rfv._referenceFrame._matchAngularVelocity = false;
		rfv._referenceFrame._minMatchAngularVelocityDistance = 0;
		rfv._referenceFrame._maxMatchAngularVelocityDistance = 0;
		rfv._referenceFrame._bracketsRadius = 200;
		rfv._referenceFrame._useCenterOfMass = false;
		rfv._referenceFrame._localPosition = pos;

		Delay.RunWhen(() => ProxyHandler.GetProxy("Inspired") != null, () =>
		{
			var proxy = ProxyHandler.GetProxy("Inspired");
			proxy.root.transform.localPosition = pos;
			proxy.root.transform.localEulerAngles = rot;
		});

		AddSingularityController(sector.GetComponent<Sector>(), "BlackHole/BlackHoleRenderer", SingularityModule.SingularityType.BlackHole, true);
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
	}

	private void initAshTwin_Late()
	{
		var ashTwin = Locator.GetAstroObject(AstroObject.Name.TowerTwin);
		var sector = ashTwin.GetRootSector();
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
		CreateInvertedOccluder(sector.transform, "CloudsInvertedOccluder", 40);
		sector.GetComponent<SphereShape>().radius = 1000;
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
		var sector = caveTwin.GetRootSector();
		sector.GetComponent<SphereShape>().radius = 1500;
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
		var collider = body.GetComponentInChildren<ForceApplier>(true).GetComponent<Collider>();
		if (body.FindChild("RepellentVolume") == null)
		{
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
			else if (collider is BoxCollider bCollider)
			{
				var box = repellent.AddComponent<BoxCollider>();
				box.size = bCollider.size * sizeMultiplier;
				box.center = bCollider.center;
				box.isTrigger = true;
			}
			repellent.AddComponent<OWTriggerVolume>();
			repellent.AddComponent<OtherIslandRepelFluidVolume>();
			repellent.AddComponent<DebugVolume>();
		}
		if (collider.GetComponent<Shape>() == null)
		{
			if (collider is SphereCollider sCollider)
			{
				var sphereShape = collider.gameObject.AddComponent<SphereShape>();
				sphereShape._collisionMode = Shape.CollisionMode.Detector;
				sphereShape._layer = Shape.Layer.Default;
				sphereShape._layerMask = 5;
				sphereShape.center = sCollider.center;
				sphereShape.radius = sCollider.radius;
				foreach (var detector in collider.GetComponents<Detector>())
				{
					detector._shape = sphereShape;
				}
			}
			else if (collider is CapsuleCollider cCollider)
			{
				var capsuleShape = collider.gameObject.AddComponent<CapsuleShape>();
				capsuleShape._collisionMode = Shape.CollisionMode.Detector;
				capsuleShape._layer = Shape.Layer.Default;
				capsuleShape._layerMask = 5;
				capsuleShape.center = cCollider.center;
				capsuleShape.direction = cCollider.direction;
				capsuleShape.height = cCollider.height;
				capsuleShape.radius = cCollider.radius;
				foreach (var detector in collider.GetComponents<Detector>())
				{
					detector._shape = capsuleShape;
				}
			}
			else if (collider is BoxCollider bCollider)
			{
				var boxShape = collider.gameObject.AddComponent<BoxShape>();
				boxShape._collisionMode = Shape.CollisionMode.Detector;
				boxShape._layer = Shape.Layer.Default;
				boxShape._layerMask = 5;
				boxShape.center = bCollider.center;
				boxShape.size = bCollider.size;
				foreach (var detector in collider.GetComponents<Detector>())
				{
					detector._shape = boxShape;
				}
			}
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
		var rootSector = giantsDeep.GetRootSector();
		var inspired = NewHorizonsAPI.GetPlanet("Inspired").GetComponent<AstroObject>().GetRootSector();
		inspired.SetParentSector(rootSector);

		rootSector.transform.Find("SectorTrigger_GD").GetComponent<SphereShape>().radius = 1700;
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
		DialogueConditionManager.SharedInstance.SetConditionState("GolemDisconnection", conditionState: false);
	}

	private void golemConnectionExited()
	{
		Write("GOLEM CONNECTION EXIT");
		IsGolemConnection = false;
		DialogueConditionManager.SharedInstance.SetConditionState("GolemConnection", conditionState: false);
		DialogueConditionManager.SharedInstance.SetConditionState("GolemDisconnection", conditionState: true);
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

		var solarPanel = constructionYardBody.transform.Find("Sector_ConstructionYard/ConstructYard Solar Panels");
		if (solarPanel)
		{
			var solarPanelComponent = solarPanel.gameObject.GetAddComponent<SolarPanelCollision>();
			solarPanelComponent.SetIsland(constructionYardBody?.GetComponent<IslandController>());
		}

		constructionYardBody.transform.Find("Detector_ConstructionYardIsland").GetComponent<DynamicFluidDetector>()._buoyancy.boundingRadius = 30;
		constructionYardBody.transform.Find("Sector_ConstructionYard/Interactables_ConstructionYard/RingGravity/RingGravityVolume").GetComponent<CylindricalForceVolume>()._acceleration = -15;

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

		var solarPanel = statueIslandBody.transform.Find("Sector_StatueIsland/StatueIsle Solar Panels");
		if (solarPanel)
		{
			var solarPanelComponent = solarPanel.gameObject.GetAddComponent<SolarPanelCollision>();
			solarPanelComponent.SetIsland(statueIslandBody?.GetComponent<IslandController>());
		}
	}

	private void initGabbroIsland_Late()
	{
		var gabbroIsland = SearchUtilities.Find("GabbroIsland_Body");

		gabbroIsland.transform.Find("Detector_GabbroIsland").GetComponent<DynamicFluidDetector>()._buoyancy.boundingRadius = 30;
	}

	private void initDensityComponents()
	{
		var statueIslandBody = SearchUtilities.Find("StatueIsland_Body");
		if(statueIslandBody)
		{
			statueIslandBody.AddComponent<OverheadDetector>();
			statueIslandBody.AddComponent<IslandDensityModifier>();
		}
		var cyardIslandBody = SearchUtilities.Find("ConstructionYardIsland_Body");
		if (cyardIslandBody)
		{
			cyardIslandBody.AddComponent<OverheadDetector>();
			cyardIslandBody.AddComponent<IslandDensityModifier>();
		}
		var brambleIslandBody = SearchUtilities.Find("BrambleIsland_Body");
		if (brambleIslandBody)
		{
			brambleIslandBody.AddComponent<OverheadDetector>();
			brambleIslandBody.AddComponent<IslandDensityModifier>();
		}
		var gabbroIslandBody = SearchUtilities.Find("GabbroIsland_Body");
		if (gabbroIslandBody)
		{
			gabbroIslandBody.AddComponent<OverheadDetector>();
			gabbroIslandBody.AddComponent<IslandDensityModifier>();
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
			WriteSuccess("Found object at path " + key + "/Sector/" + path);
			var gmObject = gmTransform.gameObject;
			var overhead = gmObject.GetAddComponent<OverheadDetector>();
			var gmFloat = gmObject.GetAddComponent<GhostMatterFloatController>();
		}
		else
		{
			WriteError("Cannot find object at path " + key + "/Sector/" + path);
		}
	}

	public static void AddSingularityController(Sector sector, string renderer, SingularityModule.SingularityType singularityType, bool startActive)
	{
		var rendererTransform = sector.transform.Find(renderer);
		if (rendererTransform == null)
		{
			WriteWarning($"Couldn't find \"{renderer}\" in {sector.transform.GetPath()} (this code is potentially running before the renderer is made and will be rerun later)");
			return;
		}

		rendererTransform.gameObject.SetActive(false);
		var sc = rendererTransform.gameObject.AddComponent<SingularityController>();
		sc._startActive = startActive;
		var sl = rendererTransform.gameObject.AddComponent<SingularityLOD>();
		switch (singularityType)
		{
			case SingularityModule.SingularityType.WhiteHole:
				sl._lodMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Effects_WH_WhiteHole_Proxy_mat");
				break;
			case SingularityModule.SingularityType.BlackHole:
			default:
				sl._lodMaterial = SearchUtilities.FindResourceOfTypeAndName<Material>("Effects_BH_BlackHole_Proxy_mat");
				break;
		}
		rendererTransform.gameObject.SetActive(true);
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