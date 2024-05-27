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

		public void Awake()
		{
			_nomaiDoor = GetComponent<NomaiMultiPartDoor>();
			_overheadDetector = GetComponent<OverheadDetector>();
			_overheadDetector.OnMoonOverhead += OnMoonOverhead;
			_overheadDetector.OnMoonNoLongerOverhead += OnMoonNoLongerOverhead;
		}

		public void OnMoonOverhead(OWRigidbody bodyOverhead)
		{
			Open();
		}

		public void OnMoonNoLongerOverhead(OWRigidbody bodyOverhead)
		{
			Close();
		}

		private bool IsCyclable() => _nomaiDoor._cycleSwitches.Length > 0;

		private void Open()
		{
			if (_nomaiDoor._currentRotationState != RotationState.OPEN)
			{
				if (IsCyclable())
					_nomaiDoor._listInterfaceOrb[0].SetOrbPosition(_nomaiDoor._cycleSwitches[0].transform.position);
				else
					_nomaiDoor._listInterfaceOrb[0].SetOrbPosition(_nomaiDoor._openSwitches[0].transform.position);
			}
		}

		private void Close()
		{
			if (_nomaiDoor._currentRotationState != RotationState.CLOSED)
			{
				if (IsCyclable())
					_nomaiDoor._listInterfaceOrb[1].SetOrbPosition(_nomaiDoor._cycleSwitches[1].transform.position);
				else
					_nomaiDoor._listInterfaceOrb[0].SetOrbPosition(_nomaiDoor._closeSwitches[0].transform.position);
			}
		}
	}
}
