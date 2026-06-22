using System;
using Godot;

public partial class GameManager : Node
{
	const int maxPlayers = 32;

	public static Node staticParent,
		dynamicParent;

	public bool isServerDedicated;

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void askServerIfItIsDedicated()
	{
		GD.Print("askServerIfItIsDedicated");
		RpcId(
			Multiplayer.GetRemoteSenderId(),
			MethodName.serverDedicationResponse,
			OS.HasFeature("dedicated_server")
		);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void serverDedicationResponse(bool dedicated)
	{
		isServerDedicated = dedicated;

		spawnPlayer(Multiplayer.GetUniqueId());

		foreach (int peerId in Multiplayer.GetPeers())
		{
			spawnPlayer(peerId);
		}

		Multiplayer.PeerConnected += spawnPlayer;
		Multiplayer.PeerDisconnected += despawnPlayer;
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

		RpcId(1, MethodName.askServerIfItIsDedicated);
	}

	public void spawnPlayer(long peerId)
	{
		if (peerId == 1 && isServerDedicated)
		{
			GD.Print("Tried to spawn player for server, but is dedicated");
			return;
		}

		GD.Print("spawning: ", peerId);
		Player player = ResourceLoader
			.Load<PackedScene>("res://Scenes/player.tscn")
			.Instantiate<Player>();
		player.id = peerId;
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

	public void despawnPlayer(long peerId)
	{
		GD.Print("despawning: ", peerId);
		dynamicParent.GetNode("player_" + peerId).QueueFree();
	}
}

/*
	SceneTree.get_network_unique_id()
*/
