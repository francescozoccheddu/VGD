using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Wheeled.Core;

internal sealed class MenuHelper : MonoBehaviour
{

    public Text customClientIPText;

    public void StartServer()
    {
        GameManager.Instance.StartGameAsServer(9050);
    }

    public void StartFirstClient()
    {
        GameManager.Instance.OnGameRoomDiscovered -= GameRoomDiscovered;
        GameManager.Instance.OnGameRoomDiscovered += GameRoomDiscovered;
        GameManager.Instance.StartServerDiscovery(9050);
    }

    public void StartCustomClient()
    {
        string ipText = customClientIPText.text;
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse(ipText), 9050);
        GameManager.Instance.StartGameAsClient(new GameRoomInfo { remoteEndPoint = ip });
    }

    private void GameRoomDiscovered(GameRoomInfo _room)
    {
        GameManager.Instance.StartGameAsClient(_room);
        GameManager.Instance.OnGameRoomDiscovered -= GameRoomDiscovered;
    }
}
