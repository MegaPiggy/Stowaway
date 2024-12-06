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
			_original = transform.position - _rigidbody.transform.position;
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

			}

			if (isMoonOverhead) // moves
			{
				// gradually rises during alignment, then falls back to starting position.
				var moonFactor = _overheadDetector.GetMoonOverheadPercentage();
			}
		}
	}
}
