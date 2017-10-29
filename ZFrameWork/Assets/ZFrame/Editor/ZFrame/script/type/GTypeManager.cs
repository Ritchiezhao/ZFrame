using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;


namespace ZFEditor
{

    // TODO: 版本兼容处理 //
    public partial class GTypeManager
    {
        static GTypeManager _Instance;
        public static GTypeManager Instance {
            get {
                if (_Instance == null) {
                    _Instance = new GTypeManager();
                    _Instance.AddBasicTypes();
                }
                return _Instance;
            }
        }

        List<GType> mTypeList = new List<GType>();
        Dictionary<string, GType> mTypeMap = new Dictionary<string, GType>();

        Dictionary<string, long> mEnumGroups = new Dictionary<string, long>();


        void AddBasicTypes()
        {
            mTypeMap.Add(GType.Bool, GType.CreateBasicType(GType.Bool));
            mTypeMap.Add(GType.Byte, GType.CreateBasicType(GType.Byte));
            mTypeMap.Add(GType.Short, GType.CreateBasicType(GType.Short));
            mTypeMap.Add(GType.Int, GType.CreateBasicType(GType.Int));
            mTypeMap.Add(GType.Float, GType.CreateBasicType(GType.Float));
            mTypeMap.Add(GType.String, GType.CreateBasicType(GType.String));
            mTypeMap.Add(GType.TID, GType.CreateBasicType(GType.TID));
            mTypeMap.Add(GType.List, new GTypeArray(true));
            mTypeMap.Add(GType.Array, new GTypeArray());
            mTypeMap.Add(GType.Map, new GTypeMap());
            mTypeMap.Add(GType.Handle, new GTypeHandle());
        }


        /*        public bool LoadYaml(string content)
                {
                    try
                    {
                        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder().Build();
                        var yamlObject = deserializer.Deserialize(new StringReader(content));
                        var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                            .JsonCompatible()
                            .Build();
                    }
                    catch (Exception ex)
                    {
                        GLog.LogError("YAML parse error: " + ex.Message);
                    }

                    var json = serializer.Serialize(yamlObject);
                    return LoadJson(json);
                }
        */

        /// <summary>
        /// Load types json.
        /// 可以多次Load多个json并合并
        /// 最终必须调用Build()
        /// </summary>
        public bool LoadJson(string content, string fileName, BatchInfo batch = null)
        {
            LitJson.JsonData typeList;
            try {
                typeList = LitJson.JsonMapper.ToObject(content);
            }
            catch (Exception ex) {
                GLog.LogError("Exception catched while parsing : " + fileName + "\n" + ex.Message);
                return false;
            }

            for (int i = 0; i < typeList.Count; ++i) {
                GType tp = GType.CreateFromJson(typeList[i]);

                if (tp == null)
                    return false;

                if (mTypeMap.ContainsKey(tp.Name)) {
                    GLog.LogError("GTypeManager.Load: Multi definition of " + tp.Name);
                    return false;
                }
                tp.batch = batch;

                if (tp.Namespace == null) {
                    if (batch != null)
                        tp.Namespace = batch.code_namespace;
                    else
                        tp.Namespace = "zf.util";
                }

                mTypeMap.Add(tp.Name, tp);
                mTypeList.Add(tp);
                //GLog.Log("Finish " + tp.Name + "  isEnum:" + tp.IsEnum() + "\n");
            }
            return true;
        }


        partial void Convert(Assembly asm, BatchInfo batch);


        /// <summary>
        /// Load Types from C# Assembly
        /// </summary>
        public bool LoadFromAssembly(string fileName, BatchInfo batch = null, string[] namespaceFilter = null)
        {
            Assembly asm = Assembly.Load(File.ReadAllBytes(fileName));
            foreach (var desc in GCSAssembly_Convertor.Convert(asm, namespaceFilter)) {
                GType tp = GType.CreateFromDesc(desc);
                if (tp == null)
                    return false;

                if (mTypeMap.ContainsKey(tp.Name)) {
                    GLog.LogError("GTypeManager.Load: Multi definition of " + tp.Name);
                    return false;
                }
                tp.batch = batch;
                mTypeMap.Add(tp.Name, tp);
                mTypeList.Add(tp);
            }
            return true;
        }

