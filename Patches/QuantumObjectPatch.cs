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
		[HarmonyPostfix]
		[HarmonyPatch(nameof(QuantumObject.Start))]
		public static void QuantumObject_Start_Postfix(QuantumObject __instance)
		{
			__instance.gameObject.GetAddComponent<QuantumDropTarget>();
		}
	}
}
