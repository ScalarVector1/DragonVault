using DragonVault.Content.Filters;
using DragonVault.Content.Filters.ItemFilters;
using DragonVault.Content.GUI.FieldEditors;
using DragonVault.Content.Items.Dragonstones;
using DragonVault.Core.Loaders.UILoading;
using DragonVault.Core.Systems;
using DragonVault.Core.Systems.ThemeSystem;
using DragonVault.Helpers;
using Microsoft.Win32;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.ObjectData;
using Terraria.UI;

namespace DragonVault.Content.GUI.Vault
{
	internal class RecipeBrowser : Browser
	{
		public static RecipeDisplay display;

		public VaultSwapButton vault;

		public bool fromTile;

		public override string Name => "Crafting";

		public override string IconTexture => "InfiniteReach";

		public override string HelpLink => "https://github.com/ScalarVector1/DragonVault/wiki/Item-spawner";

		public override Vector2 DefaultPosition => new(0.6f, 0.4f);

		public override List<string> Favorites => StorageSystem.CraftFavorites;

		public override void PostInitialize()
		{
			display = new();
			display.OnInitialize();
			Append(display);

			vault = new();
			Append(vault);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			display.Left.Set(newPos.X - 240, 0);
			display.Top.Set(newPos.Y, 0);

			vault?.Left.Set(newPos.X, 0);
			vault?.Top.Set(newPos.Y - 46, 0);

			base.AdjustPositions(newPos);
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<RecipeButton>();

			foreach (Recipe recipe in Main.recipe.Where(n => n.createItem != null && !n.createItem.IsAir))
			{
				buttons.Add(new RecipeButton(this, recipe));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Crafting");
			filters.AddFilter(new Filter(Assets.Filters.Hammer, "Tools.ItemSpawner.Filters.Craftable", n => !(n is RecipeButton && (n as RecipeButton).craftable)));

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Mod");
			filters.AddFilter(new Filter(Assets.Filters.Vanilla, "Tools.ItemSpawner.Filters.Vanilla", n => !(n is RecipeButton && (n as RecipeButton).result.ModItem is null)) { isModFilter = true });

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModItem>().Count() > 0))
			{
				filters.AddFilter(new ItemModFilter(mod));
			}

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Damage");
			filters.AddFilter(new Filter(Assets.Filters.Unknown, "Tools.ItemSpawner.Filters.AnyDamage", n => !(n is RecipeButton && (n as RecipeButton).result.damage > 0)));
			filters.AddFilter(new DamageClassFilter(DamageClass.Melee, Assets.Filters.Melee));
			filters.AddFilter(new DamageClassFilter(DamageClass.Ranged, Assets.Filters.Ranged));
			filters.AddFilter(new Filter(Assets.Filters.Ammo, "Tools.ItemSpawner.Filters.Ammo", n => n is RecipeButton ib && ib.result.ammo == AmmoID.None));
			filters.AddFilter(new DamageClassFilter(DamageClass.Magic, Assets.Filters.Magic));
			filters.AddFilter(new DamageClassFilter(DamageClass.Summon, Assets.Filters.Summon));
			filters.AddFilter(new DamageClassFilter(DamageClass.Throwing, Assets.Filters.Throwing));

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Equipment");
			filters.AddFilter(new Filter(Assets.Filters.Defense, "Tools.ItemSpawner.Filters.Armor", n => !(n is RecipeButton && (n as RecipeButton).result.defense > 0)));
			filters.AddFilter(new Filter(Assets.Filters.Accessory, "Tools.ItemSpawner.Filters.Accessory", n => !(n is RecipeButton && (n as RecipeButton).result.accessory)));
			filters.AddFilter(new Filter(Assets.Filters.Wings, "Tools.ItemSpawner.Filters.Wings", n => n is RecipeButton ib && ib.result.wingSlot == -1));
			filters.AddFilter(new Filter(Assets.Filters.Hooks, "Tools.ItemSpawner.Filters.Hooks", n => n is RecipeButton ib && !Main.projHook[ib.result.shoot]));
			filters.AddFilter(new Filter(Assets.Filters.Mounts, "Tools.ItemSpawner.Filters.Mounts", n => n is RecipeButton ib && ib.result.mountType == -1));
			filters.AddFilter(new Filter(Assets.Filters.Vanity, "Tools.ItemSpawner.Filters.Vanity", n => n is RecipeButton ib && !ib.result.vanity));
			filters.AddFilter(new Filter(Assets.Filters.Pets, "Tools.ItemSpawner.Filters.Pets", n => n is RecipeButton ib && !(Main.vanityPet[ib.result.buffType] || Main.lightPet[ib.result.buffType])));

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Utility");
			filters.AddFilter(new Filter(Assets.Filters.Pickaxe, "Tools.ItemSpawner.Filters.Pickaxe", n => n is RecipeButton ib && ib.result.pick == 0));
			filters.AddFilter(new Filter(Assets.Filters.Axe, "Tools.ItemSpawner.Filters.Axe", n => n is RecipeButton ib && ib.result.axe == 0));
			filters.AddFilter(new Filter(Assets.Filters.Hammer, "Tools.ItemSpawner.Filters.Hammer", n => n is RecipeButton ib && ib.result.hammer == 0));
			filters.AddFilter(new Filter(Assets.Filters.Placeable, "Tools.ItemSpawner.Filters.Placeable", n => !(n is RecipeButton && (n as RecipeButton).result.createTile >= TileID.Dirt || (n as RecipeButton).result.createWall >= 0)));
			filters.AddFilter(new Filter(Assets.Filters.Consumables, "Tools.ItemSpawner.Filters.Consumables", n => n is RecipeButton ib && (!ib.result.consumable || ib.result.createTile >= TileID.Dirt || ib.result.createWall >= 0)));

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Misc");
			filters.AddFilter(new Filter(Assets.Filters.MakeNPC, "Tools.ItemSpawner.Filters.MakeNPC", n => n is RecipeButton ib && ib.result.makeNPC == 0));
			filters.AddFilter(new Filter(Assets.Filters.Expert, "Tools.ItemSpawner.Filters.Expert", n => n is RecipeButton ib && !ib.result.expert));
			filters.AddFilter(new Filter(Assets.Filters.Master, "Tools.ItemSpawner.Filters.Master", n => n is RecipeButton ib && !ib.result.master));
			filters.AddFilter(new Filter(Assets.Filters.Material, "Tools.ItemSpawner.Filters.Material", n => n is RecipeButton ib && !ItemID.Sets.IsAMaterial[ib.result.type]));
			filters.AddFilter(new Filter(Assets.Filters.Unknown, "Tools.ItemSpawner.Filters.Deprecated", n => n is RecipeButton ib && !ItemID.Sets.Deprecated[ib.result.type]));
		}

