using System;
using System.Collections.Generic;
using zf.util;
using LiteNetLib;
using zf.net;

namespace zf.core
{
    public partial class GameNetService : GameService
    {
        public const string APP_CONN_KEY = "zf.frame";

        protected NetBase netBase;

        protected INetEventListener netEventListener;

        protected IMessagePacker messagePacker;

        public override void Start()
        {
            base.Start();
            messagePacker = new ProtobufPacker();
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
            if (this.netBase != null) {
                this.netBase.Stop();
            }
            base.Shutdown();
        }


    }
}
