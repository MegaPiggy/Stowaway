using HarmonyLib;

namespace Stowaway
{
    [HarmonyPatch]
	public class NomaiRemoteCameraPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(NomaiRemoteCamera), nameof(NomaiRemoteCamera.LateUpdate))]
		public static bool LateUpdate(NomaiRemoteCamera __instance)
		{
			if (!Stowaway.Instance.IsGolemConnection)
				return true;

			if (__instance._owningPlatform && __instance._controllingPlatform)
			{
				__instance.transform.position = __instance._controllingCamera.transform.position;
				__instance.transform.rotation = __instance._controllingCamera.transform.rotation;
				__instance._camera.fieldOfView = __instance._controllingCamera.fieldOfView;
				return false;
			}
			__instance.enabled = false;
			return false;
		}
	}
}