        /// <summary>
        /// Build this instance.
        /// 在所有LoadJson()调用完成后调用
        /// </summary>
        public void Build()
        {
            // build parent-children inheritance
            foreach (var tp in mTypeList) {
                if (tp.IsClass()) {
                    GTypeClass tpClass = tp as GTypeClass;
                    tpClass.InheritParentFields();
                }
            }
        }

        public void Clear()
        {
            mTypeMap = new Dictionary<string, GType>();
            mTypeList.Clear();
            AddBasicTypes();
        }

        public List<GType> GetTypeList()
        {
            return mTypeList;
        }

        public GType GetType(string name)
        {
            GType tp;
            if (mTypeMap.TryGetValue(name, out tp)) {
                return tp;
            } else {
                GLog.LogError("GTypeManager GetType, error, can't find type: " + name);
                return null;
            }
        }

        public bool isDerived(string subType, string baseType)
        {
            return isDerived(GetType(subType), GetType(baseType));
        }

        public bool isDerived(GType subType, GType baseType)
        {
            if (subType == null || !subType.IsClass()
                || baseType == null || !baseType.IsClass())
                return false;
            GTypeClass subClassType = (GTypeClass)subType;
            GTypeClass tmp;
            do {
                tmp = (GTypeClass)this.GetType(subClassType.Parent);
            }
            while (tmp != null && !tmp.isEqual(baseType));

            return (tmp != null);
        }

        // get base type inherited from TBase //
        public string GetRootType(string type)
        {
            GTypeClass tmp = (GTypeClass)this.GetType(type);
            while (tmp != null && !string.IsNullOrEmpty(tmp.Parent)) {
                tmp = (GTypeClass)this.GetType(tmp.Parent);
            }

            if (tmp == null) {
                GLog.LogError("Cant find Root Type of " + type);
                return null;
            }

            return tmp.Name;
        }

        public long GetValueOfEnumGroup(string enumgroup)
        {
            long ret;
            if (mEnumGroups.TryGetValue(enumgroup, out ret)) {
                return ret;
            } else {
                mEnumGroups.Add(enumgroup, 0);
                return 0;
            }
        }


        public void SetValueOfEnumGroup(string enumgroup, long val)
        {
            mEnumGroups[enumgroup] = val;
        }


        public bool GenCode_CS_Head(out string code, BatchInfo batch)
        {
            StringWriter writer = new StringWriter();
            CodeGenerator gen = new CodeGenerator(writer);
            gen.Line("// ============================================================================================= //");
            gen.Line("// This is generated by tool. Don't edit this manually.");
            gen.Line("// Encoding: " + writer.Encoding.EncodingName);
            gen.Line("// ============================================================================================= //");
            gen.Line();
            gen.Line();
            gen.Line("using System.Collections.Generic;");
            gen.Line("using zf.util;");
            gen.Line();

            foreach (var tp in mTypeList) {
                if ((tp.batch == null && batch == null) || (tp.batch != null && tp.batch.Equals(batch))) {
                    if (tp.CompileTo == CompileTarget.CSharp && tp.Gen_Head) {
                        gen.Line("// ----------------------------------------------------------------------------");
                        if (!string.IsNullOrEmpty(tp.Namespace)) {
                            gen.Line("namespace {0}", tp.Namespace);
                            gen.AddIndent("{");
                        }

                        tp.GenCode_CS_Head(gen);

                        if (!string.IsNullOrEmpty(tp.Namespace)) {
                            gen.RemIndent("}");
                        }
                        gen.Line();
                    }
                }
            }

            writer.Flush();
            code = writer.ToString();
            return true;
        }


