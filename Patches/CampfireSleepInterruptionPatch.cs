using HarmonyLib;
using Stowaway.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(Campfire))]
	public class CampfireSleepInterruptionPatch
	{
		public static bool IsCampfiresIslandAirborne(Campfire campfire)
		{
			if (campfire != null && campfire.GetComponent<TornadoIslandCampfireDetector>() != null)
			{
				return campfire.GetComponent<TornadoIslandCampfireDetector>().IslandAirborne;
			}
			return false;
		}

		/// <summary>
		/// Adds a check for no tornados nearby
		/// </summary>
		[HarmonyTranspiler]
		[HarmonyPatch(nameof(Campfire.CanSleepHereNow))]
		public static IEnumerable<CodeInstruction> Campfire_CanSleepHereNow_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			return new CodeMatcher(instructions, generator).MatchForward(false,
					new CodeMatch(OpCodes.Ldc_I4_0),
					new CodeMatch(OpCodes.Ret))
				.CreateLabel(out Label retFalse).Start().MatchForward(false,
					new CodeMatch(OpCodes.Ble_Un)).Advance(1)
				.Insert(
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CampfireSleepInterruptionPatch), nameof(IsCampfiresIslandAirborne))),
					new CodeInstruction(OpCodes.Brtrue_S, retFalse)
				).InstructionEnumeration();
		}

		/// <summary>
		/// Adds a check for tornados nearby
		/// </summary>
		[HarmonyTranspiler]
		[HarmonyPatch(nameof(Campfire.ShouldWakeUp))]
		public static IEnumerable<CodeInstruction> Campfire_ShouldWakeUp_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			return new CodeMatcher(instructions, generator).MatchForward(false,
					new CodeMatch(OpCodes.Ldc_I4_1),
					new CodeMatch(OpCodes.Ret))
				.CreateLabel(out Label retTrue).Start().MatchForward(false,
					new CodeMatch(OpCodes.Brtrue_S),
					new CodeMatch(OpCodes.Call),
					new CodeMatch(OpCodes.Ldc_R4),
					new CodeMatch(OpCodes.Clt)
				).Advance(1).Insert(
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CampfireSleepInterruptionPatch), nameof(IsCampfiresIslandAirborne))),
					new CodeInstruction(OpCodes.Brtrue_S, retTrue)
				).InstructionEnumeration();
		}
	}
}
