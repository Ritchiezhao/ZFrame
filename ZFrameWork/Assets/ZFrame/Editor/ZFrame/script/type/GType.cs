using System;
using System.Collections.Generic;
using System.IO;

using LitJson;

namespace ZFEditor
{
    /// <summary>
    /// Tid.
    /// </summary>
	public struct TID
	{
		public string ID;
		public string Type;
	}

    /// <summary>
    /// Compile target.
    /// </summary>
    public enum CompileTarget
    {
        CSharp,
        FlatBuffer
    }

    /// <summary>
    /// Batch info.
    /// </summary>
    public class BatchInfo
    {
        public string name;
        public string code_namespace;
        public string json_types_dir;
        public string cs_output_dir;

        public string fb_temp;
        public string fb_src;
        public string fb_output_dir;

        public override bool Equals(object obj)
        {
            BatchInfo b = obj as BatchInfo;
            return b != null && this.name.Equals(b.name);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }


    public struct TypeDesc
    {
        public enum TT {
            Enum,
            BitField,
            Struct,
            Class
        }
        public TT Tt;
        public CompileTarget CompileTo;
        public bool Gen_Head;
        public bool Gen_Serialize;
        public bool Gen_Deserialize;

        public string Name;
        public string Namespace;
		public string Parent;
        // 用于跳过检查Parent是否存在 临时处理// 
		public string builtinParent;
        public bool isTemplateClass;
		public string Category;
		public string BindingClass;
        public string BindingClassMacro;

        public List<TypeFieldDesc> Fields;
    }


    public struct TypeFieldDesc
    {
        public string Name;
        public string Type;
        public string Category;
        public JsonData Default;
        public string Limit;
    }

    /// <summary>
    /// GType.
    /// </summary>
	public class GType
	{
		#region consts and statics
		public const string Bool = "bool";
        public const string Byte = "byte";
        public const string Short = "short";
        public const string Int = "int";
		public const string Float = "float";
		public const string String = "string";
		public const string TID = "TID";
		public const string Array = "Array";
		public const string List = "List";
        public const string Map = "Map";
        public const string Handle = "Handle";

		protected static string NewLine = "\n";

		public static GType CreateBasicType(string type)
		{
			// TODO:error check
			GType tp = new GType();
			tp.Name = type;
			return tp;
		}

		public static GType CreateFromJson(JsonData data)
		{
			GType ret = null;
			if (data.Keys.Contains("Class")) {
				ret = new GTypeClass();
			}
            else if (data.Keys.Contains("Struct")) {
				ret = new GTypeStruct();
			}
            else if (data.Keys.Contains("Enum")) {
				ret = new GTypeEnum();
			}
            else if (data.Keys.Contains("BitField")) {
				ret = new GTypeBitField();
			}
            else {
                GLog.LogError("Type must be declared as 'Class' or 'Struct' or 'Enum' or 'BitField'. \n" + data.ToJson());
				return null;
			}


			if (ret != null)
            {
                if (data.Keys.Contains("CompileTo"))
                    ret.CompileTo = (CompileTarget)Enum.Parse(typeof(CompileTarget), (string)data["CompileTo"]);
                else
                    ret.CompileTo = CompileTarget.CSharp;
                
                if (ret.ParseJson(data))
                    return ret;
            }

			return null;
		}


		public static GType CreateFromDesc(TypeDesc desc)
		{
			GType ret = null;
			if (desc.Tt == TypeDesc.TT.Class) {
				ret = new GTypeClass();
			}
            else if (desc.Tt == TypeDesc.TT.Struct) {
				ret = new GTypeStruct();
			}
            else if (desc.Tt == TypeDesc.TT.Enum) {
				ret = new GTypeEnum();
			}
            else if (desc.Tt == TypeDesc.TT.BitField) {
				ret = new GTypeBitField();
			}
            else {
                GLog.LogError("Type must be declared as 'Class' or 'Struct' or 'Enum' or 'BitField'. \n" + desc.Tt);
				return null;
			}

            ret.Name = desc.Name;
            ret.Namespace = desc.Namespace;
            ret.CompileTo = desc.CompileTo;
            ret.Gen_Head = desc.Gen_Head;
            ret.Gen_Serialize = desc.Gen_Serialize;
            ret.Gen_Deserialize = desc.Gen_Deserialize;

            if (ret.Parse(desc))
                return ret;

			return null;
		}

        /// <summary>
        /// Is reserved word.
        /// </summary>
        public static bool isReservedWord(string word)
        {
            if (word == null)
                return false;

            if (word.Equals("All")
                || word.Equals("Class")
                || word.Equals("Enum")
                || word.Equals("Bitfield")
                || word.Equals("Struct")
                || word.Equals("None")
                || word.Equals("Null")
                || word.Equals("Handle")
               )
                return false;
            return false;
        }
		#endregion

