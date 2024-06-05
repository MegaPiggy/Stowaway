using HarmonyLib;
using NewHorizons.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stowaway.Patches
{
	[HarmonyPatch(typeof(GabbroDialogueSwapper))]
	public static class GabbroDialogueSwapperPatch
	{
		public static readonly string pathToNewDialogue = "planets/ExistingPlanets/dialogue/Gabbro new dialogue.xml";
		public static readonly string gabbroDialogueInfo = $"{{ pathToExistingDialogue: \"Sector_GabbroIsland/Interactables_GabbroIsland/Traveller_HEA_Gabbro/ConversationZone_Gabbro\" }}";

		// Special patching for Gabbro.
		[HarmonyPostfix]
		[HarmonyPatch(nameof(GabbroDialogueSwapper.Start))]
		public static void GabbroDialogueSwapper_Start_Postfix()
		{
			Stowaway.WriteSuccess("GabbroDialogueSwapper Start postfix has been run.");

			Stowaway.NewHorizonsAPI.CreateDialogueFromXML(textAssetID: null, xml: File.ReadAllText(Path.Combine(Stowaway.Instance.ModHelper.Manifest.ModFolderPath, pathToNewDialogue)), dialogueInfo: gabbroDialogueInfo, SearchUtilities.Find("GabbroIsland_Body"));
		}
	}
}
