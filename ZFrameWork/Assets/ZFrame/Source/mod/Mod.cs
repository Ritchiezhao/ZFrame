using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using zf.util;

namespace zf.core
{
    public partial class Mod : BaseObject
    {
        protected RunEnv ctrlEnv;
       
        public Mod parent;
        protected bool isStarted;
        protected bool isFileCacheLoaded;

        protected Dictionary<StringAtom, FileUri> langFileCache;
        protected Dictionary<StringAtom, FileUri> fileCache;

        public bool IsStarted
        {
            get { return isStarted; }
        }

        public string ModPath
        {
            get { return this.template.name.Str; }
        }

        public string Name
        {
            get { return this.template.name.Str; }
        }

        public int MaxPlayerNum
        {
            get { return this.template.maxPlayerNum; }
        }

        public int TeamNum
        {
            get { return this.template.teams.Length; }
        }

        public int CampRelationNum
        {
            get { return this.template.campRelations.Length; }
        }

        protected TemplateManager templateMgr;

        protected List<RunEnv> mainloopEnvs;
        protected List<RunEnv> threadEnvs;

        public Mod()
        {
            ctrlEnv = null;
            parent = null;
            isStarted = false;
            mainloopEnvs = new List<RunEnv>();
            threadEnvs = new List<RunEnv>();
            isFileCacheLoaded = false;
            templateMgr = null;
        }

        public GameApp App
        {
            get; private set;
        }

        public void SetApp(GameApp app)
        {
            this.App = app;
        }