        // Name
        public string Name;
        public string Namespace;
        public CompileTarget CompileTo;
        public bool Gen_Head;
        public bool Gen_Serialize;
        public bool Gen_Deserialize;
        public BatchInfo batch;

        public string FullName {
            get {
                return Namespace == null ? Name : string.Format("{0}.{1}", Namespace, Name);
            }
        }

		public virtual bool IsEnum()
		{
			return false;
		}

		public virtual bool IsMacro()
		{
			return false;
		}

		public virtual bool IsBitField()
		{
			return false;
		}

		public virtual bool IsStruct()
		{
			return false;
		}

		public virtual bool IsClass()
		{
			return false;
		}

        public virtual bool isEqual(GType b)
        {
            if (b == null)
                return false;
            return this.Name.Equals(b.Name);
        }

		public virtual bool ParseJson(LitJson.JsonData data)
		{
			return false;
		}

		public virtual bool Parse(TypeDesc desc)
		{
			return false;
		}

        public virtual string GetCSName()
        {
            if (Name == null)
                return null;
            if (Name.Equals(GType.String)) {
                return "StringAtom";
            } else {
                return Name;
            }
        }

        public virtual GData ReadData(JsonData jsonData, bool inherit, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GData data = new GData(Name);
            data.inheritParent = inherit;
            data.debugInfo = ownerDebugInfo;

            if (Name.Equals(GType.Bool)) {
                data.Bool = (bool)jsonData;
            }
            else if (Name.Equals(GType.Byte)
                     || Name.Equals(GType.Short)
                     || Name.Equals(GType.Int)) {
                int intVal = 0;
                if (jsonData.IsInt)
                    intVal = (int)jsonData;
                else if (jsonData.IsDouble) {
                    float tmp = (float)jsonData;
                    intVal = (int)tmp;
                    if (tmp > intVal)
                        GLog.LogError(jsonData + " is converted to int!" + data.debugInfo);
                } else if (jsonData.IsString) {
                    string[] splits = ((string)jsonData).Split('.');
                    if (splits.Length >= 2) {
                        GType tp = GTypeManager.Instance.GetType(splits[0]);
                        if (tp.IsEnum()) {
                            GTypeEnum enumTp = tp as GTypeEnum;
                            int val = (int)enumTp.GetVal(splits[1]);
                            if (val != -1)
                                intVal = val;
                            else
                                GLog.LogError("Can't parse {0} while assigning to int field {1}{2}", (string)jsonData, field.Name, data.debugInfo);
                        } else
                            GLog.LogError("Can't parse {0} while assigning to int field {1}{2}", (string)jsonData, field.Name, data.debugInfo);
                    } else {
                        GLog.LogError("Can't parse string \"{0}\" to enum while assigning to int field {1}{2}", (string)jsonData, field.Name, data.debugInfo);
                    }
                }
                else
                    GLog.LogError("(" + jsonData.GetJsonType() + ") is not int" + data.debugInfo);

                if (Name.Equals(GType.Int)) {
                    data.Int = intVal;
                } else if (Name.Equals(GType.Short)) {
                    data.Short = (short)intVal;
                    if (data.Short != intVal) {
                        GLog.LogError("{0} is cast to short {1}", intVal, data.Short);
                    }
                } else if (Name.Equals(GType.Byte)) {
                    data.Byte = (byte)intVal;
                    if (data.Byte != intVal) {
                        GLog.LogError("{0} is cast to byte {1}", intVal, data.Byte);
                    }
                }
            }
            else if (Name.Equals(GType.Float)) {
                if (jsonData.IsInt)
                    data.Float = (int)jsonData;
                else if (jsonData.IsDouble)
                    data.Float = (float)jsonData;
                else
                    GLog.LogError("(" + jsonData.GetJsonType() + ") is not a number" + data.debugInfo);
            }
            else if (Name.Equals(GType.String)) {
                data.String = (string)jsonData;
            }
            else if (Name.Equals(GType.TID)) {
                if (string.IsNullOrEmpty(field.Category)) {
                    data.TID = (string)jsonData;
                } else {
                    // todo: ����Ƿ��Ѿ�����Ϊ Catagory.Name ��ʽ
                    data.TID = field.Category + "." + (string)jsonData;
                }
            }

            return data;
        }

