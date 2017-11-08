using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;

namespace zf.net
{
    public class ServerNetListener : NetListener
    {
        public override void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {
            throw new NotImplementedException();
        }

        public override void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            throw new NotImplementedException();
        }

        public override void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            throw new NotImplementedException();
        }

        public override void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {
            throw new NotImplementedException();
        }

        public override void OnPeerConnected(NetPeer peer)
        {
            throw new NotImplementedException();
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectReason disconnectReason, int socketErrorCode)
        {
            throw new NotImplementedException();
        }
    }
}
