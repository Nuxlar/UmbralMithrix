using BepInEx;
using EntityStates;
using EntityStates.BrotherMonster;
using R2API;
using Rewired.ComponentControls.Effects;
using RoR2;
using RoR2.CharacterAI;
using RoR2.ContentManagement;
using RoR2.Projectile;
using RoR2.Skills;
using System.Collections.Generic;
using System.Linq;
using UmbralMithrix.EntityStates;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using BepInEx.Bootstrap;
using HG;
using System;
using UmbralMithrix.Components;

namespace UmbralMithrix
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    public class UmbralMithrix : BaseUnityPlugin
    {
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Nuxlar";
        public const string PluginName = "UmbralMithrix";
        public const string PluginVersion = "2.5.6";

        internal static UmbralMithrix Instance { get; private set; }

        public static bool RooInstalled => Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
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

        public static GameObject umbralSlamImpact;
        public static GameObject umbralSlamProjectile;
        public static GameObject umbralSlamPillar;
        public static GameObject umbralSlamHitEffect;
        public static NetworkSoundEventDef umbralSlamHitSound;

        public static GameObject umbralLeapWave;
        public static GameObject umbralSwingEffect;
        public static GameObject umbralUltMuzzleFlash;

        public static GameObject leapIndicatorPrefab;
        public static GameObject leapIndicator;
        public static SpawnCard timeCrystalCard;
        public static GameObject lunarMissile;
        public static GameObject mithrixHurtP3Master;
        public static GameObject mithrix;
        public static GameObject mithrixHurtP3;
        public static GameObject mithrixHurt;
        public static SpawnCard mithrixCard;
        public static SpawnCard mithrixHurtCard;
        public static GameObject mithrixGlass;
        public static SpawnCard mithrixGlassCard;
        public static GameObject leftP4Line;
        public static GameObject rightP4Line;
        public static GameObject leftUltLine;
        public static GameObject rightUltLine;
        public static GameObject staticUltLine;
        public static GameObject shardProjectile;
        public static Material preBossMat;
        public static Material arenaWallMat;
        public static Material stealAuraMat;
        public static Material moonMat;
        public static Material doppelMat;
        public static GameObject youngTeleporter;
        public static Transform practiceFire;
        public static GameObject implodeEffect;
        public static GameObject tether;
        public static GameObject voidling;
        public static SpawnCard mithrixHurtP3Card = ScriptableObject.CreateInstance<CharacterSpawnCard>();
        // TODO: actually make the skills and do this less lazily
        private static SkillDef shardDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/FireLunarShards.asset").WaitForCompletion();
        private static SkillDef slamDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/WeaponSlam.asset").WaitForCompletion();
        private static SkillDef bashDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/SprintBash.asset").WaitForCompletion();
        private static SkillDef leapDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/SkyLeap.asset").WaitForCompletion();
        public static SkillDef ultDef = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Brother/Ult.asset").WaitForCompletion();

        public void Awake()
        {
            Instance = this;

            Log.Init(Logger);

            ModConfig.Init();

            LoadAssets();
            CloneAssets();
            SetupVoidling();

            /*
                P2 HP most likely
                P3 Pizza being vanilla
            */

            // TODO: actually make the skills and do this less lazily
            shardDef.activationState = new SerializableEntityStateType(typeof(FireUmbralShards));
            slamDef.activationState = new SerializableEntityStateType(typeof(UmbralHammerSlam));
            bashDef.activationState = new SerializableEntityStateType(typeof(UmbralBash));
            leapDef.activationState = new SerializableEntityStateType(typeof(EnterUmbralLeap));
            ultDef.activationState = new SerializableEntityStateType(typeof(EnterUmbralUlt));

            CreateDoppelItem();
            AddEntityStates();
            ChangeVanillaEntityStateValues();

            new MiscHooks();
            new MissionHooks();
            new MithrixMiscHooks();

            LanguageManager.Register(System.IO.Path.GetDirectoryName(Info.Location));
        }

        private void ChangeVanillaEntityStateValues()
        {
            SetVanillaEntityStateField(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother_EntityStates_BrotherMonster.FistSlam_asset, "healthCostFraction", "0");
            SetVanillaEntityStateField(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother_EntityStates_BrotherMonster.SpellChannelEnterState_asset, "duration", "3");
            SetVanillaEntityStateField(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother_EntityStates_BrotherMonster.SpellChannelState_asset, "maxDuration", "5");
        }

        public static void ArenaSetup()
        {
            GameObject finalAreaHolder = GameObject.Find("HOLDER: Final Arena");
            if (finalAreaHolder)
            {
                Transform innerColumns = finalAreaHolder.transform.Find("Columns_Inner");
                if (innerColumns)
                {
                    innerColumns.gameObject.SetActive(false);
                }

                Transform rocks = finalAreaHolder.transform.Find("Rocks");
                if (rocks)
                {
                    rocks.gameObject.SetActive(false);
                }
            }

            SceneInfo sceneInfo = SceneInfo.instance;
            if (sceneInfo)
            {
                Transform throneTransform = sceneInfo.transform.Find("BrotherMissionController/BrotherEncounter, Phase 1/PhaseObjects/mdlBrotherThrone");
                if (throneTransform)
                {
                    throneTransform.gameObject.SetActive(true);
                }
            }
        }

        public static void SpawnPracticeModeShrine()
        {
            GameObject gameObject1 = Instantiate(youngTeleporter, new Vector3(1090.1f, -283.1f, 1138.6f), Quaternion.identity);
            GameObject gameObject2 = Instantiate(practiceFire.gameObject, new Vector3(1090.1f, -283.1f, 1138.6f), Quaternion.identity);
            gameObject1.GetComponent<PurchaseInteraction>().NetworkcontextToken = "UMBRAL_PRACTICE_MODE_CONTEXT";
            gameObject1.name = "PracticeModeShrine";
            gameObject2.transform.parent = gameObject1.transform;
            gameObject2.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            NetworkServer.Spawn(gameObject1);
        }

        private void AddEntityStates()
        {
            ContentAddition.AddEntityState<GlassSpawnState>(out _);
            ContentAddition.AddEntityState<FireUmbralShards>(out _);
            ContentAddition.AddEntityState<UmbralHammerSlam>(out _);
            ContentAddition.AddEntityState<EnterUmbralLeap>(out _);
            ContentAddition.AddEntityState<HoldUmbralLeap>(out _);
            ContentAddition.AddEntityState<ExitUmbralLeap>(out _);
            ContentAddition.AddEntityState<UmbralBash>(out _);
        }

        private void CreateDoppelItem()
        {
            UmbralItem = ScriptableObject.CreateInstance<ItemDef>();
            UmbralItem.name = "UmbralMithrixUmbralItem";
            UmbralItem.deprecatedTier = ItemTier.Lunar;
            UmbralItem.tags = new ItemTag[] { ItemTag.WorldUnique, ItemTag.AIBlacklist, ItemTag.CannotCopy, ItemTag.CannotDuplicate };
            UmbralItem.nameToken = "UMBRALMITHRIX_UMBRAL_ITEM_NAME";
            UmbralItem.pickupToken = "UMBRALMITHRIX_UMBRAL_ITEM_PICKUP";
            UmbralItem.descriptionToken = "UMBRALMITHRIX_UMBRAL_ITEM_DESC";
            UmbralItem.loreToken = "UMBRALMITHRIX_UMBRAL_ITEM_LORE";
            UmbralItem.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/TeamDeath/texArtifactDeathDisabled.png").WaitForCompletion();
            UmbralItem.pickupModelPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamDeath/PickupTeamDeath.prefab").WaitForCompletion(), "PickupUmbralCore", false);
            Material material = Addressables.LoadAssetAsync<Material>("RoR2/Base/InvadingDoppelganger/matDoppelganger.mat").WaitForCompletion();
            foreach (Renderer componentsInChild in UmbralItem.pickupModelPrefab.GetComponentsInChildren<Renderer>())
                componentsInChild.material = material;
            ContentAddition.AddItemDef(UmbralItem);
        }

        public static void SetVanillaEntityStateField(string fullEntityStatePath, string fieldName, string value)
        {
            AssetReferenceT<EntityStateConfiguration> escRef = new AssetReferenceT<EntityStateConfiguration>(fullEntityStatePath);
            AssetAsyncReferenceManager<EntityStateConfiguration>.LoadAsset(escRef).Completed += (x) =>
            {
                EntityStateConfiguration esc = x.Result;
                for (int i = 0; i < esc.serializedFieldsCollection.serializedFields.Length; i++)
                {
                    if (esc.serializedFieldsCollection.serializedFields[i].fieldName == fieldName)
                    {
                        esc.serializedFieldsCollection.serializedFields[i].fieldValue.stringValue = value;
                    }
                }
            };
        }

        private void SetupVoidling()
        {
            AssetReferenceT<GameObject> voidlingRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidRaidCrab.MiniVoidRaidCrabBodyPhase3_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(voidlingRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                voidling = PrefabAPI.InstantiateClone(result, "InactiveVoidling");

                if (voidling.TryGetComponent(out ModelLocator modelLocator))
                {
                    Transform modelTransform = modelLocator.modelTransform;
                    if (modelTransform)
                    {
                        modelTransform.gameObject.SetActive(false);
                    }
                }

                SphereZone safeZone = voidling.GetComponent<SphereZone>();
                safeZone.radius = 275f;

                if (safeZone.rangeIndicator)
                {
                    MeshRenderer rangeIndicatorRenderer = safeZone.rangeIndicator.GetComponentInChildren<MeshRenderer>();
                    if (rangeIndicatorRenderer)
                    {
                        rangeIndicatorRenderer.sharedMaterials = [
                            preBossMat,
                            arenaWallMat,
                            stealAuraMat
                        ];
                    }
                }

                FogDamageController fogDamageController = voidling.GetComponent<FogDamageController>();
                fogDamageController.healthFractionPerSecond = 0.01f;
                fogDamageController.healthFractionRampCoefficientPerSecond = 2.5f;
            };
        }

        private void CloneAssets()
        {
            AssetReferenceT<GameObject> leftUltLineRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherUltLineProjectileRotateLeft_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(leftUltLineRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                leftUltLine = PrefabAPI.InstantiateClone(result, "UmbralUltLineLeft");
                leftP4Line = PrefabAPI.InstantiateClone(result, "P4UltLineLeft");
                staticUltLine = PrefabAPI.InstantiateClone(result, "StaticUltLine");
                Destroy(staticUltLine.GetComponent<RotateAroundAxis>());

                RotateAroundAxis leftUltRotate = leftUltLine.GetComponent<RotateAroundAxis>();
                leftUltRotate.fastRotationSpeed = 21f;
                leftUltRotate.slowRotationSpeed = 21f;

                RotateAroundAxis leftP4Rotate = leftP4Line.GetComponent<RotateAroundAxis>();
                leftP4Rotate.fastRotationSpeed = 10f;
                leftP4Rotate.slowRotationSpeed = 10f;
            };
            AssetReferenceT<GameObject> rightUltLineRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherUltLineProjectileRotateRight_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(rightUltLineRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                rightUltLine = PrefabAPI.InstantiateClone(result, "UmbralUltLineRight");
                rightP4Line = PrefabAPI.InstantiateClone(result, "P4UltLineRight");

                RotateAroundAxis rightUltRotate = rightUltLine.GetComponent<RotateAroundAxis>();
                rightUltRotate.fastRotationSpeed = 21f;
                rightUltRotate.slowRotationSpeed = 21f;

                RotateAroundAxis rightP4Rotate = rightP4Line.GetComponent<RotateAroundAxis>();
                rightP4Rotate.fastRotationSpeed = 10f;
                rightP4Rotate.slowRotationSpeed = 10f;
            };
            AssetReferenceT<GameObject> bodyP3Ref = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherHurtBody_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(bodyP3Ref).Completed += (x) =>
            {
                GameObject result = x.Result;
                mithrixHurtP3 = PrefabAPI.InstantiateClone(result, "BrotherHurtBodyP3");
                mithrixHurtP3.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(StaggerEnter));
                CharacterBody characterBody = mithrixHurtP3.GetComponent<CharacterBody>();

                characterBody.bodyFlags |= CharacterBody.BodyFlags.ImmuneToExecutes;
                characterBody.baseMaxHealth = ModConfig.basehealth.Value;
                characterBody.levelMaxHealth = ModConfig.levelhealth.Value;
                characterBody.baseDamage = ModConfig.basedamage.Value / 2f;
                characterBody.levelDamage = ModConfig.leveldamage.Value / 2f;

                ContentAddition.AddBody(mithrixHurtP3);

                mithrixHurt = x.Result;
                mithrixHurt.AddComponent<P4Controller>();
                CharacterBody hurtBody = mithrixHurt.GetComponent<CharacterBody>();

                hurtBody.baseDamage = ModConfig.basedamage.Value;
                hurtBody.levelDamage = ModConfig.leveldamage.Value;
            };
            AssetReferenceT<GameObject> masterP3Ref = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherHurtMaster_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(masterP3Ref).Completed += (x) =>
            {
                GameObject result = x.Result;
                mithrixHurtP3Master = PrefabAPI.InstantiateClone(result, "BrotherHurtMasterP3");
                mithrixHurtP3Master.GetComponent<CharacterMaster>().bodyPrefab = mithrixHurtP3;

                mithrixHurtP3Card.name = "cscBrotherHurtP3";
                mithrixHurtP3Card.prefab = mithrixHurtP3Master;
                mithrixHurtP3Card.hullSize = mithrixHurtCard.hullSize;
                mithrixHurtP3Card.nodeGraphType = mithrixHurtCard.nodeGraphType;
                mithrixHurtP3Card.requiredFlags = mithrixHurtCard.requiredFlags;
                mithrixHurtP3Card.forbiddenFlags = mithrixHurtCard.forbiddenFlags;

                ContentAddition.AddMaster(mithrixHurtP3Master);
            };
            AssetReferenceT<GameObject> practiceFireRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_bazaar_Bazaar.Light_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(practiceFireRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                practiceFire = PrefabAPI.InstantiateClone(result.transform.Find("FireLODLevel").gameObject, "PracticeFire").transform;
            };
            AssetReferenceT<GameObject> lunarMissileRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_EliteLunar.LunarMissileProjectile_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(lunarMissileRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                lunarMissile = PrefabAPI.InstantiateClone(result, "UmbralLunarMissile", false);
            };
            AssetReferenceT<GameObject> indicatorRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Vagrant.VagrantNovaAreaIndicator_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(indicatorRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                leapIndicatorPrefab = PrefabAPI.InstantiateClone(result, "UmbralLeapIndicator");
                leapIndicatorPrefab.AddComponent<NetworkIdentity>();
            };
            AssetReferenceT<GameObject> waveRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherSunderWave_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(waveRef).Completed += (x) =>
            {
                GameObject result = x.Result;
                umbralLeapWave = PrefabAPI.InstantiateClone(result, "UmbralLeapWave");
            };
        }

        private void LoadAssets()
        {
            AssetReferenceT<GameObject> tpRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Junk.YoungTeleporter_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(tpRef).Completed += (x) => youngTeleporter = x.Result;
            AssetReferenceT<GameObject> tetherRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_EliteEarth.AffixEarthTetherVFX_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(tetherRef).Completed += (x) => tether = x.Result;
            AssetReferenceT<GameObject> implodeRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Vagrant.VagrantNovaExplosion_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(implodeRef).Completed += (x) => implodeEffect = x.Result;
            AssetReferenceT<GameObject> slamImpactRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherSlamImpact_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(slamImpactRef).Completed += (x) => umbralSlamImpact = x.Result;
            AssetReferenceT<GameObject> slamProjectileRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother_BrotherSunderWave.Energized_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(slamProjectileRef).Completed += (x) => umbralSlamProjectile = x.Result;
            AssetReferenceT<GameObject> slamPillarRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherFirePillar_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(slamPillarRef).Completed += (x) => umbralSlamPillar = x.Result;
            AssetReferenceT<GameObject> slamHitRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Huntress.OmniImpactVFXHuntress_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(slamHitRef).Completed += (x) => umbralSlamHitEffect = x.Result;
            AssetReferenceT<GameObject> swingEffectRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother_BrotherSwing1.Kickup_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(swingEffectRef).Completed += (x) => umbralSwingEffect = x.Result;
            AssetReferenceT<GameObject> ultFlashRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.ItemStealEndMuzzleflash_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(ultFlashRef).Completed += (x) => umbralUltMuzzleFlash = x.Result;
            AssetReferenceT<GameObject> masterRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherMaster_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(masterRef).Completed += (x) =>
            {
                GameObject mithrixMaster = x.Result;
                //  mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "Sprint after Target").First().minDistance = 25f;
                mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "CastUlt").First().requiredSkill = null;
                AISkillDriver fireShards = mithrixMaster.GetComponents<AISkillDriver>().Where(x => x.customName == "Sprint and FireLunarShards").First();
                fireShards.minDistance = 20f;
            };
            AssetReferenceT<GameObject> bodyRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.BrotherBody_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(bodyRef).Completed += (x) =>
            {
                mithrix = x.Result;
                CharacterBody characterBody = mithrix.GetComponent<CharacterBody>();
                CharacterDirection characterDirection = mithrix.GetComponent<CharacterDirection>();
                CharacterMotor characterMotor = mithrix.GetComponent<CharacterMotor>();

                characterBody.bodyFlags |= CharacterBody.BodyFlags.ImmuneToExecutes;
                characterBody.baseMaxHealth = ModConfig.basehealth.Value;
                characterBody.levelMaxHealth = ModConfig.levelhealth.Value;
                characterBody.baseDamage = ModConfig.basedamage.Value;
                characterBody.levelDamage = ModConfig.leveldamage.Value;
                characterBody.baseAttackSpeed = ModConfig.baseattackspeed.Value;
                characterBody.baseMoveSpeed = ModConfig.basespeed.Value;
                characterBody.baseAcceleration = ModConfig.acceleration.Value;
                characterBody.baseJumpPower = ModConfig.jumpingpower.Value;
                characterBody.baseArmor = ModConfig.basearmor.Value;

                characterDirection.turnSpeed = ModConfig.turningspeed.Value;

                characterMotor.mass = ModConfig.mass.Value;
                characterMotor.airControl = ModConfig.aircontrol.Value;

                SkillLocator skillLocator = mithrix.GetComponent<SkillLocator>();
                SkillDef skillDef1 = skillLocator.primary.skillFamily.variants[0].skillDef;
                skillDef1.baseRechargeInterval = ModConfig.PrimCD.Value;
                skillDef1.baseMaxStock = ModConfig.PrimStocks.Value;
                SkillDef skillDef2 = skillLocator.secondary.skillFamily.variants[0].skillDef;
                skillDef2.baseRechargeInterval = ModConfig.SecCD.Value;
                skillDef2.baseMaxStock = ModConfig.SecStocks.Value;
                SkillDef skillDef3 = skillLocator.utility.skillFamily.variants[0].skillDef;
                skillDef3.baseMaxStock = ModConfig.UtilStocks.Value;
                skillDef3.baseRechargeInterval = ModConfig.UtilCD.Value;
                SkillDef skillDef4 = skillLocator.special.skillFamily.variants[0].skillDef;
                skillDef4.baseRechargeInterval = ModConfig.SpecialCD.Value;
            };
            AssetReferenceT<GameObject> glassBodyRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Junk_BrotherGlass.BrotherGlassBody_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(glassBodyRef).Completed += (x) =>
            {
                mithrixGlass = x.Result;
                mithrixGlass.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(EntityStates.GlassSpawnState));

                CharacterBody characterBody = mithrixGlass.GetComponent<CharacterBody>();
                CharacterDirection characterDirection = mithrixGlass.GetComponent<CharacterDirection>();
                CharacterMotor characterMotor = mithrixGlass.GetComponent<CharacterMotor>();

                characterBody.baseMaxHealth = ModConfig.basehealth.Value;
                characterBody.levelMaxHealth = ModConfig.levelhealth.Value;

                characterBody.baseDamage = ModConfig.basedamage.Value / 2f;
                characterBody.levelDamage = ModConfig.leveldamage.Value / 2f;
                characterMotor.airControl = ModConfig.aircontrol.Value;
                characterDirection.turnSpeed = ModConfig.turningspeed.Value;

                Transform modelTransform = null;
                if (mithrixGlass.TryGetComponent(out ModelLocator modelLocator))
                {
                    modelTransform = modelLocator.modelTransform;
                }

                if (modelTransform)
                {
                    SkinDef originalSkin = AssetAsyncReferenceManager<SkinDef>.LoadAsset(new AssetReferenceT<SkinDef>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.skinBrotherBodyDefault_asset)).WaitForCompletion();

                    ModelSkinController modelSkinController = modelTransform.gameObject.EnsureComponent<ModelSkinController>();
                    int replacementSkinIndex = Array.IndexOf(modelSkinController.skins, originalSkin);

                    SkinDef skinDef = Instantiate(originalSkin);
                    skinDef.name = "skinBrotherGlassBodyDefault";
                    Transform originalSkinRoot = skinDef.rootObject.transform;
                    skinDef.rootObject = modelTransform.gameObject;

                    (AssetReferenceT<SkinDefParams> paramsAddress, SkinDefParams paramsDirect) = skinDef.GetSkinParams();
                    AssetOrDirectReference<SkinDefParams> skinDefParamsReference = new AssetOrDirectReference<SkinDefParams>
                    {
                        address = paramsAddress,
                        directRef = paramsDirect
                    };

                    SkinDefParams skinDefParams = Instantiate(skinDefParamsReference.WaitForCompletion());
                    skinDefParams.name = $"{skinDef.name}_params";
                    skinDef.skinDefParams = skinDefParams;
                    skinDef.skinDefParamsAddress = new AssetReferenceT<SkinDefParams>(string.Empty);
                    skinDef.optimizedSkinDefParams = skinDefParams;
                    skinDef.optimizedSkinDefParamsAddress = new AssetReferenceT<SkinDefParams>(string.Empty);

                    List<CharacterModel.RendererInfo> rendererInfos = [.. skinDefParams.rendererInfos];
                    List<SkinDefParams.GameObjectActivation> gameObjectActivations = [.. skinDefParams.gameObjectActivations];
                    List<SkinDefParams.MeshReplacement> meshReplacements = [.. skinDefParams.meshReplacements];
                    List<CharacterModel.LightInfo> lightReplacements = [.. skinDefParams.lightReplacements];

                    for (int i = rendererInfos.Count - 1; i >= 0; i--)
                    {
                        CharacterModel.RendererInfo rendererInfo = rendererInfos[i];

                        Renderer originalRenderer = rendererInfo.renderer;
                        rendererInfo.renderer = null;
                        if (originalRenderer && originalRenderer.transform.IsChildOf(originalSkinRoot))
                        {
                            string rendererPath = Util.BuildPrefabTransformPath(originalSkinRoot, originalRenderer.transform);

                            if (!string.IsNullOrEmpty(rendererPath))
                            {
                                Transform rendererTransform = modelTransform.Find(rendererPath);
                                if (rendererTransform)
                                {
                                    rendererInfo.renderer = rendererTransform.GetComponent(originalRenderer.GetType()) as Renderer;
                                }
                            }
                        }

                        if (rendererInfo.renderer)
                        {
                            switch (rendererInfo.renderer.name)
                            {
                                case "BrotherHammerConcrete":
                                case "BrotherBodyMesh":
                                    rendererInfo.defaultMaterial = null;
                                    rendererInfo.defaultMaterialAddress = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.maBrotherGlassOverlay_mat);
                                    break;
                            }

                            rendererInfos[i] = rendererInfo;
                        }
                        else
                        {
                            rendererInfos.RemoveAt(i);
                        }
                    }

                    for (int i = gameObjectActivations.Count - 1; i >= 0; i--)
                    {
                        SkinDefParams.GameObjectActivation gameObjectActivation = gameObjectActivations[i];

                        GameObject originalObject = gameObjectActivation.gameObject;
                        gameObjectActivation.gameObject = null;

                        if (originalObject && originalObject.transform.IsChildOf(originalSkinRoot))
                        {
                            string objectPath = Util.BuildPrefabTransformPath(originalSkinRoot, originalObject.transform);

                            if (!string.IsNullOrEmpty(objectPath))
                            {
                                Transform objectTransform = modelTransform.Find(objectPath);
                                if (objectTransform)
                                {
                                    gameObjectActivation.gameObject = objectTransform.gameObject;
                                }
                            }
                        }

                        if (gameObjectActivation.gameObject)
                        {
                            gameObjectActivations[i] = gameObjectActivation;
                        }
                        else
                        {
                            gameObjectActivations.RemoveAt(i);
                        }
                    }

                    for (int i = meshReplacements.Count - 1; i >= 0; i--)
                    {
                        SkinDefParams.MeshReplacement meshReplacement = meshReplacements[i];

                        Renderer originalRenderer = meshReplacement.renderer;
                        meshReplacement.renderer = null;

                        if (originalRenderer && originalRenderer.transform.IsChildOf(originalSkinRoot))
                        {
                            string rendererPath = Util.BuildPrefabTransformPath(originalSkinRoot, originalRenderer.transform);

                            if (!string.IsNullOrEmpty(rendererPath))
                            {
                                Transform rendererTransform = modelTransform.Find(rendererPath);
                                if (rendererTransform)
                                {
                                    meshReplacement.renderer = rendererTransform.GetComponent(originalRenderer.GetType()) as Renderer;
                                }
                            }
                        }

                        if (meshReplacement.renderer)
                        {
                            meshReplacements[i] = meshReplacement;
                        }
                        else
                        {
                            meshReplacements.RemoveAt(i);
                        }
                    }

                    for (int i = lightReplacements.Count - 1; i >= 0; i--)
                    {
                        CharacterModel.LightInfo lightReplacement = lightReplacements[i];

                        Light originalLight = lightReplacement.light;
                        lightReplacement.light = null;

                        if (originalLight && originalLight.transform.IsChildOf(originalSkinRoot))
                        {
                            string lightPath = Util.BuildPrefabTransformPath(originalSkinRoot, originalLight.transform);
                            if (!string.IsNullOrEmpty(lightPath))
                            {
                                Transform lightTransform = modelTransform.Find(lightPath);
                                if (lightTransform)
                                {
                                    lightReplacement.light = lightTransform.GetComponent(originalLight.GetType()) as Light;
                                }
                            }
                        }

                        if (lightReplacement.light)
                        {
                            lightReplacements[i] = lightReplacement;
                        }
                        else
                        {
                            lightReplacements.RemoveAt(i);
                        }
                    }

                    skinDefParams.rendererInfos = [.. rendererInfos];
                    skinDefParams.gameObjectActivations = [.. gameObjectActivations];
                    skinDefParams.meshReplacements = [.. meshReplacements];
                    skinDefParams.lightReplacements = [.. lightReplacements];

                    if (ArrayUtils.IsInBounds(modelSkinController.skins, replacementSkinIndex))
                    {
                        modelSkinController.skins[replacementSkinIndex] = skinDef;
                    }
                    else
                    {
                        ArrayUtils.ArrayAppend(ref modelSkinController.skins, skinDef);
                    }

                    PersistentOverlayController overlayController = modelTransform.gameObject.EnsureComponent<PersistentOverlayController>();
                    overlayController.OverlayMaterialReference = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.matBrotherGlassDistortion_mat);
                }
            };
            AssetReferenceT<GameObject> shardRef = new AssetReferenceT<GameObject>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.LunarShardProjectile_prefab);
            AssetAsyncReferenceManager<GameObject>.LoadAsset(shardRef).Completed += (x) =>
            {
                shardProjectile = x.Result;
                shardProjectile.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = ModConfig.ShardHoming.Value;
                ProjectileDirectionalTargetFinder component7 = shardProjectile.GetComponent<ProjectileDirectionalTargetFinder>();
                component7.lookRange = ModConfig.ShardRange.Value;
                component7.lookCone = ModConfig.ShardCone.Value;
                component7.allowTargetLoss = true;
            };

            AssetReferenceT<SpawnCard> crystalCardRef = new AssetReferenceT<SpawnCard>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_WeeklyRun.bscTimeCrystal_asset);
            AssetAsyncReferenceManager<SpawnCard>.LoadAsset(crystalCardRef).Completed += (x) => timeCrystalCard = x.Result;
            AssetReferenceT<SpawnCard> mithrixCardRef = new AssetReferenceT<SpawnCard>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.cscBrother_asset);
            AssetAsyncReferenceManager<SpawnCard>.LoadAsset(mithrixCardRef).Completed += (x) => mithrixCard = x.Result;
            AssetReferenceT<SpawnCard> mithrixHurtCardRef = new AssetReferenceT<SpawnCard>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.cscBrotherHurt_asset);
            AssetAsyncReferenceManager<SpawnCard>.LoadAsset(mithrixHurtCardRef).Completed += (x) => mithrixHurtCard = x.Result;
            AssetReferenceT<SpawnCard> glassCardRef = new AssetReferenceT<SpawnCard>(RoR2BepInExPack.GameAssetPaths.RoR2_Junk_BrotherGlass.cscBrotherGlass_asset);
            AssetAsyncReferenceManager<SpawnCard>.LoadAsset(glassCardRef).Completed += (x) => mithrixGlassCard = x.Result;

            AssetReferenceT<Material> bossMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.matBrotherPreBossSphere_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(bossMatRef).Completed += (x) => preBossMat = x.Result;
            AssetReferenceT<Material> arenaWallMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_moon.matMoonArenaWall_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(arenaWallMatRef).Completed += (x) => arenaWallMat = x.Result;
            AssetReferenceT<Material> stealAuraMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.matBrotherStealAura_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(stealAuraMatRef).Completed += (x) => stealAuraMat = x.Result;
            AssetReferenceT<Material> moonBridgeMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_moon.matMoonBridge_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(moonBridgeMatRef).Completed += (x) => moonMat = x.Result;
            AssetReferenceT<Material> doppelMatRef = new AssetReferenceT<Material>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_InvadingDoppelganger.matDoppelganger_mat);
            AssetAsyncReferenceManager<Material>.LoadAsset(doppelMatRef).Completed += (x) => doppelMat = x.Result;
            AssetReferenceT<NetworkSoundEventDef> slamSoundRef = new AssetReferenceT<NetworkSoundEventDef>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Croco.nseAcridBiteHit_asset);
            AssetAsyncReferenceManager<NetworkSoundEventDef>.LoadAsset(slamSoundRef).Completed += (x) => umbralSlamHitSound = x.Result;

            AssetReferenceT<SkillDef> fireShardSkillRef = new AssetReferenceT<SkillDef>(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Brother.FireLunarShards_asset);
            AssetAsyncReferenceManager<SkillDef>.LoadAsset(fireShardSkillRef).Completed += (x) =>
            {
                SkillDef fireLunarShardsDef = x.Result;
                fireLunarShardsDef.baseMaxStock = 6;
            };
        }
    }
}