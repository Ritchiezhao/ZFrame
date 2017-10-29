//#define DEBUG_INFO

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace zf.util
{
    public delegate void CreateTemplateFunc(uint typeId, ref BaseTemplate ret);
    public delegate void CreateObjectFunc(uint typeId, TID tid, ref BaseObject ret);


    /// <summary>
    /// Template manager.
    /// </summary>
    public partial class TemplateManager
    {
		Dictionary<TID, BaseTemplate> tmpls = new Dictionary<TID, BaseTemplate>();

        List<CreateTemplateFunc> createTmplFuncs = new List<CreateTemplateFunc>();
        List<CreateObjectFunc> createObjFuncs = new List<CreateObjectFunc>();

        bool loadFinished = false;

        /// <summary>
        /// Set load finished mark;
        /// </summary>
        public void LoadFinished() {
            loadFinished = true;
        }

        /// <summary>
        /// Registers CreateTemplateFunc.
        /// </summary>
        /// <param name="func">Func.</param>
        public void RegisterCreateTemplateFunc(CreateTemplateFunc func)
        {
            createTmplFuncs.Add(func);
        }

        /// <summary>
        /// Registers CreateObjectFunc.
        /// </summary>
        /// <param name="func">Func.</param>
        public void RegisterCreateObjectFunc(CreateObjectFunc func)
        {
            createObjFuncs.Add(func);
        }

        public void CopyRegisteredFuncs(TemplateManager mgr2)
        {
            createTmplFuncs.AddRange(mgr2.createTmplFuncs);
            createObjFuncs.AddRange(mgr2.createObjFuncs);
        }

        /// <summary>
        /// Creates the template.
        /// </summary>
        /// <returns>The template.</returns>
        /// <param name="typeId">Type identifier.</param>
        public BaseTemplate CreateTemplate(uint typeId)
        {
            BaseTemplate ret = null;
            CreateTemplate_Inner(typeId, ref ret);
            if (ret != null)
                return ret;
            
            for (int i = 0; i < createTmplFuncs.Count; ++i) {
                createTmplFuncs[i](typeId, ref ret);
                if (ret != null)
                    return ret;
            }

            Logger.Error("Create Template failed, typeid: {0}", typeId);
            return null;
        }


		/// <summary>
		/// Creates the template.
		/// Auto generate the implement in Templates_impl.cs
		/// </summary>
        static partial void CreateTemplate_Inner(uint id, ref BaseTemplate ret);


        /// <summary>
        /// Load the templates.
        /// Absolute file path.
        /// </summary>
        public void LoadTemplates(string[] binFiles)
        {
            for (int i = 0; i < binFiles.Length; ++i) {
                LoadTemplates(binFiles[i]);
            }
        }

		/// <summary>
		/// Load the templates.
        /// Absolute file path.
		/// </summary>
        public void LoadTemplates(string binFile)
		{
            if (System.IO.File.Exists(binFile)) {
                LoadTemplates(new FileStream(binFile, FileMode.Open, FileAccess.Read));
            }
		}

        /// <summary>
        /// Load templates from bytes.
        /// </summary>
        public void LoadTemplates(byte[] bytes)
        {
            //Logger.Log("bytes len: " + bytes.Length);
            if (bytes != null && bytes.Length > 0) {
                LoadTemplates(new MemoryStream(bytes));
            }
        }

        /// <summary>
        /// Load templates from stream.
        /// </summary>
        public void LoadTemplates(Stream stream)
        {
            if (loadFinished) {
                Logger.Error("Try load templates while TemplateMgr is read-only.");
                return;
            }
            
            if (stream == null) {
                Logger.Warning("LoadTemplates from null stream, ignored");
                return;
            }
            
            // BinaryReader little endian 
            BinaryReader reader = new BinaryReader(stream);
            int count = reader.ReadInt32();
            for (int i = 0; i < count; ++i) {
                #if DEBUG_INFO
                Logger.Log(string.Format("Start Read at offset: {0}", reader.BaseStream.Position));
                #endif
                uint typeId = reader.ReadUInt32();
                bool isRuntimeLink = reader.ReadBoolean();
                if (isRuntimeLink) {
                    int tid = reader.ReadInt32();
                    tmpls[tid] = null;
                } else {
                    BaseTemplate tmpl = CreateTemplate(typeId);
                    if (tmpl == null)
                        continue;
                    
                    tmpl.TypeID = typeId;
                    tmpl.Deserialize(reader);
#if DEBUG_INFO
                    Logger.Log(string.Format("Start Read: {0}", tmpl.tid));
#endif
                    if (tmpls.ContainsKey(tmpl.tid))
                        Logger.Warning("{0} is already loaded, the old one will be discarded.", tmpl.tid);
                    tmpls[tmpl.tid] = tmpl;
                }
            }
        }

        /// <summary>
        /// Load all templates files in directory
        /// Absolute file path.
        /// </summary>
        public void LoadDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath)) {
                LoadTemplates(Directory.GetFiles(dirPath, "*.bytes"));
            }
        }


		/// <summary>
		/// Get the template.
		/// </summary>
		public BaseTemplate GetTemplate(TID id)
		{
			BaseTemplate ret;
			if (tmpls.TryGetValue(id, out ret)) {
				return ret;
			}
			return null;
		}


		/// <summary>
		/// Get the template.
		/// </summary>
		public T GetTemplate<T>(TID id) where T : BaseTemplate
		{
			BaseTemplate ret;
			if (tmpls.TryGetValue(id, out ret))
			{
				return ret as T;
			}
			return null;
		}


        /// <summary>
        /// Link tid to template.
        /// </summary>
        public void LinkTemplate(TID id, TID to)
        {
            BaseTemplate ret;
            if (tmpls.TryGetValue(id, out ret)) {
                // 目前认为template为空的TID都是可以RuntimeLink的
                if (ret == null) {
                    ret = GetTemplate(to);
                    tmpls[id] = ret;
                    // todo: demonyang   singleton
                    if (ret == null) {
                        Logger.Error("LinkTemplate '{0}' to '{1}' failed, {1} doesn't have a template", id, to);
                    }
                }
                else
                    Logger.Error("LinkTemplate failed, {0} was assigned before.", id);
            } else {
                Logger.Error("LinkTemplate failed, {0} is not a linkable TID.", id);
            }
        }




		/// <summary>
		/// Create the object.
		/// Auto generate the implement in Templates_impl.cs.
		/// </summary>
		static partial void CreateObject_Inner(uint typeId, TID tid, ref BaseObject ret);


        /// <summary>
        /// Get object instance of TID.
        /// </summary>
        Dictionary<TID, BaseObject> tidInsts = new Dictionary<TID, BaseObject>();
		/// <summary>
		/// Create binding class instance with tid. if exists.
		/// </summary>
		public BaseObject CreateObject(TID tid)
		{
            BaseObject obj;

            if (tidInsts.TryGetValue(tid, out obj)) {
                return obj;
            }

			BaseTemplate tmpl = GetTemplate(tid);
            if (tmpl == null) {
                Logger.Error("CreateObject failed, tmpl of {0} is null", tid);
                return null;
            }

			CreateObject_Inner(tmpl.TypeID, tid, ref obj);
            if (obj == null) {
                for (int i = 0; i < createObjFuncs.Count; ++i) {
                    createObjFuncs[i](tmpl.TypeID, tid, ref obj);
                    if (obj != null)
                        break;
                }
            }

            if (obj == null) {
                Logger.Error("CreateObject failed, can't create Object of {0}", tid);
                return null;
            }

            if (tmpl.singletonType == SingletonType.TemplateMgr) {
                tidInsts.Add(tid, obj);
            }

			obj.Tid = tid;
			obj.InitTemplate(tmpl);
            obj.OnCreated();
			return obj;
		}

        public Dictionary<TID, BaseTemplate> GetTemplates() { return tmpls; }
	}
}
