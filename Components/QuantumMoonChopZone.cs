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
		private SphereShape _sphereShape;
		private ChopFluidVolume _fluidVolume;
		private QuantumOrbit _orbit;
		private bool _isLocked;

		public const float qmChopRadius = 73;
		public const float gdOceanRadius = 500;

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

			gameObject.layer = LayerMask.NameToLayer("BasicEffectVolume");
			_sphereShape = gameObject.AddComponent<SphereShape>();
			_sphereShape.radius = 0;
			_sphereShape.SetCollisionMode(Shape.CollisionMode.Volume);
			gameObject.AddComponent<OWTriggerVolume>();
			_fluidVolume = gameObject.AddComponent<ChopFluidVolume>();
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
			return (planet + (gdOceanRadius * direction));
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
			var fluidRadius = _currentRadius / 2;
			_sphereShape.radius = fluidRadius;
			_fluidVolume.radius = fluidRadius;
		}

		public void OnQuantumMoonStateChanged(OWRigidbody qmBody)
		{
			if (qmBody == null || _orbit == null) return;

			var qm = Locator.GetQuantumMoon();
			if (qm == null) return;

			if (_orbit._stateIndex == qm.GetStateIndex())
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

		internal class ChopFluidVolume : FluidVolume
		{
			public float buoyancyDensity = 1;
			public float radius;

			public ChopFluidVolume() : base()
			{
				_fluidType = Type.WATER;
			}

			public override bool IsSpherical() => true;

			public override Vector3 GetBuoyancy(FluidDetector detector, float fractionSubmerged)
			{
				if (detector.GetAttachedOWRigidbody().GetAttachedForceDetector() != null)
				{
					Vector3 negativeVector = detector.GetAttachedOWRigidbody().GetAttachedForceDetector().GetForceAcceleration() - _attachedBody.GetAttachedForceDetector().GetForceAcceleration();
					return Vector3.Project(onNormal: detector.transform.position - base.transform.position, vector: -negativeVector) * fractionSubmerged * buoyancyDensity / detector.GetBuoyancyData().density;
				}
				return Vector3.zero;
			}

			public override float GetFractionSubmerged(FluidDetector detector) => detector.GetBuoyancyData().CalculateSubmergedFraction((detector.transform.position - transform.position).magnitude, radius);
		}
	}
}
