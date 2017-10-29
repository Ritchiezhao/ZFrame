using System;
using System.Collections.Generic;
using System.IO;

using LitJson;


namespace ZFEditor
{
    public class GTypeArray : GType
    {
        public bool isList;

        public GTypeArray(bool isList = false)
        {
            this.isList = isList;
        }

        public override GData ReadData(JsonData jsonData, bool inherit, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GData data = new GData(GType.Array);
            data.inheritParent = inherit;
            data.debugInfo = ownerDebugInfo;

            data.fieldInfo = field;
            data.Array = new List<GData>();
            
            GType itemType = GTypeManager.Instance.GetType(field.SubTypes[0]);
            
            // 完整数组 //
            if (jsonData.IsArray) {
                for (int iSub = 0; iSub < jsonData.Count; ++iSub) {
                    GData arrayItem = itemType.ReadData(jsonData[iSub], false, field, data.debugInfo);
                    if (arrayItem == null)
                        return null;
                    data.Array.Add(arrayItem);
                }
                return data;
            }
            // 指定index的数组 //
            else if (jsonData.IsObject) {
                data.isKVArray = true;

                var keys = jsonData.Keys;
                int lastIndex = -1;
                var e = keys.GetEnumerator();
                while (e.MoveNext()) {
                    int index = 0;
                    if (!int.TryParse(e.Current, out index)) {
                        GLog.LogError("Array index must be integer. '{0}' {1}", e.Current, data.debugInfo);
                        return null;
                    }

                    if (index <= lastIndex) {
                        GLog.LogError("Array index must be incremental. '{0}' {1}", e.Current, data.debugInfo);
                        return null;
                    }

                    while (lastIndex++ < index - 1)
                        data.Array.Add(null);

                    GData arrayItem = itemType.ReadData(jsonData[e.Current], false, field, data.debugInfo);
                    if (arrayItem == null)
                        return null;
                    data.Array.Add(arrayItem);
                }

                return data;
            }
            else {
                GLog.LogError("Field '{0}' expect array data.{1}", field.Name, data.debugInfo);
                return null;
            }
        }

        public override bool WriteJson(ref JsonData jsonData, GData data)
        {
            jsonData = new JsonData();
            jsonData.SetJsonType(JsonType.Array);

            for (int iSub = 0; iSub < data.Count; ++iSub) {
                JsonData item = new JsonData();
                data.Array[iSub].WriteJson(ref item);
                jsonData.Add(item);
            }
            return true;
        }

        public override bool WriteBinary(BinaryWriter writer, GData data, GObject inObj)
        {
            writer.Write(data.Count);

            for (int iSub = 0; iSub < data.Count; ++iSub) {
                if (data.Array[iSub] == null) {
                    GLog.LogError("Array item is null! in {1}:{0}", data.fieldInfo.Name, inObj.ID);
                    return false;
                }
                data.Array[iSub].WriteBinary(writer, inObj);
            }
            return true;
        }

        public override bool WriteDefault(BinaryWriter writer, JsonData defaultVal = null)
        {
            writer.Write(0);
            return true;
        }


        public override void GenCode_CS_FieldSerialize(CodeGenerator gen, string varName = null, string[] subTypes = null)
        {
            GType subTp1 = GTypeManager.Instance.GetType(subTypes[0]);
            if (isList)
                gen.Line("int len_{0} = {0}.Count;", varName);
            else
                gen.Line("int len_{0} = {0}.Length;", varName);
            
            gen.Line("writer.Write(len_{0});", varName);
            gen.Line("for (int i_{0} = 0; i_{0} < len_{0}; ++i_{0})", varName);
            gen.AddIndent("{");
            subTp1.GenCode_CS_FieldSerialize(gen, string.Format("{0}[i_{0}]", varName));
            gen.RemIndent("}");
        }
        

        public override void GenCode_CS_FieldDeserialize(CodeGenerator gen, string left, string varName = null, string[] subTypes = null)
        {
            if (varName == null)
                varName = left;
            GType subTp1 = GTypeManager.Instance.GetType(subTypes[0]);
            gen.Line("int len_{0} = reader.ReadInt32();", varName);

            if (isList)
                gen.Line("{0} = new List<{1}>();", left, subTp1.GetCSName());
            else
                gen.Line("{0} = new {1}[len_{0}];", left, subTp1.GetCSName());

            gen.Line("for (int i_{0} = 0; i_{0} < len_{0}; ++i_{0})", varName);
            gen.AddIndent("{");
            subTp1.GenCode_CS_FieldDeserialize(gen, string.Format("{0}[i_{0}]", varName));
            gen.RemIndent("}");
        }
    }
}
