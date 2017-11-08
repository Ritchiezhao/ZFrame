using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;

using zf.util;
using zf.msg;

namespace zf.core
{
    public enum ModOperCmd
    {
        OPEN,
        CLOSE,
        RESET
    }

    public class ModInfo
    {
        public string name;
        public string tid;
        public string baseMod;
        public string path;

        public bool modTemplateLoaded = false;
    }

    public struct ModOper
    {
        public ModOper(ModOperCmd cmd, string modName)
        {
            this.cmd = cmd; 
            this.modName = modName;
        }
        public ModOperCmd cmd;
        public string modName;
    }

    public partial class GameApp : BaseObject
    {
        private int mainThreadId;

        protected Dictionary<string, ModInfo> modInfos;

        protected List<RunEnv> mainloopEnvs;
        protected List<Mod> runingMods;

        protected Dictionary<string, Mod> name2ModDict;
        protected Dictionary<string, RunEnv> name2EnvDict;
        protected Dictionary<TID, RunEnv> tid2EnvDict;

        public bool IsClosed
        {
            get;
            set;
        }

        public TemplateManager TemplateMgr
        {
            get; private set;
        }

        private string name = "";

        public string Name
        {
            get { return name; }
        }

        public string Language
        {
            get; private set;
        }

        public GameApp()
        {
            IsClosed = false;
            modInfos = new Dictionary<string, ModInfo>();
            mainloopEnvs = new List<RunEnv>();
            runingMods = new List<Mod>();
            name2EnvDict = new Dictionary<string, RunEnv>();
            name2ModDict = new Dictionary<string, Mod>();
            tid2EnvDict = new Dictionary<TID, RunEnv>();
            mainThreadId = Thread.CurrentThread.ManagedThreadId;
            Language = "";
        }

        public void SetTemplateMgr(TemplateManager mgr)
        {
            TemplateMgr = mgr;
        }

        /// <summary>
        /// Adds the env to mainloop.
        /// </summary>
        /// <param name="env">Env.</param>
        public void AddEnvToMainloop(RunEnv env)
        {
            mainloopEnvs.Add(env);
        }

        /// <summary>
        /// Adds the env to dict.
        /// </summary>
        /// <param name="env">Env.</param>
        public void AddEnvToDict(RunEnv env)
        {
            name2EnvDict[env.Name] = env;
            tid2EnvDict[env.Tid] = env;
        }

        /// <summary>
        /// Removes the env from mainloop.
        /// </summary>
        /// <param name="env">Env.</param>
        public void RemoveEnvFromMainloop(RunEnv env)
        {
            mainloopEnvs.Remove(env);
        }

        /// <summary>
        /// Removes the env from dict.
        /// </summary>
        /// <param name="env">Env.</param>
        public void RemoveEnvFromDict(RunEnv env)
        {
            name2EnvDict.Remove(env.Name);
            tid2EnvDict.Remove(env.Tid);
        }

        /// <summary>
        /// Gets the run env.
        /// </summary>
        /// <returns>The run env.</returns>
        /// <param name="envName">Env name.</param>
        public RunEnv GetRunEnv(string envName)
        {
            RunEnv env = null;
            name2EnvDict.TryGetValue(envName, out env);
            return env;
        }

        /// <summary>
        /// Gets the run env.
        /// </summary>
        /// <returns>The run env.</returns>
        /// <param name="tid">Tid.</param>
        public RunEnv GetRunEnv(TID tid)
        {
            RunEnv env = null;
            tid2EnvDict.TryGetValue(tid, out env);
            return env;
        }

        /// <summary>
        /// Gets the mod.
        /// </summary>
        /// <returns>The mod.</returns>
        /// <param name="modName">Mod name.</param>
        public Mod GetMod(string modName)
        {
            Mod mod = null;
            this.name2ModDict.TryGetValue(modName, out mod);
            return mod;
        }

        protected bool LoadModTemplate(ModInfo modInfo, string file)
        {
            if (modInfo.modTemplateLoaded)
                return true;

            if (!string.IsNullOrEmpty(modInfo.baseMod)) {
                ModInfo baseModInfo;
                if (!modInfos.TryGetValue(modInfo.baseMod, out baseModInfo)) {
                    Logger.Error("LoadModTemplate failed, {0}, does't have it's ModInfo.", modInfo.baseMod);
                    return false;
                }

                if (!baseModInfo.modTemplateLoaded) {
                    LoadModTemplate(baseModInfo, file);
                }
            }

            // for patch dir
            if (!string.IsNullOrEmpty(Language)) {
                byte[] bytes = util.File.LoadBin(new FileLoc(FileUri.PATCH_LANG_DIR, modInfo.path), Language, file);
                if (bytes != null) {
                    TemplateMgr.LoadTemplates(bytes);
                    modInfo.modTemplateLoaded = true;
                    return true;
                }
            }

            {
                byte[] bytes = util.File.LoadBin(new FileLoc(FileUri.PATCH_DIR, modInfo.path), Language, file);
                if (bytes != null) {
                    TemplateMgr.LoadTemplates(bytes);
                    modInfo.modTemplateLoaded = true;
                    return true;
                }
            }

            // for stream dir

            if (!string.IsNullOrEmpty(Language)) {
                byte[] bytes = util.File.LoadBin(new FileLoc(FileUri.STREAM_LANG_DIR, modInfo.path), Language, file);
                if (bytes != null) {
                    TemplateMgr.LoadTemplates(bytes);
                    modInfo.modTemplateLoaded = true;
                    return true;
                }
            }

            {
                byte[] bytes = util.File.LoadBin(new FileLoc(FileUri.STREAM_DIR, modInfo.path), Language, file);
                if (bytes != null) {
                    TemplateMgr.LoadTemplates(bytes);
                    modInfo.modTemplateLoaded = true;
                    return true;
                }
            }


            // for res dir
            if (!string.IsNullOrEmpty(Language)) {
                byte[] bytes = util.File.LoadBin(new FileLoc(FileUri.RES_LANG_DIR, modInfo.path), Language, file);
                if (bytes != null) {
                    TemplateMgr.LoadTemplates(bytes);
                    modInfo.modTemplateLoaded = true;
                    return true;
                }
            }

            {
                byte[] bytes = util.File.LoadBin(new FileLoc(FileUri.RES_DIR, modInfo.path), Language, file);
                if (bytes != null) {
                    TemplateMgr.LoadTemplates(bytes);
                    modInfo.modTemplateLoaded = true;
                    return true;
                }
            }

            return false;
        }

