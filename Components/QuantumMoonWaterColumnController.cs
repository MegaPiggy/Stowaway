using NewHorizons.Components.SizeControllers;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Utility;
using UnityEngine;

namespace Stowaway.Components
{
	public class QuantumMoonWaterColumnController : MonoBehaviour
	{
		public static float multiplier = 5;
		public static float height = 500;
#if DEBUG
		public static bool debug = true;
#else
		public static bool debug = false;
#endif

		private Transform target;
		private Transform anchor;
		private Transform align;
		private Transform lastPosition;
		private Transform lastTarget;
		private QuantumOrbit orbit;
		private bool isLocked;
		private float startTime;
		private float duration = 0.5f;
		private float endTime => startTime + duration;
		private float lastScale;
		private float scale;

		public void Start()
		{
			anchor = Locator.GetAstroObject(AstroObject.Name.GiantsDeep).transform;
			lastPosition = new GameObject("LastPosition").transform;
			lastPosition.transform.SetParent(anchor, false);
			lastTarget = new GameObject("LastTarget").transform;
			lastTarget.transform.SetParent(anchor, false);
			orbit = anchor.GetComponent<QuantumOrbit>();
			target = Locator.GetAstroObject(AstroObject.Name.QuantumMoon).transform;
			align = Locator.GetAstroObject(AstroObject.Name.Sun).transform;

			UpdateScale();

			GlobalMessenger<OWRigidbody>.AddListener("QuantumMoonChangeState", OnQuantumMoonStateChanged);

			if (scale == 0f)
			{
				Vanish();
			}
		}

		public void FixedUpdate()
		{
			var position = IsQuantumMoonLockedToOrbit() ? GetPositionBetweenAnchorAndTarget() : lastPosition.position;
			transform.position = position;

			if (IsQuantumMoonLockedToOrbit())
			{
				lastPosition.position = transform.position;
				lastTarget.transform.position = target.position;
			}

			UpdateScale();

			var currentTarget = IsQuantumMoonLockedToOrbit() ? target : lastTarget;

			float currentScale = scale;

			var dist = (position - currentTarget.position).magnitude;
			transform.localScale = new Vector3(currentScale, currentScale, dist / 500);

			transform.LookAt(currentTarget);

			if (!currentTarget.gameObject.activeInHierarchy || !anchor.gameObject.activeInHierarchy || !align.gameObject.activeInHierarchy)
			{
				gameObject.SetActive(false);
			}
		}

		public Vector3 GetPositionBetweenAnchorAndTarget()
		{
			var anchorPos = anchor.position;
			var targetPos = target.position;
			var direction = (targetPos - anchorPos).normalized;
			return (anchorPos + (height * direction));
		}

		protected void UpdateScale()
		{
			var prevScale = scale;
			if (IsQuantumMoonLockedToOrbit())
			{
				var realScale = debug ? multiplier : target.CosTo(anchor, align).SmoothStep(0.945f, 1) * multiplier;
				scale = Mathf.Lerp(lastScale, realScale, Mathf.InverseLerp(startTime, endTime, Time.time));
			}
			else
			{
				scale = Mathf.Lerp(lastScale, 0, Mathf.InverseLerp(startTime, endTime, Time.time));
			}

			if (prevScale != scale)
			{
				if (scale == 0f)
				{
					Vanish();
				}
				else if (prevScale == 0f)
				{
					Appear();
				}
			}
		}

		public void Vanish()
		{
			foreach (var child in gameObject.GetAllChildren())
			{
				child.SetActive(false);
			}
		}

		public void Appear()
		{
			foreach (var child in gameObject.GetAllChildren())
			{
				child.SetActive(true);
			}
		}

		public bool IsQuantumMoonLockedToOrbit()
		{
			return isLocked;
			/*var quantumMoon = Locator.GetQuantumMoon();
			return quantumMoon != null && orbit._stateIndex == quantumMoon.GetStateIndex();*/
		}

		public void OnQuantumMoonStateChanged(OWRigidbody qmBody)
		{
			if (qmBody == null || orbit == null) return;

			var qm = Locator.GetQuantumMoon();
			if (qm == null) return;

			if (orbit._stateIndex == qm.GetStateIndex())
			{
				isLocked = true;
				startTime = Time.time;
				lastScale = scale;
			}
			else
			{
				isLocked = false;
				startTime = Time.time;
				lastScale = scale;
			}
		}
	}
}
