using HarmonyLib;
using Stowaway.Components;
using UnityEngine;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(VisibilityOccluder))]
	public static class InvertedVisibilityOccluderPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(VisibilityOccluder.CanYouSee))]
		public static void VisibilityOccluder_CanYouSee_Postfix(ShapeVisibilityTracker tracker, Vector3 cameraPos, ref bool __result)
		{
			if (InvertedVisibilityOccluder.DoesAnyOccludeTracker(tracker, cameraPos)) __result = false;
		}
	}
}
