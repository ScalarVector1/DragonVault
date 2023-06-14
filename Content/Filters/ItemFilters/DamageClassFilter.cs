using DragonVault.Content.GUI;
using DragonVault.Content.GUI.Vault;

namespace DragonVault.Content.Filters.ItemFilters
{
	internal class DamageClassFilter : Filter
	{
		public DamageClass damageClass;

		public DamageClassFilter(DamageClass damageClass, string texture) : base(texture, "", n => FilterByDamageClass(n, damageClass))
		{
			this.damageClass = damageClass;
		}

		public override string Name => damageClass.DisplayName.Value.Trim();

		public override string Description => "";

		public static bool FilterByDamageClass(BrowserButton button, DamageClass damageClass)
		{
			if (button is ItemButton)
			{
				var ib = button as ItemButton;

				if (ib.entry.item.damage > 0 && ib.entry.item.DamageType.CountsAsClass(damageClass))
					return false;
			}

			return true;
		}
	}
}