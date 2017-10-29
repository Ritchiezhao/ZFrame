using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

namespace ZFEditor
{
	public class GDataManager
	{
		static GDataManager _Instance;
		public static GDataManager Instance
		{
            get {
                if (_Instance == null) {
                    _Instance = new GDataManager();
                }
                return _Instance;
            }
        }

		Dictionary<string, GObject> mIDMap = new Dictionary<string, GObject>();

		Dictionary<string, List<GObject>> mTypeMap = new Dictionary<string, List<GObject>>();

		// string cache
		// List<string> mStrings = new List<string>();


        public bool LoadJson(string jsonStr, string dir, string fileName, string patchName)
		{
            GLog.Log("Load json: " + Path.Combine(dir, fileName));
            LitJson.JsonData jsonData;
            try {
                jsonData = LitJson.JsonMapper.ToObject(jsonStr);
            }
            catch (LitJson.JsonException ex) {
                GLog.LogError("Exception catched while parsing : " + Path.Combine(dir, fileName) + "\n" + ex.Message);
                return false;
            }

			for (int i = 0; i < jsonData.Count; ++i)
			{
				if (!jsonData[i].Keys.Contains("Type"))
				{
					GLog.LogError("GDataManager.LoadJson: " + jsonData.ToJson() + " does not contain 'Type' definition");
					return false;
				}
				string type = (string)jsonData[i]["Type"];

                // check is class
                GTypeClass tp = GTypeManager.Instance.GetType(type) as GTypeClass;
                if (tp == null) {
                    GLog.LogError("LoadJson. '" + type + "' is not defined or is not class\n");
                    return false;
                }

                DataDebugInfo info = new DataDebugInfo();
                info.fileName = Path.Combine(dir, fileName);
                GObject obj = tp.ReadData(jsonData[i], false, null, info) as GObject;
                if (obj == null) {
                    continue;
                }

                obj.DirPath = dir;
                obj.FileName = Path.GetFileNameWithoutExtension(fileName);
                obj.patch = patchName;


                GObject oldObj;
                if (mIDMap.TryGetValue(obj.ID, out oldObj)) {
                    if (oldObj.patch.Equals((obj.patch))) {
                        GLog.LogError("GDataManager.LoadJson: Multi definition of " + oldObj.ID);
                        return false;
                    } else {
                        mIDMap[obj.ID] = obj;
                        List<GObject> typeObjs;
                        if (!mTypeMap.TryGetValue(type, out typeObjs)) {
                            typeObjs = new List<GObject>();
                            mTypeMap.Add(type, typeObjs);
                        }
                        typeObjs.Remove(oldObj);
                        typeObjs.Add(obj);
                    }
                } else {
	                mIDMap[obj.ID] = obj;

	                List<GObject> typeObjs;
	                if (!mTypeMap.TryGetValue(type, out typeObjs)) {
	                    typeObjs = new List<GObject>();
	                    mTypeMap.Add(type, typeObjs);
	                }
	                typeObjs.Add(obj);

                }
			}

			return true;
		}

        public bool Build()
        {
            var e = mIDMap.GetEnumerator();
            while (e.MoveNext()) {
                if (!e.Current.Value.PreBuild())
                    return false;
            }


            var e2 = mIDMap.GetEnumerator();
            while (e2.MoveNext()) {
                if (!e2.Current.Value.Build())
                    return false;
            }

            return true;
        }

		public void Clear()
		{
			mIDMap = new Dictionary<string, GObject>();
			mTypeMap = new Dictionary<string, List<GObject>>();
			//mStrings = new List<string>();
		}

		public List<GObject> GetObjsByType(string type)
		{
			List<GObject> ret;
			if (mTypeMap.TryGetValue(type, out ret))
			{
				return ret;
			}
			return null;
		}


		public GObject GetObj(string id)
		{
			if (string.IsNullOrEmpty(id))
				return null;

			GObject ret;
			if (mIDMap.TryGetValue(id, out ret))
				return ret;
			else
				return null;
		}

/*		public int GetStringIndex(string str)
		{
			int index = mStrings.IndexOf(str);
			if (index < 0)
			{
				index = mStrings.Count;
				mStrings.Add(str);
			}
			return index;
		}*/

		#region serialization
		void SortHelper(int index, List<GObject> list, ref bool[] dealed, ref List<GObject> newList)
		{
			if (dealed[index])
				return;
			GObject obj = list[index];
			GObject parent = this.GetObj(obj.Parent);
			if (parent != null)
			{
				int parentIndex = list.IndexOf(parent);
				if (parentIndex >= 0 && !dealed[parentIndex])
					SortHelper(parentIndex, list, ref dealed, ref newList);
			}

			newList.Add(obj);
			dealed[index] = true;
		}

