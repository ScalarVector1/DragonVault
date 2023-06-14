using DragonVault.Content.Filters;
using DragonVault.Content.Filters.ItemFilters;
using DragonVault.Core.Loaders.UILoading;
using DragonVault.Core.Systems;
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
		public override string Name => Main.worldName + "'s Vault";

		public override string IconTexture => "ItemSpawner";

		public override string HelpLink => "https://github.com/ScalarVector1/DragonVault/wiki/Item-spawner";

		public override Vector2 DefaultPosition => new(0.3f, 0.4f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<ItemButton>();

			foreach (ItemEntry entry in StorageSystem.vault)
			{
				buttons.Add(new ItemButton(entry, this));
			}

			grid.AddRange(buttons);
			Main.NewText(grid._items.Count);
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

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			Utils.DrawBorderStringBig(spriteBatch, $"{StorageSystem.maxCapacity - StorageSystem.remainingCapacity}/{StorageSystem.maxCapacity}", basePos + new Vector2(24, 48), Color.White, 0.4f);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			Main.NewText(options._items.Count);

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseItem != null && !Main.mouseItem.IsAir)
			{
				bool added = StorageSystem.TryAddItem(Main.mouseItem, out ItemEntry newEntry);

				if (added && newEntry != null)
				{
					Main.NewText("Deposited and created!");
					Rebuild();
					return;
				}

				Main.NewText(added ? "Deposited!" : "Not deposited...");
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
			if (Main.mouseItem.IsAir)
			{
				int withdrawn = Math.Min(entry.item.maxStack, entry.simStack);

				Main.mouseItem = entry.item.Clone();
				Main.mouseItem.stack = withdrawn;
				entry.simStack -= withdrawn;

				if (entry.CheckGone())
					VaultBrowser.Rebuild();
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
			}
			else if (Main.mouseItem.type == entry.item.type)
			{
				Main.mouseItem.stack++;
				entry.simStack--;

				if (entry.CheckGone())
					VaultBrowser.Rebuild();
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

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
}