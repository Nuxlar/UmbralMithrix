using EntityStates;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using EntityStates.Destructible;
using HG;
using RoR2;
using RoR2.Networking;
using RoR2.Projectile;
using System;
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
      On.RoR2.CharacterMaster.OnBodyDeath += CharacterMaster_OnBodyDeath;
      On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
      On.RoR2.CombatDirector.OnEnable += CombatDirector_OnEnable;
      On.EntityStates.Destructible.TimeCrystalDeath.Explode += TimeCrystalDeath_Explode;
      On.RoR2.BasicPickupDropTable.GenerateWeightedSelection += BasicPickupDropTable_GenerateWeightedSelection;
      On.RoR2.PickupTransmutationManager.RebuildPickupGroups += PickupTransmutationManager_RebuildPickupGroups;
      // On.EntityStates.EntityState.Update += EntityState_Update;
      On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
      On.RoR2.Stage.Start += Stage_Start;
      On.RoR2.Run.Start += Run_Start;
      On.RoR2.CharacterMaster.OnBodyStart += CharacterMaster_OnBodyStart;
      On.EntityStates.FrozenState.OnEnter += FrozenState_OnEnter;
      On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += AddTimedBuff_BuffDef_float;
      On.RoR2.ItemStealController.BrotherItemFilter += ItemStealController_BrotherItemFilter;
    }

    private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
    {
      if (self.body && self.body.name == "BrotherBody(Clone)" && PhaseCounter.instance.phase == 2 && !UmbralMithrix.p2ThresholdReached)
      {
        if (self.health - damageInfo.damage <= self.fullHealth * 0.75f)
        {
          UmbralMithrix.p2ThresholdReached = true;
          this.P2ThresholdEvent(self.body.gameObject);
          self.body.AddBuff(RoR2Content.Buffs.Immune);
        }
      }
      if (self.body && self.body.name == "BrotherHurtBodyP3(Clone)" && PhaseCounter.instance.phase == 3 && !UmbralMithrix.p3ThresholdReached)
      {
        if (self.health - damageInfo.damage <= self.fullHealth * 0.75f)
        {
          UmbralMithrix.p3ThresholdReached = true;
          GameObject.Find("BrotherBody(Clone)").GetComponent<HealthComponent>().health = 1f;
          this.P3ThresholdEvent(self.body.gameObject);
          self.health = self.fullHealth * 0.75f;
          damageInfo.rejected = true;
          self.body.AddBuff(RoR2Content.Buffs.Immune);
        }
      }
      orig(self, damageInfo);
    }

    private void CharacterMaster_OnBodyStart(
      On.RoR2.CharacterMaster.orig_OnBodyStart orig,
      CharacterMaster self,
      CharacterBody body)
    {
      orig(self, body);
      GameObject bonfireContainer = GameObject.Find("BonfireContainer");
      if (bonfireContainer)
      {
        BonfireController bonfireController = bonfireContainer.GetComponent<BonfireController>();
        if (body.isPlayerControlled && SceneManager.GetActiveScene().name == "moon2" && body.HasBuff(RoR2Content.Buffs.Immune))
          return;
        this.SetPosition(new Vector3(UnityEngine.Random.Range(100, 127), 501f, 101f), body);
        if (UmbralMithrix.bonfireInventory.ContainsKey(self) && UmbralMithrix.bonfireGold.ContainsKey(self))
        {
          self.money = UmbralMithrix.bonfireGold[self];
          body.inventory.itemAcquisitionOrder.Clear();
          int[] itemStacks = body.inventory.itemStacks;
          int num = 0;
          ref int local = ref num;
          ArrayUtils.SetAll<int>(itemStacks, in local);
          body.inventory.AddItemsFrom(UmbralMithrix.bonfireInventory[self], (Func<ItemIndex, bool>)(_ => true));
          body.inventory.CopyEquipmentFrom(UmbralMithrix.bonfireInventory[self]);
          Dictionary<BuffIndex, int> dictionary;
          if (UmbralMithrix.persistentBuffs.TryGetValue(body.master, out dictionary))
          {
            foreach (KeyValuePair<BuffIndex, int> keyValuePair in dictionary)
              body.SetBuffCount(keyValuePair.Key, keyValuePair.Value);
          }
          if (!bonfireController.spawnedAllies)
          {
            foreach (EquipmentIndex droneEquip in UmbralMithrix.droneEquips)
              new MasterSummon()
              {
                masterPrefab = UmbralMithrix.droneMasters["EquipmentDroneBody(Clone)"],
                position = new Vector3(1079.6f, -283f, 1155f),
                rotation = Quaternion.identity,
                summonerBodyObject = body.gameObject,
                ignoreTeamMemberLimit = true,
                useAmbientLevel = new bool?(true)
              }.Perform().inventory.SetEquipmentIndexForSlot(droneEquip, 0U);
            foreach (string bonfireAlly in UmbralMithrix.bonfireAllies)
            {
              if (UmbralMithrix.droneMasters.ContainsKey(bonfireAlly) && !bonfireAlly.Contains("EquipmentDrone"))
                new MasterSummon()
                {
                  masterPrefab = UmbralMithrix.droneMasters[bonfireAlly],
                  position = new Vector3(1079.6f, -283f, 1155f),
                  rotation = Quaternion.identity,
                  summonerBodyObject = PlayerCharacterMasterController.instances[0].body.gameObject,
                  ignoreTeamMemberLimit = true,
                  useAmbientLevel = new bool?(true)
                }.Perform();
            }
            bonfireController.spawnedAllies = true;
          }
        }
      }
      if (!(bool)PhaseCounter.instance)
        return;
      if (body.name == "BrotherHurtBodyP3(Clone)")
      {
        self.inventory.GiveItemString(UmbralMithrix.UmbralItem.name);
        self.inventory.GiveItemString(RoR2Content.Items.AdaptiveArmor.name);
        this.KillAllDrones();
        EffectManager.SpawnEffect(UmbralMithrix.implodeEffect, new EffectData()
        {
          origin = body.corePosition,
          scale = body.radius + 100f
        }, false);
      }
      if (body.name == "BrotherBody(Clone)" || body.name == "BrotherGlassBody(Clone)")
        self.inventory.GiveItemString(UmbralMithrix.UmbralItem.name);
      if (body.name == "BrotherBody(Clone)" && PhaseCounter.instance.phase == 1)
        self.gameObject.AddComponent<CloneController>();
      if (!(body.name == "BrotherHurtBody(Clone)") || PhaseCounter.instance.phase != 4)
        return;
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
      if (UmbralMithrix.practiceModeEnabled && !self.IsExtraLifePendingServer())
        self.RespawnExtraLife();
      GameObject bonfireContainer = GameObject.Find("BonfireContainer");
      if (bonfireContainer)
      {
        bonfireContainer.GetComponent<BonfireController>().spawnedAllies = false;
        bool flag = true;
        if (this.CountLivingPlayers() != 0 || self.IsExtraLifePendingServer())
          flag = false;
        if (!flag || !UmbralMithrix.bonfireInventory.ContainsKey(self) && UmbralMithrix.bonfireAllies.Count <= 0)
          return;
        Run.instance.SetRunStopwatch(UmbralMithrix.bonfireRunTime);
        TeamManager.instance.SetTeamLevel(TeamIndex.Monster, 1U);
        this.KillAllDrones();
        self.preventGameOver = true;
        this.SetScene("moon2");
      }
    }

    private void CombatDirector_OnEnable(On.RoR2.CombatDirector.orig_OnEnable orig, CombatDirector self)
    {
      if ((bool)PhaseCounter.instance && (PhaseCounter.instance.phase == 2 || PhaseCounter.instance.phase == 3))
        self.gameObject.SetActive(false);
      else
        orig(self);
    }

    private void TimeCrystalDeath_Explode(On.EntityStates.Destructible.TimeCrystalDeath.orig_Explode orig, TimeCrystalDeath self)
    {
      if ((bool)PhaseCounter.instance && PhaseCounter.instance.phase == 3)
        self.damageStat = 0;
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
        DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(UmbralMithrix.timeCrystalCard, placementRule, rng)
        {
          summonerBodyObject = summoner,
          onSpawnedServer = (Action<SpawnCard.SpawnResult>)(spawnResult =>
          {
            TetherVfxOrigin tetherVfxOrigin = spawnResult.spawnedInstance.gameObject.AddComponent<TetherVfxOrigin>();
            tetherVfxOrigin.tetherPrefab = UmbralMithrix.tether;
            tetherVfxOrigin.AddTether(summoner.transform);
            spawnResult.spawnedInstance.GetComponent<TeamComponent>().teamIndex = TeamIndex.Monster;
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
      UmbralMithrix.bonfireRunTime = 0.0f;
      UmbralMithrix.practiceModeEnabled = false;
      UmbralMithrix.bonfireModeEnabled = false;
      UmbralMithrix.spawnedAllies = false;
      UmbralMithrix.persistentBuffs.Clear();
      UmbralMithrix.bonfireInventory.Clear();
      UmbralMithrix.bonfireGold.Clear();
      UmbralMithrix.bonfireAllies.Clear();
      UmbralMithrix.droneEquips.Clear();
      orig(self);
    }

    private void Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
    {
      orig(self);
      if (!(self.sceneDef.cachedName == "moon2"))
        return;
      if (NetworkServer.active && UmbralMithrix.bonfireModeEnabled)
      {
        GameObject bonfireContainer = new("BonfireContainer");
        bonfireContainer.AddComponent<BonfireController>();
        NetworkServer.Spawn(bonfireContainer);
      }
      UmbralMithrix.ArenaSetup();
      UmbralMithrix.SpawnPracticeModeShrine();
      UmbralMithrix.SpawnBonfireModeShrine();
      UmbralMithrix.mithrix.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(typeof(ThroneSpawnState));
    }

    private void PurchaseInteraction_OnInteractionBegin(
      On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig,
      PurchaseInteraction self,
      Interactor activator)
    {
      if (self.name == "PracticeModeShrine")
        UmbralMithrix.practiceModeEnabled = true;
      if (self.name == "BonfireModeShrine")
      {
        UmbralMithrix.bonfireModeEnabled = true;
        if (NetworkServer.active)
        {
          GameObject bonfireContainer = new("BonfireContainer");
          bonfireContainer.AddComponent<BonfireController>();
          NetworkServer.Spawn(bonfireContainer);
        }
      }
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

    private void EntityState_Update(On.EntityStates.EntityState.orig_Update orig, EntityState self)
    {
      orig(self);
      if (!(bool)self.characterBody || !(bool)PhaseCounter.instance)
        return;

      if (self.characterBody.name == "InactiveVoidling(Clone)" && PhaseCounter.instance.phase == 4)
      {
        UmbralMithrix.elapsedStorm += Time.deltaTime;
        if (UmbralMithrix.elapsedStorm >= 1.0 && (double)self.gameObject.GetComponent<SphereZone>().Networkradius > 100.0)
        {
          UmbralMithrix.elapsedStorm %= 1f;
          self.gameObject.GetComponent<SphereZone>().Networkradius -= 6f;
        }
      }
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
          if ((bool)(UnityEngine.Object)body && (bool)(UnityEngine.Object)body.healthComponent && (body.name.Contains("Drone") || body.name.Contains("Turret")))
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

    private int CountLivingPlayers()
    {
      int num = 0;
      ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Player);
      for (int index = 0; index < teamMembers.Count; ++index)
      {
        CharacterBody body = teamMembers[index].body;
        if (body.isPlayerControlled && (bool)(UnityEngine.Object)body.healthComponent && body.healthComponent.alive)
          ++num;
      }
      return num;
    }

    private void SetScene(string sceneName)
    {
      if (!(bool)(UnityEngine.Object)NetworkManagerSystem.singleton)
        throw new ConCommandException("set_scene failed: NetworkManagerSystem is not available.");
      SceneCatalog.GetSceneDefForCurrentScene();
      SceneDef defFromSceneName = SceneCatalog.GetSceneDefFromSceneName(sceneName);
      if (!(bool)(UnityEngine.Object)defFromSceneName)
        throw new ConCommandException("\"" + sceneName + "\" is not a valid scene.");
      if ((bool)(UnityEngine.Object)NetworkManager.singleton)
      {
        int num = NetworkManager.singleton.isNetworkActive ? 1 : 0;
      }
      if (NetworkManager.singleton.isNetworkActive)
      {
        if (defFromSceneName.isOfflineScene)
          throw new ConCommandException("Cannot switch to scene \"" + sceneName + "\": Cannot switch to offline-only scene while in a network session.");
      }
      else if (!defFromSceneName.isOfflineScene)
        throw new ConCommandException("Cannot switch to scene \"" + sceneName + "\": Cannot switch to online-only scene while not in a network session.");
      if (NetworkServer.active)
      {
        Debug.LogFormat("Setting server scene to {0}", (object)sceneName);
        NetworkManagerSystem.singleton.ServerChangeScene(sceneName);
      }
      else
      {
        if (NetworkClient.active)
          throw new ConCommandException("Cannot change scene while connected to a remote server.");
        Debug.LogFormat("Setting offline scene to {0}", (object)sceneName);
        NetworkManagerSystem.singleton.ServerChangeScene(sceneName);
      }
    }
  }
}
