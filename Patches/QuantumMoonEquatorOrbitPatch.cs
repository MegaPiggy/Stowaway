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
			var matcher = new CodeMatcher(instructions, generator);
			matcher.MatchForward(false,
					new CodeMatch(OpCodes.Ldloc_2),
					new CodeMatch(OpCodes.Ldc_I4_5)
				);
			matcher.Advance(3).CreateLabel(out Label unitY).Advance(-3);
			matcher.Insert(new CodeInstruction(OpCodes.Ldloc_2), new CodeInstruction(OpCodes.Ldc_I4_3), new CodeInstruction(OpCodes.Beq_S, unitY));
			matcher.MatchForward(true,
					new CodeMatch(OpCodes.Conv_R4)
				);
			matcher.CreateLabel(out Label calculateOrbitVelocity);
			matcher.Insert(new CodeInstruction(OpCodes.Br_S, calculateOrbitVelocity), new CodeInstruction(OpCodes.Ldc_I4_0, 0));
			matcher.Advance(1).CreateLabel(out Label giantsDeepAngle);
			matcher.Start();
			matcher.MatchForward(false,
					new CodeMatch(OpCodes.Ldc_I4_0),
					new CodeMatch(OpCodes.Ldc_I4, 360)
				);
			matcher.Insert(new CodeInstruction(OpCodes.Ldloc_2), new CodeInstruction(OpCodes.Ldc_I4_3), new CodeInstruction(OpCodes.Beq_S, giantsDeepAngle));
			return matcher
				.InstructionEnumeration();
		}
	}
}