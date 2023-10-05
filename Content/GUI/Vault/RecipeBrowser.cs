using DragonVault.Content.Filters;
using DragonVault.Content.Filters.ItemFilters;
using DragonVault.Core.Loaders.UILoading;
using DragonVault.Core.Systems.ThemeSystem;
using DragonVault.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
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
			display.Left.Set(newPos.X - 220, 0);
			display.Top.Set(newPos.Y, 0);

			base.AdjustPositions(newPos);
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<RecipeButton>();

			foreach (Recipe recipe in Main.recipe)
			{
				buttons.Add(new RecipeButton(this, recipe));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Mod");
			filters.AddFilter(new Filter("DragonVault/Assets/Filters/Vanilla", "Tools.ItemSpawner.Filters.Vanilla", n => !(n is RecipeButton && (n as RecipeButton).result.ModItem is null)));

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModItem>().Count() > 0))
			{

			}
		}
	}

	internal class RecipeDisplay : SmartUIElement
	{
		public Recipe activeRecipe;

		public UIList requirements;

		public RecipeDisplay()
		{
			Width.Set(200, 0);
			Height.Set(550, 0);
		}

		public override void OnInitialize()
		{
			requirements = new();
			requirements.Left.Set(10, 0);
			requirements.Top.Set(66, 0);
			requirements.Width.Set(180, 0);
			requirements.Height.Set(700, 0);
			Append(requirements);
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

			Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.BackgroundColor);

			Vector2 pos = GetDimensions().Position();

			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)pos.X + 10, (int)pos.Y + 10, 46, 46), ThemeHandler.ButtonColor);
			Utils.DrawBorderString(spriteBatch, $"{activeRecipe.createItem.Name} ({activeRecipe.createItem.stack})", pos + new Vector2(62, 42), Color.White, 0.9f, 0, 0.7f);

			Main.inventoryScale = 38f / 36f;
			ItemSlot.Draw(spriteBatch, ref activeRecipe.createItem, 21, GetDimensions().Position() + Vector2.One * 4);

			base.Draw(spriteBatch);
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
				return recipe.RecipeIndex.CompareTo(button.recipe.RecipeIndex);

			return base.CompareTo(obj);
		}
	}

	internal class RecipeInfo : SmartUIElement
	{
		public Item item;
		public string text = "Unknown";

		private RecipeGroup group;

		private int groupTimer;
		private HashSet<int>.Enumerator enumerator;

		public RecipeInfo(Item item)
		{
			this.item = item;
			text = item.Name;

			Width.Set(180, 0);
			Height.Set(36, 0);
		}

		public RecipeInfo(RecipeGroup group, int stack)
		{
			this.item = new Item();
			item.SetDefaults(group.IconicItemId);
			item.stack = stack;

			this.group = group;
			this.enumerator = group.ValidItems.GetEnumerator();

			text = group.GetText();

			Width.Set(180, 0);
			Height.Set(36, 0);
		}

		public RecipeInfo(int tileType)
		{
			this.item = null;

			Width.Set(180, 0);
			Height.Set(36, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var iconBox = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, iconBox, ThemeHandler.ButtonColor);
			GUIHelper.DrawBox(spriteBatch, new Rectangle(iconBox.X, iconBox.Y, 36, 36), ThemeHandler.ButtonColor);

			Main.inventoryScale = 36 / 52f * 36 / 36f;
			ItemSlot.Draw(spriteBatch, ref item, 21, GetDimensions().Position());

			float scale = Math.Min(0.8f, 144f / Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(text).X * 0.8f);
			Utils.DrawBorderString(spriteBatch, text ?? "Unknown", iconBox.TopLeft() + new Vector2(40, 21), Color.White, scale, 0, 0.5f);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = item.Clone();
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

			// TODO: Darken if materials are not available
		}
	}
}
