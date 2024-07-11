using NewHorizons.Components.SizeControllers;
using NewHorizons.External.Modules.VariableSize;
using NewHorizons.Utility;
using UnityEngine;

namespace Stowaway.Components
{
	public class QuantumMoonWaterColumnController : MonoBehaviour
	{
		private static readonly float multiplier = 5;

		private Transform target;
		private Transform anchor;
		private Transform align;
		private QuantumOrbit orbit;
		public float height = 380;
		private float scale;

		public void Start()
		{
			anchor = Locator.GetAstroObject(AstroObject.Name.GiantsDeep).transform;
			orbit = anchor.GetComponent<QuantumOrbit>();
			target = Locator.GetAstroObject(AstroObject.Name.QuantumMoon).transform;
			align = Locator.GetAstroObject(AstroObject.Name.Sun).transform;

			UpdateScale();

			if (scale == 0f)
			{
				Vanish();
			}
		}

		public void FixedUpdate()
		{
			var position = GetPositionBetweenAnchorAndTarget();
			transform.position = position;

			UpdateScale();

			float currentScale = scale;

			var dist = (position - target.position).magnitude;
			transform.localScale = new Vector3(currentScale, currentScale, dist / 500);

			transform.LookAt(target);

			if (!target.gameObject.activeInHierarchy || !anchor.gameObject.activeInHierarchy || !align.gameObject.activeInHierarchy)
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
				scale = target.CosTo(anchor, align).SmoothStep(0.945f, 1) * multiplier;
			else
				scale = 0;

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
			var quantumMoon = Locator.GetQuantumMoon();
			return quantumMoon != null && orbit._stateIndex == quantumMoon.GetStateIndex();
		}
	}
}
