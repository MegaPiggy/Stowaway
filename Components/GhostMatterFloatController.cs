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
		private float _originalY;

		public void Start()
		{
			_rigidbody = this.GetAttachedOWRigidbody();
			_overheadDetector = this.GetRequiredComponent<OverheadDetector>();
			_overheadDetector.DefaultDirectMoonClamps();
			_original = _rigidbody.transform.InverseTransformPoint(transform.position);
			_originalY = _original.magnitude;
			_up = _original / _original.magnitude;

		}

		public void OnDestroy()
		{
		}

		public void Update()
		{
			var isMoonOverhead = _overheadDetector.IsMoonOverhead();
			var isSunOverhead = _overheadDetector.IsSunOverhead();

			if (isMoonOverhead || isSunOverhead) // stretches
			{
				var bothFactor = Mathf.Clamp01(_overheadDetector.GetMoonOverheadPercentage() * _overheadDetector.GetSunOverheadPercentage());
				transform.localScale = new Vector3(transform.localScale.x, 1 + ((_overheadDetector.GetMoonOverheadPercentage() + _overheadDetector.GetSunOverheadPercentage()) * 2), transform.localScale.z);
			}

			if (isMoonOverhead) // moves
			{
				// gradually rises during alignment, then falls back to starting position.
				var moonFactor = _overheadDetector.GetMoonOverheadPercentage();
				transform.position = _rigidbody.transform.TransformPoint(_up * (_originalY + (moonFactor * 5)));
			}
		}
	}
}
