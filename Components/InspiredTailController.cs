using NewHorizons.Components.SizeControllers;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	public class InspiredTailController : MonoBehaviour
	{
		private Transform _dustTargetTransform;

		public GameObject gasTail;
		public GameObject dustTail;

		private Vector3 _gasTarget;
		private Vector3 _dustTarget;

		public void FixedUpdate()
		{
			UpdateTargetPositions();

			if (dustTail != null)
				dustTail.LookDir(_dustTarget);

			if (gasTail != null)
				gasTail.LookDir(_gasTarget);
		}

		private void UpdateTargetPositions()
		{
			if (_dustTargetTransform != null)
			{
				var target = (transform.position - _dustTargetTransform.transform.position).normalized;

				_gasTarget = target;
				_dustTarget = target;
			}
		}

		public void SetTarget(Transform dustTarget)
		{
			_dustTargetTransform = dustTarget;
		}
	}
}
