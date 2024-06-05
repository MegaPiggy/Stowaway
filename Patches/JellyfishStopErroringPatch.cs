using HarmonyLib;
using UnityEngine;
using Stowaway.Components;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(JellyfishController))]
	public static class JellyfishStopErroringPatch
	{
		public static void SetAttractiveVolumeActivation(this JellyfishController __instance, bool isRising)
		{
			if (__instance._attractiveFluidVolume != null && __instance._attractiveFluidVolume.gameObject.activeSelf)
			{
				__instance._attractiveFluidVolume.SetVolumeActivation(isRising);
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(JellyfishController.Start))]
		public static bool JellyfishController_Start_Prefix(JellyfishController __instance)
		{
			__instance._planetBody = __instance._jellyfishBody.GetOrigParentBody();
			AlignWithTargetBody component = __instance.GetComponent<AlignWithTargetBody>();
			if (component != null) component.SetTargetBody(__instance._planetBody);
			var randomBetweenLimit = Random.Range(__instance._lowerLimit, __instance._upperLimit);
			var distance = __instance._jellyfishBody.GetPosition() - __instance._planetBody.GetPosition();
			__instance.transform.position = __instance._planetBody.GetPosition() + distance.normalized * randomBetweenLimit;
			__instance._isRising = Random.value > 0.5f;
			__instance.SetAttractiveVolumeActivation(!__instance._isRising);
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(JellyfishController.FixedUpdate))]
		public static bool JellyfishController_FixedUpdate_Prefix(JellyfishController __instance)
		{
			float sqrMagnitude = (__instance._jellyfishBody.GetPosition() - __instance._planetBody.GetPosition()).sqrMagnitude;
			if (__instance._isRising)
			{
				__instance._jellyfishBody.AddAcceleration(__instance.transform.up * __instance._upwardsAcceleration);
				if (sqrMagnitude > __instance._upperLimit * __instance._upperLimit)
				{
					__instance._isRising = false;
					__instance.SetAttractiveVolumeActivation(true);
				}
			}
			else
			{
				__instance._jellyfishBody.AddAcceleration(-__instance.transform.up * __instance._downwardsAcceleration);
				if (sqrMagnitude < __instance._lowerLimit * __instance._lowerLimit)
				{
					__instance._isRising = true;
					__instance.SetAttractiveVolumeActivation(false);
				}
			}
			return false;
		}
	}
}
