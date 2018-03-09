using System;
using System.Collections.Generic;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Spells;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Fourth;
using Server.Spells.Fifth;
using Server.Spells.Sixth;
using Server.Spells.Seventh;
using Server.Spells.Eighth;
using Server.Spells.Ninjitsu;
using Server.Misc;
using Server.Regions;
using Server.SkillHandlers;

namespace Server.Mobiles
{
	public class MageEpicAI : BaseAI
	{
		private DateTime m_NextCastTime;

		public MageEpicAI( BaseCreature m ) : base( m )
		{
		}

		public override bool Think()
		{
			if ( m_Mobile.Deleted )
				return false;

			Target targ = m_Mobile.Target;

			if ( targ != null )
			{
				ProcessTarget( targ );

				return true;
			}
			else
			{
				return base.Think();
			}
		}

		private const double HealChance = 0.05; // 5% chance to heal at gm necromancy? Probably not what this does
		private const double TeleportChance = 0.10; // 10% chance to teleport at gm necromancy
		//private const double DispelChance = 0.75; // 75% chance to dispel at gm necromancy
        //private const double ItemChance = 0.0; //0% chance to use an offensive item
        //private const double AbilityChance = 0.0; //0% chance to use special abilities
		
		public virtual double ScaleByMagery( double v )
		{
			return m_Mobile.Skills[SkillName.Magery].Value * v * 0.01;
		}

