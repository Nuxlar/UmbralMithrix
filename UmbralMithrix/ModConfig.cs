using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;

namespace UmbralMithrix
{
  internal class ModConfig
  {
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

    public static void InitConfig(ConfigFile config)
    {
      ModConfig.basehealth = config.Bind<float>("Stats", "Base Health", 1000f, "Vanilla: 1000");
      ModConfig.levelhealth = config.Bind<float>("Stats", "Level Health", 325f, "Health gained per level. Vanilla: 300");
      ModConfig.basedamage = config.Bind<float>("Stats", "Base Damage", 15f, "Vanilla: 16");
      ModConfig.leveldamage = config.Bind<float>("Stats", "Level Damage", 3f, "Damage gained per level. Vanilla: 3.2");
      ModConfig.basearmor = config.Bind<float>("Stats", "Base Armor", 30f, "Vanilla: 20");
      ModConfig.baseattackspeed = config.Bind<float>("Stats", "Base Attack Speed", 1.25f, "Vanilla: 1");
      ModConfig.basespeed = config.Bind<float>("Stats", "Base Move Speed", 15f, "Vanilla: 15");
      ModConfig.mass = config.Bind<float>("Stats", "Mass", 5000f, "Recommended to increase if you increase his movement speed. Vanilla: 900");
      ModConfig.turningspeed = config.Bind<float>("Stats", "Turn Speed", 300f, "Vanilla: 270");
      ModConfig.jumpingpower = config.Bind<float>("Stats", "Moon Shoes", 50f, "How high Mithrix jumps. Vanilla: 25");
      ModConfig.acceleration = config.Bind<float>("Stats", "Acceleration", 100f, "Vanilla: 45");
      ModConfig.aircontrol = config.Bind<float>("Stats", "Air Control", 0.5f, "Vanilla: 0.25");
      ModConfig.PrimStocks = config.Bind<int>("Skills", "Primary Stocks", 1, "Max Stocks for Mithrix's Weapon Slam. Vanilla: 1");
      ModConfig.SecStocks = config.Bind<int>("Skills", "Secondary Stocks", 1, "Max Stocks for Mithrix's Dash Attack. Vanilla: 1");
      ModConfig.UtilStocks = config.Bind<int>("Skills", "Util Stocks", 3, "Max Stocks for Mithrix's Dash. Vanilla: 2");
      ModConfig.PrimCD = config.Bind<float>("Skills", "Primary Cooldown", 4f, "Cooldown for Mithrix's Weapon Slam. Vanilla: 4");
      ModConfig.SecCD = config.Bind<float>("Skills", "Secondary Cooldown", 4.5f, "Cooldown for Mithrix's Dash Attack. Vanilla: 5");
      ModConfig.UtilCD = config.Bind<float>("Skills", "Util Cooldown", 2.5f, "Cooldown for Mithrix's Dash. Vanilla: 3");
      ModConfig.SpecialCD = config.Bind<float>("Skills", "Special Cooldown", 30f, "Cooldown for Mithrix's Jump Attack. Vanilla: 30");
      ModConfig.CrushingLeap = config.Bind<float>("Skill Mods", "Crushing Leap", 3f, "How long Mithrix stays in the air during the crushing leap. Vanilla: 3");
      ModConfig.SlamOrbProjectileCount = config.Bind<int>("Skill Mods", "Orb Projectile Count", 3, "Orbs fired by weapon slam in a circle. Vanilla: N/A");
      ModConfig.LunarShardAdd = config.Bind<int>("Skill Mods", "Shard Add Count", 1, "Bonus shards added to each shot of lunar shards. Vanilla: N/A");
      ModConfig.UltimateWaves = config.Bind<int>("Skill Mods", "P3 Ult Lines", 8, "Total lines in ultimate per burst. Vanilla: 4");
      ModConfig.UltimateCount = config.Bind<int>("Skill Mods", "P3 Ult Bursts", 6, "Total times the ultimate fires. Vanilla: 4");
      ModConfig.UltimateDuration = config.Bind<float>("Skill Mods", "P3 Ult Duration", 8f, "How long ultimate lasts. Vanilla: 8");
      ModConfig.JumpWaveCount = config.Bind<int>("Skill Mods", "Jump Wave Count", 16, "Shockwave count when Mithrix lands after a jump. Vanilla: 12");
      ModConfig.ShardHoming = config.Bind<float>("Skill Mods", "Shard Homing", 25f, "How strongly lunar shards home in to targets. Vanilla: 20");
      ModConfig.ShardRange = config.Bind<float>("Skill Mods", "Shard Range", 100f, "Range (distance) in which shards look for targets. Vanilla: 80");
      ModConfig.ShardCone = config.Bind<float>("Skill Mods", "Shard Cone", 120f, "Cone (Angle) in which shards look for targets. Vanilla: 90");
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.basehealth, new StepSliderConfig()
      {
        min = 500f,
        max = 2500f,
        increment = 50f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.levelhealth, new StepSliderConfig()
      {
        min = 100f,
        max = 500f,
        increment = 25f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.basedamage, new StepSliderConfig()
      {
        min = 10f,
        max = 30f,
        increment = 1f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.leveldamage, new StepSliderConfig()
      {
        min = 1f,
        max = 6.4f,
        increment = 0.25f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.basearmor, new StepSliderConfig()
      {
        min = 5f,
        max = 50f,
        increment = 5f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.baseattackspeed, new StepSliderConfig()
      {
        min = 0.25f,
        max = 3f,
        increment = 0.25f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.basespeed, new StepSliderConfig()
      {
        min = 10f,
        max = 30f,
        increment = 1f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.mass, new StepSliderConfig()
      {
        min = 900f,
        max = 10000f,
        increment = 100f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.turningspeed, new StepSliderConfig()
      {
        min = 200f,
        max = 1000f,
        increment = 10f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.jumpingpower, new StepSliderConfig()
      {
        min = 25f,
        max = 100f,
        increment = 5f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.acceleration, new StepSliderConfig()
      {
        min = 45f,
        max = 500f,
        increment = 5f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.aircontrol, new StepSliderConfig()
      {
        min = 0.25f,
        max = 3f,
        increment = 0.25f
      }));
      ModSettingsManager.AddOption(new IntSliderOption(ModConfig.PrimStocks, new IntSliderConfig()
      {
        min = 1,
        max = 5
      }));
      ModSettingsManager.AddOption(new IntSliderOption(ModConfig.SecStocks, new IntSliderConfig()
      {
        min = 1,
        max = 5
      }));
      ModSettingsManager.AddOption(new IntSliderOption(ModConfig.UtilStocks, new IntSliderConfig()
      {
        min = 1,
        max = 5
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.PrimCD, new StepSliderConfig()
      {
        min = 1f,
        max = 5f,
        increment = 0.25f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.SecCD, new StepSliderConfig()
      {
        min = 1f,
        max = 5f,
        increment = 0.25f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.UtilCD, new StepSliderConfig()
      {
        min = 1f,
        max = 5f,
        increment = 0.25f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.SpecialCD, new StepSliderConfig()
      {
        min = 10f,
        max = 50f,
        increment = 1f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.CrushingLeap, new StepSliderConfig()
      {
        min = 0.1f,
        max = 6f,
        increment = 0.1f
      }));
      ModSettingsManager.AddOption(new IntSliderOption(ModConfig.SlamOrbProjectileCount, new IntSliderConfig()
      {
        min = 0,
        max = 16
      }));
      ModSettingsManager.AddOption(new IntSliderOption(ModConfig.LunarShardAdd, new IntSliderConfig()
      {
        min = 1,
        max = 5
      }));
      ModSettingsManager.AddOption(new IntSliderOption(ModConfig.UltimateWaves, new IntSliderConfig()
      {
        min = 4,
        max = 18
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.UltimateDuration, new StepSliderConfig()
      {
        min = 5f,
        max = 10f,
        increment = 0.25f
      }));
      ModSettingsManager.AddOption(new IntSliderOption(ModConfig.JumpWaveCount, new IntSliderConfig()
      {
        min = 12,
        max = 24
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.ShardHoming, new StepSliderConfig()
      {
        min = 10f,
        max = 60f,
        increment = 5f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.ShardRange, new StepSliderConfig()
      {
        min = 80f,
        max = 160f,
        increment = 10f
      }));
      ModSettingsManager.AddOption(new StepSliderOption(ModConfig.ShardCone, new StepSliderConfig()
      {
        min = 90f,
        max = 180f,
        increment = 10f
      }));
      ModSettingsManager.SetModDescription("Moon man with shadows");
    }
  }
}
