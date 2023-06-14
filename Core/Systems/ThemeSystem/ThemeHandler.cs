using DragonVault.Content.Themes.BoxProviders;
using System;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace DragonVault.Core.Systems.ThemeSystem
{
	internal class ThemeHandler : ModSystem
	{
		public readonly static Dictionary<string, ThemeBoxProvider> allBoxProviders = new();

		private readonly static Dictionary<Type, ThemeBoxProvider> allBoxProvidersByType = new();

		public static ThemeBoxProvider currentBoxProvider = new SimpleBoxes();
		public static ThemeColorProvider currentColorProvider = new();

		/// <summary>
		/// The color that buttons should be drawn in.
		/// </summary>
		public static Color ButtonColor => currentColorProvider.buttonColor;

		/// <summary>
		/// The color that background boxes should be drawn in.
		/// </summary>
		public static Color BackgroundColor => currentColorProvider.backgroundColor;

		/// <summary>
		/// Sets the current box provider based on a string key. The key should be the name of the ThemeBoxProvider's type.
		/// </summary>
		/// <param name="key">The type name of the ThemeBoxProvider to set</param>
		private static void SetBoxProvider(string key)
		{
			currentBoxProvider = allBoxProviders[key];
		}

		/// <summary>
		/// Sets the current box provider based on a type.
		/// </summary>
		/// <typeparam name="T">The type of the box provider to set</typeparam>
		public static void SetBoxProvider<T>() where T : ThemeBoxProvider
		{
			currentBoxProvider = allBoxProvidersByType[typeof(T)];
		}

		/// <summary>
		/// Sets the current box provider to a given box provider instance.
		/// </summary>
		/// <param name="provider">The provider to use</param>
		public static void SetBoxProvider(ThemeBoxProvider provider)
		{
			currentBoxProvider = provider;
		}

		public static ThemeBoxProvider GetBoxProvider<T>() where T : ThemeBoxProvider
		{
			return allBoxProvidersByType[typeof(T)];
		}

		public override void Load()
		{
			foreach (Type t in GetType().Assembly.GetTypes())
			{
				if (!t.IsAbstract && t.IsSubclassOf(typeof(ThemeBoxProvider)))
				{
					allBoxProviders.Add(t.FullName, (ThemeBoxProvider)Activator.CreateInstance(t));
					allBoxProvidersByType.Add(t, (ThemeBoxProvider)Activator.CreateInstance(t));
				}
			}
		}

		public static void SaveData(TagCompound tag)
		{
			var themeTag = new TagCompound
			{
				["BoxTheme"] = currentBoxProvider.GetType().FullName,

				["backColor"] = currentColorProvider.backgroundColor,
				["buttonColor"] = currentColorProvider.buttonColor
			};

			tag["Theme"] = themeTag;
		}

		public static void LoadData(TagCompound tag)
		{
			if (tag.TryGet("Theme", out TagCompound themeTag))
			{
				SetBoxProvider(themeTag.GetString("BoxTheme"));

				currentColorProvider.backgroundColor = themeTag.Get<Color>("backColor");
				currentColorProvider.buttonColor = themeTag.Get<Color>("buttonColor");
			}
			else //defaults
			{
				SetBoxProvider<SimpleBoxes>();
			}
		}
	}
}