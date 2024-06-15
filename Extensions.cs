using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stowaway
{
	public static class Extensions
	{
		/// <summary>
		/// Returns if a value in the <paramref name="source"/> meets both the <paramref name="condition"/> and <paramref name="secondCondition"/>.
		/// 
		/// If none of them do then it returns the first value that meets the <paramref name="secondCondition"/>.
		/// </summary>
		public static TSource FirstSecondOrDefault<TSource>(this IEnumerable<TSource> source, Predicate<TSource> condition, Predicate<TSource> secondCondition, TSource customDefault = default(TSource))
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			if (secondCondition == null)
				throw new ArgumentNullException(nameof(secondCondition));
			foreach (TSource source1 in source)
			{
				if (condition(source1) && secondCondition(source1))
					return source1;
			}
			foreach (TSource source1 in source)
			{
				if (secondCondition(source1))
					return source1;
			}
			return customDefault;
		}

		/// <summary>
		/// Returns if a value in the <paramref name="source"/> meets the <paramref name="condition"/>.
		/// 
		/// If none of them do then it returns the first value.
		/// </summary>
		public static TSource FirstSecondOrDefault<TSource>(this IEnumerable<TSource> source, Predicate<TSource> condition)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));
			if (condition == null)
				throw new ArgumentNullException(nameof(condition));
			foreach (TSource source1 in source)
			{
				if (condition(source1))
					return source1;
			}
			return source.FirstOrDefault();
		}

		public static CodeMatcher LogInstructions(this CodeMatcher matcher, string prefix)
		{
#if DEBUG
			matcher.InstructionEnumeration().LogInstructions(prefix);
#endif
			return matcher;
		}

		public static IEnumerable<CodeInstruction> LogInstructions(this IEnumerable<CodeInstruction> instructions, string prefix)
		{
#if DEBUG
			var message = prefix;
			foreach (var instruction in instructions)
			{
				message += $"\n{instruction}";
			}
			Debug.LogError(message);
#endif
			return instructions;
		}

		public static float CosTo(this Transform transform, OWRigidbody anchor, OWRigidbody target)
		{
			if (target == null) return 0;
			return CosTo(transform.position, anchor.transform.position, target.transform.position);
		}

		public static float CosTo(this Transform transform, Transform anchor, Transform target)
		{
			if (target == null) return 0;
			return CosTo(transform.position, anchor.position, target.position);
		}

		public static float CosTo(this Vector3 position, Vector3 anchor, Vector3 target)
		{
			var v1 = (position - anchor).normalized;
			var v2 = (target - anchor).normalized;

			var cos = Vector3.Dot(v1, v2);
			return Mathf.Max(0f, cos);
		}

		public static float SmoothStep(this float x, float edge0, float edge1)
		{
			// Scale, and clamp x to 0..1 range
			x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));

			return x * x * (3.0f - 2.0f * x);
		}

		public static float LowerProgress(this float current, float duration) => Mathf.MoveTowards(current, 0, Time.deltaTime / duration);

		public static float RaiseProgress(this float current, float duration) => Mathf.MoveTowards(current, 1, Time.deltaTime / duration);

		public static Sector GetSectorOrControllingProxySector(this SectorCullGroup sectorCullGroup)
		{
			Sector sector = sectorCullGroup.GetSector();
			if (sector == null)
			{
				SectorProxy controllingProxy = sectorCullGroup.GetControllingProxy();
				if (controllingProxy != null)
				{
					sector = controllingProxy.GetSector();
				}
			}
			return sector;
		}

		public static Sector GetSectorFromRaycastHit(this RaycastHit hit)
		{
			Sector sector = null;
			SectorCullGroup sectorCullGroup = hit.collider.gameObject.GetComponentInParent<SectorCullGroup>();
			if (sectorCullGroup != null) sector = sectorCullGroup.GetSectorOrControllingProxySector();
			else
			{
				var sectorGroup = hit.collider.gameObject.GetComponentInParent<ISectorGroup>();
				if (sectorGroup != null) sector = sectorGroup.GetSector();
			}
			return sector;
		}

		public static void DropItemToGround(this ItemTool tool, Transform socket, Sector sector)
		{
			var heldItem = tool.GetHeldItem();
			if (heldItem != null)
			{
				var playerTransform = Locator.GetPlayerTransform();
				if (Physics.Raycast(playerTransform.position, -playerTransform.up, out var hit, 200, OWLayerMask.groundMask))
				{
					var newSector = hit.GetSectorFromRaycastHit();
					sector = newSector != null ? newSector : sector;
					GameObject gameObject = hit.collider.gameObject;
					IItemDropTarget customDropTarget = gameObject.GetComponentInParent<IItemDropTarget>();
					Transform parent = ((customDropTarget == null) ? socket : customDropTarget.GetItemDropTargetTransform(gameObject));
					heldItem.DropItem(hit.point, hit.normal, parent, sector, customDropTarget);
					customDropTarget?.AddDroppedItem(gameObject, heldItem);
				}
				else
				{
					heldItem.DropItem(playerTransform.position, playerTransform.up, socket, sector, null);
				}
				tool._heldItem = null;
			}
		}
	}
}
