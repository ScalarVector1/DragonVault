using DragonVault.Core.Systems.ThemeSystem;
using DragonVault.Helpers;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace DragonVault.Content.GUI
{
	internal class StyledScrollbar : Terraria.ModLoader.UI.Elements.FixedUIScrollbar
	{
		public static MethodInfo handleMethod = typeof(UIScrollbar).GetMethod("GetHandleRectangle", BindingFlags.NonPublic | BindingFlags.Instance);

		public StyledScrollbar(UserInterface userInterface) : base(userInterface) { }

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (CanScroll)
			{
				var back = GetDimensions().ToRectangle();
				back.Inflate(2, 2);

				GUIHelper.DrawBox(spriteBatch, back, ThemeHandler.ButtonColor);

				var handle = (Rectangle)handleMethod.Invoke(this, null);
				handle.Width = (int)(GetDimensions().Width - 4);
				handle.Offset(2, 0);

				GUIHelper.DrawBox(spriteBatch, handle, ThemeHandler.ButtonColor);
			}
		}
	}
}