global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using Terraria.ModLoader;
using DragonVault.Content.GUI.Vault;
using DragonVault.Content.Items.Dragonstones;
using DragonVault.Core.Networking;
using DragonVault.Core.Systems;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace DragonVault
{
	public class DragonVault : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			string type = reader.ReadString();

			if (type == "Withdrawl")
			{
				int amount = reader.ReadInt32();
				Item item = ItemIO.Receive(reader);
				Item toSync = item.Clone();

				Logger.Info($"Withdrawl of {amount} {item.Name} recieved.");

				ItemEntry entry = StorageSystem.vaultByID[item.type].Find(n => Helpers.Helper.CanStack(n.item, item));

				if (entry is null)
				{
					Logger.Warn("Failed to find item entry for a withdrawl packet! Has it already been withdrawn?");
					return;
				}

				entry.simStack -= amount;

				if (entry.CheckGone() && Main.netMode != NetmodeID.Server)
					VaultBrowser.Rebuild();

				if (Main.netMode == NetmodeID.Server)
					VaultNet.SendWithdrawl(amount, toSync, -1, whoAmI);
			}
			else if (type == "Deposit")
			{
				int amount = reader.ReadInt32();
				Item item = ItemIO.Receive(reader);
				Item toSync = item.Clone();

				Logger.Info($"Deposit of {amount} {item.Name} recieved.");

				item.stack = amount;
				bool result = StorageSystem.TryAddItem(item, out ItemEntry newEntry);

				if (!result)
				{
					Logger.Warn("Failed to add an item to the vault! Did something else fill it up?");
					return;
				}

				if (newEntry != null && Main.netMode != NetmodeID.Server)
					VaultBrowser.Rebuild();

				if (Main.netMode == NetmodeID.Server)
					VaultNet.SendDeposit(amount, toSync, -1, whoAmI);
			}
			else if (type == "JoinReq")
			{
				// Only the server should recieve these
				if (Main.netMode != NetmodeID.Server)
					return;

				int sequence = reader.ReadInt32();

				Logger.Info($"Request for item {sequence} ({StorageSystem.vault[sequence].item.Name}) recieved.");

				VaultNet.SendOnJoin(sequence, StorageSystem.vault.Count, whoAmI);
			}
			else if (type == "Join")
			{
				// Only clients should recieve these
				if (Main.netMode != NetmodeID.MultiplayerClient)
					return;

				int stack = reader.ReadInt32();
				int sequence = reader.ReadInt32();
				int maxSequence = reader.ReadInt32();
				Item item = ItemIO.Receive(reader);
				item.stack = stack;

				bool result = StorageSystem.TryAddItem(item, out ItemEntry newEntry);

				if (!result)
				{
					Logger.Warn("Failed to add an item to the vault on sync! Did something else fill it up?");
					return;
				}

				Logger.Info($"Data for item {sequence} ({item.Name}) recieved.");

				if (sequence < maxSequence)
					VaultNet.OnJoinReq(sequence + 1, maxSequence);
			}
			else if (type == "DataReq")
			{
				// Only the server should recieve these
				if (Main.netMode != NetmodeID.Server)
					return;

				Logger.Info($"Request for world data recieved.");

				VaultNet.Data(whoAmI);
			}
			else if (type == "Data")
			{
				int cap = reader.ReadInt32();
				int stone = reader.ReadInt32();

				StorageSystem.baseCapacity = cap;
				StorageSystem.stoneFlags = (Stones)stone;

				for (int k = 0; k < 8; k++)
				{
					var ston = (Stones)(1 << k);

					if ((StorageSystem.stoneFlags & ston) > 0)
					{
						(Dragonstone.samples[ston].ModItem as Dragonstone).Reset();
					}
				}

				for (int k = 0; k < 8; k++)
				{
					var ston = (Stones)(1 << k);

					if ((StorageSystem.stoneFlags & ston) > 0)
					{
						(Dragonstone.samples[ston].ModItem as Dragonstone).OnSlot();
					}
				}

				Logger.Info($"Data for world recieved.");

				if (Main.netMode == NetmodeID.Server)
					VaultNet.Data(-1, whoAmI);
			}
			else if (type == "CraftingDataReq")
			{
				// Only the server should recieve these
				if (Main.netMode != NetmodeID.Server)
					return;

				Logger.Info($"Request for crafting data recieved.");

				VaultNet.CraftingData(whoAmI);
			}
			else if (type == "CraftingData")
			{
				CraftingSystem.slots = reader.ReadInt32();
				int readTo = reader.ReadInt32();

				CraftingSystem.stations = new();

				for (int k = 0; k < readTo; k++)
				{
					CraftingSystem.stations.Add(ItemIO.Receive(reader));
				}

				Logger.Info($"Crafting data recieved.");

				if (Main.netMode == NetmodeID.Server)
					VaultNet.CraftingData(-1, whoAmI);
			}
		}
	}
}