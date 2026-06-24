using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct PlayerInfo
{
	public long id;
	public string name;

	public PlayerInfo(long id, string name)
	{
		this.id = id;
		this.name = name;
	}
}

public partial class GameManager : Node
{
	const int maxPlayers = 32;

	public static PlayerInfo localPlayer = new PlayerInfo(0, "");

	List<PlayerInfo> players = [];

	public static Node staticParent,
		dynamicParent;

	public PlayerInfo getPlayerInfo(long peerId)
	{
		return players.Find((PlayerInfo player) => player.id == peerId);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void setPlayerProfile(string name)
	{
		if(Multiplayer.IsServer())
		{
			Rpc(MethodName.setPlayerProfile, Multiplayer.GetRemoteSenderId(), name);
		}
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void setPlayerProfile(long peerId, string name)
	{
		GD.Print($"Set player profile {peerId} => {name}");
		if (players.Exists((PlayerInfo player) => player.id == peerId))
		{
			PlayerInfo player = getPlayerInfo(peerId);
			player.name = name;
		}
		else
		{
			players.Add(new PlayerInfo(peerId, name));
			spawnPlayer(peerId);
		}
	}

	public override void _Ready()
	{
		staticParent = this.GetNode("Static");
		dynamicParent = this.GetNode("Dynamic");

		if (OS.HasFeature("dedicated_server"))
		{
			string[] arguments = OS.GetCmdlineUserArgs();
			int port = 10056;
			for (int i = 0; i < arguments.Length; i++)
			{
				GD.Print($"Argument {i} = {arguments[i]}");
				if (arguments[i] == "-p")
				{
					i++;
					if (!int.TryParse(arguments[i], out port))
					{
						GD.Print($"Parsing {arguments[i]} to port failed");
						port = 10056;
					}
				}
			}

			GD.Print($"Starting server, port {port}");

			ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
			Error error = peer.CreateServer(port, maxPlayers);
			GD.Print(error);
			Multiplayer.MultiplayerPeer = peer;
		}

		if (Multiplayer.IsServer())
		{
			Multiplayer.PeerConnected += onPlayerConnect;
		}

		if (localPlayer.id != 0)
		{
			Rpc(MethodName.setPlayerProfile, localPlayer.name);
		}

		Multiplayer.PeerDisconnected += onPlayerDisconnect;
	}

	public void onPlayerConnect(long peerId)
	{
		foreach (PlayerInfo playerInfo in players)
		{
			RpcId(peerId, MethodName.setPlayerProfile, playerInfo.id, playerInfo.name);
		}
	}

	public void onPlayerDisconnect(long peerId)
	{
		GD.Print("despawning: ", peerId);
		foreach (Node child in dynamicParent.GetChildren())
		{
			try
			{
				if (((Projectile)child).shooter.playerInfo.id == peerId)
				{
					child.QueueFree();
				}
			}
			catch { }
		}
		dynamicParent.GetNode("player_" + peerId).QueueFree();
		players.Remove(getPlayerInfo(peerId));
	}

	public void spawnPlayer(long peerId)
	{
		GD.Print("spawning: ", peerId);
		Player player = ResourceLoader
			.Load<PackedScene>("res://Scenes/player.tscn")
			.Instantiate<Player>();
		player.playerInfo = getPlayerInfo(peerId);
		dynamicParent.AddChild(player);
		player.Name = "player_" + peerId;
		player.SetMultiplayerAuthority((int)peerId);
		Random random = new Random();
		player.Position = new Vector3(
			random.Next() % 10,
			random.Next() % 10 + 20,
			random.Next() % 10
		);
	}
}
