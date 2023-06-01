using System.Collections.Generic;
using UnityEngine;

namespace TheStowaways.Components
{
    internal class SolarPanelCollisionComponent : MonoBehaviour
    {
        private OWAudioSource _audioSource;
        private GameObject[] _zappers;
        private bool _broken = false;
        private IslandController _island;

        void Start()
        {
            _audioSource = gameObject.AddComponent<OWAudioSource>();
            var list = new List<GameObject>();
            foreach(Transform child in gameObject.transform)
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

        public void SetIsland(IslandController island)
        {
            _island = island;
        }

        public void Bonk()
        {
            if (_broken)
                return;

            foreach(var zapper in _zappers)
            {
                zapper.SetActive(true);
            }
            _audioSource.PlayOneShot(global::AudioType.ElectricShock, 1f);
            _broken = true;
            if(_island != null && _island._tractorBeamsActive)
            {
                _island.SetSafetyBeamActivation(false);
            }
            if (_island != null)
            {
                _island._safetyTractorBeams = new SafetyTractorBeamController[0];
            }
        }
    }
}
