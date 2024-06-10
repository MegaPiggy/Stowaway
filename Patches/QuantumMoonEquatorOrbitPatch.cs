using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace Stowaway.Patches
{
	[HarmonyPatch]
	public class QuantumMoonEquatorOrbitPatch
	{
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(QuantumMoon), nameof(QuantumMoon.ChangeQuantumState))]
		public static IEnumerable<CodeInstruction> QuantumMoon_ChangeQuantumState_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			return new CodeMatcher(instructions, generator)
				.MatchForward(false,
					new CodeMatch(OpCodes.Ldloc_2),
					new CodeMatch(OpCodes.Ldc_I4_5)
				).Advance(3).CreateLabel(out Label unitY).Advance(-3)
				.Insert(
					new CodeInstruction(OpCodes.Ldloc_2),
					new CodeInstruction(OpCodes.Ldc_I4_3),
					new CodeInstruction(OpCodes.Beq_S, unitY),
					new CodeInstruction(OpCodes.Ldloc_2),
					new CodeInstruction(OpCodes.Ldc_I4_1),
					new CodeInstruction(OpCodes.Beq_S, unitY)
				)
				.MatchForward(true, new CodeMatch(OpCodes.Conv_R4)).CreateLabel(out Label calculateOrbitVelocity)
				.Insert(
					new CodeInstruction(OpCodes.Br_S, calculateOrbitVelocity),
					new CodeInstruction(OpCodes.Ldc_I4_0, 0)
				)
				.Advance(1).CreateLabel(out Label zeroAngle).Start().MatchForward(false,
					new CodeMatch(OpCodes.Ldc_I4_0),
					new CodeMatch(OpCodes.Ldc_I4, 360)
				)
				.Insert(
					new CodeInstruction(OpCodes.Ldloc_2),
					new CodeInstruction(OpCodes.Ldc_I4_3),
					new CodeInstruction(OpCodes.Beq_S, zeroAngle),
					new CodeInstruction(OpCodes.Ldloc_2),
					new CodeInstruction(OpCodes.Ldc_I4_5),
					new CodeInstruction(OpCodes.Beq_S, zeroAngle)
				)
				.InstructionEnumeration();
		}
	}
}