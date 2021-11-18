using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ExileCore.Shared.Static;
using SharpDX;

namespace KillCounter
{
    public class KillCounter : BaseSettingsPlugin<KillCounterSettings>
    {
        private bool _canRender;
        private Dictionary<uint, HashSet<long>> countedIds;
        private Dictionary<MonsterRarity, int> counters;
        private int sessionCounter;
        private int summaryCounter;
        private int scourgeRareCounter;


        private List<string> Ignored = new List<string>
        {
            "Metadata/Monsters/LeagueAffliction/Volatile/AfflictionVolatile",
            "Metadata/Monsters/VolatileCore/VolatileCore",
            
            // Delirium Ignores
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonEyes1",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonEyes2",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonEyes3",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonSpikes",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonSpikes2",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonSpikes3",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonPimple1",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonPimple2",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonPimple3",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonGoatFillet1Vanish",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonGoatFillet2Vanish",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonGoatRhoa1Vanish",
            "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemonGoatRhoa2Vanish",
            
            // Conquerors Ignores
            "Metadata/Monsters/AtlasExiles/AtlasExile1@",
            "Metadata/Monsters/AtlasExiles/CrusaderInfluenceMonsters/CrusaderArcaneRune",
            "Metadata/Monsters/AtlasExiles/AtlasExile2_",
            "Metadata/Monsters/AtlasExiles/EyrieInfluenceMonsters/EyrieFrostnadoDaemon",
            "Metadata/Monsters/AtlasExiles/AtlasExile3@",
            "Metadata/Monsters/AtlasExiles/AtlasExile3AcidPitDaemon",
            "Metadata/Monsters/AtlasExiles/AtlasExile3BurrowingViperMelee",
            "Metadata/Monsters/AtlasExiles/AtlasExile3BurrowingViperRanged",
            "Metadata/Monsters/AtlasExiles/AtlasExile4@",
            "Metadata/Monsters/AtlasExiles/AtlasExile4ApparitionCascade",
            "Metadata/Monsters/AtlasExiles/AtlasExile5Apparition",
            "Metadata/Monsters/AtlasExiles/AtlasExile5Throne",

            // Incursion Ignores
            "Metadata/Monsters/LeagueIncursion/VaalSaucerRoomTurret",
            "Metadata/Monsters/LeagueIncursion/VaalSaucerTurret",
            "Metadata/Monsters/LeagueIncursion/VaalSaucerTurret",
            
            // Betrayal Ignores
            "Metadata/Monsters/LeagueBetrayal/BetrayalTaserNet",
            "Metadata/Monsters/LeagueBetrayal/FortTurret/FortTurret1Safehouse",
            "Metadata/Monsters/LeagueBetrayal/FortTurret/FortTurret1",
            "Metadata/Monsters/LeagueBetrayal/MasterNinjaCop",

            // Legion Ignores
            "Metadata/Monsters/LegionLeague/LegionVaalGeneralProjectileDaemon",
            "Metadata/Monsters/LegionLeague/LegionSergeantStampedeDaemon",
            "Metadata/Monsters/LegionLeague/LegionSandTornadoDaemon",

            // Random Ignores
            "Metadata/Monsters/InvisibleFire/InvisibleSandstorm_",
            "Metadata/Monsters/InvisibleFire/InvisibleFrostnado",
            "Metadata/Monsters/InvisibleFire/InvisibleFireAfflictionDemonColdDegen",
            "Metadata/Monsters/InvisibleFire/InvisibleFireAfflictionDemonColdDegenUnique",
            "Metadata/Monsters/InvisibleFire/InvisibleFireAfflictionCorpseDegen",
            "Metadata/Monsters/InvisibleFire/InvisibleFireEyrieHurricane",
            "Metadata/Monsters/InvisibleFire/InvisibleIonCannonFrost",
            "Metadata/Monsters/InvisibleFire/AfflictionBossFinalDeathZone",

            "Metadata/Monsters/InvisibleCurse/InvisibleFrostbiteStationary",
            "Metadata/Monsters/InvisibleCurse/InvisibleConductivityStationary",
            "Metadata/Monsters/InvisibleCurse/InvisibleEnfeeble",

            "Metadata/Monsters/InvisibleAura/InvisibleWrathStationary",

            "Metadata/Monsters/LeagueSynthesis/SynthesisDroneBossTurret1",
            "Metadata/Monsters/LeagueSynthesis/SynthesisDroneBossTurret2",
            "Metadata/Monsters/LeagueSynthesis/SynthesisDroneBossTurret3",
            "Metadata/Monsters/LeagueSynthesis/SynthesisDroneBossTurret4",

            "Metadata/Monsters/LeagueBestiary/RootSpiderBestiaryAmbush",
            "Metadata/Monsters/LeagueBestiary/BlackScorpionBestiaryBurrowTornado",

            // "Metadata/Monsters/Labyrinth/GoddessOfJustice",
            // "Metadata/Monsters/Labyrinth/GoddessOfJusticeMapBoss",
            "Metadata/Monsters/Frog/FrogGod/SilverOrb",
            "Metadata/Monsters/Frog/FrogGod/SilverPool",
            "Metadata/Monsters/LunarisSolaris/SolarisCelestialFormAmbushUniqueMap",
            "Metadata/Monsters/Invisible/MaligaroSoulInvisibleBladeVortex",
            "Metadata/Monsters/Daemon",
            "Metadata/Monsters/Daemon/MaligaroBladeVortexDaemon",
            "Metadata/Monsters/Daemon/SilverPoolChillDaemon",
            "Metadata/Monsters/AvariusCasticus/AvariusCasticusStatue",
        };

