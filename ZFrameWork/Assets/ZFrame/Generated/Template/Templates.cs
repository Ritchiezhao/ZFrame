// ============================================================================================= //
// This is generated by tool. Don't edit this manually.
// Encoding: Unicode
// ============================================================================================= //


using System.Collections.Generic;
using zf.util;

// ----------------------------------------------------------------------------
namespace zf.util
{
    public enum ETimeMode
    {
        REAL = 0,
        LOCKSTEP = 1
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    public partial class SEnvLink
    {
        public TID fromEnv;

        public TID toEnv;

        public bool sameThreadDirect;

        public int bufferSize;
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    public class ECampRelation
    {
        public const long Same = 0x0000000000000001;
        public const long Ally = 0x0000000000000002;
        public const long Enermy = 0x0000000000000004;
        public const long Neutral = 0x0000000000000008;
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    public partial class TGameApp : BaseTemplate
    {
        public const uint TYPE = 2507337389;

        public StringAtom name;

        public StringAtom language;

        public int modConfig;

        public StringAtom[] modPaths;

        public StringAtom[] launchers;
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    public partial class STeam
    {
        public int[] players;
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    public partial class STIDLink
    {
        public TID link;

        public TID to;
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    public partial class SModConfig
    {
        public STIDLink[] tidLinks;

        public TID[] envs;

        public SEnvLink[] envLinks;
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    public partial class SCampRelation
    {
        public int team1;

        public int team2;

        public long relation;
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    public partial class TMod : BaseTemplate
    {
        public const uint TYPE = 3050539859;

        public StringAtom name;

        public int maxPlayerNum;

        public STeam[] teams;

        public SCampRelation[] campRelations;

        public Dictionary<TID,SModConfig> configs;

        public int spawnVehicleNum;
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    public partial class TRunEnv : BaseTemplate
    {
        public const uint TYPE = 3576859042;

        public StringAtom name;

        public bool startNow;

        public ETimeMode timeMode;

        public int updateTick;

        public int fixedUpdateTick;

        public int sleepMinTick;

        public int minUid;

        public bool runInThread;

        public TID[] services;

        public bool proxyMode;

        public TID proxyNetEnv;

        public TID proxyApp;
    }
}

