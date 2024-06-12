using HarmonyLib;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(HazardDetector))]
	public static class HazardDetectorPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(HazardDetector.Update))]
		public static void HazardDetector_Update_Postfix(HazardDetector __instance)
		{
			//No ghost matter damage when in golem connection
			if (Stowaway.Instance.IsGolemConnection)
			{
				__instance._darkMatterDamagePerSecond = 0f;
				__instance._electricityDamagePerSecond = 0f;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(HazardDetector.InHazardType))]
		public static bool HazardDetector_InHazardType_Prefix(ref bool __result, HazardVolume.HazardType type)
		{
			if(Stowaway.Instance.IsGolemConnection && (type == HazardVolume.HazardType.DARKMATTER || type == HazardVolume.HazardType.ELECTRICITY))
			{
				__result = false;
				return false;
			}
			return true;
		}
	}
}