		public override void SetupSorts()
		{
			SortModes.Add(new("ID", (a, b) => (a as RecipeButton).result.type - (b as RecipeButton).result.type));
			SortModes.Add(new("Alphabetical", (a, b) => a.Identifier.CompareTo(b.Identifier)));
			SortModes.Add(new("Damage", (a, b) => -1 * ((a as RecipeButton).result.damage - (b as RecipeButton).result.damage)));
			SortModes.Add(new("Defense", (a, b) => -1 * ((a as RecipeButton).result.defense - (b as RecipeButton).result.defense)));
			SortModes.Add(new("Value", (a, b) => -1 * ((a as RecipeButton).result.value - (b as RecipeButton).result.value)));

			SortFunction = SortModes.First().Function;
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
	}

	internal class RecipeDisplay : SmartUIElement
	{
		public Recipe activeRecipe;

		public UIList requirements;
		public TextField quantityField;
		public UIText max;

		public static int amountToMake = 1;

		public string shortName = "";

		static Asset<Texture2D> divider = Main.Assets.Request<Texture2D>("Images/UI/Divider");

		public RecipeDisplay()
		{
			Width.Set(220, 0);
			Height.Set(550, 0);
		}

		public override void OnInitialize()
		{
			if (UILoader.GetUIState<RecipeBrowser>()?.UserInterface != null)
			{
				StyledScrollbar scroll = new(UILoader.GetUIState<RecipeBrowser>().UserInterface);
				scroll.Left.Set(196, 0);
				scroll.Top.Set(66, 0);
				scroll.Width.Set(16, 0);
				scroll.Height.Set(430, 0);
				Append(scroll);

				requirements = new();
				requirements.Left.Set(10, 0);
				requirements.Top.Set(66, 0);
				requirements.Width.Set(180, 0);
				requirements.Height.Set(430, 0);
				requirements.SetScrollbar(scroll);
				Append(requirements);
			}

			quantityField = new();
			quantityField.Left.Set(136, 0);
			quantityField.Top.Set(512, 0);
			quantityField.Width.Set(64, 0);
			quantityField.Height.Set(26, 0);
			quantityField.inputType = InputType.integer;
			quantityField.currentValue = "1";
			Append(quantityField);

			UIText ex = new("x");
			ex.Left.Set(124, 0);
			ex.Top.Set(516, 0);
			Append(ex);

			max = new("^");
			max.Left.Set(174, 0);
			max.Top.Set(516, 0);
			max.Width.Set(26, 0);
			max.Height.Set(26, 0);
			max.TextOriginY = 0.5f;
			max.OnLeftClick += (a, b) => Max();
			Append(max);

			CraftButton craftButton = new();
			craftButton.Left.Set(20, 0);
			craftButton.Top.Set(508, 0);
			craftButton.OnLeftClick += (a, b) => Craft();
			Append(craftButton);
		}

