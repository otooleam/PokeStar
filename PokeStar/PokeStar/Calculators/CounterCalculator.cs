using System;
using System.Linq;
using System.Collections.Generic;
using PokeStar.DataModels;
using PokeStar.ConnectionInterface;

namespace PokeStar.Calculators
{
   /// <summary>
   /// Calculates counters for Pokémon.
   /// </summary>
   public static class CounterCalculator
   {
      /// <summary>
      /// Index of normal counters list.
      /// </summary>
      private const int REGULAR_COUNTER_INDEX = 0;

      /// <summary>
      /// Index of special counters list.
      /// </summary>
      private const int SPECIAL_COUNTER_INDEX = 1;

      /// <summary>
      /// Number of results to return.
      /// </summary>
      private const int RESULTS = 6;

      /// <summary>
      /// Number of decimal points to round rating to.
      /// </summary>
      private const int ROUND = 8;

      /// <summary>
      /// Default boss DPS multiplier.
      /// </summary>
      private const int DEFAULT_DPS = 900;

      /// <summary>
      /// Default boss defense stat.
      /// </summary>
      private const int DEFAULT_DEF = 150;

      /// <summary>
      /// One bar charge move energy.
      /// </summary>
      private const int ONE_BAR_ENERGY = 100;

      /// <summary>
      /// Same type attack bonus multiplier.
      /// </summary>
      private const double STAB_MULTIPLIER = 1.2;

      /// <summary>
      /// Shadow Pokémon attack multiplier
      /// </summary>
      private const double SHADOW_ATK_MUL = 6.0 / 5.0;

      /// <summary>
      /// Shadow Pokémon defense multiplier.
      /// </summary>
      private const double SHADOW_DEF_MUL = 5.0 / 6.0;

