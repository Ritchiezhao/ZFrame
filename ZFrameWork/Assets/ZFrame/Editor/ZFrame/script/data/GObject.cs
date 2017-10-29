//#define DEBUG_INFO

using System;
using System.IO;

using LitJson;

namespace ZFEditor
{

    public class GObject : GData
    {
        public enum SingletonType : byte {
            Undefined = 0,
            None = 1,
            TemplateMgr,
            RunEnv
        }
        
        const uint DefaultCrcMask = 0xFFFFFFFF;

        public string ID;
        public bool isRuntimeLink = false;
        public string Parent;
        public SingletonType singletonType;

        // Used for avoiding CRC collision by specifying the start value in calculation of the ID's CRC.
        public uint CrcMask = DefaultCrcMask;

        // crc is used as the index of this object in runtime, no zero, no repetition //
        public uint crc;

        #region miscs
        // Path to the directory that this GData stores in //
        public string DirPath;
        public string FileName;

        public string patch;
        #endregion

        public GObject(string type)
            :base(type)
        {
        }

        public override bool Equals(object obj)
        {
            GObject b = (GObject)obj;
            if (b == null || string.IsNullOrEmpty(b.ID)) {
                return false;
            }
            return this.ID.Equals(b.ID);
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        /// <summary>
        /// Replaces the macro.
        /// </summary>
        /// <returns>new string</returns>
        public string ReplaceMacro(string str, DataDebugInfo debugInfo)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            int index = 0;
            int numOfMacro = 0;
            string newStr = "";
            while (index < str.Length) {
                int leftIndex = str.IndexOf("##", index, StringComparison.Ordinal);
                if (leftIndex < 0) {
                    if (index > 0)
                        newStr += str.Substring(index);
                    break;
                }

                int rightIndex = str.IndexOf("##", leftIndex + 2, StringComparison.Ordinal);
                if (rightIndex < 0) {
                    GLog.LogError("Can't parse macro in string : " + str + debugInfo);
                    return str;
                }

                numOfMacro += 1;
                newStr += str.Substring(index, leftIndex - index);

                string macroName = str.Substring(leftIndex + 2, rightIndex - leftIndex - 2);
                // todo: 目前仅支持ID宏 其他的后续看情况添加 //
                if (macroName.Equals("ID")) {
                    if (this.ID.IndexOf('.') < 0)
                        newStr += this.ID;
                    else {
                        string[] splits = this.ID.Split('.');
                        newStr += splits[1];
                    }
                } else {
                    GLog.LogError("Unknown macro '{0}' in string '{1}'{2}", macroName, str, debugInfo);
                    return str;
                }

                index = rightIndex + 2;
            }

            if (numOfMacro > 0)
                GLog.Log("ReplaceMacro '{0}' to '{1}' {2}", str, newStr, debugInfo);
            else
                newStr = str;
            return newStr;
        }


        public override GData GetFieldWithInheritance(string fieldName)
        {
            GData ret = this.GetField(fieldName);
            if (ret == null && !string.IsNullOrEmpty(this.Parent)) {
                GObject parent = GDataManager.Instance.GetObj(this.Parent);
                // TODO: check errors
                if (parent == null) {
                    GLog.LogError("Can't find template definition: " + this.Parent + debugInfo);
                    return null;
                }
                ret = parent.GetFieldWithInheritance(fieldName);
            }
            return ret;
        }


        bool prebuilded = false;
        public bool PreBuild()
        {
            if (prebuilded)
                return true;
            prebuilded = true;

            if (!string.IsNullOrEmpty(Parent)) {
                GLog.Log("Build:  " + this.ID);

                GObject parentObj = GDataManager.Instance.GetObj(Parent);
                if (parentObj == null) {
                    GLog.LogError("Parent '{0}' can't be found.{1}", Parent, debugInfo);
                    return false;
                }

                // 0. parent prebuild
                parentObj.PreBuild();

                // 1. Inherite Array、Map ...
                var e = inst_object.GetEnumerator();
                while (e.MoveNext()) {
                    if (e.Current.Value.inheritParent) {
                        GLog.Log("inheriteParent true");
                        GData parentData = parentObj.GetField(e.Current.Key);
                        e.Current.Value.InheriteParent(parentData);
                    }
                }
            }

            return true;
        }

        // todo: demonyang  builded set false after modification.
        bool builded = false;
        public override bool Build()
        {
            if (builded)
                return true;

            builded = true;

            return base.Build();
        }


        public override bool WriteJson(ref JsonData jsonData)
        {
            jsonData = new JsonData();
            jsonData.SetJsonType(JsonType.Object);
            jsonData.Add("Type", this.Type);
            jsonData.Add("ID", this.ID);
            if (this.Parent != null)
                jsonData.Add("Parent", this.Parent);
            return base.WriteJson(ref jsonData);
        }

        public override bool WriteBinary(BinaryWriter writer, GObject inObject)
        {
            GTypeClass tp = (GTypeClass)GTypeManager.Instance.GetType(this.Type);
#if DEBUG_INFO
            GLog.Log("Start Write: {0} at pos {1}", this.ID, writer.BaseStream.Position);
#endif
            writer.Write(tp.crc);
            writer.Write(isRuntimeLink);
            writer.Write(GTIDGenerator.Instance.GetID(this.ID));
            if (isRuntimeLink)
                return true;
            writer.Write((byte)this.singletonType);
            return base.WriteBinary(writer, this);
        }


        private string _category = null;
        public string Category {
            get {
                if (string.IsNullOrEmpty(_category))
                    _category = this.GetType<GTypeClass>().Category;
                return _category;
            }
        }
    }
}
