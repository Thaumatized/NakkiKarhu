using System;
using Godot;
using Godot.Collections;

public partial class MainMenu : Control
{
	//http://www.iana.org/assignments/port-numbers
	//https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.txt
	//10056-10079 are unassigned

	const int port = 10056;
	const int maxPlayers = 32;

	//const string SERVER_IP = "192.168.1.213";
	//const string SERVER_IP = "127.0.0.1";
	const string SERVER_IP = "hydrogen.thaumatized.com";

	public void play()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		Error error = peer.CreateClient(SERVER_IP, port);
		GD.Print(error);
		Multiplayer.MultiplayerPeer = peer;

		GetTree().ChangeSceneToFile("res://Scenes/match.tscn");
	}

	public void startServer()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(port, maxPlayers);
		GD.Print(error);
		Multiplayer.MultiplayerPeer = peer;

		GetTree().ChangeSceneToFile("res://Scenes/match.tscn");
	}

	public void printPeers()
	{
		foreach (Dictionary signal in GetTree().GetSignalList())
		{
			GD.Print(signal["name"]);
		}
		foreach (int peerId in Multiplayer.GetPeers())
		{
			GD.Print(peerId);
		}
	}

	public void connectionStatus(Error error)
	{
		GD.Print(error);
	}
}
