using System;
using System.Collections.Generic;
using System.Threading;

using zf.util;

namespace zf.core
{

    public partial class RunEnv : BaseObject
    {
        public int ThreadId
        {
            get; private set;
        }

        public bool StartNow
        {
            get { return this.template.startNow; }
        }

        public int UpdateTick
        {
            get { return this.template.updateTick; }
        }

        public int FixedUpdateTick
        {
            get { return this.template.fixedUpdateTick; }
        }

        public int SleepMinTick
        {
            get { return this.template.sleepMinTick; }
        }

        public GameApp App
        {
            get; private set;
        }

        public Mod Mod
        {
            get; private set;
        }

        private int lastFrameTick;
        private int curFrameTick;
        private int logicTick;

        private int lastFixedUpdateTick = 0;

        public ETimeMode GameTimeMode
        {
            get { return this.template.timeMode; }
        }

        public bool RunInThread
        {
            get { return this.template.runInThread; }
        }

        public bool IsStarted { get; private set; }

        public string Name { get; private set; }

        public bool Quit { get; set; }

        public TemplateManager TemplateMgr
        {
            get; private set;
        }

        public List<GameService> services;

        protected bool isServiceChanged;
        protected List<GameService> runServices;

        public RunEnv()
        {
            IsStarted = false;
            isServiceChanged = false;
            services = new List<GameService>();
            runServices = new List<GameService>();
        }

        public void SetTemplateMgr(TemplateManager mgr)
        {
            this.TemplateMgr = mgr;
        }

        public void SetApp(GameApp app)
        {
            this.App = app;
        }

        public void SetMod(Mod mod)
        {
            this.Mod = mod;
        }

        protected void StartInternal()
        {
            for (int i = 0; i < runServices.Count; ++i) {
                runServices[i].Start();
            }
        }

        protected void ThreadLoop()
        {
            //获取当前线程Id
            ThreadId = Thread.CurrentThread.ManagedThreadId;

            this.StartInternal();

            while (!this.Quit) {
                int tick1 = System.Environment.TickCount;
                this.TickUpdate();
                int deltaTick = System.Environment.TickCount - tick1;
                int sleepTick = UpdateTick - deltaTick;
                if (sleepTick > UpdateTick) {
                    sleepTick = UpdateTick;
                }
                else if (sleepTick < SleepMinTick) {
                    sleepTick = SleepMinTick;
                }
                Thread.Sleep(sleepTick);
            }
            this.Close();
        }

        protected void TickUpdate()
        {
            this.lastFrameTick = this.curFrameTick;
            this.curFrameTick = System.Environment.TickCount;
            this.logicTick += this.UpdateTick;

            if (isServiceChanged) {
                runServices.Clear();
                runServices.AddRange(services);
                isServiceChanged = false;
            }

            for (int i = 0; i < runServices.Count; ++i) {
                runServices[i].Update();
            }

            int maxTickCount = 2;
            int tickCount = 0;
            int nowFixedUpdateTick = System.Environment.TickCount;
            while ((nowFixedUpdateTick - this.lastFixedUpdateTick) >= this.FixedUpdateTick) {
                for (int i = 0; i < runServices.Count; ++i) {
                    runServices[i].FixedUpdate();
                }
                ++tickCount;
                if (tickCount > maxTickCount) {
                    this.lastFixedUpdateTick = nowFixedUpdateTick;
                }
                else {
                    this.lastFixedUpdateTick += this.FixedUpdateTick;
                }
            }

            for (int i = 0; i < runServices.Count; ++i) {
                runServices[i].LateUpdate();
            }
        }


        #region public

        public int GetFrameTime()
        {
            if (GameTimeMode == ETimeMode.LOCKSTEP) {
                return this.logicTick;
            }
            else {
                return System.Environment.TickCount;
            }
        }

        public int GetLastFrameTimeDelta()
        {
            if (GameTimeMode == ETimeMode.LOCKSTEP) {
                return this.UpdateTick;
            }
            else {
                int tickDelta = System.Environment.TickCount - this.lastFrameTick;
                if (tickDelta < 0) {
                    tickDelta += System.Int32.MaxValue;
                }
                return tickDelta;
            }
        }

        public static int GetTimeDelta(int newtime, int lasttime)
        {
            int tickDelta = newtime - lasttime;
            if (tickDelta < 0) {
                tickDelta += System.Int32.MaxValue;
            }
            return tickDelta;
        }

        public GameService AddService(GameService service)
        {
            isServiceChanged = true;
            services.Add(service);
            if (this.IsStarted) {
                service.Start();
            }
            return service;
        }

        public T AddService<T>() where T : GameService, new()
        {
            isServiceChanged = true;
            T service = new T();
            services.Add(service);
            return service;
        }

        public T GetService<T>() where T : GameService
        {
            for (int i = 0; i < services.Count; ++i) {
                GameService service = services[i];
                if (service is T) {
                    return service as T;
                }
            } 
            return null;
        }

        public bool Init()
        {
            bool result = true;

            this.Name = this.template.name.Str;

            if (this.template.services != null) {
                for (int i = 0; i < this.template.services.Length; ++i) {
                    TID tid = this.template.services[i];
                    if (tid != TID.None) {
                        GameService service = App.TemplateMgr.CreateObject(tid) as GameService;
                        if (service != null) {
                            service.Init(this);
                            this.services.Add(service);
                        }
                    }
                }
            }

            this.runServices.AddRange(services);
            this.curFrameTick = System.Environment.TickCount;
            this.lastFrameTick = System.Environment.TickCount;
            this.logicTick = 0;
            this.lastFixedUpdateTick = System.Environment.TickCount;

            return result;
        }

        public void Start()
        {
            if (!IsStarted) {

                IsStarted = true;

                if (this.template.startNow) {
                    if (!this.RunInThread) {
                        this.StartInternal();
                    }
                    else {
                        Thread thread = new Thread(ThreadLoop);
                        thread.Start();
                    }
                }
            }
        }

        public void Reset()
        {
            Logger.Info("Env {0} Reset", this.Name);
            if (isServiceChanged) {
                runServices.Clear();
                runServices.AddRange(services);
                isServiceChanged = false;
            }

            for (int i = 0; i < runServices.Count; ++i) {
                runServices[i].Reset();
            }
        }

        public void Update()
        {
            int deltaTick = System.Environment.TickCount - this.lastFrameTick;
            if (deltaTick > this.UpdateTick) {
                this.TickUpdate();
            }
        }

        public void Close()
        {
            Logger.Info("Env {0} Close", this.Name);
            for (int i = 0; i < runServices.Count; ++i) {
                runServices[i].Shutdown();
            }
            services.Clear();
            runServices.Clear();

            IsStarted = false;
            Quit = true;
        }

        #endregion
    }
}