        public bool Open()
        {
            bool result = true;

            //gameapp init已经初始化了parent
            LoadFileCache();
            LoadTemplates();

            // init env
            TID[] envs = template.configs[App.Tid].envs;
            for (int i = 0; i < envs.Length; ++i) {
                TID tid = envs[i];
                if (tid != TID.None) {
                    // todo: GameApp:templateMgr
                    RunEnv env = App.TemplateMgr.CreateObject(tid) as RunEnv;
                    if (env != null) {
                        env.SetApp(App);
                        env.SetMod(this);
                        env.SetTemplateMgr(templateMgr);
                        env.Init();

                        if (env.RunInThread) {
                            App.AddEnvToDict(env);
                            AddThreadEnv(env);
                        }
                        else {
                            App.AddEnvToDict(env);
                            App.AddEnvToMainloop(env);
                            AddMainloopEnv(env);
                        }
                        //if (env.modService != null) {
                        //    if (env.modService.SetMod(this) <= 0) {
                        //        result = false;
                        //    }
                        //}
                    }
                    else {
                        Logger.Error("init mod: runenv {0} create failed!", tid);
                        result = false;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the file location.
        /// </summary>
        /// <returns>The file location.</returns>
        /// <param name="relativePath">Relative path.</param>
        public FileLoc GetFileLoc(util.StringAtom relativePath)
        {
            FileLoc loc = new FileLoc();
            Mod curMod = this;
            while (curMod != null) {
                loc.uri = curMod.GetFileUri(true, relativePath);
                if (FileUri.UNKNOWN != loc.uri) {
                    loc.modPath = curMod.ModPath;
                    return loc;
                }
                loc.uri = curMod.GetFileUri(false, relativePath);
                if (FileUri.UNKNOWN != loc.uri) {
                    loc.modPath = curMod.ModPath;
                    return loc;
                }
                curMod = curMod.parent;
            }
            return loc;
        }

        /// <summary>
        /// Foreachs the files do down to up.
        /// </summary>
        /// <param name="files">Files.</param>
        /// <param name="doFile">Do file.</param>
        public void ForeachFilesDoDownToUp(StringAtom[] files, Action<FileLoc, StringAtom> doFile)
        {
            FileLoc loc = new FileLoc();
            Mod curMod = this;
            List<Mod> stack = new List<Mod>();
            while (curMod != null) {
                stack.Add(curMod);
                curMod = curMod.parent;
            }
            for (int i = stack.Count - 1; i >= 0; --i) {
                Mod mod = stack[i];
                loc.modPath = mod.ModPath;
                for (int j = 0; j < files.Length; ++j) {
                    loc.uri = mod.GetFileUri(true, files[j]);
                    if (loc.uri != FileUri.UNKNOWN) {
                        doFile(loc, files[j]);
                    }
                    loc.uri = mod.GetFileUri(false, files[j]);
                    if (loc.uri != FileUri.UNKNOWN) {
                        doFile(loc, files[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Fors the each file do up to down.
        /// </summary>
        /// <param name="relativePath">Relative path.</param>
        /// <param name="doFile">Do file.</param>
        /// 
        public void ForEachFileDoUpToDown(StringAtom[] files, Action<FileLoc, StringAtom> doFile)
        {
            FileLoc loc = new FileLoc();
            Mod curMod = this;
            while (curMod != null) {
                loc.modPath = curMod.ModPath;
                for (int j = 0; j < files.Length; ++j) {
                    loc.uri = curMod.GetFileUri(true, files[j]);
                    if (loc.uri != FileUri.UNKNOWN) {
                        doFile(loc, files[j]);
                    }
                    loc.uri = curMod.GetFileUri(false, files[j]);
                    if (loc.uri != FileUri.UNKNOWN) {
                        doFile(loc, files[j]);
                    }
                }
                curMod = curMod.parent;
            }
        }

        protected int LoadFileCache(Dictionary<StringAtom, FileUri> cache, FileUri uri, StringAtom path)
        {
            int count = 0;
            FileLoc loc;
            loc.uri = uri;
            loc.modPath = this.ModPath;
            string txt = util.File.LoadTxt(loc, App.Language, path.Str);
            if (txt == null)
                return 0;

            StringReader sr = new StringReader(txt);
            while (true) {
                string line = sr.ReadLine();
                if (line == null) {
                    break;
                }
                string regline = line.Trim();
                if (!string.IsNullOrEmpty(regline)) {
                    StringAtom realpath = StringAtom.FromStr(regline);
                    cache[realpath] = uri;
                    ++count;
                }
            }
            sr.Close();
            return count;
        }

        public int LoadFileCache()
        {
            int count = 0;

            if (!isFileCacheLoaded) {
                if (langFileCache == null) {
                    langFileCache = new Dictionary<StringAtom, FileUri>();
                }
                if (fileCache == null) {
                    fileCache = new Dictionary<StringAtom, FileUri>();
                }

                StringAtom fileCacheName = StringAtom.FromStr("filecache.txt");

                StringAtom langPath = null;
                if (!string.IsNullOrEmpty(App.Language)) {
                    langPath = StringAtom.FromStr(App.Language + "/filecache.txt");
                }

                //resource no lang
                count += LoadFileCache(fileCache, FileUri.RES_DIR, fileCacheName);

                if (langPath != null) {
                    //resource lang
                    count += LoadFileCache(langFileCache, FileUri.RES_LANG_DIR, langPath);
                }

                //steam no lang
                count += LoadFileCache(fileCache, FileUri.STREAM_DIR, fileCacheName);

                if (langPath != null) {
                    //steam lang
                    count += LoadFileCache(langFileCache, FileUri.STREAM_LANG_DIR, langPath);
                }

                //patch no lang
                count += LoadFileCache(fileCache, FileUri.PATCH_DIR, fileCacheName);

                if (langPath != null) {
                    //patch lang
                    count += LoadFileCache(langFileCache, FileUri.PATCH_LANG_DIR, langPath);
                }

                Mod nextMod = parent;
                while (nextMod != null) {
                    nextMod.LoadFileCache();
                    nextMod = nextMod.parent;
                }

                isFileCacheLoaded = true;
            }

            return count;
        }

        /// <summary>
        /// Gets the file location.
        /// </summary>
        /// <returns>The file location.</returns>
        /// <param name="lang">If set to <c>true</c> lang.</param>
        /// <param name="path">Path.</param>
        public FileUri GetFileUri(bool lang, StringAtom path)
        {
            FileUri loc;
            if (lang) {
                if (langFileCache.TryGetValue(path, out loc)) {
                    return loc;
                }
            }
            else {
                if (fileCache.TryGetValue(path, out loc)) {
                    return loc;
                }
            }
            return FileUri.UNKNOWN;
        }

        public void Start()
        {
            if (!isStarted) {

                List<RunEnv> cached = this.mainloopEnvs;
                for (int i = 0; i < cached.Count; ++i) {
                    RunEnv env = cached[i];
                    if (env != null && env.StartNow) {
                        env.Start();
                    }
                }

                List<RunEnv> cachedThread = this.threadEnvs;
                for (int i = 0; i < cachedThread.Count; ++i) {
                    RunEnv env = cachedThread[i];
                    if (env != null && env.StartNow) {
                        env.Start();
                    }
                }

                isStarted = true;
            }
        }

        public void Close()
        {
            if (isStarted) {

                List<RunEnv> cached = this.mainloopEnvs;
                for (int i = 0; i < cached.Count; ++i) {
                    RunEnv env = cached[i];
                    if (env != null && env.IsStarted) {
                        env.Close();
                        App.RemoveEnvFromDict(env);
                        App.RemoveEnvFromMainloop(env);
                    }
                }

                // unload templates
                templateMgr = null;

                isStarted = false;

                // 清除cache
                fileCache = null;
                langFileCache = null;
                isFileCacheLoaded = false;
            }
        }

        public void Reset()
        {
            if (isStarted) {
                List<RunEnv> cached = this.mainloopEnvs;
                for (int i = 0; i < cached.Count; ++i) {
                    RunEnv env = cached[i];
                    if (env != null && env.IsStarted) {
                        env.Reset();
                    }
                }
            }
        }

        /// <summary>
        /// 加载模板
        /// 依赖于FileCache和父节点的FileCache
        /// </summary>
        protected void LoadTemplates()
        {
            if (templateMgr != null) {
                Logger.Warning("templateMgr is already loaded. ignored.");
                return;
            }

            templateMgr = new TemplateManager();
            templateMgr.CopyRegisteredFuncs(App.TemplateMgr);

            // load parent mod templates first
            Mod curMod = this;
            List<Mod> stack = new List<Mod>();
            while (curMod != null) {
                stack.Add(curMod);
                curMod = curMod.parent;
            }

            for (int i = stack.Count - 1; i >= 0; --i) {
                // fileCache
                var enumer = stack[i].fileCache.GetEnumerator();
                while (enumer.MoveNext()) {
                    // 不加载根目录
                    string dir = Path.GetDirectoryName(enumer.Current.Key.Str);
                    if (dir == null || dir == "")
                        continue;

                    if (enumer.Current.Key.Str.EndsWith(".bytes", StringComparison.Ordinal)) {
                        byte[] bytes = util.File.LoadBin(new FileLoc(enumer.Current.Value, stack[i].ModPath), App.Language, enumer.Current.Key.Str);
                        templateMgr.LoadTemplates(bytes);
                    }
                }

                // language fileCache
                var enumer2 = stack[i].langFileCache.GetEnumerator();
                while (enumer2.MoveNext()) {
                    if (enumer2.Current.Key.Str.EndsWith(".bytes", StringComparison.Ordinal)) {
                        byte[] bytes = util.File.LoadBin(new FileLoc(enumer2.Current.Value, stack[i].ModPath), App.Language, enumer2.Current.Key.Str);
                        templateMgr.LoadTemplates(bytes);
                    }
                }
            }

            templateMgr.LoadFinished();
        }


        public void AddMainloopEnv(RunEnv env)
        {
            mainloopEnvs.Add(env);
        }

        public void AddThreadEnv(RunEnv env)
        {
            threadEnvs.Add(env);
        }
    }
}
