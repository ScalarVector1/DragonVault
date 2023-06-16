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
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonVault.Content.GUI.Vault
{
	internal class VaultBrowser : Browser
	{
		public StorageButton button;

		public StoneSlot[] slots;

		public override string Name => Main.worldName + "'s Vault";

		public override string IconTexture => "ItemSpawner";

		public override string HelpLink => "https://github.com/ScalarVector1/DragonVault/wiki/Item-spawner";

		public override Vector2 DefaultPosition => new(0.6f, 0.4f);

		public override void PostInitialize()
		{
			button = new();
			Append(button);

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
			filters.AddFilter(new Filter("DragonVault/Assets/Filters/Vanilla", "Tools.ItemSpawner.Filters.Vanilla", n => !(n is ItemButton && (n as ItemButton).entry.item.ModItem is null)));

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModItem>().Count() > 0))
			{
				filters.AddFilter(new ItemModFilter(mod));
			}

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Damage");
			filters.AddFilter(new Filter("DragonVault/Assets/Filters/Unknown", "Tools.ItemSpawner.Filters.AnyDamage", n => !(n is ItemButton && (n as ItemButton).entry.item.damage > 0)));
			filters.AddFilter(new DamageClassFilter(DamageClass.Melee, "DragonVault/Assets/Filters/Melee"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Ranged, "DragonVault/Assets/Filters/Ranged"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Magic, "DragonVault/Assets/Filters/Magic"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Summon, "DragonVault/Assets/Filters/Summon"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Throwing, "DragonVault/Assets/Filters/Throwing"));

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Misc");

			filters.AddFilter(new Filter("DragonVault/Assets/Filters/Accessory", "Tools.ItemSpawner.Filters.Accessory", n => !(n is ItemButton && (n as ItemButton).entry.item.accessory)));
			filters.AddFilter(new Filter("DragonVault/Assets/Filters/Defense", "Tools.ItemSpawner.Filters.Armor", n => !(n is ItemButton && (n as ItemButton).entry.item.defense > 0)));
			filters.AddFilter(new Filter("DragonVault/Assets/Filters/Placeable", "Tools.ItemSpawner.Filters.Placeable", n => !(n is ItemButton && (n as ItemButton).entry.item.createTile >= TileID.Dirt || (n as ItemButton).entry.item.createWall >= 0)));
			filters.AddFilter(new Filter("DragonVault/Assets/Filters/Unknown", "Tools.ItemSpawner.Filters.Deprecated", n => n is ItemButton ib && !ItemID.Sets.Deprecated[ib.entry.item.type]));
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			base.AdjustPositions(newPos);

			button.Left.Set(newPos.X - 180, 0);
			button.Top.Set(newPos.Y, 0);

			int y = 0;

			foreach (StoneSlot slot in slots)
			{
				slot.Left.Set(newPos.X - 74, 0);
				slot.Top.Set(newPos.Y + 104 + y, 0);
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

		public override void SafeClick(UIMouseEvent evt)
		{
			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseItem != null && !Main.mouseItem.IsAir)
			{
				bool added = StorageSystem.TryAddItem(Main.mouseItem, out ItemEntry newEntry);

				if (added && newEntry != null)
					Rebuild();

				VaultNet.SendDeposit(newEntry.simStack, newEntry.item);
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

		public override string Identifier => entry.item.Name;

		public ItemButton(ItemEntry item, Browser browser) : base(browser)
		{
			this.entry = item;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Main.inventoryScale = 36 / 52f * iconBox.Width / 36f;
			ItemSlot.Draw(spriteBatch, ref entry.item, 21, GetDimensions().Position());

			Utils.DrawBorderString(spriteBatch, entry.simStack > 999 ? $"{entry.simStack / 1000}k" : $"{entry.simStack}", GetDimensions().ToRectangle().BottomLeft() + new Vector2(2, -16), Color.White, 0.8f);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = entry.item.Clone();
				Main.HoverItem.stack = entry.simStack;
				Main.hoverItemName = "a";
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
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
			else if (Main.mouseItem.type == entry.item.type)
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
			Utils.DrawBorderString(spriteBatch, $"Cost: {StorageSystem.baseCapacity / 5000} gold", drawBox.Center.ToVector2() + new Vector2(0, 0), Color.Gold, 1, 0.5f, 0f);

		}

		public override void LeftClick(UIMouseEvent evt)
		{
			if (Main.LocalPlayer.CanAfford(Item.buyPrice(0, StorageSystem.baseCapacity / 5000)))
			{
				Main.LocalPlayer.PayCurrency(Item.buyPrice(0, StorageSystem.baseCapacity / 5000, 0, 0));
				StorageSystem.baseCapacity += 20000;
				Main.NewText($"Vault size increased to {StorageSystem.MaxCapacity} for {Main.worldName}!", Color.Gold);
			}
			else
			{
				Main.NewText("Insufficient coins", Color.Red);
			}
		}
	}
}