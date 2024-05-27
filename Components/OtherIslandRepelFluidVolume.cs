using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stowaway.Components
{
	public class OtherIslandRepelFluidVolume : SimpleFluidVolume
	{
		public override void Awake()
		{
			_flowSpeed = 50;
			_flowType = FlowType.Repulsive;
			_density = 100;
			_fluidType = Type.NONE;
			_layer = 5;
			_priority = 10;
			base.Awake();
		}

		public override void OnEffectVolumeEnter(GameObject hitObj)
		{
			if (!hitObj.CompareTag("IslandDetector")) return;
			FluidDetector component = hitObj.GetComponent<FluidDetector>();
			if (component != null)
			{
				component.AddVolume(this);
			}
		}

		public override void OnEffectVolumeExit(GameObject hitObj)
		{
			if (!hitObj.CompareTag("IslandDetector")) return;
			FluidDetector component = hitObj.GetComponent<FluidDetector>();
			if (component != null)
			{
				component.RemoveVolume(this);
			}
		}
	}
}
