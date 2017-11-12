using System;
using System.Collections.Generic;
using System.Threading;

using zf.util;

namespace zf.core
{

    public struct MsgKey
    {
        public int msgType;
        public TID remoteValidatorTid;

        public MsgKey(int msgType, TID remoteValidatorTid)
        {
            this.msgType = msgType;
            this.remoteValidatorTid = remoteValidatorTid;
        }

        public static EqualityComparer comparer = new EqualityComparer();

        public class EqualityComparer : IEqualityComparer<MsgKey>
        {
            public bool Equals(MsgKey x, MsgKey y)
            {
                return (x.msgType == y.msgType && x.remoteValidatorTid == y.remoteValidatorTid);
            }

            public int GetHashCode(MsgKey obj)
            {
                TID tid = obj.remoteValidatorTid;
                uint id = 0;
                if (tid != TID.None) {
                    id = (uint)tid.id;
                }
                //都是从低往高分配的id，移位后冲突更少
                uint objId = (uint)obj.msgType ^ ((id << 16) + (id >> 16));
                return objId.GetHashCode();
            }
        }
    }


    public class MsgDispatcher : IMsgDispatcher
    {
        public void PostMessage()
        {
            throw new NotImplementedException();
        }

        public void RegisterMessage()
        {
            throw new NotImplementedException();
        }

        public void SendMessage()
        {
            throw new NotImplementedException();
        }

        public void UnregisterMessage()
        {
            throw new NotImplementedException();
        }
    }
}