using System;
using System.IO;

using LitJson;

namespace ZFEditor
{
	public class GTypeClass : GTypeStruct
	{
		const uint DefaultCrcMask = 0xFFFFFFFF;

		#region fields
		public string Parent;
        // todo:   用于跳过检查Parent是否存在 临时处理// 
		public string builtinParent;
        // todo:   临时处理// 
		public bool isTemplateClass;
		public string Category;
		public string BindingClass;
        public string BindingClassMacro;
        //public bool Multiton;

		// Avoid CRC collision by specifying the start value in calculation of the Name's CRC.
		public uint CrcMask = DefaultCrcMask;

		// crc is used as the index of a custom type in runtime, no zero, no repetition //
		public uint crc;

        protected GStructField[] mInheritedFields;
		public override int FieldCount
		{
			get
			{
				int count = 0;
				if (this.mFields != null)
					count += this.mFields.Length;

				if (this.mInheritedFields != null)
					count += this.mInheritedFields.Length;
				return count;
			}
		}

		public override GStructField GetField(string name)
		{
			foreach (var item in this.mFields)
			{
				if (item.Name.Equals(name))
					return item;
			}

			if (this.mInheritedFields != null)
			{
				foreach (var item in this.mInheritedFields)
				{
					if (item.Name.Equals(name))
						return item;
				}
			}

			return null;
		}

		public override GStructField GetField(int index)
		{
			int curIndex = index;
			if (this.mInheritedFields != null)
			{
				if (index < mInheritedFields.Length)
					return mInheritedFields[index];
				curIndex = index - mInheritedFields.Length;
			}

			if (this.mFields != null && this.mFields.Length > curIndex)
				return this.mFields[curIndex];
			else
				return null;
		}
		#endregion


		#region Inherited Methods
		public override bool IsClass()
		{
			return true;
		}

		public override bool Parse(TypeDesc desc)
		{
			this.Parent = desc.Parent;
			this.builtinParent = desc.builtinParent;
			this.isTemplateClass = desc.isTemplateClass;
			this.Category = desc.Category;
			this.BindingClass = desc.BindingClass;
			this.BindingClassMacro = desc.BindingClassMacro;
			this.crc = Crc32.Calc(this.Name, this.CrcMask);

			return ParseDescFields(desc);
		}

		public override bool ParseJson(JsonData data)
		{
			this.isTemplateClass = true;
			this.Gen_Head = true;
			this.Gen_Serialize = false;
			this.Gen_Deserialize = true;

			JsonData classHeader;
            JsonData classData = data["Class"];
            if (classData.IsObject) {
                classHeader = classData;
                // name
                if (!classHeader.Keys.Contains("Name")) {
                    GLog.LogError(data.ToJson() + "\n 'Name' is required'");
                    return false;
                }
                this.Name = (string)classHeader["Name"];
            } else {
                classHeader = data;
                this.Name = (string)classData;
            }


			// parent
			if (classHeader.Keys.Contains("Parent")) {
				this.Parent = (string)classHeader["Parent"];
			}
			else {
				this.builtinParent = "BaseTemplate";
			}

			// Category
			if (classHeader.Keys.Contains("Category")) {
                if (!string.IsNullOrEmpty(this.Parent)) {
                    GLog.LogError("Category can be declared only in root class. Error in " + this.Name + "\nGType.Parse: \n" + data.ToJson());
                } else {
                    this.Category = (string)classHeader["Category"];
                }
            }

			// Binding class
			if (classHeader.Keys.Contains("BindingClass")) {
				this.BindingClass = (string)classHeader["BindingClass"];
			}
            // Binding class macro
            if (classHeader.Keys.Contains("BindingClassMacro")) {
                this.BindingClassMacro = (string)classHeader["BindingClassMacro"];
            }

            // Multiton
/*            if (classHeader.Keys.Contains("Multiton"))
                this.Multiton = (bool)classHeader["Multiton"];
            else
                this.Multiton = false;*/

			// type id
			// disable CrcMask
			// if (classHeader.Keys.Contains("CrcMask"))
			//    this.CrcMask = (uint)(int)classHeader["CrcMask"];
			this.crc = Crc32.Calc(this.Name, this.CrcMask);

			// TODO: check repeated typeid, in a Mgr.CheckErrors() method

			return ParseJsonFields(data);
		}


