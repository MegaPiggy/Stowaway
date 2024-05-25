using UnityEngine;

namespace Stowaway.Components
{
	internal class ShipSolarPanelCollision : MonoBehaviour
	{
		private void OnCollisionEnter(Collision collision)
		{
			var solarPanel = collision.collider?.transform?.parent?.GetComponent<SolarPanelCollision>();
			if (solarPanel != null)
			{
				solarPanel.Bonk();
			}
		}
	}
}