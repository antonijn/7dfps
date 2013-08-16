using System;

using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class Enemy : BillBoard {
		private bool gotKilled = false;

		public bool ShouldBeRemoved { get; private set; }
		private float health = 1f;
		private float shootCounter = 0f;
		private float keepWalkingFor;
		private float stopFor;
		private float angle = 0f;
		public float Health {
			get { return health; }
			set { health = value; if (health <= 0f) { Kill(); } }
		}

		public Enemy(float x, float z) : base(TextureTools.TextureEnemy, x, z, .65f, 0f) {
			speed += ((float)MainClass.Random.NextDouble() - .5f) * 0.2f;
			keepWalkingFor = ((float)MainClass.Random.NextDouble() * .5f + .5f) * 5f;
		}

		public void Update(MainGameState game, float time) {
			if (gotKilled) {
				ShouldBeRemoved = UpdateAnimation(TextureTools.EnemyDeathAnimation, 4f, time);
				if (ShouldBeRemoved) {
					Drop(game);
				}
			} else {
				UpdateAI(game, time);
			}
		}

		private float speed = .5f;

		private bool AreWallsBlockingSight(MainGameState game, float playerDiffX, float playerDiffZ) {
			Vector2 vec = new Vector2(playerDiffX, playerDiffZ);
			vec.Normalize();
			vec /= 10f;
			float playerDiffXN = vec.X;
			float playerDiffZN = vec.Y;

			float x = 0;
			float z = 0;
			// thanks SixPairsOfFeet, again!
			// thanks SixPairsOfFeet, again, again!
			while (x * x + z * z < 8 * 8 && 
			       Math.Abs(x) < Math.Abs(playerDiffX) && Math.Abs(z) < Math.Abs(playerDiffZ)) {
				float xAbs = x + X;
				float zAbs = z + Z;

				int xWall = (int)xAbs;
				int zWall = (int)zAbs;
				if (game.BlockGrid [xWall, zWall] != null) {
					return true;
				}

				x += playerDiffXN;
				z += playerDiffZN;
			}

			return false;
		}

		private void UpdateAI(MainGameState game, float time) {
			// idea of super amazing AI with more randomness conceived by SixPairsOfFeet
			keepWalkingFor -= time;
			stopFor -= time;
			if (keepWalkingFor >= 0f && stopFor <= 0f) {
				float playerDiffX = game.CurrentPlayer.X - X;
				float playerDiffZ = game.CurrentPlayer.Z - Z;
				float playerDist = playerDiffX * playerDiffX + playerDiffZ * playerDiffZ;
				if (playerDist < MainGameState.WallsDisappearAt * MainGameState.WallsDisappearAt / (2 * 2)) {
					shootCounter -= time;
					if (shootCounter <= 0f) {
						if (!AreWallsBlockingSight(game, playerDiffX, playerDiffZ)) {
							game.CurrentPlayer.Health -= ((float)MainClass.Random.NextDouble() * .5f + .5f) * .05f;
							shootCounter = ((float)MainClass.Random.NextDouble() * .5f + .5f) * 1f;
							Sounds.Shoot1.Play();
						}
					}

					Vector2 movement = Vector2.Normalize(new Vector2(playerDiffX, playerDiffZ));
					movement = Vector3.Transform(new Vector3(movement.X, 0f, movement.Y), Matrix.CreateRotationY(angle)).Xz;
					// thanks killraptor for the suggestion!
					Vector2 movement1 = new Vector2(movement.X, 0f);
					Vector2 movement2 = new Vector2(0f, movement.Y);
					Vector2 finalPosition1 = new Vector2(X, Z) + movement1 * time * speed;
					Vector2 finalPosition2 = new Vector2(X, Z) + movement2 * time * speed;
					{
						Block collidingWithPlayer = game.BlockGrid [(int)finalPosition1.X, (int)finalPosition1.Y];
						if (collidingWithPlayer == null || !collidingWithPlayer.CollidesWithVector(finalPosition1)) {
							X = finalPosition1.X;
						}
					}
					{
						Block collidingWithPlayer = game.BlockGrid [(int)finalPosition2.X, (int)finalPosition2.Y];
						if (collidingWithPlayer == null || !collidingWithPlayer.CollidesWithVector(finalPosition2)) {
							Z = finalPosition2.Y;
						}
					}
				}
			} else {
				if (stopFor <= 0f) {
					stopFor = ((float)MainClass.Random.NextDouble() * .5f + .5f) * 2f;
				}
				keepWalkingFor = ((float)MainClass.Random.NextDouble() * .5f + .5f) * 5f;
				angle = ((float)MainClass.Random.NextDouble() * 2f - 1f) * MathHelper.TauOver4;
			}
		}

		private void Drop(MainGameState game) {
			DroppedWeapon toDrop = null;

			int rand = MainClass.Random.Next(6);
			switch (rand) {
			case 0:
				toDrop = new DroppedWeapon<Grenade>(X, Z);
				break;
			case 1:
				toDrop = new DroppedWeapon<Rifle>(X, Z);
				break;
			case 2:
				toDrop = new DroppedWeapon<SemiAuto>(X, Z);
				break;
			}

			if (toDrop != null) {
				game.DroppedWeapons.Add(toDrop);
			}
		}

		private void Kill() {
			gotKilled = true;
			Texture = TextureTools.EnemyDeathAnimation [0];
		}
	}
}

