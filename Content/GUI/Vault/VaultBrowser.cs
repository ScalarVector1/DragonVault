using DragonVault.Content.Filters;
using DragonVault.Content.Filters.ItemFilters;
using DragonVault.Content.Items.Dragonstones;
using DragonVault.Core.Loaders.UILoading;
using DragonVault.Core.Networking;
using DragonVault.Core.Systems;
using DragonVault.Core.Systems.ThemeSystem;
using DragonVault.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonVault.Content.GUI.Vault
{
	internal class VaultBrowser : Browser
	{
		public bool canWithdraw = false;
		public bool fromTile = false;

		public StorageButton button;
		public DepositButton deposit;

		public StoneSlot[] slots;

		public override string Name => Main.worldName + "'s Vault";

		public override string IconTexture => "ItemSpawner";

		public override Vector2 DefaultPosition => new(0.6f, 0.4f);

		public override void PostInitialize()
		{
			button = new();
			Append(button);

			deposit = new();
			Append(deposit);

			slots = new StoneSlot[]
			{
				new StoneSlot(Stones.Rose),
				new StoneSlot(Stones.Citrine),
				new StoneSlot(Stones.Radiant),
				new StoneSlot(Stones.Verdant),
				new StoneSlot(Stones.Cerulean),
				new StoneSlot(Stones.Azure),
				new StoneSlot(Stones.Midnight),
				new StoneSlot(Stones.Pure)
			};

			foreach (StoneSlot slot in slots)
			{
				Append(slot);
			}
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<ItemButton>();

			foreach (ItemEntry entry in StorageSystem.vault)
			{
				buttons.Add(new ItemButton(entry, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Mod");
			filters.AddFilter(new Filter(Assets.Filters.Vanilla, "Tools.ItemSpawner.Filters.Vanilla", n => !(n is ItemButton && (n as ItemButton).entry.item.ModItem is null)) { isModFilter = true });

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModItem>().Count() > 0))
			{
				filters.AddFilter(new ItemModFilter(mod));
			}

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Damage");
			filters.AddFilter(new Filter(Assets.Filters.Unknown, "Tools.ItemSpawner.Filters.AnyDamage", n => !(n is ItemButton && (n as ItemButton).entry.item.damage > 0)));
			filters.AddFilter(new DamageClassFilter(DamageClass.Melee, Assets.Filters.Melee));
			filters.AddFilter(new DamageClassFilter(DamageClass.Ranged, Assets.Filters.Ranged));
			filters.AddFilter(new Filter(Assets.Filters.Ammo, "Tools.ItemSpawner.Filters.Ammo", n => n is ItemButton ib && ib.entry.item.ammo == AmmoID.None));
			filters.AddFilter(new DamageClassFilter(DamageClass.Magic, Assets.Filters.Magic));
			filters.AddFilter(new DamageClassFilter(DamageClass.Summon, Assets.Filters.Summon));
			filters.AddFilter(new DamageClassFilter(DamageClass.Throwing, Assets.Filters.Throwing));

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Equipment");
			filters.AddFilter(new Filter(Assets.Filters.Defense, "Tools.ItemSpawner.Filters.Armor", n => !(n is ItemButton && (n as ItemButton).entry.item.defense > 0)));
			filters.AddFilter(new Filter(Assets.Filters.Accessory, "Tools.ItemSpawner.Filters.Accessory", n => !(n is ItemButton && (n as ItemButton).entry.item.accessory)));
			filters.AddFilter(new Filter(Assets.Filters.Wings, "Tools.ItemSpawner.Filters.Wings", n => n is ItemButton ib && ib.entry.item.wingSlot == -1));
			filters.AddFilter(new Filter(Assets.Filters.Hooks, "Tools.ItemSpawner.Filters.Hooks", n => n is ItemButton ib && !Main.projHook[ib.entry.item.shoot]));
			filters.AddFilter(new Filter(Assets.Filters.Mounts, "Tools.ItemSpawner.Filters.Mounts", n => n is ItemButton ib && ib.entry.item.mountType == -1));
			filters.AddFilter(new Filter(Assets.Filters.Vanity, "Tools.ItemSpawner.Filters.Vanity", n => n is ItemButton ib && !ib.entry.item.vanity));
			filters.AddFilter(new Filter(Assets.Filters.Pets, "Tools.ItemSpawner.Filters.Pets", n => n is ItemButton ib && !(Main.vanityPet[ib.entry.item.buffType] || Main.lightPet[ib.entry.item.buffType])));

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Utility");
			filters.AddFilter(new Filter(Assets.Filters.Pickaxe, "Tools.ItemSpawner.Filters.Pickaxe", n => n is ItemButton ib && ib.entry.item.pick == 0));
			filters.AddFilter(new Filter(Assets.Filters.Axe, "Tools.ItemSpawner.Filters.Axe", n => n is ItemButton ib && ib.entry.item.axe == 0));
			filters.AddFilter(new Filter(Assets.Filters.Hammer, "Tools.ItemSpawner.Filters.Hammer", n => n is ItemButton ib && ib.entry.item.hammer == 0));
			filters.AddFilter(new Filter(Assets.Filters.Placeable, "Tools.ItemSpawner.Filters.Placeable", n => !(n is ItemButton && (n as ItemButton).entry.item.createTile >= TileID.Dirt || (n as ItemButton).entry.item.createWall >= 0)));
			filters.AddFilter(new Filter(Assets.Filters.Consumables, "Tools.ItemSpawner.Filters.Consumables", n => n is ItemButton ib && (!ib.entry.item.consumable || ib.entry.item.createTile >= TileID.Dirt || ib.entry.item.createWall >= 0)));

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Misc");
			filters.AddFilter(new Filter(Assets.Filters.MakeNPC, "Tools.ItemSpawner.Filters.MakeNPC", n => n is ItemButton ib && ib.entry.item.makeNPC == 0));
			filters.AddFilter(new Filter(Assets.Filters.Expert, "Tools.ItemSpawner.Filters.Expert", n => n is ItemButton ib && !ib.entry.item.expert));
			filters.AddFilter(new Filter(Assets.Filters.Master, "Tools.ItemSpawner.Filters.Master", n => n is ItemButton ib && !ib.entry.item.master));
			filters.AddFilter(new Filter(Assets.Filters.Material, "Tools.ItemSpawner.Filters.Material", n => n is ItemButton ib && !ItemID.Sets.IsAMaterial[ib.entry.item.type]));
			filters.AddFilter(new Filter(Assets.Filters.Unknown, "Tools.ItemSpawner.Filters.Deprecated", n => n is ItemButton ib && !ItemID.Sets.Deprecated[ib.entry.item.type]));
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			base.AdjustPositions(newPos);

			button.Left.Set(newPos.X - 180, 0);
			button.Top.Set(newPos.Y, 0);

			deposit.Left.Set(newPos.X - 180, 0);
			deposit.Top.Set(newPos.Y + 85, 0);

			int y = 0;

			foreach (StoneSlot slot in slots)
			{
				slot.Left.Set(newPos.X - 74, 0);
				slot.Top.Set(newPos.Y + 130 + y, 0);
				y += 60;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			int used = StorageSystem.MaxCapacity - StorageSystem.RemainingCapacity;
			int max = StorageSystem.MaxCapacity;

			Utils.DrawBorderStringBig(spriteBatch, $"{used}/{max}", basePos + new Vector2(24, 48), Color.White, 0.4f);
		}

		public override void DraggableUpdate(GameTime gameTime)
		{
			Main.playerInventory = true;

			if (Main.LocalPlayer.controlInv)
			{
				visible = false;
				Main.playerInventory = false;
				return;
			}

			bool nearVault = false;
			var tilePos = (Main.LocalPlayer.Center / 16).ToPoint16();

			for (int x = -10; x < 10; x++)
			{
				for (int y = -10; y < 10; y++)
				{
					Point16 off = new(x, y);
					Point16 target = tilePos + off;

					Tile tile = Framing.GetTileSafely(target);

					if (tile.HasTile && tile.TileType == ModContent.TileType<Tiles.Vault>())
						nearVault = true;
				}
			}

			if (fromTile && !nearVault)
				visible = false;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseItem != null && !Main.mouseItem.IsAir)
			{
				Item item = Main.mouseItem.Clone();
				VaultNet.SendDeposit(item.stack, item);

				bool added = StorageSystem.TryAddItem(Main.mouseItem, out ItemEntry newEntry);

				if (added && newEntry != null)
					Rebuild();
			}
		}

		public static void Rebuild()
		{
			UILoader.GetUIState<VaultBrowser>().options.Clear();
			UILoader.GetUIState<VaultBrowser>().PopulateGrid(UILoader.GetUIState<VaultBrowser>().options);
		}
	}

	internal class ItemButton : BrowserButton
	{
		public ItemEntry entry;
		public int stackDelay = 0;
		public Item renderCopy;

		public override string Identifier => entry.item.Name;

		public ItemButton(ItemEntry item, Browser browser) : base(browser)
		{
			entry = item;
			renderCopy = entry.item.Clone();
			renderCopy.stack = 1;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Main.inventoryScale = 36 / 52f * iconBox.Width / 36f;
			ItemSlot.Draw(spriteBatch, ref renderCopy, 21, GetDimensions().Position());

			Utils.DrawBorderString(spriteBatch, entry.simStack > 999 ? $"{entry.simStack / 1000}k" : $"{entry.simStack}", GetDimensions().ToRectangle().BottomLeft() + new Vector2(2, -16), Color.White, 0.8f);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = entry.item.Clone();
				Main.HoverItem.stack = entry.simStack;
				Main.hoverItemName = "Unknown";
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!UILoader.GetUIState<VaultBrowser>().canWithdraw)
			{
				Main.NewText("Requires Azure Dragonstone", new Color(50, 50, 255));
				return;
			}

			int withdrawn = Math.Min(entry.item.maxStack, entry.simStack);

			if (Main.keyState.PressingShift())
			{
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), entry.item, withdrawn);
				entry.simStack -= withdrawn;

				if (entry.CheckGone())
					VaultBrowser.Rebuild();

				VaultNet.SendWithdrawl(withdrawn, entry.item);
			}
			else if (Main.mouseItem.IsAir)
			{
				Main.playerInventory = true;
				Main.mouseItem = entry.item.Clone();
				Main.mouseItem.stack = withdrawn;
				entry.simStack -= withdrawn;

				if (entry.CheckGone())
					VaultBrowser.Rebuild();

				VaultNet.SendWithdrawl(withdrawn, entry.item);
			}
		}

		public override void SafeRightMouseDown(UIMouseEvent evt)
		{
			if (!UILoader.GetUIState<VaultBrowser>().canWithdraw)
			{
				Main.NewText("Requires Azure Dragonstone", new Color(50, 50, 255));
				return;
			}

			stackDelay = 30;

			if (Main.mouseItem.IsAir)
			{
				Main.mouseItem = entry.item.Clone();
				Main.mouseItem.stack = 1;
				entry.simStack--;

				if (entry.CheckGone())
					VaultBrowser.Rebuild();

				VaultNet.SendWithdrawl(1, entry.item);
			}
			else if (Main.mouseItem.type == entry.item.type && Helper.CanStack(Main.mouseItem, entry.item) && Main.mouseItem.stack < Main.mouseItem.maxStack)
			{
				Main.mouseItem.stack++;
				entry.simStack--;

				if (entry.CheckGone())
					VaultBrowser.Rebuild();

				VaultNet.SendWithdrawl(1, entry.item);
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

			// Prevents net spam... a tad inconvenient but.. oh well
			if (Main.netMode != NetmodeID.SinglePlayer)
				return;

			if (!UILoader.GetUIState<VaultBrowser>().canWithdraw)
				return;

			// Allows for "Hold RMB to get more
			if (IsMouseHovering && Main.mouseRight && Main.mouseItem.type == entry.item.type)
			{
				if (stackDelay > 0)
				{
					stackDelay--;
				}
				else if (Main.mouseItem.stack < Main.mouseItem.maxStack)
				{
					Main.mouseItem.stack++;
					entry.simStack--;

					if (entry.CheckGone())
						VaultBrowser.Rebuild();
				}
			}
		}

		public override int CompareTo(object obj)
		{
			return entry.item.type - (obj as ItemButton).entry.item.type;
		}
	}

	internal class StorageButton : UIElement
	{
		public StorageButton()
		{
			Width.Set(160, 0);
			Height.Set(80, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawBox = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, drawBox, ThemeHandler.ButtonColor);

			Utils.DrawBorderString(spriteBatch, $"Increase storage", drawBox.Center.ToVector2() + new Vector2(0, -24), Color.White, 1, 0.5f, 0f);
			Utils.DrawBorderString(spriteBatch, $"Cost: {StorageSystem.baseCapacity / 1000} gold", drawBox.Center.ToVector2() + new Vector2(0, 0), Color.Gold, 1, 0.5f, 0f);

		}

		public override void LeftClick(UIMouseEvent evt)
		{
			if (Main.LocalPlayer.CanAfford(Item.buyPrice(0, StorageSystem.baseCapacity / 1000)))
			{
				Main.LocalPlayer.PayCurrency(Item.buyPrice(0, StorageSystem.baseCapacity / 1000, 0, 0));
				StorageSystem.baseCapacity += 2000;
				Main.NewText($"Vault size increased to {StorageSystem.MaxCapacity} for {Main.worldName}!", Color.Gold);

				VaultNet.Data();
			}
			else
			{
				Main.NewText("Insufficient coins", Color.Red);
			}
		}
	}

	internal class DepositButton : UIElement
	{
		public DepositButton()
		{
			Width.Set(160, 0);
			Height.Set(40, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawBox = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, drawBox, ThemeHandler.ButtonColor);

			Utils.DrawBorderString(spriteBatch, $"Deposit all", drawBox.Center.ToVector2(), Color.White, 1, 0.5f, 0.4f);

			if (IsMouseHovering)
			{
				Tooltip.SetName("Deposit all");
				Tooltip.SetTooltip("Deposit all non-favorited items into the vault");
			}
		}

		public override void LeftClick(UIMouseEvent evt)
		{
			// Ignore hotbar and ammo/coin slots
			for (int k = 10; k < Main.LocalPlayer.inventory.Length - 9; k++)
			{
				var item = Main.LocalPlayer.inventory[k];

				// Ignore favorited items
				if (!item.IsAir && !item.favorited)
				{
					VaultNet.SendDeposit(item.stack, item);

					bool added = StorageSystem.TryAddItem(item, out ItemEntry newEntry);

					if (added && newEntry != null)
						VaultBrowser.Rebuild();
				}
			}
		}
	}
}