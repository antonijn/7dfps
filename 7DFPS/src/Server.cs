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
	public class Server {

		public static Server Current { get; private set; }

		public static readonly object playerMutex = new object();
		public static readonly object leftClickItemsMutex = new object();
		public static readonly object worldMutex = new object();
		public static readonly object mouseMutex = new object();

		public List<Tuple<InventoryItem, Player>> ToPerformLeftClickOn = new List<Tuple<InventoryItem, Player>>();

		public List<TcpClient> Clients { get; private set; }
		public Dictionary<TcpClient, Player> Players { get; private set; }
		public readonly List<Player> PlayerList = new List<Player>();
		public MainGameState World { get; private set; }
		public MainClass ServerSideProgram { get; private set; }
		public Dictionary<Player, bool> MouseStates { get; private set; }
		public Dictionary<Player, bool> PreviousMouseStates { get; private set; }

		private TcpListener tcpListener;
		private Thread listenThread;

		public int Port { get; private set; }

		public Server(int port) {

			Current = this;

			MouseStates = new Dictionary<Player, bool>();
			PreviousMouseStates = new Dictionary<Player, bool>();

			Clients = new List<TcpClient>();
			Players = new Dictionary<TcpClient, Player>();

			Port = port;
			tcpListener = new TcpListener(IPAddress.Any, port);
			Console.WriteLine("INFO: Opened server on {0}", port);

			listenThread = new Thread(ListenForClients);
			listenThread.Start();

			ServerSideProgram = new MainClass();
			MainGameState.IsServer = true;
			World = new MainGameState(ServerSideProgram);
		}

		private void ListenForClients() {
			Console.WriteLine("INFO: Started listener thread: {0}", Thread.CurrentThread.ManagedThreadId);

			tcpListener.Start();
			while (true) {
				TcpClient client = tcpListener.AcceptTcpClient();
				Clients.Add(client);

				Thread clientThread = new Thread(HandleClient);
				clientThread.Start(client);
			}
		}

		private void HandleClient(object param) {
			Console.WriteLine("INFO: Client connected");

			TcpClient client = (TcpClient)param;
			lock (playerMutex) {
				Player newPlayer = new Player(World, World.Spawn.X, World.Spawn.Y, "UNNAMED");
				PlayerList.Add(newPlayer);
				Players [client] = newPlayer;

				lock (mouseMutex) {
					MouseStates [newPlayer] = false;
					PreviousMouseStates [newPlayer] = false;
				}
				Players [client].Client = client;
			}

			NetworkStream stream = client.GetStream();

			byte[] length = new byte[2];
			byte[] msg;
			int bytesRead;
			short msgLength = -1;

			while (true) {
				bytesRead = 0;
				if (msgLength == -1) {
					try {
						bytesRead = stream.Read(length, 0, sizeof(short));
						msgLength = BitConverter.ToInt16(length, 0);
					} catch (Exception) {
						break;
					}
				} else {
					msg = new byte[msgLength];
					try {
						bytesRead = stream.Read(msg, 0, msgLength);
						while (bytesRead != msgLength) {
							bytesRead += stream.Read(msg, bytesRead, msgLength - bytesRead);
						}
					} catch (Exception) {
						break;
					}
					HandleMessage(client, msg, bytesRead);
					msgLength = -1;
				}

				if (bytesRead == 0) {
					break;
				}

				//Console.WriteLine("INFO: Recieved message from client {0}: {1}", Clients.IndexOf(client), ;
			}

			DisconnectClient(client);
		}

		private void HandleMessage(TcpClient client, byte[] message, int bytesRead) {
			Message header = (Message)BitConverter.ToUInt16(message, 0);
#if DEBUG
			Console.WriteLine(header.ToString());
#endif
			switch (header) {
			case Message.Move:
				Vector2 newPosition = new Vector2();
				newPosition.X = BitConverter.ToSingle(message, 2);
				newPosition.Y = BitConverter.ToSingle(message, 2 + sizeof(float));
				lock (playerMutex) {
					Player playerPosChanged = Players [client];
					playerPosChanged.Position = newPosition;
					foreach (Player playerOther in PlayerList) {
						if (playerOther != Players[client]) {
							SendOtherPlayerMove(playerOther.Client, playerPosChanged.Name, newPosition.X, newPosition.Y);
						}
					}
				}
				break;
			case Message.SetUsername:
				string username = new ASCIIEncoding().GetString(message, 2, bytesRead - 2);
				lock (playerMutex) {
					if (PlayerList.Any(p => p.Name == username)) {
						//DisconnectClient(client);
					}
					Players [client].Name = username;

					foreach (Player pOther in PlayerList) {
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						// ajdi0pawjudiopajdpawjdpajwdipaipfuisweahofraswhu8io34rhuiawghosuerh
						if (pOther != Players[client]) {
							SendPlayeronnectMessage(pOther.Client, username);
							SendPlayeronnectMessage(client, pOther.Name);
						}
					}
				}
				UpdateWorld(client);
				SendMoveMessage(client, World.Spawn.X, World.Spawn.Y);
				break;
			case Message.LeftClick:
				int slot = message [bytesRead - 1];
				lock (playerMutex) {
					InventoryItem inHand = Players [client].GetInventoryItem(slot);

					lock (leftClickItemsMutex) {
						ToPerformLeftClickOn.Add(new Tuple<InventoryItem, Player>(inHand, Players[client]));
					}

					lock (mouseMutex) {
						MouseStates [Players [client]] = true;
					}
				}
				break;
			case Message.LeftUnclick:
				lock (leftClickItemsMutex) {
					ToPerformLeftClickOn = ToPerformLeftClickOn.Where(tplco => tplco.Item2 != Players[client]).ToList();
				}
				lock (mouseMutex) {
					MouseStates [Players [client]] = false;
				}
				break;
			case Message.UpdateLookingAt:
				Player plyr = Players [client];
				if (bytesRead > 2) {
					string msg = new ASCIIEncoding().GetString(message, 2, bytesRead - 2);
					lock (playerMutex) {
						Player other = PlayerList.SingleOrDefault(x => x.Name == msg);
						if (other == null) {
							DisconnectClient(client);
						}
						Tuple<object, float, float> lookingAt = new Tuple<object, float, float>(other, 0f, 0f);
						plyr.AtCrossHair = lookingAt;
					}
				} else {
					plyr.AtCrossHair = null;
				}
				break;
			case Message.UpdateDirectionFacing:
				float sinT = BitConverter.ToSingle(message, 2);
				float cosT = BitConverter.ToSingle(message, 2 + sizeof(float));
				lock (playerMutex) {
					Player player = Players [client];
					player.SinWorldRotation = sinT;
					player.CosWorldRotation = cosT;
				}
				break;
			case Message.Chat:
				string chatMsg = new ASCIIEncoding().GetString(message, 2, bytesRead - 2);
				Player chatFrom = Players [client];
				chatMsg = string.Format("{0}: {1}", chatFrom.Name, chatMsg);
				lock (playerMutex) {
					foreach (TcpClient tcp in PlayerList.Select(plyrr => plyrr.Client)) {
						SendChatMessage(tcp, chatMsg);
					}
				}
				break;
			}
		}

		public void SendPlayeronnectMessage(TcpClient client, string name) {
			SendMessage(client, Message.ConnectPlayer, name);
		}

		public void SendPlayerDisconnectMessage(TcpClient client, string name) {
			SendMessage(client, Message.DisconnectPlayer, name);
		}

		public void DisconnectClient(TcpClient client) {
			lock (playerMutex) {
				Player toBeRemoved = Players [client];
				PlayerList.Remove(toBeRemoved);
				Players.Remove(client);
			
				foreach (Player other in PlayerList) {
					SendPlayerDisconnectMessage(other.Client, toBeRemoved.Name);
				}
			}
			client.Close();
			Console.WriteLine("INFO: Client {0} disconnected", Clients.IndexOf(client));
		}

		public void SendDropItemMessage(TcpClient client, InventoryItem item, float x, float z, float xDir, float zDir) {
			List<byte> bytes = new List<byte>();
			ItemID itemID = ItemID.Nothing;
			if (item is InventoryStack) {
				itemID = ItemID.Grenade;
			} else if (item != null) {
				itemID = (ItemID)Enum.Parse(typeof(ItemID), item.GetType().Name);
			}
			bytes.AddRange(BitConverter.GetBytes((ushort)itemID));
			switch (itemID) {
			case ItemID.DefaultPistol:
			case ItemID.Rifle:
			case ItemID.SemiAuto:
				bytes.AddRange(BitConverter.GetBytes((item as Gun).Ammo));
				bytes.AddRange(BitConverter.GetBytes((item as Gun).MagazinesLeft));
				break;
			case ItemID.Grenade:
				InventoryStack stack = item as InventoryStack;
				if (stack != null) {
					bytes.AddRange(BitConverter.GetBytes(stack.Amount));
				} else {
					bytes.Add(1);
				}
				break;
			}
			byte[] xBytes = BitConverter.GetBytes(x);
			byte[] zBytes = BitConverter.GetBytes(z);
			byte[] xDirBytes = BitConverter.GetBytes(xDir);
			byte[] zDirBytes = BitConverter.GetBytes(zDir);
			bytes.AddRange(xBytes);
			bytes.AddRange(zBytes);
			bytes.AddRange(xDirBytes);
			bytes.AddRange(zDirBytes);
			SendMessage(client, Message.DropItem, bytes.ToArray());
		}

		public void SendThrowGrenadeMessage(TcpClient client, float x, float z, float xDir, float zDir) {
			byte[] xBytes = BitConverter.GetBytes(x);
			byte[] zBytes = BitConverter.GetBytes(z);
			byte[] xDirBytes = BitConverter.GetBytes(xDir);
			byte[] zDirBytes = BitConverter.GetBytes(zDir);
			SendMessage(client, Message.ThrowGrenade, xBytes.Concat(zBytes).Concat(xDirBytes).Concat(zDirBytes).ToArray());
		}

		public void SendMoveMessage(TcpClient client, float x, float z) {
			byte[] xBytes = BitConverter.GetBytes(x);
			byte[] zBytes = BitConverter.GetBytes(z);
			SendMessage(client, Message.Move, xBytes.Concat(zBytes).ToArray());
		}

		public void SendAddHealthPack(TcpClient client, float x, float z) {
			byte[] xBytes = BitConverter.GetBytes(x);
			byte[] zBytes = BitConverter.GetBytes(z);
			SendMessage(client, Message.AddHealthPack, xBytes.Concat(zBytes).ToArray());
		}

		public void SendRemoveHealthPack(TcpClient client, float x, float z) {
			byte[] xBytes = BitConverter.GetBytes(x);
			byte[] zBytes = BitConverter.GetBytes(z);
			SendMessage(client, Message.RemoveHealthPack, xBytes.Concat(zBytes).ToArray());
		}

		public void SendAddAmmoCrates(TcpClient client, float x, float z) {
			byte[] xBytes = BitConverter.GetBytes(x);
			byte[] zBytes = BitConverter.GetBytes(z);
			SendMessage(client, Message.AddCrate, xBytes.Concat(zBytes).ToArray());
		}

		public void SendRemoveAmmoCrates(TcpClient client, float x, float z) {
			byte[] xBytes = BitConverter.GetBytes(x);
			byte[] zBytes = BitConverter.GetBytes(z);
			SendMessage(client, Message.RemoveCrate, xBytes.Concat(zBytes).ToArray());
		}

		public void SendUpdateHealthMessage(TcpClient client, float newhealth) {
			if (newhealth <= 0) {
				Player plyr = Players [client];
				plyr.Inventory [0] = new DefaultPistol();
				plyr.Inventory [1] = new ButterKnife();
				plyr.Inventory [2] = new Grenade();
				plyr.Inventory [3] = null;
				plyr.Inventory [4] = null;

				SendSetInventoryItemMessage(client, 0, new DefaultPistol());
				SendSetInventoryItemMessage(client, 1, new ButterKnife());
				SendSetInventoryItemMessage(client, 2, new Grenade());
				SendSetInventoryItemMessage(client, 3, null);
				SendSetInventoryItemMessage(client, 4, null);

				SendUpdateHealthMessage(client, 1f);
				plyr.Health = 1f;
				SendMoveMessage(client, World.Spawn.X, World.Spawn.X);
			} else {
				byte[] healthBytes = BitConverter.GetBytes(newhealth);
				SendMessage(client, Message.UpdateHealth, healthBytes);
			}
		}

		public void SendSetInventoryItemMessage(TcpClient client, int slot, InventoryItem item) {
			// TODO: grenade inventory stacks
			List<byte> bytes = new List<byte>();
			bytes.Add((byte)slot);
			ItemID itemID = ItemID.Nothing;
			if (item is InventoryStack) {
				itemID = ItemID.Grenade;
			} else if (item != null) {
				itemID = (ItemID)Enum.Parse(typeof(ItemID), item.GetType().Name);
			}
			bytes.AddRange(BitConverter.GetBytes((ushort)itemID));
			switch (itemID) {
				case ItemID.DefaultPistol:
				case ItemID.Rifle:
				case ItemID.SemiAuto:
				bytes.AddRange(BitConverter.GetBytes((item as Gun).Ammo));
				bytes.AddRange(BitConverter.GetBytes((item as Gun).MagazinesLeft));
				break;
				case ItemID.Grenade:
				InventoryStack stack = item as InventoryStack;
				if (stack != null) {
					bytes.AddRange(BitConverter.GetBytes(stack.Amount));
				} else {
					bytes.AddRange(BitConverter.GetBytes((int)1));
				}
				break;
			}
			SendMessage(client, Message.SetInventoryItem, bytes.ToArray());
		}

		public void SendChatMessage(TcpClient client, string message) {
			SendMessage(client, Message.Chat, message);
		}
		public void SendShot1Message(TcpClient client) {
			SendMessage(client, Message.Shot1Confirmed, new byte[] { });
		}
		public void SendShot2Message(TcpClient client) {
			SendMessage(client, Message.Shot2Confirmed, new byte[] { });
		}
		public void SendShot3Message(TcpClient client) {
			SendMessage(client, Message.Shot3Confirmed, new byte[] { });
		}
		public void SendReloadConfirmedMessage(TcpClient client) {
			SendMessage(client, Message.ReloadConfimed, new byte[] { });
		}

		public void SendMessage(TcpClient client, Message msg, string msgString) {
			byte[] header = BitConverter.GetBytes((ushort)msg);
			byte[] contents = new ASCIIEncoding().GetBytes(msgString);
			byte[] full = header.Concat(contents).ToArray();
			//Console.WriteLine(msg.ToString() + ": " + msgString);
			SendMessage(client, full);
		}

		public void SendMessage(TcpClient client, Message msg, byte[] msgBytes) {
			byte[] header = BitConverter.GetBytes((ushort)msg);
			byte[] full = header.Concat(msgBytes).ToArray();
			//Console.WriteLine(msg.ToString() + ": " + msgBytes.ToString());
			SendMessage(client, full);
		}

		public void SendMessage(TcpClient client, byte[] message) {
			if (client.Connected) {
				byte[] len = BitConverter.GetBytes((ushort)message.Length);
				NetworkStream stream = client.GetStream();
				stream.Write(len.Concat(message).ToArray(), 0, message.Length + len.Length);
				stream.Flush();
			}
		}

		/*public void UpdatePlayers() {
			lock (playerMutex) {
				foreach (Player p in PlayerList) {
					TcpClient client = p.Client;
					IEnumerable<Player> others = PlayerList.Where(x => x != p);
					List<byte> toSend = new List<byte>();
					toSend.AddRange(BitConverter.GetBytes(others.Count()));
					foreach (Player other in others) {
						toSend.AddRange(BitConverter.GetBytes(other.Name.Length));
						toSend.AddRange(new ASCIIEncoding().GetBytes(other.Name));
						byte[] position = BitConverter.GetBytes(other.Position.X).Concat(BitConverter.GetBytes(other.Position.Y)).ToArray();
						toSend.AddRange(position);
					}
					SendMessage(client, Message.OtherPlayerInformation, toSend.ToArray());
				}
			}
		}*/


		public void SendOtherPlayerMove(TcpClient client, string playerName, float x, float z) {
			List<byte> bytes = new List<byte>();
			bytes.AddRange(new ASCIIEncoding().GetBytes(playerName));
			bytes.AddRange(BitConverter.GetBytes(x));
			bytes.AddRange(BitConverter.GetBytes(z));
			SendMessage(client, Message.OtherPlayerMove, bytes.ToArray());
		}

		public void UpdateWorld(TcpClient client) {
			List<byte> bytes = new List<byte>();
			bytes.Add((byte)World.WorldWidth);
			bytes.Add((byte)World.WorldHeight);
			lock (worldMutex) {
				foreach (Block b in World.Blocks) {
					if (b is Door) {
						bytes.Add(2);
					} else if (b.Wall1.Texture == TextureTools.TextureWall1) {
						bytes.Add(0);
					} else if (b.Wall1.Texture == TextureTools.TextureWall2) {
						bytes.Add(1);
					}
					byte[] positionX = BitConverter.GetBytes(b.X);
					byte[] positionZ = BitConverter.GetBytes(b.Z);
					bytes.AddRange(positionX);
					bytes.AddRange(positionZ);
				}
				foreach (HealthPack hp in World.HealthPacks) {
					bytes.Add(3);
					byte[] positionX = BitConverter.GetBytes(hp.X);
					byte[] positionZ = BitConverter.GetBytes(hp.Z);
					bytes.AddRange(positionX);
					bytes.AddRange(positionZ);
				}
				foreach (AmmoCrate ac in World.Crates) {
					bytes.Add(4);
					byte[] positionX = BitConverter.GetBytes(ac.X);
					byte[] positionZ = BitConverter.GetBytes(ac.Z);
					bytes.AddRange(positionX);
					bytes.AddRange(positionZ);
				}
				SendMessage(client, Message.UpdateWorld, bytes.ToArray());
			}
		}
	}
}
