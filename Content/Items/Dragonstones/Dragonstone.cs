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

		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemNoGravity[Type] = true;
		}

		public override void SetDefaults()
		{
			Item.color = color;
			Item.width = 38;
			Item.height = 38;
			Item.rare = ItemRarityID.Quest;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			Color color2 = Color.White;
			color2.A = 0;

			Texture2D tex = ModContent.Request<Texture2D>(Texture + "Over").Value;
			spriteBatch.Draw(tex, position, frame, color2, 0, origin, scale, 0, 0);
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			Color color2 = Color.White.MultiplyRGB(lightColor);
			color2.A = 0;

			Texture2D tex = ModContent.Request<Texture2D>(Texture + "Over").Value;
			spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, color2, rotation, tex.Size() / 2f, scale, 0, 0);

			Texture2D tex4 = ModContent.Request<Texture2D>("DragonVault/Assets/Flare").Value;

			float alpha = 1f;
			Color color3 = color * alpha;
			color3.A = 0;

			spriteBatch.Draw(tex4, Item.Center - Main.screenPosition, null, color3 * 0.5f, Main.GameUpdateCount * 0.02f, tex4.Size() / 2f, 0.06f, 0, 0);

			spriteBatch.Draw(tex4, Item.Center - Main.screenPosition, null, color2 * alpha * 0.45f, Main.GameUpdateCount * 0.02f, tex4.Size() / 2f, 0.06f, 0, 0);
		}

		public override void PostUpdate()
		{
			Lighting.AddLight(Item.Center, Color.Lerp(Item.color, Color.White, 0.5f).ToVector3());
			var d = Dust.NewDustPerfect(Item.Center, DustID.FireworksRGB, Main.rand.NextVector2Circular(4, 4), 0, Color.Lerp(Item.color, Color.White, 0.5f), Main.rand.NextFloat());
			d.noGravity = true;
		}

		/// <summary>
		/// What should happen when the stone is slotted in
		/// </summary>
		public abstract void OnSlot();

		/// <summary>
		/// Resets this stones effects on world unload
		/// </summary>
		public abstract void Reset();
	}

	internal class RoseStone : Dragonstone
	{
		public RoseStone() : base(Stones.Rose, Color.Red) { }

		public override void OnSlot()
		{
			StorageSystem.extraCapacity += 50000;
		}

		public override void Reset()
		{
			StorageSystem.extraCapacity -= 50000;
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

		public override void Reset()
		{
			ModContent.GetModTile(ModContent.TileType<Vault>()).AdjTiles = new int[] { };
		}
	}

	internal class RadiantStone : Dragonstone
	{
		public RadiantStone() : base(Stones.Radiant, new Color(200, 200, 10)) { }

		public override void OnSlot()
		{
			// enable remote UI
		}

		public override void Reset()
		{

		}
	}

	internal class VerdantStone : Dragonstone
	{
		public VerdantStone() : base(Stones.Verdant, new Color(50, 220, 65)) { }

		public override void OnSlot()
		{
			StorageSystem.extraCapacity += 100000;
		}

		public override void Reset()
		{
			StorageSystem.extraCapacity -= 100000;
		}
	}

	internal class CeruleanStone : Dragonstone
	{
		public CeruleanStone() : base(Stones.Cerulean, new Color(10, 200, 200)) { }

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
				TileID.Bottles,
				TileID.DemonAltar,
				TileID.MythrilAnvil,
				TileID.AdamantiteForge,
				TileID.LunarCraftingStation
			};
		}

		public override void Reset()
		{
			ModContent.GetModTile(ModContent.TileType<Vault>()).AdjTiles = new int[] { };
		}
	}

	internal class AzureStone : Dragonstone
	{
		public AzureStone() : base(Stones.Azure, new Color(10, 40, 200)) { }

		public override void OnSlot()
		{
			// enable full remote UI
		}

		public override void Reset()
		{

		}
	}

	internal class MidnightStone : Dragonstone
	{
		public MidnightStone() : base(Stones.Midnight, new Color(100, 10, 220)) { }

		public override void OnSlot()
		{
			StorageSystem.extraCapacity += 1000000000;
		}

		public override void Reset()
		{
			StorageSystem.extraCapacity = 0;
		}
	}

	internal class PureStone : Dragonstone
	{
		public PureStone() : base(Stones.Pure, new Color(200, 200, 200)) { }

		public override void OnSlot()
		{
			// enable vault spawning pet projectile
		}

		public override void Reset()
		{

		}
	}
}
