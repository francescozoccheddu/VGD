using UnityEngine;
using Wheeled.Networking;

internal sealed class MenuHelper : MonoBehaviour
{

    public NetworkHostHolder holder;

    public void StartServer()
    {
        holder.InstantiateHost(true);
    }

    public void StartClient()
    {
        holder.InstantiateHost(false);
    }
}
