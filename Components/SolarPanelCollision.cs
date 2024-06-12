using System.Collections.Generic;
using UnityEngine;

namespace Stowaway.Components
{
	internal class SolarPanelCollision : MonoBehaviour
	{
		private OWAudioSource _audioSource;
		private GameObject[] _zappers;
		private bool _broken = false;
		private IslandController _island;
		private SolarTideStormShelter _solarTideStormShelter;

		public void SetIsland(IslandController island)
		{
			_island = island;
		}

		public void SetSolarTideStormShelter(SolarTideStormShelter solarTideHandler)
		{
			_solarTideStormShelter = solarTideHandler;
		}

		public void Bonk()
		{
			if (_broken)
				return;

			foreach (var zapper in _zappers)
			{
				zapper.SetActive(true);
			}
			_audioSource.PlayOneShot(global::AudioType.ElectricShock, 1f);
			_broken = true;
			if (_island != null)
			{
				if (_island._tractorBeamsActive) _island.SetSafetyBeamActivation(false);
				_island._safetyTractorBeams = new SafetyTractorBeamController[0];
			}
			if (_solarTideStormShelter != null)
			{
				_solarTideStormShelter.Deactivate();
			}
		}

		private void Start()
		{
			_audioSource = gameObject.AddComponent<OWAudioSource>();
			var list = new List<GameObject>();
			foreach (Transform child in gameObject.transform)
			{
				if (child == null)
					continue;

				if (child.gameObject.name.Contains("Zap"))
				{
					list.Add(child.gameObject);
					child.gameObject.SetActive(false);
				}
			}
			_zappers = list.ToArray();
		}
	}
}