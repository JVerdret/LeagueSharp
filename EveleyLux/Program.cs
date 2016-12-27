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
            var laneclear = menu_.AddSubMenu(new Menu("Laneclear", "Laneclear"));
            var harass = menu_.AddSubMenu(new Menu("Harass", "Harass"));
            var drawing = menu_.AddSubMenu(new Menu("Drawing", "Drawing"));
            laneclear.AddItem(new MenuItem("lqu", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("lwu", "Use W with very low HP").SetValue(false));
            laneclear.AddItem(new MenuItem("leu", "Use E").SetValue(true));
            laneclear.AddItem(new MenuItem("lru", "Use R").SetValue(false));
            //laneclear.AddItem(new MenuItem("lquc", "Q Minion Number").SetValue(true));
            //laneclear.AddItem(new MenuItem("leuc", "E Minion Count").SetValue(true));
            harass.AddItem(new MenuItem("autoh", "AutoHarass Toggle").SetValue(new KeyBind('L', KeyBindType.Toggle)));
            harass.AddItem(new MenuItem("hqu", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("hqsu", "Use Q only on CC").SetValue(false));
            harass.AddItem(new MenuItem("heu", "Use E").SetValue(true));
            drawing.AddItem(new MenuItem("qdr", "Q range").SetValue(new Circle()));
            drawing.AddItem(new MenuItem("wdr", "W range").SetValue(new Circle()));
            drawing.AddItem(new MenuItem("edr", "E range").SetValue(new Circle()));
            drawing.AddItem(new MenuItem("rdr", "R range").SetValue(new Circle()));

            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnDelete += GameObject_OnDelete;
            GameObject.OnCreate += GameObject_OnCreate;
            Drawing.OnDraw += OnDraw;
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
            if (efarmpos.MinionsHit >= 3 && e_.IsReady() && menu_.Item("leu").GetValue<bool>()) e_.Cast(efarmpos.Position);
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
            if (ts == null || ts.IsInvulnerable)
                return;
            //if (e_.IsReady() && ts.IsValidTarget(r_.Range) && menu_.Item("heu").GetValue<bool>()) ELogic(); //In create

        }
        private static void Elogic()
        {
            var ts = TargetSelector.GetTarget(e_.Range + e_.Width, TargetSelector.DamageType.Magical);
            var epred = e_.GetPrediction(ts);

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
        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (orbwalker_.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                case Orbwalking.OrbwalkingMode.LastHit:
                    LastHit();
                    break;
            }
        }
    }
}
