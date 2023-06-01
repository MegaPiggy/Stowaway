using UnityEngine;

namespace TheStowaways.Components
{
    internal class ShipCollisionComponent : MonoBehaviour
    {
        void OnCollisionEnter(Collision collision)
        {
            var solarPanel = collision.collider?.transform?.parent?.GetComponent<SolarPanelCollisionComponent>();
            if (solarPanel != null)
            {
                solarPanel.Bonk();
            }
        }
    }
}
