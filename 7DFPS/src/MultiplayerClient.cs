using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Threading;

using Pencil.Gaming.MathUtils;

namespace DFPS {
	public class MultiplayerClient : IDisposable {

		public static readonly object WorldMutex = new object();
		public static readonly object PlayerMutex = new object();

		private TcpClient client;
		private MainGameState game;
		private Thread serverHandler;
		public static volatile bool Flag = false;

		public MultiplayerClient(MainGameState _game, string ip, int port) {
			game = _game;
			client = new TcpClient();
			IPEndPoint server = new IPEndPoint(IPAddress.Parse(ip), port);
			client.Connect(server);
			serverHandler = new Thread(HandleServer);
			serverHandler.Start(client);
		}

		private void HandleServer(object param) {
			Console.WriteLine("INFO: Server connected");

			TcpClient thisClient = (TcpClient)param;
			NetworkStream stream = thisClient.GetStream();

			byte[] length = new byte[2];
			byte[] msg;
			int bytesRead;
			short msgLength = -1;

			while (true) {
				if (Flag) {
					break;
				}

				bytesRead = 0;

				if (msgLength == -1) {
					try {
						bytesRead = stream.Read(length, 0, sizeof(short));
						msgLength = BitConverter.ToInt16(length, 0);
					} catch (Exception) {
						return;
					}
				} else {
					msg = new byte[msgLength];
					bytesRead = stream.Read(msg, 0, msgLength);
					while (bytesRead != msgLength) {
						try {
							bytesRead += stream.Read(msg, bytesRead, msgLength - bytesRead);
						} catch (Exception) {
							return;
						}
					}
					HandleMessage(client, msg, bytesRead);
					msgLength = -1;
				}

				if (bytesRead == 0) {
					break;
				}
			}

			thisClient.Close();
			Console.WriteLine("INFO: Disconnected from server");
		}

