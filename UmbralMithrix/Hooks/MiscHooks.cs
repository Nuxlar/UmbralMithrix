using EntityStates;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using EntityStates.Destructible;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace UmbralMithrix
{
    public class MiscHooks
    {
        public MiscHooks()
        {
            IL.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += TargetOnlyPlayers;
            IL.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += AddUmbralParticles;
            IL.RoR2.CharacterModel.UpdateOverlays += AddUmbralOverlay;
            IL.RoR2.CharacterModel.UpdateOverlays += AddUmbralOverlayCrystal;
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += ChangeP3CloneTargeting;
            On.RoR2.CharacterMaster.OnBodyDeath += CharacterMaster_OnBodyDeath;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.CombatDirector.OnEnable += CombatDirector_OnEnable;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            SceneCatalog.onMostRecentSceneDefChanged += onMostRecentSceneDefChanged;
            Run.onRunStartGlobal += onRunStartGlobal;
            On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
            On.EntityStates.FrozenState.OnEnter += FrozenState_OnEnter;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += AddTimedBuff_BuffDef_float;
            On.EntityStates.Destructible.TimeCrystalDeath.OnEnter += RemoveUmbralImmune;
            On.RoR2.ItemStealController.BrotherItemFilter += ItemStealController_BrotherItemFilter;
            IL.RoR2.TetherVfxOrigin.AddTether += TetherVfxOrigin_AddTether;
        }

        static void TetherVfxOrigin_AddTether(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdfld<TetherVfxOrigin>(nameof(TetherVfxOrigin.onTetherAdded))))
            {
                Log.Error("Failed to find onTetherAdded field load");
                return;
            }

            c.Emit(OpCodes.Dup);

            ILLabel skipInvokeLabel = c.DefineLabel();
            c.Emit(OpCodes.Brfalse, skipInvokeLabel);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<TetherVfxOrigin.TetherAddDelegate>("Invoke")))
            {
                Log.Error("Failed to find onTetherAdded Invoke call");
                return;
            }

            ILLabel skipPopLabel = c.DefineLabel();
            c.Emit(OpCodes.Br, skipPopLabel);

            c.Emit(OpCodes.Pop);
            skipInvokeLabel.Target = c.Prev;

            c.MarkLabel(skipPopLabel);
        }

        private bool ItemStealController_BrotherItemFilter(On.RoR2.ItemStealController.orig_BrotherItemFilter orig, ItemIndex itemIndex)
        {
            return false;
        }

        private void RemoveUmbralImmune(On.EntityStates.Destructible.TimeCrystalDeath.orig_OnEnter orig, TimeCrystalDeath self)
        {
            if (PhaseCounter.instance && (PhaseCounter.instance.phase == 2 || PhaseCounter.instance.phase == 3))
            {
                TimeCrystalDeath.explosionDamageCoefficient = 0f;
                TimeCrystalDeath.explosionForce = 0f;
                GameObject gameObject = PhaseCounter.instance.phase == 2 ? GameObject.Find("BrotherBody(Clone)") : GameObject.Find("BrotherHurtBodyP3(Clone)");
                if ((bool)gameObject && gameObject.GetComponent<CharacterBody>().HasBuff(RoR2Content.Buffs.Immune) && UmbralMithrix.timeCrystals.Count == 1)
                {
                    UmbralMithrix.timeCrystals.RemoveAt(0);
                    gameObject.GetComponent<CharacterBody>().RemoveBuff(RoR2Content.Buffs.Immune);
                }
                else if (UmbralMithrix.timeCrystals.Count > 0)
                {
                    UmbralMithrix.timeCrystals.RemoveAt(0);
                }
            }
            else
            {
                TimeCrystalDeath.explosionForce = 4000f;
                TimeCrystalDeath.explosionDamageCoefficient = 2f;
            }

            orig(self);
        }

        private void TargetOnlyPlayers(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<BullseyeSearch>(nameof(BullseyeSearch.GetResults))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((IEnumerable<HurtBox> results, BaseAI instance) =>
                {
                    if (instance && (instance.body.name == "BrotherBody(Clone)" || instance.body.name == "BrotherGlassBody(Clone)" || instance.body.name == "BrotherHurtBody(Clone)"))
                    {
                        // Filter results to only target players (don't target player allies like drones)
                        IEnumerable<HurtBox> playerControlledTargets = results.Where(hurtBox =>
                        {
                            GameObject entityObject = HurtBox.FindEntityObject(hurtBox);
                            return entityObject && entityObject.TryGetComponent(out CharacterBody characterBody) && characterBody.isPlayerControlled;
                        });

                        // If there are no players, use the default target so that the AI doesn't end up doing nothing
                        return playerControlledTargets.Any() ? playerControlledTargets : results;
                    }
                    else
                    {
                        return results;
                    }
                });
            }
        }

        private HurtBox ChangeP3CloneTargeting(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
        {
            if (self && (self.body.name == "BrotherBody(Clone)" || self.body.name == "BrotherGlassBody(Clone)" || self.body.name == "BrotherHurtBody(Clone)") && PhaseCounter.instance && PhaseCounter.instance.phase == 3)
            {
                maxDistance = float.PositiveInfinity;
                filterByLoS = false;
                full360Vision = true;
            }

            return orig(self, maxDistance, full360Vision, filterByLoS);
        }

        private void AddUmbralParticles(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                 x => x.MatchLdsfld(typeof(RoR2Content.Items), "InvadingDoppelganger")
                );
            c.Index += 2;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<int, CharacterBody, int>>((vengeanceCount, self) =>
            {
                if (self.name.Contains("Brother") && self.inventory && self.inventory.GetItemCount(UmbralMithrix.UmbralItem) > 0 && ModConfig.purpleMithrix.Value)
                    vengeanceCount++;
                return vengeanceCount;
            });
        }

        private void AddUmbralOverlay(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                 x => x.MatchLdsfld(typeof(RoR2Content.Items), "InvadingDoppelganger")
                );
            c.Index += 2;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<int, CharacterModel, int>>((vengeanceCount, self) =>
            {
                if (self.body.name.Contains("Brother") && self.body.inventory && self.body.inventory.GetItemCount(UmbralMithrix.UmbralItem) > 0 && ModConfig.purpleMithrix.Value)
                {
                    vengeanceCount++;
                }

                return vengeanceCount;
            });
        }

        private void AddUmbralOverlayCrystal(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(x => x.MatchLdsfld(typeof(RoR2Content.Items), "InvadingDoppelganger"));

            c.Index -= 1;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<bool, CharacterModel, bool>>((hasInventory, self) =>
            {
                if (self.body.name.Contains("Crystal") && self.body.gameObject.GetComponent<ArbitraryCrystalComponent>())
                    return true;

                return hasInventory;
            });
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            CharacterBody body = self.body;
            HealthComponent hc = self;
            if (PhaseCounter.instance)
            {
                bool flag = true;
                if (body && hc && body.name == "BrotherBody(Clone)" && PhaseCounter.instance.phase == 2 && !UmbralMithrix.p2ThresholdReached)
                {
                    if (hc.health - damageInfo.damage <= hc.fullHealth * 0.75f)
                    {
                        UmbralMithrix.p2ThresholdReached = true;
                        P2ThresholdEvent(body.gameObject);
                        hc.health = hc.fullHealth * 0.75f;
                        GameObject.Find("BrotherBody(Clone)").GetComponent<CharacterBody>().AddBuff(RoR2Content.Buffs.Immune);
                        flag = false;
                    }
                }
                if (body && hc && body.name == "BrotherHurtBodyP3(Clone)" && PhaseCounter.instance.phase == 3 && !UmbralMithrix.p3ThresholdReached)
                {
                    if (hc.health - damageInfo.damage <= hc.fullHealth * 0.75f)
                    {
                        UmbralMithrix.p3ThresholdReached = true;
                        GameObject.Find("BrotherBody(Clone)").GetComponent<HealthComponent>().health = 1f;
                        P3ThresholdEvent(body.gameObject);
                        hc.health = hc.fullHealth * 0.75f;
                        GameObject.Find("BrotherHurtBodyP3(Clone)").GetComponent<CharacterBody>().AddBuff(RoR2Content.Buffs.Immune);
                        flag = false;
                    }
                }
                if (flag)
                    orig(self, damageInfo);
            }
            else
                orig(self, damageInfo);
        }

        private void CharacterMaster_OnBodyStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);

            if (!PhaseCounter.instance)
                return;

            if (body.name == "BrotherHurtBodyP3(Clone)")
            {
                if (ModConfig.purpleMithrix.Value)
                {
                    self.inventory.GiveItem(UmbralMithrix.UmbralItem);
                }

                self.inventory.GiveItem(RoR2Content.Items.AdaptiveArmor);
            }

            if ((body.name == "BrotherBody(Clone)") && ModConfig.purpleMithrix.Value)
            {
                self.inventory.GiveItem(UmbralMithrix.UmbralItem);
            }

            if (body.name == "BrotherBody(Clone)")
            {
                if (PhaseCounter.instance.phase == 1)
                {
                    ChildLocator component = SceneInfo.instance.GetComponent<ChildLocator>();
                    if (component)
                    {
                        Transform child = component.FindChild("CenterOfArena");
                        if (child)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                    }
                }
                if (PhaseCounter.instance.phase != 3)
                {
                    body.gameObject.AddComponent<CloneController>();
                }
            }

            if (body.name == "BrotherHurtBody(Clone)" && PhaseCounter.instance.phase == 4)
            {
                if (ModConfig.skipPhase4.Value)
                {
                    body.healthComponent.Suicide();
                }
                else
                {
                    body.levelMoveSpeed = 0;
                    body.baseMoveSpeed = 0;
                    body.inventory.GiveItem(UmbralMithrix.UmbralItem);
                    body.AddBuff(RoR2Content.Buffs.Immune);
                    body.inventory.GiveItem(RoR2Content.Items.HealthDecay, 40);
                    body.GetComponent<SkillLocator>().primary = new GenericSkill();
                    body.GetComponent<SkillLocator>().secondary = new GenericSkill();
                }
            }
        }

        private void CharacterMaster_OnBodyDeath(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self, CharacterBody body)
        {
            orig(self, body);

            if (!NetworkServer.active)
                return;

            if (!body.isPlayerControlled)
                return;

            if (UmbralMithrix.practiceModeEnabled && !self.IsExtraLifePendingServer() && PhaseCounter.instance)
            {
                self.RespawnExtraLife();
            }
        }

        private void CombatDirector_OnEnable(On.RoR2.CombatDirector.orig_OnEnable orig, CombatDirector self)
        {
            if (PhaseCounter.instance && (PhaseCounter.instance.phase == 2 || PhaseCounter.instance.phase == 3))
            {
                self.gameObject.SetActive(false);
            }
            else
            {
                orig(self);
            }
        }

        private void P2ThresholdEvent(GameObject summoner)
        {
            UmbralMithrix.timeCrystals.Clear();
            int num = 4;
            for (int key = 0; key < num; ++key)
            {
                DirectorPlacementRule placementRule = new DirectorPlacementRule();
                placementRule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
                placementRule.minDistance = 3f;
                placementRule.maxDistance = 10f;
                placementRule.position = UmbralMithrix.p23PizzaPoints[key];
                Xoroshiro128Plus rng = RoR2Application.rng;
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(UmbralMithrix.timeCrystalCard, placementRule, rng)
                {
                    summonerBodyObject = summoner,
                    onSpawnedServer = spawnResult =>
                    {
                        spawnResult.spawnedInstance.AddComponent<ArbitraryCrystalComponent>();
                        spawnResult.spawnedInstance.GetComponent<TeamComponent>().teamIndex = TeamIndex.Monster;
                        TetherVfxOrigin tetherVfxOrigin = spawnResult.spawnedInstance.gameObject.AddComponent<TetherVfxOrigin>();
                        tetherVfxOrigin.tetherPrefab = UmbralMithrix.tether;
                        tetherVfxOrigin.AddTether(summoner.transform);
                    }
                });
                UmbralMithrix.timeCrystals.Add(summoner);
            }
        }

        private void P3ThresholdEvent(GameObject summoner)
        {
            UmbralMithrix.timeCrystals.Clear();
            int num = 4;
            for (int key = 0; key < num; ++key)
            {
                DirectorPlacementRule placementRule = new DirectorPlacementRule();
                placementRule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
                placementRule.minDistance = 3f;
                placementRule.maxDistance = 10f;
                placementRule.position = UmbralMithrix.p23PizzaPoints[key];
                Xoroshiro128Plus rng = RoR2Application.rng;
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(UmbralMithrix.timeCrystalCard, placementRule, rng)
                {
                    summonerBodyObject = summoner,
                    onSpawnedServer = spawnResult =>
                    {
                        spawnResult.spawnedInstance.AddComponent<ArbitraryCrystalComponent>();
                        spawnResult.spawnedInstance.GetComponent<TeamComponent>().teamIndex = TeamIndex.Monster;
                        TetherVfxOrigin tetherVfxOrigin = spawnResult.spawnedInstance.gameObject.AddComponent<TetherVfxOrigin>();
                        tetherVfxOrigin.tetherPrefab = UmbralMithrix.tether;
                        tetherVfxOrigin.AddTether(summoner.transform);
                    }
                });
                UmbralMithrix.timeCrystals.Add(summoner);
            }
        }

        static void onRunStartGlobal(Run run)
        {
            UmbralMithrix.practiceModeEnabled = false;
        }

        static void onMostRecentSceneDefChanged(SceneDef sceneDef)
        {
            if (sceneDef.cachedName == "moon2")
            {
                UmbralMithrix.ArenaSetup();
                UmbralMithrix.SpawnPracticeModeShrine();
            }
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (self.name == "PracticeModeShrine")
                UmbralMithrix.practiceModeEnabled = true;

            orig(self, activator);
        }

        private void FrozenState_OnEnter(On.EntityStates.FrozenState.orig_OnEnter orig, FrozenState self)
        {
            if (self.characterBody.name == "BrotherBody(Clone)")
            {
                float chance = UnityEngine.Random.value;
                if (chance > 0.5f)
                {
                    Ray aimRay = self.GetAimRay();
                    for (int index = 0; index < 6; ++index)
                    {
                        Util.PlaySound(FireLunarShards.fireSound, self.gameObject);
                        ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), self.gameObject, (float)((double)self.characterBody.damage * 0.100000001490116 / 12.0), 0.0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master));
                    }
                }
            }

            orig(self);
        }

        private void AddTimedBuff_BuffDef_float(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            if (self.name == "BrotherBody(Clone)" && buffDef == RoR2Content.Buffs.Nullified)
            {
                float chance = UnityEngine.Random.value;
                if (chance < 0.25f)
                {
                    duration /= 2f;
                    Ray ray = (bool)self.inputBank ? new Ray(self.inputBank.aimOrigin, self.inputBank.aimDirection) : new Ray(self.transform.position, self.transform.forward);
                    for (int index = 0; index < 6; ++index)
                    {
                        int num = (int)Util.PlaySound(FireLunarShards.fireSound, self.gameObject);
                        ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, ray.origin, Quaternion.LookRotation(ray.direction), self.gameObject, (float)((double)self.damage * 0.100000001490116 / 12.0), 0.0f, Util.CheckRoll(self.crit, self.master));
                    }
                }
            }

            orig(self, buffDef, duration);
        }

        private void SetPosition(Vector3 newPosition, CharacterBody body)
        {
            if (!body.characterMotor)
                return;

            body.characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity);
        }
    }
}
