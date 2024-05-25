using HarmonyLib;
using Stowaway.Components;
using UnityEngine;

namespace Stowaway
{
	[HarmonyPatch]
	public class NomaiRemoteCameraPlatformPatch
	{
		private static Vector3 _storedPosition;
		private static Quaternion _storedRotation;
		private static Vector3 _storedShapePos;
		private static Quaternion _storedShapeRot;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.SwitchToRemoteCamera))]
		public static bool NomaiRemoteCameraPlatform_SwitchToRemoteCamera_Prefix(NomaiRemoteCameraPlatform __instance)
		{
			if (Stowaway.IsSpecialStone(__instance._sharedStone))
			{
				var comp = Locator.GetPlayerController().gameObject.AddComponent<PlayerGolem>();
				comp._platform = __instance;

				Stowaway.Write($"Switch to remote camera");

				GlobalMessenger.FireEvent("EnterNomaiGolemConnection");
				return true;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.SwitchToPlayerCamera))]
		public static bool NomaiRemoteCameraPlatform_SwitchToPlayerCamera(NomaiRemoteCameraPlatform __instance)
		{
			if (Stowaway.Instance.IsGolemConnection)
			{
				Stowaway.Write($"Switch to player camera");

				var comp = Locator.GetPlayerController().gameObject.GetComponent<PlayerGolem>();
				if (comp != null)
					Object.Destroy(comp);

				GlobalMessenger.FireEvent("ExitNomaiGolemConnection");
				return true;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.Update))]
		public static void NomaiRemoteCameraPlatform_Update(NomaiRemoteCameraPlatform __instance)
		{
			if (__instance._active && Stowaway.Instance.IsGolemConnection)
			{
				if (OWInput.IsPressed(InputLibrary.cancel, 0f) || OWInput.IsPressed(InputLibrary.toolActionPrimary, 0f))
				{
					__instance.OnLeaveBounds();
				}
			}
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiRemoteCameraPlatform), nameof(NomaiRemoteCameraPlatform.UpdateHologramTransforms))]
		public static bool NomaiRemoteCameraPlatform_UpdateHologramTransforms_Prefix(NomaiRemoteCameraPlatform __instance)
		{
			if (!Stowaway.Instance.IsGolemConnection)
				return true;

			__instance._hologramGroup.transform.position = __instance._slavePlatform.transform.position;
			__instance._hologramGroup.transform.rotation = __instance._slavePlatform.transform.rotation;
			Transform playerTransform = Locator.GetPlayerTransform();

			__instance._playerHologram.position = playerTransform.position;
			__instance._playerHologram.rotation = playerTransform.rotation;
			if (__instance._sharedStone != null)
			{
				Transform transform = __instance._sharedStone.transform;
				__instance._stoneHologram.position = NomaiRemoteCameraPlatform.TransformPoint(transform.position, __instance, __instance._slavePlatform);
				__instance._stoneHologram.rotation = NomaiRemoteCameraPlatform.TransformRotation(transform.rotation, __instance, __instance._slavePlatform);
				return false;
			}
			__instance._stoneHologram.SetPositionAndRotation(new Vector3(0f, -2f, 0f), Quaternion.identity);
			return false;
		}
	}
}
