using System;

using System.Collections.Generic;
using System.IO;

using File = System.IO.File;

namespace ZFEditor
{
	public class TIDMap
	{
		public Dictionary<string, int> idMap;
	}

    /// <summary>
    /// GTIDG enerator.
    /// todo: 后面需要支持多人、多分支并行开发时保证ID不重复
    /// </summary>
    public class GTIDGenerator
    {
        static GTIDGenerator _Instance;
        public static GTIDGenerator Instance {
            get {
                if (_Instance == null) {
                    _Instance = new GTIDGenerator();
                }
                return _Instance;
            }
        }

        TIDMap record;
        string path;

        public void LoadRecord(string path)
        {
            if (File.Exists(path))
            {
                try {
                    record = LitJson.JsonMapper.ToObject<TIDMap>(File.ReadAllText(path));
                    this.path = path;
                }
                catch (Exception ex)
                {
                    GLog.LogError("Exception while reading {0} :\n{1}", path, ex);
                }
            }
            else
            {
                this.path = path;
                record = new TIDMap();
                record.idMap = new Dictionary<string, int>();
            }
        }


        public void WriteRecord()
        {
            if (File.Exists(path))
                File.Delete(path);
            File.WriteAllText(path, LitJson.JsonMapper.ToJson(record));
        }


        public int GetID(string str, bool autoGen = false)
        {
            int ret = 0;
            if (!record.idMap.TryGetValue(str, out ret)) {
                if (autoGen) {
                    // todo: 临时使用CRC32，后面需要支持多人、多分支并行开发时保证ID不重复
                    ret = (int)Crc32.Calc(str);
                    record.idMap[str] = ret;
                } else {
                    GLog.LogError("Template of {0} is not defined.", str);
                }
            }
            return ret;
        }

        public void PrepareTIDForJsonTmpls(string jsonPath)
        {
            if (!File.Exists(jsonPath))
                return;
            
            string jsonStr = File.ReadAllText(jsonPath);
            LitJson.JsonData jsonData;
            try {
                jsonData = LitJson.JsonMapper.ToObject(jsonStr);
            } catch (LitJson.JsonException ex) {
                GLog.LogError("Exception catched while parsing : " + jsonPath + "\n" + ex.Message);
                return;
            }

            if (!jsonData.IsArray)
                return;

            for (int i = 0; i < jsonData.Count; ++i) {
                if (!jsonData[i].Keys.Contains("Type")
                   || !jsonData[i].Keys.Contains("ID")) {
                    return;
                }
                string type = (string)jsonData[i]["Type"];

                // check is class
                GTypeClass tp = GTypeManager.Instance.GetType(type) as GTypeClass;
                if (tp == null) {
                    GLog.LogError("LoadJson. '" + type + "' is not defined or is not class\n");
                    return;
                }

                string finalTID = string.IsNullOrEmpty(tp.Category) ? (string)jsonData[i]["ID"] : (tp.Category + "." + (string)jsonData[i]["ID"]);
                GetID(finalTID, true);
            }
        }
    }
}
