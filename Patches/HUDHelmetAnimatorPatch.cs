using HarmonyLib;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(HUDHelmetAnimator))]
	public static class HUDHelmetAnimatorPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(HUDHelmetAnimator.OnInstantDamage))]
		public static bool HUDHelmetAnimator_OnInstantDamage_Prefix(float instantDamage, InstantDamageType damageType)
		{
			return !Stowaway.Instance.IsGolemConnection;
		}
	}
}
