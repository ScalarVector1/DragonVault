using DragonVault.Content.Items.Dragonstones;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.Default;
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
			if (type == TileID.TeleportationPylon && StorageSystem.stoneFlags.HasFlag(Stones.Radiant))
			{
				int count = CraftingSystem.stations?.Count ?? 0;

				List<int> tiles = new();

				for (int k = 0; k < count; k++)
				{
					if (CraftingSystem.stations[k].type == ModContent.ItemType<UnloadedItem>())
						continue;

					var type2 = CraftingSystem.stations[k]?.createTile ?? -1;
					tiles.Add(type2);

					switch (type2)
					{
						case 77:
						case 302:
							tiles.Add(17);
							break;
						case 133:
							tiles.Add(17);
							tiles.Add(77);
							break;
						case 134:
							tiles.Add(16);
							break;
						case 354:
						case 469:
						case 487:
							tiles.Add(14);
							break;
						case 355:
							tiles.Add(13);
							tiles.Add(14);
							Main.LocalPlayer.alchemyTable = true;
							break;
					}

					if (ModContent.GetModTile(type2) != null)
					{
						tiles.AddRange(ModContent.GetModTile(type2).AdjTiles);
					}
				}

				int[] newAdj = tiles.ToArray();

				return newAdj;
			}

			return new int[0];
		}
	}
}