        public bool GenCode_CS_Impl(out string code, BatchInfo batch)
        {
            StringWriter writer = new StringWriter();
            CodeGenerator gen = new CodeGenerator(writer);
            gen.Line("// ============================================================================================= //");
            gen.Line("// This is generated by tool. Don't edit this manually.");
            gen.Line("// Encoding: " + writer.Encoding.EncodingName);
            gen.Line("// ============================================================================================= //");
            gen.Line();
            gen.Line();
            gen.Line("using System.IO;");
            gen.Line("using System.Text;");
            gen.Line("using System.Collections.Generic;");
            gen.Line();
            gen.Line("using zf.util;");
            gen.Line();


            // classes
            foreach (var tp in mTypeList) {
                if ((tp.batch == null && batch == null) || (tp.batch != null && tp.batch.Equals(batch))) {
                    if (tp.CompileTo == CompileTarget.CSharp) {
                        if (tp.IsStruct() || tp.IsClass()) {
                            gen.Line("// ----------------------------------------------------------------------------");
                            if (!string.IsNullOrEmpty(tp.Namespace)) {
                                gen.Line("namespace {0}", tp.Namespace);
                                gen.AddIndent("{");
                            }

                            tp.GenCode_CS_Impl(gen);

                            if (!string.IsNullOrEmpty(tp.Namespace)) {
                                gen.RemIndent("}");
                            }
                            gen.Line();
                        }
                    }
                }
            }

            writer.Flush();
            code = writer.ToString();
            return true;
        }



        void GenCode_CS_Mgr(CodeGenerator gen, BatchInfo batch)
        {
            if (batch == null) {
                gen.Line("namespace zf.util");
                gen.AddIndent("{");
                gen.Line("public partial class TemplateManager");
            }
            else
                gen.Line("public class {0}", "Gen_" + batch.name);
            gen.AddIndent("{");
            // Function CreateTemplate
            if (batch == null)
                gen.Line("static partial void CreateTemplate_Inner(uint typeId, ref BaseTemplate ret)");
            else
                gen.Line("public static void CreateTemplate(uint typeId, ref BaseTemplate ret)");
            gen.AddIndent("{");
            gen.Line("switch (typeId)");
            gen.AddIndent("{");

            foreach (var tp in this.mTypeList) {
                if ((tp.batch == null && batch == null) || (tp.batch != null && tp.batch.Equals(batch))) {
                    if (tp.CompileTo == CompileTarget.CSharp) {
                        if (tp.IsClass()) {
                            GTypeClass classTp = tp as GTypeClass;
                            if (classTp.isTemplateClass)
                                gen.Line("case {0}.TYPE: ret = new {0}(); break;", tp.FullName);
                        }
                    }
                }
            }
            gen.Line("default:");
            gen.AddIndent();
            //gen.Line("GBGenLog.LogError(\"Can't find template type id: \" + typeId);");
            gen.Line("break;");
            gen.RemIndent();
            gen.RemIndent("}");
            gen.RemIndent("}");

            // Function CreateObject
            if (batch == null)
                gen.Line("static partial void CreateObject_Inner(uint typeId, TID tid, ref BaseObject ret)");
            else
                gen.Line("public static void CreateObject(uint typeId, TID tid, ref BaseObject ret)");
            gen.AddIndent("{");
            gen.Line("switch (typeId)");
            gen.AddIndent("{");

            foreach (var tp in this.mTypeList) {
                if ((tp.batch == null && batch == null) || (tp.batch != null && tp.batch.Equals(batch))) {
                    if (tp.CompileTo == CompileTarget.CSharp) {
                        if (tp.IsClass()) {
                            GTypeClass classTp = tp as GTypeClass;
                            if (!string.IsNullOrEmpty(classTp.BindingClass)) {
                                if (!string.IsNullOrEmpty(classTp.BindingClassMacro))
                                    gen.Line("#if {0}", classTp.BindingClassMacro);

                                gen.Line("case {0}.TYPE: ret = new {1}(); break;", tp.Name, classTp.BindingClass);

                                if (!string.IsNullOrEmpty(classTp.BindingClassMacro))
                                    gen.Line("#endif");
                            }
                        }
                    }
                }
            }
            gen.Line("default:");
            gen.AddIndent();
            //gen.Line("GBGenLog.LogError(\"BindingClass is not defiend! type: \" + typeId);");
            gen.Line("break;");
            gen.RemIndent();
            gen.RemIndent("}");
            gen.RemIndent("}");
            gen.RemIndent("}");

            if (batch == null)
                gen.RemIndent("}");
        }

