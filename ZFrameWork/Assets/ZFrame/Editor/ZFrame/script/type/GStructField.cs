using System;

using LitJson;

namespace ZFEditor
{
    public class GStructField
    {
        public string Name;
        public string Type;
        public string Category;
        public JsonData Default;
        public string Limit;

        // converted data
        public string[] Tags;
        public string[] SubTypes;

        public bool hasTag(string tag)
        {
            if (this.Tags == null)
                return false;
            return System.Array.IndexOf(this.Tags, tag) >= 0;
        }

        public bool Parse(TypeFieldDesc desc)
        {
            this.Name = desc.Name;
            this.Type = desc.Type;
            this.Category = desc.Category;
            this.Default = desc.Default;
            this.Limit = desc.Limit;

            if (desc.Type.StartsWith("Array<") && desc.Type.EndsWith(">")) {
                this.Type = GType.Array;
                int preLength = 6; // "Array<".Length
                string innerStr = desc.Type.Substring(preLength, desc.Type.Length - preLength - 1);
                this.SubTypes = new string[1];
                this.SubTypes[0] = innerStr.Trim();
            } else if (desc.Type.StartsWith("List<") && desc.Type.EndsWith(">")) {
                this.Type = GType.List;
                int preLength = 5; // "List<".Length
                string innerStr = desc.Type.Substring(preLength, desc.Type.Length - preLength - 1);
                this.SubTypes = new string[1];
                this.SubTypes[0] = innerStr.Trim();
            } else if (desc.Type.Contains("Map<") && desc.Type.Contains(">")) {
                this.Type = GType.Map;
                int preLength = 4; // "Map<".Length
                string innerStr = desc.Type.Substring(preLength, desc.Type.Length - preLength - 1);
                string[] splits = innerStr.Split(',');
                if (splits.Length != 2) {
                    GLog.LogError("Parse Type {0} failed!", desc.Type);
                    return false;
                }
                this.SubTypes = new string[2];
                this.SubTypes[0] = splits[0].Trim();
                this.SubTypes[1] = splits[1].Trim();
            } else {
                this.Type = desc.Type;
            }

            // check errors
            if (string.IsNullOrEmpty(this.Name)) {
                GLog.LogError("GType.Field.Parse: 'Name' is required");
                return false;
            }

            if (string.IsNullOrEmpty(this.Type)) {
                GLog.LogError("GType.Field.Parse: 'Type' is required");
                return false;
            }

            return true;
        }

        public bool Parse(JsonData jsonData)
        {
            foreach (var key in jsonData.Keys)
            {
                // 兼容旧版本 //
                if (key.Equals("Field")) {
                    this.Name = (string)jsonData["Field"];
                }
                else if (key.Equals("Array")) {
                    this.Name = (string)jsonData["Array"];
                    this.Type = GType.Array;
                }
                // 兼容旧版本 //
                else if (key.Equals("Type")) {
                    if (jsonData["Type"].IsString) {
                        if (this.Type == null)
                            this.Type = (string)jsonData["Type"];
                        else {
                            this.SubTypes = new string[1];
                            this.SubTypes[0] = (string)jsonData["Type"];
                        }
                    } else {
                        GLog.LogError("'Type' must be string. " + jsonData["Type"]);
                        return false;
                    }
                }
                else if (key.Equals("Category")) {
                    this.Category = (string)jsonData["Category"];
                }
                else if (key.Equals("Default")) {
                    this.Default = jsonData["Default"];
                }
                else if (key.Equals("Tags")) {
                    this.Tags = ((string)jsonData["Tags"]).Split(';');
                }
                else if (key.Equals("Limit")) {
                    this.Limit = (string)jsonData["Limit"];
                }
                else if (key.Equals("Desc")) {
                    // todo:   desc
                }
                else {
                    // new way to define type-name
                    if (key.StartsWith("Array<") && key.EndsWith(">")) {
                        this.Type = GType.Array;
                        int preLength = 6; // "Array<".Length
                        string innerStr = key.Substring(preLength, key.Length - preLength - 1);
                        this.SubTypes = new string[1];
                        this.SubTypes[0] = innerStr.Trim();
                    } else if (key.Contains("Map<") && key.Contains(">")) {
                        this.Type = GType.Map;
                        int preLength = 4; // "Map<".Length
                        string innerStr = key.Substring(preLength, key.Length - preLength - 1);
                        string[] splits = innerStr.Split(',');
                        if (splits.Length != 2) {
                            GLog.LogError("Parse Type {0} failed!", key);
                            return false;
                        }
                        this.SubTypes = new string[2];
                        this.SubTypes[0] = splits[0].Trim();
                        this.SubTypes[1] = splits[1].Trim();
                    } else {
                        this.Type = key;
                    }

                    this.Name = (string)jsonData[key];
                }
            }

            // TODO: check type is correct

            // check errors
            if (string.IsNullOrEmpty(this.Name)) {
                GLog.LogError("GType.Field.Parse: 'Name' is required");
                return false;
            }

            if (string.IsNullOrEmpty(this.Type)) {
                GLog.LogError("GType.Field.Parse: 'Type' is required");
                return false;
            }

            return true;
        }

        public void GenCode_CS_Head(CodeGenerator gen)
        {
            GType tp = GTypeManager.Instance.GetType(this.Type);
            if (this.Type.Equals(GType.Array)) {
                GType subTp = GTypeManager.Instance.GetType(this.SubTypes[0]);
                gen.Line("public {0}[] {1};", subTp.GetCSName(), this.Name);
            } else if (this.Type.Equals(GType.List)) {
                GType subTp = GTypeManager.Instance.GetType(this.SubTypes[0]);
                gen.Line("public List<{0}> {1};", subTp.GetCSName(), this.Name);
            } else if (this.Type.Equals(GType.Map)) {
                GType subTp1 = GTypeManager.Instance.GetType(this.SubTypes[0]);
                GType subTp2 = GTypeManager.Instance.GetType(this.SubTypes[1]);
                gen.Line("public Dictionary<{0},{1}> {2};", subTp1.GetCSName(), subTp2.GetCSName(), this.Name);
            } else {
                gen.Line("public {0} {1};", tp.GetCSName(), this.Name);
            }
        }

        public void GenCode_CS_Deserialize(CodeGenerator gen)
        {
            GType tp = GTypeManager.Instance.GetType(Type);
            tp.GenCode_CS_FieldDeserialize(gen, this.Name, this.Name, this.SubTypes);
        }


        public void GenCode_CS_Serialize(CodeGenerator gen)
        {
            GType tp = GTypeManager.Instance.GetType(Type);
            tp.GenCode_CS_FieldSerialize(gen, this.Name, this.SubTypes);
        }
    }
}
