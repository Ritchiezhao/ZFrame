using zf.util;
using LiteNetLib;
using zf.net;
using zf.msg;
using System.IO;

namespace zf.core
{
    public partial class GameClientNetService : GameNetService
    {
        public override void Start()
        {
            base.Start();

            NetClient client = new NetClient(new ClientNetListener(), GameNetService.APP_CONN_KEY);
            if (client != null) {
                this.netBase = client;
                client.Start();
                NetEndPoint endPoint = new NetEndPoint("", 9050);
                client.Connect(endPoint);
            }
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }

        public void Send(MsgPack msg)
        {
            NetClient client = this.netBase as NetClient;
            if (client == null) {
                return;
            }

            NetPeer peer = client.Peer;
            if (peer == null) {
                return;
            }

            byte[] data = messagePacker.SerializeToByteArray(msg);
            peer.Send(data, SendOptions.ReliableOrdered);
        }
    }
}

