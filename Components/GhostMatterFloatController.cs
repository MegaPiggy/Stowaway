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
		private OverheadDetector _overheadDetector;

		public void Start()
		{
			_overheadDetector = this.GetRequiredComponent<OverheadDetector>();
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
