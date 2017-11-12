using System;
using System.Collections.Generic;

namespace zf.core
{
    public interface IMsgDispatcher
    {
        void RegisterMessage();
        void UnregisterMessage();
        void SendMessage();
        void PostMessage();
    }
}
