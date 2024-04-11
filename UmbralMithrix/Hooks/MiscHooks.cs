using EntityStates;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using EntityStates.Destructible;
using HG;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Networking;
using RoR2.Projectile;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace UmbralMithrix
{
  public class MiscHooks
  {
    public MiscHooks()
    {
      IL.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += TargetOnlyPlayers;
      IL.RoR2.CharacterBody.UpdateAllTemporaryVisualEffects += AddUmbralParticles;
      IL.RoR2.CharacterModel.UpdateOverlays += AddUmbralOverlay;
      On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += ChangeP3CloneTargeting;
      On.RoR2.CharacterMaster.OnBodyDeath += CharacterMaster_OnBodyDeath;
      On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
      On.RoR2.CombatDirector.OnEnable += CombatDirector_OnEnable;
      On.RoR2.BasicPickupDropTable.GenerateWeightedSelection += BasicPickupDropTable_GenerateWeightedSelection;
      On.RoR2.PickupTransmutationManager.RebuildPickupGroups += PickupTransmutationManager_RebuildPickupGroups;
      On.RoR2.HealthComponent.SendDamageDealt += HealthComponent_SendDamageDealt;
      On.RoR2.Stage.Start += Stage_Start;
      On.RoR2.Run.Start += Run_Start;
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
      On.EntityStates.FrozenState.OnEnter += FrozenState_OnEnter;
      On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += AddTimedBuff_BuffDef_float;
      On.RoR2.ItemStealController.BrotherItemFilter += ItemStealController_BrotherItemFilter;
    }

    private void TargetOnlyPlayers(ILContext il)
    {
      ILCursor c = new ILCursor(il);

      if (c.TryGotoNext(MoveType.After, x => x.MatchCallOrCallvirt<BullseyeSearch>(nameof(BullseyeSearch.GetResults))))
      {
        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate((IEnumerable<HurtBox> results, BaseAI instance) =>
        {
          if (instance && instance.body.name == "BrotherBody(Clone)" && PhaseCounter.instance && PhaseCounter.instance.phase == 3)
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
            return results;
        });
      }
    }

    private HurtBox ChangeP3CloneTargeting(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
    {
      if (self && self.body.name == "BrotherBody(Clone)" && PhaseCounter.instance && PhaseCounter.instance.phase == 3)
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
        if (self.name.Contains("Brother") && self.inventory && self.inventory.GetItemCount(UmbralMithrix.UmbralItem) > 0)
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
        if (self.body.name.Contains("Brother") && self.body.inventory && self.body.inventory.GetItemCount(UmbralMithrix.UmbralItem) > 0)
          vengeanceCount++;
        return vengeanceCount;
      });
    }

    private void HealthComponent_SendDamageDealt(On.RoR2.HealthComponent.orig_SendDamageDealt orig, DamageReport damageReport)
    {
      CharacterBody body = damageReport.victimBody;
      HealthComponent hc = damageReport.victimBody.healthComponent;
      if (body && hc && body.name == "BrotherBody(Clone)" && PhaseCounter.instance.phase == 2 && !UmbralMithrix.p2ThresholdReached)
      {
        if (hc.health - damageReport.damageDealt <= hc.fullHealth * 0.75f)
        {
          UmbralMithrix.p2ThresholdReached = true;
          this.P2ThresholdEvent(body.gameObject);
          hc.health = hc.fullHealth * 0.75f;
          damageReport.damageInfo.rejected = true;
          GameObject.Find("BrotherBody(Clone)").GetComponent<CharacterBody>().AddBuff(RoR2Content.Buffs.Immune);
        }
      }
      if (body && hc && body.name == "BrotherHurtBodyP3(Clone)" && PhaseCounter.instance.phase == 3 && !UmbralMithrix.p3ThresholdReached)
      {
        if (hc.health - damageReport.damageDealt <= hc.fullHealth * 0.75f)
        {
          UmbralMithrix.p3ThresholdReached = true;
          GameObject.Find("BrotherBody(Clone)").GetComponent<HealthComponent>().health = 1f;
          this.P3ThresholdEvent(body.gameObject);
          hc.health = hc.fullHealth * 0.75f;
          damageReport.damageInfo.rejected = true;
          GameObject.Find("BrotherHurtBodyP3(Clone)").GetComponent<CharacterBody>().AddBuff(RoR2Content.Buffs.Immune);
        }
      }
      orig(damageReport);
    }

    private void CharacterMaster_OnBodyStart(
      On.RoR2.CharacterMaster.orig_OnBodyStart orig,
      CharacterMaster self,
      CharacterBody body)
    {
      orig(self, body);
      if (!(bool)PhaseCounter.instance)
        return;
      if (body.name == "BrotherHurtBodyP3(Clone)")
      {
        self.inventory.GiveItemString(UmbralMithrix.UmbralItem.name);
        self.inventory.GiveItemString(RoR2Content.Items.AdaptiveArmor.name);
      }
      if (body.name == "BrotherBody(Clone)" || body.name == "BrotherGlassBody(Clone)")
        self.inventory.GiveItemString(UmbralMithrix.UmbralItem.name);
      if (body.name == "BrotherBody(Clone)" && PhaseCounter.instance.phase == 1)
      {
        body.gameObject.AddComponent<CloneController>();
        ChildLocator component = SceneInfo.instance.GetComponent<ChildLocator>();
        if (!(bool)component)
          return;
        Transform child = component.FindChild("CenterOfArena");
        if ((bool)child)
          GameObject.Destroy(child.gameObject);
      }
      if (body.name == "BrotherGlassBody(Clone)" && PhaseCounter.instance.phase == 3)
      {
        body.levelMoveSpeed = 0;
        body.baseMoveSpeed = 0;
        body.baseMaxHealth = 50f;
        body.levelMaxHealth = 15f;
        body.teamComponent.teamIndex = TeamIndex.Monster;
        SkillLocator skillLocator = body.skillLocator;
        skillLocator.primary = new GenericSkill();
        skillLocator.secondary = new GenericSkill();
        skillLocator.utility = new GenericSkill();
        skillLocator.special = new GenericSkill();
      }
      if (!(body.name == "BrotherHurtBody(Clone)") || PhaseCounter.instance.phase != 4)
        return;
      body.levelMoveSpeed = 0;
      body.baseMoveSpeed = 0;
      body.inventory.GiveItem(UmbralMithrix.UmbralItem);
      body.AddBuff(RoR2Content.Buffs.Immune);
      body.inventory.GiveItem(RoR2Content.Items.HealthDecay, 40);
    }

    private void CharacterMaster_OnBodyDeath(
      On.RoR2.CharacterMaster.orig_OnBodyDeath orig,
      CharacterMaster self,
      CharacterBody body)
    {
      orig(self, body);
      if (!NetworkServer.active || !body.isPlayerControlled)
        return;
      if (UmbralMithrix.practiceModeEnabled && !self.IsExtraLifePendingServer() && PhaseCounter.instance)
      {
        self.RespawnExtraLife();
        if (PhaseCounter.instance.phase == 4)
        {
          List<CharacterBody> bodies = new();
          foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
          {
            if (cm.teamIndex == TeamIndex.Player)
            {
              CharacterBody cb = cm.GetBody();
              if (cb && cb.isPlayerControlled)
                bodies.Add(cb);
            }
          }
          GameObject.Find("BrotherHurtBody(Clone)").GetComponent<P4Controller>().playerBodies = bodies;
        }
      }
    }

    private void CombatDirector_OnEnable(On.RoR2.CombatDirector.orig_OnEnable orig, CombatDirector self)
    {
      if ((bool)PhaseCounter.instance && (PhaseCounter.instance.phase == 2 || PhaseCounter.instance.phase == 3))
        self.gameObject.SetActive(false);
      else
        orig(self);
    }

    private void BasicPickupDropTable_GenerateWeightedSelection(
      On.RoR2.BasicPickupDropTable.orig_GenerateWeightedSelection orig,
      BasicPickupDropTable self,
      Run run)
    {
      List<PickupIndex> pickupIndexList1 = new List<PickupIndex>();
      List<PickupIndex> pickupIndexList2 = new List<PickupIndex>();
      foreach (PickupIndex lunarCombinedDrop in run.availableLunarCombinedDropList)
      {
        ItemDef itemDef = ItemCatalog.GetItemDef(lunarCombinedDrop.itemIndex);
        EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(lunarCombinedDrop.equipmentIndex);
        if ((bool)itemDef && !itemDef.name.Contains("Umbral"))
          pickupIndexList2.Add(lunarCombinedDrop);
        if ((bool)equipmentDef)
          pickupIndexList2.Add(lunarCombinedDrop);
      }
      foreach (PickupIndex availableLunarItemDrop in run.availableLunarItemDropList)
      {
        ItemDef itemDef = ItemCatalog.GetItemDef(availableLunarItemDrop.itemIndex);
        if ((bool)itemDef && !itemDef.name.Contains("Umbral"))
          pickupIndexList1.Add(availableLunarItemDrop);
      }
      run.availableLunarCombinedDropList = pickupIndexList2;
      run.availableLunarItemDropList = pickupIndexList1;
      orig(self, run);
    }

    private void PickupTransmutationManager_RebuildPickupGroups(
      On.RoR2.PickupTransmutationManager.orig_RebuildPickupGroups orig)
    {
      orig();
      List<PickupIndex> pickupIndexList = new List<PickupIndex>();
      foreach (PickupIndex pickupIndex in PickupTransmutationManager.itemTierLunarGroup)
      {
        ItemDef itemDef = ItemCatalog.GetItemDef(pickupIndex.itemIndex);
        EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(pickupIndex.equipmentIndex);
        if ((bool)itemDef && !itemDef.name.Contains("Umbral"))
          pickupIndexList.Add(pickupIndex);
        if ((bool)equipmentDef)
          pickupIndexList.Add(pickupIndex);
      }
      PickupTransmutationManager.itemTierLunarGroup = pickupIndexList.ToArray();
    }

    private void P2ThresholdEvent(GameObject summoner)
    {
      UmbralMithrix.timeCrystals.Clear();
      DirectorPlacementRule placementRule = new DirectorPlacementRule();
      placementRule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
      placementRule.minDistance = 3f;
      placementRule.maxDistance = 10f;
      placementRule.position = new Vector3(-88.5f, 491.5f, -0.3f);
      Xoroshiro128Plus rng = RoR2Application.rng;
      DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(UmbralMithrix.mithrixGlassCard, placementRule, rng)
      {
        summonerBodyObject = summoner,
        onSpawnedServer = (Action<SpawnCard.SpawnResult>)(spawnResult =>
        {
          TetherVfxOrigin tetherVfxOrigin = spawnResult.spawnedInstance.GetComponent<CharacterMaster>().GetBody().gameObject.AddComponent<TetherVfxOrigin>();
          tetherVfxOrigin.tetherPrefab = UmbralMithrix.tether;
          tetherVfxOrigin.AddTether(summoner.transform);
        })
      });
      UmbralMithrix.timeCrystals.Add(summoner);
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
        DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(UmbralMithrix.mithrixGlassCard, placementRule, rng)
        {
          summonerBodyObject = summoner,
          onSpawnedServer = (Action<SpawnCard.SpawnResult>)(spawnResult =>
          {
            TetherVfxOrigin tetherVfxOrigin = spawnResult.spawnedInstance.gameObject.AddComponent<TetherVfxOrigin>();
            tetherVfxOrigin.tetherPrefab = UmbralMithrix.tether;
            tetherVfxOrigin.AddTether(summoner.transform);
          })
        });
        UmbralMithrix.timeCrystals.Add(summoner);
      }
    }

    private bool ItemStealController_BrotherItemFilter(
      On.RoR2.ItemStealController.orig_BrotherItemFilter orig,
      ItemIndex itemIndex)
    {
      return false;
    }

    private void Run_Start(On.RoR2.Run.orig_Start orig, Run self)
    {
      UmbralMithrix.practiceModeEnabled = false;
      orig(self);
    }

    private void Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
    {
      orig(self);
      if (!(self.sceneDef.cachedName == "moon2"))
        return;
      UmbralMithrix.ArenaSetup();
      UmbralMithrix.SpawnPracticeModeShrine();
      UmbralMithrix.mithrix.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(ThroneSpawnState));
    }

    private void PurchaseInteraction_OnInteractionBegin(
      On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig,
      PurchaseInteraction self,
      Interactor activator)
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

    private void AddTimedBuff_BuffDef_float(
      On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig,
      CharacterBody self,
      BuffDef buffDef,
      float duration)
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
      if (!(bool)(UnityEngine.Object)body.characterMotor)
        return;
      body.characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity);
    }

    private void KillAllDrones()
    {
      int num = 0;
      TeamIndex teamIndex = TeamIndex.Player;
      foreach (CharacterMaster characterMaster in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
      {
        if (characterMaster.teamIndex == teamIndex)
        {
          CharacterBody body = characterMaster.GetBody();
          if ((bool)(UnityEngine.Object)body && (bool)(UnityEngine.Object)body.healthComponent && body.name.Contains("Drone"))
          {
            body.healthComponent.Suicide();
            ++num;
          }
        }
      }
      if (num <= 0)
        return;
      Debug.Log((object)string.Format("Killed {0} drones.", (object)num));
      Chat.SendBroadcastChat((ChatMessageBase)new Chat.SimpleChatMessage()
      {
        baseToken = "<color=#c6d5ff><size=120%>Mithrix: Enough of your toys.</color></size>"
      });
    }
  }
}
