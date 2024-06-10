using Delaunay;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Stowaway.Components
{
	[RequireComponent(typeof(SphereShape))]
	public class InvertedVisibilityOccluder : MonoBehaviour
	{
		private static List<InvertedVisibilityOccluder> list = new List<InvertedVisibilityOccluder>(64);

		private SphereShape _sphereShape;

		public void Awake()
		{
			_sphereShape = GetComponent<SphereShape>();
		}

		public void OnEnable()
		{
			list.Add(this);
		}

		public void OnDisable()
		{
			list.Remove(this);
		}

		public bool IsShapeOccluded(Shape shape, Vector3 cameraPos)
		{
			var isCameraInside = _sphereShape.PointInside(cameraPos);
			if (shape != null)
			{
				var isShapeInside = _sphereShape.PointInside(shape.ClosestPoint(cameraPos));
				if ((!isCameraInside && isShapeInside) || (isCameraInside && !isShapeInside))
					return true;
			}
			return false;
		}

		public bool IsTrackerBlocked(ShapeVisibilityTracker tracker, Vector3 cameraPos)
		{
			Vector3 centerLine = ShapeUtil.Sphere.CalcWorldSpaceCenter(_sphereShape) - cameraPos;
			float num = ShapeUtil.Sphere.CalcWorldSpaceRadius(_sphereShape);
			float magnitude = centerLine.magnitude;
			if (magnitude > 0.01f)
			{
				centerLine /= magnitude;
				float halfAngle = Mathf.Asin(num / magnitude);
				return tracker.IsBlocked(cameraPos, centerLine, magnitude, halfAngle);
			}
			return false;
		}

		public bool IsTrackerOccluded(ShapeVisibilityTracker tracker, Vector3 cameraPos)
		{
			var isCameraInside = _sphereShape.PointInside(cameraPos);
			foreach (var shape in tracker._shapes)
			{
				if (shape != null)
				{
					var isShapeInside = _sphereShape.PointInside(shape.ClosestPoint(cameraPos));
					if ((!isCameraInside && isShapeInside) || (isCameraInside && !isShapeInside))
						return true;
				}
			}
			return false;
		}

		public static bool DoesAnyOccludeTracker(ShapeVisibilityTracker tracker, Vector3 cameraPos)
		{
			foreach (var invertedOccluder in list)
			{
				if (invertedOccluder.IsTrackerOccluded(tracker, cameraPos) || invertedOccluder.IsTrackerBlocked(tracker, cameraPos))
				{
					return true;
				}
			}
			return false;
		}
	}
}
