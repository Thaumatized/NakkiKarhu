using Godot;

public enum Country
{
	Finland,
	Mexico,
	Thailand,
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

public partial class Menu : Control
{
	//http://www.iana.org/assignments/port-numbers
	//https://www.iana.org/assignments/service-names-port-numbers/service-names-port-numbers.txt
	//10056-10079 are unassigned

	ServerInfo[] serverInfos =
	[
		new ServerInfo(Country.Finland, "localhost", 10056),
		new ServerInfo(Country.Finland, "localhost", 10057),
		//new ServerInfo(Country.Finland, "hydrogen.thaumatized.com", 10056),
		//new ServerInfo(Country.Finland, "helium.thaumatized.com", 10056),
		//new ServerInfo(Country.Finland, "helium.thaumatized.com", 10057),
		//new ServerInfo(Country.Finland, "lithium.thaumatized.com", 10056),
		//new ServerInfo(Country.Finland, "beryllium.thaumatized.com", 10056),
	];

	public const int maxPlayers = 32;

	[Export]
	OptionButton serverSelect;

	[Export]
	LineEdit nameField,
		ipOrDomainField,
		portField;

	[Export]
	Node levelRoot;

	[Export]
	PackedScene level;

	public override void _Ready()
	{
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
			if (peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Disconnected)
			{
				GD.Print("Failed to host server");
				return;
			}
			Multiplayer.MultiplayerPeer = peer;

			startGame();
		}
		else
		{
			foreach (ServerInfo info in serverInfos)
			{
				Texture2D flag = ResourceLoader.Load<Texture2D>(
					"res://2DTextures/CountryFlags/" + info.country.ToString() + ".png"
				);
				serverSelect.AddIconItem(flag, info.ipOrDomain);
			}

			selectServer(0);
		}

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
		if (peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Disconnected)
		{
			GD.Print("Failed to connect to server");
			return;
		}
		Multiplayer.MultiplayerPeer = peer;
		startGame();
	}

	public void host()
	{
		ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(getPort(), maxPlayers);
		GD.Print(error);
		if (peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Disconnected)
		{
			GD.Print("Failed to host server");
			return;
		}
		Multiplayer.MultiplayerPeer = peer;
		GameManager.localPlayer = new PlayerInfo(Multiplayer.GetUniqueId(), nameField.Text);
		startGame();
	}

	public void startGame()
	{
		GameManager.localPlayer = new PlayerInfo(Multiplayer.GetUniqueId(), nameField.Text);
		if (Multiplayer.IsServer())
		{
			levelRoot.AddChild(level.Instantiate());
		}
		this.QueueFree();
	}

	public void exitGame()
	{
		GetTree().Quit();
	}
}
