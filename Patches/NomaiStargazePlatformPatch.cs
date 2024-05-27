using HarmonyLib;
using Stowaway.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(NomaiStargazePlatform))]
	public class NomaiStargazePlatformPatch
	{
		private static Dictionary<NomaiStargazePlatform, NomaiStargazePlatformOccupants> occupants = new Dictionary<NomaiStargazePlatform, NomaiStargazePlatformOccupants>();

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiStargazePlatform.Start))]
		public static void NomaiStargazePlatform_Start_Prefix(NomaiStargazePlatform __instance)
		{
			occupants.Add(__instance, new NomaiStargazePlatformOccupants());
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiStargazePlatform.OnSectorOccupantAdded))]
		public static bool NomaiStargazePlatform_OnSectorOccupantAdded_Prefix(NomaiStargazePlatform __instance, SectorDetector sectorDetector)
		{
			if (occupants.TryGetValue(__instance, out var platformOccupants))
			{
				platformOccupants.AddOccupant(sectorDetector.GetOccupantType());
				__instance.enabled = platformOccupants.IsEnabled();
			}
			return false;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(NomaiStargazePlatform.OnSectorOccupantRemoved))]
		public static bool NomaiStargazePlatform_OnSectorOccupantRemoved_Prefix(NomaiStargazePlatform __instance, SectorDetector sectorDetector)
		{
			if (occupants.TryGetValue(__instance, out var platformOccupants))
			{
				platformOccupants.RemoveOccupant(sectorDetector.GetOccupantType());
				__instance.enabled = platformOccupants.IsEnabled();
			}
			return false;
		}

		public class NomaiStargazePlatformOccupants
		{
			private bool _player;
			private bool _probe;

			public void AddOccupant(DynamicOccupant occupant)
			{
				if (occupant == DynamicOccupant.Player)
					_player = true;
				else if (occupant == DynamicOccupant.Probe)
					_probe = true;
			}

			public void RemoveOccupant(DynamicOccupant occupant)
			{
				if (occupant == DynamicOccupant.Player)
					_player = false;
				else if (occupant == DynamicOccupant.Probe)
					_probe = false;
			}

			public bool IsEnabled() => _player || _probe;
		}
	}
}
