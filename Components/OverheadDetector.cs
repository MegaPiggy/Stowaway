﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	public class OverheadDetector : MonoBehaviour
	{
		private QuantumOrbit _orbit;
		private QuantumMoon _quantumMoon;
		private OWRigidbody _planet, _sun, _moon, _qm;

		private float _angleToQuantumMoon;
		private float _angleToMoon;
		private float _angleToSun;

		private float _quantumMoonOverhead;
		private float _moonOverhead;
		private float _sunOverhead;

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
			var moon = _planet.GetComponent<AstroObject>().GetMoon();
			_moon = moon != null ? moon.GetOWRigidbody() : null;
			_sun = Locator.GetAstroObject(AstroObject.Name.Sun).GetOWRigidbody();
			_qm = Locator.GetAstroObject(AstroObject.Name.QuantumMoon).GetOWRigidbody();
			_quantumMoon = _qm.GetComponent<QuantumMoon>();
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
			}
			else if (previousQMOverhead && !nowQMOverhead)
			{
				Stowaway.Write("Quantum moon is no longer overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
			}

			var previousMOverhead = IsMoonOverhead();
			_moonOverhead = smoothstep(0.707f, 0.85f, _angleToMoon);
			var nowMOverhead = IsMoonOverhead();
			if (!previousMOverhead && nowMOverhead)
			{
				Stowaway.Write(_moon.name.Replace("_Body", "") + " is overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
				Stowaway.Write("Quantum moon is overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
			}
			else if (previousMOverhead && !nowMOverhead)
			{
				Stowaway.Write(_moon.name.Replace("_Body", "") + " is no longer overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
			}

			var previousSOverhead = IsQuantumMoonOverhead();
			_sunOverhead = smoothstep(0.707f, 0.866f, _angleToSun);
			var nowSOverhead = IsQuantumMoonOverhead();
			if (!previousSOverhead && nowSOverhead)
			{
				Stowaway.Write("Sun is overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
			}
			else if (previousSOverhead && !nowSOverhead)
			{
				Stowaway.Write("Sun is no longer overhead " + gameObject.name.Replace("_Body", "") + " on " + _planet.name.Replace("_Body", ""));
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
	}
}