using System;
using System.IO;

using LitJson;


namespace ZFEditor
{
    public enum EnumType : short
    {
        Byte,
        Short,
        Int,
        Long,
    }

	public class GTypeEnum : GType
	{
		public string[] Fields;
        public long[] FieldsValue;

        public string EnumGroup = null;
        public int bitSize;

		public override bool IsEnum()
		{
			return true;
		}

		public override bool ParseJson(JsonData data)
		{
            this.Gen_Head = true;
			this.Name = (string)data["Enum"];

            if (data.Keys.Contains("EnumGroup"))
                this.EnumGroup = (string)data["EnumGroup"];

            if (data.Keys.Contains("BitSize")) {
                this.bitSize = int.Parse((string)data["BitSize"]);
                if (bitSize != 8
                    && bitSize != 16
                    && bitSize != 32
                    && bitSize != 64)
                {
                    GLog.LogError("Enum BitSize must be 8 or 16 or 32 or 64: " + this.Name + "  bit size:" + bitSize);
                    bitSize = 16;
                }
            }
            else
                this.bitSize = 16;
            
			if (data.Keys.Contains("Fields"))
			{
				JsonData fieldsJson = data["Fields"];

				if (!fieldsJson.IsArray)
				{
                    GLog.LogError("'Fields' must be array.\n" + data.ToJson());
					return false;
				}

				this.Fields = new string[fieldsJson.Count];
                this.FieldsValue = new long[fieldsJson.Count];
				for (int i = 0; i < fieldsJson.Count; ++i)
				{
					string jsonDesc = (string)fieldsJson[i];
					if (jsonDesc.IndexOf('=') >= 0)
					{
                        if (!string.IsNullOrEmpty(EnumGroup))
                            GLog.LogError("Enum {0}'s value can't be specified because it's in EnumGroup.", this.Name);
                        
						string[] splits = jsonDesc.Split('=');
						if (splits.Length != 2)
						{
							GLog.LogError("Invalid Enum Item: " + jsonDesc);
							return false;
						}

						this.Fields[i] = splits[0].Trim();
                        long newVal = long.Parse(splits[1].Trim());
						if (i > 0 && newVal <= this.FieldsValue[i-1])
							GLog.LogError("Enum item value must be bigger than previous item. Invalid Item: " + jsonDesc);

                        switch (bitSize) {
                            case 8:
                                this.FieldsValue[i] = (sbyte)newVal;
                                break;
                            case 16:
                                this.FieldsValue[i] = (short)newVal;
                                break;
                            case 32:
                                this.FieldsValue[i] = (int)newVal;
                                break;
                            case 64:
                                this.FieldsValue[i] = (long)newVal;
                                break;
                        }
                        if (this.FieldsValue[i] != newVal) {
                            GLog.LogError("Enum {0}.{1} value cast from {2} to {3}", this.Name, this.Fields[i], newVal, this.FieldsValue[i]);
                        }
					}
					else
					{
						this.Fields[i] = jsonDesc;
                        if (i == 0) {
                            if (!string.IsNullOrEmpty(EnumGroup))
                                this.FieldsValue[i] = GTypeManager.Instance.GetValueOfEnumGroup(EnumGroup);
                            else
                                this.FieldsValue[i] = 0;
                        }
						else
							this.FieldsValue[i] = this.FieldsValue[i - 1] + 1;
					}

                    if (isReservedWord(this.Fields[i])) {
                        GLog.LogError(Fields[i] + " is reserved word.");
                        return false;
                    }
				}

                if (!string.IsNullOrEmpty(EnumGroup))
                    GTypeManager.Instance.SetValueOfEnumGroup(EnumGroup, this.FieldsValue[this.FieldsValue.Length - 1]);
                
				return true;
			}
			else
			{
				this.Fields = new string[0];
				return true;
			}
		}

		public bool Contains(string b)
		{
			foreach (var item in this.Fields)
			{
				if (item.Equals(b))
					return true;
			}
			return false;
		}

        public long GetVal(string key)
		{
			for (int i = 0; i < Fields.Length; ++i)
			{
				if (Fields[i].Equals(key))
					return FieldsValue[i];
			}
			return -1;
		}

        public override GData ReadData(JsonData jsonData, bool inherit, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GData data = new GData(Name);
            data.inheritParent = inherit;
            data.debugInfo = ownerDebugInfo;

            data.Enum = (string)jsonData;
            if (!this.Contains(data.Enum)) {
                GLog.LogError(data.Enum + " is not in " + Name + data.debugInfo);
                return null;
            }
            return data;
        }

