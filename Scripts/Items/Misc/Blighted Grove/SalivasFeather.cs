using System;
using Server;

namespace Server.Items
{
	public class SalivasFeather : Item
	{
		public override int LabelNumber{ get{ return 1074234; } } // Saliva's Feather

		[Constructable]
		public SalivasFeather() : base( 0x1020 )
		{
			LootType = LootType.Blessed;
			Hue = 0x5C;
            Weight = 0.01;
            Timer.DelayCall(TimeSpan.FromHours(3.0), new TimerStateCallback(DeleteKey), this);
        }

        public void DeleteKey(object state)
        {
            Item from = (Item)state;
            from.Delete();
        }

        public SalivasFeather( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}

