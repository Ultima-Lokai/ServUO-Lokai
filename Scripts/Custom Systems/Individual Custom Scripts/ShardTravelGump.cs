using System;
using System.Collections.Generic;
using Server.Commands;
using Server.Network;

namespace Server.Gumps
{
    public class ShardTravelGump : Gump
    {
        private int m_Page;
        private Mobile m_From;
        private ShardTravelMap m_TravelMap;
        private string[] MapPages = new string[]{"", "Trammel Cities", "Trammel Dungeons", "Felucca Cities", "Felucca Dungeons", "Trammel Moongates", "Felucca Moongates", 
                 "Ilshenar Cities", "Ilshenar Dungeons", "Ilshenar Shrines", "Malas Cities", "Malas Dungeons", "Tokuno Cities", "Tokuno Dungeons", "TerMur Cities", "TerMur Points of Interest"};

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
                case 1:
                case 2:
                case 5:
                    AddImage(38, 30, 0x15D9);
                    break;
                case 3:
                case 4:
                case 6:
                    AddImage(38, 30, 0x15DA);
                    break;
                case 7:
                case 8:
                case 9:
                    AddImage(38, 30, 0x15DB);
                    break;
                case 10:
                case 11:
                    AddImage(38, 30, 0x15DC);
                    break;
                case 12:
                case 13:
                    AddImage(38, 30, 0x15DD);
                    break;
                case 14:
                case 15:
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
                p = m_TravelMap.GetEntry(id).Destination;
                map = m_TravelMap.GetEntry(id).Map;

                m_From.MoveToWorld(p, map);
                m_From.SendMessage("You have been moved to X:{0}, Y:{1}, Z:{2}, Map: {3}", p.X, p.Y, p.Z, map);
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

