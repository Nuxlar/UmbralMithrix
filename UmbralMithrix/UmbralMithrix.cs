using BepInEx;
using EntityStates;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using MonoMod.Cil;
using R2API;
using Rewired.ComponentControls.Effects;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UmbralMithrix
{
  [BepInPlugin("com.Nuxlar.UmbralMithrix", "UmbralMithrix", "2.1.0")]
  [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
  [BepInDependency(LanguageAPI.PluginGUID)]
  [BepInDependency(PrefabAPI.PluginGUID)]
  public class UmbralMithrix : BaseUnityPlugin
  {
    public static bool practiceModeEnabled;
    public static bool hasfired;
    public static bool spawnedClone = false;
    public static bool finishedItemSteal = false;
    public static bool p2ThresholdReached = false;
    public static bool p3ThresholdReached = false;
    public static List<GameObject> timeCrystals = new List<GameObject>();
    public static Dictionary<int, Vector3> p23PizzaPoints = new Dictionary<int, Vector3>()
    {
      {
        0,
        new Vector3(13.5f, 489.7f, -107f)
      },
      {
        1,
        new Vector3(-189f, 489.7f, 107f)
      },
      {
        2,
        new Vector3(16.7f, 489.7f, 101f)
      },
      {
        3,
        new Vector3(-196f, 489.7f, -101f)
      }
    };
    public static Dictionary<int, Vector3> p4PizzaPoints = new Dictionary<int, Vector3>()
    {
      {
        0,
        new Vector3(-175f, 489.7f, -0.08f)
      },
      {
        1,
        new Vector3(-0.08f, 489.7f, 0.08f)
      },
      {
        2,
        new Vector3(-91f, 489.7f, -89f)
      },
      {
        3,
        new Vector3(-89f, 489.7f, 89f)
      }
    };
    public static ItemDef UmbralItem;
    public static GameObject leapIndicatorPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantNovaAreaIndicator.prefab").WaitForCompletion();
    public static GameObject leapIndicator;
    public static SpawnCard timeCrystalCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/WeeklyRun/bscTimeCrystal.asset").WaitForCompletion();
    public static GameObject mithrixMaster = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherMaster.prefab").WaitForCompletion();
    public static GameObject lunarMissile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/EliteLunar/LunarMissileProjectile.prefab").WaitForCompletion();
    public static GameObject mithrixHurtP3Master = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtMaster.prefab").WaitForCompletion(), "BrotherHurtMasterP3");
    public static GameObject mithrix = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherBody.prefab").WaitForCompletion();
    public static GameObject mithrixHurtP3 = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtBody.prefab").WaitForCompletion(), "BrotherHurtBodyP3");
    public static GameObject mithrixHurt = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherHurtBody.prefab").WaitForCompletion();
    public static SpawnCard mithrixCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Brother/cscBrother.asset").WaitForCompletion();
    public static SpawnCard mithrixHurtCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Base/Brother/cscBrotherHurt.asset").WaitForCompletion();
    public static GameObject mithrixGlass = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/BrotherGlass/BrotherGlassBody.prefab").WaitForCompletion();
    public static SpawnCard mithrixGlassCard = Addressables.LoadAssetAsync<SpawnCard>("RoR2/Junk/BrotherGlass/cscBrotherGlass.asset").WaitForCompletion();
    public static GameObject firePillar = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherFirePillar.prefab").WaitForCompletion();
    public static GameObject firePillarGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherFirePillarGhost.prefab").WaitForCompletion();
    public static GameObject leftP4Line = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateLeft.prefab").WaitForCompletion(), "P4UltLineLeft");
    public static GameObject rightP4Line = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateRight.prefab").WaitForCompletion(), "P4UltLineRight");
    public static GameObject leftUltLine = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateLeft.prefab").WaitForCompletion();
    public static GameObject rightUltLine = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateRight.prefab").WaitForCompletion();
    public static GameObject staticUltLine = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherUltLineProjectileRotateLeft.prefab").WaitForCompletion(), "StaticUltLine");
    public static GameObject voidling = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/MiniVoidRaidCrabBodyPhase3.prefab").WaitForCompletion(), "InactiveVoidling");
    public static Material preBossMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Brother/matBrotherPreBossSphere.mat").WaitForCompletion();
    public static Material arenaWallMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon/matMoonArenaWall.mat").WaitForCompletion();
    public static Material stealAuraMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/Brother/matBrotherStealAura.mat").WaitForCompletion();
    public static Material moonMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/moon/matMoonBridge.mat").WaitForCompletion();
    public static Material doppelMat = Addressables.LoadAssetAsync<Material>("RoR2/Base/InvadingDoppelganger/matDoppelganger.mat").WaitForCompletion();
    public static MeteorStormController meteorStormController = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Meteor/MeteorStorm.prefab").WaitForCompletion().GetComponent<MeteorStormController>();
    public static SpawnCard mithrixHurtP3Card = ScriptableObject.CreateInstance<CharacterSpawnCard>();
    public static GameObject youngTeleporter = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/YoungTeleporter.prefab").WaitForCompletion();
    public static Transform practiceFire = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/bazaar/BazaarLight1Large Variant.prefab").WaitForCompletion(), "PracticeFire").transform.GetChild(0);
    public static GameObject implodeEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vagrant/VagrantNovaExplosion.prefab").WaitForCompletion();
    public static GameObject tether = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteEarth/AffixEarthTetherVFX.prefab").WaitForCompletion();
    BuffDef fogNotify = Addressables.LoadAssetAsync<BuffDef>("RoR2/Base/Common/bdVoidFogStrong.asset").WaitForCompletion();

    public void Awake()
    {
      mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "CastUlt").First().requiredSkill = null;
      mithrixHurtP3.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(StaggerEnter));
      mithrixHurtP3Master.GetComponent<CharacterMaster>().bodyPrefab = mithrixHurtP3;
      mithrixHurtP3Card.name = "cscBrotherHurtP3";
      mithrixHurtP3Card.prefab = mithrixHurtP3Master;
      mithrixHurtP3Card.hullSize = mithrixHurtCard.hullSize;
      mithrixHurtP3Card.nodeGraphType = mithrixHurtCard.nodeGraphType;
      mithrixHurtP3Card.requiredFlags = mithrixHurtCard.requiredFlags;
      mithrixHurtP3Card.forbiddenFlags = mithrixHurtCard.forbiddenFlags;
      Destroy(staticUltLine.GetComponent<RotateAroundAxis>());
      leftP4Line.GetComponent<RotateAroundAxis>().fastRotationSpeed = 10f;
      rightP4Line.GetComponent<RotateAroundAxis>().fastRotationSpeed = 10f;
      leftP4Line.GetComponent<RotateAroundAxis>().slowRotationSpeed = 10f;
      rightP4Line.GetComponent<RotateAroundAxis>().slowRotationSpeed = 10f;
      leftUltLine.GetComponent<RotateAroundAxis>().fastRotationSpeed = 21f;
      rightUltLine.GetComponent<RotateAroundAxis>().fastRotationSpeed = 21f;
      leftUltLine.GetComponent<RotateAroundAxis>().slowRotationSpeed = 21f;
      rightUltLine.GetComponent<RotateAroundAxis>().slowRotationSpeed = 21f;
      firePillar.transform.localScale = new Vector3(2f, 2f, 2f);
      firePillarGhost.transform.localScale = new Vector3(2f, 2f, 2f);
      ModConfig.InitConfig(Config);
      CreateDoppelItem();
      P4DeathOrbSetup();
      MiscSetup();
      AddContent();
      MiscHooks miscHooks = new MiscHooks();
      PrimaryHooks primaryHooks = new PrimaryHooks();
      MissionHooks missionHooks = new MissionHooks();
      MithrixMiscHooks mithrixMiscHooks = new MithrixMiscHooks();
    }

    private void P4DeathOrbSetup()
    {
      UmbralMithrix.voidling.transform.GetChild(0).gameObject.SetActive(false);
      List<Material> materials = new List<Material>()
      {
        UmbralMithrix.preBossMat,
        UmbralMithrix.arenaWallMat,
        UmbralMithrix.stealAuraMat
      };
      UmbralMithrix.voidling.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().SetMaterials(materials);
      UmbralMithrix.voidling.GetComponent<SphereZone>().radius = 275f;
      FogDamageController component = UmbralMithrix.voidling.GetComponent<FogDamageController>();
      component.healthFractionPerSecond = 0.01f;
      component.healthFractionRampCoefficientPerSecond = 2.5f;
    }

    private void MiscSetup()
    {
      mithrix.GetComponent<CharacterBody>().bodyFlags = mithrix.GetComponent<CharacterBody>().bodyFlags | CharacterBody.BodyFlags.ImmuneToExecutes;
      mithrixHurtP3.GetComponent<CharacterBody>().bodyFlags = mithrix.GetComponent<CharacterBody>().bodyFlags | CharacterBody.BodyFlags.ImmuneToExecutes;
      mithrixGlass.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(GlassSpawnState));
    }

    public static void ArenaSetup()
    {
      GameObject gameObject1 = GameObject.Find("HOLDER: Final Arena");
      if ((bool)gameObject1)
      {
        gameObject1.transform.GetChild(0).gameObject.SetActive(false);
        gameObject1.transform.GetChild(6).gameObject.SetActive(false);
        Transform child1 = gameObject1.transform.GetChild(1);
        for (int index1 = 8; index1 < 12; ++index1)
        {
          Transform child2 = child1.GetChild(index1);
          for (int index2 = 0; index2 < 4; ++index2)
            child2.GetChild(index2).gameObject.SetActive(true);
        }
      }
      GameObject gameObject2 = GameObject.Find("SceneInfo");
      if (!(bool)gameObject2)
        return;
      gameObject2.transform.GetChild(0).transform.GetChild(3).GetChild(0).GetChild(3).gameObject.SetActive(true);
    }

    public static void SpawnPracticeModeShrine()
    {
      GameObject gameObject1 = Instantiate(youngTeleporter, new Vector3(1090.1f, -283.1f, 1138.6f), Quaternion.identity);
      GameObject gameObject2 = Instantiate(practiceFire.gameObject, new Vector3(1090.1f, -283.1f, 1138.6f), Quaternion.identity);
      gameObject1.GetComponent<PurchaseInteraction>().NetworkcontextToken = "Enable Practice Mode? (respawn on death)";
      gameObject1.name = "PracticeModeShrine";
      gameObject2.transform.parent = gameObject1.transform;
      gameObject1.transform.GetChild(1).localPosition = new Vector3(0.0f, 0.0f, 0.0f);
      NetworkServer.Spawn(gameObject1);
    }

    public static void AdjustBaseStats()
    {
      CharacterBody component1 = mithrix.GetComponent<CharacterBody>();
      CharacterDirection component2 = mithrix.GetComponent<CharacterDirection>();
      CharacterMotor component3 = mithrix.GetComponent<CharacterMotor>();
      CharacterBody component4 = mithrixGlass.GetComponent<CharacterBody>();
      CharacterDirection component5 = mithrixGlass.GetComponent<CharacterDirection>();
      CharacterMotor component6 = mithrixGlass.GetComponent<CharacterMotor>();
      component3.mass = ModConfig.mass.Value;
      component3.airControl = ModConfig.aircontrol.Value;
      component4.baseMaxHealth = ModConfig.basehealth.Value;
      component4.levelMaxHealth = ModConfig.levelhealth.Value;
      component4.baseDamage = ModConfig.basedamage.Value / 2f;
      component4.levelDamage = ModConfig.leveldamage.Value / 2f;
      component6.airControl = ModConfig.aircontrol.Value;
      component5.turnSpeed = ModConfig.turningspeed.Value;
      component1.baseMaxHealth = ModConfig.basehealth.Value;
      component1.levelMaxHealth = ModConfig.levelhealth.Value;
      component1.baseDamage = ModConfig.basedamage.Value;
      component1.levelDamage = ModConfig.leveldamage.Value;
      component1.baseAttackSpeed = ModConfig.baseattackspeed.Value;
      component1.baseMoveSpeed = ModConfig.basespeed.Value;
      component1.baseAcceleration = ModConfig.acceleration.Value;
      component1.baseJumpPower = ModConfig.jumpingpower.Value;
      component2.turnSpeed = ModConfig.turningspeed.Value;
      component1.baseArmor = ModConfig.basearmor.Value;
      FireLunarShards.projectilePrefab.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = ModConfig.ShardHoming.Value;
      ProjectileDirectionalTargetFinder component7 = FireLunarShards.projectilePrefab.GetComponent<ProjectileDirectionalTargetFinder>();
      component7.lookRange = ModConfig.ShardRange.Value;
      component7.lookCone = ModConfig.ShardCone.Value;
      component7.allowTargetLoss = true;
      WeaponSlam.duration = 3.5f / ModConfig.baseattackspeed.Value;
      ExitSkyLeap.waveProjectileCount = ModConfig.JumpWaveCount.Value;
      UltChannelState.waveProjectileCount = ModConfig.UltimateWaves.Value;
      UltChannelState.maxDuration = ModConfig.UltimateDuration.Value;
      UltChannelState.totalWaves = ModConfig.UltimateCount.Value;
      ExitSkyLeap.cloneDuration = (int)Math.Round((double)ModConfig.SpecialCD.Value);
    }

    public static void AdjustBaseSkills()
    {
      SkillLocator component = mithrix.GetComponent<SkillLocator>();
      SkillDef skillDef1 = component.primary.skillFamily.variants[0].skillDef;
      skillDef1.baseRechargeInterval = ModConfig.PrimCD.Value;
      skillDef1.baseMaxStock = ModConfig.PrimStocks.Value;
      SkillDef skillDef2 = component.secondary.skillFamily.variants[0].skillDef;
      skillDef2.baseRechargeInterval = ModConfig.SecCD.Value;
      skillDef2.baseMaxStock = ModConfig.SecStocks.Value;
      SkillDef skillDef3 = component.utility.skillFamily.variants[0].skillDef;
      skillDef3.baseMaxStock = ModConfig.UtilStocks.Value;
      skillDef3.baseRechargeInterval = ModConfig.UtilCD.Value;
      SkillDef skillDef4 = component.special.skillFamily.variants[0].skillDef;
      skillDef4.baseRechargeInterval = ModConfig.SpecialCD.Value;
      // skillDef4.activationState = PlayerCharacterMasterController.instances.Count > 1 ? new SerializableEntityStateType(typeof(EnterSkyLeap)) : new SerializableEntityStateType(typeof(EnterCrushingLeap));
    }

    public static void AdjustPhase2Stats()
    {
      mithrix.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(SkySpawnState));
      CharacterBody component1 = mithrix.GetComponent<CharacterBody>();
      CharacterDirection component2 = mithrix.GetComponent<CharacterDirection>();
      component1.baseMaxHealth = ModConfig.basehealth.Value * 1.5f;
      component1.levelMaxHealth = ModConfig.levelhealth.Value * 1.5f;
      component1.baseMoveSpeed = ModConfig.basespeed.Value;
      component1.baseAcceleration = ModConfig.acceleration.Value;
      component1.baseJumpPower = ModConfig.jumpingpower.Value;
      component2.turnSpeed = ModConfig.turningspeed.Value;
      WeaponSlam.duration = 3.5f / ModConfig.baseattackspeed.Value;
    }

    public static void AdjustPhase3Stats()
    {
      CharacterBody component1 = mithrix.GetComponent<CharacterBody>();
      CharacterBody component2 = mithrixHurtP3.GetComponent<CharacterBody>();
      CharacterDirection component3 = mithrix.GetComponent<CharacterDirection>();
      component1.baseMaxHealth = ModConfig.basehealth.Value;
      component1.levelMaxHealth = ModConfig.levelhealth.Value;
      component1.baseDamage = ModConfig.basedamage.Value;
      component1.levelDamage = ModConfig.leveldamage.Value;
      component2.baseMaxHealth = ModConfig.basehealth.Value;
      component2.levelMaxHealth = ModConfig.levelhealth.Value;
      component2.baseDamage = ModConfig.basedamage.Value / 2f;
      component2.levelDamage = ModConfig.leveldamage.Value / 2f;
      component1.baseMoveSpeed = ModConfig.basespeed.Value;
      component1.baseAcceleration = ModConfig.acceleration.Value;
      component1.baseJumpPower = ModConfig.jumpingpower.Value;
      component3.turnSpeed = ModConfig.turningspeed.Value;
      WeaponSlam.duration = 3.5f / ModConfig.baseattackspeed.Value;
      UltChannelState.waveProjectileCount = ModConfig.UltimateWaves.Value;
      UltChannelState.maxDuration = ModConfig.UltimateDuration.Value;
    }

    public static void AdjustPhase4Stats()
    {
      CharacterBody component = mithrixHurt.GetComponent<CharacterBody>();
      component.baseDamage = ModConfig.basedamage.Value;
      component.levelDamage = ModConfig.leveldamage.Value;
      component.GetComponent<SkillLocator>().primary = new GenericSkill();
      component.GetComponent<SkillLocator>().secondary = new GenericSkill();
    }

    private void AddContent()
    {
      ContentAddition.AddEntityState<GlassSpawnState>(out _);
    }

    private void CreateDoppelItem()
    {
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_NAME", "Core of The Collective");
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_PICKUP", "A trophy from a battle hard fought.");
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_DESC", "A trophy from a battle hard fought.");
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_LORE", "We should have stopped.\n\nWhen we discovered the void, it raised a lot of questions for us. The void was a physical place, a depository for a sapient species that fit somewhere in the matter of our realm, a curiosity we could observe. But there were seams, an inbetween space that sewed places like the void and ours together. The wall that separated corporeality, a fathomless null stretched to infinity. An ineffable, dark vastness that resided between, truly embodying the name of what 'the void' meant to many before our discoveries. It was decidedly designated as separate from our physical realm, it was simply declared as 'something else'. A dark nothing, an umbral domain.\n\nWe should have left it alone.\n\nA few of us wanted to study this further, an advancement into metaphysical elements could have bridged the gap into faster than light travel. Tapping into this space, it became immediately clear we weren't alone. We expected the dread, the feeling of cold nothing as your physical senses were ripped from your consciousness. We didn't expect the hatred. Overwhelming malevolence and contempt filled my mind, touching edges of my consciousness I've never felt before, laying waste to memories, replacing them all with that twisted disdain. It was only for a moment, but I felt myself there for eternity, drifting in that inbetween. I had a revelation in that time, in the fleeting moments between where my mind could bear the onslaught. The hatred wasn't directed at me, this wasn't a result of us trespassing or invading, it was loathing pure and undiluted. It was indiscriminate\n\nIt was inevitable.\n\nWe destroyed the pathway to this place of course. We held on to this secret, bearing the responsibility as guardians. A terrible burden, but required. Years later I still find myself reeling, unable to banish that violating malevolence from the hidden corners of my mind. Now though, I’m at the end of my life and I've long since realized my greatest fears may come to pass without my intervention. When I - the last guardian - am gone, the gate of this prison may yet again be opened and without concern for that malignant presence that would pass through. I use this as my last testament to implore you, to beg, do not open that gate. Do not let that horror manifest itself outside of that ethereal realm. Do not set that umbra free.");
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_SUBTITLENAMETOKEN", "The Collective");
      LanguageAPI.Add("UMBRALMITHRIX_UMBRAL_MODIFIER", "Umbral");
      UmbralItem = ScriptableObject.CreateInstance<ItemDef>();
      UmbralItem.name = "UmbralMithrixUmbralItem";
      UmbralItem.deprecatedTier = ItemTier.Lunar;
      UmbralItem.nameToken = "UMBRALMITHRIX_UMBRAL_NAME";
      UmbralItem.pickupToken = "UMBRALMITHRIX_UMBRAL_PICKUP";
      UmbralItem.descriptionToken = "UMBRALMITHRIX_UMBRAL_DESC";
      UmbralItem.loreToken = "UMBRALMITHRIX_UMBRAL_LORE";
      UmbralItem.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/TeamDeath/texArtifactDeathDisabled.png").WaitForCompletion();
      UmbralItem.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamDeath/PickupTeamDeath.prefab").WaitForCompletion(), "PickupUmbralCore", false);
      Material material = Addressables.LoadAssetAsync<Material>("RoR2/Base/InvadingDoppelganger/matDoppelganger.mat").WaitForCompletion();
      foreach (Renderer componentsInChild in UmbralItem.pickupModelPrefab.GetComponentsInChildren<Renderer>())
        componentsInChild.material = material;
      ContentAddition.AddItemDef(UmbralItem);
    }
  }
}