		public override bool DoActionWander()
		{
			if ( AcquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
			{
				if ( m_Mobile.Debug )
					m_Mobile.DebugSay( "I am going to attack {0}", m_Mobile.FocusMob.Name );

				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;
				m_NextCastTime = DateTime.Now;
			}
			else if ( m_Mobile.Mana < m_Mobile.ManaMax )
			{
				m_Mobile.DebugSay( "I am going to meditate" );

				m_Mobile.UseSkill( SkillName.Meditation );
			}
			else
			{
				m_Mobile.DebugSay( "I am wandering" );

				m_Mobile.Warmode = false;

				base.DoActionWander();

				if ( m_Mobile.Poisoned )
				{
                    new CureSpell( m_Mobile, null ).Cast();
				}
				else if (ScaleByMagery( HealChance ) > Utility.RandomDouble())
				{
					if ( m_Mobile.Hits < (m_Mobile.HitsMax - 50) )
					{
						if ( !new GreaterHealSpell( m_Mobile, null ).Cast() )
							new HealSpell( m_Mobile, null ).Cast();
					}
					else if ( m_Mobile.Hits < (m_Mobile.HitsMax - 10) )
					{
						new HealSpell( m_Mobile, null ).Cast();
					}
				}
			}

			return true;
		}

		public void RunTo( Mobile m )
		{
			if ( m.Paralyzed || m.Frozen )
			{
				if ( m_Mobile.InRange( m, 1 ) )
					RunFrom( m );
				else if ( !m_Mobile.InRange( m, m_Mobile.RangeFight > 2 ? m_Mobile.RangeFight : 2 ) && !MoveTo( m, true, 1 ) )
					OnFailedMove();
			}
			else
			{
				if ( !m_Mobile.InRange( m, m_Mobile.RangeFight ) )
				{
					if ( !MoveTo( m, true, 1 ) )
						OnFailedMove();
				}
				else if ( m_Mobile.InRange( m, m_Mobile.RangeFight - 1 ) )
				{
					RunFrom( m );
				}
			}
		}

		public void RunFrom( Mobile m )
		{
			Run( (m_Mobile.GetDirectionTo( m ) - 4) & Direction.Mask );
		}

		public void OnFailedMove()
		{
			if ( !m_Mobile.DisallowAllMoves && ScaleByMagery( TeleportChance ) > Utility.RandomDouble() )
			{
				if ( m_Mobile.Target != null )
					m_Mobile.Target.Cancel( m_Mobile, TargetCancelType.Canceled );

				new TeleportSpell( m_Mobile, null ).Cast();

				m_Mobile.DebugSay( "I am stuck, I'm going to try teleporting away" );
			}
			else if ( AcquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
			{
				if ( m_Mobile.Debug )
					m_Mobile.DebugSay( "My move is blocked, so I am going to attack {0}", m_Mobile.FocusMob.Name );

				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;
			}
			else
			{
				m_Mobile.DebugSay( "I am stuck" );
			}
		}

		public void Run( Direction d )
		{
			if ( (m_Mobile.Spell != null && m_Mobile.Spell.IsCasting) || m_Mobile.Paralyzed || m_Mobile.Frozen || m_Mobile.DisallowAllMoves )
				return;

			m_Mobile.Direction = d | Direction.Running;

			if ( !DoMove( m_Mobile.Direction, true ) )
				OnFailedMove();
		}

		public virtual Spell GetRandomDamageSpell()
		{
            /*
			int maxCircle = (int)((m_Mobile.Skills[SkillName.Necromancy].Value + 50.0) / (100.0 / 7.0));
			int minCircle = (int)((m_Mobile.Skills[SkillName.Magery].Value + 50.0) / (100.0 / 7.0));
            *///removed since all epics are presumed to be able to cast any spell

            int numSpells = 12;

            switch (numSpells)
			{
				
				case  0: return new HarmSpell( m_Mobile, null );
				case  1: return new FireballSpell( m_Mobile, null );
				case  2: return new LightningSpell( m_Mobile, null );
				case  3: return new CurseSpell( m_Mobile, null );
				case  4: return new FireFieldSpell( m_Mobile, null );
				case  5: return new ParalyzeSpell( m_Mobile, null );
				case  6: return new MindBlastSpell( m_Mobile, null );
				case  7: return new PoisonFieldSpell( m_Mobile, null );
                case  8: return new ParalyzeFieldSpell(m_Mobile, null);
                case  9: return new ChainLightningSpell( m_Mobile, null );
				case 10: return new MeteorSwarmSpell( m_Mobile, null );
				case 11: return new SummonDaemonSpell( m_Mobile, null );
				default: return new ExplosionSpell( m_Mobile, null );
			}
		}

        public virtual Spell GetSmallDamageSpell()
        {
            int maxCircle = (int)((m_Mobile.Skills[SkillName.Magery].Value + 50.0) / (100.0 / 7.0));
            int minCircle = (int)((m_Mobile.Skills[SkillName.Magery].Value + 50.0) / (100.0 / 7.0));
            // m_Mobile.DebugSay("Casting random small spell");
            if (maxCircle < 2 && minCircle < 8)
            {
                maxCircle = 2;
                minCircle = 8;
            }

            switch (Utility.Random(minCircle + (maxCircle * 2)))
            {

                case 0: return new HarmSpell(m_Mobile, null);
                case 1: return new FireballSpell(m_Mobile, null);
                case 2: return new LightningSpell(m_Mobile, null);
                case 3: return new CurseSpell(m_Mobile, null);
                case 4: return new MagicArrowSpell(m_Mobile, null);
                case 5: return new FireFieldSpell(m_Mobile, null);
                case 6: return new WeakenSpell(m_Mobile, null);
                case 7: return new HarmSpell(m_Mobile, null);
                case 8: return new FireballSpell(m_Mobile, null);
                case 9: return new LightningSpell(m_Mobile, null);
                case 10: return new CurseSpell(m_Mobile, null);
                case 11: return new MagicArrowSpell(m_Mobile, null);
                case 12: return new FireFieldSpell(m_Mobile, null);
                default: return new WeakenSpell(m_Mobile, null);
            }
        }

        public virtual Spell DoDispel( Mobile toDispel )
		{
			Spell spell = null;

			if ( !m_Mobile.Summoned && Utility.Random( 0, 4 + (m_Mobile.Hits == 0 ? m_Mobile.HitsMax : (m_Mobile.HitsMax / m_Mobile.Hits)) ) >= 3 )
			{
                if (m_Mobile.Hits < (m_Mobile.HitsMax - 50))
                    spell = new GreaterHealSpell(m_Mobile, null);
                else if (m_Mobile.Hits < (m_Mobile.HitsMax - 20))
                    spell = new HealSpell(m_Mobile, null);
            }

			if ( spell == null )
			{
				if ( !m_Mobile.DisallowAllMoves && Utility.Random( (int)m_Mobile.GetDistanceToSqrt( toDispel ) ) == 0 )
					spell = new TeleportSpell( m_Mobile, null );
				else if ( Utility.Random( 3 ) == 0 && !m_Mobile.InRange( toDispel, 3 ) && !toDispel.Paralyzed && !toDispel.Frozen )
					spell = new ParalyzeSpell( m_Mobile, null );
				else
					spell = new DispelSpell( m_Mobile, null );
			}

			return spell;
		}

		public virtual Spell ChooseSpell( Mobile c )
		{
			Spell spell = null;

			int healChance = (m_Mobile.Hits == 0 ? m_Mobile.HitsMax : (m_Mobile.HitsMax / m_Mobile.Hits));
            int spellResult;
            double witherChance = .05; //05% chance to wither per enemy in range
            m_Mobile.DebugSay("Choosing a Spell");
            spellResult = Utility.Random(4 + healChance);
            m_Mobile.DebugSay("Chose " + spellResult);
            switch ( spellResult )
			{
				case 0: // Heal  myself
				{
                        m_Mobile.DebugSay("0. Heal");
                        if (m_Mobile.Hits < (m_Mobile.HitsMax - 50))
                            spell = new GreaterHealSpell(m_Mobile, null);
                        else if (m_Mobile.Hits < (m_Mobile.HitsMax - 20))
                            spell = new HealSpell(m_Mobile, null);

                        break;
				}
				case 1: // PoisonStrike them
				{
                    if (!c.Poisoned && (!(c is BaseCreature) ||
                      (((BaseCreature)c).PoisonImmune != null && ((BaseCreature)m_Mobile).HitPoison != null &&
                      ((BaseCreature)c).PoisonImmune.Level != null && ((BaseCreature)m_Mobile).HitPoison.Level != null &&
                      ((BaseCreature)c).PoisonImmune.Level < ((BaseCreature)m_Mobile).HitPoison.Level)))
                            if (Utility.RandomDouble() > .5 )
						    spell = new FlameStrikeSpell( m_Mobile, null );
                        else
                            spell = new FireFieldSpell( m_Mobile, null );
                    else
                        spell = new PoisonFieldSpell( m_Mobile, null );

					break;
				}
				case 2: // Deal some damage
				{
                    List<Mobile> targets = new List<Mobile>();

                    BaseCreature cbc = m_Mobile as BaseCreature;
                    bool isMonster = (cbc != null && !cbc.Controlled && !cbc.Summoned);
                    //check if enough Earthquake targets.
                    if (m_Mobile.Map != null)
                        foreach (Mobile m in m_Mobile.GetMobilesInRange(1 + (int)(m_Mobile.Skills[SkillName.Magery].Value / 15.0)))
                            if (m_Mobile != m && SpellHelper.ValidIndirectTarget(m_Mobile, m) && m_Mobile.CanBeHarmful(m, false) && (!Core.AOS || m_Mobile.InLOS(m)))
                                targets.Add(m);

                    if (targets.Count * witherChance > Utility.RandomDouble())
                    {
                        m_Mobile.DebugSay("2. Earthquake");
                        spell = new EarthquakeSpell( m_Mobile, null );
                    }
                    else
                    {
                        spell = GetRandomDamageSpell();
                        m_Mobile.DebugSay("2. Random Spell");
                    }


                    break;
				}
				case 3: // Set up a combo of attacks
				{
					if ( m_Mobile.Mana < 30 && m_Mobile.Mana > 15 )
					{
						if ( c.Paralyzed && !c.Poisoned )
						{
							m_Mobile.DebugSay( "I am going to meditate" );

							m_Mobile.UseSkill( SkillName.Meditation );
						}
						else if (!c.Poisoned && (!(c is BaseCreature) ||
                      (((BaseCreature)c).PoisonImmune != null && ((BaseCreature)m_Mobile).HitPoison != null &&
                      ((BaseCreature)c).PoisonImmune.Level != null && ((BaseCreature)m_Mobile).HitPoison.Level != null && 
                      ((BaseCreature)c).PoisonImmune.Level < ((BaseCreature)m_Mobile).HitPoison.Level)))
                            {
                            m_Mobile.DebugSay("3. Casting Poison");
                            spell = new PoisonSpell( m_Mobile, null );
						}
					}
					else if ( m_Mobile.Mana > 30 && m_Mobile.Mana < 80 )
					{
						if ( Utility.Random( 2 ) == 0 && !c.Paralyzed && !c.Frozen && !c.Poisoned )
						{
                            m_Mobile.DebugSay("3. Curse (Explo)");
                            m_Combo = 0;
							spell = new CurseSpell( m_Mobile, null );
						}
						else
						{
                            m_Mobile.DebugSay("3. Curse (FS)");
                            m_Combo = 1;
							spell = new CurseSpell( m_Mobile, null );
						}
					     
					}
                    else
                    {
                        if (Utility.Random(2) == 0 && !c.Paralyzed && !c.Frozen && !c.Poisoned)
                        {
                            m_Mobile.DebugSay("4. Mana Vampire (Explo)");
                            m_Combo = 0;
                            spell = new ManaVampireSpell(m_Mobile, null);
                        }
                        else
                        {
                            m_Mobile.DebugSay("4. Mana Vampire (FS)");
                            m_Combo = 1;
                            spell = new ManaVampireSpell(m_Mobile, null);
                        }
                    }
                    break;
				}
			
			}

			return spell;
		}

		protected int m_Combo = -1;

		public virtual Spell DoCombo( Mobile c )
		{
			Spell spell = null;
            m_Mobile.DebugSay("Doing Combo");
            if ( m_Combo == 0 )
            {
                m_Mobile.DebugSay(" Casting Explosion and moving to the next spell ");
                spell = new ExplosionSpell( m_Mobile, null );
                ++m_Combo; // Move to next spell
            }
			else if ( m_Combo == 1 )
			{
                spell = new FlameStrikeSpell( m_Mobile, null );
                m_Mobile.DebugSay("Casting Flamestrike and moving to the next spell ");
                ++m_Combo; // Move to next spell
			}
			else if ( m_Combo == 2 )
			{
                m_Mobile.DebugSay("Casting maybe poison and moving to next spell");
                if (!c.Poisoned && (!(c is BaseCreature) ||
                      (((BaseCreature)c).PoisonImmune != null && ((BaseCreature)m_Mobile).HitPoison != null &&
                      ((BaseCreature)c).PoisonImmune.Level != null && ((BaseCreature)m_Mobile).HitPoison.Level != null &&
                      ((BaseCreature)c).PoisonImmune.Level < ((BaseCreature)m_Mobile).HitPoison.Level)))
                    spell = new PoisonSpell( m_Mobile, null );
                else
                    spell = new HarmSpell( m_Mobile, null );

				++m_Combo; // Move to next spell
			}
            else if ( m_Combo == 3 && spell == null )
			{
				switch ( Utility.Random( 3 ) )
				{
					default:
					case 0:
					{
                        m_Mobile.DebugSay( "Random small Spell and next spell" );
                        spell = GetSmallDamageSpell();
                        ++m_Combo; // Move to next spell

						break;
					}
					case 1:
					{
                        m_Mobile.DebugSay("Random small spell and resetting combo");
                        spell = GetSmallDamageSpell();
                        m_Combo = -1; // Reset combo state
						break;
					}
					case 2:
					{
                        m_Mobile.DebugSay("Random small spell and same state");
                        spell = GetSmallDamageSpell();
						break;
					}
				}
			}
			else if ( m_Combo == 4 && spell == null )
			{
                m_Mobile.DebugSay("Casting Flamestrike and resetting combo");
                spell = new FlameStrikeSpell( m_Mobile, null );
				m_Combo = -1;
			}
			return spell;
		}

		public override bool DoActionCombat()
		{
			Mobile c = m_Mobile.Combatant;
			m_Mobile.Warmode = true;

			if ( c == null || c.Deleted || !c.Alive || c.IsDeadBondedPet || !m_Mobile.CanSee( c ) || !m_Mobile.CanBeHarmful( c, false ) || c.Map != m_Mobile.Map )
			{
				// Our combatant is deleted, dead, hidden, or we cannot hurt them
				// Try to find another combatant

				if ( AcquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
				{
					if ( m_Mobile.Debug )
						m_Mobile.DebugSay( "Something happened to my combatant, so I am going to fight {0}", m_Mobile.FocusMob.Name );

					m_Mobile.Combatant = c = m_Mobile.FocusMob;
					m_Mobile.FocusMob = null;
				}
				else
				{
					m_Mobile.DebugSay( "Something happened to my combatant, and nothing is around. I am on guard." );
					Action = ActionType.Guard;
					return true;
				}
			}

			if ( !m_Mobile.InLOS( c ) )
			{
				if ( AcquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
				{
					m_Mobile.Combatant = c = m_Mobile.FocusMob;
					m_Mobile.FocusMob = null;
				}
			}

			if ( !m_Mobile.StunReady && m_Mobile.Skills[SkillName.Wrestling].Value >= 80.0 && m_Mobile.Skills[SkillName.Anatomy].Value >= 80.0 )
				EventSink.InvokeStunRequest( new StunRequestEventArgs( m_Mobile ) );

			if ( !m_Mobile.InRange( c, m_Mobile.RangePerception ) )
			{
				// They are somewhat far away, can we find something else?

				if ( AcquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
				{
					m_Mobile.Combatant = m_Mobile.FocusMob;
					m_Mobile.FocusMob = null;
				}
				else if ( !m_Mobile.InRange( c, m_Mobile.RangePerception * 3 ) )
				{
					m_Mobile.Combatant = null;
				}

				c = m_Mobile.Combatant;

				if ( c == null )
				{
					m_Mobile.DebugSay( "My combatant has fled, so I am on guard" );
					Action = ActionType.Guard;

					return true;
				}
			}

			if ( !m_Mobile.Controlled && !m_Mobile.Summoned )
			{
				if ( m_Mobile.Hits < m_Mobile.HitsMax * 20/100 )
				{
					// We are low on health, should we flee?

					bool flee = false;

					if ( m_Mobile.Hits < c.Hits )
					{
						// We are more hurt than them

						int diff = c.Hits - m_Mobile.Hits;

						flee = ( Utility.Random( 0, 100 ) > (10 + diff) ); // (10 + diff)% chance to flee
					}
					else
					{
						flee = Utility.Random( 0, 100 ) > 10; // 10% chance to flee
					}

					if ( flee )
					{
						if ( m_Mobile.Debug )
							m_Mobile.DebugSay( "I am going to flee from {0}", c.Name );

						Action = ActionType.Flee;
						return true;
					}
				}
			}

			if ( m_Mobile.Spell == null && DateTime.Now > m_NextCastTime && m_Mobile.InRange( c, 12 ) )
			{
				// We are ready to cast a spell

				Spell spell = null;
				Mobile toDispel = FindDispelTarget( true );

				if ( m_Mobile.Poisoned ) // Top cast priority is cure
				{
                    IPooledEnumerable eable = (IPooledEnumerable)m_Mobile.Map.GetItemsInRange(m_Mobile.Location, 1);

                    foreach (object o in eable)
                    {
                        if (o is PoisonFieldSpell.InternalItem)
                        {
                            m_Mobile.Move(m_Mobile.Direction);
                            break;
                        }
                    }
                    spell = new CureSpell( m_Mobile, null );
				}
				else if ( toDispel != null ) // Something dispellable is attacking us
				{
					spell = DoDispel( toDispel );
				}
				else if ( m_Combo != -1 ) // We are doing a spell combo
				{
					spell = DoCombo( c );
				}
				else if ( (c.Spell is HealSpell || c.Spell is GreaterHealSpell) && (!c.Poisoned && (!(c is BaseCreature) ||
                      (((BaseCreature)c).PoisonImmune != null && ((BaseCreature)m_Mobile).HitPoison != null &&
                      ((BaseCreature)c).PoisonImmune.Level != null && ((BaseCreature)m_Mobile).HitPoison.Level != null &&
                      ((BaseCreature)c).PoisonImmune.Level < ((BaseCreature)m_Mobile).HitPoison.Level)))) // They have a heal spell out
				{
					spell = new PoisonSpell( m_Mobile, null );
				}
				else
				{
					spell = ChooseSpell( c );
				}

				// Now we have a spell picked
				// Move first before casting

				if ( toDispel != null )
				{
					if ( m_Mobile.InRange( toDispel, 10 ) )
						RunFrom( toDispel );
					else if ( !m_Mobile.InRange( toDispel, 12 ) )
						RunTo( toDispel );
				}
				else
				{
					RunTo( c );
				}

				if ( spell != null && spell.Cast() )
				{
					TimeSpan delay;

				    delay = spell.GetCastDelay();
					m_NextCastTime = DateTime.Now + delay;
                    m_Mobile.DebugSay("Spell Delay is " + delay );
				}
			}
			else if ( m_Mobile.Spell == null || !m_Mobile.Spell.IsCasting )
			{
				RunTo( c );
			}

			return true;
		}

		public override bool DoActionGuard()
		{
			if ( AcquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
			{
				if ( m_Mobile.Debug )
					m_Mobile.DebugSay( "I am going to attack {0}", m_Mobile.FocusMob.Name );

				m_Mobile.Combatant = m_Mobile.FocusMob;
				Action = ActionType.Combat;
			}
			else
			{
				if ( m_Mobile.Poisoned )
				{
					new CureSpell( m_Mobile, null ).Cast();
				}
				else
				{
                    m_Mobile.DebugSay("Casting some heal spell and guarding");
                    if ( m_Mobile.Hits < (m_Mobile.HitsMax - 50) )
					{
						if ( !new GreaterHealSpell( m_Mobile, null ).Cast() )
							new HealSpell( m_Mobile, null ).Cast();
					}
					else if ( m_Mobile.Hits < (m_Mobile.HitsMax - 10) )
					{
						new HealSpell( m_Mobile, null ).Cast();
					}
					else
					{
						base.DoActionGuard();
					}
				}
			}

			return true;
		}

		public override bool DoActionFlee()
		{
			Mobile c = m_Mobile.Combatant;

			if ( (m_Mobile.Mana > 100 || m_Mobile.Mana == m_Mobile.ManaMax) && m_Mobile.Hits > (m_Mobile.HitsMax / 2) )
			{
				m_Mobile.DebugSay( "I am stronger now, my guard is up" );
				Action = ActionType.Guard;
			}
			else if ( AcquireFocusMob( m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true ) )
			{
				if ( m_Mobile.Debug )
					m_Mobile.DebugSay( "I am scared of {0}", m_Mobile.FocusMob.Name );

				RunFrom( m_Mobile.FocusMob );
				m_Mobile.FocusMob = null;

				if ( m_Mobile.Poisoned && Utility.Random( 0, 5 ) == 0 )
					new CureSpell( m_Mobile, null ).Cast();
			}
			else
			{
				m_Mobile.DebugSay( "Area seems clear, but my guard is up" );

				Action = ActionType.Guard;
				m_Mobile.Warmode = true;
			}

			return true;
		}

		public Mobile FindDispelTarget( bool activeOnly )
		{
			if ( m_Mobile.Deleted || m_Mobile.Int < 95 || CanDispel( m_Mobile ) || m_Mobile.AutoDispel )
				return null;

			if ( activeOnly )
			{
				List<AggressorInfo> aggressed = m_Mobile.Aggressed;
				List<AggressorInfo> aggressors = m_Mobile.Aggressors;

				Mobile active = null;
				double activePrio = 0.0;

				Mobile comb = m_Mobile.Combatant;

				if ( comb != null && !comb.Deleted && comb.Alive && !comb.IsDeadBondedPet && m_Mobile.InRange( comb, 12 ) && CanDispel( comb ) )
				{
					active = comb;
					activePrio = m_Mobile.GetDistanceToSqrt( comb );

					if ( activePrio <= 2 )
						return active;
				}

				for ( int i = 0; i < aggressed.Count; ++i )
				{
					AggressorInfo info = (AggressorInfo)aggressed[i];
					Mobile m = (Mobile)info.Defender;

					if ( m != comb && m.Combatant == m_Mobile && m_Mobile.InRange( m, 12 ) && CanDispel( m ) )
					{
						double prio = m_Mobile.GetDistanceToSqrt( m );

						if ( active == null || prio < activePrio )
						{
							active = m;
							activePrio = prio;

							if ( activePrio <= 2 )
								return active;
						}
					}
				}

				for ( int i = 0; i < aggressors.Count; ++i )
				{
					AggressorInfo info = (AggressorInfo)aggressors[i];
					Mobile m = (Mobile)info.Attacker;

					if ( m != comb && m.Combatant == m_Mobile && m_Mobile.InRange( m, 12 ) && CanDispel( m ) )
					{
						double prio = m_Mobile.GetDistanceToSqrt( m );

						if ( active == null || prio < activePrio )
						{
							active = m;
							activePrio = prio;

							if ( activePrio <= 2 )
								return active;
						}
					}
				}

				return active;
			}
			else
			{
				Map map = m_Mobile.Map;

				if ( map != null )
				{
					Mobile active = null, inactive = null;
					double actPrio = 0.0, inactPrio = 0.0;

					Mobile comb = m_Mobile.Combatant;

					if ( comb != null && !comb.Deleted && comb.Alive && !comb.IsDeadBondedPet && CanDispel( comb ) )
					{
						active = inactive = comb;
						actPrio = inactPrio = m_Mobile.GetDistanceToSqrt( comb );
					}

					foreach ( Mobile m in m_Mobile.GetMobilesInRange( 12 ) )
					{
						if ( m != m_Mobile && CanDispel( m ) )
						{
							double prio = m_Mobile.GetDistanceToSqrt( m );

							if ( !activeOnly && (inactive == null || prio < inactPrio) )
							{
								inactive = m;
								inactPrio = prio;
							}

							if ( (m_Mobile.Combatant == m || m.Combatant == m_Mobile) && (active == null || prio < actPrio) )
							{
								active = m;
								actPrio = prio;
							}
						}
					}

					return active != null ? active : inactive;
				}
			}

			return null;
		}

        public bool CanDispel(Mobile m)
        {
            return ((TransformationSpellHelper.GetContext(m) != null) || !m.CanBeginAction(typeof(PolymorphSpell)) || AnimalForm.UnderTransformation(m) ||
                TransformationSpellHelper.UnderTransformation(m) ||
                (m is BaseCreature && ((BaseCreature)m).Summoned && m_Mobile.CanBeHarmful(m, false) && !((BaseCreature)m).IsAnimatedDead));
        }

        private static int[] m_Offsets = new int[]
			{
				-1, -1,
				-1,  0,
				-1,  1,
				 0, -1,
				 0,  1,
				 1, -1,
				 1,  0,
				 1,  1,

				-2, -2,
				-2, -1,
				-2,  0,
				-2,  1,
				-2,  2,
				-1, -2,
				-1,  2,
				 0, -2,
				 0,  2,
				 1, -2,
				 1,  2,
				 2, -2,
				 2, -1,
				 2,  0,
				 2,  1,
				 2,  2
			};

		private void ProcessTarget( Target targ )
		{
			bool isDispel = ( targ is DispelSpell.InternalTarget );
			bool isParalyze = ( targ is ParalyzeSpell.InternalTarget );
			bool isTeleport = ( targ is TeleportSpell.InternalTarget );
            bool isFireField = ( targ is FireFieldSpell.InternalTarget );
            bool isPoisonField = ( targ is PoisonFieldSpell.InternalTarget );
			bool teleportAway = false;

			Mobile toTarget;

			if ( isDispel )
			{
				toTarget = FindDispelTarget( false );

				if ( toTarget != null )
					RunTo( toTarget );
				else if ( toTarget != null && m_Mobile.InRange( toTarget, 10 ) )
					RunFrom( toTarget );
			}
			else if (isParalyze || isTeleport)
			{
				toTarget = FindDispelTarget( true );

				if ( toTarget == null )
				{
					toTarget = m_Mobile.Combatant;

					if ( toTarget != null )
						RunTo( toTarget );
				}
				else if (toTarget != null && m_Mobile.InRange( toTarget, 10 ) )
				{
					RunFrom( toTarget );
					teleportAway = true;
				}
				else
				{
					teleportAway = true;
				}
			}
			else
			{
				toTarget = m_Mobile.Combatant;

				if ( toTarget != null )
					RunTo( toTarget );
			}

			if ( (toTarget != null && ( (targ.Flags & TargetFlags.Harmful) != 0) || (isPoisonField || isFireField ) ) )
			{
				if ( (targ.Range == -1 || m_Mobile.InRange( toTarget, targ.Range )) && m_Mobile.CanSee( toTarget ) && m_Mobile.InLOS( toTarget ) )
				{
					targ.Invoke( m_Mobile, toTarget );
				}
				else if ( isDispel )
				{
					targ.Cancel( m_Mobile, TargetCancelType.Canceled );
				}
			}
			else if ( (targ.Flags & TargetFlags.Beneficial) != 0 )
			{
				targ.Invoke( m_Mobile, m_Mobile );
			}
			else if ( isTeleport && toTarget != null )
			{
				Map map = m_Mobile.Map;

				if ( map == null )
				{
					targ.Cancel( m_Mobile, TargetCancelType.Canceled );
					return;
				}

				int px, py;

				if ( teleportAway )
				{
					int rx = m_Mobile.X - toTarget.X;
					int ry = m_Mobile.Y - toTarget.Y;

					double d = m_Mobile.GetDistanceToSqrt( toTarget );

					px = toTarget.X + (int)(rx * (10 / d));
					py = toTarget.Y + (int)(ry * (10 / d));
				}
				else
				{
					px = toTarget.X;
					py = toTarget.Y;
				}

				for ( int i = 0; i < m_Offsets.Length; i += 2 )
				{
					int x = m_Offsets[i], y = m_Offsets[i + 1];

					Point3D p = new Point3D( px + x, py + y, 0 );

					LandTarget lt = new LandTarget( p, map );

					if ( (targ.Range == -1 || m_Mobile.InRange( p, targ.Range )) && m_Mobile.InLOS( lt ) && map.CanSpawnMobile( px + x, py + y, lt.Z ) && !SpellHelper.CheckMulti( p, map ) )
					{
						targ.Invoke( m_Mobile, lt );
						return;
					}
				}

				int teleRange = targ.Range;

				if ( teleRange < 0 )
					teleRange = 12;

				for ( int i = 0; i < 10; ++i )
				{
					Point3D randomPoint = new Point3D( m_Mobile.X - teleRange + Utility.Random( teleRange * 2 + 1 ), m_Mobile.Y - teleRange + Utility.Random( teleRange * 2 + 1 ), 0 );

					LandTarget lt = new LandTarget( randomPoint, map );

					if ( m_Mobile.InLOS( lt ) && map.CanSpawnMobile( lt.X, lt.Y, lt.Z ) && !SpellHelper.CheckMulti( randomPoint, map ) )
					{
						targ.Invoke( m_Mobile, new LandTarget( randomPoint, map ) );
						return;
					}
				}

				targ.Cancel( m_Mobile, TargetCancelType.Canceled );
			}
			else
			{
				targ.Cancel( m_Mobile, TargetCancelType.Canceled );
			}
		}
	}
}
