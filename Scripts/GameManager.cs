using System;
using System.Collections.Generic;
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
	public static PlayerInfo localPlayer = new PlayerInfo(0, "");

	List<PlayerInfo> players = [];

	[Export]
	public Node playerContainer,
		projectileContainer;

	public static GameManager instance;

	public PlayerInfo getPlayerInfo(long id)
	{
		return players.Find((PlayerInfo player) => player.id == id);
	}

	public Player GetPlayer(long id)
	{
		return playerContainer.GetNode<Player>(id.ToString());
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void setPlayerProfile(string name)
	{
		if (Multiplayer.IsServer())
		{
			long peerId = Multiplayer.GetRemoteSenderId();

			Rpc(MethodName.setPlayerProfile, peerId, name);
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
		}
		GetPlayer(peerId).setPlayerName(name);
	}

	public override void _Ready()
	{
		GD.Print("I exsists");
		Input.MouseMode = Input.MouseModeEnum.Captured;

		instance = this;

		if (Multiplayer.IsServer())
		{
			if (!OS.HasFeature("dedicated_server") && !OS.HasFeature("test_dedicated_server"))
			{
				spawnPlayer(Multiplayer.GetUniqueId());
				Rpc(MethodName.setPlayerProfile, localPlayer.name);
			}

			Multiplayer.PeerConnected += onPlayerConnect;
			Multiplayer.PeerDisconnected += onPlayerDisconnect;
		}
		else
		{
			Rpc(MethodName.setPlayerProfile, localPlayer.name);
		}
	}

	public void onPlayerConnect(long peerId)
	{
		GD.Print($"Player {peerId} connected");
		spawnPlayer(peerId);
		foreach (PlayerInfo playerInfo in players)
		{
			RpcId(peerId, MethodName.setPlayerProfile, playerInfo.id, playerInfo.name);
		}
	}

	public void onPlayerDisconnect(long peerId)
	{
		GD.Print("despawning: ", peerId);
		foreach (Node child in playerContainer.GetChildren())
		{
			try
			{
				if (((Projectile)child).shooter.playerId == peerId)
				{
					child.QueueFree();
				}
			}
			catch { }
		}
		playerContainer.GetNode(peerId.ToString()).QueueFree();
		players.Remove(getPlayerInfo(peerId));
	}

	public void spawnPlayer(long peerId)
	{
		GD.Print("spawning: ", peerId);
		Player player = ResourceLoader
			.Load<PackedScene>("res://Scenes/player.tscn")
			.Instantiate<Player>();
		player.Name = peerId.ToString();
		(player).Set("playerId", peerId);
		playerContainer.AddChild(player);
	}
}
