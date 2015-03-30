using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenUO.Ultima;
using Server.Commands;
using Server.Misc;
using Server.Network;

namespace Server.Gumps
{
    public class ShardTravelCommand
    {
        
		public static void Initialize()
        {
            CommandSystem.Register("ShardTravel", AccessLevel.GameMaster, new CommandEventHandler(ShardTravel_OnCommand));
        }

        [Usage("ShardTravel")]
        [Description("Shows the ShardTravelGump.")]
        public static void ShardTravel_OnCommand(CommandEventArgs e)
        {
            if (e.Mobile != null && !e.Mobile.Deleted)
                e.Mobile.SendGump(new ShardTravelGump(e.Mobile, 1));
        }
    }

    public class ShardTravelGump : Gump
    {
        private int m_Page;
        private Mobile m_From;

        public ShardTravelGump(Mobile from, int page) : this(from, page, 50, 60)
        {
        }

        public ShardTravelGump(Mobile from, int page, int x, int y) : base(x, y)
        {
            m_Page = page;
            m_From = from;

            AddPage(0);
            AddBackground(0, 0, 403, 403, 9200);
            AddImage(5, 5, 0x15DF);

            switch (m_Page)
            {
                case 1:
                    AddImage(10, 10, 0x15D9);
                    break;
                case 2:
                    AddImage(10, 10, 0x15DA);
                    break;
                case 3:
                    AddImage(10, 10, 0x15DB);
                    break;
                case 4:
                    AddImage(10, 10, 0x15DC);
                    break;
                case 5:
                    AddImage(10, 10, 0x15DD);
                    break;
                case 6:
                    AddImage(10, 10, 0x15DE);
                    break;
            }
            if (m_Page > 1)
                AddButton(5, 5, 0x15E3, 0x15E7, 2, GumpButtonType.Reply, 0); // Previous Page
            if (m_Page < 6)
                AddButton(388, 5, 0x15E1, 0x15E5, 3, GumpButtonType.Reply, 0); // Next Page
            int buttonBase = m_Page*100000;
            int index = 1;


            for (int xpos = 16; xpos < 393; xpos += 4)
            {
                for (int ypos = 16; ypos < 393; ypos += 4)
                {
                    int buttonID = buttonBase + index;
                    AddButton(xpos, ypos, 0x1772, 0x1772, buttonID, GumpButtonType.Reply, 0);
                    // Console.WriteLine("Displaying button index {0}, at xPos {1}, yPos {2}", index, xpos, ypos);
                    index++;
                }
            }

        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            m_From.CloseGump(typeof (ShardTravelGump));
            int id = info.ButtonID;
            Map map = Map.Internal;
            int width;
            int height;
            int marker = 0;
            int xPos, yPos, zPos;

            if (id < 100000)
            {
                if (id == 2)
                    m_From.SendGump(new ShardTravelGump(m_From, m_Page - 1));
                if (id == 3)
                    m_From.SendGump(new ShardTravelGump(m_From, m_Page + 1));
                return;
            }
            else if (id < 200000)
            {
                map = Map.Trammel;
                marker = id - 100000;
            }
            else if (id < 300000)
            {
                map = Map.Felucca;
                marker = id - 200000;
            }
            else if (id < 400000)
            {
                map = Map.Ilshenar;
                marker = id - 300000;
            }
            else if (id < 500000)
            {
                map = Map.Malas;
                marker = id - 400000;
            }
            else if (id < 600000)
            {
                map = Map.Tokuno;
                marker = id - 500000;
            }
            else if (id < 700000)
            {
                map = Map.TerMur;
                marker = id - 600000;
            }

            width = map.Width;
            height = map.Height;
            xPos = Math.Max((width/94)*(int) Math.Floor(((double) marker/94)),
                (int) Math.Floor(((double) marker/94)) + 1);
            yPos = Math.Max((height/94)*(marker%94), (marker%94) + 1);
            zPos = map.GetAverageZ(xPos, yPos);

            Point3D point3D = new Point3D(xPos, yPos, zPos);
            m_From.MoveToWorld(point3D, map);
            m_From.SendMessage("You have been moved to X:{0}, Y:{1}, Z:{2}, Map: {3}", xPos, yPos, zPos, map);

            m_From.SendGump(new ShardTravelGump(m_From, m_Page, X, Y));

        }
    }

    public class ShardTravelEntry
    {
        private string m_Name;
        private Point3D m_Destination;
        private Map m_Map;
        private int m_Xpos;
        private int m_Ypos;

        public string Name { get { return m_Name; } }
        public Point3D Destination { get { return m_Destination; } }
        public Map Map { get { return m_Map; } }
        public int Xpos { get { return m_Xpos; } set { m_Xpos = value; } }
        public int Ypos { get { return m_Ypos; } set { m_Ypos = value; } }

        public ShardTravelEntry(string name, Point3D p, Map map, int xpos, int ypos)
        {
            m_Name = name;
            m_Destination = p;
            m_Map = map;
            m_Xpos = xpos;
            m_Ypos = ypos;
        }
    }
}
