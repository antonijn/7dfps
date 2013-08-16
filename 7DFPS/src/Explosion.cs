using System;

namespace DFPS {
	public class Explosion : Projectile {
		public Explosion(float x, float z) : base(TextureTools.ExplosionAnimation[0], x, z, 1f, 0f) {
			AlwaysBright = true;
		}

		public override void Update(MainGameState game, float time) {
			ShouldBeRemoved = UpdateAnimation(TextureTools.ExplosionAnimation, 10f, time);
		}
	}
}

