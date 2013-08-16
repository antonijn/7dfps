using System;
using Pencil.Gaming.MathUtils;

namespace DFPS {
	public abstract class DroppedWeapon : Projectile {
		private float XDir;
		private float ZDir;

		public DroppedWeapon(float x, float z, float xDir, float zDir) : this(x, z) {
			XDir = xDir;
			ZDir = zDir;
		}
		public DroppedWeapon(float x, float z) : base(TextureTools.TextureCeiling1, x, z, .5f, 0f) {
			Weapon dropped = GetDropped();
			if (dropped != null) {
				Texture = GetDropped().Texture;
			}
		}

		public abstract Weapon GetDropped();

		public override void Update(MainGameState game, float time) {
			float playerPosRelX = game.CurrentPlayer.X - X;
			float playerPosRelZ = game.CurrentPlayer.Z - Z;
			if (Altitude == 0f && playerPosRelX * playerPosRelX + playerPosRelZ * playerPosRelZ < .5f * .5f) {
				Weapon drop = GetDropped();
				if (drop.MaxStack <= 1) {
					for (int i = 0; i < Player.InventorySize; ++i) {
						if (game.CurrentPlayer.GetInventoryItem(i) == null) {
							game.CurrentPlayer.SetInventoryItem(i, drop);
							Sounds.PickupWeapon.Play();
							ShouldBeRemoved = true;
							return;
						}
					}
				} else {
					for (int i = 0; i < Player.InventorySize; ++i) {
						InventoryStack iStack = game.CurrentPlayer.GetInventoryItem(i) as InventoryStack;
						// thanks novynn for pointing out the silly != / == mistake!
						if (iStack != null && iStack.GetItemType() == drop.GetType() && iStack.Amount != iStack.MaxValue) {
							++iStack.Amount;
							Sounds.PickupWeapon.Play();
							ShouldBeRemoved = true;
							return;
						}
					}
					for (int i = 0; i < Player.InventorySize; ++i) {
						if (game.CurrentPlayer.GetInventoryItem(i) == null) {
							game.CurrentPlayer.SetInventoryItem(i, new DroppedItemInventoryStack(drop.GetType(), drop.MaxStack));
							Sounds.PickupWeapon.Play();
							ShouldBeRemoved = true;
							return;
						}
					}
				}
			}

			UpdateGravity(time);
			if (Altitude != 0f) {
				Vector2 movement = new Vector2(XDir * time * 3f, ZDir * time * 3f);
				Vector2 finalPosition = new Vector2(X, Z) + movement;
				Block b = game.BlockGrid[(int)finalPosition.X, (int)finalPosition.Y];
				if (b == null || !b.CollidesWithVector(finalPosition)) {
					X = finalPosition.X;
					Z = finalPosition.Y;
				}
			}
		}
	}

	public class DroppedWeapon<T> : DroppedWeapon where T : Weapon, new() {
		public DroppedWeapon(float x, float z) : base(x, z) {
			if (typeof(T) == typeof(Grenade)) {
				Scale = .25f;
			} else if (typeof(T) == typeof(ButterKnife)) {
				Scale = .25f;
			}
		}

		public override Weapon GetDropped() {
			return new T();
		}
	}

	public class InventoryDroppedWeapon : DroppedWeapon {
		private Weapon dropped;

		public InventoryDroppedWeapon(Weapon drop, float x, float z, float xDir, float zDir) : base(x, z, xDir, zDir) {
			if (drop is Grenade) {
				Scale = .25f;
			} else if (drop is ButterKnife) {
				Scale = .25f;
			}
			dropped = drop;
			Altitude = .5f;
			Texture = dropped.Texture;
		}

		public override Weapon GetDropped() {
			return dropped;
		}
	}
}

