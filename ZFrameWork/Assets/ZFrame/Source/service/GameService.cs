using System;
using System.Collections.Generic;
using zf.util;

namespace zf.core
{

    public partial class GameService : BaseObject
	{
        private RunEnv env;

        public RunEnv Env {
            get {
                return env;
            }
        }

        public GameService()
        {
        }

        public virtual void Init(RunEnv env)
        {
            this.env = env;
        }

     
        void RegisterMessages()
        {
        }

        //启动系统
		public virtual void Start ()
		{
            RegisterMessages();
		}

        //软重启系统
		public virtual void Reset ()
		{
		}

        //更新
		public virtual void Update ()
		{
		}

        //固定帧率更新
		public virtual void FixedUpdate()
		{
		}

        //更新后调用
		public virtual void LateUpdate()
		{
		}

        //关闭系统
		public virtual void Shutdown ()
		{
		}
	}

}