using DragonVault.Content.GUI.Vault;
using DragonVault.Core.Loaders.UILoading;
using DragonVault.Core.Systems;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace DragonVault.Content.GUI
{
	internal class InventoryButton : SmartUIState
	{
		public UIImageButton vaultButton = new(ModContent.Request<Texture2D>("DragonVault/Content/Tiles/VaultItem", ReLogic.Content.AssetRequestMode.ImmediateLoad));

		public override bool Visible => Main.playerInventory;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			vaultButton.Left.Set(574, 0);
			vaultButton.Top.Set(242, 0);
			vaultButton.Width.Set(32, 0);
			vaultButton.Height.Set(32, 0);
			vaultButton.OnLeftClick += VaultClick;
			Append(vaultButton);
		}

		private void VaultClick(UIMouseEvent evt, UIElement listeningElement)
		{
			if ((StorageSystem.stoneFlags & Items.Dragonstones.Stones.Cerulean) != 0)
			{
				VaultBrowser state = UILoader.GetUIState<VaultBrowser>();
				state.visible = true;
				state.canWithdraw = StorageSystem.stoneFlags.HasFlag(Items.Dragonstones.Stones.Midnight);
				state.fromTile = false;

				if (!state.initialized)
				{
					state.Refresh();
					state.initialized = true;
				}
			}
			else
			{
				Main.NewText("Requires Cerulean Dragonstone", new Color(10, 200, 200));
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (vaultButton.IsMouseHovering)
			{
				Tooltip.SetName("Remote Vault Access");
				Tooltip.SetTooltip("Requires Cerulean Dragonstone");
			}

			base.Draw(spriteBatch);
		}
	}
}
