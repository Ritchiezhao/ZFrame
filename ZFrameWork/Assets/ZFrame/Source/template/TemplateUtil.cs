using System;

using System.Text;
using System.IO;
using System.Collections.Generic;

using zf.core;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace zf.util
{

    /// <summary>
    /// log.
    /// </summary>
    public class GBGenLog
    {
        public static void LogError(string str)
        {
#if UNITY_EDITOR
            Debug.LogError("[GBGen Error]: " + str);
#else
            Console.WriteLine("[GBGen Error]: " + str);
#endif
		}
	}

    /// <summary>
    /// TID Map.
    /// </summary>
    public class TIDMap
    {
        public Dictionary<string, int> idMap;
    }


	/// <summary>
	/// Tid.
	/// </summary>
	public struct TID
	{
        static Dictionary<int, string> int2string = null;
        static TIDMap tidMap = null;
        public static bool LoadTIDMap(string path)
        {
            // only load once
            if (tidMap == null) {
              //  try {
                    string json = zf.util.File.LoadTxt(new FileLoc(FileUri.RES_DIR, ""), null, path);
                    if (json == null) {
                        Logger.Error("LoadTIDMap: path:{0} not exist!", path);
                        return false;
                    }

                    tidMap = LitJson.JsonMapper.ToObject<TIDMap>(json);
                    // todo: #if DEBUG_MODE
                    int2string = new Dictionary<int, string>();
                    var iter = tidMap.idMap.GetEnumerator();
                    while(iter.MoveNext()) {
                        int2string.Add(iter.Current.Value, iter.Current.Key);
                    }
                    // todo: #endif
                //} catch (Exception ex) {
                //    Logger.Error("Exception while reading {0} :\n{1}", path, ex);
                //    return false;
                //}
                return true;
            }
            return false;
        }

        public static implicit operator int (TID tid)
        {
            return tid.id;
        }

        public static implicit operator TID (int id)
        {
            return new TID(id);
        }

        public static TID FromString(string val)
        {
            if (tidMap == null) {
                Logger.Error("TIDMap has not been loaded.");
                return TID.None;
            }

            int ret;
            if (tidMap.idMap.TryGetValue(val, out ret)) {
                return new TID(ret);
            }
            else {
                Logger.Error("'{0}' can't be converted to 'TID' because it's not defined.", val); 
                return TID.None;
            }
        }

		public static TID None = new TID(0);

        public int id;
		public TID(int id)
		{
			this.id = id;
		}

		public void Deserialize(BinaryReader reader)
		{
			id = reader.ReadInt32();
		}

		public override int GetHashCode()
		{
			return id;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TID))
				return false;
			TID tid = (TID)obj;
			return tid.id == this.id;
		}

		public override string ToString()
		{
            // todo: #if DEBUG_MODE
            string ret = null;
            if (int2string.TryGetValue(this.id, out ret)) {
                return ret;
            }
            // todo: #endif
            return id.ToString();
		}

		public static bool operator ==(TID obj1, TID obj2)
		{
			return obj1.id == obj2.id;
		}
		public static bool operator !=(TID obj1, TID obj2)
		{
			return !(obj1 == obj2);
		}
	}


    /// <summary>
    /// Singleton type.
    /// </summary>
    public enum SingletonType : byte {
        Undefined = 0,
        None = 1,
        TemplateMgr,
        RunEnv
    }

	/// <summary>
	/// Base template.
	/// </summary>
	public class BaseTemplate : ISGAFSerializable
	{
		public uint TypeID;
		public TID tid;
        public SingletonType singletonType;

		public virtual void Serialize(BinaryWriter writer)
        {
            
        }

		public virtual void Deserialize(BinaryReader reader)
		{
			tid = new TID();
            tid.id = reader.ReadInt32();
            singletonType = (SingletonType)reader.ReadByte();
		}
	}



	/// <summary>
	/// Base object.
	/// </summary>
	public class BaseObject
	{
		public TID Tid { get; set; }
		public virtual void InitTemplate(BaseTemplate tmpl)
		{
		}

        public virtual void OnCreated()
        {
        }
	}



    /// <summary>
    /// Preload strings.
    /// 从strings.map预加载的string列表,解决重复string占用内存问题
    /// </summary>
    public class PreloadStrings
    {
        static string[] mStrings;

        /// <summary>
        /// Load the strings.map
        /// </summary>
        public static void InitStringMap(string path)
        {
            // BinaryReader little endian
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);

            int tmpStrLen = 0;
            int strCount = br.ReadInt32();
            mStrings = new string[strCount];

            for (int curIndex = 0; curIndex < strCount; ++curIndex) {
                tmpStrLen = br.ReadInt32();
                byte[] bytes = br.ReadBytes(tmpStrLen);
                mStrings[curIndex] = Encoding.UTF8.GetString(bytes);
            }
        }

        /// <summary>
        /// Get the string at index.
        /// </summary>
        public static string GetString(int index)
        {
            if (index < 0 || index > mStrings.Length) {
                GBGenLog.LogError("Try to get string with index: " + index);
                return null;
            }
            return mStrings[index];
        }
    }

    /// <summary>
    /// String array.
    /// </summary>
    public class StringArray
	{
		int[] indices;

		public bool Load(BinaryReader br)
		{
			int count = br.ReadInt32();
			// 数组大小暂时限制到10K个，一般count超过10K都是读的错误数据 //
			if (count < 0 || count > 10240)
			{
				GBGenLog.LogError("Array size is invalid: " + count);
				return false;
			}
			indices = new int[count];
			for (int i = 0; i < indices.Length; ++i)
			{
				indices[i] = br.ReadInt32();
			}
			return true;
		}

		public string this[int i]
		{
			get
			{
				if (i < this.indices.Length)
					return PreloadStrings.GetString(this.indices[i]);
				else
					return null;
			}
		}

		public int Length
		{
			get
			{
				return this.indices.Length;
			}
		}
	}


	/// <summary>
	/// Bit field helper.
    /// Return if a contains b.
	/// </summary>
	public class BitField
	{
        /// <summary>
        /// Return if 'a' contains 'b'.
        /// </summary>
		public static bool Contains(long a, long b)
		{
			return (a & b) == b;
		}

		public static bool Intersect(long a, long b)
		{
			return (a & b) != 0;
		}
	}


}
