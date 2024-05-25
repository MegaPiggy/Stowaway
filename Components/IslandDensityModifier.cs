using UnityEngine;

namespace Stowaway.Components
{
	internal class IslandDensityModifier : MonoBehaviour
	{
		public float DensityModifierSun;
		public float DensityModifierMoon;
		public float DensityModifierBoth;
		public float MaxDensity = 9999f;

		private OWRigidbody _giantsDeep, _sun, _moon, _self;
		private QuantumMoon _quantumMoonBehaviour;
		private DynamicFluidDetector _fluidDetector;

		private float _startDensity;

		private const int GDState = 3;

		private bool _bothOverhead = false;

		private void Awake()
		{
			_self = GetComponent<OWRigidbody>();
			_giantsDeep = Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetOWRigidbody();
			_sun = Locator.GetAstroObject(AstroObject.Name.Sun).GetOWRigidbody();
			_moon = Locator.GetAstroObject(AstroObject.Name.QuantumMoon).GetOWRigidbody();
			if (_moon)
			{
				_quantumMoonBehaviour = _moon.GetComponent<QuantumMoon>();
			}
			_fluidDetector = GetComponentInChildren<DynamicFluidDetector>();
			if(_fluidDetector != null)
			{
				_startDensity = _fluidDetector.GetBuoyancyData().density;
			}
		}

		private float smoothstep(float edge0, float edge1, float x)
		{
			// Scale, and clamp x to 0..1 range
			x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));

			return x * x * (3.0f - 2.0f * x);
		}

		private void FixedUpdate()
		{
			if (!_fluidDetector)
				return;

			float angleToMoon = _quantumMoonBehaviour != null && _quantumMoonBehaviour._stateIndex == GDState ? getCosTo(_moon) : 0f;
			float angleToSun = getCosTo(_sun);

			var bothFactor = Mathf.Clamp01(smoothstep(0.707f, 0.85f, angleToMoon) * smoothstep(0.707f, 0.866f, angleToSun));

			var density = Mathf.Min(MaxDensity, _startDensity + (angleToMoon * DensityModifierMoon + angleToSun * DensityModifierSun) * (1f - bothFactor) + DensityModifierBoth * bothFactor);
			
			if (bothFactor > 0.0f != _bothOverhead)
			{
				_bothOverhead = bothFactor > 0.0f;
				Stowaway.Write($"Both sun and moon are {(_bothOverhead ? "" : "no longer")} above {transform.name}");
			}
			_fluidDetector.GetBuoyancyData().density = density;
		}

		private float getCosTo(OWRigidbody target)
		{
			var v1 = (_self.transform.position - _giantsDeep.transform.position).normalized;
			var v2 = (target.transform.position - _giantsDeep.transform.position).normalized;

			var cos = Vector3.Dot(v1, v2);
			return Mathf.Max(0f, cos);
		}
	}
}
