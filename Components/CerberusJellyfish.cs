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
		private float _riseTime = 90;
		private float _currentTime;
		private float _originalY;
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
			{
				_currentTime += Time.deltaTime;
			}
			transform.localPosition = new Vector3(transform.localPosition.x, Mathf.Lerp(_originalY, _risenY, GetProgress()), transform.localPosition.z);
		}

		public float GetProgress()
		{
			return Mathf.Clamp01(_currentTime/_riseTime);
		}
	}
}
