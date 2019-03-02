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
        GameManager.Instance.StartServerDiscovery(9050);
    }

}
