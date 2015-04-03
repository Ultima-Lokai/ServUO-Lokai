using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Mobiles;
using Server.Network;
using Server.Spells;

namespace Server.Gumps
{
    public class ShardTravelGump : Gump
    {
        private int m_Page;
        private Mobile m_From;
        private ShardTravelMap m_TravelMap;
        private string[] MapPages = new string[]{"", "Trammel Cities", "Trammel Dungeons", "Felucca Cities", "Felucca Dungeons", "Trammel Moongates", "Felucca Moongates", 
                 "Trammel Shrines", "Felucca Shrines", "Ilshenar Cities", "Ilshenar Dungeons", "Ilshenar Shrines", "Malas", "Tokuno Cities", "Tokuno Dungeons", "TerMur Points of Interest"};

        public ShardTravelGump(Mobile from, int page, int x, int y, ShardTravelMap travelmap) : base(x, y)
        {
            m_Page = page;
            m_From = from;
            m_TravelMap = travelmap;
            if (m_TravelMap == null) return;

            if (m_TravelMap.Entries == null || m_TravelMap.Entries.Count <= 0)
            {
                m_TravelMap.Entries = m_TravelMap.GetDefaultEntries();
            }

            if (m_TravelMap.Entries == null || m_TravelMap.Entries.Count <= 0)
            {
                from.SendMessage("No entries were found on this map.");
                m_From.CloseGump(typeof(ShardTravelGump));
                return;
            }

            AddPage(0);
            AddBackground(0, 0, 600, 460, 3600);
            AddBackground(20, 20, 560, 420, 3000);
            AddImage(34, 26, 0x15DF);

            switch (m_Page)
            {
                case 1: // Trammel Cities
                case 2: // Trammel Dungeons
                case 5: // Trammel Moongates
                case 7: // Trammel Shrines
                    AddImage(38, 30, 0x15D9);
                    break;
                case 3: // Felucca Cities
                case 4: // Felucca Dungeons
                case 6: // Felucca Moongates
                case 8: // Felucca Shrines
                    AddImage(38, 30, 0x15DA);
                    break;
                case 9: // Ilshenar Cities
                case 10: // Ilshenar Dungeons
                case 11: // Ilshenar Shrines
                    AddImage(38, 30, 0x15DB);
                    break;
                case 12: // Malas
                    AddImage(38, 30, 0x15DC);
                    break;
                case 13: // Tokuno Cities
                case 14: // Tokuno Dungeons
                    AddImage(38, 30, 0x15DD);
                    break;
                case 15: // TerMur Points of Interest
                    AddImage(38, 30, 0x15DE);
                    break;
            }

            AddLabel(206, 417, 0, MapPages[m_Page]);
            if (m_Page > 1)
                AddButton(14, 25, 0x15E3, 0x15E7, 2, GumpButtonType.Reply, 0); // Previous Page
            if (m_Page < 15)
                AddButton(427, 25, 0x15E1, 0x15E5, 3, GumpButtonType.Reply, 0); // Next Page

            foreach (ShardTravelEntry entry in m_TravelMap.Entries)
            {
                if (entry.MapIndex == m_Page)
                {
                    AddButton(entry.XposButton, entry.YposButton, 1210, 1209, entry.Index, GumpButtonType.Reply,
                        0);
                    AddLabel(entry.XposLabel, entry.YposLabel, 0, entry.Name);
                }
            }

        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            m_From.CloseGump(typeof (ShardTravelGump));
            int id = info.ButtonID;
            Point3D p;
            Map map;

            if (id < 100)
            {
                if (id == 2)
                    m_From.SendGump(new ShardTravelGump(m_From, m_Page - 1, X, Y, m_TravelMap));
                if (id == 3)
                    m_From.SendGump(new ShardTravelGump(m_From, m_Page + 1, X, Y, m_TravelMap));
                return;
            }
            // Here begins the teleport
            try
            {
                ShardTravelEntry entry = m_TravelMap.GetEntry(id);
                if (entry == null) return;

                p = m_TravelMap.GetEntry(id).Destination;
                map = m_TravelMap.GetEntry(id).Map;

                if (Factions.Sigil.ExistsOn(m_From))
                {
                    m_From.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
                }
                else if (map == Map.Felucca && m_From is PlayerMobile && ((PlayerMobile)m_From).Young)
                {
                    m_From.SendLocalizedMessage(1049543); // You decide against traveling to Felucca while you are still young.
                }
                else if (m_From.Kills >= 5 && map != Map.Felucca)
                {
                    m_From.SendLocalizedMessage(1019004); // You are not allowed to travel there.
                }
                else if (m_From.Criminal)
                {
                    m_From.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
                }
                else if (SpellHelper.CheckCombat(m_From))
                {
                    m_From.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
                }
                else if (Server.Misc.WeightOverloading.IsOverloaded(m_From))
                {
                    m_From.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
                }
                else if (!map.CanSpawnMobile(p.X, p.Y, p.Z))
                {
                    m_From.SendLocalizedMessage(501942); // That location is blocked.
                }
                else if (m_From.Holding != null)
                {
                    m_From.SendLocalizedMessage(1071955); // You cannot teleport while dragging an object.
                }
                else if (entry.Unlocked || m_From.AccessLevel >= AccessLevel.GameMaster)
                {

                    m_From.MoveToWorld(p, map);
                    m_From.SendMessage("You have been moved to X:{0}, Y:{1}, Z:{2}, Map: {3}", p.X, p.Y, p.Z, map);
                }
            }
            catch
            {
                m_From.SendMessage("Teleport failed.");
            }

            m_From.SendGump(new ShardTravelGump(m_From, m_Page, X, Y, m_TravelMap));

        }
    }

