using System;
using System.Collections.Generic;
using zf.util;
using LiteNetLib;

namespace zf.core
{
    public partial class GameNetService : GameService
    {
        protected NetBase netBase;

        protected INetEventListener netEventListener;

        public override void Start()
        {
            base.Start();
        }

        public override void Update()
        {
            base.Update();
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
