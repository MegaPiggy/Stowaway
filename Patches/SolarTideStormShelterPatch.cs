using HarmonyLib;
using Stowaway.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(IslandController))]
	public static class SolarTideStormShelterPatch
	{
		public static bool IsSunOverhead(this IslandController island)
		{
			if (island != null && island.TryGetComponent<OverheadDetector>(out OverheadDetector overheadDetector))
			{
				return overheadDetector.IsSunOverhead();
			}
			return false;
		}

		/// <summary>
		/// Adds a check for solar tide
		/// </summary>
		[HarmonyTranspiler]
		[HarmonyPatch(nameof(IslandController.FixedUpdate))]
		public static IEnumerable<CodeInstruction> IslandController_FixedUpdate_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			return new CodeMatcher(instructions, generator).MatchForward(false,
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(OpCodes.Ldfld),
					new CodeMatch(OpCodes.Ldc_I4_4),
					new CodeMatch(OpCodes.Callvirt),
					new CodeMatch(OpCodes.Brfalse),
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(OpCodes.Ldfld),
					new CodeMatch(OpCodes.Callvirt))
				.CreateLabel(out Label inheritance).Start().MatchForward(true,
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(OpCodes.Ldfld),
					new CodeMatch(OpCodes.Brfalse),
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(OpCodes.Ldfld),
					new CodeMatch(OpCodes.Ldc_I4_4),
					new CodeMatch(OpCodes.Callvirt),
					new CodeMatch(OpCodes.Brtrue))
				.Insert(
					new CodeInstruction(OpCodes.Brtrue, inheritance),
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SolarTideStormShelterPatch), nameof(IsSunOverhead)))
				).Start().MatchForward(true,
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(OpCodes.Ldfld),
					new CodeMatch(OpCodes.Brtrue),
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(OpCodes.Ldfld),
					new CodeMatch(OpCodes.Ldc_I4_4),
					new CodeMatch(OpCodes.Callvirt),
					new CodeMatch(OpCodes.Brfalse))
				.Advance(1).CreateLabel(out Label activate).Advance(-1)
				.Insert(
					new CodeInstruction(OpCodes.Brtrue, activate),
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SolarTideStormShelterPatch), nameof(IsSunOverhead)))
				).InstructionEnumeration();
		}
	}
}
