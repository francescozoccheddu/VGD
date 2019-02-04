using LiteNetLib;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetClient : MonoBehaviour, INetClient, INetEventListener
{

    private NetManager m_netManager;

    public GameObject prefab;

    public void Move(PlayerController.SimulationState simulation, PlayerController.InputStroke[] input, float timestep)
    {
        throw new System.NotImplementedException();
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Debug.LogFormat("Connection request. Data = {0}", request.Data);
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Debug.LogFormat("Network error. SocketError = {0}", socketError);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    { }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        Debug.LogFormat("Network receive unconnected. Data = {0}", reader.RawData);
        if (messageType == UnconnectedMessageType.DiscoveryResponse)
        {
            m_netManager.Connect(remoteEndPoint, "key");
            Debug.Log("Sent connect request");
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.LogFormat("Peer connected. Id = {0}", peer.Id);
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
        };
        if (!m_netManager.Start())
        {
            Debug.LogError("Start failed");
            m_netManager = null;
        }
        else
        {
            m_netManager.SendDiscoveryRequest(new byte[0], NetServer.port);
            Debug.Log("Started");
        }
    }

    private void Update()
    {
        m_netManager.PollEvents();
    }
}