        public bool Init()
        {
            bool result = true;

            StringAtom[] modPaths = this.template.modPaths;
            for (int i = 0; i < modPaths.Length; ++i) {
                // load mod info
                string json = zf.util.File.LoadTxt(new FileLoc(FileUri.RES_DIR, modPaths[i].Str), Language, "modinfo.txt");
                if (string.IsNullOrEmpty(json)) {
                    Logger.Error("Can't load modinfo of {0}", modPaths[i].Str);
                    continue;
                }

                ModInfo info = LitJson.JsonMapper.ToObject<ModInfo>(json);
                if (info != null)
                    modInfos[info.name] = info;
                else
                    Logger.Error("{0} context can't be converted to ModInfo.", modPaths[i]);
            }

            return result;
        }

        public bool CreateMod(string modName)
        {
            ModInfo info;
            if (!modInfos.TryGetValue(modName, out info)) {
                Logger.Error("CreateMod failed, can't find ModInfo of {0}", modName);
                return false;
            }

            // 有父mod要先确定父mod是否已加载
            if (!string.IsNullOrEmpty(info.baseMod)) {
                // 
                if (!name2ModDict.ContainsKey(info.baseMod)) {
                    if (!CreateMod(info.baseMod))
                        return false;
                }
            }

            // load mod
            if (!LoadModTemplate(info, "mod_bin.bytes")) {
                Logger.Error("error load mod {0}'s main template", modName);
                return false;
            }

            Mod mod = TemplateMgr.CreateObject(TID.FromString(info.tid)) as Mod;

            if (mod != null) {
                mod.SetApp(this);
                if (!string.IsNullOrEmpty(info.baseMod))
                    mod.parent = this.name2ModDict[info.baseMod];
                this.name2ModDict.Add(modName, mod);
            }
            else {
                Logger.Error("mod {0} create failed!", modName);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Start the specified modName.
        /// 注意此函数不保证同步返回
        /// </summary>
        /// <returns>The start.</returns>
        /// <param name="modName">Mod name.</param>
        public bool Open(string modName)
        {
            bool result = false;

            Mod mod = null;
            if (!this.name2ModDict.TryGetValue(modName, out mod)) {
                if (CreateMod(modName)) {
                    mod = name2ModDict[modName];
                }
                else {
                    Logger.Error("Open Mod failed {0}", modName);
                } 
            }

            if (mod != null) {
                if (mod.Open()) {
                    mod.Start();
                    this.runingMods.Add(mod);
                    if (this.IsClosed) {
                        this.IsClosed = false;
                    }
                    result = true;
                }
                else {
                    Logger.Error("GameApp Start: mod {0} start failed!", mod.Name);
                }
            }
            return result;
        }

        public bool Open()
        {
            bool result = true;
            StringAtom[] launchers = this.template.launchers;
            for (int i = 0; i < launchers.Length; ++i) {
                if (!Open(launchers[i].Str)) {
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Reset the specified modName.
        /// 注意此函数不保证同步返回
        /// </summary>
        /// <returns>The reset.</returns>
        /// <param name="modName">Mod name.</param>
        public void Reset(string modName = null)
        {
            if (string.IsNullOrEmpty(modName)) {
                List<Mod> cached = this.runingMods;
                for (int i = 0; i < cached.Count; ++i) {
                    Mod mod = cached[i];
                    if (mod != null && mod.IsStarted) {
                        mod.Reset();
                    }
                }
            }
            else {
                Mod mod = null;
                if (name2ModDict.TryGetValue(modName, out mod)) {
                    if (mod != null && mod.IsStarted) {
                        mod.Reset();
                    }
                }
            }
        }

        /// <summary>
        /// Close the specified modName.
        /// 注意此函数不保证同步返回
        /// </summary>
        /// <returns>The close.</returns>
        /// <param name="modName">Mod name.</param>
        public void Close(string modName = null)
        {
            if (string.IsNullOrEmpty(modName)) {
                List<Mod> cached = this.runingMods;
                for (int i = 0; i < cached.Count; ++i) {
                    Mod mod = cached[i];
                    if (mod != null && mod.IsStarted) {
                        mod.Close();
                    }
                }
                this.runingMods.Clear();
                IsClosed = true;
            }
            else {
                Mod mod = null;
                if (name2ModDict.TryGetValue(modName, out mod)) {
                    if (mod != null && mod.IsStarted) {
                        mod.Close();
                        this.runingMods.Remove(mod);
                        if (this.runingMods.Count == 0) {
                            IsClosed = true;
                        }
                    }
                }
            }
        }
    }
}
