using System;

using Pencil.Gaming.Audio;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public abstract class Gun : Weapon {
		public Gun(Texture2D texture) : base(texture) {
			Ammo = DefaultAmmo;
			MagazinesLeft = MagazineSize - 1;
		}

		public abstract float FlashTime { get; }
		public abstract float Damage { get; }
		public abstract bool SemiAutomatic { get; }
		public abstract int RoundsPerSecond { get; }
		public abstract bool OpensDoors { get; }
		public abstract Sound Shot { get; }
		public abstract int DefaultAmmo { get; }
		public abstract Texture2D AmmoTexture { get; }
		public abstract Texture2D Scope { get; }
		public abstract int MagazineSize { get; }
		public abstract float ReloadTime { get; }

		public int MagazinesLeft { get; set; }
		public bool IsReloading { get; private set; }

		public int Ammo { get; set; }

		private float roundsCounter = 0f;
		private float reloadCounter = 0f;

		public override void Update(float time) {
			if (IsReloading) {
				reloadCounter -= time;
				if (reloadCounter <= 0f) {
					IsReloading = false;
					Ammo = DefaultAmmo;
					--MagazinesLeft;
				}
			}
		}

		public void Shoot(MainGameState game) {
			if (Ammo > 0) {
				--Ammo;
				game.Flash = FlashTime;
				if (!MainGameState.IsServer) {
					Shot.Play();
				}
				Tuple<object, float, float> crossHairWall = game.CurrentPlayer.AtCrossHair as Tuple<object, float, float>;
				if (crossHairWall != null) {
					Wall wall = crossHairWall.Item1 as Wall;
					Enemy e = crossHairWall.Item1 as Enemy;
					Player p = crossHairWall.Item1 as Player;
					if (wall != null) {
						Door door = wall.Owner as Door;
						if (OpensDoors && door != null) {
							door.Open();
						}
						int s = (int)(crossHairWall.Item2 * wall.DecalTexture.Width);
						int t = (int)(crossHairWall.Item3 * wall.DecalTexture.Width);
						wall.DecalTexture.Set(s, t, MathUtils3D.GetColor(0f, 0f, 0f, .25f));
					} else if (e != null) {
						e.Health -= Damage;
					} else if (p != null) {
						p.Health -= Damage / 5f;
						Server.Current.SendUpdateHealthMessage(p.Client, p.Health);
					}
				}
			}
		}

		public override bool LeftClick(MainGameState game, float time) {
			if (!IsReloading && Ammo == 0 && MagazinesLeft > 0 && !game.Game.MouseClickPrevious) {
				if (!MainGameState.IsServer) {
					Sounds.Reload1.Play();
				}
				IsReloading = true;
				reloadCounter = ReloadTime;
			}

			if (SemiAutomatic) {
				roundsCounter += time;
				if (roundsCounter > 1f / RoundsPerSecond) {
					Shoot(game);
					roundsCounter = 0f;
				}
			} else if (!game.Game.MouseClickPrevious) {
				Shoot(game);
			}
			return false;
		}
	}
}

