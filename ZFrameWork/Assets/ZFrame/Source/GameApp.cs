using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;

using zf.util;

namespace zf.core
{
    public enum ModOperCmd
    {
        OPEN,
        CLOSE,
        RESET
    }

    public class ModInfo
    {
        public string name;
        public string tid;
        public string baseMod;
        public string path;

        public bool modTemplateLoaded = false;
    }

    public struct ModOper
    {
        public ModOper(ModOperCmd cmd, string modName)
        {
            this.cmd = cmd; 
            this.modName = modName;
        }
        public ModOperCmd cmd;
        public string modName;
    }

    public partial class GameApp : BaseObject
    {
    }
}
