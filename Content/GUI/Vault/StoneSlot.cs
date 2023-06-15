using DragonVault.Content.Items.Dragonstones;
using DragonVault.Core.Systems;
using DragonVault.Core.Systems.ThemeSystem;
using DragonVault.Helpers;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace DragonVault.Content.GUI.Vault
{
	internal class StoneSlot : UIElement
	{
		public Stones id;

		public int slotTimer;

		public bool Active => (StorageSystem.stoneFlags & id) > 0;

		public StoneSlot(Stones id) : base()
		{
			this.id = id;
			Width.Set(54, 0);
			Height.Set(54, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (slotTimer > 0)
				slotTimer--;

			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonVault/Assets/DragonstoneOutline").Value;
			spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.Black * 0.4f, 0, tex.Size() / 2f, 1, 0, 0);

			if (Active)
			{
				Color color = Dragonstone.samples[id].color;

				Texture2D tex2 = ModContent.Request<Texture2D>("DragonVault/Assets/Dragonstone").Value;
				spriteBatch.Draw(tex2, GetDimensions().Center(), null, color, 0, tex2.Size() / 2f, 1, 0, 0);

				Color color2 = Color.White;
				color2.A = 0;

				Texture2D tex3 = ModContent.Request<Texture2D>("DragonVault/Assets/DragonstoneOver").Value;
				spriteBatch.Draw(tex3, GetDimensions().Center(), null, color2, 0, tex3.Size() / 2f, 1, 0, 0);

				if (slotTimer > 0)
				{
					Texture2D tex4 = ModContent.Request<Texture2D>("DragonVault/Assets/Flare").Value;

					float alpha = slotTimer > 110 ? 1f - (slotTimer - 110) / 10f : slotTimer / 110f;
					Color color3 = color * alpha;
					color3.A = 0;

					spriteBatch.Draw(tex4, GetDimensions().Center(), null, color3, slotTimer / 60f * 3.14f, tex4.Size() / 2f, 0.07f, 0, 0);

					spriteBatch.Draw(tex4, GetDimensions().Center(), null, color2 * alpha * 0.5f, slotTimer / 60f * 3.14f, tex4.Size() / 2f, 0.07f, 0, 0);
				}
			}

			if (IsMouseHovering)
			{
				Tooltip.SetName(Dragonstone.samples[id].Name);
				Tooltip.SetTooltip(Dragonstone.samples[id].ModItem.Tooltip.Value.Replace("\n", "\n------\n") + "\n------\n" + (Active ? "Active" : "Slot the item here to activate this power for " + Main.worldName));
			}
		}

		public override void LeftClick(UIMouseEvent evt)
		{
			if (Main.mouseItem.ModItem is Dragonstone stone)
			{
				if (stone.id == id)
				{
					stone.OnSlot();
					StorageSystem.stoneFlags |= id;
					Main.mouseItem.TurnToAir();
					slotTimer = 120;

					SoundEngine.PlaySound(SoundID.DD2_WitherBeastDeath);
					SoundEngine.PlaySound(SoundID.DD2_WitherBeastAuraPulse);
				}
			}
		}
	}
}