        public virtual GData ReadData(string strData, GStructField field, DataDebugInfo ownerDebugInfo)
        {
            GData data = new GData(Name);
            data.debugInfo = ownerDebugInfo;

            if (Name.Equals(GType.Bool)) {
                bool ret;
                if (bool.TryParse(strData, out ret)) {
                    data.Bool = bool.Parse(strData);
                } else {
                    GLog.LogError("Parse Bool failed: {0}", strData);
                    return null;
                }
            } else if (Name.Equals(GType.Byte)
                  || Name.Equals(GType.Short)
                  || Name.Equals(GType.Int)) {
                int intVal = 0;
                int ret;
                if (int.TryParse(strData, out ret)) {
                    intVal = ret;
                } else {
                    string[] splits = strData.Split('.');
                    if (splits.Length >= 2) {
                        GType tp = GTypeManager.Instance.GetType(splits[0]);
                        if (tp.IsEnum()) {
                            GTypeEnum enumTp = tp as GTypeEnum;
                            int val = (int)enumTp.GetVal(splits[1]);
                            if (val != -1)
                                intVal = val;
                            else
                                GLog.LogError("Can't parse {0} while assigning to int field {1}{2}", strData, field.Name, data.debugInfo);
                        } else
                            GLog.LogError("Can't parse {0} while assigning to int field {1}{2}", strData, field.Name, data.debugInfo);
                    } else {
                        GLog.LogError("Can't parse {0} while assigning to int field {1}{2}", strData, field.Name, data.debugInfo);
                    }
                }

                if (Name.Equals(GType.Int)) {
                    data.Int = intVal;
                } else if (Name.Equals(GType.Short)) {
                    data.Short = (short)intVal;
                    if (data.Short != intVal) {
                        GLog.LogError("{0} is cast to short {1}", intVal, data.Short);
                    }
                } else if (Name.Equals(GType.Byte)) {
                    data.Byte = (byte)intVal;
                    if (data.Byte != intVal) {
                        GLog.LogError("{0} is cast to byte {1}", intVal, data.Byte);
                    }
                }
            } else if (Name.Equals(GType.Float)) {
                float ret;
                if (float.TryParse(strData, out ret)) {
                    data.Float = ret;
                } else {
                    GLog.LogError("(" + strData + ") is not a number" + data.debugInfo);
                    return null;
                }
            } else if (Name.Equals(GType.String)) {
                data.String = strData;
            } else if (Name.Equals(GType.TID)) {
                if (string.IsNullOrEmpty(field.Category)) {
                    data.TID = strData;
                } else {
                    data.TID = field.Category + "." + strData;
                }
            }

            return data;
        }


        public virtual bool WriteJson(ref JsonData jsonData, GData data)
        {
            if (Name.Equals(GType.Bool)) {
                jsonData = new JsonData(data.Bool);
            } else if (Name.Equals(GType.Byte)) {
                jsonData = new JsonData(data.Byte);
            } else if (Name.Equals(GType.Short)) {
                jsonData = new JsonData(data.Short);
            } else if (Name.Equals(GType.Int)) {
                jsonData = new JsonData(data.Int);
            } else if (Name.Equals(GType.Float)) {
                jsonData = new JsonData((double)data.Float);
            } else if (Name.Equals(GType.String)) {
                jsonData = new JsonData(data.String);
            } else if (Name.Equals(GType.TID)) {
                jsonData = new JsonData(data.TID);
            }
            return true;
        }

        public virtual bool WriteBinary(BinaryWriter writer, GData data, GObject inObj)
        {
            if (Name.Equals(GType.Bool)) {
                writer.Write(data.Bool);
            } else if (Name.Equals(GType.Byte)) {
                writer.Write(data.Byte);
            } else if (Name.Equals(GType.Short)) {
                writer.Write(data.Short);
            } else if (Name.Equals(GType.Int)) {
                writer.Write(data.Int);
            } else if (Name.Equals(GType.Float)) {
                writer.Write(data.Float);
            } else if (Name.Equals(GType.String)) {
                SerializeUtil.WriteString(writer, inObj.ReplaceMacro(data.String, data.debugInfo));
            } else if (Name.Equals(GType.TID)) {
                string realTID = inObj.ReplaceMacro(data.TID, data.debugInfo);
                //GObject obj = GDataManager.Instance.GetObj(realTID);
                int id = GTIDGenerator.Instance.GetID(realTID);
                if (id == 0) {
                    GLog.LogError("Can't find the definition of '" + realTID + "'" + data.debugInfo);
                    writer.Write(0);
                } else {
                    writer.Write(id);
                }
            }
            return true;
        }

