using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	public class DeterministicMeteorLauncher : MeteorLauncher
	{
		[Tooltip("Fixed interval between meteor launches.")]
		[SerializeField]
		internal float _fixedInterval = 17.75f;

		[Tooltip("Fixed launch speed for all meteors.")]
		[SerializeField]
		internal float _fixedLaunchSpeed = 150f;

		[Tooltip("Always launch standard meteors (disable dynamic ones).")]
		[SerializeField]
		private bool _disableDynamicMeteors = true;

		protected new void Start()
		{
			base.Start();
			_launchDelay = _fixedInterval;
			_lastLaunchTime = Time.time + (30f - _fixedInterval);
			if (_disableDynamicMeteors)
			{
				_dynamicProbability = 0;
				_dynamicMeteorPool = null;
			}
		}

		protected new void FixedUpdate()
		{
			base.FixedUpdate();
			if (_launchedMeteors != null)
			{
				for (int num = _launchedMeteors.Count - 1; num >= 0; num--)
				{
					if (_launchedMeteors[num] == null)
					{
						_launchedMeteors.QuickRemoveAt(num);
					}
					else if (_launchedMeteors[num].isSuspended)
					{
						_meteorPool.Add(_launchedMeteors[num]);
						_launchedMeteors.QuickRemoveAt(num);
					}
				}
			}
			if (_launchedDynamicMeteors != null)
			{
				for (int num2 = _launchedDynamicMeteors.Count - 1; num2 >= 0; num2--)
				{
					if (_launchedDynamicMeteors[num2] == null)
					{
						_launchedDynamicMeteors.QuickRemoveAt(num2);
					}
					else if (_launchedDynamicMeteors[num2].isSuspended)
					{
						_dynamicMeteorPool.Add(_launchedDynamicMeteors[num2]);
						_launchedDynamicMeteors.QuickRemoveAt(num2);
					}
				}
			}
			if (!_initialized || !(Time.time > _lastLaunchTime + _launchDelay))
			{
				return;
			}
			if (!_areParticlesPlaying)
			{
				_areParticlesPlaying = true;
				foreach (var particle in _launchParticles)
					particle.Play();
			}
			// Override randomness with fixed delay
			if (Time.time > _lastLaunchTime + _launchDelay + 2.3f)
			{
				LaunchMeteor();
				_lastLaunchTime = Time.time;
				_launchDelay = _fixedInterval;
				_areParticlesPlaying = false;
				foreach (var particle in _launchParticles)
					particle.Stop();
			}
		}
	}
}
