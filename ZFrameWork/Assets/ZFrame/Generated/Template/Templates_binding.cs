// ============================================================================================= //
// This is generated by tool. Don't edit this manually.
// Encoding: Unicode
// ============================================================================================= //


using System.IO;
using sgaf.util;

#pragma warning disable 0108

namespace sgaf.core
{
    public partial class GameApp
    {
        public TGameApp template;
        public override void InitTemplate(BaseTemplate tmpl)
        {
            base.InitTemplate(tmpl);
            template = tmpl as TGameApp;
        }
    }
}
// ----------------------------------------------------------------------------
namespace sgaf.util
{
    public partial class TemplateManager
    {
        static partial void CreateTemplate_Inner(uint typeId, ref BaseTemplate ret)
        {
            switch (typeId)
            {
                case sgaf.util.TGameApp.TYPE: ret = new sgaf.util.TGameApp(); break;
                default:
                    break;
            }
        }
        static partial void CreateObject_Inner(uint typeId, TID tid, ref BaseObject ret)
        {
            switch (typeId)
            {
                case TGameApp.TYPE: ret = new sgaf.core.GameApp(); break;
                default:
                    break;
            }
        }
    }
}
