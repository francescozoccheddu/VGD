using LiteNetLib;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetServer : MonoBehaviour, INetServer, INetEventListener
{

    public const int port = 9050;

    private NetManager m_netManager;

    private PlayerController controller;
    public GameObject prefab;

    public void Move(int id, PlayerController.SimulationState simulation, PlayerController.InputState input, float timestep)
    {
        throw new System.NotImplementedException();
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Debug.LogFormat("Connection request. Data = {0}", request.Data);
        if (NetTestChooser.IsServer)
        {
            request.AcceptIfKey("key");
            Debug.Log("Accepted connect request");
        }
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Debug.LogFormat("Network error. SocketError = {0}", socketError);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        Debug.LogFormat("Network receive unconnected. Data = {0}", reader.RawData);
        m_netManager.SendDiscoveryResponse(new byte[0], remoteEndPoint);
        Debug.Log("Sent discovery response");
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.LogFormat("Peer connected. Id = {0}", peer.Id);
        GameObject go = Instantiate(prefab);
        controller = go.GetComponent<PlayerController>();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.LogFormat("Peer disconnected. Id = {0}", peer.Id);
    }

    public void Shoot()
    {
        throw new System.NotImplementedException();
    }

    private void Start()
    {
        m_netManager = new NetManager(this)
        {
            SimulateLatency = true,
            SimulationMaxLatency = 1500,
            DisconnectTimeout = 5000,
            DiscoveryEnabled = true
        };
        if (!m_netManager.Start(port))
        {
            Debug.LogError("Start failed");
            m_netManager = null;
        }
        else
        {
            Debug.Log("Started");
        }
    }

    private void Update()
    {
        m_netManager.PollEvents();
    }
}
