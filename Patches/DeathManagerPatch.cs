using HarmonyLib;
using TheStowaways.Components;

namespace TheStowaways
{
    [HarmonyPatch]
    public class DeathManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DeathManager), nameof(DeathManager.KillPlayer))]
        public static bool DeathManager_KillPlayer_Prefix(DeathType deathType)
        {
            if(TheStowaways.Instance.IsGolemConnection)
            {
                var pl = Locator.GetPlayerController().gameObject.GetComponent<PlayerGolemComponent>();
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
                    pl._platform.OnLeaveBounds();

                //If not resurrecting, return true and run the original death code
                return !resurrect;
            }
            return true;
        }
    }
}
