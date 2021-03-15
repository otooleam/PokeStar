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
      private const int REGULAR_COUNTERS = 0;

      /// <summary>
      /// Index of special counters list.
      /// </summary>
      private const int SPECIAL_COUNTERS = 1;

      /// <summary>
      /// Number of results to return.
      /// </summary>
      private const int RESULTS = 6;

      /// <summary>
      /// Number of decimal points to round rating to.
      /// </summary>
      private const int ROUND = 8;

      /// <summary>
      /// Runs the counter simulation against a boss using raiders.
      /// </summary>
      /// <param name="boss">Boss to fight against.</param>
      /// <param name="raiders">Raiders used to attack the boss.</param>
      /// <returns>Tuple where item1 is the list of normal counters and itme2 is the list of special counters.</returns>
      public static Tuple<List<Counter>, List<Counter>> RunSim(Tuple<Pokemon, bool> boss, List<Tuple<Pokemon, bool>> raiders)
      {
         Tuple<List<Counter>, List<Counter>> fullCounters = new Tuple<List<Counter>, List<Counter>>(new List<Counter>(), new List<Counter>());

         for (int mode = 0; mode < 2; mode++)
         {
            List<Counter> counters = new List<Counter>();
            TypeRelation bossTypes = Connections.Instance().GetTypeDefenseRelations(boss.Item1.Type);
            foreach (Tuple<Pokemon, bool> raider in raiders)
            {
               if (raider.Item1.Released &&
                  (mode == REGULAR_COUNTERS && !raider.Item2 && !raider.Item1.Name.Contains("Mega ") ||
                  (mode == SPECIAL_COUNTERS && (raider.Item2 || raider.Item1.Name.Contains("Mega "))))
               )
               {
                  List<Counter> allCounters = new List<Counter>();
                  TypeRelation raiderTypes = Connections.Instance().GetTypeDefenseRelations(raider.Item1.Type);
                  foreach (Move raiderFast in raider.Item1.FastMove)
                  {
                     foreach (Move raiderCharge in raider.Item1.ChargeMove)
                     {
                        SimPokemon raiderSim = new SimPokemon
                        {
                           Number = raider.Item1.Number,
                           Name = raider.Item1.Name,
                           Fast = raiderFast,
                           Charge = raiderCharge,
                           FastEffect = TypeCalculator.GetMultiplier(bossTypes, raiderFast.Type),
                           ChargeEffect = TypeCalculator.GetMultiplier(bossTypes, raiderCharge.Type),
                           FastStab = raider.Item1.Type.Contains(raiderFast.Type) ? 1.2 : 1.0,
                           ChargeStab = raider.Item1.Type.Contains(raiderCharge.Type) ? 1.2 : 1.0,
                           StadowAtkMul = raider.Item2 ? (6.0 / 5.0) : 1.0,
                           StadowDefMul = raider.Item2 ? (5.0 / 6.0) : 1.0,
                           AtkStat = (int)((raider.Item1.Attack + 15) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1]),
                           DefStat = (int)((raider.Item1.Defense + 15) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1]),
                           StamStat = (int)((raider.Item1.Stamina + 15) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1])
                        };

                        SimPokemon bossSim = new SimPokemon
                        {
                           Number = boss.Item1.Number,
                           Name = boss.Item1.Name,
                           StadowAtkMul = 1.0,
                           StadowDefMul = 1.0,
                           AtkStat = (int)((boss.Item1.Attack + 15) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1]),
                           DefStat = (int)((boss.Item1.Defense + 15) * Global.DISCRETE_CPM[Global.MAX_REG_LEVEL - 1]),
                           StamStat = boss.Item1.Name.Contains("Mega ") ? 22500 : 15000
                        };

                        double x = 0.0;
                        double y = 0.0;
                        double moveSets = 1;

                        List<Move> bossFastMoves = boss.Item1.FastMove.Where(m => !m.IsLegacy).ToList();
                        List<Move> bossChargeMoves = boss.Item1.ChargeMove.Where(m => !m.IsLegacy).ToList();

                        if (boss.Item1.Defense == 0 || bossFastMoves.Count + bossChargeMoves.Count == 0)
                        {
                           if (boss.Item1.Defense == 0)
                           {
                              bossSim.DefStat = 150;
                           }
                           x = (0.5 * raiderSim.Fast.PvEEnergy) + (0.5 * raiderSim.Charge.PvEEnergy);
                           y = 900.0 / raiderSim.DefStat;
                        }
                        else
                        {
                           moveSets = boss.Item1.FastMove.Count * boss.Item1.ChargeMove.Count;
                           foreach (Move bossFast in bossFastMoves)
                           {
                              bossSim.Fast = bossFast;
                              bossSim.FastEffect = TypeCalculator.GetMultiplier(raiderTypes, bossFast.Type);
                              bossSim.FastStab = boss.Item1.Type.Contains(bossFast.Type) ? 1.2 : 1.0;
                              foreach (Move bossCharge in bossChargeMoves)
                              {
                                 bossSim.Charge = bossCharge;
                                 bossSim.ChargeEffect = TypeCalculator.GetMultiplier(raiderTypes, bossCharge.Type);
                                 bossSim.ChargeStab = boss.Item1.Type.Contains(bossCharge.Type) ? 1.2 : 1.0;

                                 Tuple<double, double> inputs = CalcDPSInput(bossSim, raiderSim);
                                 x += inputs.Item1;
                                 y += inputs.Item2;
                              }
                           }
                        }

                        allCounters.Add(CalcDps(bossSim, raiderSim, x / moveSets, y / moveSets));
                     }
                  }
                  if (allCounters.Count != 0)
                  {
                     counters.Add(allCounters.OrderByDescending(x => x.Rating).First());
                  }
               }
            }

            if (mode == REGULAR_COUNTERS)
            {
               fullCounters.Item1.AddRange(counters.OrderByDescending(x => x.Rating).ThenBy(x => x.Name).Take(RESULTS).ToList());
            }
            else
            {
               fullCounters.Item2.AddRange(counters.OrderByDescending(x => x.Rating).ThenBy(x => x.Name).Take(RESULTS).ToList());
            }
         }
         return fullCounters;
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
         double CEPS = (CE >= 100 ? CE + (0.5 * FE) + (0.5 * y * CDWS) : CE) / CDur;

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
      /// <returns>Tuple where item1 is the extra energy (x) and item2 is the boss DPS (y).</returns>
      private static Tuple<double, double> CalcDPSInput(SimPokemon boss, SimPokemon raider)
      {
         double FDmg = CalcDamage(raider, boss, true);
         double CDmg = CalcDamage(raider, boss, false);
         double CE = boss.Charge.PvEEnergy;

         double FDur = boss.Fast.Cooldown / 1000.0;
         double CDur = boss.Charge.Cooldown / 1000.0;
         double n = Math.Max(1.0, 3.0 * CE / 100.0);

         return new Tuple<double, double>(
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