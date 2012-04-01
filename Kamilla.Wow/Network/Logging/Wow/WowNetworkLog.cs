using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kamilla.Network.Protocols;
using Kamilla.Network.Protocols.Wow;

namespace Kamilla.Network.Logging.Wow
{
    public abstract class WowNetworkLog : NetworkLog
    {
        #region Static Members And Nested Classes
        class ClientBuildInfo
        {
            public ClientBuildInfo(Version version, uint latestBuild, string explicitName = null)
            {
                this.ClientVersion = version;
                this.LatestBuild = latestBuild;
                this.ExplicitName = explicitName;
            }

            public uint ClientBuild { get { return (uint)ClientVersion.Revision; } }
            public readonly Version ClientVersion;
            public readonly uint LatestBuild;
            public readonly string ExplicitName;

            public override string ToString()
            {
                var str = ClientVersion.ToString() + " (latest build " + LatestBuild + ")";
                if (ExplicitName != null)
                    str += " (\"" + ExplicitName + "\")";
                return str;
            }
        }

        struct ClientBuildInfoComparer : IComparer<ClientBuildInfo>
        {
            public int Compare(ClientBuildInfo x, ClientBuildInfo y)
            {
                return x.ClientBuild.CompareTo(y.ClientBuild);
            }
        }

        static List<ClientBuildInfo> s_clientBuildInfos;

        static void ChainAdd(Version baseVersion, params uint[] builds)
        {
            ChainAdd(baseVersion, null, builds);
        }

        static void ChainAdd(Version baseVersion, string explicitName, params uint[] builds)
        {
            var latestBuild = builds.Max();

            foreach (var info in builds.Select(build => new ClientBuildInfo(
                new Version(baseVersion.Major, baseVersion.Minor, baseVersion.Build, (int)build),
                latestBuild, explicitName)))
            {
                s_clientBuildInfos.Add(info);
            }
        }

        static void Add(Version version, uint latestBuild)
        {
            s_clientBuildInfos.Add(new ClientBuildInfo(version, latestBuild));
        }