		private void HandleMessage(TcpClient client, byte[] message, int bytesRead) {
			Message header = (Message)BitConverter.ToUInt16(message, 0);
			Console.WriteLine(header.ToString());
			switch (header) {
//			case Message.OtherPlayerInformation:
//				lock (PlayerMutex) {
//					game.OtherPlayers.Clear();
//					int playerAmount = BitConverter.ToInt32(message, 2);
//					//Console.WriteLine(playerAmount);
//					for (int i = 2 + sizeof(int), j = 0; j < playerAmount; ++j) {
//						int unameLength = BitConverter.ToInt32(message, i);
//						i += sizeof(int);
//						string uname = new ASCIIEncoding().GetString(message, i, unameLength);
//						//Console.WriteLine(unameLength);
//						i += unameLength;
//						float x = BitConverter.ToSingle(message, i);
//						i += sizeof(float);
//						float y = BitConverter.ToSingle(message, i);
//						i += sizeof(float);
//						//Console.WriteLine("X: {0}, Y: {1}", x, y);
//						game.OtherPlayers.Add(new Player(game, x, y, uname));
//					}
//				}
//				break;
			case Message.ConnectPlayer:
				string newPlayerUname = new ASCIIEncoding().GetString(message, 2, bytesRead - 2);
				lock (PlayerMutex) {
					if (!game.OtherPlayers.Any(xOP => xOP.Name == newPlayerUname)) {
						game.OtherPlayers.Add(new Player(game, game.Spawn.X, game.Spawn.Y, newPlayerUname));
					}
				}
				break;
			case Message.OtherPlayerMove:
				string username = new ASCIIEncoding().GetString(message, 2, bytesRead - 2 - sizeof(float) * 2);
				Console.WriteLine(username);
				float xNewPos = BitConverter.ToSingle(message, bytesRead - sizeof(float) * 2);
				float zNewPos = BitConverter.ToSingle(message, bytesRead - sizeof(float) * 1);
				lock (PlayerMutex) {
					Player pl = game.OtherPlayers.SingleOrDefault(x => x.Name == username);
					if (pl == null) {
						break;
					}
					pl.PreviousPosition = pl.ActualPosition;
					pl.ActualPosition = new Vector2(xNewPos, zNewPos);
					pl.PositionCounter = pl.PreviousPositionCounter;
					pl.PreviousPositionCounter = 0f;
					pl.InterpolationCounter = 0f;
				}
				break;
			case Message.SetInventoryItem:
				int slot = message [2];
				ItemID itemID = (ItemID)BitConverter.ToUInt16(message, 3);
				InventoryItem item = null;
				if (itemID != ItemID.Nothing) {
					Type invItemType = typeof(ItemID).Assembly.GetType("DFPS." + itemID.ToString());
					item = Activator.CreateInstance(invItemType) as InventoryItem;

					switch (itemID) {
					case ItemID.DefaultPistol:
					case ItemID.Rifle:
					case ItemID.SemiAuto:
						((Gun)item).Ammo = BitConverter.ToInt32(message, 3 + sizeof(ushort));
						((Gun)item).MagazinesLeft = BitConverter.ToInt32(message, 3 + sizeof(ushort) + sizeof(int));
						break;
					case ItemID.Grenade:
						item = new InventoryStack<Grenade>(BitConverter.ToInt32(message, 3 + sizeof(ushort)));
						break;
					}
				}
				game.CurrentPlayer.SetInventoryItem(slot, item);
				break;
			case Message.Move:
				float xPlayer = BitConverter.ToSingle(message, 2);
				float yPlayer = BitConverter.ToSingle(message, 2 + sizeof(float));
				game.CurrentPlayer.Position = new Vector2(xPlayer, yPlayer);
				break;
			case Message.UpdateWorld:
				lock (WorldMutex) {
					game.BlockGrid = new Block[message [2], message [3]];
					for (int i = 4; i < bytesRead - 1 - sizeof(int) * 2;) {
						byte wallType = message [i++];

						switch (wallType) {
						case 0:
							{
								int x = BitConverter.ToInt32(message, i);
								i += sizeof(int);
								int z = BitConverter.ToInt32(message, i);
								i += sizeof(int);
								Block toAdd = new Block(x, z, TextureTools.TextureWall1);
								game.Blocks.Add(toAdd);
								game.BlockGrid [toAdd.X, toAdd.Z] = toAdd;
							}
							break;
						case 1:
							{
								int x = BitConverter.ToInt32(message, i);
								i += sizeof(int);
								int z = BitConverter.ToInt32(message, i);
								i += sizeof(int);
								Block toAdd = new Block(x, z, TextureTools.TextureWall2);
								game.Blocks.Add(toAdd);
								game.BlockGrid [toAdd.X, toAdd.Z] = toAdd;
							}
							break;
						case 2:
							{
								int x = BitConverter.ToInt32(message, i);
								i += sizeof(int);
								int z = BitConverter.ToInt32(message, i);
								i += sizeof(int);
								Door toAdd = new Door(x, z, TextureTools.TextureDoor1, TextureTools.TextureDoor1Upper, TextureTools.TextureDoor1Lower);
								game.Blocks.Add(toAdd);
								game.BlockGrid [toAdd.X, toAdd.Z] = toAdd;
							}
							break;
						case 3:
							{
								float x = BitConverter.ToSingle(message, i);
								i += sizeof(float);
								float y = BitConverter.ToSingle(message, i);
								i += sizeof(float);
								HealthPack hp = new HealthPack(x, y);
								game.HealthPacks.Add(hp);
							}
							break;
						case 4:
							{
								float x = BitConverter.ToSingle(message, i);
								i += sizeof(float);
								float y = BitConverter.ToSingle(message, i);
								i += sizeof(float);
								AmmoCrate ac = new AmmoCrate(x, y);
								game.Crates.Add(ac);
							}
							break;
						default:
							throw new Exception();
						}
					}
				}
				break;
			case Message.Shot1Confirmed:
				Sounds.Shoot1.Play();
				break;
			case Message.Shot2Confirmed:
				Sounds.Shoot2.Play();
				break;
			case Message.Shot3Confirmed:
				Sounds.Shoot3.Play();
				break;
			case Message.ReloadConfimed:
				Sounds.Reload1.Play();
				break;
			case Message.UpdateHealth:
				float health = BitConverter.ToSingle(message, 2);
				game.CurrentPlayer.Health = health;
				break;
			case Message.AddCrate:
				game.Crates.Add(new AmmoCrate(BitConverter.ToSingle(message, 2), BitConverter.ToSingle(message, 2 + sizeof(float))));
				break;
			case Message.AddHealthPack:
				game.HealthPacks.Add(new HealthPack(BitConverter.ToSingle(message, 2), BitConverter.ToSingle(message, 2 + sizeof(float))));
				break;
			case Message.RemoveCrate:
				float xCrate = BitConverter.ToSingle(message, 2);
				float zCrate = BitConverter.ToSingle(message, 2 + sizeof(float));
				game.Crates = game.Crates.Where(c => c.X != xCrate && c.Z != zCrate).ToList();
				break;
			case Message.RemoveHealthPack:
				float xHP = BitConverter.ToSingle(message, 2);
				float zHP = BitConverter.ToSingle(message, 2 + sizeof(float));
				game.HealthPacks = game.HealthPacks.Where(c => c.X != xHP && c.Z != zHP).ToList();
				break;
			case Message.ThrowGrenade:
				float xGren = BitConverter.ToSingle(message, 2);
				float yGren = BitConverter.ToSingle(message, 2 + sizeof(float) * 1);
				float xGrenDir = BitConverter.ToSingle(message, 2 + sizeof(float) * 2);
				float yGrenDir = BitConverter.ToSingle(message, 2 + sizeof(float) * 3);
				game.Projectiles.Add(new Grenade.GenadeProjectile(xGren, yGren, new Vector2(xGrenDir, yGrenDir)));
				break;
			case Message.DropItem:
				ItemID toDrop = (ItemID)BitConverter.ToUInt16(message, 2);
				int position = sizeof(ItemID);
				InventoryItem iItem = null;
				if (toDrop != ItemID.Nothing) {
					Type invItemType = typeof(ItemID).Assembly.GetType("DFPS." + toDrop.ToString());
					iItem = Activator.CreateInstance(invItemType) as InventoryItem;

					switch (toDrop) {
						case ItemID.DefaultPistol:
						case ItemID.Rifle:
						case ItemID.SemiAuto:
						((Gun)iItem).Ammo = BitConverter.ToInt32(message, 3 + sizeof(ushort));
						((Gun)iItem).MagazinesLeft = BitConverter.ToInt32(message, 3 + sizeof(ushort) + sizeof(int));
						position += 2 * sizeof(int);
						break;
					case ItemID.Grenade:
						iItem = new InventoryStack<Grenade>(BitConverter.ToInt32(message, 3 + sizeof(ushort)));
						position += sizeof(int);
						break;
					}
				}
				float xDrop = BitConverter.ToSingle(message, 2 + position);
				float yDrop = BitConverter.ToSingle(message, 2 + position + sizeof(float) * 1);
				float xDropDir = BitConverter.ToSingle(message, 2 + position + sizeof(float) * 2);
				float yDropDir = BitConverter.ToSingle(message, 2 + position + sizeof(float) * 3);
				game.Projectiles.Add(new InventoryDroppedWeapon((Weapon)iItem, xDrop, yDrop, xDropDir, yDropDir));
				break;
			case Message.DisconnectPlayer:
				string otherUsername = new ASCIIEncoding().GetString(message, 2, bytesRead - 2);
				lock (PlayerMutex) {
					game.OtherPlayers = game.OtherPlayers.Where(op => op.Name != otherUsername).ToList();
				}
				break;
			case Message.Chat:
				string chatMessage = new ASCIIEncoding().GetString(message, 2, bytesRead - 2);
				lock (MainGameState.ChatMutex) {
					game.Chat.Insert(0, new Tuple<DateTime, string>(DateTime.Now, chatMessage));
				}
				break;
			}
		}

