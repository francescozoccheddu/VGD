using UnityEngine;

public class NetTestChooser : MonoBehaviour
{

    public static bool IsServer { get; private set; }
    private static GameObject instance;

    public static void StartServer()
    {
        IsServer = true;
    }

    public static void StartClient()
    {
        IsServer = false;
    }

    public static INetServer Server => instance.GetComponent<NetServer>();
    public static INetClient Client => instance.GetComponent<NetClient>();

    public void Start()
    {
        instance = gameObject;
        if (IsServer)
        {
            gameObject.AddComponent<NetServer>();
        }
        else
        {
            gameObject.AddComponent<NetClient>();
        }
    }

}
