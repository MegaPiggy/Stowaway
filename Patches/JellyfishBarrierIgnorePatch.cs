using HarmonyLib;
using UnityEngine;
using Stowaway.Components;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(SphereOceanFluidVolume))]
	public class JellyfishBarrierIgnorePatch
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SphereOceanFluidVolume.GetOceanCurrentVelocity))]
		public static bool SphereOceanFluidVolume_GetOceanCurrentVelocity_Prefix(Vector3 displacement, FluidDetector detector, ref Vector3 __result)
		{
			if (detector != null && detector.GetComponent<JellyfishBarrierIgnorer>() != null)
			{
				__result = Vector3.zero;
				return false;
			}
			return true;
		}
	}
}
