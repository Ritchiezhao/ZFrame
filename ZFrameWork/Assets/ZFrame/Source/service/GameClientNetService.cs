using zf.util;
using LiteNetLib;

namespace zf.core
{
    public partial class GameClientNetService : GameNetService
    {

        public override void Start()
        {
            base.Start();

            NetClient client = new NetClient(new ClientNetListener(), "zf.frame");
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

            if (this.netBase != null) {
                this.netBase.PollEvents();
            }
        }

        public override void Shutdown()
        {
            base.Shutdown();
        }
    }
}

