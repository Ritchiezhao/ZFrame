using zf.util;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace zf.net
{
    public class NetListener : BaseObject, INetEventListener
    {
        public virtual void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            throw new NotImplementedException();
        }

        public virtual void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            throw new NotImplementedException();
        }

        public virtual void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            throw new NotImplementedException();
        }

        public virtual void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            throw new NotImplementedException();
        }

        public virtual void OnPeerConnected(NetPeer peer)
        {
            throw new NotImplementedException();
        }

        public virtual void OnPeerDisconnected(NetPeer peer, DisconnectReason disconnectReason, int socketErrorCode)
        {
            throw new NotImplementedException();
        }
    }
}