        public override GData ReadData(string strData, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GData data = new GData(Name);
            data.debugInfo = ownerDebugInfo;

            data.Enum = strData;
            if (!this.Contains(data.Enum)) {
                GLog.LogError(data.Enum + " is not in " + Name + data.debugInfo);
                return null;
            }
            return data;
        }


        public override bool WriteJson(ref JsonData jsonData, GData data)
        {
            jsonData = new JsonData(data.Enum);
            return true;
        }


        public override bool WriteBinary(BinaryWriter writer, GData data, GObject inObj)
        {
            switch (bitSize) {
                case 8:
                    writer.Write((sbyte)GetVal(data.Enum));
                    break;
                case 16:
                    writer.Write((short)GetVal(data.Enum));
                    break;
                case 32:
                    writer.Write((int)GetVal(data.Enum));
                    break;
                case 64:
                    writer.Write((long)GetVal(data.Enum));
                    break;
            }
            return true;
        }


		public override bool WriteDefault(BinaryWriter writer, JsonData defaultVal)
		{
            long val = 0;
            if (defaultVal != null) {
                if (!defaultVal.IsString) {
                    GLog.LogError("Invalid default value:{0} of {1}. Need string.", defaultVal, Name);
                    return false;
                }

                if (GetVal((string)defaultVal) == -1) {
                    GLog.LogError("Invalid default value:{0} of {1}.", defaultVal, Name);
                    return false;
                }
                val = GetVal((string)defaultVal);
            } else
                val = 0;

            switch (bitSize) {
                case 8:
                    writer.Write((sbyte)val);
                    break;
                case 16:
                    writer.Write((short)val);
                    break;
                case 32:
                    writer.Write((int)val);
                    break;
                case 64:
                    writer.Write((long)val);
                    break;
            }
			return true;
		}


		public override bool GenCode_CS_Head(CodeGenerator gen)
		{
			gen.Line("public enum {0}", this.Name);

            gen.AddIndent("{");
			for (int i = 0; i < Fields.Length; ++i)
			{
				string codeLine = Fields[i] + " = " + FieldsValue[i];
				if (i == Fields.Length - 1)
					gen.Line(codeLine);
				else
					gen.Line(codeLine+ ",");
			}
			gen.RemIndent("}");

			return true;
		}

        public override void GenCode_CS_FieldSerialize(CodeGenerator gen, string varName = null, string[] subTypes = null)
        {
            switch (bitSize) {
                case 8:
                    gen.Line("writer.Write((byte){0});", varName);
                    break;
                case 16:
                    gen.Line("writer.Write((short){0});", varName);
                    break;
                case 32:
                    gen.Line("writer.Write((int){0});", varName);
                    break;
                case 64:
                    gen.Line("writer.Write((long){0});", varName);
                    break;
            }
        }


        public override void GenCode_CS_FieldDeserialize(CodeGenerator gen, string left, string varName = null, string[] subTypes = null)
        {
            if (varName == null)
                varName = left;
            switch (bitSize) {
                case 8:
                    gen.Line("{0} = ({1})reader.ReadInt8();", left, Name);
                    break;
                case 16:
                    gen.Line("{0} = ({1})reader.ReadInt16();", left, Name);
                    break;
                case 32:
                    gen.Line("{0} = ({1})reader.ReadInt32();", left, Name);
                    break;
                case 64:
                    gen.Line("{0} = ({1})reader.ReadInt64();", left, Name);
                    break;
            }
        }


        public override bool GenCode_FlatBuffer(CodeGenerator gen)
        {
            gen.Line("// Enum : {0}", this.Name);
            switch (bitSize) {
                case 8:
                    gen.Line("enum {0} : sbyte", this.Name);
                    break;
                case 16:
                    gen.Line("enum {0} : short", this.Name);
                    break;
                case 32:
                    gen.Line("enum {0} : int", this.Name);
                    break;
                case 64:
                    gen.Line("enum {0} : long", this.Name);
                    break;
            }

            gen.AddIndent("{");
            for (int i = 0; i < Fields.Length; ++i) {
                string codeLine = Fields[i] + " = " + FieldsValue[i];
                if (i == Fields.Length - 1)
                    gen.Line(codeLine);
                else
                    gen.Line(codeLine + ",");
            }
            gen.RemIndent("}");

            return true;
        }
	}

}
