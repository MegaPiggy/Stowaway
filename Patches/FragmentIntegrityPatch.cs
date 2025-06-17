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
    [HarmonyPatch(typeof(FragmentIntegrity))]
    public static class FragmentIntegrityPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FragmentIntegrity.Awake))]
        public static void FragmentIntegrity_Awake_Prefix(FragmentIntegrity __instance, out float __state)
        {
            // Save original integrity before randomization
            __state = __instance._integrity;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(FragmentIntegrity.Awake))]
        public static void FragmentIntegrity_Awake_Postfix(FragmentIntegrity __instance, float __state)
        {
            float newIntegrity = Mathf.Max(0f, __state);
            __instance._integrity = newIntegrity;
            __instance._origIntegrity = newIntegrity;
        }
    }
}
