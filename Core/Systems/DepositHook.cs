﻿using DragonVault.Content.GUI.Vault;
using DragonVault.Core.Loaders.UILoading;
using DragonVault.Core.Networking;
using Terraria.UI;

namespace DragonVault.Core.Systems
{
	internal class DepositHook : ModSystem
	{
		public override void Load()
		{
			On_ItemSlot.LeftClick_ItemArray_int_int += Test;
		}

		private void Test(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
		{
			if (UILoader.GetUIState<VaultBrowser>().visible && Main.mouseLeft && Main.keyState.PressingShift())
			{
				bool added = StorageSystem.TryAddItem(inv[slot], out ItemEntry newEntry);

				if (added && newEntry != null)
					VaultBrowser.Rebuild();

				VaultNet.SendDeposit(newEntry.simStack, newEntry.item);

				return;
			}

			orig(inv, context, slot);
		}
	}
}