        public override bool Initialise()
        {
            GameController.LeftPanel.WantUse(() => Settings.Enable);
            countedIds = new Dictionary<uint, HashSet<long>>();
            counters = new Dictionary<MonsterRarity, int>();
            Init();
            return true;
        }

        public override void OnLoad()
        {
            CanUseMultiThreading = true;
            Order = -10;
            Graphics.InitImage("preload-new.png");
        }

        private void Init()
        {
            foreach (MonsterRarity rarity in Enum.GetValues(typeof(MonsterRarity)))
            {
                counters[rarity] = 0;
            }
            scourgeRareCounter = 0;
        }

        public override void AreaChange(AreaInstance area)
        {
            if (!Settings.Enable.Value) return;
            countedIds.Clear();
            counters.Clear();
            sessionCounter += summaryCounter;
            summaryCounter = 0;
            scourgeRareCounter = 0;
            Init();
        }

        public override Job Tick()
        {
            if (Settings.MultiThreading)
                return GameController.MultiThreadManager.AddJob(TickLogic, nameof(KillCounter));

            TickLogic();
            return null;
        }

        private void TickLogic()
        {
            foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster])
            {
                if (entity.IsAlive) continue;
                if (Ignored.Any(x => entity.Path.StartsWith(x))) continue;
                Calc(entity);
            }
        }

        public override void Render()
        {
            var UIHover = GameController.Game.IngameState.UIHover;
            var miniMap = GameController.Game.IngameState.IngameUi.Map.SmallMiniMap;

            if (Settings.Enable.Value && UIHover.Address != 0x00 && UIHover.Tooltip.Address != 0x00 && UIHover.Tooltip.IsVisibleLocal &&
                UIHover.Tooltip.GetClientRect().Intersects(miniMap.GetClientRect()))
                _canRender = false;

            if (UIHover.Address == 0x00 || UIHover.Tooltip.Address == 0x00 || !UIHover.Tooltip.IsVisibleLocal)
                _canRender = true;

            if (!Settings.Enable || Input.GetKeyState(Keys.F10) || GameController.Area.CurrentArea == null ||
                !Settings.ShowInTown && GameController.Area.CurrentArea.IsTown ||
                !Settings.ShowInTown && GameController.Area.CurrentArea.IsHideout) return;

            if (!_canRender) return;

            var position = GameController.LeftPanel.StartDrawPoint;
            var size = Vector2.Zero;

            if (Settings.ShowDetail) size = DrawCounters(position);
            var session = $"({sessionCounter + summaryCounter})";
            position.Y += size.Y;

            var size2 = Graphics.DrawText($"kills: {summaryCounter} {session}", position.Translate(0, 5), Settings.TextColor,
                FontAlign.Right);

            var width = Math.Max(size.X, size2.X);
            var bounds = new RectangleF(position.X - width - 50, position.Y - size.Y, width + 50, size.Y + size2.Y + 10);
            Graphics.DrawImage("preload-new.png", bounds, Settings.BackgroundColor);
            GameController.LeftPanel.StartDrawPoint = position;
        }

        //TODO Rewrite with use ImGuiRender.DrawMultiColoredText()
        private Vector2 DrawCounters(Vector2 position)
        {
            const int INNER_MARGIN = 15;
            position.Y += 5;
            var drawText = Graphics.DrawText(scourgeRareCounter.ToString(), position, Settings.ScourgeRareCounterColor, FontAlign.Right);
            position.X -= INNER_MARGIN + drawText.X;
            drawText = Graphics.DrawText(counters[MonsterRarity.White].ToString(), position, Color.White, FontAlign.Right);
            position.X -= INNER_MARGIN + drawText.X;
            drawText = Graphics.DrawText(counters[MonsterRarity.Magic].ToString(), position, HudSkin.MagicColor, FontAlign.Right);
            position.X -= INNER_MARGIN + drawText.X;
            drawText = Graphics.DrawText(counters[MonsterRarity.Rare].ToString(), position, HudSkin.RareColor, FontAlign.Right);
            position.X -= INNER_MARGIN + drawText.X;
            drawText = Graphics.DrawText(counters[MonsterRarity.Unique].ToString(), position, HudSkin.UniqueColor, FontAlign.Right);

            return drawText.TranslateToNum();
        }

        public override void EntityAdded(Entity Entity)
        {
        }

        private void Calc(Entity Entity)
        {
            var areaHash = GameController.Area.CurrentArea.Hash;

            if (!countedIds.TryGetValue(areaHash, out var monstersHashSet))
            {
                monstersHashSet = new HashSet<long>();
                countedIds[areaHash] = monstersHashSet;
            }

            if (!Entity.HasComponent<ObjectMagicProperties>()) return;
            var hashMonster = Entity.Id;

            if (!monstersHashSet.Contains(hashMonster))
            {
                monstersHashSet.Add(hashMonster);
                var rarity = Entity.Rarity;

                if (Entity.IsHostile && rarity >= MonsterRarity.White && rarity <= MonsterRarity.Unique)
                {
                    if (Entity.Path.StartsWith("Metadata/Monsters/LeagueHellscape") && rarity == MonsterRarity.Rare)
                        scourgeRareCounter++;
                    counters[rarity]++;
                    summaryCounter++;
                }
                
            }
        }
    }

    public class KillCounterSettings : ISettings
    {
        public KillCounterSettings()
        {
            ShowDetail = new ToggleNode(true);
            ShowInTown = new ToggleNode(false);
            TextColor = new ColorBGRA(220, 190, 130, 255);
            BackgroundColor = new ColorBGRA(0, 0, 0, 255);
            ScourgeRareCounterColor = new ColorBGRA(0, 0, 0, 255);
            LabelTextSize = new RangeNode<int>(16, 10, 20);
            KillsTextSize = new RangeNode<int>(16, 10, 20);
        }

        public ToggleNode ShowInTown { get; set; }
        public ToggleNode ShowDetail { get; set; }
        public ColorNode TextColor { get; set; }
        public ColorNode BackgroundColor { get; set; }
        public ColorNode ScourgeRareCounterColor { get; set; }
        public RangeNode<int> LabelTextSize { get; set; }
        public RangeNode<int> KillsTextSize { get; set; }
        public ToggleNode UseImguiForDraw { get; set; } = new ToggleNode(true);
        public ToggleNode MultiThreading { get; set; } = new ToggleNode(false);
        public ToggleNode Enable { get; set; } = new ToggleNode(false);
    }
}