		public void SendChatMessage(string msg) {
			SendMessage(Message.Chat, msg);
		}

		public void SendUpdateWayFacingMessage(float sinT, float cosT) {
			byte[] xPosBytes = BitConverter.GetBytes(sinT);
			byte[] yPosBytes = BitConverter.GetBytes(cosT);
			SendMessage(Message.UpdateDirectionFacing, xPosBytes.Concat(yPosBytes).ToArray());
		}

		public void SendConnectMessage(string username) {
			SendMessage(Message.SetUsername, username);
		}

		public void SendLeftClickMessage(int invPosition) {
			SendMessage(Message.LeftClick, new [] { (byte)invPosition }.ToArray());
		}

		public void SendLeftUnclickMessage() {
			SendMessage(Message.LeftUnclick, new byte[] { });
		}

		public void SendUpdateLookingAtMessage(object lookingAt) {
			List<byte> message = new List<byte>();
			Tuple<object, float, float> lookingAtObj = lookingAt as Tuple<object, float, float>;
			if (lookingAtObj != null) {
				Player player = lookingAtObj.Item1 as Player;
				if (player != null) {
					message.AddRange(new ASCIIEncoding().GetBytes(player.Name));
				}
			} else {
			}
			SendMessage(Message.UpdateLookingAt, message.ToArray());
		}

		public void SendMoveMessage(float xPos, float yPos) {
			byte[] xPosBytes = BitConverter.GetBytes(xPos);
			byte[] yPosBytes = BitConverter.GetBytes(yPos);
			SendMessage(Message.Move, xPosBytes.Concat(yPosBytes).ToArray());
		}

		public void SendMessage(Message msg, string msgString) {
			byte[] header = BitConverter.GetBytes((ushort)msg);
			byte[] contents = new ASCIIEncoding().GetBytes(msgString);
			byte[] full = header.Concat(contents).ToArray();
			SendMessage(full);
		}

		public void SendMessage(Message msg, byte[] messageBytes) {
			byte[] header = BitConverter.GetBytes((ushort)msg);
			byte[] full = header.Concat(messageBytes).ToArray();
			SendMessage(full);
		}

		public void SendMessage(byte[] message) {
			byte[] len = BitConverter.GetBytes((ushort)message.Length);
			NetworkStream stream = client.GetStream();
			stream.Write(len.Concat(message).ToArray(), 0, message.Length + len.Length);
			stream.Flush();
		}

		public void SendMessage(string message) {
			byte[] msg = new ASCIIEncoding().GetBytes(message);
			SendMessage(msg);
		}

		public void Dispose() {
			Flag = true;
		}
	}
}
