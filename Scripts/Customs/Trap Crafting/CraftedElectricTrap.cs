using System;
using System.Collections;
using Server;
using Server.Network;
using Server.Targeting;
using Server.Spells;

namespace Server.Items
{
	public class CraftedElectricTrap : CraftedTrap
	{
		[Constructable]
		public CraftedElectricTrap()
		{
            ItemID = 3629;
            Visible = false;
			Hue = 6;
			UsesRemaining = 1;
            Name = "A static-jolt trap";
			TrapPower = 100;
            DamageScalar = .1;
            TriggerRange = 3;
            DamageRange = 3;
            ManaCost = 20;
            BonusSkill = SkillName.Inscribe;
        }

		public CraftedElectricTrap( Serial serial ) : base( serial )
		{
            Visible = false;
            Hue = 6;
            UsesRemaining = 1;
            Name = "A static-jolt trap";
            TrapPower = 100;
            DamageScalar = .1;
            TriggerRange = 3;
            DamageRange = 3;
            ManaCost = 20;
            BonusSkill = SkillName.Inscribe;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}