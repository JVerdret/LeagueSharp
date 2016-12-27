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
        static void Main(string[] args)
        {
            if (ObjectManager.Player.BaseSkinName == "Lux")
            {
                Game.PrintChat("EveleyLux Loadedd");
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
            var drawing = menu_.AddSubMenu(new Menu("Drawing", "Drawing"));
            laneclear.AddItem(new MenuItem("lqu", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("lwu", "Use W with very low HP").SetValue(true));
            laneclear.AddItem(new MenuItem("leu", "Use E").SetValue(true));
            //laneclear.AddItem(new MenuItem("lquc", "Q Minion Number").SetValue(true));
            //laneclear.AddItem(new MenuItem("leuc", "E Minion Count").SetValue(true));
            drawing.AddItem(new MenuItem("qdr", "Q range").SetValue(new Circle()));
            drawing.AddItem(new MenuItem("wdr", "W range").SetValue(new Circle()));
            drawing.AddItem(new MenuItem("edr", "E range").SetValue(new Circle()));
            drawing.AddItem(new MenuItem("rdr", "R range").SetValue(new Circle()));

            Game.OnUpdate += Game_OnGameUpdate;
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
            var minions = MinionManager.GetMinions(e_.Range, MinionTypes.All, MinionTeam.Enemy).Where(m => m.IsValid && m.Distance(ObjectManager.Player) < e_.Range).ToList();
            var aaminions = MinionManager.GetMinions(e_.Range, MinionTypes.All, MinionTeam.Enemy).Where(m => m.IsValid && m.Distance(ObjectManager.Player) < Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)).ToList();
            var efarmpos = e_.GetCircularFarmLocation(new List<Obj_AI_Base>(minions), e_.Width);
            var qfarmpos = q_.GetLineFarmLocation(new List<Obj_AI_Base>(minions), q_.Width);
            if (efarmpos.MinionsHit >= 3 && e_.IsReady() && menu_.Item("leu").GetValue<bool>()) e_.Cast(efarmpos.Position);
            if (qfarmpos.MinionsHit == 2  && q_.IsReady() && menu_.Item("lqu").GetValue<bool>()) q_.Cast(qfarmpos.Position);
            foreach (var minion in aaminions.Where(m => m.IsMinion && !m.IsDead && m.HasBuff("luxilluminatingfraulein")))
            {
                if (minion.IsValid)
                {
                    ObjectManager.Player.IssueOrder(GameObjectOrder.AutoAttack, minion);
                }
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            switch (orbwalker_.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
            }
        }
    }
}
