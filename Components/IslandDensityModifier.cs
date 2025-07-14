using UnityEngine;

namespace Stowaway.Components
{
	[RequireComponent(typeof(OverheadDetector))]
	internal class IslandDensityModifier : MonoBehaviour
	{
		public float DensityModifierSun = 0.15f;
		//public float DensityModifierMoon = 0.03f;
		//public float DensityModifierBoth = 0.4f;
		public float MaxDensity = 9999f;

		private OverheadDetector _overheadDetector;
		private DynamicFluidDetector _fluidDetector;

		private float _startDensity;

		//private bool _bothOverhead = false;

		private void Awake()
		{
			_overheadDetector = GetComponent<OverheadDetector>();
			_fluidDetector = GetComponentInChildren<DynamicFluidDetector>();
			if (_fluidDetector != null)
			{
				_startDensity = _fluidDetector.GetBuoyancyData().density;
			}
		}

		private void FixedUpdate()
		{
			if (!_fluidDetector)
				return;

			//float angleToMoon = _overheadDetector.AngleToQuantumMoon();
			float angleToSun = _overheadDetector.AngleToSun();

			//var bothFactor = Mathf.Clamp01(_overheadDetector.GetQuantumMoonOverheadPercentage() * _overheadDetector.GetSunOverheadPercentage());

			var density = Mathf.Min(MaxDensity,
				//_startDensity + (angleToMoon * DensityModifierMoon + angleToSun * DensityModifierSun) * (1f - bothFactor) + DensityModifierBoth * bothFactor
				_startDensity + (angleToSun * DensityModifierSun)
			);
			
			/*if (bothFactor > 0.0f != _bothOverhead)
			{
				_bothOverhead = bothFactor > 0.0f;
				Stowaway.Write($"Both sun and moon are {(_bothOverhead ? "" : "no longer")} above {transform.name}");
			}*/
			_fluidDetector.GetBuoyancyData().density = density;
		}
	}
}
