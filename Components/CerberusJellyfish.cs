using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	public class CerberusJellyfish : MonoBehaviour
	{
		private OWRigidbody _planetBody;
		private QuantumOrbit _orbit;
		private float _riseDuration = 30;
		private float _lowerDuration = 60;
		private float _originalY;
		private float _progress;
		private float _risenY;

		public void Start()
		{
			_planetBody = this.GetAttachedOWRigidbody().GetOrigParentBody();
			_orbit = _planetBody.GetComponent<QuantumOrbit>();
			_originalY = transform.localPosition.y;
			_risenY = _originalY + 14;
		}

		public void Update()
		{
			var quantumMoon = Locator.GetQuantumMoon();
			if (quantumMoon != null && _orbit._stateIndex == quantumMoon.GetStateIndex())
				_progress = _progress.RaiseProgress(_riseDuration);
			else
				_progress = _progress.LowerProgress(_lowerDuration);

			transform.localPosition = new Vector3(transform.localPosition.x, GetCurrentHeight(), transform.localPosition.z);
		}

		public float GetProgress() => _progress;

		public float GetCurrentHeight() => Mathf.Lerp(_originalY, _risenY, GetProgress());
	}
}
