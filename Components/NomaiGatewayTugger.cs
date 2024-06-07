using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NomaiMultiPartDoor;

namespace Stowaway.Components
{
	[RequireComponent(typeof(NomaiGateway), typeof(OverheadDetector))]
	public class NomaiGatewayTugger : NomaiTugger
	{
		private NomaiGateway _nomaiGateway;
		private NomaiInterfaceOrb _orb;
		private Transform _slotsRoot;
		private NomaiInterfaceSlot _openSlot;
		private NomaiInterfaceSlot _closedSlot;
		private Vector3 _middlePoint;
		private Vector3 middlePosition => _slotsRoot.TransformPoint(_middlePoint);

		public override void Start()
		{
			_nomaiGateway = this.GetRequiredComponent<NomaiGateway>();
			_orb = _nomaiGateway._orb;
			if (_orb == null) _orb = _nomaiGateway.GetComponentInChildren<NomaiInterfaceOrb>(true);
			_openSlot = _nomaiGateway._openSlot;
			_closedSlot = _nomaiGateway._closeSlot;
			_slotsRoot = _openSlot.transform.parent;
			_middlePoint = _slotsRoot.GetComponentInChildren<OWRail>()._railPoints[1];
			base.Start();
		}

		public override void OnMoonOverhead(OWRigidbody bodyOverhead)
		{
			TugToOpen();
		}

		public override void OnMoonNoLongerOverhead(OWRigidbody bodyOverhead)
		{
			ReleaseTug();
		}

		private void TugToOpen()
		{
			if (_orb._belowSand) return;
			if (!_nomaiGateway._open)
			{
				_orb.StartDragFromPosition(_orb.transform.position);
				_orb.SetTargetPosition(middlePosition);
			}
		}

		private void ReleaseTug()
		{
			if (_orb._belowSand) return;
			_orb.CancelDrag();
		}
	}
}
