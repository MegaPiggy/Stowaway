using System;
using UnityEngine;

namespace Stowaway.Components
{
	[RequireComponent(typeof(SafetyTractorBeamController), typeof(OverheadDetector))]
	public class SolarTideStormShelter : MonoBehaviour
	{
		private SafetyTractorBeamController _stormShelter;
		private OverheadDetector _overheadDetector;

		public void Start()
		{
			_stormShelter = this.GetRequiredComponent<SafetyTractorBeamController>();
			_overheadDetector = this.GetRequiredComponent<OverheadDetector>();
			_overheadDetector.OnSunOverhead += OnSunOverhead;
			_overheadDetector.OnSunNoLongerOverhead += OnSunNoLongerOverhead;
		}

		public void OnDestroy()
		{
			_overheadDetector.OnSunOverhead -= OnSunOverhead;
			_overheadDetector.OnSunNoLongerOverhead -= OnSunNoLongerOverhead;
		}

		private void OnSunOverhead(OWRigidbody bodyOverhead)
		{
			_stormShelter.SetActivation(true);
		}

		private void OnSunNoLongerOverhead(OWRigidbody bodyOverhead)
		{
			_stormShelter.SetActivation(false);
		}

		public void Deactivate()
		{
			_stormShelter.SetActivation(false);
			DestroyImmediate(this);
		}
	}
}
