using System;
using UnityEngine;

namespace Stowaway.Components
{
	public class StowawaySector : Sector
	{
		public void Start()
		{
			foreach (var component in GetComponentsInChildren<Component>(true))
			{
				try
				{
					FixSectoredComponent(component);
				}
				catch (Exception e)
				{
					Stowaway.WriteError($"Failed to correct component {component?.GetType()?.Name} on {name} - {e}");
				}
			}
		}

		/// <summary>
		/// Fix components that have sectors.
		/// </summary>
		private void FixSectoredComponent(Component component)
		{
			if (component is ISectorGroup sectorGroup)
			{
				sectorGroup.SetSector(this);
			}
			else if (component is SectoredMonoBehaviour behaviour)
			{
				behaviour.SetSector(this);
			}
			else if (component is OWItemSocket socket)
			{
				socket._sector = this;
			}
			else if (component is NomaiRemoteCameraPlatform remoteCameraPlatform)
			{
				remoteCameraPlatform._visualSector2 = this;
			}
			else if (component is SingleLightSensor singleLightSensor)
			{
				if (singleLightSensor._sector != null)
				{
					singleLightSensor._sector.OnSectorOccupantsUpdated -= singleLightSensor.OnSectorOccupantsUpdated;
				}
				singleLightSensor._sector = this;
				if (singleLightSensor._sector != null)
				{
					singleLightSensor._sector.OnSectorOccupantsUpdated += singleLightSensor.OnSectorOccupantsUpdated;
				}
			}
			else if (component is Campfire campfire)
			{
				if (campfire._sector != null)
				{
					campfire._sector.OnSectorOccupantsUpdated -= campfire.OnSectorOccupantsUpdated;
				}
				campfire._sector = this;
				if (campfire._sector != null)
				{
					campfire._sector.OnSectorOccupantsUpdated += campfire.OnSectorOccupantsUpdated;
				}
			}
		}
	}
}
