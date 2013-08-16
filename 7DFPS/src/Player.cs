using System;
using System.Net.Sockets;
using System.Collections.Generic;

using Pencil.Gaming;
using Pencil.Gaming.MathUtils;

using Texture2D = DFPS.Buffer2D<uint>;

namespace DFPS {
	public class Player : BillBoard {
		private float worldRotation = 0f;

		public TcpClient Client { get; set; }

		public string Name { get; set; }

		public Vector2 Position {
			get { return new Vector2(X, Z); }
			set { X = value.X; Z = value.Y; }
		}

		public float WorldRotation {
			get { return worldRotation; }
			set {
				worldRotation = value;
				CosWorldRotation = (float)Math.Cos(value);
				SinWorldRotation = (float)Math.Sin(value);
			}
		}
		public float CosWorldRotation { get; set; }
		public float SinWorldRotation { get; set; }

		public float Health { get; set; }

		public const int InventorySize = 5;
		public readonly InventoryItem[] Inventory = new InventoryItem[InventorySize];
		public InventoryItem GetInventoryItem(int item) {
			return Inventory[item];
		}
		public void SetInventoryItem(int item, InventoryItem iItem) {
			Inventory [item] = iItem;
		}
		public int CurrentInventorySlot { get; set; }
		public object AtCrossHair { get; set; }
		private object prevAtCrossHair { get; set; }
		
		public const float MovementSpeed = 2f;
		private MainGameState Game;
		public Vector2 PreviousPosition;
		public Vector2 ActualPosition;
		public float InterpolationCounter = 0f;
		public float PreviousPositionCounter = 0f;
		public float PositionCounter = 0f;

		private readonly static List<Action> toPerformWhileNoPP = new List<Action>();

		public Player(MainGameState game, float x, float z, string name) : base(TextureTools.TextureEnemy, x, z, .65f, 0f) {
			Name = name;
			Health = 1f;
			Game = game;
			SetInventoryItem(0, new DefaultPistol());
			SetInventoryItem(1, new ButterKnife());
			SetInventoryItem(2, new Grenade());
		}

		private float sendRotationTimeCounter = .2f;
		private int prevMouseWheel = MainGameState.IsServer ? 0 : Glfw.GetMouseWheel();

		public void Update(float time) {
			if (!MainGameState.IsServer) {
				if (Game.IsMultiplayer) {
					sendRotationTimeCounter -= time;
					if (sendRotationTimeCounter <= 0f) {
						Game.Client.SendUpdateWayFacingMessage(SinWorldRotation, CosWorldRotation);
						Game.Client.SendMoveMessage(X, Z);
						sendRotationTimeCounter = .1f;
					}
				}

				int mouseWheel = Glfw.GetMouseWheel();
				int scrollWheelDifference = mouseWheel - prevMouseWheel;
				CurrentInventorySlot -= scrollWheelDifference;
				CurrentInventorySlot = Math.Min(InventorySize - 1, CurrentInventorySlot);
				CurrentInventorySlot = Math.Max(0, CurrentInventorySlot);
				prevMouseWheel = mouseWheel;
			}

			if (!Game.IsMultiplayer && Health <= 0f) {
//				Game.Game.CurrentGameState = new DeathState(Game);
			}

			InventoryItem iItem = Inventory [CurrentInventorySlot];
			if (Game.Game.MouseClickCurrent) {
				if (iItem != null) {
					if (Game.IsMultiplayer && !MainGameState.IsServer && !Game.Game.MouseClickPrevious) {
						Game.Client.SendLeftClickMessage(CurrentInventorySlot);
					} else {
						if (iItem.LeftClick(Game, time)) {
							Inventory [CurrentInventorySlot] = null;
						}
					}
				}
			} else if (!MainGameState.IsServer && Game.IsMultiplayer && Game.Game.MouseClickPrevious) {
				Game.Client.SendLeftUnclickMessage();
			}

			if (!Game.IsMultiplayer) {
				Weapon w = iItem as Weapon;
				if (w != null && Game.Game.CurrentKS ['Q'] && !Game.Game.PreviousKS ['Q']) {
					Inventory [CurrentInventorySlot] = null;
					Game.DroppedWeapons.Add(new InventoryDroppedWeapon(w, X, Z, -SinWorldRotation, CosWorldRotation));
				}
				InventoryStack iStack = iItem as InventoryStack;
				if (iStack != null && Game.Game.CurrentKS ['Q'] && !Game.Game.PreviousKS ['Q']) {
					--iStack.Amount;
					if (iStack.Amount == 0) {
						Inventory [CurrentInventorySlot] = null;
					}
					Game.DroppedWeapons.Add(new InventoryDroppedWeapon(iStack.GetItem() as Weapon, X, Z, -SinWorldRotation, CosWorldRotation));
				}
			}

			if (!MainGameState.IsServer) {
				if (!Game.WritingChat) {
					if (Game.Game.CurrentKS ['1']) {
						CurrentInventorySlot = 0;
					} else if (Game.Game.CurrentKS ['2']) {
						CurrentInventorySlot = 1;
					} else if (Game.Game.CurrentKS ['3']) {
						CurrentInventorySlot = 2;
					} else if (Game.Game.CurrentKS ['4']) {
						CurrentInventorySlot = 3;
					} else if (Game.Game.CurrentKS ['5']) {
						CurrentInventorySlot = 4;
					}
				}
			}

			int x, y;
			Glfw.GetMousePos(out x, out y);
			WorldRotation = -x / 100f;

			if (!Game.WritingChat) {
				Vector2 movement = Vector2.Zero;

				if (Game.Game.CurrentKS ['W']) {
					movement.Y += CosWorldRotation;
					movement.X -= SinWorldRotation;
				}
				if (Game.Game.CurrentKS ['A']) {
					movement.X -= CosWorldRotation;
					movement.Y -= SinWorldRotation;
				}
				if (Game.Game.CurrentKS ['S']) {
					movement.Y -= CosWorldRotation;
					movement.X += SinWorldRotation;
				}
				if (Game.Game.CurrentKS ['D']) {
					movement.X += CosWorldRotation;
					movement.Y += SinWorldRotation;
				}

				if (movement != Vector2.Zero) {
					movement.Normalize();
					// thanks killraptor for the suggestion!
					Vector2 movement1 = new Vector2(movement.X, 0f);
					Vector2 movement2 = new Vector2(0f, movement.Y);
					Vector2 finalPosition1 = Position + movement1 * time * MovementSpeed;
					Vector2 finalPosition2 = Position + movement2 * time * MovementSpeed;
					{
						Block collidingWithPlayer = Game.BlockGrid [(int)finalPosition1.X, (int)finalPosition1.Y];
						Door d = collidingWithPlayer as Door;
						if (collidingWithPlayer == null || (d != null && d.IsOpen)) {
							X = finalPosition1.X;
						}
					}
					{
						Block collidingWithPlayer = Game.BlockGrid [(int)finalPosition2.X, (int)finalPosition2.Y];
						Door d = collidingWithPlayer as Door;
						if (collidingWithPlayer == null || (d != null && d.IsOpen)) {
							Z = finalPosition2.Y;
						}
					}
				}
			}

			foreach (InventoryItem item in Inventory) {
				if (item != null) {
					item.Update(time);
				}
			}
		}

