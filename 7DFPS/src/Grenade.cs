using System;
using System.Linq;

using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class Grenade : Weapon {
		public Grenade() : base(TextureTools.TextureGrenadeInv) {
		}

		public override bool LeftClick(MainGameState game, float time) {
			game.Projectiles.Add(new Grenade.GenadeProjectile(game.CurrentPlayer.X, game.CurrentPlayer.Z, new Vector2(-game.CurrentPlayer.SinWorldRotation, game.CurrentPlayer.CosWorldRotation)));
			return true;
		}

		public override int MaxStack {
			get { return 3; }
		}

		public override Buffer2D<uint> HoldingInHandTexture {
			get { return TextureTools.TextureHoldingInHandGrenade; }
		}

		public class GenadeProjectile : Projectile {
			public Vector2 Direction { get; private set; }
			private float fuse = 3f;

			public GenadeProjectile(float x, float z, Vector2 dir) : base(TextureTools.TextureGrenadeInv, x, z, .25f, .4f) {
				Direction = dir;
			}

			public override void Update(MainGameState game, float time) {
				fuse -= time;
				if (fuse <= 0f) {
					if (!MainGameState.IsServer && !game.IsMultiplayer) {
						Vector2 movement = new Vector2(Direction.X * .5f, Direction.Y * .5f);
						Vector2 finalPosition = new Vector2(X, Z) + movement;
						Door d = game.BlockGrid [(int)finalPosition.X, (int)finalPosition.Y] as Door;
						if (d != null) {
							d.Open();
						}
					}
					if (MainGameState.IsServer) {
						Server.Current.PlayerList.ForEach(x => {
							float relativeX = -x.X + X;
							float relativeZ = -x.Z + Z;
							float distSqr = relativeX * relativeX + relativeZ * relativeZ;
							x.Health -= Math.Max(0, 1.5f - distSqr);
							if (distSqr > 0f) {
								Server.Current.SendUpdateHealthMessage(x.Client, x.Health);
							}
						});
					}
					game.Enemies.ForEach(x => {
						float relativeX = -x.X + X;
						float relativeZ = -x.Z + Z;
						float distSqr = relativeX * relativeX + relativeZ * relativeZ;
						x.Health -= Math.Max(0, 1.5f - distSqr);
					});
					game.Projectiles.Add(new Explosion(X, Z));
					if (!MainGameState.IsServer) {
						Sounds.Grenade.Play();
					}
					ShouldBeRemoved = true;

					return;
				}

				UpdateGravity(time);
				if (Altitude != 0f) {
					Vector2 movement = new Vector2(Direction.X * time * 6f, Direction.Y * time * 6f);
					Vector2 finalPosition = new Vector2(X, Z) + movement;
					Block b = game.BlockGrid[(int)finalPosition.X, (int)finalPosition.Y];
					if (b == null || !b.CollidesWithVector(finalPosition)) {
						X = finalPosition.X;
						Z = finalPosition.Y;
					}
				}
			}
		}
	}
}