		List<GObject> SortByInheritance(List<GObject> list)
		{
			bool[] dealed = new bool[list.Count];
			List<GObject> newList = new List<GObject>();
			for (int i = 0; i < list.Count; ++i)
			{
				SortHelper(i, list, ref dealed, ref newList);
			}
			return newList;
		}

		// Serialize to Json strings.
		// jsonRet.Key is the target file path with format of "Directory Path + RootType"
		// jsonRet.Value is the serialized Json string.
        // todo: 参数由目录改为全路径
		public bool WriteJson(List<string> dirs, out Dictionary<string, string> jsonRet)
		{
			jsonRet = new Dictionary<string, string>();

			// <path, List<GObject>> //
			Dictionary<string, List<GObject>> tmpObjDic = new Dictionary<string, List<GObject>>();

			foreach (var kvDataList in mTypeMap)
			{
				//string baseType = GTypeManager.Instance.GetRootType(kvDataList.Key);

				foreach (var item in kvDataList.Value)
				{
					if (!dirs.Contains(item.DirPath))
						continue;

					string key = item.DirPath + "/" + GTypeManager.Instance.GetRootType(item.Type);
					List<GObject> tmpList;
					if (!tmpObjDic.TryGetValue(key, out tmpList))
					{
						tmpList = new List<GObject>();
						tmpObjDic.Add(key, tmpList);
					}
					tmpList.Add(item);
				}
			}

			foreach (var itemList in tmpObjDic)
			{
				List<GObject> sorted = SortByInheritance(itemList.Value);
				LitJson.JsonData jsonArray = new LitJson.JsonData();
				jsonArray.SetJsonType(LitJson.JsonType.Array);
				foreach (var item in sorted)
				{
					LitJson.JsonData jsonItem = null;
					item.WriteJson(ref jsonItem);
					jsonArray.Add(jsonItem);
				}

				jsonRet.Add(itemList.Key, jsonArray.ToJson());
			}

			return true;
		}


        public bool WriteBinary(string srcJsonDir, out Dictionary<string, byte[]> ret)
        {
            List<string> dirs = new List<string>();
            dirs.Add(srcJsonDir);
            return WriteBinary(dirs, out ret);
        }

		// 序列化到二进制文件
		// 二进制格式说明
		//    4字节     N字节      
		//   TypeID    Object1  重复以上数据
        public bool WriteBinary(List<string> srcJsonDirs, out Dictionary<string, byte[]> ret)
		{
			// <path, List<GObject>> //
			Dictionary<string, List<GObject>> tmpObjDic = new Dictionary<string, List<GObject>>();

			foreach (var kvDataList in mTypeMap)
			{
				//string baseType = GTypeManager.Instance.GetRootType(kvDataList.Key);

				foreach (var item in kvDataList.Value)
				{
					if (!srcJsonDirs.Contains(item.DirPath))
						continue;

					//string key = item.DirPath + "/" + GTypeManager.Instance.GetRootType(item.Type);
                    string key = item.FileName;
                    if (string.IsNullOrEmpty(key))
                        key = "Common";
                    
					List<GObject> tmpList;
					if (!tmpObjDic.TryGetValue(key, out tmpList))
					{
						tmpList = new List<GObject>();
						tmpObjDic.Add(key, tmpList);
					}
					tmpList.Add(item);
				}
			}

			ret = new Dictionary<string, byte[]>();
			foreach (var itemList in tmpObjDic)
			{
				List<GObject> sorted = SortByInheritance(itemList.Value);
				//List<GObject> sorted = itemList.Value;
				MemoryStream mem = new MemoryStream();
				BinaryWriter writer = new BinaryWriter(mem);
				writer.Write(sorted.Count);
				foreach (var item in sorted)
				{
                    //GLog.Log("Start Write : " + item.ID + "  pos: " + writer.BaseStream.Position);
                    item.WriteBinary(writer, item);
				}
				ret.Add(itemList.Key, mem.GetBuffer());
			}
			return true;
		}


/*		public void WriteStringIDs(out byte[] utf8Bytes)
		{
			MemoryStream mem = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(mem);
			writer.Write(mStrings.Count);
			// lenOfBytes(4B) utf8Bytes
			foreach (var item in mStrings)
			{
				byte[] utf8 = Encoding.UTF8.GetBytes(item);
				writer.Write(utf8.Length);
				writer.Write(utf8);
			}

			writer.Flush();
			utf8Bytes = mem.ToArray();
		}*/
		#endregion
	}

}

