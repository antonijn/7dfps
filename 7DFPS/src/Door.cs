using System;
using System.Linq;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public class Door : Block {
		private float timeToOpen = -1f;
		private const float totalTimeToOpen = .5f;
		private bool opening = false;
		public bool IsOpen {
			get { return opening && timeToOpen == 0f; }
		}
		public Texture2D Upper { get; set; }
		public Texture2D Lower { get; set; }

		private Wall wallLower1;
		private Wall wallLower2;
		private Wall wallLower3;
		private Wall wallLower4;

		public Door(int x, int z, Texture2D full, Texture2D upper, Texture2D lower) : base(x, z, full) {
			Upper = upper;
			Lower = lower;
		}

		public override bool CollidesWithVector(Vector2 playerPos) {
			if (!IsOpen) {
				return base.CollidesWithVector(playerPos);
			}
			return false;
		}

		public void Update(float time) {
			if (opening) {
				if (timeToOpen > 0f) {
					timeToOpen -= time;
				}
				if (timeToOpen < 0f) {
					timeToOpen = 0f;
				}
			
				float altitudeChange = 1f - (timeToOpen / totalTimeToOpen / 2f);
				Wall1.Altitude = altitudeChange - .5f;
				Wall2.Altitude = altitudeChange - .5f;
				Wall3.Altitude = altitudeChange - .5f;
				Wall4.Altitude = altitudeChange - .5f;

				wallLower1.Altitude = 1f - altitudeChange - .5f;
				wallLower2.Altitude = 1f - altitudeChange - .5f;
				wallLower3.Altitude = 1f - altitudeChange - .5f;
				wallLower4.Altitude = 1f - altitudeChange - .5f;
			}
		}

		public void Open() {
			opening = true;
			timeToOpen = totalTimeToOpen;

			Wall1.Texture = Upper;
			Wall2.Texture = Upper;
			Wall3.Texture = Upper;
			Wall4.Texture = Upper;

			wallLower1 = new Wall(X, Z, 0f, Lower);
			wallLower2 = new Wall(X, Z + 1, MathHelper.TauOver4, Lower);
			wallLower3 = new Wall(X + 1, Z, 0f, Lower);
			wallLower4 = new Wall(X, Z, MathHelper.TauOver4, Lower);
			wallLower1.Owner = this;
			wallLower2.Owner = this;
			wallLower3.Owner = this;
			wallLower4.Owner = this;
		}

		public override void Draw(MainGameState game) {
			if (!IsOpen) {
				base.Draw(game);
				if (timeToOpen != -1f) {
					wallLower1.Draw(game);
					wallLower2.Draw(game);
					wallLower3.Draw(game);
					wallLower4.Draw(game);
				}
			}
		}
	}
}
