using NewHorizons.Utility;
using System;
using UnityEngine;

namespace TheStowaways.Components
{
    internal class BrambleIslandComponent : MonoBehaviour
    {
        GameObject _brambleIsland;
        AlignWithDirection _alignBehaviour;
        IslandController _islandController;
        PullTornadoComponent _pullTornado;

        private const float SecondsUntilFlip = 0f;

        bool _timeThresholdPassed;
        bool _flipped;

        void Start()
        {
            _brambleIsland = gameObject;
            _alignBehaviour = _brambleIsland.GetComponent<AlignWithDirection>();
            _islandController = _brambleIsland.GetComponent<IslandController>();
            _islandController.OnIslandEnteredTornadoEvent += OnEnterTornado;
        }

        private void OnEnterTornado()
        {
            if(_timeThresholdPassed && !_flipped)
            {
                //No longer pull a tornado towards the island
                if(_pullTornado != null)
                    _pullTornado.enabled = false;
                flip();
            }
        }

        void Update()
        {
            if (!_timeThresholdPassed && TimeLoop.GetSecondsElapsed() > SecondsUntilFlip)
            {
                _timeThresholdPassed = true;
                //Find the tornado closest to bramble island and start moving it towards the island
                var closestTornado = findClosestsMovingTornado();
                if (closestTornado != null)
                {
                    //_pullTornado = closestTornado.AddComponent<PullTornadoComponent>();
                }
            }
        }

        void FixedUpdate()
        {
            if (_flipped)
            {
                updateInterpolationRate();
            }
        }

        private void updateInterpolationRate()
        {
            var degreesToTarget = Vector3.Angle(_alignBehaviour._currentDirection, _alignBehaviour._alignmentDirection);
            _alignBehaviour._interpolationRate = Math.Min(15f, 40f * (degreesToTarget / 180f));
        }

        private void flip()
        {
            TheStowaways.Write("Flipping Bramble Island");
            _alignBehaviour.SetLocalAlignmentAxis(new Vector3(0, 1f, 0f));
            updateInterpolationRate();
            _flipped = true;
            var barrierRepel = SearchUtilities.Find("BrambleIsland_Body/Sector_BrambleIsland/Volumes_BrambleIsland/BarrierRepelFluidVolume (1)");
            if(barrierRepel != null)
            {
                TheStowaways.Write("Disabling barrier repel volume");
                barrierRepel.SetActive(false);
            }
            var zeroGGameVolume = SearchUtilities.Find("BrambleIsland_Body/Sector_BrambleIsland/Volumes_BrambleIsland/ZeroGVolume (2)")?.GetComponent<OWTriggerVolume>();
            if (zeroGGameVolume?._shape is CapsuleShape capsule)
            {
                capsule.center = new Vector3(0, -30, 0);
                capsule.height += 60f;
                capsule.radius += 10f;
                TheStowaways.Write("Increasing size of Zero G Volume");
            }
        }

        private GameObject findClosestsMovingTornado()
        {
            var tornadoes = SearchUtilities.Find("GiantsDeep_Body/Sector_GD/Sector_GDInterior/Tornadoes_GDInterior/MovingTornadoes");
            GameObject closest = null;
            float closestDist = float.MaxValue;
            foreach(var tornado in tornadoes.GetAllChildren())
            {
                float dist;
                if((dist = Vector3.Distance(_brambleIsland.transform.position, tornado.transform.position)) < closestDist)
                {
                    closestDist = dist;
                    closest = tornado;
                }
            }
            return closest;
        }
    }
}
