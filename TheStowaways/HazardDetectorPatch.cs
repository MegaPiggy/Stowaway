using HarmonyLib;

namespace TheStowaways
{
    [HarmonyPatch]
    public class HazardDetectorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HazardDetector), nameof(HazardDetector.Update))]
        public static void HazardDetector_Update_Postfix(HazardDetector __instance)
        {
            //No ghost matter damage when in golem connection
            if (TheStowaways.Instance.IsGolemConnection)
            {
                __instance._darkMatterDamagePerSecond = 0f;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HazardDetector), nameof(HazardDetector.InHazardType))]
        public static bool HazardDetector_InHazardType_Prefix(ref bool __result, HazardVolume.HazardType type)
        {
            if(TheStowaways.Instance.IsGolemConnection && type == HazardVolume.HazardType.DARKMATTER)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
