using HarmonyLib;
using NewHorizons.Utility;
using Stowaway.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(ReferenceFrameTracker))]
	public static class ReferenceFrameTrackerPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch(nameof(ReferenceFrameTracker.FindReferenceFrameInLineOfSight))]
		public static bool ReferenceFrameTracker_FindReferenceFrameInLineOfSight_Prefix(ReferenceFrameTracker __instance, ref ReferenceFrame __result)
		{
			__result = null;
			OWCamera owCamera = __instance._isLandingView ? __instance._landingCam : __instance._activeCam;
			float maxDistance = PlayerState.InBrambleDimension() ? 120 : 1000;
			Vector3 position = owCamera.transform.position;
			if (!__instance._isMapView && Physics.Raycast(position, owCamera.transform.forward, out var hitInfo, maxDistance, PlayerState.AtFlightConsole() ? OWLayerMask.closeRangeRFMinusShipInterior : OWLayerMask.closeRangeRFMask))
			{
				var distance = Vector3.Distance(position, hitInfo.collider.transform.position);

				if (hitInfo.collider.TryGetComponent(out ReferenceFrameVolume referenceFrameVolume))
				{
					ReferenceFrame referenceFrame = referenceFrameVolume.GetReferenceFrame();
					float maxTargetDistance = referenceFrame.GetMaxTargetDistance();
					float minSuitTargetDistance = referenceFrame.GetMinSuitTargetDistance();
					bool isBelowMax = maxTargetDistance <= 0 || distance < maxTargetDistance;
					if ((PlayerState.AtFlightConsole() || distance >= minSuitTargetDistance) && isBelowMax)
					{
						__result = referenceFrame;
					}
				}
				else if (hitInfo.rigidbody != null && hitInfo.rigidbody.TryGetComponent(out OWRigidbody owRigidbody))
				{
					ReferenceFrame referenceFrame = owRigidbody.GetReferenceFrame();
					float maxTargetDistance = referenceFrame.GetMaxTargetDistance();
					float minSuitTargetDistance = referenceFrame.GetMinSuitTargetDistance();
					bool isBelowMax = maxTargetDistance <= 0 || distance < maxTargetDistance;
					if (owRigidbody.IsTargetable() && (PlayerState.AtFlightConsole() || distance >= minSuitTargetDistance) && isBelowMax)
					{
						__result = referenceFrame;
					}
				}
			}
			else if (!PlayerState.InBrambleDimension() && !PlayerState.InGiantsDeep())
			{
				float maxDistanceLR = 100000;
				RaycastHit[] hitInfos = Physics.RaycastAll(position, owCamera.transform.forward, maxDistanceLR, OWLayerMask.longRangeRFMask);
				float smallestAngle = 180;
				foreach (var lrHitInfo in hitInfos)
				{
					ReferenceFrameVolume referenceFrameVolume = lrHitInfo.collider.GetComponent<ReferenceFrameVolume>();
					ReferenceFrame referenceFrame = referenceFrameVolume.GetReferenceFrame();
					float maxTargetDistance = referenceFrame.GetMaxTargetDistance();
					Vector3 to = lrHitInfo.collider.transform.position - position;
					float angle = Vector3.Angle(owCamera.transform.forward, to);
					bool isBelowMax = maxTargetDistance <= 0 || to.magnitude < maxTargetDistance;
					if (angle < smallestAngle && isBelowMax)
					{
						smallestAngle = angle;
						__result = referenceFrame;
					}
				}
			}
			return false;
		}
	}
}
