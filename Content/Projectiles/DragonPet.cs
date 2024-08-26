using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace DragonVault.Content.Projectiles
{
	internal class DragonPet : ModProjectile
	{
		public Vector2 home;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 130;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void AI()
		{
			Projectile.timeLeft = 100;

			if (home == default)
				home = Projectile.Center;

			Projectile.ai[0]++;

			float prog = Projectile.ai[0] / 140f;

			float X = MathF.Sin(prog * 3.14f);
			float Y = MathF.Sin(prog * 2f * 3.14f);

			Vector2 next = home + new Vector2(X * 64f, Y * 24f);

			Projectile.rotation = Projectile.Center.DirectionTo(next).ToRotation();
			Projectile.Center = next;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var tex = Assets.Wyvern.Value;

			for(int k = 0; k < 12; k++)
			{
				var source = new Rectangle(264 / 12 * k, 0, 22, 48);
				Vector2 pos = Projectile.oldPos[k * 10];
				float rot = Projectile.oldRot[k * 10] + 3.14f;
				Main.spriteBatch.Draw(tex, pos - Main.screenPosition, source, new Color(Lighting.GetSubLight(pos)), rot, new Vector2(6, 24), 1, 0, 0);
			}

			var headTex = Assets.WyvernHead.Value;
			Main.spriteBatch.Draw(headTex, Projectile.Center - Main.screenPosition, null, new Color(Lighting.GetSubLight(Projectile.Center)), Projectile.rotation + 3.14f, new Vector2(36, 14), 1, 0, 0);

			return false;
		}
	}
}
