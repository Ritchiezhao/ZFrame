using System;
using System.Collections.Generic;

using zf.util;

namespace zf.core
{
    public partial class EnvObject : BaseObject
    {
        public RunEnv Env;
        
        //[GenField] to do generate id
        public int Uid = 0;

        public override int GetHashCode()
        {
            //Uid优先为HashCode, Tid次之（有的对象是单例的，也就是说Tid对应唯一对象)
            if (Uid != 0)
                return Uid.GetHashCode();
            return Tid.GetHashCode();
        }

        public virtual void OnInitEnvObject() { }
    }


	public partial class ObjectService : GameService
	{
		int uidSeq = 0;
		
        Dictionary<TID, EnvObject> tidInsts = new Dictionary<TID, EnvObject>();

        public EnvObject CreateObject(TID tid, int specifyUid = 0)
		{
            EnvObject ret;
            if (tidInsts.TryGetValue(tid, out ret)) {
                return ret;
            }

            EnvObject obj = Env.TemplateMgr.CreateObject(tid) as EnvObject;
			if (obj == null) {
				Logger.Error("CreateObject failed: " + tid);
				return null;
			}

			obj.Env = this.Env;
            obj.OnInitEnvObject();

            if (specifyUid != 0)
                obj.Uid = specifyUid;
            else
			    obj.Uid = ++uidSeq;
            
            // singleton
            BaseTemplate tmpl = Env.TemplateMgr.GetTemplate(tid);
            if (tmpl.singletonType == SingletonType.RunEnv) {
                tidInsts.Add(tid, obj);
            }

//todo: #if DEBUG_MODE
            if (uidSeq > Env.template.minUid + 10000000)
                Logger.Error("uid seq is overflow!");
//#endif
			return obj;
		}


        public T CreateObject<T>(TID tid) where T : EnvObject
        {
            T ret = CreateObject(tid) as T;
            if (ret == null) {
                Logger.Error("CreateObject failed: " + tid);
                return null;
            }

            return ret;
        }

        public override void Start()
        {
            base.Start();
            uidSeq = Env.template.minUid;
        }
	}
}