    public class ShardTravelEntry
    {
        private int m_Index;
        private int m_MapIndex;
        private string m_Name;
        private Point3D m_Destination;
        private Map m_Map;
        private int m_XposLabel;
        private int m_YposLabel;
        private int m_XposButton;
        private int m_YposButton;
        private bool m_Unlocked;

        public int Index
        {
            get { return m_Index; }
        }

        public int MapIndex
        {
            get { return m_MapIndex; }
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public Point3D Destination
        {
            get { return m_Destination; }
            set { m_Destination = value; }
        }

        public Map Map
        {
            get { return m_Map; }
            set { m_Map = value; }
        }

        public int XposLabel
        {
            get { return m_XposLabel; }
            set { m_XposLabel = value; }
        }

        public int YposLabel
        {
            get { return m_YposLabel; }
            set { m_YposLabel = value; }
        }

        public int XposButton
        {
            get { return m_XposButton; }
            set { m_XposButton = value; }
        }

        public int YposButton
        {
            get { return m_YposButton; }
            set { m_YposButton = value; }
        }

        public bool Unlocked
        {
            get { return m_Unlocked; }
            set { m_Unlocked = value; }
        }

        public ShardTravelEntry(int index, int mapindex, string name, Point3D p, Map map, int xposlabel, int yposlabel,
            int xposbutton, int yposbutton) : this(index, mapindex, name, p, map, xposlabel, yposlabel, xposbutton,
                yposbutton, false)
        {
        }

        public ShardTravelEntry(int index, int mapindex, string name, Point3D p, Map map, int xposlabel, int yposlabel, int xposbutton,
            int yposbutton, bool unlocked)
        {
            m_Index = index;
            m_MapIndex = mapindex;
            m_Name = name;
            m_Destination = p;
            m_Map = map;
            m_XposLabel = xposlabel;
            m_YposLabel = yposlabel;
            m_XposButton = xposbutton;
            m_YposButton = yposbutton;
            m_Unlocked = unlocked;
        }
    }

    public class ShardTravelMap : Item
    {
        private List<ShardTravelEntry> m_Entries;

        public List<ShardTravelEntry> Entries
        {
            get { return m_Entries; }
            set { m_Entries = value; }
        }

        public ShardTravelEntry GetEntry(int index)
        {
            foreach (ShardTravelEntry entry in m_Entries)
            {
                if (entry.Index == index)
                    return entry;
            }
            return null;
        }

        [Constructable]
        public ShardTravelMap() : base(0x14EB)
        {
            Name = "Shard Travel Map";
            LootType = LootType.Blessed;
            m_Entries = GetDefaultEntries();
        }

        public override string DefaultName
        {
            get { return "Shard Travel Map"; }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.AccessLevel >= AccessLevel.GameMaster)
                from.SendGump(new ShardTravelGump(from, 1, 50, 60, this));
        }

        public ShardTravelMap(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int) 0); // version