		public void Repopulate()
		{
			requirements.Clear();

			activeRecipe.requiredItem.ForEach(item =>
			{
				int group = activeRecipe.acceptedGroups.FirstOrDefault(n => RecipeGroup.recipeGroups[n].ValidItems.Contains(item.type));

				if (group != 0)
					requirements.Add(new RecipeInfo(RecipeGroup.recipeGroups[group], item.stack));
				else
					requirements.Add(new RecipeInfo(item));
			});

			var seperator = new UIImage(divider);
			seperator.Width.Set(180, 0);
			seperator.Height.Set(4, 0);
			seperator.Color = Color.White * 0.5f;
			seperator.ScaleToFit = true;
			requirements.Add(seperator);

			activeRecipe.requiredTile.ForEach(item => requirements.Add(new RecipeInfo(item)));

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

			string name = $"{activeRecipe?.createItem?.Name ?? "None"}";
			shortName = name;
			while (font.MeasureString(shortName).X * 0.8f > 140)
			{
				shortName = shortName[..^1];
			}

			if (shortName != name)
				shortName += "...";

			Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (activeRecipe is null)
				return;

			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.BackgroundColor);

			Vector2 pos = GetDimensions().Position();

			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)pos.X + 10, (int)pos.Y + 10, 46, 46), ThemeHandler.ButtonColor);
			Utils.DrawBorderString(spriteBatch, shortName ?? "", pos + new Vector2(62, 36), Color.White, 0.8f, 0, 0.5f);

			Main.inventoryScale = 38f / 36f;
			ItemSlot.Draw(spriteBatch, ref activeRecipe.createItem, 21, GetDimensions().Position() + Vector2.One * 4);

			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;

			if (max.IsMouseHovering)
			{
				Tooltip.SetName("Set to max");
				Tooltip.SetTooltip("");
			}

			base.Draw(spriteBatch);
		}

		/// <summary>
		/// Gets the maximum amount of times the active recipe can be crafted
		/// </summary>
		/// <returns></returns>
		public int MaxCanCraft()
		{
			if (activeRecipe is null)
				return 1;

			int max = 9999;

			activeRecipe.requiredItem.ForEach(item =>
			{
				int proposed = 9999;

				int group = activeRecipe.acceptedGroups.FirstOrDefault(n => RecipeGroup.recipeGroups[n].ValidItems.Contains(item.type));

				if (group != 0)
					proposed = RecipeUtil.GetOwnedQuantity(RecipeGroup.recipeGroups[group]) / item.stack;
				else
					proposed = RecipeUtil.GetOwnedQuantity(item.type) / item.stack;

				if (proposed < max)
					max = proposed;
			});

			return max;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (quantityField.updated)
			{
				if (int.TryParse(quantityField.currentValue, out int amount))
					amountToMake = amount;
				else
					amountToMake = 1;

				if (amountToMake < 1)
					amountToMake = 1;

				if (amountToMake > 9999)
					amountToMake = 9999;
			}

			if (!quantityField.typing)
				quantityField.currentValue = $"{amountToMake}";
		}

		/// <summary>
		/// Craft the current active quantity of items
		/// </summary>
		public void Craft()
		{
			if (Main.availableRecipe.Contains(Array.IndexOf(Main.recipe, activeRecipe)) && MaxCanCraft() >= amountToMake)
			{
				int totalMade = 0;

				for (int k = 0; k < amountToMake; k++)
				{
					if (Main.availableRecipe.Contains(Array.IndexOf(Main.recipe, activeRecipe)))
					{
						Item crafted = activeRecipe.createItem.Clone();
						crafted.Prefix(-1);
						activeRecipe.Create();
						RecipeLoader.OnCraft(crafted, activeRecipe, Main.mouseItem);

						totalMade += activeRecipe.createItem.stack;
					}
					else
					{
						Main.NewText("Could not fully craft the requested amount!", Color.Red);
						break;
					}
				}

				while (totalMade > 0)
				{
					int thisStack = totalMade > activeRecipe.createItem.maxStack ? activeRecipe.createItem.maxStack : totalMade;

					Item crafted = activeRecipe.createItem.Clone();
					crafted.Prefix(-1);

					Main.LocalPlayer.QuickSpawnItemDirect(null, crafted, thisStack);
					totalMade -= thisStack;
				}
			}
		}

		public void Max()
		{
			amountToMake = Math.Max(1, MaxCanCraft());
			quantityField.currentValue = $"{amountToMake}";
		}
	}

	internal class CraftButton : SmartUIElement
	{
		public CraftButton()
		{
			Width.Set(90, 0);
			Height.Set(32, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			bool craftable = Main.availableRecipe.Contains(Array.IndexOf(Main.recipe, RecipeBrowser.display.activeRecipe)) && RecipeBrowser.display.MaxCanCraft() >= RecipeDisplay.amountToMake;

			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);
			Utils.DrawBorderString(spriteBatch, "Craft", GetDimensions().Center(), Color.White, 1f, 0.5f, 0.4f);

			if (!craftable)
				GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), Color.Black * 0.6f);
		}
	}

	internal class RecipeButton : BrowserButton
	{
		public Recipe recipe;

		public Item result;

		public int index;
		public bool craftable;

		public override string Identifier => result.Name;

		public override string Key => recipe.RecipeIndex.ToString();

		public RecipeButton(Browser parent, Recipe recipe) : base(parent)
		{
			this.recipe = recipe;

			result = recipe.createItem.Clone();
			index = Array.IndexOf(Main.recipe, recipe);
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Main.inventoryScale = 36 / 52f * iconBox.Width / 36f;
			ItemSlot.Draw(spriteBatch, ref result, 21, GetDimensions().Position());

			if (!craftable)
				GUIHelper.DrawBox(spriteBatch, iconBox, Color.Black * 0.6f);

			if (RecipeBrowser.display.activeRecipe == recipe)
				GUIHelper.DrawOutline(spriteBatch, iconBox, ThemeHandler.ButtonColor.InvertColor());

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = result.Clone();
				Main.hoverItemName = "Unknown";
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			craftable = false;

			for(int k = 0; k < Main.numAvailableRecipes; k++)
			{
				if (Main.availableRecipe[k] == index)
					craftable = true;
			}

			base.SafeUpdate(gameTime);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			RecipeBrowser.display.activeRecipe = recipe;
			RecipeBrowser.display.Repopulate();
		}
	}

	internal class RecipeInfo : SmartUIElement
	{
		public Item item;
		public int quantityForOne = 1;
		public string text = "Unknown";

		private RecipeGroup group;

		private int groupTimer;
		private HashSet<int>.Enumerator enumerator;

		public bool isTile;
		public int tileType;

		public int QuantityWanted => RecipeDisplay.amountToMake * quantityForOne;

		public RecipeInfo(Item item)
		{
			this.item = item.Clone();
			text = item.Name;
			quantityForOne = item.stack;

			this.item.stack = 1;

			Width.Set(180, 0);
			Height.Set(36, 0);
		}

		public RecipeInfo(RecipeGroup group, int stack)
		{
			this.item = new Item();
			item.SetDefaults(group.IconicItemId);
			quantityForOne = stack;

			this.group = group;
			this.enumerator = group.ValidItems.GetEnumerator();

			text = group.GetText();

			Width.Set(180, 0);
			Height.Set(36, 0);
		}

		public RecipeInfo(int tileType)
		{
			this.tileType = tileType;
			isTile = true;

			// This is horrible.
			item = ContentSamples.ItemsByType.FirstOrDefault(n => n.Value.createTile == tileType).Value ?? new Item();

			int requiredTileStyle = Recipe.GetRequiredTileStyle(tileType);
			string mapObjectName = Lang.GetMapObjectName(MapHelper.TileToLookup(tileType, requiredTileStyle));
			text = $"At {mapObjectName}";

			Width.Set(180, 0);
			Height.Set(36, 0);
		}

		public int GetOwnedQuantity()
		{
			if (group is null)
			{
				return RecipeUtil.GetOwnedQuantity(item.type);
			}
			else
			{
				return RecipeUtil.GetOwnedQuantity(group);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var iconBox = GetDimensions().ToRectangle();

			bool satisfied;

			if (!isTile)
			{
				int owned = GetOwnedQuantity();
				satisfied = owned >= QuantityWanted;
			}
			else
			{
				satisfied = Main.LocalPlayer.adjTile[tileType];
			}

			Color textColor = satisfied ? Color.White : new Color(255, 100, 100);

			GUIHelper.DrawBox(spriteBatch, iconBox, ThemeHandler.ButtonColor);
			GUIHelper.DrawBox(spriteBatch, new Rectangle(iconBox.X, iconBox.Y, 36, 36), ThemeHandler.ButtonColor);

			Main.inventoryScale = 36 / 52f * 36 / 36f;
			ItemSlot.Draw(spriteBatch, ref item, 21, GetDimensions().Position());

			float scale = Math.Min(0.8f, 144f / Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text).X * 0.8f);
			Utils.DrawBorderString(spriteBatch, text ?? "Unknown", iconBox.TopLeft() + new Vector2(40, 21), textColor, scale, 0, 0.5f);

			if (!satisfied)
				GUIHelper.DrawBox(spriteBatch, iconBox, Color.Black * 0.6f);

			if (!isTile)
			{
				int owned = GetOwnedQuantity();
				Utils.DrawBorderString(spriteBatch, $"{GetOwnedQuantity()}/{QuantityWanted}", iconBox.TopLeft() + new Vector2(0, 36), textColor, 0.7f, 0, 0.5f);
			}

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = item.Clone();
				Main.HoverItem.stack = QuantityWanted;
				Main.hoverItemName = "Unknown";
			}

			if (group != null)
			{
				groupTimer++;

				if (groupTimer >= 30)
				{
					groupTimer = 0;

					if (enumerator.MoveNext())
					{
						item.SetDefaults(enumerator.Current);
					}
					else
					{ 
						enumerator.Dispose();
						enumerator = group.ValidItems.GetEnumerator();
						enumerator.MoveNext();
						item.SetDefaults(enumerator.Current);
					}
				}
			}
		}
	}

	internal class VaultSwapButton : UIElement
	{
		public VaultSwapButton()
		{
			Width.Set(160, 0);
			Height.Set(40, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawBox = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, drawBox, ThemeHandler.ButtonColor);

			Utils.DrawBorderString(spriteBatch, "Vault", drawBox.Center.ToVector2(), Color.White, 1, 0.5f, 0.4f);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Tooltip.SetName("Vault");
				Tooltip.SetTooltip("View vault");
			}
		}

		public override void LeftClick(UIMouseEvent evt)
		{
			VaultBrowser rb = UILoader.GetUIState<VaultBrowser>();
			rb.visible = true;

			if (!rb.initialized)
			{
				rb.Refresh();
				rb.initialized = true;
			}

			rb.basePos = UILoader.GetUIState<RecipeBrowser>().basePos;
			rb.AdjustPositions(UILoader.GetUIState<RecipeBrowser>().basePos);
			rb.fromTile = UILoader.GetUIState<RecipeBrowser>().fromTile;

			UILoader.GetUIState<RecipeBrowser>().visible = false;
		}
	}

	public static class RecipeUtil
	{
		public static Dictionary<int, int> owned = typeof(Recipe).GetField("_ownedItems", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Dictionary<int, int>;

		/// <summary>
		/// Gets how many of this item the player owns for the purpose of crafting
		/// </summary>
		/// <param name="id">The Item ID to check</param>
		/// <returns>The amount of items</returns>
		public static int GetOwnedQuantity(int id)
		{
			return owned.ContainsKey(id) ? owned[id] : 0;
		}

		/// <summary>
		/// Gets how many valid items the player owns for the puropse of crafting
		/// </summary>
		/// <param name="group">The recipe group to check against</param>
		/// <returns>The amount of items</returns>
		public static int GetOwnedQuantity(RecipeGroup group)
		{
			int sum = 0;

			var en = group.ValidItems.GetEnumerator();

			do
			{
				sum += owned.ContainsKey(en.Current) ? owned[en.Current] : 0;
			}
			while (en.MoveNext());

			return sum;
		}
	}
}
