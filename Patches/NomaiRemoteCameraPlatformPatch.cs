using HarmonyLib;
using Stowaway.Components;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using UnityEngine;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(NomaiRemoteCameraPlatform))]
	public static class NomaiRemoteCameraPlatformPatch
	{
		private static Vector3 _storedPosition = Vector3.zero;
		private static Quaternion _storedRotation = Quaternion.identity;
		private static Vector3 _storedShapePos = Vector3.zero;
		private static Quaternion _storedShapeRot = Quaternion.identity;

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiRemoteCameraPlatform.SwitchToRemoteCamera))]
		public static bool NomaiRemoteCameraPlatform_SwitchToRemoteCamera_Prefix(NomaiRemoteCameraPlatform __instance)
		{
			if (Stowaway.IsSpecialStone(__instance._sharedStone))
			{
				var comp = Locator.GetPlayerController().gameObject.AddComponent<PlayerGolem>();
				comp._platform = __instance;

				Stowaway.Write($"Switch to remote camera");

				GlobalMessenger.FireEvent("EnterNomaiGolemConnection");
				return true;
			}
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiRemoteCameraPlatform.SwitchToPlayerCamera))]
		public static bool NomaiRemoteCameraPlatform_SwitchToPlayerCamera(NomaiRemoteCameraPlatform __instance)
		{
			if (Stowaway.Instance.IsGolemConnection)
			{
				Stowaway.Write($"Switch to player camera");

				var comp = Locator.GetPlayerController().gameObject.GetComponent<PlayerGolem>();
				if (comp != null)
					Object.Destroy(comp);

				GlobalMessenger.FireEvent("ExitNomaiGolemConnection");
				return true;
			}
			return true;
		}

		public static void RunGolemUpdate(this NomaiRemoteCameraPlatform __instance)
		{
			var isConnectedPlatformActive = __instance._slavePlatform != null && __instance._slavePlatform.gameObject.activeInHierarchy; //Mobius why did you call a connected projection platform, a slave platform.
			var insideSupernova = __instance.CheckSlavePlatformInsideSupernova();
			var insideBounds = __instance._connectionBounds.PointInside(__instance._playerCamera.transform.position);
			if (!isConnectedPlatformActive || insideSupernova || !insideBounds) Stowaway.Write($"On Platform Update: {(isConnectedPlatformActive ? "ConnectedPlatformActive" : "ConnectedPlatformDisabled")} {(insideSupernova ? "InsideSupernova" : "OutsideSupernova")} {(insideBounds ? "PointInside" : "PointOutside")}");
			if (OWInput.IsPressed(InputLibrary.cancel, 0f) || OWInput.IsPressed(InputLibrary.toolActionPrimary, 0f))
			{
				__instance.OnLeaveBounds();
			}
			else if (!isConnectedPlatformActive)
			{
				__instance.Disconnect();
				if (__instance._pedestalAnimator != null)
				{
					__instance._pedestalAnimator.PlayOpen();
				}
				if (__instance._transitionPedestalAnimator != null)
				{
					__instance._transitionPedestalAnimator.PlayOpen();
				}
				__instance._active = false;
			}
			else if (insideSupernova)// || !insideBounds)
			{
				__instance.OnLeaveBounds();
			}
		}

		[HarmonyTranspiler]
		[HarmonyPatch(nameof(NomaiRemoteCameraPlatform.Update))]
		public static IEnumerable<CodeInstruction> NomaiRemoteCameraPlatform_Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			return new CodeMatcher(instructions, generator).MatchForward(false,
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(OpCodes.Ldfld),
					new CodeMatch(OpCodes.Brfalse),
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(OpCodes.Ldarg_0))
				.CreateLabel(out Label next)
				.Insert(
					new CodeInstruction(OpCodes.Br_S, next),
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NomaiRemoteCameraPlatformPatch), nameof(RunGolemUpdate)))
				).Advance(1)
				.CreateLabel(out Label golemUpdate).Start().MatchForward(true,
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(OpCodes.Ldfld),
					new CodeMatch(OpCodes.Brfalse)).Advance(1)
				.Insert(
					new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(Stowaway), nameof(Stowaway.Instance))),
					new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Stowaway), nameof(Stowaway.IsGolemConnection))),
					new CodeInstruction(OpCodes.Brtrue, golemUpdate)
				).InstructionEnumeration();
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiRemoteCameraPlatform.UpdateHologramTransforms))]
		public static bool NomaiRemoteCameraPlatform_UpdateHologramTransforms_Prefix(NomaiRemoteCameraPlatform __instance)
		{
			if (!Stowaway.Instance.IsGolemConnection)
				return true;

			__instance._hologramGroup.transform.position = __instance._slavePlatform.transform.position;
			__instance._hologramGroup.transform.rotation = __instance._slavePlatform.transform.rotation;
			Transform playerTransform = Locator.GetPlayerTransform();

			__instance._playerHologram.position = playerTransform.position;
			__instance._playerHologram.rotation = playerTransform.rotation;
			if (__instance._sharedStone != null)
			{
				Transform transform = __instance._sharedStone.transform;
				__instance._stoneHologram.position = NomaiRemoteCameraPlatform.TransformPoint(transform.position, __instance, __instance._slavePlatform);
				__instance._stoneHologram.rotation = NomaiRemoteCameraPlatform.TransformRotation(transform.rotation, __instance, __instance._slavePlatform);
				return false;
			}
			__instance._stoneHologram.SetPositionAndRotation(new Vector3(0f, -2f, 0f), Quaternion.identity);
			return false;
		}
	}
}
