﻿using NewHorizons.Utility;
using UnityEngine;

namespace Stowaway.Components
{
	internal class PlayerGolem : MonoBehaviour
	{
		public NomaiRemoteCameraPlatform _platform;
		private float _health;
		private float _fuel;
		private float _oxygen;
		private int _punctures;
		private float _suitIntegrity;
		private float _oxygenAsFuel;

		private Vector3 _storedPosition;
		private Quaternion _storedRotation;
		private Vector3 _storedShapeCenter;
		private Vector3 _storedShapePos;
		private Quaternion _storedShapeRot;

		private static void setComponentsActive(bool active)
		{
			var model = Locator.GetPlayerBody().transform.Find("Traveller_HEA_Player_v2");
			if (model != null)
				model.gameObject.SetActive(active);

			var pc = Locator.GetPlayerCamera();
			var helmet = pc.transform.Find("Helmet");
			if (helmet != null)
				helmet.gameObject.SetActive(active);
		}

		private void Start()
		{
			var pr = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
			if (pr != null)
			{
				_health = pr._currentHealth;
				_fuel = pr._currentFuel;
				_oxygen = pr._currentOxygen;
				_punctures = pr._currentNumPunctures;
				_suitIntegrity = pr._currentSuitIntegrity;
				_oxygenAsFuel = pr._oxygenUsedAsFuel;
			}
			setComponentsActive(false);
			teleportPlayer();
			GlobalMessenger<float>.AddListener("PlayerCameraEnterWater", playerCameraEnterWater);
		}

		private void teleportPlayer()
		{
			_storedPosition = _platform.transform.InverseTransformPoint(Locator.GetPlayerTransform().position);
			_storedRotation = _platform.transform.InverseTransformRotation(Locator.GetPlayerTransform().rotation);
			var connectionBounds = _platform._connectionBounds as BoxShape;
			_storedShapeCenter = connectionBounds.center;
			_storedShapePos = connectionBounds.transform.localPosition;
			_storedShapeRot = connectionBounds.transform.localRotation;

			var platformBody = _platform._slavePlatform.GetAttachedOWRigidbody();
			var relativePlatformPosition = platformBody.transform.InverseTransformPoint(_platform._slavePlatform.transform.position);
			var relativePlatformRotation = platformBody.transform.InverseTransformRotation(_platform._slavePlatform.transform.rotation);

			detachFromShip();
			Teleportation.teleportPlayerTo(platformBody, relativePlatformPosition + _storedPosition, relativePlatformRotation);

			Locator.GetPlayerTransform().position = _platform._slavePlatform.transform.TransformPoint(_storedPosition);
			Locator.GetPlayerTransform().rotation = _platform._slavePlatform.transform.rotation * _storedRotation;
			connectionBounds.center = Vector3.zero;
			connectionBounds.transform.position = Locator.GetPlayerTransform().position;

			NotificationData notificationData = new NotificationData(NotificationTarget.Player, "Test", 3f, true);
			NotificationManager.SharedInstance.PostNotification(notificationData, false);
		}

		private void returnPlayerToStartPosition()
		{
			var platformBody = _platform.GetAttachedOWRigidbody();
			Stowaway.Write($"Teleporting back to {platformBody}");
			var relativePlatformPosition = platformBody.transform.InverseTransformPoint(_platform.transform.position);
			var relativePlatformRotation = platformBody.transform.InverseTransformRotation(_platform.transform.rotation);

			detachFromShip();
			Teleportation.teleportPlayerTo(platformBody, relativePlatformPosition + _storedPosition, relativePlatformRotation);

			Locator.GetPlayerTransform().position = _platform.transform.TransformPoint(_storedPosition);
			Locator.GetPlayerTransform().rotation = _platform.transform.rotation * _storedRotation;

			var connectionBounds = _platform._connectionBounds as BoxShape;
			connectionBounds.center = _storedShapeCenter;
			connectionBounds.transform.localPosition = _storedShapePos;
			connectionBounds.transform.localRotation = _storedShapeRot;
		}

		private void detachFromShip()
		{
			if (PlayerState.IsInsideShip())
			{
				var hc = SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/Hatch/HatchControls").GetComponent<HatchController>();
				if (hc != null)
				{
					hc.OpenHatch();
					hc._isPlayerInShip = false;
				}
				var tb = SearchUtilities.Find("Ship_Body/Module_Cabin/Systems_Cabin/Hatch/TractorBeam").GetComponent<ShipTractorBeamSwitch>();
				if (tb != null)
				{
					tb.ActivateTractorBeam();
				}
				GlobalMessenger.FireEvent("ExitShip");
			}
		}

		private void LateUpdate()
		{
			if (Stowaway.Instance.IsGolemConnection)
			{
				Transform playerTransform = Locator.GetPlayerTransform();
				_platform._connectionBounds.transform.position = playerTransform.position;
			}
		}

		private void OnDestroy()
		{
			Locator.GetToolModeSwapper().GetItemCarryTool().DropItemToGround(_platform._slavePlatform.transform, _platform._slavePlatform._visualSector);
			var pr = Locator.GetPlayerTransform().GetComponent<PlayerResources>();
			if (pr != null)
			{
				pr._currentHealth = _health;
				pr._currentFuel = _fuel;
				pr._currentOxygen = _oxygen;
				pr._currentNumPunctures = _punctures;
				pr._currentSuitIntegrity = _suitIntegrity;
				pr._oxygenUsedAsFuel = _oxygenAsFuel;
			}
			returnPlayerToStartPosition();
			setComponentsActive(true);
			GlobalMessenger<float>.RemoveListener("PlayerCameraEnterWater", playerCameraEnterWater);
		}

		public static bool IsPlayerTouchingWater()
		{
			return Locator.GetPlayerDetector().GetComponent<FluidDetector>().InFluidType(FluidVolume.Type.WATER);
		}

		private void playerCameraEnterWater(float relativeSpeed)
		{
			var isPlayerTouchingWater = IsPlayerTouchingWater();
			if (isPlayerTouchingWater)
			{
				Stowaway.Write("Player touched water. Killing golem.");
				_platform.OnLeaveBounds();
			}
		}
	}
}