
////////////////////////////////////////
//                                    //
//   Generated by CEO's YAAAG - V1.2  //
// (Yet Another Arya Addon Generator) //
//                                    //
////////////////////////////////////////
using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class SmithBazaarAddon : BaseAddon
	{
        private static int[,] m_AddOnSimpleComponents = new int[,] {
			  {1302, 2, 0, 0}, {1302, 2, -1, 0}, {7144, 1, -1, 0}// 1	2	3	
			, {4017, -1, -1, 0}, {6787, 2, -1, 0}, {6786, 2, 0, 0}// 4	5	6	
			, {1302, 1, 0, 0}, {1302, 1, 2, 0}, {1302, 2, 1, 0}// 7	8	9	
			, {1302, 1, 1, 0}, {1302, 1, -1, 0}, {1302, 2, 2, 0}// 10	11	12	
			, {4022, 0, -1, 0}, {4015, -1, 0, 0}, {4024, -1, 0, 5}// 13	14	15	
			, {1302, -1, 1, 0}, {1302, -1, 0, 0}, {1302, -1, -1, 0}// 16	17	18	
			, {1302, 0, 2, 0}, {1302, 0, 1, 0}, {1302, 0, 0, 0}// 19	20	21	
			, {4027, 0, 0, 0}, {7159, -1, 1, 0}, {1302, 0, -1, 0}// 22	23	24	
			, {1302, -1, 2, 0}// 25	
		};

 
            
		public override BaseAddonDeed Deed
		{
			get
			{
				return new SmithBazaarAddonDeed();
			}
		}

		[ Constructable ]
		public SmithBazaarAddon()
		{

            for (int i = 0; i < m_AddOnSimpleComponents.Length / 4; i++)
                AddComponent( new AddonComponent( m_AddOnSimpleComponents[i,0] ), m_AddOnSimpleComponents[i,1], m_AddOnSimpleComponents[i,2], m_AddOnSimpleComponents[i,3] );


		}

		public SmithBazaarAddon( Serial serial ) : base( serial )
		{
		}


		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class SmithBazaarAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new SmithBazaarAddon();
			}
		}

		[Constructable]
		public SmithBazaarAddonDeed()
		{
			Name = "SmithBazaar";
		}

		public SmithBazaarAddonDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void	Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}