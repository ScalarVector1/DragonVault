using DragonVault.Content.Filters;
using DragonVault.Content.Filters.ItemFilters;
using DragonVault.Content.GUI.FieldEditors;
using DragonVault.Core.Loaders.UILoading;
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

		public override string Name => "Crafting";

		public override string IconTexture => "ItemSpawner";

		public override string HelpLink => "https://github.com/ScalarVector1/DragonVault/wiki/Item-spawner";

		public override Vector2 DefaultPosition => new(0.6f, 0.4f);

		public override void PostInitialize()
		{
			display = new();
			display.OnInitialize();
			Append(display);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			display.Left.Set(newPos.X - 240, 0);
			display.Top.Set(newPos.Y, 0);

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
			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Mod");
			filters.AddFilter(new Filter(Assets.Filters.Vanilla, "Tools.ItemSpawner.Filters.Vanilla", n => !(n is RecipeButton && (n as RecipeButton).result.ModItem is null)));

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModItem>().Count() > 0))
			{

			}
		}
	}

	internal class RecipeDisplay : SmartUIElement
	{
		public Recipe activeRecipe;

		public UIList requirements;

		public TextField quantityField;

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
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.BackgroundColor);

			Vector2 pos = GetDimensions().Position();

			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)pos.X + 10, (int)pos.Y + 10, 46, 46), ThemeHandler.ButtonColor);
			Utils.DrawBorderString(spriteBatch, shortName ?? "", pos + new Vector2(62, 36), Color.White, 0.8f, 0, 0.5f);


			if (activeRecipe != null)
			{
				Main.inventoryScale = 38f / 36f;
				ItemSlot.Draw(spriteBatch, ref activeRecipe.createItem, 21, GetDimensions().Position() + Vector2.One * 4);
			}

			base.Draw(spriteBatch);
		}

		/// <summary>
		/// Gets the maximum amount of times the active recipe can be crafted
		/// </summary>
		/// <returns></returns>
		public int MaxCanCraft()
		{
			int max = 9999;

			activeRecipe.requiredItem.ForEach(item =>
			{
				int proposed = 9999;

				int group = activeRecipe.acceptedGroups.FirstOrDefault(n => RecipeGroup.recipeGroups[n].ValidItems.Contains(item.type));

				if (group != 0)
					proposed = RecipeUtil.GetOwnedQuantity(group) / item.stack;
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
				var max = MaxCanCraft();

				if (int.TryParse(quantityField.currentValue, out int amount))
					amountToMake = amount;
				else
					amountToMake = 1;

				if (amountToMake < 1)
					amountToMake = 1;

				if (amountToMake > max)
					amountToMake = max;
			}

			if (!quantityField.typing)
				quantityField.currentValue = $"{amountToMake}";
		}

		/// <summary>
		/// Craft the current active quantity of items
		/// </summary>
		public void Craft()
		{
			for (int k = 0; k < amountToMake; k++)
			{
				if (Main.availableRecipe.Contains(Array.IndexOf(Main.recipe, activeRecipe)))
				{
					Item crafted = activeRecipe.createItem.Clone();
					crafted.Prefix(-1);
					activeRecipe.Create();
					RecipeLoader.OnCraft(crafted, activeRecipe, Main.mouseItem);

					Main.LocalPlayer.QuickSpawnItemDirect(null, crafted);

					PopupText.NewText(PopupTextContext.ItemCraft, crafted, activeRecipe.createItem.stack);

					//SoundEngine.PlaySound(SoundID.Item);
				}
				else
				{
					Main.NewText("Could not fully craft the requested amount!", Color.Red);
					break;
				}
			}
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
			bool craftable = Main.availableRecipe.Contains(Array.IndexOf(Main.recipe, RecipeBrowser.display.activeRecipe));

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

		public override string Identifier => result.Name;

		public RecipeButton(Browser parent, Recipe recipe) : base(parent)
		{
			this.recipe = recipe;

			result = recipe.createItem.Clone();
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Main.inventoryScale = 36 / 52f * iconBox.Width / 36f;
			ItemSlot.Draw(spriteBatch, ref result, 21, GetDimensions().Position());

			if (!Main.availableRecipe.Contains(Array.IndexOf(Main.recipe, recipe)))
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

		public override void SafeClick(UIMouseEvent evt)
		{
			RecipeBrowser.display.activeRecipe = recipe;
			RecipeBrowser.display.Repopulate();
		}

		public override int CompareTo(object obj)
		{
			if (obj is RecipeButton button)
				return result.type.CompareTo(button.result.type);

			return base.CompareTo(obj);
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
				Utils.DrawBorderString(spriteBatch, $"{GetOwnedQuantity()}/{QuantityWanted}", iconBox.TopLeft() + new Vector2(0, 36), textColor, 0.75f, 0, 0.5f);
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
