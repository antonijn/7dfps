using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Pencil.Gaming;
using Pencil.Gaming.Audio;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;

using Color = System.UInt32;
using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public class MainGameState : GameState {
		public int CurrentLevel { get; private set; }
		public bool IsMultiplayer { get; private set; }
		public static bool IsServer { get; set; }

		public Vector2i Spawn { get; private set; }

		public Player CurrentPlayer { get; set; }

		public float Flash { get; set; }
		
		public Block[,] BlockGrid;
		public int WorldWidth;
		public int WorldHeight;
		public readonly List<Block> Blocks = new List<Block>();
		public readonly List<Enemy> Enemies = new List<Enemy>();
		public List<Player> OtherPlayers = new List<Player>();
		public List<Projectile> Projectiles = new List<Projectile>();
		public List<DroppedWeapon> DroppedWeapons = new List<DroppedWeapon>();
		public List<AmmoCrate> Crates = new List<AmmoCrate>();
		public List<HealthPack> HealthPacks = new List<HealthPack>();
		private List<Vector2i> SpecialLocations = new List<Vector2i>();
		public List<Tuple<DateTime, string>> Chat = new List<Tuple<DateTime, string>>();
		public static readonly object ChatMutex = new object();
		private List<char> onScreen = new List<char>();
		public bool WritingChat = false;
		private bool steppedOnLevel1Special = false;
		private bool playerPrevOnSpecial = false;

		public MultiplayerClient Client { get; private set; }
		private bool changeToRequest = false;

		public MainGameState(MainClass game, string ip, string uname, int port) : base(game) {
			IsMultiplayer = (ip != null);
			if (IsMultiplayer) {
				CurrentPlayer = new Player(this, 32, 32, uname);
				try {
					Client = new MultiplayerClient(this, ip, port);
					Client.SendConnectMessage(uname);
				} catch (Exception) {
					changeToRequest = true;
				}
			} else {
				LoadLevel(1);
			}
			if (!IsServer) {
				Glfw.Disable(GlfwEnableCap.MouseCursor);
			}
		}
		public MainGameState(MainClass game) : this(game, null, null, -1) {
		}

		public void LoadLevel(int level) {
			CheckPoint.ClearCheckPoints();
			CurrentLevel = -level;
			if (level > 0 && level < 4) {
				Blocks.Clear();
				Enemies.Clear();
				Crates.Clear();
				SpecialLocations.Clear();
				HealthPacks.Clear();
				Projectiles.Clear();
				DroppedWeapons.Clear();
				Texture2D tex = TextureTools.LoadTexture(IsServer, "level" + level + ".gif", new Rectanglei(0, 0, 64, 64));
				BlockGrid = new Block[tex.Width, tex.Height];
				WorldWidth = tex.Width;
				WorldHeight = tex.Height;
				List<Action> enemyAdditions = new List<Action>();
				for (int x = 0; x < tex.Width; ++x) {
					int xReal = tex.Width - x - 1;
					for (int z = 0; z < tex.Height; ++z) {
						Color c = tex.Get(x, z);
						switch (c) {
						case 0xFFFF0000:
							Blocks.Add(new Block(xReal, z, TextureTools.TextureWall1));
							BlockGrid [xReal, z] = Blocks.Last();
							break;
						case 0xFF0000FF:
							Blocks.Add(new Block(xReal, z, TextureTools.TextureWall2));
							BlockGrid [xReal, z] = Blocks.Last();
							break;
						case 0xFFFFFF00:
							if (!MainGameState.IsServer) {
								Blocks.Add(new Door(xReal, z, TextureTools.TextureDoor1, TextureTools.TextureDoor1Upper, TextureTools.TextureDoor1Lower));
								BlockGrid [xReal, z] = Blocks.Last();
							}
							break;
						case 0xFF333333:
							{
								int xRealCopy = xReal;
								int zCopy = z;
								enemyAdditions.Add(() => {
									for (int i = 0; i < 2; ++i) {
										while (true) {
											float offsetX = ((float)MainClass.Random.NextDouble() - .5f);
											float offsetZ = ((float)MainClass.Random.NextDouble() - .5f);
											Enemy e = new Enemy(xRealCopy + offsetX, zCopy + offsetZ);
											if (BlockGrid [(int)e.X, (int)e.Z] == null) {
												Enemies.Add(e);
												break;
											}
										}
									}
								});
							}
							break;
						case 0xFF666666:
							{
								int xRealCopy = xReal;
								int zCopy = z;
								enemyAdditions.Add(() => {
									for (int i = 0; i < 4; ++i) {
										while (true) {
											float offsetX = ((float)MainClass.Random.NextDouble() - .5f);
											float offsetZ = ((float)MainClass.Random.NextDouble() - .5f);
											Enemy e = new Enemy(xRealCopy + offsetX, zCopy + offsetZ);
											if (BlockGrid [(int)e.X, (int)e.Z] == null) {
												Enemies.Add(e);
												break;
											}
										}
									}
								});
							}
							break;
						case 0xFF999999:
							{
								int xRealCopy = xReal;
								int zCopy = z;
								enemyAdditions.Add(() => {
									for (int i = 0; i < 6; ++i) {
										while (true) {
											float offsetX = ((float)MainClass.Random.NextDouble() - .5f);
											float offsetZ = ((float)MainClass.Random.NextDouble() - .5f);
											Enemy e = new Enemy(xRealCopy + offsetX, zCopy + offsetZ);
											if (BlockGrid [(int)e.X, (int)e.Z] == null) {
												Enemies.Add(e);
												break;
											}
										}
									}
								});
							}
							break;
						case 0xFFCCCCCC:
							{
								int xRealCopy = xReal;
								int zCopy = z;
								enemyAdditions.Add(() => {
									for (int i = 0; i < 8; ++i) {
										while (true) {
											float offsetX = ((float)MainClass.Random.NextDouble() - .5f);
											float offsetZ = ((float)MainClass.Random.NextDouble() - .5f);
											Enemy e = new Enemy(xRealCopy + offsetX, zCopy + offsetZ);
											if (BlockGrid [(int)e.X, (int)e.Z] == null) {
												Enemies.Add(e);
												break;
											}
										}
									}
								});
							}
							break;
						case 0xFFFFFFFF:
							if (!IsServer) {
								if (CurrentPlayer == null) {
									CurrentPlayer = new Player(this, xReal, z, "LOCAL");
								} else {
									CurrentPlayer.Position = new Vector2(xReal, z);
								}
							}

							Spawn = new Vector2i(xReal, z);
							break;
						case 0xFF00FF00:
							Crates.Add(new AmmoCrate(xReal + .5f, z + .5f));
							break;
						case 0xFFFF00FF:
							HealthPacks.Add(new HealthPack(xReal + .5f, z + .5f));
							break;
						case 0xFF00FFFF:
							if (!MainGameState.IsServer) {
								Projectiles.Add(new CheckPoint(xReal + .5f, z + .5f));
							}
							break;
						case 0xFF808080:
							SpecialLocations.Add(new Vector2i(xReal, z));
							break;
						}
					}
				}
				foreach (Action a in enemyAdditions) {
					a();
				}
			} else {
				CurrentLevel = short.MinValue;
				Game.CurrentGameState = new PlotScreen(Game,
				    "So we meet at last.",
				                                       (sender, e) => {
					Game.CurrentGameState = this;
					Glfw.Disable(GlfwEnableCap.MouseCursor); }, 
				                                       (sender, e) => {
													Game.CurrentGameState = new PlotScreen(Game,
													                                       "I have a confession to make now.\n\n" +
													                                       "The guys in suits, those weren't the bad guys. Or at least, you wouldn't have thought they were the bad guys.\n" +
													                                       "You see, you just killed a whole lot of your own people. Yes, you did.\n\n" +
													                                       "You had the key of that first room you were in in your pocket, but you couldn't remember after some of my people drugged you.",
													                                       (sender1, e1) => {
														Game.CurrentGameState = this;
														Glfw.Disable(GlfwEnableCap.MouseCursor); }, 
													(sender1, e1) => {
																		Game.CurrentGameState = new PlotScreen(Game,
																		                                       "I'm the bad guy. Bye bye.",
																		                                       (sender2, e2) => {
																			Game.CurrentGameState = this;
																			Glfw.Disable(GlfwEnableCap.MouseCursor); }, 
																		(sender2, e2) => {
																			Game.CurrentGameState = new DeathState(this, false);
																		});
													});
				});
			}
		}

		private void PlayerStepOnSpecialLoc(int x, int z) {
			if (CurrentLevel == 1 && !steppedOnLevel1Special) {
				steppedOnLevel1Special = true;
				Game.CurrentGameState = new PlotScreen(Game,
				    "Good.\n\n" +
					"Let me summarize the situation for you. You're in a bad place, you need to get out of here. There are guards pretty much everywhere. They are not good, shoot them on sight.\n" +
					"Also, there are locations in here marked with a blue sphere with an arrow in the middle, these store space and time temporarily, so that if you happen to die, you can go back. If you grab enough of them, I might be able to warp you elsewhere.\n\n" +
					"Shit, on the left! Shoot the bastards!", 
				                                       (sender, e) => {
					Game.CurrentGameState = this;
					Glfw.Disable(GlfwEnableCap.MouseCursor); }, 
				                                       (sender, e) => {
					Game.CurrentGameState = this;
					Glfw.Disable(GlfwEnableCap.MouseCursor); });
			}
		}

		private static float MakePositive(float f) {
			if (f < 0) {
				return Math.Abs(1f - Math.Abs(f));
			}
			return f;
		}

		private void UpdateClient(float time) {
#if DEBUG
			if (!IsMultiplayer && !MainGameState.IsServer && Game.CurrentKS['Z'] && !Game.PreviousKS['Z']) {
				LoadLevel(CurrentLevel + 1);
			}
#endif

			if (IsMultiplayer && !MainGameState.IsServer) {
				lock (MultiplayerClient.PlayerMutex) {
					OtherPlayers.ForEach(x => { 
						if (x.PositionCounter <= 0f) {
							x.PositionCounter = .1f;
						}
						x.PreviousPositionCounter += time;
						x.InterpolationCounter += time;
						x.InterpolationCounter = Math.Min(x.InterpolationCounter, x.PositionCounter);
						x.Position = x.ActualPosition + (x.ActualPosition - x.PreviousPosition) * (x.InterpolationCounter / x.PositionCounter);
						//x.Position = x.ActualPosition;
					});
				}
			}

			if (WritingChat) {
				foreach (KeyValuePair<char, Buffer2D<uint>> kvp in TextureTools.Font) {
					char ch = kvp.Key;
					try {
						if (Game.CurrentKS [ch] && !Game.PreviousKS [ch]) {
							onScreen.Add(ch);
						}
					} catch (Exception) {
						// TODO: add period support
					}
				}

				if (Game.CurrentKS[Key.Enter] && !Game.PreviousKS[Key.Enter]) {
					if (IsMultiplayer) {
						Client.SendChatMessage(new string(onScreen.ToArray()));
					}
					onScreen.Clear();
					WritingChat = false;
				}

				if (Game.CurrentKS[Key.Backspace] && !Game.PreviousKS[Key.Backspace] && onScreen.Count > 0) {
					onScreen.RemoveAt(onScreen.Count - 1);
				}

				if (Game.CurrentKS[Key.Space] && !Game.PreviousKS[Key.Space]) {
					onScreen.Add(' ');
				}
			}

			if (Game.CurrentKS['T'] && !Game.PreviousKS['T']) {
				WritingChat = true;
			}

			if (Game.CurrentKS[Key.Escape] && !Game.PreviousKS[Key.Escape]) {
				Game.CurrentGameState = new AreYouSure(this);
			}

			if (changeToRequest) {
				Game.CurrentGameState = new ServerRequestState(Game);
				return;
			}

			if (!IsMultiplayer && CurrentLevel == -1) {
				Game.CurrentGameState = new PlotScreen(Game, 
				    "Hello agent, it's nice to see you awake again, or well, the nearest equivalent to seeing.\n" + 
					"Wait, you probably don't even remember who I am, do you? Well, I'm not going to bother explaining the situation, we're already running low on time.\n" +
					"What's important now is that you get out of this room. Blow open the doors with the grenade you have.\n" +
					"How did you get a grenade? You don't even want to know...\n" +
					"I'll contact you again once you're out of this room. Hurry!", 
				                                       (sender, e) => {
					Game.CurrentGameState = this;
					Glfw.Disable(GlfwEnableCap.MouseCursor); }, 
				                                       (sender, e) => {
					Game.CurrentGameState = this;
					Glfw.Disable(GlfwEnableCap.MouseCursor); });
				CurrentLevel = 1;
			}

			if (!IsMultiplayer && CurrentLevel == -2) {
				CurrentLevel = 2;
			}

			if (!IsMultiplayer && CurrentLevel == -3) {
				Game.CurrentGameState = new PlotScreen(Game, 
				                                       "Good, we've almost gathered enough energy to warp you to me. A couple of more blue spheres are required though. Keep going like this, you're doing very well!", 
				                                       (sender, e) => {
					Game.CurrentGameState = this;
					Glfw.Disable(GlfwEnableCap.MouseCursor); }, 
				(sender, e) => {
					Game.CurrentGameState = this;
					Glfw.Disable(GlfwEnableCap.MouseCursor); });
				CurrentLevel = 3;
			}

			if (!IsMultiplayer && CurrentLevel == -4) {
				Game.CurrentGameState = new PlotScreen(Game, 
				                                       "Good, we've almost gathered enough energy to warp you to me. A couple of more blue spheres are required though. Keep going like this, you're doing very well!", 
				                                       (sender, e) => {
					Game.CurrentGameState = this;
					Glfw.Disable(GlfwEnableCap.MouseCursor); }, 
				(sender, e) => {
					Game.CurrentGameState = this;
					Glfw.Disable(GlfwEnableCap.MouseCursor); });
			}

			if (!IsMultiplayer && Projectiles.Count(proj => proj is CheckPoint) == 0) {
				LoadLevel(CurrentLevel + 1);
			}

			if (Flash >= 0f) {
				Flash -= time;
			} 
			if (Flash < 0) {
				Flash = 0f;
			}

			CurrentPlayer.Update(time);

			lock (MultiplayerClient.WorldMutex) {
				foreach (Block b in Blocks) {
					Door d = b as Door;
					if (d != null) {
						d.Update(time);
					}
				}
			}

			for (int i = 0; i < Projectiles.Count; ++i) {
				Projectiles[i].Update(this, time);
			}
			Projectiles = Projectiles.Where(p => !p.ShouldBeRemoved).ToList();

			if (!IsMultiplayer) {
				int playerXInt = (int)CurrentPlayer.X;
				int playerZInt = (int)CurrentPlayer.Z;
				bool isOnSpecial = false;
				foreach (Vector2i special in SpecialLocations) {
					if (special.X == playerXInt && special.Y == playerZInt) {
						isOnSpecial = true;
						if (!playerPrevOnSpecial) {
							PlayerStepOnSpecialLoc(playerXInt, playerZInt);
						}
						break;
					}
				}
				playerPrevOnSpecial = isOnSpecial;

				foreach (AmmoCrate crate in Crates) {
					crate.Update(this, time);
				}

				foreach (HealthPack hp in HealthPacks) {
					hp.Update(this, time);
				}
			}

			for (int i = 0; i < DroppedWeapons.Count; ++i) {
				DroppedWeapons [i].Update(this, time);
				if (DroppedWeapons[i].ShouldBeRemoved) {
					DroppedWeapons.RemoveAt(i--);
				}
			}

			for (int i = 0; i < Enemies.Count; ++i) {
				Enemies [i].Update(this, time);
				if (Enemies[i].ShouldBeRemoved) {
					Enemies.RemoveAt(i--);
				}
			}
		}

		private void UpdateServer(float time) {
			foreach (HealthPack hp in HealthPacks) {
				hp.Update(this, time);
			}
			foreach (AmmoCrate ac in Crates) {
				ac.Update(this, time);
			}
			for (int i = 0; i < Projectiles.Count; ++i) {
				Projectiles[i].Update(this, time);
			}
			Projectiles = Projectiles.Where(p => !p.ShouldBeRemoved).ToList();
		}

		public override void Update(float time) {
			if (!IsServer) {
				UpdateClient(time);
			} else {
				UpdateServer(time);
			}
		}

		// TODO: figure out why this no worky wanty
		public const float WallsDisappearAt = 50f / MainClass.Scale;
		private void PostProcess() {
			float zBufferFlicker = ((float)MainClass.Random.NextDouble() - .5f) / 50f;
			Game.Screen.Fill((x, y) => {
				float r, g, b, a;
				MathUtils3D.GetFloatsFromColor(Game.Screen.Get(x, y), out r, out g, out b, out a);
				float zBuffer = 1f - Game.ZBuffer.Get(x, y);
				zBuffer *= WallsDisappearAt;
				zBuffer -= Flash;
				zBuffer += zBufferFlicker;
				if (zBuffer > 1f) {
					zBuffer = 1f;
				}
				zBuffer = 1f - zBuffer;

				return MathUtils3D.GetColor(r * zBuffer, g * zBuffer, b * zBuffer, a);
			});
		}

		public void DrawPatch(Vector2 corner0, Vector2 corner1, Vector2 corner2, Vector2 corner3, Func<float, float, Color> textureFunc, Func<float, float, float> zBufVal, object calledBy) {
			Vector2 topLeft;
			Vector2 topRight;
			Vector2 bottomLeft;
			Vector2 bottomRight;

			Vector2[] all = new [] { corner0, corner1, corner2, corner3 };
			IEnumerable<Vector2> sorted = all.OrderBy(x => x.X);
			Vector2[] left = sorted.Take(2).ToArray();
			Vector2[] right = sorted.Skip(2).Take(2).ToArray();

			if (left [0].Y < left [1].Y) {
				topLeft = left [0];
				bottomLeft = left [1];
			} else {
				bottomLeft = left [0];
				topLeft = left [1];
			}

			if (right [0].Y < right [1].Y) {
				topRight = right [0];
				bottomRight = right [1];
			} else {
				bottomRight = right [0];
				topRight = right [1];
			}

			float dyDxTop = (topRight.Y - topLeft.Y) / (topRight.X - topLeft.X);
			float dyDxBottom = (bottomRight.Y - bottomLeft.Y) / (bottomRight.X - bottomLeft.X);

			int startX = (int)Math.Round(topLeft.X);
			int endX = (int)Math.Round(topRight.X);
			for (int x = startX; x < endX; ++x) {
				if (x >= 0 && x < MainClass.ScreenWidth) {
					float xRelative = x - startX;
					float s = xRelative / (endX - startX);
					float yTop = topLeft.Y + xRelative * dyDxTop;
					float yBottom = bottomLeft.Y + xRelative * dyDxBottom;

					float yDifference = yBottom - yTop;

					int yTopInt = (int)Math.Round(yTop);
					int yBottomInt = (int)Math.Round(yBottom);

					float z = zBufVal(yTopInt, yDifference);
					if (z > .1f) {
						for (int y = yTopInt; y < yBottomInt; ++y) {
							if (y >= 0 && y < MainClass.ScreenHeight) {
								float zPrevious = Game.ZBuffer.Get(x, y);
								if (z > zPrevious) {
									float t = (float)(y - yTopInt) / (yBottomInt - yTopInt);
									Color color = textureFunc(s, t);
									float a, r, g, b;
									MathUtils3D.GetFloatsFromColor(color, out r, out g, out b, out a);
									if (a != 0) {
										Game.Screen.Set(x, y, color);
										Game.ZBuffer.Set(x, y, z);
										if (x == MainClass.ScreenWidth / 2 && y == MainClass.ScreenHeight / 2) {
											CurrentPlayer.AtCrossHair = new Tuple<object, float, float>(calledBy, s, t);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public override void Draw() {
			CurrentPlayer.AtCrossHair = null;
			// clear screen
			Game.Screen.Fill((x, y) => {
				float xFromCenter = x - MainClass.ScreenWidth / 2f;
				float yFromCenter = y - MainClass.ScreenHeight / 2f;

				float xModelSpace = xFromCenter / yFromCenter * 0.5f;
				float zModelSpace = Math.Abs(1f / yFromCenter) * (MainClass.ScreenHeight / 2f);
				if (zModelSpace >= 0f && zModelSpace < WallsDisappearAt) {
					float xModelSpaceBackup = xModelSpace;
					if (yFromCenter < 0) {
						// ceiling
						xModelSpace = CurrentPlayer.CosWorldRotation * xModelSpace + CurrentPlayer.SinWorldRotation * zModelSpace;
						zModelSpace = CurrentPlayer.SinWorldRotation * xModelSpaceBackup - CurrentPlayer.CosWorldRotation * zModelSpace;
						xModelSpace -= CurrentPlayer.X;
						zModelSpace -= CurrentPlayer.Z;
						float s = MakePositive(zModelSpace % 1f);
						float t = MakePositive(xModelSpace % 1f);
						return TextureTools.TextureCeiling2.Get((int)(s * 16), (int)(t * 16));
					}
					{
						// floor
						xModelSpace = CurrentPlayer.CosWorldRotation * xModelSpace - CurrentPlayer.SinWorldRotation * zModelSpace;
						zModelSpace = CurrentPlayer.SinWorldRotation * xModelSpaceBackup + CurrentPlayer.CosWorldRotation * zModelSpace;
						xModelSpace += CurrentPlayer.X;
						zModelSpace += CurrentPlayer.Z;
						float s = MakePositive(zModelSpace % 1f);
						float t = MakePositive(xModelSpace % 1f);
						return TextureTools.TextureFloor1.Get((int)(s * 16), (int)(t * 16));
					}
				}
				return 0xFF000000;
			});

			// clear zbuffer
			Game.ZBuffer.Fill((x, y) => {
				float yFromCenter = y - MainClass.ScreenHeight / 2f;
				float z = 1f - (1f / Math.Abs(yFromCenter));
				if (z < 0f) {
					z = 0f;
				}

				return z;
			});

			lock (MultiplayerClient.WorldMutex) {
				foreach (Block b in Blocks) {
					b.Draw(this);
				}
			}

			foreach (Enemy e in Enemies) {
				e.Draw(this);
			}

			foreach (Projectile p in Projectiles) {
				p.Draw(this);
			}

			foreach (DroppedWeapon dw in DroppedWeapons) {
				dw.Draw(this);
			}

			lock (MultiplayerClient.PlayerMutex) {
				foreach (Player p in OtherPlayers) {
					p.Draw(this);
				}
			}

			foreach (AmmoCrate crate in Crates.Where(c => c.Enabled)) {
				crate.Draw(this);
			}

			foreach (HealthPack hp in HealthPacks.Where(h => h.Enabled)) {
				hp.Draw(this);
			}

			PostProcess();

			lock (ChatMutex) {
				int chatLine = Game.Screen.Height - 50;
				foreach (Tuple<DateTime, string> chat in Chat) {
					TextureTools.BlitString(TextureTools.Font, 4, 5, new Vector2i(10, chatLine), chat.Item2, Game.Screen);
					chatLine -= 6;
				}
				Chat = Chat.Where(ch => DateTime.Now - ch.Item1 < new TimeSpan(0, 0, 10)).Take(10).ToList();
			}

			TextureTools.BlitString(TextureTools.Font, 4, 5, new Vector2i(10, Game.Screen.Height - 50 + 6), new string(onScreen.ToArray()), Game.Screen);

			if (!IsMultiplayer) {
				TextureTools.BlitString(TextureTools.Font, 4, 5, new Vector2i(1), "CHECKPOINTS LEFT: " + Projectiles.Count(x => x is CheckPoint), Game.Screen);
			}

			CurrentPlayer.Draw();
		}

		public override void Dispose() {
			if (Client != null) {
				Client.Dispose();
			}
		}
	}
}
