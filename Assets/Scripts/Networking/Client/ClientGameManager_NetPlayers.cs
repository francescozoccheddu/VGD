namespace Wheeled.Networking.Client
{

    internal sealed partial class ClientGameManager
    {

        private sealed class NetPlayer
        {

            public int id;
            public readonly NetworkManager.Peer peer;

            public NetPlayer(NetworkManager.Peer _peer)
            {
                peer = _peer;
            }

        }


    }

}
