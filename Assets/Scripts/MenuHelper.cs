using UnityEngine;
using Wheeled.Core;

internal sealed class MenuHelper : MonoBehaviour
{

    public void StartServer()
    {
        GameManager.Instance.StartGameAsServer();
    }

    public void StartClient()
    {
        GameManager.Instance.StartServerDiscovery();
    }

}