        public ShardTravelEntry(int index, int mapindex, string name, Point3D p, Map map, int xposlabel, int yposlabel, int xposbutton,
            int yposbutton)
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
            writer.Write((int) 0);

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
                }
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            int count = reader.ReadInt();
            if (count > 0)
            {
                m_Entries = new List<ShardTravelEntry>();
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        m_Entries.Add(new ShardTravelEntry(reader.ReadInt(), reader.ReadInt(), reader.ReadString(), reader.ReadPoint3D(),
                            reader.ReadMap(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt(), reader.ReadInt()));
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
            entries.Add(new ShardTravelEntry(201, 0, "Deceit", new Point3D(4111, 434, 5), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(202, 0, "Despise", new Point3D(1301, 1080, 0), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(203, 0, "Destard", new Point3D(1176, 2640, 2), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(204, 0, "Fire", new Point3D(2923, 3409, 8), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(205, 0, "Hythloth", new Point3D(4721, 3824, 0), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(206, 0, "Ice", new Point3D(1999, 81, 4), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(207, 0, "Orc Caves", new Point3D(1017, 1429, 0), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(208, 0, "Sanctuary", new Point3D(759, 1642, 0), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(209, 0, "Shame", new Point3D(511, 1565, 0), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(210, 0, "Solen Hive", new Point3D(2607, 763, 0), Map.Trammel, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(211, 0, "Wrong", new Point3D(2043, 238, 10), Map.Trammel, 0, 0, 0, 0));

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
            entries.Add(new ShardTravelEntry(401, 0, "Deceit", new Point3D(4111, 434, 5), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(402, 0, "Despise", new Point3D(1301, 1080, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(403, 0, "Destard", new Point3D(1176, 2640, 2), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(404, 0, "Fire", new Point3D(2923, 3409, 8), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(405, 0, "Hythloth", new Point3D(4721, 3824, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(406, 0, "Ice", new Point3D(1999, 81, 4), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(407, 0, "Orc Caves", new Point3D(1017, 1429, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(408, 0, "Sanctuary", new Point3D(759, 1642, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(409, 0, "Shame", new Point3D(511, 1565, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(410, 0, "Solen Hive", new Point3D(2607, 763, 0), Map.Felucca, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(411, 0, "Wrong", new Point3D(2043, 238, 10), Map.Felucca, 0, 0, 0, 0));

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

            // Ilshenar Cities
            entries.Add(new ShardTravelEntry(700, 7, "Gargoyle City", new Point3D(852, 602, -40), Map.Ilshenar, 160, 150, 160, 170));
            entries.Add(new ShardTravelEntry(701, 7, "Lakeshire", new Point3D(1203, 1124, -25), Map.Ilshenar, 235, 285, 235, 305));
            entries.Add(new ShardTravelEntry(702, 7, "Mistas", new Point3D(819, 1130, -29), Map.Ilshenar, 160, 280, 160, 300));
            entries.Add(new ShardTravelEntry(703, 7, "Montor", new Point3D(1706, 205, 104), Map.Ilshenar, 310, 60, 310, 80));

            //Ilshenar Dungeons
            entries.Add(new ShardTravelEntry(800, 0, "Ankh Dungeon", new Point3D(576, 1150, -100), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(801, 0, "Blood Dungeon", new Point3D(1747, 1171, -2), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(802, 0, "Exodus Dungeon", new Point3D(854, 778, -80), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(803, 0, "Sorceror's Dungeon", new Point3D(548, 462, -53), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(804, 0, "Spectre Dungeon", new Point3D(1363, 1033, -8), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(805, 0, "Spider Cave", new Point3D(1420, 913, -16), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(806, 0, "Wisp Dungeon", new Point3D(651, 1302, -58), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(807, 0, "Rock Dungeon", new Point3D(1787, 572, 69), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(808, 0, "Savage Camp", new Point3D(1151, 659, -80), Map.Ilshenar, 0, 0, 0, 0));

            // Ilshenar Shrines
            entries.Add(new ShardTravelEntry(900, 0, "Compassion", new Point3D(1215, 467, -13), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(901, 0, "Honesty", new Point3D(722, 1366, -60), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(902, 0, "Honor", new Point3D(744, 724, -28), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(903, 0, "Humility", new Point3D(281, 1016, 0), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(904, 0, "Justice", new Point3D(987, 1011, -32), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(905, 0, "Sacrifice", new Point3D(1174, 1286, -30), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(906, 0, "Spirituality", new Point3D(1532, 1340, -3), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(907, 0, "Valor", new Point3D(528, 216, -45), Map.Ilshenar, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(908, 0, "Choas", new Point3D(1721, 218, 96), Map.Ilshenar, 0, 0, 0, 0));

            // Malas Cities
            entries.Add(new ShardTravelEntry(1000, 10, "Luna", new Point3D(1015, 527, -65), Map.Malas, 120, 90, 120, 110));
            entries.Add(new ShardTravelEntry(1001, 10, "Umbra", new Point3D(1997, 1386, -85), Map.Malas, 290, 260, 290, 280));

            // Malas Dungeons
            entries.Add(new ShardTravelEntry(1100, 0, "Orc Fort 1", new Point3D(912, 215, -90), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1101, 0, "Orc Fort 2", new Point3D(1678, 374, -50), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1102, 0, "Orc Fort 3", new Point3D(1375, 621, -86), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1103, 0, "Orc Fort 4", new Point3D(1184, 715, -89), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1104, 0, "Orc Fort 5", new Point3D(1279, 1324, -90), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1105, 0, "Orc Fort 6", new Point3D(1598, 1834, -107), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1106, 0, "Ruined Temple", new Point3D(1598, 1762, -110), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1107, 0, "Doom", new Point3D(2368, 1267, -85), Map.Malas, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1108, 0, "Labyrinth", new Point3D(1730, 981, -80), Map.Malas, 0, 0, 0, 0));

            // Tokuno Cities
            entries.Add(new ShardTravelEntry(1200, 0, "Isamu-Jima", new Point3D(1169, 998, 41), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1201, 0, "Makoto-Jima", new Point3D(802, 1204, 25), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1202, 0, "Homare-Jima", new Point3D(270, 628, 15), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1203, 0, "Bushido Dojo", new Point3D(322, 430, 32), Map.Tokuno, 0, 0, 0, 0));

            // Tokuno Dungeons			
            entries.Add(new ShardTravelEntry(1300, 0, "Crane Marsh", new Point3D(203, 985, 18), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1301, 0, "Fan Dancer's Dojo", new Point3D(970, 222, 23), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1302, 0, "Makoto Desert", new Point3D(724, 1050, 33), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1303, 0, "Makoto Zento", new Point3D(741, 1261, 30), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1304, 0, "Mt. Sho Castle", new Point3D(1234, 772, 3), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1305, 0, "Valor Shrine", new Point3D(1044, 523, 15), Map.Tokuno, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1306, 0, "Yomotsu Mine", new Point3D(257, 786, 63), Map.Tokuno, 0, 0, 0, 0));

            // TerMur Cities
            entries.Add(new ShardTravelEntry(1400, 0, "Royal City", new Point3D(852, 3526, -43), Map.TerMur, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1401, 0, "Holy City", new Point3D(926, 3989, -36), Map.TerMur, 0, 0, 0, 0));

            // TerMur Points of Interest
            entries.Add(new ShardTravelEntry(1500, 0, "Fisherman's Reach", new Point3D(612, 3038, 35), Map.TerMur, 0, 0, 0, 0));
            entries.Add(new ShardTravelEntry(1501, 0, "Tomb of Kings", new Point3D(997, 3843, -41), Map.TerMur, 0, 0, 0, 0));

            return entries;
        }
    }
}
