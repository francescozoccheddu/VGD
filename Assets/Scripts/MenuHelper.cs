using UnityEngine;
using Wheeled.Core;

internal sealed class MenuHelper : MonoBehaviour
{

    public void StartServer()
    {
        GameManager.Instance.StartGameAsServer(9050);
    }

    public void StartClient()
    {
        GameManager.Instance.OnGameRoomDiscovered -= GameRoomDiscovered;
        GameManager.Instance.OnGameRoomDiscovered += GameRoomDiscovered;
        GameManager.Instance.StartServerDiscovery(9050);
    }

    private void GameRoomDiscovered(GameRoomInfo _room)
    {
        GameManager.Instance.StartGameAsClient(_room);
        GameManager.Instance.OnGameRoomDiscovered -= GameRoomDiscovered;
    }
}
