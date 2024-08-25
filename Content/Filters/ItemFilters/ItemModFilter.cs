﻿using DragonVault.Content.GUI;
using DragonVault.Content.GUI.Vault;

namespace DragonVault.Content.Filters.ItemFilters
{
	internal class ItemModFilter : Filter
	{
		public Mod mod;

		public ItemModFilter(Mod mod) : base(null, "", n => FilterByMod(n, mod))
		{
			this.mod = mod;
		}

		public override string Name => mod.DisplayName;

		public override string Description => "";

		public static bool FilterByMod(BrowserButton button, Mod mod)
		{
			if (button is ItemButton)
			{
				var ib = button as ItemButton;

				if (ib.entry.item.ModItem != null && ib.entry.item.ModItem.Mod == mod)
					return false;
			}

			return true;
		}

		public override void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			Texture2D tex = null;

			string path = $"{mod.Name}/icon_small";

			if (ModContent.HasAsset(path))
				tex = ModContent.Request<Texture2D>(path).Value;

			if (tex != null)
			{
				int widest = tex.Width > tex.Height ? tex.Width : tex.Height;
				spriteBatch.Draw(tex, target.Center.ToVector2(), null, Color.White, 0, tex.Size() / 2f, target.Width / (float)widest, 0, 0);
			}
			else
			{
				Utils.DrawBorderString(spriteBatch, mod.DisplayName[..2], target.Center.ToVector2(), Color.White, 1, 0.5f, 0.4f);
			}
		}
	}
}