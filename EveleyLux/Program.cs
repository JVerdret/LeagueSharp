using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace EveleyLux
{
    class Program
    {
        public static Spell q_, w_, e_, r_;
        public static Menu menu_;
        public static SpellSlot igniteslot_;
        //public static SpellSlot barrierslot_; need to find name
        public static Orbwalking.Orbwalker orbwalker_;
        //public static Vector3 castpos; Usefull for later i think
        public static GameObject lgo_;
        static void Main(string[] args)
        {
            if (ObjectManager.Player.BaseSkinName == "Lux")
            {
                Game.PrintChat("EveleyLux Loaded");
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            }
            else
            {
                Game.PrintChat("It's absolutly not Lux ! ");
                Game.PrintChat("EveleyLux has not been loaded");
                return;
            }
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            Game.PrintChat("Let's make them disappear !");
            q_ = new Spell(SpellSlot.Q, 1175f);
            w_ = new Spell(SpellSlot.W, 1075f);
            e_ = new Spell(SpellSlot.E, 1100f);
            r_ = new Spell(SpellSlot.R, 3340f);
            //public void SetSkillshot(float delay, float width, float speed, bool collision, SkillshotType type, Vector3 from = default(Vector3), Vector3 rangeCheckFrom = default(Vector3));
            q_.SetSkillshot(0.25f, 60f, 1150f, false, SkillshotType.SkillshotLine);
            w_.SetSkillshot(0.25f, 110f, 1200f, false, SkillshotType.SkillshotLine);
            e_.SetSkillshot(0.25f, 200f, 950f, false, SkillshotType.SkillshotCircle);
            r_.SetSkillshot(1f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);
            igniteslot_ = ObjectManager.Player.GetSpellSlot("SummonerDot");


            (menu_ = new Menu("EveleyLux", "Lux", true)).AddToMainMenu();
            orbwalker_ = new Orbwalking.Orbwalker(menu_.AddSubMenu(new Menu("Orbwalker", "Orbwalker")));
            TargetSelector.AddToMenu(menu_.AddSubMenu(new Menu("Target Selector", "Target Selector")));
            var combo = menu_.AddSubMenu(new Menu("Combo", "Combo"));
            var laneclear = menu_.AddSubMenu(new Menu("Laneclear", "Laneclear"));
            var harass = menu_.AddSubMenu(new Menu("Harass", "Harass"));
            var jungleclear = menu_.AddSubMenu(new Menu("Jumgleclear", "Jungleclear"));
            var drawing = menu_.AddSubMenu(new Menu("Drawing", "Drawing"));
            combo.SubMenu("Q Settings").AddItem(new MenuItem("cqu", "Use Q").SetValue(true));
            combo.SubMenu("Q Settings").AddItem(new MenuItem("autoq", "Auto Q if cc").SetValue(true));
            combo.SubMenu("W Settings").AddItem(new MenuItem("cwu", "Use W").SetValue(true));
            combo.SubMenu("W Settings").AddItem(new MenuItem("autow", "Auto W if targetted").SetValue(true));
            combo.SubMenu("E Settings").AddItem(new MenuItem("ceu", "Use E").SetValue(true));
            combo.SubMenu("R Settings").AddItem(new MenuItem("semir", "Semi-Auto R").SetValue(new KeyBind('M', KeyBindType.Press)));
            combo.SubMenu("R Settings").AddItem(new MenuItem("cru", "Use R").SetValue(true));
            combo.SubMenu("R Settings").AddItem(new MenuItem("craoeu", "Use Special R").SetValue(false));
            combo.SubMenu("R Settings").AddItem(new MenuItem("crehc", "Ennemy Hits").SetValue(new Slider(3, 5, 1)));
            combo.SubMenu("R Settings").AddItem(new MenuItem("crq", "Auto R if Q").SetValue(false));
            combo.SubMenu("Summoners Settings").AddItem(new MenuItem("cui", "Use Ignite").SetValue(true));
            combo.SubMenu("Summoners Settings").AddItem(new MenuItem("cie", "Use Exhaust").SetValue(true));
            combo.SubMenu("Summoners Settings").AddItem(new MenuItem("cub", "Use Ignite").SetValue(true));
            combo.SubMenu("Summoners Settings").AddItem(new MenuItem("cuh", "Use Heal").SetValue(true));
            laneclear.AddItem(new MenuItem("lqu", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("lwu", "Use W with very low HP").SetValue(false));
            laneclear.AddItem(new MenuItem("leu", "Use E").SetValue(true));
            laneclear.AddItem(new MenuItem("lru", "Use R").SetValue(false));
            //laneclear.AddItem(new MenuItem("lquc", "Q Minion Number").SetValue(true));
            //laneclear.AddItem(new MenuItem("leuc", "E Minion Count").SetValue(true));
            harass.AddItem(new MenuItem("autoh", "AutoHarass Toggle").SetValue(new KeyBind('L', KeyBindType.Toggle)));
            harass.AddItem(new MenuItem("hqu", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("hqsu", "Use Q only on CC").SetValue(false));
            harass.AddItem(new MenuItem("hqfu", "Use Q on fear").SetValue(false));
            harass.AddItem(new MenuItem("hqfc", "Use Q on charm").SetValue(false));
            harass.AddItem(new MenuItem("heu", "Use E").SetValue(true));
            harass.AddItem(new MenuItem("hmana", "Min Mana").SetValue(new Slider(30, 100, 0)));
            jungleclear.AddItem(new MenuItem("jqu", "Use Q").SetValue(true));
            jungleclear.AddItem(new MenuItem("jeu", "Use E").SetValue(true));
            jungleclear.AddItem(new MenuItem("jmana", "Min mana").SetValue(new Slider(30, 100, 0)));
            drawing.AddItem(new MenuItem("qdr", "Q range").SetValue(new Circle()));
            drawing.AddItem(new MenuItem("wdr", "W range").SetValue(new Circle()));
            drawing.AddItem(new MenuItem("edr", "E range").SetValue(new Circle()));
            drawing.AddItem(new MenuItem("rdr", "R range").SetValue(new Circle()));

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnDelete += GameObject_OnDelete;
            GameObject.OnCreate += GameObject_OnCreate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }
        private static void OnDraw(EventArgs args)
        {
            var drawQ = menu_.Item("qdr").GetValue<Circle>();
            var drawW = menu_.Item("wdr").GetValue<Circle>();
            var drawE = menu_.Item("edr").GetValue<Circle>();
            var drawR = menu_.Item("rdr").GetValue<Circle>();
            if (drawQ.Active && q_.Level > 0)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 1175, System.Drawing.Color.Aqua);
            if (drawW.Active && w_.Level > 0)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 1075, System.Drawing.Color.Azure);
            if (drawE.Active && e_.Level > 0)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 1100, System.Drawing.Color.Blue);
            if (drawE.Active && r_.Level > 0)
                Render.Circle.DrawCircle(ObjectManager.Player.Position, 3340, System.Drawing.Color.BlueViolet);
        }
        private static void LaneClear()
        {
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, q_.Range);
            var allMinionsE = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, e_.Range);
            var allMinionsR = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, r_.Range);
            var minions = MinionManager.GetMinions(e_.Range, MinionTypes.All, MinionTeam.Enemy).Where(m => m.IsValid && m.Distance(ObjectManager.Player) < e_.Range).ToList();
            var rminions = MinionManager.GetMinions(r_.Range, MinionTypes.All, MinionTeam.Enemy).Where(m => m.IsValid && m.Distance(ObjectManager.Player) < r_.Range).ToList();
            var aaminions = MinionManager.GetMinions(e_.Range, MinionTypes.All, MinionTeam.Enemy).Where(m => m.IsValid && m.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)).ToList();
            var efarmpos = e_.GetCircularFarmLocation(new List<Obj_AI_Base>(minions), e_.Width);
            var qfarmpos = q_.GetLineFarmLocation(new List<Obj_AI_Base>(minions), q_.Width);
            var rfarmpos = r_.GetLineFarmLocation(new List<Obj_AI_Base>(rminions), r_.Width);
            for (int minq = 9; minq > 2; minq--)
                if (efarmpos.MinionsHit >= minq && e_.IsReady() && menu_.Item("leu").GetValue<bool>()) e_.Cast(efarmpos.Position);
            if (qfarmpos.MinionsHit == 2  && q_.IsReady() && menu_.Item("lqu").GetValue<bool>()) q_.Cast(qfarmpos.Position);
            if (rfarmpos.MinionsHit >= 9 && r_.IsReady() && menu_.Item("lru").GetValue<bool>()) r_.Cast(rfarmpos.Position);
            foreach (var minion in aaminions.Where(m => m.IsMinion && !m.IsDead && m.HasBuff("luxilluminatingfraulein")))
            {
                if (minion.IsValid)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AutoAttack, minion);
                }
            }
            if (ObjectManager.Player.Health <= 100 && w_.IsReady() && menu_.Item("lwu").GetValue<bool>()) w_.Cast(ObjectManager.Player.ServerPosition);
        }
        private static void LastHit()
        {
            var minione = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, e_.Range + e_.Width);
            foreach (var minion in minione.Where(a=>a.HasBuff("luxilluminatingfraulein")))
            {
                var passdmg = ObjectManager.Player.CalcDamage(minion, Damage.DamageType.Magical, 10 + (8 * ObjectManager.Player.Level) + 0.2 + ObjectManager.Player.FlatMagicDamageMod) + ObjectManager.Player.GetAutoAttackDamage(minion);
                if (minion.Health < passdmg)
                {
                    orbwalker_.ForceTarget(minion);
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AutoAttack, minion);
                }
            }
        }
        private static void Harass()
        {
            var ts = TargetSelector.GetTarget(q_.Range, TargetSelector.DamageType.Magical);
            var qpred = q_.GetPrediction(ts);
            var qcoll = q_.GetCollision(ObjectManager.Player.ServerPosition.To2D(), new List<Vector2> { qpred.CastPosition.To2D() });
            var mincoll = qcoll.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);
            var hamana = menu_.Item("hmana").GetValue<Slider>().Value;
            if (ts == null || ts.IsInvulnerable)
                return;
            if (e_.IsReady() && ts.IsValidTarget(r_.Range) && menu_.Item("heu").GetValue<bool>() && ObjectManager.Player.ManaPercent >= hamana)
                harassElogic();
            if (ts.IsValidTarget(q_.Range) && mincoll <= 1 && menu_.Item("hqu").GetValue<bool>() && qpred.Hitchance >= HitChance.VeryHigh && ObjectManager.Player.ManaPercent >= hamana
                && (ts.HasBuffOfType(BuffType.Slow) || ts.HasBuffOfType(BuffType.Stun) || ts.HasBuffOfType(BuffType.Snare) || ts.HasBuffOfType(BuffType.Knockup) || ts.HasBuffOfType(BuffType.Suppression) || ts.HasBuffOfType(BuffType.Fear) || ts.HasBuffOfType(BuffType.Charm)))
                q_.Cast(ts);
            if (menu_.Item("hqsu").GetValue<bool>())
                return;
            if (ts.IsValidTarget(q_.Range) && mincoll <= 1 && menu_.Item("hqu").GetValue<bool>() && qpred.Hitchance >= HitChance.VeryHigh && ObjectManager.Player.ManaPercent >= hamana)
                q_.Cast(ts);
        }
        private static void harassElogic()
        {
            var ts = TargetSelector.GetTarget(e_.Range + e_.Width, TargetSelector.DamageType.Magical);
            var epred = e_.GetPrediction(ts);
            if (lgo_ != null && e_.IsReady() && lgo_.Position.CountEnemiesInRange(e_.Width) < 1) Utility.DelayAction.Add(2000, () => e_.Cast());
            if (ts.IsInvulnerable || (ts.HasBuff("luxilluminatingfraulein") && ts.HasBuff("LuxLightBindingMis") && ObjectManager.Player.Distance(ts.Position) <= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)))
                return;
            if (lgo_ != null && (lgo_.Position.CountEnemiesInRange(300) >= 1 || ts.HasBuffOfType(BuffType.Slow)))
                e_.Cast(ts);
            if (lgo_ != null)
                return;
            if (epred.Hitchance >= HitChance.VeryHigh)
                e_.Cast(ts);
        }
        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Lux_Base_E_tar_nova.troy")
            {
                lgo_ = null;
            }
        }

        public static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Lux_Base_E_mis.troy")
            {
                lgo_ = sender;
            }
        }
        public static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if ((!w_.IsReady()) || (!args.Target.IsMe || sender.IsAlly || sender.IsMinion || sender == null))
                return;
            if (w_.IsReady() && menu_.Item("autow").GetValue<bool>())
                w_.Cast(Game.CursorPos);
        }

        private static void Combo()
        {
            var ts = TargetSelector.GetTarget(q_.Range, TargetSelector.DamageType.Magical);
            igniteslot_ = ObjectManager.Player.GetSpellSlot("summonerdot");
            if (ts == null || ts.IsInvulnerable)
                return;
            var qpred = q_.GetPrediction(ts);
            var qcoll = q_.GetCollision(ObjectManager.Player.ServerPosition.To2D(), new List<Vector2> { qpred.CastPosition.To2D() });
            var mincoll = qcoll.Where(x => !(x is Obj_AI_Hero)).Count(x => x.IsMinion);
            var passdmg = ObjectManager.Player.CalcDamage(ts, Damage.DamageType.Magical, 10 + (8 * ObjectManager.Player.Level) + 0.2 * ObjectManager.Player.FlatMagicDamageMod);
            var passiveaadmg = ObjectManager.Player.GetAutoAttackDamage(ObjectManager.Player) + passdmg;
            var lichdmg = ObjectManager.Player.CalcDamage(ts, Damage.DamageType.Magical, (ObjectManager.Player.BaseAttackDamage * 0.75) + ((ObjectManager.Player.BaseAbilityDamage + ObjectManager.Player.FlatMagicDamageMod) * 0.5));
            if (ts.IsValidTarget(q_.Range) && mincoll <= 1 && q_.IsReady() && qpred.Hitchance >= HitChance.VeryHigh && menu_.Item("cqu").GetValue<bool>())
                q_.Cast(ts);
            if (ts.IsValidTarget(e_.Range) && e_.IsReady() && menu_.Item("ceu").GetValue<bool>())
                harassElogic();
            if (ObjectManager.Player.Distance(ts.Position) <= 600 && CalcIgnite(ts) + e_.GetDamage(ts) >= ts.Health && ObjectManager.Player.HealthPercent <= 25 && menu_.Item("cui").GetValue<bool>() && ((e_.IsReady() && lgo_ == null) || (e_.IsReady() && lgo_ != null && ts.Distance(lgo_.Position) <= lgo_.BoundingRadius) || (q_.IsReady() && q_.GetPrediction(ts).Hitchance >= HitChance.High)))
                ObjectManager.Player.Spellbook.CastSpell(igniteslot_, ts);
            if (lgo_ != null && ts.Distance(lgo_.Position) <= e_.Width && ts.Health < e_.GetDamage(ts) || ts.HasBuff("LuxLightBindingMis") && ts.Health < passiveaadmg && ObjectManager.Player.Distance(ts.Position) <= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && ts.HasBuff("luxilluminatingfraulein"))
                return;
            if (ObjectManager.Player.HasBuff("lichbane") && ts.Health < lichdmg && ObjectManager.Player.Distance(ts.Position) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
                return;
            if (ObjectManager.Player.Distance(ts.Position) <= 600 && CalcIgnite(ts) >= ts.Health && menu_.Item("cui").GetValue<bool>())
                ObjectManager.Player.Spellbook.CastSpell(igniteslot_, ts);
        }
        private static float CalcIgnite(Obj_AI_Hero ts)
        {
            if (igniteslot_ == SpellSlot.Unknown || ObjectManager.Player.Spellbook.CanUseSpell(igniteslot_) != SpellState.Ready)
                return 0f;
            return (float)ObjectManager.Player.GetSummonerSpellDamage(ts, Damage.SummonerSpell.Ignite);
        }
        public static bool qcast { get; set; }
        private static void Rlogic()
        {
            {
                if (!q_.IsReady())
                    qcast = true;
                Utility.DelayAction.Add(100, () => qcast = false);
            }
            igniteslot_ = ObjectManager.Player.GetSpellSlot("summonerdot");
            var ts = TargetSelector.GetTarget(r_.Range, TargetSelector.DamageType.Magical);
            if (ts == null || ts.IsInvulnerable || ts.HasBuff("caitlynaceinthehole"))
                return;
            var passdmg = ObjectManager.Player.CalcDamage(ts, Damage.DamageType.Magical, 10 + (8 * ObjectManager.Player.Level) + 0.2 * ObjectManager.Player.FlatMagicDamageMod);
            var passaadmg = ObjectManager.Player.GetAutoAttackDamage(ObjectManager.Player) + passdmg;
            if (ts.Health <= passaadmg && ts.HasBuff("luxilluminatingfraulein") && ObjectManager.Player.Distance(ts.Position) <= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
                return;
            if (ts.Health <= q_.GetDamage(ts) && q_.GetPrediction(ts).Hitchance >= HitChance.VeryHigh && (q_.IsReady() || qcast))
                return;
            var rdmg = r_.GetDamage(ts);
            var rpred = r_.GetPrediction(ts);
            var rpdmg = rdmg + ObjectManager.Player.CalcDamage(ts, Damage.DamageType.Magical, 10 + (8 * ObjectManager.Player.Level) + 0.2 * ObjectManager.Player.FlatMagicDamageMod);
            var rpidmg = rpdmg + CalcIgnite(ts);
            var lichdmg = ObjectManager.Player.CalcDamage(ts, Damage.DamageType.Magical, (ObjectManager.Player.BaseAttackDamage * 0.75) + ((ObjectManager.Player.BaseAbilityDamage + ObjectManager.Player.FlatMagicDamageMod) * 0.5));
            var cdmg = rdmg + e_.GetDamage(ts);
            if (lgo_ != null && ts.IsValidTarget(r_.Range) && ts.Position.Distance(lgo_.Position) <= e_.Width && r_.IsReady() && ts.IsValidTarget(r_.Range) && rpred.Hitchance >= HitChance.VeryHigh && (ts.Health < cdmg + passdmg || ts.Health < cdmg + passdmg + CalcIgnite(ts)) && ts.HasBuff("LuxLightBindingMis")
                || lgo_ != null && ts.IsValidTarget(r_.Range) && ObjectManager.Player.HasBuff("lichbane") && ObjectManager.Player.Distance(ts.Position) <= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && ts.Position.Distance(lgo_.Position) <= e_.Width && r_.IsReady() /*h*/ && ts.IsValidTarget(r_.Range) && rpred.Hitchance >= HitChance.VeryHigh && ts.Health < cdmg + passdmg + lichdmg && ts.HasBuff("LuxLightBindingMis")
                || lgo_ != null && ts.IsValidTarget(r_.Range) && ObjectManager.Player.HasBuff("lichbane") && ObjectManager.Player.Distance(ts.Position) <= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && ts.Position.Distance(lgo_.Position) <= e_.Width && r_.IsReady() && igniteslot_.IsReady() && ts.IsValidTarget(r_.Range) && rpred.Hitchance >= HitChance.VeryHigh && ts.Health < cdmg + passdmg + lichdmg + CalcIgnite(ts) && ts.HasBuff("LuxLightBindingMis"))
                r_.Cast(ts);
            if (ts.HasBuff("LuxLightBindingMis") && menu_.Item("crq").GetValue<bool>() && rpred.Hitchance >= HitChance.VeryHigh)
                r_.Cast(ts);
            if (ObjectManager.Player.HasBuff("lichbane") && ObjectManager.Player.Distance(ts.Position) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && ((ts.Health < lichdmg) || (ts.HasBuff("luxilluminatingfraulein") && ts.Health < lichdmg + passdmg)))
                return;
            if (ts.IsValidTarget(q_.Range) && q_.GetPrediction(ts).Hitchance >= HitChance.VeryHigh && q_.IsReady() && e_.IsReady() && ts.Health < e_.GetDamage(ts) + q_.GetDamage(ts))
                return;
            if (ObjectManager.Player.Distance(ts.Position) < e_.Range - 200 && e_.GetDamage(ts) > ts.Health && e_.IsReady() || lgo_ != null && ts.Distance(lgo_.Position) <= e_.Width && ts.Health < e_.GetDamage(ts) || ObjectManager.Player.Distance(ts.Position) < e_.Range - 200 && q_.GetDamage(ts) > ts.Health && q_.IsReady() && q_.GetPrediction(ts).Hitchance >= HitChance.VeryHigh || ObjectManager.Player.Distance(ts.Position) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && ObjectManager.Player.GetAutoAttackDamage(ts) * 2 > ts.Health)
                return;
            if (lgo_ != null && ts.Distance(lgo_.Position) <= e_.Width && ts.Health < e_.GetDamage(ts) || ts.HasBuff("LuxLightBindingMis") && ts.Health < passaadmg && ObjectManager.Player.Distance(ts.Position) <= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) && ts.HasBuff("luxilluminatingfraulein"))
                return;
            if (ts.IsValidTarget(r_.Range) && r_.IsReady() && menu_.Item("craoeu").GetValue<bool>() && rpred.Hitchance >= HitChance.VeryHigh && ObjectManager.Player.Distance(ts.Position) <= e_.Range && ts.IsValidTarget(r_.Range) && !e_.IsReady())
                r_.CastIfWillHit(ts, menu_.Item("crehc").GetValue<Slider>().Value, menu_.Item("packetcast").GetValue<bool>());
            else if ((ts.IsValidTarget(r_.Range) && r_.IsReady() && menu_.Item("craoeu").GetValue<bool>() && rpred.Hitchance >= HitChance.VeryHigh && ObjectManager.Player.Distance(ts.Position) <= e_.Range && ts.IsValidTarget(r_.Range)))
                    r_.CastIfWillHit(ts, menu_.Item("crehc").GetValue<Slider>().Value, menu_.Item("packetcast").GetValue<bool>());
            if (ts.Health < rdmg - 100 + (3 * ObjectManager.Player.Level) && ts.Position.CountAlliesInRange(650) >= 1)
                return;
            if (ts.IsValidTarget(r_.Range) && rpred.Hitchance >= HitChance.VeryHigh && menu_.Item("cru").GetValue<bool>() && ts.HasBuff("luxilluminatingfraulein") && ts.Health < rpdmg && ((ObjectManager.Player.Distance(ts.Position) >= 100) || (ts.HasBuff("LuxLightBindingMis"))))
                r_.Cast(ts);
            if (ts.IsValidTarget(r_.Range) && menu_.Item("cru").GetValue<bool>() && rpred.Hitchance >= HitChance.VeryHigh && ts.Health < rdmg && ((ObjectManager.Player.Distance(ts.Position) >= 100) || (ts.HasBuff("LuxLightBindingMis"))))
                r_.Cast(ts);
            if ((ObjectManager.Player.Distance(ts.Position) < 600 && ((e_.GetDamage(ts) > ts.Health && e_.IsReady()) || (q_.GetDamage(ts) > ts.Health && q_.IsReady()) || (ts.Health < ObjectManager.Player.GetAutoAttackDamage(ts) * 2))) || lgo_ != null && ts.Distance(lgo_.Position) <= e_.Width && ts.Health < e_.GetDamage(ts))
                return;
            if ((ObjectManager.Player.Distance(ts.Position) < 600 && rpidmg >= ts.Health && menu_.Item("cui").GetValue<bool>() && r_.IsReady() && igniteslot_.IsReady()))
                ObjectManager.Player.Spellbook.CastSpell(igniteslot_, ts);
        }
        private static void JungleClear()
        {
            var jmana = menu_.Item("jmana").GetValue<Slider>().Value;
            var allminq = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, q_.Range + q_.Width, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var allmine = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, e_.Range + e_.Width, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            var qpos = q_.GetLineFarmLocation(allminq, q_.Width);
            var epos = e_.GetCircularFarmLocation(allmine, e_.Width);
            if (qpos.MinionsHit >= 1 && menu_.Item("jqu").GetValue<bool>() && ObjectManager.Player.ManaPercent >= jmana)
                q_.Cast(qpos.Position);
            if (epos.MinionsHit >= 1 && menu_.Item("jeu").GetValue<bool>() && ObjectManager.Player.ManaPercent >= jmana)
                e_.Cast(epos.Position);
            if (lgo_ != null)
                e_.Cast();
        }
        private static void SemiR()
        {
            var ts = TargetSelector.GetTarget(r_.Range, TargetSelector.DamageType.Magical);
            if (ts == null)
                return;
            if (r_.IsReady() && ts.IsValidTarget(r_.Range))
                r_.Cast(ts);
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (orbwalker_.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    Rlogic();
                    break;
                case Orbwalking.OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    JungleClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;
            }
            if (menu_.Item("autoh").GetValue<KeyBind>().Active)
                Harass();
            if (menu_.Item("semir").GetValue<KeyBind>().Active)
                SemiR();
        }
    }
}
