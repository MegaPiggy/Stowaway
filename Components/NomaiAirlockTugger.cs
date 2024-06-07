using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NomaiMultiPartDoor;

namespace Stowaway.Components
{
	[RequireComponent(typeof(NomaiAirlock), typeof(OverheadDetector))]
	public class NomaiAirlockTugger : NomaiTugger
	{
		private NomaiAirlock _nomaiAirlock;
		private NomaiInterfaceOrb _orb;
		private NomaiInterfaceSlot _openSlot;
		private NomaiInterfaceSlot _closedSlot;
		
		public override void Start()
		{
			_nomaiAirlock = this.GetRequiredComponent<NomaiAirlock>();
			_orb = _nomaiAirlock._listInterfaceOrb.FirstOrDefault();
			_openSlot = _orb._slots.FirstOrDefault(slot => slot.gameObject.name.EndsWith("Open"));
			if (_openSlot == null) _openSlot = _nomaiAirlock._openSwitches.FirstOrDefault();
			_closedSlot = _orb._slots.FirstOrDefault(slot => slot.gameObject.name.EndsWith("Close"));
			if (_closedSlot == null) _closedSlot = _nomaiAirlock._closeSwitches.FirstOrDefault();
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
			if (_nomaiAirlock._currentRotationState != RotationState.OPEN)
			{
				_orb.StartDragFromPosition(_closedSlot.transform.position);
				_orb.SetTargetPosition(_openSlot.transform.position);
			}
		}

		private void Close()
		{
			if (_orb._belowSand) return;
			if (_nomaiAirlock._currentRotationState != RotationState.CLOSED)
			{
				_orb.StartDragFromPosition(_openSlot.transform.position);
				_orb.SetTargetPosition(_closedSlot.transform.position);
			}
		}

		private void TugToOpen()
		{
			if (_orb._belowSand) return;
			if (_nomaiAirlock._currentRotationState == RotationState.CLOSED)
			{
				_orb.StartDragFromPosition(_orb.transform.position);
				_orb.SetTargetPosition(GetPositionTowards(_orb.transform.position, _openSlot.transform.position));
			}
			else
			{
				_orb.StartDragFromPosition(_orb.transform.position);
				_orb.SetTargetPosition(_openSlot.transform.position);
			}
		}

		private void TugToClose()
		{
			if (_orb._belowSand) return;
			if (_nomaiAirlock._currentRotationState == RotationState.OPEN)
			{
				_orb.StartDragFromPosition(_orb.transform.position);
				_orb.SetTargetPosition(GetPositionTowards(_orb.transform.position, _closedSlot.transform.position));
			}
			else
			{
				_orb.StartDragFromPosition(_orb.transform.position);
				_orb.SetTargetPosition(_closedSlot.transform.position);
			}
		}
	}
}
