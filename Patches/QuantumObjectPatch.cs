using HarmonyLib;
using Stowaway.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(VisibilityObject))]
	public static class QuantumObjectPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(VisibilityObject.Awake))]
		public static void VisibilityObject_Awake_Postfix(VisibilityObject __instance)
		{
			if (__instance is QuantumObject)
			{
				__instance.gameObject.GetAddComponent<QuantumDropTarget>(); // Add only if there isn't one already
			}
		}
	}
}
