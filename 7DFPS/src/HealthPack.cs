using System;
using System.Collections.Generic;

namespace DFPS {
	public class HealthPack : BillBoard {
		public HealthPack(float x, float z) : base(TextureTools.TextureHealthPack, x, z, .5f, 0f) {
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
							Server.Current.SendAddHealthPack(p.Client, X, Z);
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
							p.Health = 1f;
							if (MainGameState.IsServer) {
								Server.Current.SendUpdateHealthMessage(p.Client, p.Health);
							} else {
								Sounds.PickupCrate.Play();
							}
							Enabled = false;
							differentlyAbledCounter = 60f;

							if (MainGameState.IsServer) {
								foreach (Player p1 in Server.Current.PlayerList) {
									Server.Current.SendRemoveHealthPack(p1.Client, X, Z);
								}
							}
						}
					}
				}
			}
		}
	}
}

