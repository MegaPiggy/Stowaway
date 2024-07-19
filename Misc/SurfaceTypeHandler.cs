using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Stowaway.Misc
{
	internal static class SurfaceTypeHandler
	{
		public static void HandleMaterials(GameObject gameObject)
		{
			Stowaway.Instance.ModHelper.Events.Unity.FireOnNextUpdate(() =>
			{
				SurfaceManager surfaceManager = Locator.GetSurfaceManager();

				var handled = new List<string>();

				var rndrs = gameObject.GetComponentsInChildren<MeshRenderer>(true);
				foreach (var rndr in rndrs)
				{
					foreach (var mat in rndr.sharedMaterials)
					{
						if (mat == null) continue;
						if (handled.Contains(mat.name)) continue;

						handled.Add(mat.name);

						SurfaceType type = GetSurfaceType(mat);
						Stowaway.WriteWarning("Surface type of material \"" + mat.name + "\" is " + type);
						if (type != SurfaceType.None)
						{
							surfaceManager._lookupTable.SafeAdd(mat, type);
						}
					}
				}
			});
		}

		public static SurfaceType GetSurfaceType(Material mat)
		{
			switch (mat.name)
			{
				case "Snow":
					return SurfaceType.Snow;

				case "Ice":
				case "Terrain_DB_Crust_mat":
					return SurfaceType.Ice;

				case "Wood":
				case "Props_NOM_NomaiTreeBark_mat":
				case "Props_NOM_NomaiTreeBarkDEAD_mat":
					return SurfaceType.Wood;

				case "Planks":
					return SurfaceType.Planks;

				case "Bone":
				case "Character_NOM_Skeletonv2_d":
				case "Character_NOM_Skeletonv2_mat":
				case "Character_EYE_Skeletonv2_mat":
					return SurfaceType.Bone;

				case "Ceramic":
				case "Props_NOM_SmallTractorBeam_mat":
				case "Structure_NOM_Floor_mat":
				case "Structure_NOM_Grooves_Red_mat":
				case "Structure_NOM_HexagonTile_mat":
				case "Structure_NOM_PropTile_Color_mat":
				case "Structure_NOM_Spiral_Green_mat":
				case "Structure_NOM_Spiral_Red_mat":
				case "Structure_NOM_TrimPattern_mat":
				case "Structure_NOM_Whiteboard_mat":
				case "Structure_NOM_WhiteBoardTile_mat":
				case "Structure_NOM_Porcelain_mat":
					return SurfaceType.Ceramic;

				case "Fabric":
					return SurfaceType.Fabric;

				case "Grass":
					return SurfaceType.Grass;

				case "Foliage":
				case "Props_NOM_NomaiTreeLeaves_mat":
					return SurfaceType.Foliage;

				case "Dirt":
				case "Props_NOM_NomaiTreeDirt_mat":
					return SurfaceType.Dirt;

				case "Metal":
					return SurfaceType.Metal;

				case "MetalNomai":
				case "CopperOld":
				case "Terrain_HGTA_TimeLoopMetal_mat":
				case "Structure_NOM_CopperOld_mat":
				case "Structure_NOM_Silver_mat":
					return SurfaceType.MetalNomai;

				case "Crystal":
				case "GravityCrystal":
				case "Props_NOM_GravityCrystal_mat":
				case "Terrain_GM_Crystal_mat":
					return SurfaceType.Crystal;

				case "Energy":
					return SurfaceType.Energy;

				case "Glass":
				case "Props_NOM_Orb_mat":
				case "Props_NOM_Lamp_mat":
				case "Structure_NOM_Glass_Opaque_mat":
					return SurfaceType.Glass;

				case "Gravel":
					return SurfaceType.Gravel;

				case "GrittyRock":
					return SurfaceType.GrittyRock;

				case "QuantumRock":
					return SurfaceType.QuantumRock;

				case "Obsidian":
					return SurfaceType.Obsidian;

				case "Stone":
				case "Structure_NOM_Ceiling_mat":
				case "SandStone":
				case "SandStone.002":
				case "Structure_NOM_SandStone_mat":
				case "Structure_NOM_SandStone_GD_mat":
				case "Structure_NOM_WallInside_mat":
				case "Structure_NOM_WallOutside_mat":
				case "Structure_NOM_SandStone_mat Mir":
				case "Structure_NOM_WallInside_mat Mir":
				case "Structure_NOM_WallOutside_mat Mir":
				case "Tree_GD_CoreCoral_mat":
				case "Tree_GD_CoreCoral_mat 1":
					return SurfaceType.Stone;

				case "Sand":
					return SurfaceType.Sand;

				case "Vine":
					return SurfaceType.Vine;

				case "Water":
				case "Terrain_TH_Water_v2_mat":
				case "Effects_IP_PrisonWater_mat":
					return SurfaceType.Water;

				default:
					return SurfaceType.None;
			}
		}
	}
}
