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

		private void Start()
		{
			_overheadDetector = this.GetRequiredComponent<OverheadDetector>();
		}
	}
}
