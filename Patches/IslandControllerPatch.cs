using HarmonyLib;

namespace Stowaway
{
	[HarmonyPatch(typeof(IslandController))]
	public class IslandControllerPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(IslandController.Awake))]
		public static void IslandController_Awake_Postfix(IslandController __instance)
		{
			if (__instance._fluidDetector != null)
			{
				__instance._fluidDetector.gameObject.tag = "IslandDetector";
			}
		}
	}
}
