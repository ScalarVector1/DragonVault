﻿using DragonVault.Content.GUI.Vault;
using DragonVault.Content.Items.Dragonstones;
using DragonVault.Content.Tiles;
using DragonVault.Core.Loaders.UILoading;
using DragonVault.Core.Networking;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace DragonVault.Core.Systems
{
	internal class StorageSystem : ModSystem
	{
		public static List<ItemEntry> vault = new();
		public static Dictionary<int, List<ItemEntry>> vaultByID = new();
		public static List<Item> craftingCache = new();
		public static List<string> Favorites = new();
		public static List<string> CraftFavorites = new();

		public static int baseCapacity = 20000;
		public static int extraCapacity = 0;

		public static Stones stoneFlags = 0;

		public static int MaxCapacity
		{
			get {
				int cap = extraCapacity + baseCapacity;

				// Protect against overflow
				if (cap < 0)
					return int.MaxValue;
				else
					return cap;
			}
		}

		public static int RemainingCapacity => MaxCapacity - GetVaultLoad();

		/// <summary>
		/// Returns the current amount of capacity used by the vault
		/// </summary>
		/// <returns>The amount of items in the vault</returns>
		public static int GetVaultLoad()
		{
			int load = 0;

			foreach (ItemEntry entry in vault)
			{
				load += entry.simStack;
			}

			return load;
		}

		/// <summary>
		/// Attempts to add an item to the vault
		/// </summary>
		/// <param name="newItem">The item to add</param>
		/// <returns>If the item was able to be atleast partially added or not</returns>
		public static bool TryAddItem(Item newItem, out ItemEntry newEntry)
		{
			newEntry = null;

			if (newItem is null || newItem.IsAir || newItem.stack <= 0)
				return false;

			if (RemainingCapacity <= 0)
				return false;

			if (vaultByID.ContainsKey(newItem.type))
			{
				UILoader.GetUIState<VaultBrowser>().Recalculate();
				UILoader.GetUIState<VaultBrowser>().Recalculate();

				List<ItemEntry> possibles = vaultByID[newItem.type];

				foreach (ItemEntry possible in possibles)
				{
					if (possible.TryDeposit(newItem))
						return true;
				}

				newEntry = NewEntry(newItem.Clone());
				return newEntry.TryDeposit(newItem);
			}
			else
			{
				newEntry = NewEntry(newItem.Clone());
				return newEntry.TryDeposit(newItem);
			}
		}

		/// <summary>
		/// Creates and properly wires up a new entry to the vault
		/// </summary>
		/// <param name="type">The item type for this entry</param>
		/// <returns>The newly created entry</returns>
		public static ItemEntry NewEntry(Item item)
		{
			ItemEntry newEntry = new(item);

			if (vaultByID.ContainsKey(item.type))
				vaultByID[item.type].Add(newEntry);

			else
				vaultByID.Add(item.type, new List<ItemEntry>() { newEntry });

			vault.Add(newEntry);

			return newEntry;
		}

		/// <summary>
		/// Reset vault data on world clear
		/// </summary>
		public override void ClearWorld()
		{
			vault = new();
			vaultByID = new();
			baseCapacity = 20000;
			stoneFlags = 0;

			for (int k = 0; k < 8; k++)
			{
				var stone = (Stones)(1 << k);

				if ((stoneFlags & stone) > 0)
					(Dragonstone.samples[stone].ModItem as Dragonstone).Reset();
			}

			if (Main.netMode != NetmodeID.Server)
			{
				UILoader.GetUIState<VaultBrowser>().initialized = false;
				UILoader.GetUIState<VaultBrowser>().visible = false;
			}
		}

		/// <summary>
		/// Serialize vault data for saving.
		/// </summary>
		/// <param name="tag">The saved tagCompound</param>
		public override void SaveWorldData(TagCompound tag)
		{
			List<TagCompound> tags = new();

			vault.ForEach(n =>
			{
				TagCompound t = new();
				n.Save(t);
				tags.Add(t);
			});

			tag["vault"] = tags;
			tag["capacity"] = baseCapacity;
			tag["stones"] = (int)stoneFlags;
			tag["Favorites"] = Favorites;
			tag["CraftFavorites"] = CraftFavorites;
		}

		/// <summary>
		/// Deserialize vault data and re-insert it as appropriate into lookup dicts
		/// </summary>
		/// <param name="tag">The tag containing the vault data</param>
		public override void LoadWorldData(TagCompound tag)
		{
			var tags = (List<TagCompound>)tag.GetList<TagCompound>("vault");

			foreach (TagCompound entryTag in tags)
			{
				ItemEntry entry = new();
				entry.Load(entryTag);

				if (vaultByID.ContainsKey(entry.item.type))
					vaultByID[entry.item.type].Add(entry);

				else
					vaultByID.Add(entry.item.type, new List<ItemEntry>() { entry });

				vault.Add(entry);
			}

			baseCapacity = tag.GetInt("capacity");
			stoneFlags = (Stones)tag.GetInt("stones");

			for (int k = 0; k < 8; k++)
			{
				var stone = (Stones)(1 << k);

				if ((stoneFlags & stone) > 0)
					(Dragonstone.samples[stone].ModItem as Dragonstone).OnSlot();
			}

			Favorites = (List<string>)tag.GetList<string>("Favorites");
			CraftFavorites = (List<string>)tag.GetList<string>("CraftFavorites");
		}
	}

	internal class StoragePlayer : ModPlayer
	{
		public override IEnumerable<Item> AddMaterialsForCrafting(out ItemConsumedCallback itemConsumedCallback)
		{
			if ((StorageSystem.stoneFlags & Stones.Citrine) <= 0)
			{
				itemConsumedCallback = (a, b) => { };
				return new List<Item>();
			}

			bool nearValidTile = Player.adjTile[ModContent.TileType<Vault>()] || Player.adjTile[ModContent.TileType<CraftingCrucible>()];

			if ((StorageSystem.stoneFlags & Stones.Cerulean) > 0 && Player.adjTile[TileID.TeleportationPylon])
				nearValidTile = true;

			if ((StorageSystem.stoneFlags & Stones.Azure) <= 0 && !nearValidTile)
			{
				itemConsumedCallback = (a, b) => { };
				return new List<Item>();
			}

			List<Item> toCraft = new();

			foreach (ItemEntry item in StorageSystem.vault)
			{
				item.item.stack = item.simStack;
				toCraft.Add(item.item);
			}

			itemConsumedCallback = (item, index) =>
			{
				if (StorageSystem.vaultByID.ContainsKey(item.type))
				{
					List<ItemEntry> possibles = StorageSystem.vaultByID[item.type];

					foreach (ItemEntry possible in possibles)
					{
						if (Helpers.Helper.CanStack(possible.item, item))
						{
							int withdrawl = possible.simStack - item.stack;
							possible.simStack -= withdrawl;

							if (possible.CheckGone())
								VaultBrowser.Rebuild();

							VaultNet.SendWithdrawl(withdrawl, possible.item);
							return;
						}

						Mod.Logger.Error("Tried to overdraw when crafting!");
					}
				}
				else
				{
					Mod.Logger.Error("Tried to overdraw when crafting!");
				}
			};

			return toCraft;
		}
	}

	/// <summary>
	/// Represents an entry in the vault for an item. These are by type, or based on stackability.
	/// </summary>
	internal class ItemEntry
	{
		public Item item;
		public int simStack;
		public string guid;

		public ItemEntry() { }

		public ItemEntry(Item item)
		{
			this.item = item;
			simStack = 0;
			guid = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Attempts to deposit an item into this entry
		/// </summary>
		/// <param name="newItem">The item to deposit</param>
		/// <returns>If the item was atleast partially deposited or not</returns>
		public bool TryDeposit(Item newItem)
		{
			// Type matches
			if (newItem is null || newItem.type != item.type)
				return false;

			// Can be stacked
			if (!Helpers.Helper.CanStack(newItem, item))
				return false;

			int amountToAdd = newItem.stack;

			if (amountToAdd > StorageSystem.RemainingCapacity)
				amountToAdd = StorageSystem.RemainingCapacity;

			newItem.stack -= amountToAdd;
			simStack += amountToAdd;

			if (newItem.stack <= 0)
				newItem.TurnToAir();

			return true;
		}

		/// <summary>
		/// Checks and returns if this entry should be destroyed, and if so, destroys it.
		/// </summary>
		/// <returns>If this entry was destroyed</returns>
		public bool CheckGone()
		{
			if (simStack <= 0)
			{
				if (StorageSystem.vault.Contains(this))
					StorageSystem.vault.Remove(this);

				if (StorageSystem.vaultByID.ContainsKey(item.type) && StorageSystem.vaultByID[item.type].Contains(this))
				{
					StorageSystem.vaultByID[item.type].Remove(this);

					if (StorageSystem.vaultByID[item.type].Count <= 0)
						StorageSystem.vaultByID.Remove(item.type);
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Serialize an ItemEntry
		/// </summary>
		/// <param name="tag">tag containing item entry data</param>
		public void Save(TagCompound tag)
		{
			tag["item"] = item;
			tag["stack"] = simStack;
			tag["guid"] = guid;
		}

		/// <summary>
		/// Deserialize an ItemEntry
		/// </summary>
		/// <param name="tag">tag containing item entry data</param>
		public void Load(TagCompound tag)
		{
			item = tag.Get<Item>("item");
			simStack = tag.GetInt("stack");
			guid = tag.GetString("guid");

			// fallback for legacy saves
			if (string.IsNullOrEmpty(guid))
				guid = Guid.NewGuid().ToString();
		}
	}
}
