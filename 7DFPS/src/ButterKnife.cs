using System;

namespace DFPS {
	// thanks for the name suggestion, TheMaster99!
	public class ButterKnife : Weapon {
		private const float damageDealt = 1f / 3f;
		public const float Range = 1f;

		public ButterKnife() : base(TextureTools.TextureButterKnife) {
		}

		public override Buffer2D<uint> HoldingInHandTexture {
			get { return TextureTools.TextureHoldingInHandKnife; }
		}

		public override bool LeftClick(MainGameState game, float time) {
			if (!game.Game.MouseClickPrevious) {
				Tuple<object, float, float> atCrossHair = game.CurrentPlayer.AtCrossHair as Tuple<object, float, float>;
				if (atCrossHair != null) {
					Enemy enemy = atCrossHair.Item1 as Enemy;
					Player plyr = atCrossHair.Item1 as Player;
					if (enemy != null) {
						float enemyDeltaX = game.CurrentPlayer.X - enemy.X;
						float enemyDeltaZ = game.CurrentPlayer.Z - enemy.Z;
						if (enemyDeltaX * enemyDeltaX + enemyDeltaZ * enemyDeltaZ < Range * Range) {
							enemy.Health -= damageDealt;
						}
					} else if (plyr != null) {
						float enemyDeltaX = game.CurrentPlayer.X - plyr.X;
						float enemyDeltaZ = game.CurrentPlayer.Z - plyr.Z;
						if (enemyDeltaX * enemyDeltaX + enemyDeltaZ * enemyDeltaZ < Range * Range) {
							plyr.Health -= damageDealt;
							Server.Current.SendUpdateHealthMessage(plyr.Client, plyr.Health);
						}
					}
				}
				if (!MainGameState.IsServer) {
					Sounds.Squeak.Play();
				}
			}
			return false;
		}
	}
}

