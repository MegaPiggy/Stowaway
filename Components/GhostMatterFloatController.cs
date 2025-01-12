using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	[RequireComponent(typeof(OverheadDetector))]
	public class GhostMatterFloatController : MonoBehaviour
	{
		private OWRigidbody _rigidbody;
		private OverheadDetector _overheadDetector;
		private Vector3 _original;
		private Vector3 _up;
		private float _originalPosY;
		private float _originalScaleY;

		public void Start()
		{
			_rigidbody = this.GetAttachedOWRigidbody();
			_overheadDetector = this.GetRequiredComponent<OverheadDetector>();
			_overheadDetector.SetMoonClamps(0.9f, 0.99f);
			_overheadDetector.SetSunClamps(0.9f, 0.99f);
			_original = _rigidbody.transform.InverseTransformPoint(transform.position);
			_originalPosY = _original.magnitude;
			_up = _original / _original.magnitude;
			_originalScaleY = transform.localScale.y;

		}

		public void OnDestroy()
		{
		}

		public void Update()
		{
			var isMoonOverhead = _overheadDetector.IsMoonOverhead();
			var isSunOverhead = _overheadDetector.IsSunOverhead();
			var riseFactor = 0f;

			if (isMoonOverhead || isSunOverhead) // stretches
			{
				var moonFactor = _overheadDetector.GetMoonOverheadPercentage();
				var sunFactor = _overheadDetector.GetSunOverheadPercentage();
				var bothFactor = Mathf.Clamp01(moonFactor + sunFactor);
				transform.localScale = new Vector3(transform.localScale.x, _originalScaleY + bothFactor, transform.localScale.z);
				riseFactor = (bothFactor / 2);
			}

			if (isMoonOverhead) // moves
			{
				riseFactor *= 10;
			}

			// gradually rises during alignment, then falls back to starting position.
			transform.position = _rigidbody.transform.TransformPoint(_up * (_originalPosY + riseFactor));
		}
	}
}
