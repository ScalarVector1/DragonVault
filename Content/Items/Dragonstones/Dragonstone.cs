using DragonVault.Content.Tiles;
using DragonVault.Core.Systems;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace DragonVault.Content.Items.Dragonstones
{
	[Flags]
	public enum Stones
	{
		Rose = 1 << 0,
		Citrine = 1 << 1,
		Radiant = 1 << 2,
		Verdant = 1 << 3,
		Cerulean = 1 << 4,
		Azure = 1 << 5,
		Midnight = 1 << 6,
		Pure = 1 << 7
	}

	internal abstract class Dragonstone : ModItem
	{
		public Stones id;
		public Color color;

		public override string Texture => "DragonVault/Assets/Dragonstone";

		public static Dictionary<Stones, Item> samples = new()
		{
			[Stones.Rose] = ContentSamples.ItemsByType[ModContent.ItemType<RoseStone>()],
			[Stones.Citrine] = ContentSamples.ItemsByType[ModContent.ItemType<CitrineStone>()],
			[Stones.Radiant] = ContentSamples.ItemsByType[ModContent.ItemType<RadiantStone>()],
			[Stones.Verdant] = ContentSamples.ItemsByType[ModContent.ItemType<VerdantStone>()],
			[Stones.Cerulean] = ContentSamples.ItemsByType[ModContent.ItemType<CeruleanStone>()],
			[Stones.Azure] = ContentSamples.ItemsByType[ModContent.ItemType<AzureStone>()],
			[Stones.Midnight] = ContentSamples.ItemsByType[ModContent.ItemType<MidnightStone>()],
			[Stones.Pure] = ContentSamples.ItemsByType[ModContent.ItemType<PureStone>()],
		};

		public Dragonstone() { }

		public Dragonstone(Stones id, Color color)
		{
			this.id = id;
			this.color = color;
		}

		public override void SetDefaults()
		{
			Item.color = color;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Color color2 = Color.White;
			color2.A = 0;

			Texture2D tex = ModContent.Request<Texture2D>(Texture + "Over").Value;
			spriteBatch.Draw(tex, position, frame, color2, 0, origin, scale, 0, 0);
		}

		/// <summary>
		/// What should happen when the stone is slotted in
		/// </summary>
		public abstract void OnSlot();
	}

	internal class RoseStone : Dragonstone
	{
		public RoseStone() : base(Stones.Rose, Color.Red) { }

		public override void OnSlot()
		{
			StorageSystem.extraCapacity += 50000;
		}
	}

	internal class CitrineStone : Dragonstone
	{
		public CitrineStone() : base(Stones.Citrine, new Color(255, 100, 0)) { }

		public override void OnSlot()
		{
			ModContent.GetModTile(ModContent.TileType<Vault>()).AdjTiles = new int[]
			{
				TileID.WorkBenches,
				TileID.Furnaces,
				TileID.Anvils,
				TileID.Sawmill,
				TileID.Loom,
				TileID.HeavyWorkBench,
				TileID.GlassKiln,
				TileID.Bottles
			};
		}
	}

	internal class RadiantStone : Dragonstone
	{
		public RadiantStone() : base(Stones.Radiant, new Color(200, 200, 10)) { }

		public override void OnSlot()
		{

		}
	}

	internal class VerdantStone : Dragonstone
	{
		public VerdantStone() : base(Stones.Verdant, new Color(50, 220, 65)) { }

		public override void OnSlot()
		{

		}
	}

	internal class CeruleanStone : Dragonstone
	{
		public CeruleanStone() : base(Stones.Cerulean, new Color(10, 200, 200)) { }

		public override void OnSlot()
		{

		}
	}

	internal class AzureStone : Dragonstone
	{
		public AzureStone() : base(Stones.Azure, new Color(10, 40, 200)) { }

		public override void OnSlot()
		{

		}
	}

	internal class MidnightStone : Dragonstone
	{
		public MidnightStone() : base(Stones.Midnight, new Color(100, 10, 220)) { }

		public override void OnSlot()
		{

		}
	}

	internal class PureStone : Dragonstone
	{
		public PureStone() : base(Stones.Pure, new Color(200, 200, 200)) { }

		public override void OnSlot()
		{

		}
	}
}
