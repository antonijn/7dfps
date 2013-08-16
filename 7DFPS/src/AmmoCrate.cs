using System;
using System.Collections.Generic;

namespace DFPS {
	public class AmmoCrate : BillBoard {
		public AmmoCrate(float x, float z) : base(TextureTools.TextureAmmoCrate, x, z, .5f, 0f) {
			Enabled = true;
		}

		private float differentlyAbledCounter = 0f;
		public bool Enabled { get; private set; }

		public void Update(MainGameState game, float time) {
			if (!Enabled) {
				differentlyAbledCounter -= time;
				if (differentlyAbledCounter <= 0f) {
					Enabled = true;

					if (MainGameState.IsServer) {
						foreach (Player p in Server.Current.PlayerList) {
							Server.Current.SendAddAmmoCrates(p.Client, X, Z);
						}
					}
				}
			} else {
				lock (Server.playerMutex) {
					List<Player> toScrollThrough = new List<Player> { game.CurrentPlayer };
					if (MainGameState.IsServer) {
						toScrollThrough = (Server.Current.PlayerList);
					}
					foreach (Player p in toScrollThrough) {
						float playerDeltaX = p.X - X;
						float playerDeltaZ = p.Z - Z;
						if (playerDeltaX * playerDeltaX + playerDeltaZ * playerDeltaZ < 1f) {
							for (int i = 0; i < Player.InventorySize; ++i) {
								Gun g = p.GetInventoryItem(i) as Gun;
								if (g != null) {
									g.Ammo = g.DefaultAmmo;
									g.MagazinesLeft = g.MagazineSize - 1;
									if (MainGameState.IsServer) {
										Server.Current.SendSetInventoryItemMessage(p.Client, i, g);
									}
								}
							}

							if (!MainGameState.IsServer) {
								Sounds.PickupCrate.Play();
							}
							Enabled = false;
							differentlyAbledCounter = 60f;
							if (MainGameState.IsServer) {
								foreach (Player p1 in Server.Current.PlayerList) {
									Server.Current.SendRemoveAmmoCrates(p1.Client, X, Z);
								}
							}
						}
					}
				}
			}
		}
	}
}

