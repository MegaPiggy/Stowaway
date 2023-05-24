using HarmonyLib;

namespace TheStowaways
{
    [HarmonyPatch]
	public class OWItemSocketPatch
    {
		[HarmonyPrefix]
		[HarmonyPatch(typeof(OWItemSocket), nameof(OWItemSocket.AcceptsItem))]
		public static bool OWItemSocket_AcceptsItem_Prefix(ref bool __result, OWItemSocket __instance, OWItem item)
		{
			ItemType itemType = item.GetItemType();
			//Prevent socketing a new shader stone into a camera platform when already inside a golem connection
			if(__instance is SharedStoneSocket sss && 
				sss.transform.parent.GetComponentInParent<NomaiRemoteCameraPlatform>() != null &&
				itemType == ItemType.SharedStone && TheStowaways.Instance.IsGolemConnection)
            {
				__result = false;
				return false;
            }
			return true;
		}
	}
}
