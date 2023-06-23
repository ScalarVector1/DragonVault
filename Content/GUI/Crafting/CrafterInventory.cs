using DragonVault.Core.Loaders.UILoading;
using DragonVault.Core.Systems;
using DragonVault.Core.Systems.ThemeSystem;
using DragonVault.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonVault.Content.GUI.Crafting
{
	internal class CrafterInventory : DraggableUIState
	{
		public UIGrid slots;

		public FixedUIScrollbar slotsScroll;

		public AddSlotButton button;

		public bool initialized = false;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 400, 32);

		public override Vector2 DefaultPosition => new(0.6f, 0.25f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 320;
			height = 480;

			slots = new();
			slots.Width.Set(260, 0);
			slots.Height.Set(400, 0);
			Append(slots);

			button = new();
			Append(button);
		}

		public void InitializeScrollbar()
		{
			slotsScroll = new(UserInterface);
			slotsScroll.Height.Set(400, 0);
			Append(slotsScroll);

			slots.SetScrollbar(slotsScroll);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
				visible = false;

			slots.Width.Set(260, 0);
			slots.Height.Set(400, 0);
			height = 480;

			slotsScroll.Left.Set((int)newPos.X + 290, 0);
			slotsScroll.Top.Set((int)newPos.Y + 60, 0);

			slots.Left.Set((int)newPos.X + 16, 0);
			slots.Top.Set((int)newPos.Y + 62, 0);

			button.Left.Set(newPos.X - 180, 0);
			button.Top.Set(newPos.Y, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, BoundingBox, ThemeHandler.BackgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 300, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ModContent.Request<Texture2D>("DragonVault/Content/Tiles/CraftingCrucibleItem").Value;
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			var bgDims = slots.GetDimensions().ToRectangle();
			bgDims.Inflate(4, 4);
			spriteBatch.Draw(background, bgDims, Color.Black * 0.25f);

			Utils.DrawBorderStringBig(spriteBatch, "Crafting Crucible", basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			base.Draw(spriteBatch);
		}

		public void Reset()
		{
			slots.Clear();

			for (int k = 0; k < CraftingSystem.slots; k++)
			{
				var newSlot = new CraftingSlot();

				if (CraftingSystem.stations.Count > k && CraftingSystem.stations[k] != null && !CraftingSystem.stations[k].IsAir)
					newSlot.item = CraftingSystem.stations[k];

				slots.Add(newSlot);
			}
		}
	}

	internal class CraftingSlot : SmartUIElement
	{
		public Item item;

		public CraftingSlot()
		{
			Width.Set(48, 0);
			Height.Set(48, 0);
			item = new();
			item.TurnToAir();
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!Main.mouseItem.IsAir && (item is null || item.IsAir))
			{
				if (Main.mouseItem.createTile <= 0)
				{
					Main.NewText("You can only place crafting stations in the crucible!", Color.Red);
					return;
				}

				if (CraftingSystem.stations.Any(n => n.createTile == Main.mouseItem.createTile))
				{
					Main.NewText("This crafting station (or an equivelent) is already in the crucible!", Color.Red);
					return;
				}

				bool usedAnywhere = false;
				foreach (Recipe recipe in Main.recipe)
				{
					if (recipe.requiredTile.Contains(Main.mouseItem.createTile))
						usedAnywhere = true;
				}

				if (!usedAnywhere)
				{
					Main.NewText("You can only place crafting stations in the crucible!", Color.Red);
					return;
				}

				Item clone = Main.mouseItem.Clone();
				clone.stack = 1;

				item = clone;
				CraftingSystem.stations.Add(clone);

				Main.mouseItem.stack--;

				if (Main.mouseItem.stack <= 0)
					Main.mouseItem.TurnToAir();

				Main.NewText($"{clone.Name} has been added to the crafting crucible for {Main.worldName}!");
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			CalculatedStyle dims = GetDimensions();
			var iconBox = new Rectangle((int)dims.X, (int)dims.Y, 48, 48);

			Main.inventoryScale = 36 / 52f * iconBox.Width / 36f;

			ItemSlot.Draw(spriteBatch, ref item, 21, iconBox.TopLeft());

			if (IsMouseHovering)
			{
				if (item.IsAir)
				{
					Tooltip.SetName("Crafting station slot");
					Tooltip.SetTooltip("Place a crafting station here to use it from the crucible NEWBLOCK WARNING: Adding a crafting station is permanent!");
				}
				else
				{
					Main.LocalPlayer.mouseInterface = true;
					Main.HoverItem = item;
					Main.hoverItemName = "a";
				}
			}
		}

		public override int CompareTo(object obj)
		{
			if (obj is CraftingSlot slot)
				return slot.item.type - item.type;

			return base.CompareTo(obj);
		}
	}

	internal class AddSlotButton : UIElement
	{
		public AddSlotButton()
		{
			Width.Set(160, 0);
			Height.Set(80, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var drawBox = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, drawBox, ThemeHandler.ButtonColor);

			Utils.DrawBorderString(spriteBatch, $"Add slot", drawBox.Center.ToVector2() + new Vector2(0, -24), Color.White, 1, 0.5f, 0f);
			Utils.DrawBorderString(spriteBatch, $"Cost: {CraftingSystem.slots * 4} gold", drawBox.Center.ToVector2() + new Vector2(0, 0), Color.Gold, 1, 0.5f, 0f);

		}

		public override void LeftClick(UIMouseEvent evt)
		{
			if (Main.LocalPlayer.CanAfford(Item.buyPrice(0, CraftingSystem.slots * 4)))
			{
				Main.LocalPlayer.PayCurrency(Item.buyPrice(0, CraftingSystem.slots * 4, 0, 0));
				CraftingSystem.slots += 1;
				UILoader.GetUIState<CrafterInventory>().Reset();

				Main.NewText($"Crafting slots increased to {CraftingSystem.slots} for {Main.worldName}!", Color.Gold);
			}
			else
			{
				Main.NewText("Insufficient coins", Color.Red);
			}
		}
	}
}
