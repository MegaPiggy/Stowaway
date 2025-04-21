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
		public QuantumObject quantumObject;
		public QuantumState[] states;
		public Dictionary<OWItem, int> droppedItems;
		public QuantumState state;
		public int collapses;

		public void Awake()
		{
			collapses = 0;
			droppedItems = new Dictionary<OWItem, int>();
			quantumObject = GetComponent<QuantumObject>();
			states = GetStates();
			state = GetCurrentState();
			quantumObject.OnPostCollapse += OnPostCollapse;
		}

		public void OnDestroy()
		{
			quantumObject.OnPostCollapse -= OnPostCollapse;
		}

		public void OnPostCollapse(QuantumObject quantumObject, bool collapsed)
		{
			collapses++;
			state = GetCurrentState();

			if (state == null) return;
			if (droppedItems == null || droppedItems.Count == 0) return;

			foreach (var droppedItem in droppedItems.Keys)
			{
				if (droppedItem != null)
					droppedItem.transform.SetParent(state.transform);
			}
		}

		public QuantumState GetCurrentState()
		{
			if (states == null || states.Length == 0) return null;
			return states.FirstOrDefault(state => state.gameObject.activeSelf);
		}

		public QuantumState[] GetStates()
		{
			List<QuantumState> stateList = new List<QuantumState>();
			foreach (Transform child in transform)
			{
				if (child.TryGetComponent<QuantumState>(out QuantumState state))
					stateList.Add(state);
			}
			return stateList.ToArray();
		}

		public void AddDroppedItem(GameObject dropTarget, OWItem item)
		{
			if (droppedItems != null) droppedItems.SafeAdd(item, collapses);
			item.onPickedUp += new OWEvent<OWItem>.OWCallback(OnPickedUpDroppedItem);
		}

		private void OnPickedUpDroppedItem(OWItem item)
		{
			if (droppedItems != null)
			{
				if (droppedItems.TryGetValue(item, out int itemCollapse) && itemCollapse != collapses)
				{
					DialogueConditionManager.SharedInstance.SetConditionState("QuantumSmuggled", conditionState: true);
				}
				droppedItems.Remove(item);
			}
			item.onPickedUp -= new OWEvent<OWItem>.OWCallback(OnPickedUpDroppedItem);
		}

		public Transform GetItemDropTargetTransform(GameObject raycastTarget)
		{
			if (state != null) return state.transform;
			return transform;
		}
	}
}
