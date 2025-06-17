using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	public class QuantumMoonAwareForceDetector : ConstantForceDetector
	{
		private QuantumOrbit _orbit;
		private GravityVolume _qmGravity;
		private ForceDetector _qmDetector;
		private bool _isLocked;

		public new void Start()
		{
			base.Start();
			_orbit = Locator.GetAstroObject(AstroObject.Name.BrittleHollow)?.GetComponent<QuantumOrbit>();
			_qmGravity = Locator.GetQuantumMoon().GetComponentInChildren<GravityVolume>(true);
			_qmDetector = _qmGravity.GetAttachedForceDetector();
			_isLocked = _orbit._stateIndex == Locator.GetQuantumMoon().GetStateIndex();
			GlobalMessenger<OWRigidbody>.AddListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			GlobalMessenger<OWRigidbody>.RemoveListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);
		}

		public override void AccumulateAcceleration()
		{
			base.AccumulateAcceleration();

			if (IsQuantumMoonLockedToOrbit())
			{
				if (_qmGravity != null) _netAcceleration += _qmGravity.CalculateForceAccelerationOnBody(_attachedBody) * _fieldMultiplier;
				if (_qmDetector != null) _netAcceleration += _qmDetector.GetForceAcceleration();
			}
		}

		public bool IsQuantumMoonLockedToOrbit() => _isLocked;

		private void OnQuantumMoonStateChanged(OWRigidbody qmBody)
		{
			if (qmBody == null || _orbit == null) return;

			QuantumMoon qm = Locator.GetQuantumMoon();
			if (qm == null) return;

			_isLocked = (_orbit._stateIndex == qm.GetStateIndex());
		}
	}
}
