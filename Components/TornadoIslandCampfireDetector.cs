using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	[RequireComponent(typeof(Campfire))]
	public class TornadoIslandCampfireDetector : MonoBehaviour
	{
		private Campfire _campfire;
		private IslandController _islandController;
		private bool _islandAirborne;
		public bool IslandAirborne => _islandAirborne;

		private void Start()
		{
			_campfire = gameObject.GetRequiredComponent<Campfire>();
			_islandController = gameObject.GetAttachedOWRigidbody().GetComponent<IslandController>();
			if (_islandController != null)
			{
				_islandController.OnIslandEnteredTornadoEvent += OnEnterTornado;
				_islandController.OnIslandSplashEvent += OnSplashDown;
			}
		}

		private void OnDestroy()
		{
			if (_islandController != null)
			{
				_islandController.OnIslandEnteredTornadoEvent -= OnEnterTornado;
				_islandController.OnIslandSplashEvent -= OnSplashDown;
			}
		}

		private void OnEnterTornado()
		{
			_islandAirborne = true;
			_campfire.StopSleeping(sudden: true);
		}

		private void OnSplashDown()
		{
			_islandAirborne = false;
		}
	}
}