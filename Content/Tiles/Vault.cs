using DragonVault.Content.GUI.Vault;
using DragonVault.Core.Loaders.UILoading;
using DragonVault.Core.Systems;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace DragonVault.Content.Tiles
{
	internal class Vault : ModTile
	{
		public override void SetStaticDefaults()
		{
			QuickSetFurniture(this, 4, 4, DustID.Gold, SoundID.Tink, Color.Gold);
		}

		public override bool RightClick(int i, int j)
		{
			VaultBrowser state = UILoader.GetUIState<VaultBrowser>();
			state.visible = true;
			state.canWithdraw = true;
			state.fromTile = true;

			if (!state.initialized)
			{
				state.Refresh();
				state.initialized = true;
			}

			// Temporary
			RecipeBrowser rb = UILoader.GetUIState<RecipeBrowser>();
			rb.visible = true;
			rb.Refresh();
			if (!rb.initialized)
			{
				rb.Refresh();
				rb.initialized = true;
			}

			return true;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			frame = 0;

			if ((StorageSystem.stoneFlags & Items.Dragonstones.Stones.Rose) > 0)
				frame = 1;

			if ((StorageSystem.stoneFlags & Items.Dragonstones.Stones.Midnight) > 0)
				frame = 2;
		}

		#region quick setter
		public static void QuickSetFurniture(ModTile tile, int width, int height, int dustType, SoundStyle? hitSound, bool tallBottom, Color mapColor, bool solidTop = false, bool solid = false, string mapName = "", AnchorData bottomAnchor = default, AnchorData topAnchor = default, int[] anchorTiles = null, bool faceDirection = false, bool wallAnchor = false, Point16 Origin = default)
		{
			Main.tileLavaDeath[tile.Type] = false;
			Main.tileFrameImportant[tile.Type] = true;
			Main.tileSolidTop[tile.Type] = solidTop;
			Main.tileSolid[tile.Type] = solid;

			TileObjectData.newTile.Width = width;
			TileObjectData.newTile.Height = height;

			TileObjectData.newTile.CoordinateHeights = new int[height];

			for (int k = 0; k < height; k++)
			{
				TileObjectData.newTile.CoordinateHeights[k] = 16;
			}

			if (tallBottom) //this breaks for some tiles: the two leads are multitiles and tiles with random styles
				TileObjectData.newTile.CoordinateHeights[height - 1] = 18;

			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = Origin == default ? new Point16(width / 2, height - 1) : Origin;

			if (bottomAnchor != default)
				TileObjectData.newTile.AnchorBottom = bottomAnchor;
			/*else
                TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);*/

			if (topAnchor != default)
				TileObjectData.newTile.AnchorTop = topAnchor;

			if (anchorTiles != null)
				TileObjectData.newTile.AnchorAlternateTiles = anchorTiles;

			if (wallAnchor)
				TileObjectData.newTile.AnchorWall = true;

			if (faceDirection)
			{
				TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
				TileObjectData.newTile.StyleHorizontal = true;
				TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
				TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
				TileObjectData.addAlternate(1);
			}

			TileObjectData.addTile(tile.Type);

			LocalizedText name = tile.CreateMapEntryName();
			tile.AddMapEntry(mapColor, name);
			tile.DustType = dustType;
			tile.HitSound = hitSound;
		}

		public static void QuickSetFurniture(ModTile tile, int width, int height, int dustType, SoundStyle hitSound, Color mapColor, int bottomHeight = 16, bool solidTop = false, bool solid = false, string mapName = "", AnchorData bottomAnchor = default, AnchorData topAnchor = default, int[] anchorTiles = null)
		{
			Main.tileLavaDeath[tile.Type] = false;
			Main.tileFrameImportant[tile.Type] = true;
			Main.tileSolidTop[tile.Type] = solidTop;
			Main.tileSolid[tile.Type] = solid;

			TileObjectData.newTile.Width = width;
			TileObjectData.newTile.Height = height;

			TileObjectData.newTile.CoordinateHeights = new int[height];

			for (int k = 0; k < height; k++)
			{
				TileObjectData.newTile.CoordinateHeights[k] = 16;
			}

			TileObjectData.newTile.CoordinateHeights[height - 1] = bottomHeight;

			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = new Point16(width / 2, height / 2);

			if (bottomAnchor != default)
				TileObjectData.newTile.AnchorBottom = bottomAnchor;

			if (topAnchor != default)
				TileObjectData.newTile.AnchorTop = topAnchor;

			if (anchorTiles != null)
				TileObjectData.newTile.AnchorAlternateTiles = anchorTiles;

			TileObjectData.addTile(tile.Type);

			LocalizedText name = tile.CreateMapEntryName();
			tile.AddMapEntry(mapColor, name);
			tile.DustType = dustType;
			tile.HitSound = hitSound;
		}
		#endregion
	}

	internal class VaultItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.createTile = ModContent.TileType<Vault>();
			Item.consumable = true;
		}

		public override void AddRecipes()
		{
			Recipe.Create(Item.type)
				.AddIngredient(ItemID.GoldBar, 10)
				.AddTile(TileID.WorkBenches)
				.Register();

			Recipe.Create(Item.type)
				.AddIngredient(ItemID.PlatinumBar, 10)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}

