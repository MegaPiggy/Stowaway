using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	public class DebugVolume : MonoBehaviour
	{
		private OWTriggerVolume _triggerVolume;

		private void Awake()
		{
			_triggerVolume = GetComponent<OWTriggerVolume>();
			_triggerVolume.OnEntry += OnEntry;
			_triggerVolume.OnExit += OnExit;
		}

		protected virtual void OnDestroy()
		{
			_triggerVolume.OnEntry -= OnEntry;
			_triggerVolume.OnExit -= OnExit;
		}

		private void OnEntry(GameObject hitObj)
		{
			Stowaway.Write(transform.root.gameObject.name.Replace("_Body","") + " OnEntry \"" + hitObj.transform.root.gameObject.name.Replace("_Body", "") + "\"");
		}

		private void OnExit(GameObject hitObj)
		{
			Stowaway.Write(transform.root.gameObject.name.Replace("_Body", "") + " OnExit \"" + hitObj.transform.root.gameObject.name.Replace("_Body", "") + "\"");
		}
	}
}
