using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;
using Wheeled.Networking;

public sealed class MenuHelper : MonoBehaviour
{

    public Text customClientIPText;

    public void StartServer()
    {
        GameLauncher.Instance.StartGameAsServer(new GameRoomInfo(new IPEndPoint(IPAddress.Loopback, 9050), "", 0));
    }

    public void StartFirstClient()
    {
        GameLauncher.Instance.OnGameRoomDiscovered -= GameRoomDiscovered;
        GameLauncher.Instance.OnGameRoomDiscovered += GameRoomDiscovered;
        GameLauncher.Instance.StartServerDiscovery(9050);
    }

    public void StartCustomClient()
    {
        string ipText = customClientIPText.text;
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipText), 9050);
        GameLauncher.Instance.StartGameAsClient(new GameRoomInfo(ip, "", 0));
    }

    private void GameRoomDiscovered(GameRoomInfo _room)
    {
        GameLauncher.Instance.StartGameAsClient(_room);
        GameLauncher.Instance.OnGameRoomDiscovered -= GameRoomDiscovered;
    }

}
