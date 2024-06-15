using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	public class QuantumDropTarget : MonoBehaviour, IItemDropTarget
	{
		private QuantumObject _quantumObject;
		private QuantumState[] _states;
		private Dictionary<OWItem, int> _droppedItems;
		private QuantumState _state;
		private int _collapses;

		public void Awake()
		{
			_droppedItems = new Dictionary<OWItem, int>();
			_quantumObject = GetComponent<QuantumObject>();
			_states = GetComponentsInChildren<QuantumState>();
			_quantumObject.OnPostCollapse += OnPostCollapse;
		}

		private void OnDestroy()
		{
			_quantumObject.OnPostCollapse -= OnPostCollapse;
		}

		private void OnPostCollapse(QuantumObject quantumObject, bool collapsed)
		{
			_collapses++;
			_state = GetCurrentState();
		}

		private QuantumState GetCurrentState()
		{
			if (_states == null || _states.Length == 0) return null;
			return _states.FirstOrDefault(state => state.gameObject.activeSelf);
		}

		public void Update()
		{
			if (_droppedItems == null || _droppedItems.Count == 0) return;
			if (_state == null) return;
			foreach (var droppedItem in  _droppedItems.Keys)
			{
				droppedItem.gameObject.transform.SetParent(_state.transform);
			}
		}

		public void AddDroppedItem(GameObject dropTarget, OWItem item)
		{
			if (_droppedItems != null) _droppedItems.SafeAdd(item, _collapses);
			item.onPickedUp += new OWEvent<OWItem>.OWCallback(OnPickedUpDroppedItem);
		}

		private void OnPickedUpDroppedItem(OWItem item)
		{
			if (_droppedItems != null)
			{
				if (_droppedItems.TryGetValue(item, out int collapses) && collapses != _collapses)
				{
					DialogueConditionManager.SharedInstance.SetConditionState("QuantumSmuggled", conditionState: true);
				}
				_droppedItems.Remove(item);
			}
			item.onPickedUp -= new OWEvent<OWItem>.OWCallback(OnPickedUpDroppedItem);
		}

		public Transform GetItemDropTargetTransform(GameObject raycastTarget)
		{
			if (_states.Length != 0) return GetCurrentState().transform;
			return transform;
		}
	}
}
