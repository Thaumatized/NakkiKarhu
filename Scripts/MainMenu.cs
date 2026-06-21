using System;
using Godot;

public enum Country
{
	Finland,
	UnitedStatesOfAmerica,
}

public struct ServerInfo
{
	public Country country;
	public string ipOrDomain;
	public int port;

	public override string ToString() => $"({country} - {ipOrDomain}:{port})";

	public ServerInfo(Country country, string ipOrDomain, int port)
	{
		this.country = country;
		this.ipOrDomain = ipOrDomain;
		this.port = port;
	}
}

public partial class MainMenu : Control
{
	const int maxPlayers = 32;

	//http://www.iana.org/assignments/port-numbers
	//https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.txt
	//10056-10079 are unassigned

	ServerInfo[] serverInfos =
	[
		new ServerInfo(Country.Finland, "hydrogen.thaumatized.com", 10056),
		new ServerInfo(Country.Finland, "helium.thaumatized.com", 10056),
		new ServerInfo(Country.Finland, "lithium.thaumatized.com", 10056),
		new ServerInfo(Country.Finland, "beryllium.thaumatized.com", 10056),
	];

	OptionButton serverSelect;
	LineEdit ipOrDomainField;
	LineEdit portField;

	public override void _Ready()
	{
		serverSelect = this.GetNode<OptionButton>("serverOption");
		ipOrDomainField = this.GetNode<LineEdit>("IpOrDomain");
		portField = this.GetNode<LineEdit>("Port");

		foreach (ServerInfo info in serverInfos)
		{
			Texture2D flag = ResourceLoader.Load<Texture2D>(
				"res://2DTextures/CountryFlags/" + info.country.ToString() + ".png"
			);
			serverSelect.AddIconItem(flag, info.ipOrDomain);
		}

		selectServer(0);

		base._Ready();
	}

	void selectServer(int index)
	{
		ipOrDomainField.Text = serverInfos[index].ipOrDomain;
		portField.Text = serverInfos[index].port.ToString();
	}

	string getIpOrDomain()
	{
		return ipOrDomainField.Text;
	}

	int getPort()
	{
		if (int.TryParse(portField.Text, out int port))
			return port;
		else
			return 10056;
	}

	public void join()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		Error error = peer.CreateClient(getIpOrDomain(), getPort());
		GD.Print(error);
		Multiplayer.MultiplayerPeer = peer;

		GetTree().ChangeSceneToFile("res://Scenes/match.tscn");
	}

	public void host()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(getPort(), maxPlayers);
		GD.Print(error);
		Multiplayer.MultiplayerPeer = peer;

		GetTree().ChangeSceneToFile("res://Scenes/match.tscn");
	}

	public void exitGame()
	{
		GetTree().Quit();
	}

	public void connectionStatus(Error error)
	{
		GD.Print(error);
	}
}
