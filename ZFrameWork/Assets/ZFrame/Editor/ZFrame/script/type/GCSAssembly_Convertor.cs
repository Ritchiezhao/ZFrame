using System;
using System.Collections.Generic;
using System.Reflection;


namespace ZFEditor
{
    public class GCSAssembly_Convertor
    {
        public static List<TypeDesc> Convert(Assembly asm, string[] namespaceFilter)
        {
            List<TypeDesc> list = new List<TypeDesc>();
            foreach (var cstp in asm.GetTypes()) {
                if (namespaceFilter == null || Array.FindIndex(namespaceFilter, it => cstp.Namespace.Equals(it)) >= 0)
                    ConvertSingle(cstp, list);
            }

            return list;
        }

        static string BaseTypeMap(Type cstp)
        {
            if (cstp == typeof(bool)) {
                return "bool";
            } else if (cstp == typeof(int)) {
                return "int";
            } else if (cstp == typeof(short)) {
                return "short";
            } else if (cstp == typeof(float)) {
                return "float";
            } else {
                return cstp.Name;
            }
        }
        
        static bool ConvertSingle(Type cstp, List<TypeDesc> list)
        {
            object[] attrs = cstp.GetCustomAttributes(typeof(ExpandSerializeAttribute), false);
            if (attrs.Length == 0) {
                return false;
            }

            ExpandSerializeAttribute attr = (ExpandSerializeAttribute)attrs[0];

            TypeDesc desc = new TypeDesc();
            TypeDesc subDesc = new TypeDesc();

            desc.Gen_Head = false;
            desc.Gen_Serialize = true;
            desc.Gen_Deserialize = false;
            desc.Tt = TypeDesc.TT.Class;
            desc.CompileTo = CompileTarget.CSharp;
            desc.Name = cstp.Name;
            desc.Namespace = cstp.Namespace;
            desc.Fields = new List<TypeFieldDesc>();

            subDesc.Gen_Head = true;
            subDesc.Gen_Serialize = true;
            subDesc.Gen_Deserialize = true;
            subDesc.Tt = TypeDesc.TT.Class;
            subDesc.CompileTo = CompileTarget.CSharp;
            subDesc.Name = string.Format(attr.subClassName, cstp.Name);
            subDesc.Namespace = cstp.Namespace;
            subDesc.Fields = new List<TypeFieldDesc>();

            object[] parentAttrs = cstp.BaseType.GetCustomAttributes(typeof(ExpandSerializeAttribute), false);
            if (parentAttrs.Length > 0) {
                desc.Parent = cstp.BaseType.Name;
                subDesc.Parent = string.Format(((ExpandSerializeAttribute)parentAttrs[0]).subClassName, cstp.BaseType.Name);
            }

            foreach (var fieldInfo in cstp.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                if (fieldInfo.GetCustomAttributes(typeof(GenFieldAttribute), true).Length > 0) {
                    TypeFieldDesc fieldDesc = new TypeFieldDesc();
                    fieldDesc.Name = fieldInfo.Name;

                    if (fieldInfo.FieldType.IsGenericType) {
                        Type genericType = fieldInfo.FieldType.GetGenericTypeDefinition();
                        string genericTypeName = genericType.Name.Remove(genericType.Name.IndexOf('`'));
                        switch (genericTypeName) {
                            case "List" : {
                                fieldDesc.Type = string.Format("List<{0}>", fieldInfo.FieldType.GetGenericArguments()[0].Name);
                            } break;
                        }
                    }
                    else {
                        fieldDesc.Type = BaseTypeMap(fieldInfo.FieldType);
                    }
                    
                    fieldDesc.Type = BaseTypeMap(fieldInfo.FieldType);
                    desc.Fields.Add(fieldDesc);
                    subDesc.Fields.Add(fieldDesc);
                } else if (fieldInfo.GetCustomAttributes(typeof(GenUidAttribute), true).Length > 0) {
                    TypeFieldDesc fieldDesc = new TypeFieldDesc();
                    TypeFieldDesc subDescField = new TypeFieldDesc();

                    fieldDesc.Name = fieldInfo.Name;
                    subDescField.Name = fieldInfo.Name;

                    if (fieldInfo.FieldType.IsGenericType) {
                        Type genericType = fieldInfo.FieldType.GetGenericTypeDefinition();
                        string genericTypeName = genericType.Name.Remove(genericType.Name.IndexOf('`'));
                        switch (genericTypeName) {
                            case "List" : {
                                fieldDesc.Type = "List<Handle>";
                                subDescField.Type = "Array<int>";
                            } break;
                        }
                    }
                    else {
                        fieldDesc.Type = "Handle";
                        subDescField.Type = "int";
                    }

                    desc.Fields.Add(fieldDesc);
                    subDesc.Fields.Add(subDescField);
                }
            }

            list.Add(desc);
            list.Add(subDesc);

            return true;
        }
    }
}