using HarmonyLib;
using Stowaway.Components;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(DeathManager))]
	public static class DeathManagerPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(DeathManager.KillPlayer))]
		public static bool DeathManager_KillPlayer_Prefix(DeathType deathType)
		{
			if(Stowaway.Instance.IsGolemConnection)
			{
				var pl = Locator.GetPlayerController().gameObject.GetComponent<PlayerGolem>();
				//Don't resurrect if source platform is inside super nova
				bool resurrect =
					deathType != DeathType.Meditation &&
					pl != null && 
					Locator.GetSunController() != null && 
					!Locator.GetSunController().IsPointInsideSupernova(pl._platform.transform.position);
				if (resurrect)
				{
					//PlayerResurrection resets HP to 100
					//Call OnLeaveBounds after, so the player HP and other status is updated to their correct values
					GlobalMessenger.FireEvent("PlayerResurrection");
				}
				if (pl != null)
				{
					Stowaway.Write("Player died. Killing golem.");
					pl._platform.OnLeaveBounds();
				}

				//If not resurrecting, return true and run the original death code
				return !resurrect;
			}
			return true;
		}
	}
}
