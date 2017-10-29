using System;
using System.IO;

using LitJson;


namespace ZFEditor
{

	public class GTypeMacro : GType
	{
		public class Field
		{
			public string Key;
			public int Val;

			public bool Parse(JsonData data)
			{
				return true;
			}
		}

		public Field[] Fields;

		public override bool IsMacro()
		{
			return true;
		}

		public override bool ParseJson(JsonData data)
		{
			this.Gen_Head = true;
			this.Name = (string)data["Macro"];

			if (data.Keys.Contains("Fields"))
			{
				JsonData fieldsJson = data["Fields"];

				if (!fieldsJson.IsArray || fieldsJson.Count == 0)
				{
                    GLog.LogError("'Fields' is invalid.\n" + data.ToJson());
					return false;
				}

				this.Fields = new Field[fieldsJson.Count];
				int lastEnumValue = -1;
				for (int i = 0; i < fieldsJson.Count; ++i)
				{
					this.Fields[i] = new Field();
					this.Fields[i].Parse(fieldsJson[i]);
					if (!fieldsJson[i].Keys.Contains("Key"))
					{
						GLog.LogError("GType.Parse: " + this.Name + " Enums " + i + " 'Key' is not defined");
						return false;
					}
					this.Fields[i].Key = (string)fieldsJson[i]["Key"];

					if (fieldsJson[i].Keys.Contains("Value"))
					{
						if (!fieldsJson[i]["Value"].IsInt)
						{
							GLog.LogError(this.Name + " Enums " + i + " 'Value' is not an interger");
							return false;
						}
						this.Fields[i].Val = (int)fieldsJson[i]["Value"];
						if (this.Fields[i].Val <= lastEnumValue)
						{
							GLog.LogError(this.Name + " Enums " + i + " 'Value' must be bigger than " + lastEnumValue);
							return false;
						}
						lastEnumValue = this.Fields[i].Val;
					}
					else
						this.Fields[i].Val = ++lastEnumValue;

					GLog.Log("AddNumItem: " + this.Fields[i].Key + "  " + this.Fields[i].Val);
				}

				return true;
			}
			else
			{
				this.Fields = new Field[0];
				return true;
			}

		}

		public override bool WriteDefault(BinaryWriter writer, JsonData defaultVal)
		{
			writer.Write(Fields[0].Val);
			return true;
		}

		public override bool GenCode_CS_Head(CodeGenerator gen)
		{
			gen.Line("// Macro : {0}", this.Name);
			gen.Line("public class {0}", this.Name);
			gen.AddIndent("{");
			for (int i = 0; i < Fields.Length; ++i)
			{
				gen.Line("public const int {0} = {1};", Fields[i].Key, Fields[i].Val);
			}
			gen.RemIndent("}");
			return true;
		}
	}
}
