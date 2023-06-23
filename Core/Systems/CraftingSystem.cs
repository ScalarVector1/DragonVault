using DragonVault.Content.Items.Dragonstones;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace DragonVault.Core.Systems
{
	internal class CraftingSystem : ModSystem
	{
		public static int slots = 5;
		public static List<Item> stations = new();

		public override void ClearWorld()
		{
			slots = 5;
			stations = new();
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag["slots"] = slots;
			tag["stations"] = stations;
		}

		public override void LoadWorldData(TagCompound tag)
		{
			slots = tag.GetInt("slots");
			stations = (List<Item>)tag.GetList<Item>("stations");
		}
	}

	internal class CraftingTile : GlobalTile
	{
		public override int[] AdjTiles(int type)
		{
			if ((StorageSystem.stoneFlags & Stones.Cerulean) > 0)
			{
				int[] newAdj = new int[CraftingSystem.stations.Count];

				for (int k = 0; k < CraftingSystem.stations.Count; k++)
				{
					newAdj[k] = CraftingSystem.stations[k].createTile;
				}

				return newAdj;
			}

			return default;
		}
	}
}
