using System;
using LiteNetLib;
using zf.util;

namespace zf.core
{
    public class ClientNetListener : NetListener
    {
        public override void OnPeerConnected(NetPeer peer)
        {
            base.OnPeerConnected(peer);
            Logger.Log("[ClientNetListener]::OnPeerConnected", peer.EndPoint.Host, peer.EndPoint.Port);
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectReason disconnectReason, int socketErrorCode)
        {
            base.OnPeerDisconnected(peer, disconnectReason, socketErrorCode);
            Logger.Log("[ClientNetListener]::OnPeerDisconnected", peer.EndPoint.Host, peer.EndPoint.Port, disconnectReason, socketErrorCode);
        }
    }
}