        public virtual bool WriteDefault(BinaryWriter writer, JsonData defaultVal = null)
		{
			if (Name.Equals(GType.Bool))
			{
                if (defaultVal == null)
                    writer.Write(false);
                else if (defaultVal.IsBoolean)
                    writer.Write((bool)defaultVal);
                else
                    GLog.LogError("Default value is invalid. Skipped. Type: " + this.Name + "  default: " + defaultVal);
				return true;
			}
            else if (Name.Equals(GType.Byte))
			{
                if (defaultVal == null)
                    writer.Write((byte)0);
                else if (defaultVal.IsInt)
                    writer.Write((byte)defaultVal);
                else if (defaultVal.IsDouble)
                    writer.Write((byte)(float)defaultVal);
                else
                    GLog.LogError("Default value is invalid. Skipped. Type: " + this.Name + "  default: " + defaultVal);
				return true;
            } else if (Name.Equals(GType.Short)) {
                if (defaultVal == null)
                    writer.Write((short)0);
                else if (defaultVal.IsInt)
                    writer.Write((short)defaultVal);
                else if (defaultVal.IsDouble)
                    writer.Write((short)(float)defaultVal);
                else
                    GLog.LogError("Default value is invalid. Skipped. Type: " + this.Name + "  default: " + defaultVal);
                return true;
            } else if (Name.Equals(GType.Int)) {
                if (defaultVal == null)
                    writer.Write(0);
                else if (defaultVal.IsInt)
                    writer.Write((int)defaultVal);
                else if (defaultVal.IsDouble)
                    writer.Write((int)(float)defaultVal);
                else
                    GLog.LogError("Default value is invalid. Skipped. Type: " + this.Name + "  default: " + defaultVal);
                return true;
            }
			else if (Name.Equals(GType.Float))
			{
                if (defaultVal == null)
                    writer.Write(0.0f);
                else if (defaultVal.IsInt)
                    writer.Write((float)defaultVal);
                else if (defaultVal.IsDouble)
                    writer.Write((float)defaultVal);
                else
                    GLog.LogError("Default value is invalid. Skipped. Type: " + this.Name + "  default: " + defaultVal);
				return true;
			}
			else if (Name.Equals(GType.String))
			{
                if (defaultVal == null)
                    writer.Write(0);
                else if (defaultVal.IsString)
                    SerializeUtil.WriteString(writer, (string)defaultVal);
                else
                    GLog.LogError("Default value is invalid. Skipped. Type: " + this.Name + "  default: " + defaultVal);
				return true;
			}
			else if (Name.Equals(GType.TID))
			{
                if (defaultVal == null)
                    writer.Write(0);
                else if (defaultVal.IsString)
                    writer.Write(GTIDGenerator.Instance.GetID((string)defaultVal));
                else
                    GLog.LogError("Default value is invalid. Skipped. Type: " + this.Name + "  default: " + defaultVal);
				return true;
			}
			return false;
		}

		public virtual bool GenCode_CS_Head(CodeGenerator gen)
		{
			GLog.LogError("Basic GType!");
			return false;
		}

		public virtual bool GenCode_CS_Impl(CodeGenerator gen)
		{
			GLog.LogError("Basic GType!");
			return false;
		}

        public virtual void GenCode_CS_FieldSerialize(CodeGenerator gen, string varName = null, string[] subTypes = null)
        {
            if (Name.Equals(GType.Bool)) {
                gen.Line("writer.Write({0});", varName);
            } else if (Name.Equals(GType.Byte)) {
                gen.Line("writer.Write({0});", varName);
            } else if (Name.Equals(GType.Short)) {
                gen.Line("writer.Write({0});", varName);
            } else if (Name.Equals(GType.Int)) {
                gen.Line("writer.Write({0});", varName);
            } else if (Name.Equals(GType.Float)) {
                gen.Line("writer.Write({0});", varName);
            } else if (Name.Equals(GType.String)) {
                gen.Line("writer.Write({0}.Str);", varName);
            } else if (Name.Equals(GType.TID)) {
                gen.Line("writer.Write((int){0});", varName);
            }
        }

        public virtual void GenCode_CS_FieldDeserialize(CodeGenerator gen, string left, string varName = null, string[] subTypes = null)
        {
            if (varName == null)
                varName = left;

            if (Name.Equals(GType.Bool)) {
                gen.Line("{0} = reader.ReadBoolean();", left);
            } else if (Name.Equals(GType.Byte)) {
                gen.Line("{0} = reader.ReadByte();", left);
            } else if (Name.Equals(GType.Short)) {
                gen.Line("{0} = reader.ReadInt16();", left);
            } else if (Name.Equals(GType.Int)) {
                gen.Line("{0} = reader.ReadInt32();", left);
            } else if (Name.Equals(GType.Float)) {
                gen.Line("{0} = reader.ReadSingle();", left);
            } else if (Name.Equals(GType.String)) {
                gen.Line("{0} = StringAtom.FromReader(reader);", left);
            } else if (Name.Equals(GType.TID)) {
                gen.Line("{0} = new TID();", left);
                gen.Line("{0}.Deserialize(reader);", varName);
            }
        }


        public virtual bool GenCode_FlatBuffer(CodeGenerator gen)
        {
            GLog.LogError("Basic GType!");
            return false;
        }
	}



}