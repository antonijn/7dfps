using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using System.Diagnostics;

using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class MainServer {
		public static void Main(string[] args) {
			string line;
			int result;
			do {
				Console.WriteLine("Which port do you want your server to run on?");
				line = Console.ReadLine();
			} while (!int.TryParse(line, out result));
			Server server = new Server(result);
			Stopwatch sw = new Stopwatch();
			sw.Reset();
			float updateOtherPlayersTimeCounter = .2f;
			while (true) {
				float elapsedSeconds = sw.ElapsedTicks / (float)Stopwatch.Frequency;
				sw.Reset();
				sw.Start();

				lock (Server.leftClickItemsMutex) {

					for (int i = 0; i < server.ToPerformLeftClickOn.Count; ++i) {
						// update left click on items
						Tuple<InventoryItem, Player> item = server.ToPerformLeftClickOn [i];
						lock (Server.mouseMutex) {
							// set currentclick and previousclick values based on player-specific values
							server.World.Game.MouseClickCurrent = server.MouseStates [item.Item2];
							server.World.Game.MouseClickPrevious = server.PreviousMouseStates [item.Item2];
						}
						server.World.CurrentPlayer = item.Item2;
						if (item.Item1.LeftClick(server.World, elapsedSeconds)) {
							int slot = Array.IndexOf(item.Item2.Inventory, item.Item1);
							lock (Server.playerMutex) {
								server.SendSetInventoryItemMessage(item.Item2.Client, slot, null);
								item.Item2.Inventory [slot] = null;
								server.ToPerformLeftClickOn.RemoveAt(i--);
								if (item.Item1 is Grenade) {
									lock (Server.playerMutex) {
										foreach (Player plyr in server.PlayerList) {
											server.SendThrowGrenadeMessage(plyr.Client, item.Item2.X, item.Item2.Z, -item.Item2.SinWorldRotation, item.Item2.CosWorldRotation);
										}
									}
									lock (Server.worldMutex) {
										server.World.Projectiles.Add(new Grenade.GenadeProjectile(item.Item2.X, item.Item2.Z, new Vector2(-item.Item2.SinWorldRotation, item.Item2.CosWorldRotation)));
									}
								}
							}
							continue;
						}

						// update ammo if it's a gun
						Gun gun = item.Item1 as Gun;
						if (gun != null) {
							if (!server.World.Game.MouseClickPrevious) {
								if (gun.IsReloading) {
									server.SendReloadConfirmedMessage(item.Item2.Client);
								} else if (gun.Ammo != -1) {
									int slot = Array.IndexOf(item.Item2.Inventory, item.Item1);
									server.SendSetInventoryItemMessage(item.Item2.Client, slot, item.Item1);
									if (item.Item1 is DefaultPistol) {
										foreach (Player client in server.PlayerList) {
											float diffX = item.Item2.X - client.X;
											float diffZ = item.Item2.Z - client.Z;
											if (diffX * diffX + diffZ * diffZ < MainGameState.WallsDisappearAt * MainGameState.WallsDisappearAt) {
												server.SendShot1Message(client.Client);
											}
										}
									} else if (item.Item1 is Rifle) {
										foreach (Player client in server.PlayerList) {
											float diffX = item.Item2.X - client.X;
											float diffZ = item.Item2.Z - client.Z;
											if (diffX * diffX + diffZ * diffZ < MainGameState.WallsDisappearAt * MainGameState.WallsDisappearAt) {
												server.SendShot2Message(client.Client);
											}
										}
									} else if (item.Item1 is SemiAuto) {
										foreach (Player client in server.PlayerList) {
											float diffX = item.Item2.X - client.X;
											float diffZ = item.Item2.Z - client.Z;
											if (diffX * diffX + diffZ * diffZ < MainGameState.WallsDisappearAt * MainGameState.WallsDisappearAt) {
												server.SendShot3Message(client.Client);
											}
										}
									}
								}
							}
						}
					}

					// prevMouseState = currentMouseState
					lock (Server.mouseMutex) {
						server.PreviousMouseStates.Clear();
						foreach (var v in server.MouseStates) {
							server.PreviousMouseStates [v.Key] = v.Value;
						}
					}
				}
				lock (Server.playerMutex) {
					foreach (Player p in server.PlayerList) {
						server.World.CurrentPlayer = p;

						for (int j = 0; j < p.Inventory.Length; ++j) {
							InventoryItem i = p.Inventory [j];

							int prevMagazines = -1;
							Gun gPrevious = i as Gun;
							if (gPrevious != null) {
								prevMagazines = gPrevious.MagazinesLeft;
							}
							if (i != null) {
								i.Update(elapsedSeconds);
							}

							Gun g = i as Gun;
							if (g != null) {
								if (g.MagazinesLeft != prevMagazines) {
									server.SendSetInventoryItemMessage(p.Client, j, i);
								}
							}
						}
					}
				}

				lock (Server.worldMutex) {
					server.World.Update(elapsedSeconds);
				}
//
//				lock (Server.worldMutex) {
//					List<AmmoCrate> previousCrates = new List<AmmoCrate>(server.World.Crates.Where(x => x.Enabled));
//					List<HealthPack> previousHPS = new List<HealthPack>(server.World.HealthPacks.Where(x => x.Enabled));
//					IEnumerable<AmmoCrate> diffCrates = previousCrates.Except(server.World.Crates.Where(x => x.Enabled));
//					IEnumerable<HealthPack> diffHPS = previousHPS.Except(server.World.HealthPacks.Where(x => x.Enabled));
//
//					foreach (Player p in server.PlayerList) {
//						foreach (AmmoCrate crate in diffCrates) {
//							if (previousCrates.Contains(crate)) {
//								server.SendRemoveAmmoCrates(p.Client, crate.X, crate.Z);
//							} else {
//								server.SendAddAmmoCrates(p.Client, crate.X, crate.Z);
//							}
//						}
//
//						foreach (HealthPack crate in diffHPS) {
//							if (previousHPS.Contains(crate)) {
//								server.SendRemoveHealthPack(p.Client, crate.X, crate.Z);
//							} else {
//								server.SendAddHealthPack(p.Client, crate.X, crate.Z);
//							}
//						}
//					}
//				}

				// FIXME: please use a different solution for this later
				Thread.Sleep(1);

				sw.Stop();
			}
		}
	}
}

