using DragonVault.Content.GUI;
using DragonVault.Content.GUI.Vault;
using DragonVault.Helpers;
using ReLogic.Content;

namespace DragonVault.Content.Filters.ItemFilters
{
	internal class DamageClassFilter : Filter
	{
		public DamageClass damageClass;

		public DamageClassFilter(DamageClass damageClass, Asset<Texture2D> texture) : base(texture, "", n => FilterByDamageClass(n, damageClass))
		{
			this.damageClass = damageClass;
		}

		public override string Name => damageClass.DisplayName.Value.Trim();

		public override string Description => GetText("Filters.DamageClass.Description", damageClass.DisplayName.Value.Trim());

		public static string GetText(string key, params object[] args)
		{
			return LocalizationHelper.GetText($"Tools.ItemSpawner.{key}", args);
		}

		public static bool FilterByDamageClass(BrowserButton button, DamageClass damageClass)
		{
			if (button is ItemButton)
			{
				var ib = button as ItemButton;

				if (ib.entry.item.damage > 0 && ib.entry.item.DamageType.CountsAsClass(damageClass))
					return false;
			}

			if (button is RecipeButton)
			{
				var ib = button as RecipeButton;

				if (ib.result.damage > 0 && ib.result.DamageType.CountsAsClass(damageClass))
					return false;
			}

			return true;
		}
	}
}