      /// <summary>
      /// Runs the counter simulation against a boss using raiders.
      /// </summary>
      /// <param name="boss">Boss to fight against.</param>
      /// <param name="raiders">Raiders used to attack the boss.</param>
      /// <returns>Counter calculation results.</returns>
      public static CounterCalcResults RunSim(Pokemon boss, List<Pokemon> raiders)
      {
         CounterCalcResults results = new CounterCalcResults();

         for (int mode = 0; mode < 2; mode++)
         {
            List<Counter> counters = new List<Counter>();
            TypeRelation bossTypes = Connections.Instance().GetTypeDefenseRelations(boss.Type);
            foreach (Pokemon raider in raiders)
            {
               if (raider.Released &&
                  (mode == REGULAR_COUNTER_INDEX && !raider.Name.Contains("Mega ") ||
                  (mode == SPECIAL_COUNTER_INDEX && (raider.Shadow || raider.Name.Contains("Mega "))))
               )
               {
                  List<Counter> allCounters = new List<Counter>();
                  TypeRelation raiderTypes = Connections.Instance().GetTypeDefenseRelations(raider.Type);
                  foreach (Move raiderFast in raider.FastMove)
                  {
                     foreach (Move raiderCharge in raider.ChargeMove)
                     {
                        if ((!raiderCharge.Name.Equals(Global.SHADOW_MOVES.ElementAt(Global.PURIFIED_INDEX).Name, StringComparison.OrdinalIgnoreCase) &&
                             !raiderCharge.Name.Equals(Global.SHADOW_MOVES.ElementAt(Global.SHADOW_INDEX).Name, StringComparison.OrdinalIgnoreCase)) ||
                            (raiderCharge.Name.Equals(Global.SHADOW_MOVES.ElementAt(Global.PURIFIED_INDEX).Name, StringComparison.OrdinalIgnoreCase) && mode == REGULAR_COUNTER_INDEX) ||
                            (raiderCharge.Name.Equals(Global.SHADOW_MOVES.ElementAt(Global.SHADOW_INDEX).Name, StringComparison.OrdinalIgnoreCase) && mode == SPECIAL_COUNTER_INDEX))
                        {

                           SimPokemon raiderSim = new SimPokemon
                           {
                              Number = raider.Number,
                              Name = raider.Name,
                              Fast = raiderFast,
                              Charge = raiderCharge,
                              FastEffect = TypeCalculator.GetMultiplier(bossTypes, raiderFast.Type),
                              ChargeEffect = TypeCalculator.GetMultiplier(bossTypes, raiderCharge.Type),
                              FastStab = raider.Type.Contains(raiderFast.Type) ? STAB_MULTIPLIER : 1.0,
                              ChargeStab = raider.Type.Contains(raiderCharge.Type) ? STAB_MULTIPLIER : 1.0,
                              StadowAtkMul = mode == SPECIAL_COUNTER_INDEX && raider.Shadow ? SHADOW_ATK_MUL : 1.0,
                              StadowDefMul = mode == SPECIAL_COUNTER_INDEX && raider.Shadow ? SHADOW_DEF_MUL : 1.0,
                              AtkStat = (int)((raider.Attack + Global.MAX_IV) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1]),
                              DefStat = (int)((raider.Defense + Global.MAX_IV) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1]),
                              StamStat = (int)((raider.Stamina + Global.MAX_IV) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1])
                           };

                           SimPokemon bossSim = new SimPokemon
                           {
                              Number = boss.Number,
                              Name = boss.Name,
                              StadowAtkMul = 1.0,
                              StadowDefMul = 1.0,
                              AtkStat = (int)((boss.Attack + Global.MAX_IV) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1]),
                              DefStat = (int)((boss.Defense + Global.MAX_IV) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1]),
                              StamStat = (int)((boss.Stamina + Global.MAX_IV) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1])
                           };

                           double x = 0.0;
                           double y = 0.0;
                           double moveSets = 1;

                           List<Move> bossFastMoves = boss.FastMove.Where(m => !m.IsLegacy).ToList();
                           List<Move> bossChargeMoves = boss.ChargeMove.Where(m => !m.IsLegacy).ToList();

                           if (boss.Defense == 0 || bossFastMoves.Count + bossChargeMoves.Count == 0)
                           {
                              if (boss.Defense == 0)
                              {
                                 bossSim.DefStat = DEFAULT_DEF;
                              }
                              x = (0.5 * raiderSim.Fast.PvEEnergy) + (0.5 * raiderSim.Charge.PvEEnergy);
                              y = DEFAULT_DPS / (double)raiderSim.DefStat;
                           }
                           else
                           {
                              moveSets = boss.FastMove.Count * boss.ChargeMove.Count;
                              foreach (Move bossFast in bossFastMoves)
                              {
                                 bossSim.Fast = bossFast;
                                 bossSim.FastEffect = TypeCalculator.GetMultiplier(raiderTypes, bossFast.Type);
                                 bossSim.FastStab = boss.Type.Contains(bossFast.Type) ? STAB_MULTIPLIER : 1.0;
                                 foreach (Move bossCharge in bossChargeMoves)
                                 {
                                    if (!bossCharge.Name.Equals(Global.SHADOW_MOVES.ElementAt(Global.PURIFIED_INDEX).Name, StringComparison.OrdinalIgnoreCase) &&
                                        !bossCharge.Name.Equals(Global.SHADOW_MOVES.ElementAt(Global.SHADOW_INDEX).Name, StringComparison.OrdinalIgnoreCase))
                                    {
                                       bossSim.Charge = bossCharge;
                                       bossSim.ChargeEffect = TypeCalculator.GetMultiplier(raiderTypes, bossCharge.Type);
                                       bossSim.ChargeStab = boss.Type.Contains(bossCharge.Type) ? STAB_MULTIPLIER : 1.0;

                                       DPSInput inputs = CalcDPSInput(bossSim, raiderSim);
                                       x += inputs.X;
                                       y += inputs.Y;
                                    }
                                 }
                              }
                           }

                           allCounters.Add(CalcDps(bossSim, raiderSim, x / moveSets, y / moveSets));
                        }
                     }
                  }
                  if (allCounters.Count != 0)
                  {
                     counters.Add(allCounters.OrderByDescending(x => x.Rating).First());
                  }
               }
            }

            if (mode == REGULAR_COUNTER_INDEX)
            {
               results.Regular = counters.OrderByDescending(x => x.Rating).ThenBy(x => x.Name).Take(RESULTS).ToList();
            }
            else
            {
               results.Special = counters.OrderByDescending(x => x.Rating).ThenBy(x => x.Name).Take(RESULTS).ToList();
            }
         }
         return results;
      }

      /// <summary>
      /// Calculate the DPS against a boss Pokémon.
      /// </summary>
      /// <param name="boss">Boss simulation Pokémon.</param>
      /// <param name="raider">Raider simulation Pokémon.</param>
      /// <param name="x">Energy left after battle.</param>
      /// <param name="y">Boss DPS against raider.</param>
      /// <returns></returns>
      private static Counter CalcDps(SimPokemon boss, SimPokemon raider, double x, double y)
      {
         double FDmg = CalcDamage(boss, raider, true);
         double CDmg = CalcDamage(boss, raider, false);
         double FE = raider.Fast.PvEEnergy;
         double CE = raider.Charge.PvEEnergy;
         double FDur = raider.Fast.Cooldown / 1000.0;
         double CDur = raider.Charge.Cooldown / 1000.0;
         double CDWS = raider.Fast.DamageWindowStart / 1000.0;

         double FDPS = FDmg / FDur;
         double FEPS = FE / FDur;
         double CDPS = CDmg / CDur;
         double CEPS = (CE >= ONE_BAR_ENERGY ? CE + (0.5 * FE) + (0.5 * y * CDWS) : CE) / CDur;

         double dps = (((FDPS * CEPS) + (CDPS * FEPS)) / (CEPS + FEPS)) + (((CDPS - FDPS) / (CEPS + FEPS)) * ((0.5 - (x / raider.StamStat)) * y));

         if (dps > CDPS)
         {
            dps = CDPS;
         }
         else if (dps < FDPS)
         {
            dps = FDPS;
         }

         double tdo = dps * (raider.StamStat / y);

         return new Counter
         {
            Name = raider.Name,
            FastAttack = new Move() { Name = raider.Fast.Name },
            ChargeAttack = new Move() { Name = raider.Charge.Name },
            Rating = Math.Round((Math.Pow(dps, 3) / 1000.0) * tdo, ROUND)
         };
      }

      /// <summary>
      /// Calculate the input DPS from a boss Pokémon.
      /// </summary>
      /// <param name="boss">Boss simulation Pokémon.</param>
      /// <param name="raider">Raider simulation Pokémon.</param>
      /// <returns>Input values for DPS calculation.</returns>
      private static DPSInput CalcDPSInput(SimPokemon boss, SimPokemon raider)
      {
         double FDmg = CalcDamage(raider, boss, true);
         double CDmg = CalcDamage(raider, boss, false);
         double CE = boss.Charge.PvEEnergy;

         double FDur = boss.Fast.Cooldown / 1000.0;
         double CDur = boss.Charge.Cooldown / 1000.0;
         double n = Math.Max(1.0, 3.0 * CE / 100.0);

         return new DPSInput (
            (raider.Charge.PvEEnergy * 0.5) + (raider.Fast.PvEEnergy * 0.5) + (0.5 * (((n * FDmg) + CDmg) / (n + 1))),
            ((n * FDmg) + CDmg) / ((n * (FDur + 2.0)) + (CDur + 2.0))
         );
      }

      /// <summary>
      /// Calculate damage done by a Pokémon.
      /// </summary>
      /// <param name="def">Defending simulation Pokémon.</param>
      /// <param name="atk">Attacking simulation Pokémon.</param>
      /// <param name="useFastMove">Calculate fast move damage, otherwise use charge move.</param>
      /// <returns>Damage done by the desired attack.</returns>
      private static double CalcDamage(SimPokemon def, SimPokemon atk, bool useFastMove)
      {
         int power = useFastMove ? atk.Fast.PvEPower : atk.Charge.PvEPower;
         double stab = useFastMove ? atk.FastStab : atk.ChargeStab;
         double effect = def.Number == 0 ? 1.0 : useFastMove ? atk.FastEffect : atk.ChargeEffect;
         double mul = effect * stab;

         return Math.Floor(0.5 * power * ((atk.StadowAtkMul * atk.AtkStat) / (def.StadowDefMul * def.DefStat)) * mul) + 1.0;
      }
   }
}