		private void DrawInventory() {
			// inventory
			Texture2D guiInvSlot = TextureTools.TextureGuiInventorySlot;
			for (int i = 0; i < Player.InventorySize; ++i) {
				guiInvSlot.Blit(new Rectanglei(0, 0, guiInvSlot.Width, guiInvSlot.Height), new Vector2i((int)(MainClass.ScreenWidth / 2 + (i - Player.InventorySize / 2f) * 15), MainClass.ScreenHeight - 17), Game.Game.Screen);
			}

			for (int i = 0; i < Player.InventorySize; ++i) {
				InventoryItem ii = Inventory [i];
				if (ii != null) {
					ii.Texture.Blit(new Rectanglei(0, 0, ii.Texture.Width, ii.Texture.Height), new Vector2i((int)(MainClass.ScreenWidth / 2 + (i - Player.InventorySize / 2f) * 15), MainClass.ScreenHeight - 17), Game.Game.Screen);
				}
			}

			Texture2D highlighter = TextureTools.TextureInvHighlighter;
			highlighter.Blit(
				new Rectanglei(0, 0, highlighter.Width, highlighter.Height), 
				new Vector2i((int)(MainClass.ScreenWidth / 2 + (CurrentInventorySlot - Player.InventorySize / 2f) * 15), MainClass.ScreenHeight - 17), Game.Game.Screen);
		}

		private void DrawAmmo(Gun gun) {

			int realGunAmmo = (gun.Ammo - 1);
			float endPosition = MainClass.ScreenWidth / 2 + 15 * 2.5f;
			for (int j = 0; j < realGunAmmo / 10 + 1; ++j) {
				float startPosition;
				int yStart = MainClass.ScreenHeight - 19 - (gun.AmmoTexture.Height + 1) * (realGunAmmo / 10 - j + 1);
				if (j == 0) {
					startPosition = MainClass.ScreenWidth / 2 + 15 * 2.5f - (realGunAmmo % 10) * (gun.AmmoTexture.Width + 1);
				} else {
					startPosition = MainClass.ScreenWidth / 2 + 15 * 2.5f - 9 * (gun.AmmoTexture.Width + 1);
				}
				for (int i = (int)startPosition; i < endPosition; i += gun.AmmoTexture.Width + 1) {
					gun.AmmoTexture.Blit(new Rectanglei(0, 0, gun.AmmoTexture.Width, gun.AmmoTexture.Height), new Vector2i(i, yStart), Game.Game.Screen);
				}
			}

			if (gun.MagazinesLeft > 0) {
				string magString = "X" + gun.MagazinesLeft.ToString();
				int yStartMagazineCounter = MainClass.ScreenHeight - 19 - (gun.AmmoTexture.Height + 1);
				float xStartMagazineCounter = MainClass.ScreenWidth / 2 + 15 * 2.5f - 9 - (magString.Length * 5) * (gun.AmmoTexture.Width + 1);

				TextureTools.BlitString(TextureTools.Font, 4, 5, new Vector2i((int)xStartMagazineCounter, yStartMagazineCounter), magString, Game.Game.Screen);
			}
		}

