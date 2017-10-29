using System;
using System.Collections.Generic;
using System.IO;

using LitJson;


namespace ZFEditor
{
    // used for c# class with 'Uid'
    public class GTypeHandle : GType
    {
        public override string GetCSName()
        {
            return "int";
        }


        public override void GenCode_CS_FieldSerialize(CodeGenerator gen, string varName = null, string[] subTypes = null)
        {
            gen.Line("writer.Write({0} != null ? {0}.Uid : 0);", varName);
        }

        public override void GenCode_CS_FieldDeserialize(CodeGenerator gen, string left, string varName = null, string[] subTypes = null)
        {
            //gen.Line("{0}.Uid = reader.ReadInt32();", left);
        }
    }
}
