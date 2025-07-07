using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UmbralMithrix
{
    public static class ModConfig
    {
        internal static ConfigFile UMConfig;
        public static ConfigEntry<bool> purpleArena;
        public static ConfigEntry<bool> skipPhase4;
        public static ConfigEntry<bool> addShockwave;
        public static ConfigEntry<bool> purpleMithrix;
        public static ConfigEntry<float> basehealth;
        public static ConfigEntry<float> levelhealth;
        public static ConfigEntry<float> basedamage;
        public static ConfigEntry<float> leveldamage;
        public static ConfigEntry<float> basearmor;
        public static ConfigEntry<float> baseattackspeed;
        public static ConfigEntry<float> basespeed;
        public static ConfigEntry<float> mass;
        public static ConfigEntry<float> turningspeed;
        public static ConfigEntry<float> jumpingpower;
        public static ConfigEntry<float> acceleration;
        public static ConfigEntry<float> aircontrol;
        public static ConfigEntry<int> PrimStocks;
        public static ConfigEntry<int> SecStocks;
        public static ConfigEntry<int> UtilStocks;
        public static ConfigEntry<float> PrimCD;
        public static ConfigEntry<float> SecCD;
        public static ConfigEntry<float> UtilCD;
        public static ConfigEntry<float> SpecialCD;
        public static ConfigEntry<float> CrushingLeap;
        public static ConfigEntry<int> SlamOrbProjectileCount;
        public static ConfigEntry<int> LunarShardAdd;
        public static ConfigEntry<int> UltimateWaves;
        public static ConfigEntry<int> UltimateCount;
        public static ConfigEntry<float> UltimateDuration;
        public static ConfigEntry<int> JumpWaveCount;
        public static ConfigEntry<float> ShardHoming;
        public static ConfigEntry<float> ShardRange;
        public static ConfigEntry<float> ShardCone;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Init()
        {
            if (UmbralMithrix.RooInstalled)
                InitRoO();

            UMConfig = new ConfigFile(Paths.ConfigPath + "\\com.Nuxlar.UmbralMithrix.cfg", true);

            purpleMithrix = UMConfig.BindOption(
                "General",
                "Purple Mithrix",
                true,
                "Adds umbral effects to Mithrix (purple when spawning in).");
            purpleArena = UMConfig.BindOption(
                "General",
                "Purple Arena",
                false,
                "Adds swirling purple walls/ceiling to the arena.");
            skipPhase4 = UMConfig.BindOption(
                "General",
                "Skip Phase 4",
                false,
                "Skips Phase 4.");
            addShockwave = UMConfig.BindOption(
                "General",
                "Add shockwave to Hammer Swipe",
                false,
                "Adds a 1 shockwave when Mithrix swipes.");

            basehealth = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Base Health",
                1000f,
                25f,
                "Vanilla: 1000",
                500f, 2500f);
            levelhealth = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Level Health",
                325f,
                25f,
                "Health gained per level. Vanilla: 300",
                100f, 1000f);
            basedamage = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Base Damage",
                15f,
                1f,
                "Vanilla: 16",
                10f, 30f);
            leveldamage = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Level Damage",
                3f,
                0.25f,
                "Damage gained per level. Vanilla: 3.2",
                1f, 6.4f);
            basearmor = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Base Armor",
                30f,
                5f,
                "Vanilla: 20",
                5f, 50f);
            baseattackspeed = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Base Attack Speed",
                1.25f,
                0.25f,
                "Vanilla: 1",
                0.25f, 3f);
            basespeed = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Base Move Speed",
                15f,
                1f,
                "Vanilla: 15",
                10f, 30f);
            mass = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Mass",
                5000f,
                100f,
                "Recommended to increase if you increase his movement speed. Vanilla: 900",
                900f, 10000f);
            turningspeed = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Turn Speed",
                300f,
                10f,
                "Vanilla: 270",
                200f, 1000f);
            jumpingpower = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Jump Power",
                50f,
                5f,
                "How high Mithrix jumps. Vanilla: 25",
                25f, 100f);
            acceleration = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Acceleration",
                100f,
                5f,
                "Vanilla: 45",
                45f, 500f);
            aircontrol = UMConfig.BindOptionSteppedSlider(
                "Stats",
                "Air Control",
                1f,
                0.5f,
                "Vanilla: 0.25",
                0.25f, 3f);
            PrimStocks = UMConfig.BindOptionSlider(
                "Skills",
                "Primary Stocks",
                1,
                "Max Stocks for Mithrix's Weapon Slam. Vanilla: 1",
                1, 5);
            SecStocks = UMConfig.BindOptionSlider(
                "Skills",
                "Secondary Stocks",
                1,
                "Max Stocks for Mithrix's Dash Attack. Vanilla: 1",
                1, 5);
            UtilStocks = UMConfig.BindOptionSlider(
                "Skills",
                "Util Stocks",
                3,
                "Max Stocks for Mithrix's Dash. Vanilla: 2",
                1, 5);
            PrimCD = UMConfig.BindOptionSteppedSlider(
                "Skills",
                "Primary Cooldown",
                4f,
                0.25f,
                "Cooldown for Mithrix's Weapon Slam. Vanilla: 4",
                1f, 5f);
            SecCD = UMConfig.BindOptionSteppedSlider(
                "Skills",
                "Secondary Cooldown",
                4.5f,
                0.25f,
                "Cooldown for Mithrix's Dash Attack. Vanilla: 5",
                1f, 5f);
            UtilCD = UMConfig.BindOptionSteppedSlider(
                "Skills",
                "Util Cooldown",
                2.5f,
                0.25f,
                "Cooldown for Mithrix's Dash. Vanilla: 3",
                1f, 5f);
            SpecialCD = UMConfig.BindOptionSteppedSlider(
                "Skills",
                "Special Cooldown",
                30f,
                1f,
                "Cooldown for Mithrix's Jump Attack. Vanilla: 30",
                10f, 50f);

            CrushingLeap = UMConfig.BindOptionSteppedSlider(
                "Skill Mods",
                "Crushing Leap",
                3f,
                0.1f,
                "How long Mithrix stays in the air during the crushing leap. Vanilla: 3",
                0.1f, 6f);
            SlamOrbProjectileCount = UMConfig.BindOptionSlider(
                "Skill Mods",
                "Orb Projectile Count",
                3,
                "Orbs fired by weapon slam in a circle. Vanilla: N/A",
                0, 16);
            LunarShardAdd = UMConfig.BindOptionSlider(
                "Skill Mods",
                "Shard Add Count",
                1,
                "Bonus shards added to each shot of lunar shards. Vanilla: N/A",
                1, 5);
            UltimateWaves = UMConfig.BindOptionSlider(
                "Skill Mods",
                "P3 Ult Lines",
                8,
                "Total lines in ultimate per burst. Vanilla: 4",
                4, 18);
            UltimateCount = UMConfig.BindOptionSlider(
                "Skill Mods",
                "P3 Ult Bursts",
                6,
                "Total times the ultimate fires. Vanilla: 4",
                4, 8);
            UltimateDuration = UMConfig.BindOptionSteppedSlider(
                "Skill Mods",
                "P3 Ult Duration",
                8f,
                0.25f,
                "How long ultimate lasts. Vanilla: 8",
                5f, 10f);
            JumpWaveCount = UMConfig.BindOptionSlider(
                "Skill Mods",
                "Jump Wave Count",
                16,
                "Shockwave count when Mithrix lands after a jump. Vanilla: 12",
                12, 24);
            ShardHoming = UMConfig.BindOptionSteppedSlider(
                "Skill Mods",
                "Shard Homing",
                25f,
                5f,
                "How strongly lunar shards home in to targets. Vanilla: 20",
                10f, 60f);
            ShardRange = UMConfig.BindOptionSteppedSlider(
                "Skill Mods",
                "Shard Range",
                100f,
                10f,
                "Range (distance) in which shards look for targets. Vanilla: 80",
                80f, 160f);
            ShardCone = UMConfig.BindOptionSteppedSlider(
                "Skill Mods",
                "Shard Cone",
                120f,
                10f,
                "Cone (Angle) in which shards look for targets. Vanilla: 90",
                90f, 180f);

            WipeConfig();
        }

        private static void WipeConfig()
        {
            PropertyInfo orphanedEntriesProp = typeof(ConfigFile).GetProperty("OrphanedEntries", BindingFlags.Instance | BindingFlags.NonPublic);
            Dictionary<ConfigDefinition, string> orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(UMConfig);
            orphanedEntries.Clear();

            UMConfig.Save();
        }

        #region Config Binding
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void InitRoO()
        {
            try
            {
                RiskOfOptions.ModSettingsManager.SetModDescription("Umbral Mithrix", UmbralMithrix.PluginGUID, UmbralMithrix.PluginName);
                string pathString = Path.GetDirectoryName(UmbralMithrix.Instance.Info.Location);
                string iconPath = pathString.Substring(0, pathString.Length - 21);
                var iconStream = File.ReadAllBytes(Path.Combine(iconPath, "icon.png"));
                var tex = new Texture2D(256, 256);
                tex.LoadImage(iconStream);
                var icon = Sprite.Create(tex, new Rect(0, 0, 256, 256), new Vector2(0.5f, 0.5f));

                RiskOfOptions.ModSettingsManager.SetModIcon(icon, UmbralMithrix.PluginGUID, UmbralMithrix.PluginName);
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static ConfigEntry<T> BindOption<T>(this ConfigFile myConfig, string section, string name, T defaultValue, string description = "", bool restartRequired = true)
        {
            if (defaultValue is int or float)
            {
                return myConfig.BindOptionSlider(section, name, defaultValue, description, 0, 20, restartRequired);
            }
            if (string.IsNullOrEmpty(description))
                description = name;

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, new ConfigDescription(description, null));

            if (UmbralMithrix.RooInstalled)
                TryRegisterOption(configEntry, restartRequired);

            return configEntry;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static ConfigEntry<T> BindOptionSlider<T>(this ConfigFile myConfig, string section, string name, T defaultValue, string description = "", float min = 0, float max = 20, bool restartRequired = true)
        {
            if (defaultValue is not int and not float)
            {
                return myConfig.BindOption(section, name, defaultValue, description, restartRequired);
            }

            if (string.IsNullOrEmpty(description))
                description = name;

            description += " (Default: " + defaultValue + ")";

            if (restartRequired)
                description += " (restart required)";

            AcceptableValueBase range = typeof(T) == typeof(int)
                ? new AcceptableValueRange<int>((int)min, (int)max)
                : new AcceptableValueRange<float>(min, max);

            var configEntry = myConfig.Bind(section, name, defaultValue, new ConfigDescription(description, range));

            if (UmbralMithrix.RooInstalled)
                TryRegisterOptionSlider(configEntry, min, max, restartRequired);

            return configEntry;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static ConfigEntry<T> BindOptionSteppedSlider<T>(this ConfigFile myConfig, string section, string name, T defaultValue, float increment = 1f, string description = "", float min = 0, float max = 20, bool restartRequired = true)
        {
            if (string.IsNullOrEmpty(description))
                description = name;

            description += " (Default: " + defaultValue + ")";

            if (restartRequired)
                description += " (restart required)";

            var configEntry = myConfig.Bind(section, name, defaultValue, new ConfigDescription(description, new AcceptableValueRange<float>(min, max)));

            if (UmbralMithrix.RooInstalled)
                TryRegisterOptionSteppedSlider(configEntry, increment, min, max, restartRequired);

            return configEntry;
        }
        #endregion

        #region RoO
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TryRegisterOption<T>(ConfigEntry<T> entry, bool restartRequired)
        {
            if (entry is ConfigEntry<string> stringEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(stringEntry, new RiskOfOptions.OptionConfigs.InputFieldConfig()
                {
                    submitOn = RiskOfOptions.OptionConfigs.InputFieldConfig.SubmitEnum.OnExitOrSubmit,
                    restartRequired = restartRequired
                }), UmbralMithrix.PluginGUID, UmbralMithrix.PluginName);
            }
            else if (entry is ConfigEntry<bool> boolEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(boolEntry, restartRequired), UmbralMithrix.PluginGUID, UmbralMithrix.PluginName);
            }
            else if (entry is ConfigEntry<KeyboardShortcut> shortCutEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(shortCutEntry, restartRequired), UmbralMithrix.PluginGUID, UmbralMithrix.PluginName);
            }
            else if (typeof(T).IsEnum)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.ChoiceOption(entry, restartRequired), UmbralMithrix.PluginGUID, UmbralMithrix.PluginName);
            }
            else
            {
#if DEBUG
                Log.Warning($"Config entry {entry.Definition.Key} in section {entry.Definition.Section} with type {typeof(T).Name} " +
                    $"could not be registered in Risk Of Options using {nameof(TryRegisterOption)}.");
#endif
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TryRegisterOptionSlider<T>(ConfigEntry<T> entry, float min, float max, bool restartRequired)
        {
            if (entry is ConfigEntry<int> intEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.IntSliderOption(intEntry, new RiskOfOptions.OptionConfigs.IntSliderConfig()
                {
                    min = (int)min,
                    max = (int)max,
                    formatString = "{0:0.00}",
                    restartRequired = restartRequired
                }), UmbralMithrix.PluginGUID, UmbralMithrix.PluginName);
            }
            else if (entry is ConfigEntry<float> floatEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(floatEntry, new RiskOfOptions.OptionConfigs.SliderConfig()
                {
                    min = min,
                    max = max,
                    FormatString = "{0:0.00}",
                    restartRequired = restartRequired
                }), UmbralMithrix.PluginGUID, UmbralMithrix.PluginName);
            }
            else
            {
#if DEBUG
                Log.Warning($"Config entry {entry.Definition.Key} in section {entry.Definition.Section} with type {typeof(T).Name} " +
                    $"could not be registered in Risk Of Options using {nameof(TryRegisterOptionSlider)}.");
#endif
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void TryRegisterOptionSteppedSlider<T>(ConfigEntry<T> entry, float increment, float min, float max, bool restartRequired)
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StepSliderOption(floatEntry, new RiskOfOptions.OptionConfigs.StepSliderConfig()
                {
                    increment = increment,
                    min = min,
                    max = max,
                    FormatString = "{0:0.00}",
                    restartRequired = restartRequired
                }), UmbralMithrix.PluginGUID, UmbralMithrix.PluginName);
            }
            else
            {
#if DEBUG
                Log.Warning($"Config entry {entry.Definition.Key} in section {entry.Definition.Section} with type {typeof(T).Name} " +
                    $"could not be registered in Risk Of Options using {nameof(TryRegisterOptionSteppedSlider)}.");
#endif
            }
        }
        #endregion
    }
}