        bool inherited = false;
        public bool InheritParentFields()
        {
            if (inherited)
                return true;

            if (this.Parent == null)
                return true;
            
            // check
            GTypeClass parentType = (GTypeClass)GTypeManager.Instance.GetType(this.Parent);
            if (parentType == null || !parentType.IsClass()) {
                GLog.LogError("'" + this.Parent + "' is not defined or is not a 'Class'");
                return false;
            }

            parentType.InheritParentFields();

            // inherit category
            this.Category = parentType.Category;

            // inherit fiedls
            this.mInheritedFields = new GStructField[parentType.FieldCount];
            for (int i = 0; i < this.mInheritedFields.Length; ++i) {
                // TODO: copy or ref ? //
                this.mInheritedFields[i] = parentType.GetField(i);
            }
            inherited = true;
            return true;
        }

        public override GData ReadData(JsonData jsonData, bool inherit, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GObject obj = new GObject(Name);
            obj.inheritParent = inherit;
            obj.debugInfo = ownerDebugInfo;

            // TODO: check errors
            // todo: 检查是否已经定义为 Category.Name 格式
            if (string.IsNullOrEmpty(this.Category))
                obj.ID = (string)jsonData["ID"];
            else
                obj.ID = this.Category + "." + (string)jsonData["ID"];
            
            obj.debugInfo.ownerTID = obj.ID;

            if (jsonData.Keys.Contains("RuntimeLink")) {
                obj.isRuntimeLink = (bool)jsonData["RuntimeLink"];
            }
            
            // todo: 检查是否已经定义为 Category.Name 格式
            if (jsonData.Keys.Contains("Parent")) {
                if (string.IsNullOrEmpty(this.Category))
                    obj.Parent = (string)jsonData["Parent"];
                else
                    obj.Parent = this.Category + "." + (string)jsonData["Parent"];
            }

            if (jsonData.Keys.Contains("Singleton")) {
                try {
                    obj.singletonType = (GObject.SingletonType)Enum.Parse(typeof(GObject.SingletonType), (string)jsonData["Singleton"]);
                }
                catch (Exception ex) {
                    GLog.LogError("Unrecognized SingletonType '{0}' {1}", (string)jsonData["Singleton"], obj.debugInfo);
                }
            }

            if (jsonData.Keys.Contains("CrcMask"))
                obj.CrcMask = (uint)(int)jsonData["CrcMask"];

            obj.crc = Crc32.Calc(obj.ID, obj.CrcMask);

            if (!ReadData_Impl(obj, jsonData, field))
                return null;
            return obj;
        }

		public override bool GenCode_CS_Head(CodeGenerator gen)
		{
			if (!string.IsNullOrEmpty(this.Parent))
				gen.Line("public partial class {0} : {1}", this.Name, this.Parent);
			else if (!string.IsNullOrEmpty(this.builtinParent))
				gen.Line("public partial class {0} : {1}", this.Name, this.builtinParent);
			else
				gen.Line("public partial class {0}", this.Name);

			gen.AddIndent("{");

			if (string.IsNullOrEmpty(this.Parent))
				gen.Line("public const uint TYPE = {0};", this.crc);
			else
				gen.Line("public new const uint TYPE = {0};", this.crc);
			gen.Line();

			for (int i = 0; i < mFields.Length; ++i)
			{
				mFields[i].GenCode_CS_Head(gen);
				if (i != mFields.Length - 1)
					gen.Line();
			}
			gen.RemIndent("}");

			return true;
		}

		public override bool GenCode_CS_Impl(CodeGenerator gen)
		{
			gen.Line("// Class : {0}", this.Name);
			gen.Line("public partial class {0}", this.Name);
			gen.AddIndent("{");

			// serialize method
			if (Gen_Serialize) {
				bool needOverride = (!string.IsNullOrEmpty(this.Parent)) || (!string.IsNullOrEmpty(this.builtinParent));
				if (needOverride) {
					gen.Line("public override void Serialize(BinaryWriter writer)");
					gen.AddIndent("{");
					gen.Line("base.Serialize(writer);");
				} else {
					gen.Line("public virtual void Serialize(BinaryWriter writer)");
					gen.AddIndent("{");
				}

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
				bool needOverride = (!string.IsNullOrEmpty(this.Parent)) || (!string.IsNullOrEmpty(this.builtinParent));
				if (needOverride) {
					gen.Line("public override void Deserialize(BinaryReader reader)");
					gen.AddIndent("{");
					gen.Line("base.Deserialize(reader);");
				} else {
					gen.Line("public virtual void Deserialize(BinaryReader reader)");
					gen.AddIndent("{");
				}

				for (int i = 0; i < mFields.Length; ++i) {
					mFields[i].GenCode_CS_Deserialize(gen);
					if (i != mFields.Length - 1)
						gen.Line();
				}

				gen.RemIndent("}");
			}

			// end class
			gen.RemIndent("}");
			return true;
		}

		#endregion
	}

}