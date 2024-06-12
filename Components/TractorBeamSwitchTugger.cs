using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NomaiMultiPartDoor;

namespace Stowaway.Components
{
	[RequireComponent(typeof(TractorBeamSwitch), typeof(OverheadDetector))]
	public class TractorBeamSwitchTugger : NomaiTugger
	{
		private TractorBeamSwitch.State _state;
		private TractorBeamSwitch _tractorBeamSwitch;
		private NomaiInterfaceOrb _orb;
		private NomaiInterfaceSlot _offSlot;
		private NomaiInterfaceSlot _reverseSlot;
		private NomaiInterfaceSlot _forwardSlot;

		public override void Start()
		{
			_tractorBeamSwitch = this.GetRequiredComponent<TractorBeamSwitch>();
			_state = _tractorBeamSwitch._initialState;
			_orb = _tractorBeamSwitch._orb;
			_offSlot = _tractorBeamSwitch._offSlot;
			_reverseSlot = _tractorBeamSwitch._reverseSlot;
			_forwardSlot = _tractorBeamSwitch._forwardSlot;
			_forwardSlot.OnSlotActivated += OnForwardSlotActivated;
			_reverseSlot.OnSlotActivated += OnReverseSlotActivated;
			_offSlot.OnSlotActivated += OnOffSlotActivated;
			base.Start();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			_forwardSlot.OnSlotActivated -= OnForwardSlotActivated;
			_reverseSlot.OnSlotActivated -= OnReverseSlotActivated;
			_offSlot.OnSlotActivated -= OnOffSlotActivated;
		}

		private void OnForwardSlotActivated(NomaiInterfaceSlot slot)
		{
			_state = TractorBeamSwitch.State.FORWARD;
		}
		private void OnReverseSlotActivated(NomaiInterfaceSlot slot)
		{
			_state = TractorBeamSwitch.State.REVERSE;
		}

		private void OnOffSlotActivated(NomaiInterfaceSlot slot)
		{
			_state = TractorBeamSwitch.State.OFF;
		}

		public override void OnMoonOverhead(OWRigidbody bodyOverhead)
		{
			Forward();
		}

		public override void OnMoonNoLongerOverhead(OWRigidbody bodyOverhead)
		{
			Reverse();
		}

		public NomaiInterfaceSlot GetCurrentSlot()
		{
			switch (_state)
			{
				case TractorBeamSwitch.State.FORWARD:
					return _forwardSlot;
				case TractorBeamSwitch.State.REVERSE:
					return _reverseSlot;
				case TractorBeamSwitch.State.OFF:
				default:
					return _offSlot;
			}
		}

		public void Forward()
		{
			if (_orb._belowSand) return;
			if (_state == TractorBeamSwitch.State.FORWARD) return;
			_orb.StartDragFromPosition(GetCurrentSlot().transform.position);
			_orb.SetTargetPosition(_forwardSlot.transform.position);
		}

		public void Reverse()
		{
			if (_orb._belowSand) return;
			if (_state == TractorBeamSwitch.State.REVERSE) return;
			_orb.StartDragFromPosition(GetCurrentSlot().transform.position);
			_orb.SetTargetPosition(_reverseSlot.transform.position);
		}

		public void Off()
		{
			if (_orb._belowSand) return;
			if (_state == TractorBeamSwitch.State.OFF) return;
			_orb.StartDragFromPosition(GetCurrentSlot().transform.position);
			_orb.SetTargetPosition(_offSlot.transform.position);
		}
	}
}
