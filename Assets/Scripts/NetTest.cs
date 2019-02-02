using LiteNetLib;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetTest : MonoBehaviour, INetEventListener
{

    private const int port = 9050;
    private const string key = "Chiave";

    private NetManager m_netManager;

    public void OnConnectionRequest(ConnectionRequest request)
    {
        Debug.LogFormat("Connection request. Data = {0}", request.Data);
        if (NetTestChooser.IsServer)
        {
            request.AcceptIfKey(key);
            Debug.Log("Accepted connect request");
        }
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Debug.LogFormat("Network error. SocketError = {0}", socketError);
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        Debug.LogFormat("Network latency update. Latency = {0}", latency);
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        Debug.LogFormat("Network receive. Data = {0}", reader.RawData);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        Debug.LogFormat("Network receive unconnected. Data = {0}", reader.RawData);
        if (NetTestChooser.IsServer)
        {
            m_netManager.SendDiscoveryResponse(new byte[] { 123, 45 }, remoteEndPoint);
            Debug.Log("Sent discovery response");
        }
        else
        {
            if (messageType == UnconnectedMessageType.DiscoveryResponse)
            {
                m_netManager.Connect(remoteEndPoint, key);
                Debug.Log("Sent connect request");
            }
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

    private void Start()
    {
        m_netManager = new NetManager(this)
        {
            SimulateLatency = true,
            SimulationMaxLatency = 1500,
            DisconnectTimeout = 5000,
        };
        bool started;
        if (NetTestChooser.IsServer)
        {
            m_netManager.DiscoveryEnabled = true;
            started = m_netManager.Start(port);
        }
        else
        {
            started = m_netManager.Start(port + 1);
            m_netManager.SendDiscoveryRequest(new byte[] { 174, 14 }, port);
        }
        if (!started)
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
        m_netManager?.PollEvents();
    }
}
