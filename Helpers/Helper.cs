namespace DragonVault.Helpers
{
	internal class Helper
	{
		public static bool CanStack(Item one, Item two)
		{
			bool mod1 = one.ModItem?.CanStack(two) ?? true;
			bool mod2 = two.ModItem?.CanStack(one) ?? true;
			bool prefix = one.prefix == two.prefix;

			return mod1 && mod2 && prefix;
		}
	}
}
