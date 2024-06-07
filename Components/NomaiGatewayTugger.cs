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
		private NomaiInterfaceSlot _openSlot;
		private NomaiInterfaceSlot _closedSlot;

		public override void Start()
		{
			_nomaiGateway = this.GetRequiredComponent<NomaiGateway>();
			_orb = _nomaiGateway._orb;
			if (_orb == null) _orb = _nomaiGateway.GetComponentInChildren<NomaiInterfaceOrb>(true);
			_openSlot = _nomaiGateway._closeSlot;
			_closedSlot = _nomaiGateway._closeSlot;
			base.Start();
		}

		public override void OnMoonOverhead(OWRigidbody bodyOverhead)
		{
			if (canOpenAndClose) Open();
			else TugToOpen();
		}

		public override void OnMoonNoLongerOverhead(OWRigidbody bodyOverhead)
		{
			if (canOpenAndClose) Close();
			else TugToClose();
		}

		public float GetDistanceBetweenSlots()
		{
			return Vector3.Distance(_openSlot.transform.position, _closedSlot.transform.position);
		}

		public Vector3 GetPositionTowards(Vector3 a, Vector3 b)
		{
			var direction = (b - a).normalized;
			return (a + ((GetDistanceBetweenSlots() / 2) * direction));
		}

		private void Open()
		{
			if (_orb._belowSand) return;
			if (!_nomaiGateway._open)
			{
			}
		}

		private void Close()
		{
			if (_orb._belowSand) return;
			if (_nomaiGateway._open)
			{
			}
		}

		private void TugToOpen()
		{
			if (_orb._belowSand) return;
			if (!_nomaiGateway._open)
			{
			}
			else
			{
			}
		}

		private void TugToClose()
		{
			if (_orb._belowSand) return;
			if (_nomaiGateway._open)
			{
			}
			else
			{
			}
		}
	}
}