        static WowNetworkLog()
        {
            s_clientBuildInfos = new List<ClientBuildInfo>(50);

            ChainAdd(new Version(3, 3, 5), new uint[] {
                11993, // World of Warcraft patch 3.3.5 PTR
                12213, // World of Warcraft patch 3.3.5 "Defending the Ruby Sanctum"
                12340, // World of Warcraft patch 3.3.5a
            });
            Add(new Version(4, 0, 0, 11792), 13329); // World of Warcraft patch 4.0.0
            Add(new Version(4, 0, 1, 13131), 13329); // World of Warcraft patch 4.0.1
            Add(new Version(4, 0, 1, 13164), 13329); // World of Warcraft patch 4.0.1 "Cataclysm Systems"
            Add(new Version(4, 0, 1, 13205), 13329); // World of Warcraft patch 4.0.1a
            Add(new Version(4, 0, 3, 13287), 13329); // World of Warcraft patch 4.0.3
            Add(new Version(4, 0, 3, 13329), 13329); // World of Warcraft patch 4.0.3a "The Shattering"
            ChainAdd(new Version(4, 0, 6), new uint[] {
                13561, // World of Warcraft patch 4.0.6 PTR
                13596, // World of Warcraft patch 4.0.6
                13623, // World of Warcraft patch 4.0.6a
            });
            ChainAdd(new Version(4, 1, 0), new uint[] {
                13682, // World of Warcraft patch 4.1.0 PTR
                13707, // World of Warcraft patch 4.1.0 PTR
                13726, // World of Warcraft patch 4.1.0 PTR
                13750, // World of Warcraft patch 4.1.0 PTR
                13793, // World of Warcraft patch 4.1.0 PTR
                13812, // World of Warcraft patch 4.1.0 PTR
                13850, // World of Warcraft patch 4.1.0 PTR
                13860, // World of Warcraft patch 4.1.0 PTR
                13875, // World of Warcraft patch 4.1.0 PTR
                13914, // World of Warcraft patch 4.1.0 "Rise of the Zandalari"
                14007, // World of Warcraft patch 4.1.0a
            });
            ChainAdd(new Version(4, 2, 0), new uint[] {
                14002, // World of Warcraft patch 4.2.0 PTR
                14040, // World of Warcraft patch 4.2.0 PTR
                14107, // World of Warcraft patch 4.2.0 PTR
                14133, // World of Warcraft patch 4.2.0 PTR
                14199, // World of Warcraft patch 4.2.0 PTR
                14241, // World of Warcraft patch 4.2.0 PTR
                14265, // World of Warcraft patch 4.2.0 PTR
                14288, // World of Warcraft patch 4.2.0 PTR
                14299, // World of Warcraft patch 4.2.0 PTR
                14313, // World of Warcraft patch 4.2.0 PTR
                14316, // World of Warcraft patch 4.2.0 PTR
                14333, // World of Warcraft patch 4.2.0 "Rage of the Firelands"
                14480, // World of Warcraft patch 4.2.0a
            });
            ChainAdd(new Version(4, 2, 2), new uint[] {
                14522, // World of Warcraft patch 4.2.2 PTR
                14534, // World of Warcraft patch 4.2.2 PTR
                14545, // World of Warcraft patch 4.2.2
            });
            ChainAdd(new Version(4, 3, 0), new uint[] {
                14732, // World of Warcraft patch 4.3.0 PTR
                14791, // World of Warcraft patch 4.3.0 PTR
                14809, // World of Warcraft patch 4.3.0 PTR
                14849, // World of Warcraft patch 4.3.0 PTR
                14890, // World of Warcraft patch 4.3.0 PTR
                14899, // World of Warcraft patch 4.3.0 PTR
                14911, // World of Warcraft patch 4.3.0 PTR
                14942, // World of Warcraft patch 4.3.0 PTR
                14966, // World of Warcraft patch 4.3.0 PTR
                14976, // World of Warcraft patch 4.3.0 PTR
                14980, // World of Warcraft patch 4.3.0 PTR
                14995, // World of Warcraft patch 4.3.0 PTR
                15005, // World of Warcraft patch 4.3.0
                15050, // World of Warcraft patch 4.3.0a
            });
            ChainAdd(new Version(4, 3, 2), new uint[] {
                15148, // World of Warcraft patch 4.3.2 PTR
                15171, // World of Warcraft patch 4.3.2 PTR
                15176, // World of Warcraft patch 4.3.2 PTR
                15201, // World of Warcraft patch 4.3.2 PTR
                15211, // World of Warcraft patch 4.3.2
            });
            ChainAdd(new Version(4, 3, 3), new uint[] {
                15314, // World of Warcraft patch 4.3.3 PTR
                15338, // World of Warcraft patch 4.3.3 PTR
                15354, // World of Warcraft patch 4.3.3
            });
            ChainAdd(new Version(4, 3, 4), new uint[] {
                15499, // World of Warcraft patch 4.3.4 PTR
            });
            ChainAdd(new Version(5, 0, 1), "MistsOfPandariaBeta", new uint[] {
                15464, // World of Warcraft patch 5.0.1 Beta
                15508, // World of Warcraft patch 5.0.1 Beta
            });

            s_clientBuildInfos.Sort(new ClientBuildInfoComparer());
        }
        #endregion

        public WowNetworkLog(NetworkLogMode mode)
            : base(mode)
        {
        }

        protected ProtocolWrapper m_suggestedProtocol;

        protected Version GetClientBuildInfo(uint clientBuild)
        {
            var latest = m_suggestedProtocol = ProtocolManager.FindWrapper("WowLatest");

            if (clientBuild != 0)
            {
                int index = s_clientBuildInfos.BinaryIndexOf(info => info.ClientBuild.CompareTo(clientBuild));
                if (index >= 0)
                {
                    var info = s_clientBuildInfos[index];

                    var wrapperName = "Wow" + (info.ExplicitName ?? info.LatestBuild.ToString());
                    var protocol = ProtocolManager.FindWrapper(wrapperName);
                    if (protocol != null)
                        m_suggestedProtocol = protocol;

                    return info.ClientVersion;
                }
            }

            return new Version(0, 0, 0, (int)clientBuild);
        }

        public sealed override ProtocolWrapper SuggestedProtocol { get { return m_suggestedProtocol; } }

        protected override void InternalAddPacket(Packet packet)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");

            if (!(packet is WowPacket))
                throw new ArgumentException("packet must be an instance of WowPacket");

            base.InternalAddPacket(packet);
        }

        protected sealed override void InternalWritePacket(Packet packet)
        {
            if (packet == null)
                throw new ArgumentNullException("packet");

            var wowPacket = packet as WowPacket;
            if (wowPacket == null)
                throw new ArgumentException("packet must be an instance of WowPacket");

            this.InternalWritePacket2(wowPacket);
        }

        protected abstract void InternalWritePacket2(WowPacket packet);
    }
}
