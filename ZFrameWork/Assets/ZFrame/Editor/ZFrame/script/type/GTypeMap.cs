using System;
using System.Collections.Generic;
using System.IO;

using LitJson;


namespace ZFEditor
{
    public class GTypeMap : GType
    {
        public override GData ReadData(JsonData jsonData, bool inherit, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GData data = new GData(GType.Map);
            data.inheritParent = inherit;
            data.debugInfo = ownerDebugInfo;
            data.fieldInfo = field;
            data.Map = new List<MapKV>();

            GType keyType = GTypeManager.Instance.GetType(field.SubTypes[0]);
            GType valType = GTypeManager.Instance.GetType(field.SubTypes[1]);
            
            if (jsonData.IsObject) {
                var keys = jsonData.Keys;
                var e = keys.GetEnumerator();
                while (e.MoveNext()) {
                    GData key = keyType.ReadData(e.Current, field, data.debugInfo);
                    if (key == null)
                        return null;
                    GData val = valType.ReadData(jsonData[e.Current], false, field, data.debugInfo);
                    if (val == null)
                        return null;

                    data.Map.Add(new MapKV() { key = key, val = val});
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
            GLog.LogError("GTypeMap WriteJson is not implemented.");
            return false;
        }

        public override bool WriteBinary(BinaryWriter writer, GData data, GObject inObj)
        {
            writer.Write(data.Count);

            for (int iSub = 0; iSub < data.Count; ++iSub) {
                if (data.Map[iSub] == null) {
                    GLog.LogError("Map item is null!");
                }
                data.Map[iSub].key.WriteBinary(writer, inObj);
                data.Map[iSub].val.WriteBinary(writer, inObj);
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
            GType subTp2 = GTypeManager.Instance.GetType(subTypes[1]);

            gen.Line("writer.Write({0}.Count);", varName);
            gen.Line("var iter_{0} = {0}.GetEnumerator();", varName);
            gen.Line("while (iter_{0}.MoveNext())", varName);
            gen.AddIndent("{");
            subTp1.GenCode_CS_FieldSerialize(gen, string.Format("iter_{0}.Current.Key", varName));
            subTp2.GenCode_CS_FieldSerialize(gen, string.Format("iter_{0}.Current.Value", varName));
            gen.RemIndent("}");
        }

        public override void GenCode_CS_FieldDeserialize(CodeGenerator gen, string left, string varName = null, string[] subTypes = null)
        {
            if (varName == null)
                varName = left;

            GType subTp1 = GTypeManager.Instance.GetType(subTypes[0]);
            GType subTp2 = GTypeManager.Instance.GetType(subTypes[1]);

            gen.Line("int len_{0} = reader.ReadInt32();", varName);
            gen.Line("{0} = new Dictionary<{1}, {2}>();", varName, subTp1.GetCSName(), subTp2.GetCSName());

            gen.Line("for (int i_{0} = 0; i_{0} < len_{0}; ++i_{0})", varName);
            gen.AddIndent("{");
            subTp1.GenCode_CS_FieldDeserialize(gen, string.Format("{0} key", subTp1.GetCSName()), "key");
            subTp2.GenCode_CS_FieldDeserialize(gen, string.Format("{0} val", subTp2.GetCSName()), "val");
            gen.Line("{0}.Add(key, val);", varName);
            gen.RemIndent("}");
        }
    }
}
