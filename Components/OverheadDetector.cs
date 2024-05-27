using UnityEngine;

namespace Stowaway.Components
{
	public class OverheadDetector : MonoBehaviour
	{
		public delegate void OverheadEvent(OWRigidbody bodyOverhead);

		private QuantumOrbit _orbit;
		private QuantumMoon _quantumMoon;
		private OWRigidbody _planet, _sun, _moon, _qm;

		private float _angleToQuantumMoon;
		private float _angleToMoon;
		private float _angleToSun;

		private float _quantumMoonOverhead;
		private float _moonOverhead;
		private float _sunOverhead;

		public event OverheadEvent OnQuantumMoonOverhead;
		public event OverheadEvent OnMoonOverhead;
		public event OverheadEvent OnSunOverhead;
		public event OverheadEvent OnQuantumMoonNoLongerOverhead;
		public event OverheadEvent OnMoonNoLongerOverhead;
		public event OverheadEvent OnSunNoLongerOverhead;

		private float getCosTo(OWRigidbody target)
		{
			if (target == null) return 0;
			var v1 = (transform.position - _planet.transform.position).normalized;
			var v2 = (target.transform.position - _planet.transform.position).normalized;

			var cos = Vector3.Dot(v1, v2);
			return Mathf.Max(0f, cos);
		}

		private void Start()
		{
			_planet = GetParentBody();
			_orbit = _planet.GetComponent<QuantumOrbit>();
			var moon = GetMoon(_planet.GetComponent<AstroObject>());
			_moon = moon != null ? moon.GetOWRigidbody() : null;
			_sun = Locator.GetAstroObject(AstroObject.Name.Sun).GetOWRigidbody();
			_qm = Locator.GetAstroObject(AstroObject.Name.QuantumMoon).GetOWRigidbody();
			_quantumMoon = _qm.GetComponent<QuantumMoon>();
		}

		private AstroObject GetMoon(AstroObject planet)
		{
			switch (planet.GetAstroObjectName())
			{
				case AstroObject.Name.CaveTwin:
					return Locator.GetAstroObject(AstroObject.Name.TowerTwin);
				case AstroObject.Name.TowerTwin:
					return Locator.GetAstroObject(AstroObject.Name.CaveTwin);
				default:
					return planet.GetMoon();
			}
		}

		private OWRigidbody GetParentBody()
		{
			var body = this.GetAttachedOWRigidbody();
			var parentBody = body.GetOrigParentBody();
			return parentBody != null ? parentBody : body;
		}

		private float smoothstep(float edge0, float edge1, float x)
		{
			// Scale, and clamp x to 0..1 range
			x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));

			return x * x * (3.0f - 2.0f * x);
		}

		private void FixedUpdate()
		{
			_angleToQuantumMoon = _quantumMoon != null && _quantumMoon._stateIndex == _orbit._stateIndex ? getCosTo(_qm) : 0f;
			_angleToMoon = getCosTo(_moon);
			_angleToSun = getCosTo(_sun);

			var previousQMOverhead = IsQuantumMoonOverhead();
			_quantumMoonOverhead = smoothstep(0.707f, 0.85f, _angleToQuantumMoon);
			var nowQMOverhead = IsQuantumMoonOverhead();
			if (!previousQMOverhead && nowQMOverhead)
			{
				Stowaway.Write("Quantum moon is overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
				if (OnQuantumMoonOverhead != null) OnQuantumMoonOverhead(_qm);
			}
			else if (previousQMOverhead && !nowQMOverhead)
			{
				Stowaway.Write("Quantum moon is no longer overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
				if (OnQuantumMoonNoLongerOverhead != null) OnQuantumMoonNoLongerOverhead(_qm);
			}

			var previousMOverhead = IsMoonOverhead();
			_moonOverhead = smoothstep(0.707f, 0.85f, _angleToMoon);
			var nowMOverhead = IsMoonOverhead();
			if (!previousMOverhead && nowMOverhead)
			{
				Stowaway.Write(_moon.name.Replace("_Body", "") + " is overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
				if (OnMoonOverhead != null) OnMoonOverhead(_moon);
			}
			else if (previousMOverhead && !nowMOverhead)
			{
				Stowaway.Write(_moon.name.Replace("_Body", "") + " is no longer overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
				if (OnMoonNoLongerOverhead != null) OnMoonNoLongerOverhead(_moon);
			}

			var previousSOverhead = IsQuantumMoonOverhead();
			_sunOverhead = smoothstep(0.707f, 0.866f, _angleToSun);
			var nowSOverhead = IsQuantumMoonOverhead();
			if (!previousSOverhead && nowSOverhead)
			{
				Stowaway.Write("Sun is overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
				if (OnSunOverhead != null) OnSunOverhead(_sun);
			}
			else if (previousSOverhead && !nowSOverhead)
			{
				Stowaway.Write("Sun is no longer overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
				if (OnSunNoLongerOverhead != null) OnSunNoLongerOverhead(_sun);
			}
		}

		public bool IsQuantumMoonOverhead()
		{
			return _quantumMoonOverhead > 0;
		}

		public bool IsMoonOverhead()
		{
			return _moonOverhead > 0;
		}

		public bool IsSunOverhead()
		{
			return _sunOverhead > 0;
		}

		public float GetQuantumMoonOverheadPercentage()
		{
			return _quantumMoonOverhead;
		}

		public float GetMoonOverheadPercentage()
		{
			return _moonOverhead;
		}

		public float GetSunOverheadPercentage()
		{
			return _sunOverhead;
		}

		public float AngleToQuantumMoon()
		{
			return _angleToQuantumMoon;
		}

		public float AngleToMoon()
		{
			return _angleToMoon;
		}

		public float AngleToSun()
		{
			return _angleToSun;
		}
	}
}