		private void DrawCrossHair(Gun gun) {
			int crossHairX = MainClass.ScreenWidth / 2 - gun.Scope.Width / 2;
			int crossHairY = MainClass.ScreenHeight / 2 - gun.Scope.Height / 2;
			gun.Scope.Blit(new Rectanglei(0, 0, gun.Scope.Width, gun.Scope.Height), new Vector2i(crossHairX, crossHairY), Game.Game.Screen);
		}

		private void DrawHand(InventoryItem iItem) {
			Texture2D tex = iItem.HoldingInHandTexture;

			const int scale = 2;
			int startY = MainClass.ScreenHeight - tex.Height * scale;
			int startX = MainClass.ScreenWidth / 2 + 80 - tex.Width * scale / 2;

			tex.Blit(new Rectanglei(0, 0, tex.Width, tex.Height), new Vector2i(startX, startY), scale, Game.Game.Screen);
		}

		private void DrawHealthBar() {
			// health bar
			float startPosition = MainClass.ScreenWidth / 2 - 15 * 2.5f;

			for (int i = (int)startPosition; i < startPosition + 32f; ++i) {
				Game.Game.Screen.Set(i, MainClass.ScreenHeight - 21, MathUtils3D.GetColor(0.5f, 0.5f, 0.5f, 1f));
				Game.Game.Screen.Set(i, MainClass.ScreenHeight - 20, MathUtils3D.GetColor(0.5f, 0.5f, 0.5f, 1f));
				Game.Game.Screen.Set(i, MainClass.ScreenHeight - 19, MathUtils3D.GetColor(0.5f, 0.5f, 0.5f, 1f));
			}
			for (int i = (int)startPosition + 1; i < startPosition + Health * 31f; ++i) {
				Game.Game.Screen.Set(i, MainClass.ScreenHeight - 20, MathUtils3D.GetColor(1f, 0f, 0f, 1f));
			}
			for (int i = (int)Math.Ceiling(startPosition + Health * 31f); i < startPosition + 31f; ++i) {
				Game.Game.Screen.Set(i, MainClass.ScreenHeight - 20, MathUtils3D.GetColor(.2f, .2f, .2f, 1f));
			}
		}

		public override Vector2i Draw(MainGameState game) {
			Vector2i v = base.Draw(game);
			toPerformWhileNoPP.Add(() => TextureTools.BlitString(TextureTools.Font, 4, 5, new Vector2i(v.X - TextureTools.MeasureString(Name, 4, 5).X / 2, v.Y - 6), Name, game.Game.Screen));
			return v;
		}

		public void Draw() {

			if (Game.IsMultiplayer && AtCrossHair != prevAtCrossHair) {
				Tuple<object, float, float> cur = AtCrossHair as Tuple<object, float, float>;
				Tuple<object, float, float> prev = prevAtCrossHair as Tuple<object, float, float>;
				if (cur != null && prev != null) {
					if (cur.Item1 is Wall && prev.Item1 is Player) {
						Game.Client.SendUpdateLookingAtMessage(AtCrossHair);
					} else if (prev.Item1 is Wall && cur.Item1 is Player) {
						Game.Client.SendUpdateLookingAtMessage(AtCrossHair);
					}
				}

				if ((cur != null && prev == null) || (prev == null && cur != null && cur.Item1 is Player)) {
					Game.Client.SendUpdateLookingAtMessage(AtCrossHair);
				}
			}

			Gun invGun = Inventory[CurrentInventorySlot] as Gun;
			if (invGun != null) {
				DrawAmmo(invGun);
				DrawCrossHair(invGun);
			}

			InventoryItem iItem = Inventory [CurrentInventorySlot];
			if (iItem != null) {
				DrawHand(iItem);
			}

			foreach (Action a in toPerformWhileNoPP) {
				a();
			}
			toPerformWhileNoPP.Clear();

			DrawInventory();
			DrawHealthBar();
			prevAtCrossHair = AtCrossHair;
		}
	}
}

