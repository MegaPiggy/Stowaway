using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NomaiMultiPartDoor;

namespace Stowaway.Components
{
	[RequireComponent(typeof(NomaiMultiPartDoor), typeof(OverheadDetector))]
	public class NomaiDoorTugger : NomaiTugger
	{
		private NomaiMultiPartDoor _nomaiDoor;
		private NomaiInterfaceOrb _orb;
		private NomaiInterfaceSlot _idleSlot;
		private NomaiInterfaceSlot _activateSlot;

		public override void Start()
		{
			_nomaiDoor = this.GetRequiredComponent<NomaiMultiPartDoor>();
			_orb = _nomaiDoor._listInterfaceOrb.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"));
			_idleSlot = _orb._slots.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"), slot => slot.gameObject.name.StartsWith("Idle"));
			if (_idleSlot == null) _idleSlot = _nomaiDoor._cycleSwitches.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"), slot => slot.gameObject.name.StartsWith("Idle"));
			_activateSlot = _orb._slots.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"), slot => slot.gameObject.name.StartsWith("Activate"));
			if (_activateSlot == null) _activateSlot = _nomaiDoor._cycleSwitches.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"), slot => slot.gameObject.name.StartsWith("Activate"));
			base.Start();
		}

		public override void OnMoonOverhead(OWRigidbody bodyOverhead)
		{
			if (canOpenAndClose) Activate();
			else TugToActivate();
		}

		public override void OnMoonNoLongerOverhead(OWRigidbody bodyOverhead)
		{
			ReleaseTug();
		}

		private bool IsCyclable() => _nomaiDoor._cycleSwitches.Length > 0;

		public float GetDistanceBetweenSlots()
		{
			return Vector3.Distance(_activateSlot.transform.position, _idleSlot.transform.position);
		}

		public Vector3 GetPositionTowards(Vector3 a, Vector3 b)
		{
			var direction = (b - a).normalized;
			return (a + ((GetDistanceBetweenSlots() / 2) * direction));
		}

		private void Activate()
		{
			if (_orb._belowSand) return;
			_orb.StartDragFromPosition(_idleSlot.transform.position);
			_orb.SetTargetPosition(_activateSlot.transform.position);
		}

		private void TugToActivate()
		{
			if (_orb._belowSand) return;
			_orb.StartDragFromPosition(_orb.transform.position);
			_orb.SetTargetPosition(GetPositionTowards(_orb.transform.position, _activateSlot.transform.position));
		}

		private void ReleaseTug()
		{
			if (_orb._belowSand) return;
			_orb.CancelDrag();
		}
	}
}
