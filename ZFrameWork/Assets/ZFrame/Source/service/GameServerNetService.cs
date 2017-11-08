using System;
using LiteNetLib;
using zf.net;

namespace zf.core
{
    public partial class GameServerNetService : GameNetService
    {
        public override void Start()
        {
            base.Start();

            NetServer server = new NetServer(new ServerNetListener(), 100, GameNetService.APP_CONN_KEY);
            if(server!=null) {
                this.netBase = server;
                server.Start(9050);
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
    }
}
