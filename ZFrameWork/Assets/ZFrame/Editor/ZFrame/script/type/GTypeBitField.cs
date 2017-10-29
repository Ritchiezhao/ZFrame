using System;
using System.IO;

using LitJson;


namespace ZFEditor
{
	public class GTypeBitField : GType
	{
		public string[] Fields;


		public override bool IsBitField()
		{
			return true;
		}

        public override string GetCSName()
        {
            return "long";
        }


		public override bool ParseJson(JsonData data)
		{
			this.Name = (string)data["BitField"];
			this.Gen_Head = true;

			if (data.Keys.Contains("Fields")) {
				JsonData fieldsJson = data["Fields"];

				if (!fieldsJson.IsArray) {
                    GLog.LogError("'Fields' is invalid.\n" + data.ToJson());
					return false;
				}

				this.Fields = new string[fieldsJson.Count];
				for (int i = 0; i < fieldsJson.Count; ++i) {
					this.Fields[i] = (string)fieldsJson[i];
                    if (isReservedWord(this.Fields[i])) {
                        GLog.LogError(Fields[i] + " is reserved word.");
                        return false;
                    }
				}

				return true;
			}
			else {
				this.Fields = new string[0];
				return true;
			}
		}


		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <param name="desc">Desc. Array of bitfield items, split with ','</param>
        public long GetVal(string desc, DataDebugInfo debugInfo)
		{
			string[] splits = desc.Split(',');

			long retVal = 0;

			foreach(var item in splits) {
                if (item.Equals("All")) {
                    return -1; // 0xFFFFFFFFFFFFFFFF
                }
                else {
                    int index = 0;
                    for (; index < Fields.Length; ++index) {
                        if (Fields[index].Equals(item))
                            break;
                    }

    				if (index >= Fields.Length) {
                        GLog.LogError("BitField" + this.Name + " does't contain a '" + item + "'" + debugInfo);
    					return 0;
    				}
                    retVal |= ((long)1 << index);
                }
			}

			return retVal;
		}

        public override GData ReadData(JsonData jsonData, bool inherit, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GData data = new GData(Name);
            data.inheritParent = inherit;
            data.debugInfo = ownerDebugInfo;

            data.BitField = (string)jsonData;
            if (!string.IsNullOrEmpty(data.BitField)) {
                if (GetVal(data.BitField, data.debugInfo) == 0)
                    return null;
            }
            return data;
        }


        public override GData ReadData(string strData, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GData data = new GData(Name);
            data.debugInfo = ownerDebugInfo;

            data.BitField = strData;
            if (!string.IsNullOrEmpty(data.BitField)) {
                if (GetVal(data.BitField, data.debugInfo) == 0)
                    return null;
            }
            return data;
        }

        public override bool WriteJson(ref JsonData jsonData, GData data)
        {
            jsonData = new JsonData(data.BitField);
            return true;
        }


        public override bool WriteBinary(BinaryWriter writer, GData data, GObject inObj)
        {
            writer.Write(GetVal(data.BitField, data.debugInfo));
            return true;
        }

        public override bool WriteDefault(BinaryWriter writer, JsonData defaultVal)
		{
            if (defaultVal != null)
            {
                if (!defaultVal.IsString) {
                    GLog.LogError("Invalid default value:{0} of {1}. Need string.", defaultVal, Name);
                    return false;
                }
                writer.Write(GetVal((string)defaultVal, null));
            }
            else
			    writer.Write((long)0);
			return true;
		}

        public override void GenCode_CS_FieldSerialize(CodeGenerator gen, string varName = null, string[] subTypes = null)
        {
            gen.Line("writer.Write((long){0});", varName);
        }

        public override void GenCode_CS_FieldDeserialize(CodeGenerator gen, string left, string varName = null, string[] subTypes = null)
        {
            if (varName == null)
                varName = left;
            gen.Line("{0} = reader.ReadInt64();", left);
        }

        public override bool GenCode_CS_Head(CodeGenerator gen)
		{
			gen.Line("public class {0}", this.Name);
			gen.AddIndent("{");
			for (int i = 0; i < Fields.Length; ++i)
			{
				gen.Line("public const long " + Fields[i] + " = " + string.Format("0x{0:x16};", (long)1 << i));
			}
			gen.RemIndent("}");
			return true;
		}
	}
}
