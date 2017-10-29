using System;
using System.IO;
using System.Collections.Generic;

using LitJson;

namespace ZFEditor
{
    
	public class GTypeStruct : GType
	{
        protected GStructField[] mFields;

		public virtual int FieldCount
		{
			get
			{
				if (this.mFields == null)
					return 0;
				return this.mFields.Length;
			}
		}

		public override bool IsStruct()
		{
			return true;
		}

		public override bool ParseJson(JsonData data)
		{
			this.Gen_Head = true;
			this.Gen_Serialize = false;
			this.Gen_Deserialize = true;
			this.Name = (string)data["Struct"];
			return ParseJsonFields(data);
		}

		public bool ParseJsonFields(JsonData data)
		{
			if (data.Keys.Contains("Fields"))
			{
				JsonData fieldsJson = data["Fields"];

				if (!fieldsJson.IsArray || fieldsJson.Count == 0)
				{
                    GLog.LogError("'Fields' is invalid.\n" + data.ToJson());
					return false;
				}

                this.mFields = new GStructField[fieldsJson.Count];
				for (int i = 0; i < fieldsJson.Count; ++i)
				{
					this.mFields[i] = new GStructField();
					this.mFields[i].Parse(fieldsJson[i]);
				}

				return true;
			}
			else
			{
				this.mFields = new GStructField[0];
				return true;
			}
		}

		public bool ParseDescFields(TypeDesc desc)
        {
			if (desc.Fields != null && desc.Fields.Count > 0)
			{
                this.mFields = new GStructField[desc.Fields.Count];
				for (int i = 0; i < desc.Fields.Count; ++i)
				{
					this.mFields[i] = new GStructField();
					this.mFields[i].Parse(desc.Fields[i]);
				}

				return true;
			}
			else
			{
				this.mFields = new GStructField[0];
				return true;
			}
        }


        public virtual GStructField GetField(string name)
        {
            foreach (var item in this.mFields) {
                if (item.Name.Equals(name))
                    return item;
            }
            return null;
        }

		public virtual GStructField GetField(int index)
		{
			if (index < mFields.Length)
				return mFields[index];
			else
				return null;
		}

        public override GData ReadData(JsonData jsonData, bool inherit, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GData data = new GData(Name);
            data.inheritParent = inherit;
            data.debugInfo = ownerDebugInfo;

            data.fieldInfo = field;
            if (!ReadData_Impl(data, jsonData, field))
                return null;
            return data;
        }

        protected bool ReadData_Impl(GData data, JsonData jsonData, GStructField field)
        {
            data.Obj = new Dictionary<string, GData>();

            if (!jsonData.IsObject) {
                GLog.LogError("JsonData of {0}({1}) is not a struct {2}", field.Name, this.Name, data.debugInfo);
                return false;
            }
            bool inheritParent = false;
            string fieldName;

            // check if field name is not defined.
            foreach (var key in jsonData.Keys) {
                if (key.Equals("ID")
                   || key.Equals("Type")
                   || key.Equals("Parent")
                   || key.Equals("Singleton")
                    || key.Equals("RuntimeLink")
                   )
                    continue;

                if (key[0] == '+') {
                    GLog.Log("inheritParent  " + key);
                    inheritParent = true;
                    fieldName = key.Substring(1, key.Length - 1);
                } else {
                    fieldName = key;
                }

                GStructField desc = GetField(fieldName);
                if (desc == null) {
                    GLog.LogError("{0} is not a field of {1} {2}", fieldName, this.Name, data.debugInfo);
                    return false;
                }

                GType tp = GTypeManager.Instance.GetType(desc.Type);
                GData subData = tp.ReadData(jsonData[key], inheritParent, desc, data.debugInfo);
                if (subData == null)
                    return false;

                data.Obj.Add(desc.Name, subData);
            }
            return true;
        }

        public override bool WriteJson(ref JsonData jsonData, GData data)
        {
            if (jsonData == null)
                jsonData = new JsonData();
            jsonData.SetJsonType(JsonType.Object);

            for (int i = 0; i < this.FieldCount; ++i) {
                GStructField desc = this.GetField(i);
                if (!data.Obj.Keys.Contains(desc.Name)) {
                    continue;
                }

                JsonData subJsonData = new JsonData();
                data.GetField(desc.Name).WriteJson(ref subJsonData);
                jsonData.Add(desc.Name, subJsonData);
            }
            return true;
        }

        public override bool WriteBinary(BinaryWriter writer, GData data, GObject inObj)
        {
            // todo: check if template field name is defined in type config.

            for (int i = 0; i < FieldCount; ++i) {
                GStructField field = GetField(i);
                GData fieldData = data.GetFieldWithInheritance(field.Name);
                if (fieldData != null) {
                    fieldData.WriteBinary(writer, inObj);
                } else {
                    // write default
                    GType fieldTp = GTypeManager.Instance.GetType(field.Type);
                    if (fieldTp != null) {
                        fieldTp.WriteDefault(writer, field.Default);
                    } else {
                        GLog.LogError("Can't find type definition of " + field.Type + data.debugInfo);
                    }
                }
            }
            return true;
        }

		public override bool WriteDefault(BinaryWriter writer, JsonData defaultVal)
		{
			for (int i = 0; i < this.FieldCount; ++i)
			{
                GType fieldTp = GTypeManager.Instance.GetType(this.GetField(i).Type);
                fieldTp.WriteDefault(writer, null);
			}

			return true;
		}

		public override bool GenCode_CS_Head(CodeGenerator gen)
		{
			gen.Line("public partial class {0}", this.Name);
			gen.AddIndent("{");
			for (int i = 0; i < mFields.Length; ++i)
			{
				mFields[i].GenCode_CS_Head(gen);
				if (i != mFields.Length - 1)
				{
					gen.Line();
				}
			}
			gen.RemIndent("}");
			return true;
		}

		public override bool GenCode_CS_Impl(CodeGenerator gen)
		{
			gen.Line("// Struct : {0}", this.Name);
			gen.Line("public partial class {0}", this.Name);
			gen.AddIndent("{");

			// serialize method
			if (Gen_Serialize) {
				gen.Line("public void Serialize(BinaryWriter writer)");
				gen.AddIndent("{");
				for (int i = 0; i < mFields.Length; ++i) {
					mFields[i].GenCode_CS_Serialize(gen);
					if (i != mFields.Length - 1)
						gen.Line();
				}
				gen.RemIndent("}");
				gen.Line();
			}

			// deserialize method
			if (Gen_Deserialize) {
				gen.Line("public void Deserialize(BinaryReader reader)");
				gen.AddIndent("{");
				for (int i = 0; i < mFields.Length; ++i)
				{
					mFields[i].GenCode_CS_Deserialize(gen);
					if (i != mFields.Length - 1)
						gen.Line();
				}
				gen.RemIndent("}");
			}

			// end
			gen.RemIndent("}");
			return true;
		}

        public override void GenCode_CS_FieldSerialize(CodeGenerator gen, string varName = null, string[] subTypes = null)
        {
            gen.Line("{0}.Serialize(writer);", varName);
        }

        public override void GenCode_CS_FieldDeserialize(CodeGenerator gen, string left, string varName = null, string[] subTypes = null)
        {
            if (varName == null)
                varName = left;
            gen.Line("{0} = new {1}();", left, Name);
            gen.Line("{0}.Deserialize(reader);", varName);
        }

	}
}
