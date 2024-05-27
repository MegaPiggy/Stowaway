using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Stowaway.Components
{
	public class QuantumMoonChopZone : OceanChopZone
	{
		private QuantumOrbit _orbit;
		private bool _isLocked;

		public const float qmChopRadius = 73;

		public float _lastRadius = 0;
		public float _currentRadius = 0;
		public float _startTime = 0;
		public float _endTime => _startTime + 2;

		public void Awake()
		{
			_radius = 0;
			var body = this.GetAttachedOWRigidbody();
			_orbit = body.GetComponent<QuantumOrbit>();
			_ocean = body.GetComponentInChildren<OceanEffectController>(true);
			GlobalMessenger<OWRigidbody>.AddListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
		}

		public void OnDestroy()
		{
			GlobalMessenger<OWRigidbody>.RemoveListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
		}

		public Vector3 GetPositionBetweenPlanetAndQM()
		{
			var planet = this.GetAttachedOWRigidbody().GetPosition();
			var quantumMoon = Locator.GetQuantumMoon().GetAttachedOWRigidbody().GetPosition();
			var direction = (quantumMoon - planet).normalized;
			return (planet + (500 * direction));
		}

		public void Update()
		{
			if (_isLocked)
			{
				_currentRadius = Mathf.Lerp(_lastRadius, qmChopRadius, Mathf.InverseLerp(_startTime, _endTime, Time.time));
				transform.position = GetPositionBetweenPlanetAndQM();
			}
			else
			{
				_currentRadius = Mathf.Lerp(_lastRadius, 0, Mathf.InverseLerp(_startTime, _endTime, Time.time));
			}
			_radius = _currentRadius;
		}

		public void OnQuantumMoonStateChanged(OWRigidbody qmBody)
		{
			if (qmBody == null || _orbit == null) return;

			if (_orbit._stateIndex == qmBody.GetComponent<QuantumMoon>().GetStateIndex())
			{
				_isLocked = true;
				_startTime = Time.time;
				_lastRadius = _currentRadius;
			}
			else
			{
				_isLocked = false;
				_startTime = Time.time;
				_lastRadius = _currentRadius;
			}
		}
	}
}
