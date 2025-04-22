using HarmonyLib;
using NewHorizons.Utility;
using System.IO;
using UnityEngine;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(SurfaceManager))]
	public static class SurfaceManagerPatch
	{
		/// <summary>
		/// Patch to find missing materials on colliders
		/// </summary>
		[HarmonyPrefix]
		[HarmonyPatch(nameof(SurfaceManager.GetHitMaterial))]
		public static bool SurfaceManager_GetHitMaterial_Prefix(SurfaceManager __instance, RaycastHit hitInfo, ref Material __result)
		{
			var path = hitInfo.collider.transform.GetPath();
			Renderer component = hitInfo.collider.GetComponent<Renderer>();
			BatchedMaterialLookup component2 = hitInfo.collider.GetComponent<BatchedMaterialLookup>();
			if (component == null && component2 == null)
			{
				//Stowaway.WriteError("Collider at \"" + path + "\" is missing materials");
				__result = null;
				return false;
			}
			int hitSubmesh = __instance.GetHitSubmesh(hitInfo);
			if (component2 != null)
			{
				if (component2.materials.Length > 0)
				{
					__result = component2.materials[hitSubmesh];
				}
				else
				{
					Stowaway.WriteError("BatchedMaterialLookup at \"" + path + "\" is missing material for submesh #" + hitSubmesh);
					__result = null;
				}
				return false;
			}
			else
			{
				if (component.sharedMaterials.Length > 0)
				{
					__result = component.sharedMaterials[hitSubmesh];
				}
				else
				{
					Stowaway.WriteError("Renderer at \"" + path + "\" is missing shared material for submesh #" + hitSubmesh);
					__result = null;
				}
				return false;
			}
		}
	}
}