            if (m_Entries == null || m_Entries.Count<=0)
                writer.Write(0);
            else
            {
                writer.Write(m_Entries.Count);
                foreach (ShardTravelEntry entry in m_Entries)
                {
                    writer.Write(entry.Index);
                    writer.Write(entry.MapIndex);
                    writer.Write(entry.Name);
                    writer.Write(entry.Destination);
                    writer.Write(entry.Map);
                    writer.Write(entry.XposLabel);
                    writer.Write(entry.YposLabel);
                    writer.Write(entry.XposButton);
                    writer.Write(entry.YposButton);
                    writer.Write(entry.Unlocked);
                }
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            switch (version)
            {
                case 0:
                {
                    int count = reader.ReadInt();
                    if (count > 0)
                    {
                        m_Entries = new List<ShardTravelEntry>();
                        for (int i = 0; i < count; i++)
                        {
                            try
                            {
                                m_Entries.Add(new ShardTravelEntry(reader.ReadInt(), reader.ReadInt(),
                                    reader.ReadString(), reader.ReadPoint3D(),
                                    reader.ReadMap(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(),
                                    reader.ReadInt(), reader.ReadBool()));
                            }
                            catch
                            {
                            }
                        }
                        if (m_Entries.Count != count)
                        {
                            Console.WriteLine("There was an error reading the Shard Travel Entries for a Travel Map.");
                        }
                    }
                    else
                    {
                        m_Entries = GetDefaultEntries();
                    }
                    break;
                }
            }
        }

        public List<ShardTravelEntry> LoadEntries()
        {
            
            try
            {
                // Here we will try loading from Xml or Binary...?
                throw new Exception();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception caught loading default Shard Travel Entries: {0}", e.Message);
                Console.WriteLine("Using local defaults instead.");
                return GetDefaultEntries();
            }
        }

        public List<ShardTravelEntry> GetDefaultEntries()
        {
            List<ShardTravelEntry> entries = new List<ShardTravelEntry>();

            // Trammel Cities			
            entries.Add(new ShardTravelEntry(100, 1, "Britain", new Point3D(1434, 1699, 2), Map.Trammel, 171, 172, 147, 176));
            entries.Add(new ShardTravelEntry(101, 1, "Bucs Den", new Point3D(2705, 2162, 0), Map.Trammel, 245, 244, 245, 264));
            entries.Add(new ShardTravelEntry(102, 1, "Cove", new Point3D(2237, 1214, 0), Map.Trammel, 212, 124, 212, 144));
            entries.Add(new ShardTravelEntry(103, 1, "New Haven", new Point3D(3493, 2577, 14), Map.Trammel, 314, 248, 314, 268));
            entries.Add(new ShardTravelEntry(104, 1, "Jhelom", new Point3D(1417, 3821, 0), Map.Trammel, 146, 369, 141, 391));
            entries.Add(new ShardTravelEntry(105, 1, "New Magincia", new Point3D(3728, 2164, 20), Map.Trammel, 340, 244, 340, 264));
            entries.Add(new ShardTravelEntry(106, 1, "Minoc", new Point3D(2525, 582, 0), Map.Trammel, 232, 50, 232, 70));
            entries.Add(new ShardTravelEntry(107, 1, "Moonglow", new Point3D(4471, 1177, 0), Map.Trammel, 312, 117, 364, 130));
            entries.Add(new ShardTravelEntry(108, 1, "Nujel'm", new Point3D(3770, 1308, 0), Map.Trammel, 343, 134, 343, 114));
            entries.Add(new ShardTravelEntry(109, 1, "Serpents Hold", new Point3D(2895, 3479, 15), Map.Trammel, 247, 330, 247, 350));
            entries.Add(new ShardTravelEntry(110, 1, "Skara Brae", new Point3D(596, 2138, 0), Map.Trammel, 45, 246, 78, 228));
            entries.Add(new ShardTravelEntry(111, 1, "Trinsic", new Point3D(1823, 2821, 0), Map.Trammel, 182, 260, 182, 280));
            entries.Add(new ShardTravelEntry(112, 1, "Vesper", new Point3D(2899, 676, 0), Map.Trammel, 247, 104, 247, 124));
            entries.Add(new ShardTravelEntry(113, 1, "Yew", new Point3D(542, 985, 0), Map.Trammel, 62, 81, 78, 104));

            // Trammel Dungeons
            entries.Add(new ShardTravelEntry(200, 2, "Covetous", new Point3D(2498, 921, 0), Map.Trammel, 230, 79, 230, 99));
            entries.Add(new ShardTravelEntry(201, 2, "Deceit", new Point3D(4111, 434, 5), Map.Trammel, 342, 47, 338, 65));
            entries.Add(new ShardTravelEntry(202, 2, "Despise", new Point3D(1301, 1080, 0), Map.Trammel, 131, 135, 131, 121));
            entries.Add(new ShardTravelEntry(203, 2, "Destard", new Point3D(1176, 2640, 2), Map.Trammel, 118, 289, 118, 273));
            entries.Add(new ShardTravelEntry(204, 0, "Fire", new Point3D(2923, 3409, 8), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(205, 2, "Hythloth", new Point3D(4721, 3824, 0), Map.Trammel, 325, 385, 382, 377));
            entries.Add(new ShardTravelEntry(206, 2, "Ice", new Point3D(1999, 81, 4), Map.Trammel, 147, 30, 172, 33));
            entries.Add(new ShardTravelEntry(207, 2, "Orc Caves", new Point3D(1017, 1429, 0), Map.Trammel, 106, 172, 104, 156));
            entries.Add(new ShardTravelEntry(208, 0, "Sanctuary", new Point3D(759, 1642, 0), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(209, 2, "Shame", new Point3D(511, 1565, 0), Map.Trammel, 70, 185, 70, 169));
            entries.Add(new ShardTravelEntry(210, 0, "Solen Hive", new Point3D(2607, 763, 0), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(211, 2, "Wrong", new Point3D(2043, 238, 10), Map.Trammel, 190, 65, 182, 55));

            // Felucca Cities
            entries.Add(new ShardTravelEntry(300, 3, "Britain", new Point3D(1434, 1699, 2), Map.Felucca, 171, 172, 147, 176));
            entries.Add(new ShardTravelEntry(301, 3, "Bucs Den", new Point3D(2705, 2162, 0), Map.Felucca, 245, 244, 245, 264));
            entries.Add(new ShardTravelEntry(302, 3, "Cove", new Point3D(2237, 1214, 0), Map.Felucca, 212, 124, 212, 144));
            entries.Add(new ShardTravelEntry(303, 3, "Jhelom", new Point3D(1417, 3821, 0), Map.Felucca, 146, 369, 141, 391));
            entries.Add(new ShardTravelEntry(304, 3, "Magincia", new Point3D(3728, 2164, 20), Map.Felucca, 340, 244, 340, 264));
            entries.Add(new ShardTravelEntry(305, 3, "Minoc", new Point3D(2525, 582, 0), Map.Felucca, 232, 50, 232, 70));
            entries.Add(new ShardTravelEntry(306, 3, "Moonglow", new Point3D(4471, 1177, 0), Map.Felucca, 312, 117, 364, 130));
            entries.Add(new ShardTravelEntry(307, 3, "Nujel'm", new Point3D(3770, 1308, 0), Map.Felucca, 343, 134, 343, 114));
            entries.Add(new ShardTravelEntry(308, 3, "Ocllo", new Point3D(3626, 2611, 0), Map.Felucca, 325, 280, 325, 300));
            entries.Add(new ShardTravelEntry(309, 3, "Serpents Hold", new Point3D(2895, 3479, 15), Map.Felucca, 247, 330, 247, 350));
            entries.Add(new ShardTravelEntry(310, 3, "Skara Brae", new Point3D(596, 2138, 0), Map.Felucca, 45, 246, 78, 228));
            entries.Add(new ShardTravelEntry(311, 3, "Trinsic", new Point3D(1823, 2821, 0), Map.Felucca, 182, 260, 182, 280));
            entries.Add(new ShardTravelEntry(312, 3, "Vesper", new Point3D(2899, 676, 0), Map.Felucca, 247, 104, 247, 124));
            entries.Add(new ShardTravelEntry(313, 3, "Yew", new Point3D(542, 985, 0), Map.Felucca, 62, 81, 78, 104));

            // Felucca Dungeons
            entries.Add(new ShardTravelEntry(400, 4, "Covetous", new Point3D(2498, 921, 0), Map.Felucca, 230, 79, 230, 99));
            entries.Add(new ShardTravelEntry(401, 4, "Deceit", new Point3D(4111, 434, 5), Map.Felucca, 342, 47, 338, 65));
            entries.Add(new ShardTravelEntry(402, 4, "Despise", new Point3D(1301, 1080, 0), Map.Felucca, 131, 135, 131, 121));
            entries.Add(new ShardTravelEntry(403, 4, "Destard", new Point3D(1176, 2640, 2), Map.Felucca, 118, 289, 118, 273));
            entries.Add(new ShardTravelEntry(404, 0, "Fire", new Point3D(2923, 3409, 8), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(405, 4, "Hythloth", new Point3D(4721, 3824, 0), Map.Felucca, 325, 385, 382, 377));
            entries.Add(new ShardTravelEntry(406, 0, "Ice", new Point3D(1999, 81, 4), Map.Felucca, 147, 30, 172, 33));
            entries.Add(new ShardTravelEntry(407, 4, "Orc Caves", new Point3D(1017, 1429, 0), Map.Felucca, 106, 172, 104, 156));
            entries.Add(new ShardTravelEntry(408, 0, "Sanctuary", new Point3D(759, 1642, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(409, 4, "Shame", new Point3D(511, 1565, 0), Map.Felucca, 70, 185, 70, 169));
            entries.Add(new ShardTravelEntry(410, 0, "Solen Hive", new Point3D(2607, 763, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(411, 4, "Wrong", new Point3D(2043, 238, 10), Map.Felucca, 190, 65, 182, 55));

            // Trammel Moongates
            entries.Add(new ShardTravelEntry(500, 0, "Britain", new Point3D(1336, 1997, 5), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(501, 0, "New Haven", new Point3D(3450, 2677, 25), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(502, 0, "Jhelom", new Point3D(1499, 3771, 5), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(503, 0, "Magincia", new Point3D(3563, 2139, 34), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(504, 0, "Minoc", new Point3D(2701, 692, 5), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(505, 0, "Moonglow", new Point3D(4467, 1283, 5), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(506, 0, "Skara Brae", new Point3D(643, 2067, 5), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(507, 0, "Trinsic", new Point3D(1828, 2948, -20), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(508, 0, "Yew", new Point3D(771, 752, 5), Map.Trammel, 0, 0, 0, 0));

            // Felucca Moongates
            entries.Add(new ShardTravelEntry(600, 0, "Britain", new Point3D(1336, 1997, 5), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(601, 0, "Buccaneer's Den", new Point3D(2711, 2234, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(602, 0, "Jhelom", new Point3D(1499, 3771, 5), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(603, 0, "Magincia", new Point3D(3563, 2139, 34), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(604, 0, "Minoc", new Point3D(2701, 692, 5), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(605, 0, "Moonglow", new Point3D(4467, 1283, 5), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(606, 0, "Skara Brae", new Point3D(643, 2067, 5), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(607, 0, "Trinsic", new Point3D(1828, 2948, -20), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(608, 0, "Yew", new Point3D(771, 752, 5), Map.Felucca, 0, 0, 0, 0));

            // Trammel Shrines
            entries.Add(new ShardTravelEntry(700, 0, "Compassion", new Point3D(1215, 467, -13), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(701, 0, "Honesty", new Point3D(722, 1366, -60), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(702, 0, "Honor", new Point3D(744, 724, -28), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(703, 0, "Humility", new Point3D(281, 1016, 0), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(704, 0, "Justice", new Point3D(987, 1011, -32), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(705, 0, "Sacrifice", new Point3D(1174, 1286, -30), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(706, 0, "Spirituality", new Point3D(1532, 1340, -3), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(707, 0, "Valor", new Point3D(528, 216, -45), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(708, 0, "Choas", new Point3D(1721, 218, 96), Map.Trammel, 0, 0, 0, 0));

            // Felucca Shrines
            entries.Add(new ShardTravelEntry(800, 0, "Compassion", new Point3D(1215, 467, -13), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(801, 0, "Honesty", new Point3D(722, 1366, -60), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(802, 0, "Honor", new Point3D(744, 724, -28), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(803, 0, "Humility", new Point3D(281, 1016, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(804, 0, "Justice", new Point3D(987, 1011, -32), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(805, 0, "Sacrifice", new Point3D(1174, 1286, -30), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(806, 0, "Spirituality", new Point3D(1532, 1340, -3), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(807, 0, "Valor", new Point3D(528, 216, -45), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(808, 0, "Choas", new Point3D(1721, 218, 96), Map.Felucca, 0, 0, 0, 0));

            // Ilshenar Cities
            entries.Add(new ShardTravelEntry(900, 9, "Gargoyle City", new Point3D(852, 602, -40), Map.Ilshenar, 160, 150, 160, 170));
            entries.Add(new ShardTravelEntry(901, 9, "Lakeshire", new Point3D(1203, 1124, -25), Map.Ilshenar, 235, 285, 235, 305));
            entries.Add(new ShardTravelEntry(902, 9, "Mistas", new Point3D(819, 1130, -29), Map.Ilshenar, 160, 280, 160, 300));
            entries.Add(new ShardTravelEntry(903, 9, "Montor", new Point3D(1706, 205, 104), Map.Ilshenar, 310, 60, 310, 80));

            //Ilshenar Dungeons
            entries.Add(new ShardTravelEntry(1000, 0, "Ankh Dungeon", new Point3D(576, 1150, -100), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1001, 0, "Blood Dungeon", new Point3D(1747, 1171, -2), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1002, 0, "Exodus Dungeon", new Point3D(854, 778, -80), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1003, 0, "Sorceror's Dungeon", new Point3D(548, 462, -53), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1004, 0, "Spectre Dungeon", new Point3D(1363, 1033, -8), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1005, 0, "Spider Cave", new Point3D(1420, 913, -16), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1006, 0, "Wisp Dungeon", new Point3D(651, 1302, -58), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1007, 0, "Rock Dungeon", new Point3D(1787, 572, 69), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1008, 0, "Savage Camp", new Point3D(1151, 659, -80), Map.Ilshenar, 0, 0, 0, 0));

            // Ilshenar Shrines
            entries.Add(new ShardTravelEntry(1100, 0, "Compassion", new Point3D(1215, 467, -13), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1101, 0, "Honesty", new Point3D(722, 1366, -60), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1102, 0, "Honor", new Point3D(744, 724, -28), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1103, 0, "Humility", new Point3D(281, 1016, 0), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1104, 0, "Justice", new Point3D(987, 1011, -32), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1105, 0, "Sacrifice", new Point3D(1174, 1286, -30), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1106, 0, "Spirituality", new Point3D(1532, 1340, -3), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1107, 0, "Valor", new Point3D(528, 216, -45), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1108, 0, "Choas", new Point3D(1721, 218, 96), Map.Ilshenar, 0, 0, 0, 0));

            // Malas
            entries.Add(new ShardTravelEntry(1200, 12, "Luna", new Point3D(1015, 527, -65), Map.Malas, 120, 90, 120, 110));
            entries.Add(new ShardTravelEntry(1201, 12, "Umbra", new Point3D(1997, 1386, -85), Map.Malas, 290, 260, 290, 280));
            entries.Add(new ShardTravelEntry(1202, 0, "Orc Fort 1", new Point3D(912, 215, -90), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1203, 0, "Orc Fort 2", new Point3D(1678, 374, -50), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1204, 0, "Orc Fort 3", new Point3D(1375, 621, -86), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1205, 0, "Orc Fort 4", new Point3D(1184, 715, -89), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1206, 0, "Orc Fort 5", new Point3D(1279, 1324, -90), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1207, 0, "Orc Fort 6", new Point3D(1598, 1834, -107), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1208, 0, "Ruined Temple", new Point3D(1598, 1762, -110), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1209, 0, "Doom", new Point3D(2368, 1267, -85), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1210, 0, "Labyrinth", new Point3D(1730, 981, -80), Map.Malas, 0, 0, 0, 0));

            // Tokuno Cities
            entries.Add(new ShardTravelEntry(1300, 0, "Isamu-Jima", new Point3D(1169, 998, 41), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1301, 0, "Makoto-Jima", new Point3D(802, 1204, 25), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1302, 0, "Homare-Jima", new Point3D(270, 628, 15), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1303, 0, "Bushido Dojo", new Point3D(322, 430, 32), Map.Tokuno, 0, 0, 0, 0));

            // Tokuno Dungeons			
            entries.Add(new ShardTravelEntry(1400, 0, "Crane Marsh", new Point3D(203, 985, 18), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1401, 0, "Fan Dancer's Dojo", new Point3D(970, 222, 23), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1402, 0, "Makoto Desert", new Point3D(724, 1050, 33), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1403, 0, "Makoto Zento", new Point3D(741, 1261, 30), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1404, 0, "Mt. Sho Castle", new Point3D(1234, 772, 3), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1405, 0, "Valor Shrine", new Point3D(1044, 523, 15), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1406, 0, "Yomotsu Mine", new Point3D(257, 786, 63), Map.Tokuno, 0, 0, 0, 0));

            // TerMur Points of Interest
            entries.Add(new ShardTravelEntry(1500, 0, "Royal City", new Point3D(852, 3526, -43), Map.TerMur, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1501, 0, "Holy City", new Point3D(926, 3989, -36), Map.TerMur, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1502, 0, "Fisherman's Reach", new Point3D(612, 3038, 35), Map.TerMur, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1503, 0, "Tomb of Kings", new Point3D(997, 3843, -41), Map.TerMur, 0, 0, 0, 0));

            return entries;
        }
    }
}
