using System;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public abstract class Projectile : BillBoard {
		public bool ShouldBeRemoved { get; protected set; }

		public Projectile(Texture2D texture, float x, float z, float scale, float altitude) : base(texture, x, z, scale, altitude) {
		}

		public abstract void Update(MainGameState game, float time);

		private float verticalSpeed = 0f;
		public void UpdateGravity(float time) {
			if (Altitude <= 0) {
				Altitude = 0f;
			} else {
				verticalSpeed += time * 9.81f;
				Altitude -= verticalSpeed / 100f;
			}
		}
	}
}

