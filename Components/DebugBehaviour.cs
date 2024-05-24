using UnityEngine;
using UnityEngine.InputSystem;

namespace Stowaway
{
    public class DebugBehaviour : MonoBehaviour
    {
        public void Update()
        {
            if (Keyboard.current[Key.N].wasPressedThisFrame)
            {
                var mh = Stowaway.Instance.ModHelper;
                var thBody = Locator.GetAstroObject(AstroObject.Name.TimberHearth);
                var volMoon = Locator.GetAstroObject(AstroObject.Name.VolcanicMoon);
                mh.Console.WriteLine($"Player Position {Locator.GetPlayerTransform().position}");
                mh.Console.WriteLine($"Timber Hearth Position {thBody.transform.position}");
                mh.Console.WriteLine($"VMOON Position {volMoon.transform.position}");
            }
        }
    }
}
