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
	public class NomaiDoorTugger : MonoBehaviour
	{
		private NomaiMultiPartDoor _nomaiDoor;
		private NomaiInterfaceOrb _orb;
		private NomaiInterfaceSlot _idleSlot;
		private NomaiInterfaceSlot _activateSlot;
		private NomaiInterfaceSlot _openSlot;
		private NomaiInterfaceSlot _closedSlot;
		private OverheadDetector _overheadDetector;
		public bool canOpenAndClose = false;

		public void Start()
		{
			_nomaiDoor = this.GetRequiredComponent<NomaiMultiPartDoor>();
			if (IsCyclable())
			{
				_orb = _nomaiDoor._listInterfaceOrb.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"));
				_idleSlot = _orb._slots.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"), slot => slot.gameObject.name.StartsWith("Idle"));
				if (_idleSlot == null) _idleSlot = _nomaiDoor._cycleSwitches.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"), slot => slot.gameObject.name.StartsWith("Idle"));
				_activateSlot = _orb._slots.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"), slot => slot.gameObject.name.StartsWith("Activate"));
				if (_activateSlot == null) _activateSlot = _nomaiDoor._cycleSwitches.FirstSecondOrDefault(slot => slot.gameObject.name.EndsWith("Front"), slot => slot.gameObject.name.StartsWith("Activate"));
			}
			else
			{
				_orb = _nomaiDoor._listInterfaceOrb.FirstOrDefault();
				_openSlot = _orb._slots.FirstOrDefault(slot => slot.gameObject.name.EndsWith("Open"));
				if (_openSlot == null) _openSlot = _nomaiDoor._openSwitches.FirstOrDefault();
				_closedSlot = _orb._slots.FirstOrDefault(slot => slot.gameObject.name.EndsWith("Close"));
				if (_closedSlot == null) _closedSlot = _nomaiDoor._closeSwitches.FirstOrDefault();
			}
			_overheadDetector = this.GetRequiredComponent<OverheadDetector>();
			_overheadDetector.OnMoonOverhead += OnMoonOverhead;
			_overheadDetector.OnMoonNoLongerOverhead += OnMoonNoLongerOverhead;
		}

		public void OnMoonOverhead(OWRigidbody bodyOverhead)
		{
			if (canOpenAndClose) Open();
			else TugToOpen();
		}

		public void OnMoonNoLongerOverhead(OWRigidbody bodyOverhead)
		{
			if (canOpenAndClose) Close();
			else TugToClose();
		}

		private bool IsCyclable() => _nomaiDoor._cycleSwitches.Length > 0;

		public float GetDistanceBetweenSlots()
		{
			if (IsCyclable())
			{
				return Vector3.Distance(_activateSlot.transform.position, _idleSlot.transform.position);
			}
			else
			{
				return Vector3.Distance(_openSlot.transform.position, _closedSlot.transform.position);
			}
		}

		public Vector3 GetPositionTowards(Vector3 a, Vector3 b)
		{
			var direction = (b - a).normalized;
			return (a + ((GetDistanceBetweenSlots() / 2) * direction));
		}

		private void Open()
		{
			if (_orb._belowSand) return;
			if (_nomaiDoor._currentRotationState != RotationState.OPEN)
			{
				if (IsCyclable())
				{
					_orb.StartDragFromPosition(_idleSlot.transform.position);
					_orb.SetTargetPosition(_activateSlot.transform.position);
				}
				else
				{
					_orb.StartDragFromPosition(_closedSlot.transform.position);
					_orb.SetTargetPosition(_openSlot.transform.position);
				}
			}
		}

		private void Close()
		{
			if (_orb._belowSand) return;
			if (_nomaiDoor._currentRotationState != RotationState.CLOSED)
			{
				if (IsCyclable())
				{
					_orb.StartDragFromPosition(_idleSlot.transform.position);
					_orb.SetTargetPosition(_activateSlot.transform.position);
				}
				else
				{
					_orb.StartDragFromPosition(_openSlot.transform.position);
					_orb.SetTargetPosition(_closedSlot.transform.position);
				}
			}
		}

		private void TugToOpen()
		{
			if (_orb._belowSand) return;
			if (IsCyclable())
			{
				if (_nomaiDoor._currentRotationState != RotationState.OPEN)
				{
					_orb.StartDragFromPosition(_orb.transform.position);
					_orb.SetTargetPosition(GetPositionTowards(_orb.transform.position, _activateSlot.transform.position));
				}
			}
			else
			{
				if (_nomaiDoor._currentRotationState == RotationState.CLOSED)
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
		}

		private void TugToClose()
		{
			if (_orb._belowSand) return;
			if (IsCyclable())
			{
				if (_nomaiDoor._currentRotationState != RotationState.CLOSED)
				{
					_orb.StartDragFromPosition(_orb.transform.position);
					_orb.SetTargetPosition(GetPositionTowards(_orb.transform.position, _activateSlot.transform.position));
				}
			}
			else
			{
				if (_nomaiDoor._currentRotationState == RotationState.OPEN)
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
}