        public bool GenCode_CS_Binding(out string code, BatchInfo batch)
        {
            StringWriter writer = new StringWriter();
            CodeGenerator gen = new CodeGenerator(writer);
            gen.Line("// ============================================================================================= //");
            gen.Line("// This is generated by tool. Don't edit this manually.");
            gen.Line("// Encoding: " + writer.Encoding.EncodingName);
            gen.Line("// ============================================================================================= //");
            gen.Line();
            gen.Line();
            gen.Line("using System.IO;");
            gen.Line("using zf.util;");
            gen.Line();
            gen.Line("#pragma warning disable 0108");
            gen.Line();

            // classes
            foreach (var tp in mTypeList) {
                if ((tp.batch == null && batch == null) || (tp.batch != null && tp.batch.Equals(batch))) {
                    if (tp.CompileTo == CompileTarget.CSharp) {
                        if (tp.IsClass()) {
                            GTypeClass classTp = tp as GTypeClass;
                            if (!string.IsNullOrEmpty(classTp.BindingClass)) {
                                string spaceName = null;
                                string bindclassName = null;
                                if (classTp.BindingClass.Contains(".")) {
                                    bindclassName = classTp.BindingClass.Substring(classTp.BindingClass.LastIndexOf('.') + 1);
                                    spaceName = classTp.BindingClass.Substring(0, classTp.BindingClass.LastIndexOf('.'));
                                } else {
                                    bindclassName = classTp.BindingClass;
                                }

                                if (!string.IsNullOrEmpty(classTp.BindingClassMacro))
                                    gen.Line("#if {0}", classTp.BindingClassMacro);

                                if (!string.IsNullOrEmpty(spaceName)) {
                                    gen.Line("namespace {0}", spaceName);
                                    gen.AddIndent("{");
                                }
                                gen.Line("public partial class {0}", bindclassName);
                                gen.AddIndent("{");
                                gen.Line("public {0} template;", tp.Name);
                                gen.Line("public override void InitTemplate(BaseTemplate tmpl)");
                                gen.AddIndent("{");
                                gen.Line("base.InitTemplate(tmpl);");
                                gen.Line("template = tmpl as {0};", tp.Name);
                                gen.RemIndent("}");
                                gen.RemIndent("}");
                                if (!string.IsNullOrEmpty(spaceName)) {
                                    gen.RemIndent("}");
                                }

                                if (!string.IsNullOrEmpty(classTp.BindingClassMacro))
                                    gen.Line("#endif");
                            }
                        }
                    }
                }
			}

            // partial TemplateManager
            gen.Line("// ----------------------------------------------------------------------------");
            GenCode_CS_Mgr(gen, batch);
            gen.Line();

			writer.Flush();
			code = writer.ToString();
			return true;
		}


        public bool GenCode_FlatBuffer_Head(out string code)
        {
            StringWriter writer = new StringWriter();
            CodeGenerator gen = new CodeGenerator(writer);
            gen.Line("// ============================================================================================= //");
            gen.Line("// This is generated by tool. Don't edit this manually.");
            gen.Line("// Encoding: " + writer.Encoding.EncodingName);
            gen.Line("// ============================================================================================= //");
            gen.Line();

            foreach (var tp in mTypeList) {
                if (tp.CompileTo != CompileTarget.FlatBuffer)
                    continue;
                gen.Line("// ----------------------------------------------------------------------------");
                tp.GenCode_FlatBuffer(gen);
                gen.Line();
            }

            gen.Line("// end");

            writer.Flush();
            code = writer.ToString();
            return true;
        }
    }
}

