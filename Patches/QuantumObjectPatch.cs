using HarmonyLib;
using Stowaway.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(QuantumObject))]
	public class QuantumObjectPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(QuantumObject.Awake))]
		public static void QuantumObject_Awake_Prefix(QuantumObject __instance)
		{
			__instance.gameObject.GetAddComponent<QuantumDropTarget>();
		}
	}
}
