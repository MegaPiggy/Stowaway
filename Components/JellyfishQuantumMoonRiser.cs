using System;
using UnityEngine;

namespace Stowaway.Components
{
	[RequireComponent(typeof(JellyfishController))]
	public class JellyfishQuantumMoonRiser : MonoBehaviour
	{
		private QuantumOrbit _orbit;
		private JellyfishController _controller;
		private bool _isLocked;
		private float _upperLimit = 496; // Vanilla: 350
		private float _acceleration = 20; // Vanilla: 10;

		public void Start()
		{
			_controller = GetComponent<JellyfishController>();
			_orbit = _controller._jellyfishBody.GetOrigParentBody().GetComponent<QuantumOrbit>();
			GlobalMessenger<OWRigidbody>.AddListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
		}

		public void OnDestroy()
		{
			GlobalMessenger<OWRigidbody>.RemoveListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
		}

		public void FixedUpdate()
		{
			if (_isLocked)
			{
				float sqrMagnitude = (_controller._jellyfishBody.GetPosition() - _controller._planetBody.GetPosition()).sqrMagnitude;
				if (sqrMagnitude <= _upperLimit * _upperLimit)
				{
					_controller._jellyfishBody.AddAcceleration(transform.up * _acceleration);
				}
				else
				{
					_controller._jellyfishBody.AddAcceleration(-transform.up * _acceleration);
				}
			}
		}

		public void OnQuantumMoonStateChanged(OWRigidbody qmBody)
		{
			if (qmBody == null || _orbit == null) return;

			var qm = Locator.GetQuantumMoon();
			if (qm == null) return;

			if (_orbit._stateIndex == qm.GetStateIndex())
			{
				_isLocked = true;
				_controller.enabled = false;
			}
			else
			{
				_isLocked = false;
				_controller.enabled = true;
			}
		}
	}
}
