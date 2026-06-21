using System;
using System.Data.Common;
using Godot;

public partial class GameManager : Node
{
	public static Node staticParent,
		dynamicParent;

	public override void _Ready()
	{
		staticParent = this.GetNode("Static");
		dynamicParent = this.GetNode("Dynamic");

		spawnPlayer(Multiplayer.GetUniqueId());

		foreach (int peerId in Multiplayer.GetPeers())
		{
			spawnPlayer(peerId);
		}

		Multiplayer.PeerConnected += spawnPlayer;
		Multiplayer.PeerDisconnected += despawnPlayer;
		//GetTree().Connect("network_peer_disconnected", this, "connectionStatus");
	}

	public void spawnPlayer(long peerId)
	{
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
