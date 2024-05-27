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
		private OverheadDetector _overheadDetector;
		public bool canOpenAndClose = false;

		public void Awake()
		{
			_nomaiDoor = GetComponent<NomaiMultiPartDoor>();
			_overheadDetector = GetComponent<OverheadDetector>();
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

		public float GetDistanceBetweenOpenAndClosed()
		{
			if (IsCyclable())
			{
				return Vector3.Distance(_nomaiDoor._listInterfaceOrb[0].transform.position, _nomaiDoor._cycleSwitches[0].transform.position);
			}
			else
			{
				return Vector3.Distance(_nomaiDoor._openSwitches[0].transform.position, _nomaiDoor._closeSwitches[0].transform.position);
			}
		}

		public Vector3 GetPositionTowards(Vector3 a, Vector3 b)
		{
			var direction = (b - a).normalized;
			return (a + ((GetDistanceBetweenOpenAndClosed() / 2) * direction));
		}

		private void Open()
		{
			if (_nomaiDoor._currentRotationState != RotationState.OPEN)
			{
				if (IsCyclable())
					_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(_nomaiDoor._cycleSwitches[0].transform.position);
				else
					_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(_nomaiDoor._openSwitches[0].transform.position);
			}
		}

		private void Close()
		{
			if (_nomaiDoor._currentRotationState != RotationState.CLOSED)
			{
				if (IsCyclable())
					_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(_nomaiDoor._cycleSwitches[0].transform.position);
				else
					_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(_nomaiDoor._closeSwitches[0].transform.position);
			}
		}

		private void TugToOpen()
		{
			if (IsCyclable())
			{
				_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(_nomaiDoor._cycleSwitches[0].transform.position);
			}
			else if (_nomaiDoor._currentRotationState == RotationState.CLOSED)
				_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(GetPositionTowards(_nomaiDoor._listInterfaceOrb[0].transform.position, _nomaiDoor._openSwitches[0].transform.position));
			else
				_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(_nomaiDoor._openSwitches[0].transform.position);
		}

		private void TugToClose()
		{
			if (IsCyclable())
			{
				if (_nomaiDoor._currentRotationState != RotationState.CLOSED)
					_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(_nomaiDoor._cycleSwitches[0].transform.position);
			}
			else if (_nomaiDoor._currentRotationState == RotationState.OPEN)
				_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(GetPositionTowards(_nomaiDoor._listInterfaceOrb[0].transform.position, _nomaiDoor._closeSwitches[0].transform.position));
			else
				_nomaiDoor._listInterfaceOrb[0].MoveTowardPosition(_nomaiDoor._closeSwitches[0].transform.position);
		}
	}
}
