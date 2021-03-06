// ============================================================================================= //
// This is generated by tool. Don't edit this manually.
// Encoding: Unicode
// ============================================================================================= //


using System.IO;
using System.Text;
using System.Collections.Generic;

using zf.util;

// ----------------------------------------------------------------------------
namespace zf.util
{
    // Struct : SEnvLink
    public partial class SEnvLink
    {
        public void Deserialize(BinaryReader reader)
        {
            fromEnv = new TID();
            fromEnv.Deserialize(reader);

            toEnv = new TID();
            toEnv.Deserialize(reader);

            sameThreadDirect = reader.ReadBoolean();

            bufferSize = reader.ReadInt32();
        }
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    // Class : TGameApp
    public partial class TGameApp
    {
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            name = StringAtom.FromReader(reader);

            language = StringAtom.FromReader(reader);

            modConfig = reader.ReadInt32();

            int len_modPaths = reader.ReadInt32();
            modPaths = new StringAtom[len_modPaths];
            for (int i_modPaths = 0; i_modPaths < len_modPaths; ++i_modPaths)
            {
                modPaths[i_modPaths] = StringAtom.FromReader(reader);
            }

            int len_launchers = reader.ReadInt32();
            launchers = new StringAtom[len_launchers];
            for (int i_launchers = 0; i_launchers < len_launchers; ++i_launchers)
            {
                launchers[i_launchers] = StringAtom.FromReader(reader);
            }
        }
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    // Struct : STeam
    public partial class STeam
    {
        public void Deserialize(BinaryReader reader)
        {
            int len_players = reader.ReadInt32();
            players = new int[len_players];
            for (int i_players = 0; i_players < len_players; ++i_players)
            {
                players[i_players] = reader.ReadInt32();
            }
        }
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    // Struct : STIDLink
    public partial class STIDLink
    {
        public void Deserialize(BinaryReader reader)
        {
            link = new TID();
            link.Deserialize(reader);

            to = new TID();
            to.Deserialize(reader);
        }
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    // Struct : SModConfig
    public partial class SModConfig
    {
        public void Deserialize(BinaryReader reader)
        {
            int len_tidLinks = reader.ReadInt32();
            tidLinks = new STIDLink[len_tidLinks];
            for (int i_tidLinks = 0; i_tidLinks < len_tidLinks; ++i_tidLinks)
            {
                tidLinks[i_tidLinks] = new STIDLink();
                tidLinks[i_tidLinks].Deserialize(reader);
            }

            int len_envs = reader.ReadInt32();
            envs = new TID[len_envs];
            for (int i_envs = 0; i_envs < len_envs; ++i_envs)
            {
                envs[i_envs] = new TID();
                envs[i_envs].Deserialize(reader);
            }

            int len_envLinks = reader.ReadInt32();
            envLinks = new SEnvLink[len_envLinks];
            for (int i_envLinks = 0; i_envLinks < len_envLinks; ++i_envLinks)
            {
                envLinks[i_envLinks] = new SEnvLink();
                envLinks[i_envLinks].Deserialize(reader);
            }
        }
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    // Struct : SCampRelation
    public partial class SCampRelation
    {
        public void Deserialize(BinaryReader reader)
        {
            team1 = reader.ReadInt32();

            team2 = reader.ReadInt32();

            relation = reader.ReadInt64();
        }
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    // Class : TMod
    public partial class TMod
    {
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            name = StringAtom.FromReader(reader);

            maxPlayerNum = reader.ReadInt32();

            int len_teams = reader.ReadInt32();
            teams = new STeam[len_teams];
            for (int i_teams = 0; i_teams < len_teams; ++i_teams)
            {
                teams[i_teams] = new STeam();
                teams[i_teams].Deserialize(reader);
            }

            int len_campRelations = reader.ReadInt32();
            campRelations = new SCampRelation[len_campRelations];
            for (int i_campRelations = 0; i_campRelations < len_campRelations; ++i_campRelations)
            {
                campRelations[i_campRelations] = new SCampRelation();
                campRelations[i_campRelations].Deserialize(reader);
            }

            int len_configs = reader.ReadInt32();
            configs = new Dictionary<TID, SModConfig>();
            for (int i_configs = 0; i_configs < len_configs; ++i_configs)
            {
                TID key = new TID();
                key.Deserialize(reader);
                SModConfig val = new SModConfig();
                val.Deserialize(reader);
                configs.Add(key, val);
            }

            spawnVehicleNum = reader.ReadInt32();
        }
    }
}

// ----------------------------------------------------------------------------
namespace zf.util
{
    // Class : TRunEnv
    public partial class TRunEnv
    {
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            name = StringAtom.FromReader(reader);

            startNow = reader.ReadBoolean();

            timeMode = (ETimeMode)reader.ReadInt16();

            updateTick = reader.ReadInt32();

            fixedUpdateTick = reader.ReadInt32();

            sleepMinTick = reader.ReadInt32();

            minUid = reader.ReadInt32();

            runInThread = reader.ReadBoolean();

            int len_services = reader.ReadInt32();
            services = new TID[len_services];
            for (int i_services = 0; i_services < len_services; ++i_services)
            {
                services[i_services] = new TID();
                services[i_services].Deserialize(reader);
            }

            proxyMode = reader.ReadBoolean();

            proxyNetEnv = new TID();
            proxyNetEnv.Deserialize(reader);

            proxyApp = new TID();
            proxyApp.Deserialize(reader);
        }
    }
}

