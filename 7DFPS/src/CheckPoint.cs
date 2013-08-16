using System;
using System.Collections.Generic;

namespace DFPS {
	public class CheckPoint : Projectile {
		private static CheckPoint last = null;

		private List<Enemy> enemies = new List<Enemy>();
		private List<HealthPack> healthPack = new List<HealthPack>();
		private List<AmmoCrate> crates = new List<AmmoCrate>();
		private List<Block> blocks = new List<Block>();
		private Block[,] blockGrid;
		private InventoryItem[] playerInventory = new InventoryItem[5];
		private float xPlayer;
		private float zPlayer;
		private float healthPlayer;
		private List<DroppedWeapon> dropped = new List<DroppedWeapon>();

		public CheckPoint(float x, float z) : base(TextureTools.TextureCheckPoint, x, z, .75f, (1f - .75f) / 2f) {
		}

		public static void ClearCheckPoints() {
			last = null;
		}

		public static void Restore(MainGameState world) {
			if (last == null) {
				world.CurrentPlayer = null;
				world.LoadLevel(world.CurrentLevel);
			} else {
				world.Enemies.Clear();
				foreach (Enemy e in last.enemies) {
					world.Enemies.Add(new Enemy(e.X, e.Z));
				}

				world.HealthPacks.Clear();
				// TODO: take healthpack timer into account
				foreach (HealthPack hp in last.healthPack) {
					world.HealthPacks.Add(new HealthPack(hp.X, hp.Z));
				}

				world.Crates.Clear();
				// TODO: idem
				foreach (AmmoCrate ac in last.crates) {
					world.Crates.Add(new AmmoCrate(ac.X, ac.Z));
				}

				world.Blocks.Clear();
				world.BlockGrid = new Block[world.WorldWidth, world.WorldHeight];
				for (int x = 0; x < world.WorldWidth; ++x) {
					for (int z = 0; z < world.WorldWidth; ++z) {
						Block oldBlock = last.blockGrid [x, z];
						Door oldDoor = oldBlock as Door;
						if (oldBlock != null) {
							Block newBlock = null;
							if (oldDoor != null) {
								// TODO: take full door texture into account
								Door newDoor = new Door(oldDoor.X, oldDoor.Z, TextureTools.TextureDoor1, oldDoor.Upper, oldDoor.Lower);
								if (oldDoor.IsOpen) {
									newDoor.Open();
								}
								newBlock = newDoor;
							} else {
								newBlock = new Block(oldBlock.X, oldBlock.Z, oldBlock.Wall1.Texture);
							}
							world.BlockGrid [x, z] = newBlock;
							world.Blocks.Add(newBlock);
						}
					}
				}

				world.DroppedWeapons.Clear();
				foreach (DroppedWeapon weapon in last.dropped) {
					Weapon droppedWeapon = weapon.GetDropped();
					world.DroppedWeapons.Add(new InventoryDroppedWeapon(droppedWeapon, weapon.X, weapon.Z, 0f, 0f));
				}

				for (int i = 0; i < world.CurrentPlayer.Inventory.Length; ++i) {
					InventoryItem current = last.playerInventory [i];
					if (current != null) {
						InventoryStack stack = current as InventoryStack;
						if (stack != null) {
							DroppedItemInventoryStack newStack = new DroppedItemInventoryStack(stack.GetItemType(), stack.MaxValue);
							newStack.Amount = stack.Amount;
							world.CurrentPlayer.Inventory [i] = newStack;
						} else {
							world.CurrentPlayer.Inventory [i] = (InventoryItem)Activator.CreateInstance(current.GetType());
							Gun g = current as Gun;
							if (g != null) {
								((Gun)world.CurrentPlayer.Inventory [i]).Ammo = g.Ammo;
								((Gun)world.CurrentPlayer.Inventory [i]).MagazinesLeft = g.MagazinesLeft;
							}
						}
					}
				}

				world.CurrentPlayer.X = last.xPlayer;
				world.CurrentPlayer.Z = last.zPlayer;
				world.CurrentPlayer.Health = last.healthPlayer;
			}
		}

		private void CopyWorld(MainGameState world) {
			foreach (Enemy e in world.Enemies) {
				enemies.Add(new Enemy(e.X, e.Z));
			}

			// TODO: take healthpack timer into account
			foreach (HealthPack hp in world.HealthPacks) {
				healthPack.Add(new HealthPack(hp.X, hp.Z));
			}

			// TODO: idem
			foreach (AmmoCrate ac in world.Crates) {
				crates.Add(new AmmoCrate(ac.X, ac.Z));
			}

			blockGrid = new Block[world.WorldWidth, world.WorldHeight];
			for (int x = 0; x < world.WorldWidth; ++x) {
				for (int z = 0; z < world.WorldWidth; ++z) {
					Block oldBlock = world.BlockGrid[x, z];
					Door oldDoor = oldBlock as Door;
					if (oldBlock != null) {
						Block newBlock = null;
						if (oldDoor != null) {
							// TODO: take full door texture into account
							Door newDoor = new Door(oldDoor.X, oldDoor.Z, TextureTools.TextureDoor1, oldDoor.Upper, oldDoor.Lower);
							if (oldDoor.IsOpen) {
								newDoor.Open();
							}
							newBlock = newDoor;
						} else {
							newBlock = new Block(oldBlock.X, oldBlock.Z, oldBlock.Wall1.Texture);
						}
						blockGrid [x, z] = newBlock;
						blocks.Add(newBlock);
					}
				}
			}

			foreach (DroppedWeapon weapon in world.DroppedWeapons) {
				Weapon droppedWeapon = weapon.GetDropped();
				dropped.Add(new InventoryDroppedWeapon(droppedWeapon, weapon.X, weapon.Z, 0f, 0f));
			}

			for (int i = 0; i < world.CurrentPlayer.Inventory.Length; ++i) {
				InventoryItem current = world.CurrentPlayer.Inventory[i];
				if (current != null) {
					InventoryStack stack = current as InventoryStack;
					if (stack != null) {
						DroppedItemInventoryStack newStack = new DroppedItemInventoryStack(stack.GetItemType(), stack.MaxValue);
						newStack.Amount = stack.Amount;
						playerInventory [i] = newStack;
					} else {
						playerInventory [i] = (InventoryItem)Activator.CreateInstance(current.GetType());
						Gun g = current as Gun;
						if (g != null) {
							((Gun)playerInventory [i]).Ammo = g.Ammo;
							((Gun)playerInventory [i]).MagazinesLeft = g.MagazinesLeft;
						}
					}
				}
			}

			xPlayer = world.CurrentPlayer.X;
			zPlayer = world.CurrentPlayer.Z;
			healthPlayer = world.CurrentPlayer.Health;
		}

		public override void Update(MainGameState game, float time) {
			float xPlayerDiff = game.CurrentPlayer.X - X;
			float zPlayerDiff = game.CurrentPlayer.Z - Z;
			if (xPlayerDiff * xPlayerDiff + zPlayerDiff * zPlayerDiff < 1f) {
				last = this;
				ShouldBeRemoved = true;
				Sounds.PickupCrate.Play();
				CopyWorld(game);
			}
		}
	}
}

