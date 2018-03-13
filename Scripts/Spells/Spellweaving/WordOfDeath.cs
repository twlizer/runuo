using System;
using Server.Targeting;
using Server.Network;

namespace Server.Spells.Spellweaving
{
	public class WordOfDeathSpell : ArcanistSpell
	{
		private static SpellInfo m_Info = new SpellInfo( "Word of Death", "Nyraxle", -1 );

		public override TimeSpan CastDelayBase { get { return TimeSpan.FromSeconds( 3.5 ); } }

		public override double RequiredSkill { get { return 80.0; } }
		public override int RequiredMana { get { return 50; } }

		public WordOfDeathSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget( this );
		}

		public void Target( Mobile m )
		{
			if( !Caster.CanSee( m ) )
			{
				Caster.SendLocalizedMessage( 500237 ); // Target can not be seen.
			}
			else if( CheckHSequence( m ) )
			{
				Point3D loc = m.Location;
				loc.Z += 50;

				m.PlaySound( 0x211 );
				m.FixedParticles( 0x3779, 1, 30, 0x26EC, 0x3, 0x3, EffectLayer.Waist );

				Effects.SendMovingParticles( new Entity( Serial.Zero, loc, m.Map ), new Entity( Serial.Zero, m.Location, m.Map ), 0xF5F, 1, 0, true, false, 0x21, 0x3F, 0x251D, 0, 0, EffectLayer.Head, 0 );

				double percentage = 0.05 * FocusLevel;
                double inscribeSkill = GetInscribeSkill(Caster);
                int damage;

				if( !m.Player && (((double)m.Hits / (double)m.HitsMax) < percentage ))
				{
					damage = 300;
                    Caster.SendMessage(String.Format("under percentage {0}", percentage));
				}
				else
				{
                    Caster.SendMessage(String.Format("over percentage {0}", percentage));
                    int minDamage = (int)Caster.Skills.Spellweaving.Value / 5;
					int maxDamage = (int)Caster.Skills.Spellweaving.Value / 3;
					damage = Utility.RandomMinMax(minDamage, maxDamage);
				}

                int damageBonus = 0;
                int sdiBonus = AosAttributes.GetValue(Caster, AosAttribute.SpellDamage);
                int intBonus = Caster.Int / 10;
                int inscribeBonus = (int)(inscribeSkill + (100 * (int)(inscribeSkill / 100))) / 10;
                TransformContext context = TransformationSpellHelper.GetContext(Caster);
                if (context != null && context.Spell is ReaperFormSpell)
                    damageBonus += ((ReaperFormSpell)context.Spell).SpellDamageBonus;
                if (m.Player && sdiBonus > 15 + ((int)inscribeSkill) / 10)
                    sdiBonus = 15 + ((int)inscribeSkill) / 10;
                damageBonus += Spellweaving.ArcaneEmpowermentSpell.GetSpellBonus(m, m.Player);
                damageBonus += inscribeBonus + intBonus;

                damage *= damageBonus + 100;
                damage /= 100;

                int[] types = new int[4];
				types[Utility.Random( types.Length )] = 100;

				SpellHelper.Damage( this, m, damage, 0, types[0], types[1], types[2], types[3] );	//Chaos damage.  Random elemental damage
			}

			FinishSequence();
		}

		public class InternalTarget : Target
		{
			private WordOfDeathSpell m_Owner;

			public InternalTarget( WordOfDeathSpell owner ) : base( 10, false, TargetFlags.Harmful )
			{
				m_Owner = owner;
			}

			protected override void OnTarget( Mobile m, object o )
			{
				if( o is Mobile )
				{
					m_Owner.Target( (Mobile)o );
				}
			}

			protected override void OnTargetFinish( Mobile m )
			{
				m_Owner.FinishSequence();
			}
		}
	}
}