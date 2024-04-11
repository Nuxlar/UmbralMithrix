using EntityStates;
using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using EntityStates.LunarWisp;
using RoR2;
using RoR2.Projectile;
using RoR2.CharacterAI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UmbralMithrix
{
  public class MithrixMiscHooks
  {
    public MithrixMiscHooks()
    {
      On.EntityStates.BrotherMonster.EnterSkyLeap.OnEnter += EnterSkyLeap_OnEnter;
      On.EntityStates.BrotherMonster.HoldSkyLeap.OnEnter += HoldSkyLeap_OnEnter;
      On.EntityStates.BrotherMonster.ExitSkyLeap.OnEnter += ExitSkyLeap_OnEnter;
      On.EntityStates.BrotherMonster.SprintBash.OnEnter += SprintBash_OnEnter;
      On.EntityStates.BrotherMonster.SprintBash.OnExit += SprintBash_OnExit;
      On.EntityStates.BrotherMonster.FistSlam.OnEnter += FistSlam_OnEnter;
      On.EntityStates.BrotherMonster.UltChannelState.FireWave += UltChannelState_FireWave;
      On.EntityStates.BrotherMonster.SkyLeapDeathState.OnEnter += SkyLeapDeathState_OnEnter;
      On.EntityStates.BrotherMonster.SpellChannelEnterState.OnEnter += SpellChannelEnterState_OnEnter;
      On.EntityStates.BrotherMonster.SpellChannelState.OnEnter += SpellChannelState_OnEnter;
      On.EntityStates.BrotherMonster.SpellChannelExitState.OnEnter += SpellChannelExitState_OnEnter;
      On.EntityStates.BrotherMonster.StaggerEnter.OnEnter += StaggerEnter_OnEnter;
      On.EntityStates.BrotherMonster.TrueDeathState.OnEnter += TrueDeathState_OnEnter;
    }

    private void EnterSkyLeap_OnEnter(On.EntityStates.BrotherMonster.EnterSkyLeap.orig_OnEnter orig, EnterSkyLeap self)
    {
      EnterSkyLeap.baseDuration = 0.25f;
      Util.PlaySound("Play_voidRaid_snipe_shoot_final", self.characterBody.gameObject);
      orig(self);
    }

    private void HoldSkyLeap_OnEnter(On.EntityStates.BrotherMonster.HoldSkyLeap.orig_OnEnter orig, HoldSkyLeap self)
    {
      HoldSkyLeap.duration = ModConfig.CrushingLeap.Value;
      if (self.isAuthority)
      {
        List<CharacterBody> playerBodies = new();
        foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
        {
          if (cm.teamIndex == TeamIndex.Player)
          {
            CharacterBody cb = cm.GetBody();
            if (cb && cb.isPlayerControlled)
              playerBodies.Add(cb);
          }
        }
        if (playerBodies.Count > 0)
        {
          Vector3 target = playerBodies[UnityEngine.Random.Range(0, playerBodies.Count)].footPosition;
          RaycastHit hitInfo;
          if (Physics.Raycast(new Ray(target, Vector3.down), out hitInfo, 200f, (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            self.characterMotor.Motor.SetPositionAndRotation(hitInfo.point + new Vector3(0, 10, 0), Quaternion.identity);
          else
            self.characterMotor.Motor.SetPositionAndRotation(target, Quaternion.identity);
        }
        GameObject workPls = GameObject.Instantiate(UmbralMithrix.leapIndicatorPrefab, self.characterBody.footPosition, Quaternion.identity);
        float radius = self.characterBody.radius / 2;
        workPls.transform.localScale = new Vector3(radius, radius, radius);
        workPls.AddComponent<SelfDestructController>();
        UmbralMithrix.leapIndicator = workPls;
        NetworkServer.Spawn(workPls);
      }
      orig(self);
    }

    private void ExitSkyLeap_OnEnter(On.EntityStates.BrotherMonster.ExitSkyLeap.orig_OnEnter orig, ExitSkyLeap self)
    {
      orig(self);
      if (!(bool)PhaseCounter.instance || PhaseCounter.instance.phase != 2)
        return;
      GenericSkill genericSkill = (bool)self.skillLocator ? self.skillLocator.special : null;
      if (!(bool)genericSkill)
        return;
      UltChannelState.replacementSkillDef.activationState = new SerializableEntityStateType(typeof(UltEnterState));
      genericSkill.SetSkillOverride(self.outer, UltChannelState.replacementSkillDef, GenericSkill.SkillOverridePriority.Contextual);
    }

    private void FistSlam_OnEnter(On.EntityStates.BrotherMonster.FistSlam.orig_OnEnter orig, FistSlam self)
    {
      FistSlam.healthCostFraction = 0.0f;
      orig(self);
    }

    private void UltChannelState_FireWave(On.EntityStates.BrotherMonster.UltChannelState.orig_FireWave orig, UltChannelState self)
    {
      if ((bool)PhaseCounter.instance)
      {
        if (PhaseCounter.instance.phase == 2)
        {
          float num = 360f / ModConfig.UltimateWaves.Value;
          Vector3 vector3 = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
          for (int key = 0; key < 4; ++key)
          {
            for (int index = 0; index < ModConfig.UltimateWaves.Value; ++index)
            {
              Vector3 forward = Quaternion.AngleAxis(num * index, Vector3.up) * vector3;
              ProjectileManager.instance.FireProjectile(UmbralMithrix.staticUltLine, UmbralMithrix.p23PizzaPoints[key], Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * UltChannelState.waveProjectileDamageCoefficient, UltChannelState.waveProjectileForce, Util.CheckRoll(self.characterBody.crit, self.characterBody.master));
            }
          }
        }
        if (PhaseCounter.instance.phase == 3)
        {
          UltChannelState.waveProjectileCount = 0;
          int count = PlayerCharacterMasterController.instances.Count;
          int num1 = ModConfig.UltimateWaves.Value;
          float num2 = 360f / num1;
          Vector3 normalized = Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, Vector3.up).normalized;
          GameObject prefab = UmbralMithrix.leftUltLine;
          if ((double)UnityEngine.Random.value <= 0.5)
            prefab = UmbralMithrix.rightUltLine;
          PlayerCharacterMasterController instance = PlayerCharacterMasterController.instances[new System.Random().Next(0, count - 1)];
          Vector3[] vector3Array = new Vector3[2]
          {
            new Vector3(instance.body.footPosition.x, self.characterBody.footPosition.y, instance.body.footPosition.z) + new Vector3(UnityEngine.Random.Range(-45f, -15f), 0.0f, UnityEngine.Random.Range(-45f, -15f)),
            new Vector3(instance.body.footPosition.x, self.characterBody.footPosition.y, instance.body.footPosition.z) + new Vector3(UnityEngine.Random.Range(15f, 45f), 0.0f, UnityEngine.Random.Range(15f, 45f))
          };
          for (int index1 = 0; index1 < 2; ++index1)
          {
            for (int index2 = 0; index2 < num1; ++index2)
            {
              Vector3 forward = Quaternion.AngleAxis(num2 * index2, Vector3.up) * normalized;
              ProjectileManager.instance.FireProjectile(prefab, vector3Array[index1], Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * UltChannelState.waveProjectileDamageCoefficient, UltChannelState.waveProjectileForce, Util.CheckRoll(self.characterBody.crit, self.characterBody.master));
            }
          }
        }
      }
      orig(self);
    }

    private void SkyLeapDeathState_OnEnter(
      On.EntityStates.BrotherMonster.SkyLeapDeathState.orig_OnEnter orig,
      SkyLeapDeathState self)
    {
      if (self.characterBody.name == "BrotherGlassBody(Clone)" && PhaseCounter.instance && PhaseCounter.instance.phase == 2)
      {
        GameObject gameObject = GameObject.Find("BrotherBody(Clone)");
        if ((bool)gameObject && gameObject.GetComponent<CharacterBody>().healthComponent.alive && gameObject.GetComponent<CharacterBody>().HasBuff(RoR2Content.Buffs.Immune) && UmbralMithrix.timeCrystals.Count == 1)
        {
          UmbralMithrix.timeCrystals.RemoveAt(0);
          gameObject.GetComponent<CharacterBody>().RemoveBuff(RoR2Content.Buffs.Immune);
        }
        self.DestroyModel();
        if (!NetworkServer.active)
          return;
        self.DestroyBodyAsapServer();
      }
      else if (self.characterBody.name == "BrotherGlassBody(Clone)" && PhaseCounter.instance && PhaseCounter.instance.phase == 3)
      {
        GameObject gameObject = GameObject.Find("BrotherHurtBodyP3(Clone)");
        if ((bool)gameObject && gameObject.GetComponent<CharacterBody>().HasBuff(RoR2Content.Buffs.Immune) && UmbralMithrix.timeCrystals.Count == 1)
        {
          UmbralMithrix.timeCrystals.RemoveAt(0);
          gameObject.GetComponent<CharacterBody>().RemoveBuff(RoR2Content.Buffs.Immune);
        }
        else
          UmbralMithrix.timeCrystals.RemoveAt(0);
        orig(self);
      }
      else
        orig(self);
    }

    private void SprintBash_OnEnter(On.EntityStates.BrotherMonster.SprintBash.orig_OnEnter orig, SprintBash self)
    {
      if (self.isAuthority)
      {
        Ray aimRay = self.GetAimRay();
        if (self.characterBody.name == "BrotherBody(Clone)")
        {
          for (int index = 0; index < 6; ++index)
          {
            int num = (int)Util.PlaySound(FireLunarShards.fireSound, self.gameObject);
            ProjectileManager.instance.FireProjectile(FireLunarShards.projectilePrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), self.gameObject, (float)((double)self.characterBody.damage * 0.100000001490116 / 12.0), 0.0f, Util.CheckRoll(self.characterBody.crit, self.characterBody.master));
          }
        }
        else
          ProjectileManager.instance.FireProjectile(SeekingBomb.projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), self.gameObject, self.characterBody.damage * (SeekingBomb.bombDamageCoefficient * 0.75f), SeekingBomb.bombForce, Util.CheckRoll(self.critStat, self.characterBody.master), speedOverride: 0.0f);
        if ((bool)PhaseCounter.instance && self.characterBody.name == "BrotherBody(Clone)")
        {
          if (PhaseCounter.instance.phase != 1)
          {
            Vector3 vector3 = Vector3.ProjectOnPlane(self.inputBank.aimDirection, Vector3.up);
            Vector3 footPosition = self.characterBody.footPosition;
            Vector3 forward = Quaternion.AngleAxis(0.0f, Vector3.up) * vector3;
            ProjectileManager.instance.FireProjectile(WeaponSlam.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * WeaponSlam.waveProjectileDamageCoefficient, WeaponSlam.waveProjectileForce, Util.CheckRoll(self.characterBody.crit, self.characterBody.master));
          }
        }
      }
      orig(self);
    }

    private void SprintBash_OnExit(On.EntityStates.BrotherMonster.SprintBash.orig_OnExit orig, SprintBash self)
    {
      orig(self);
      ProjectileSimple[] objectsOfType = UnityEngine.Object.FindObjectsOfType<ProjectileSimple>();
      if (objectsOfType.Length == 0)
        return;
      foreach (ProjectileSimple projectileSimple in objectsOfType)
      {
        if (projectileSimple.name == "LunarWispTrackingBomb(Clone)")
          projectileSimple.desiredForwardSpeed = 50f;
      }
    }

    private void SpellChannelEnterState_OnEnter(
      On.EntityStates.BrotherMonster.SpellChannelEnterState.orig_OnEnter orig,
      SpellChannelEnterState self)
    {
      SpellChannelEnterState.duration = 3f;
      orig(self);
    }

    private static void SpellChannelState_OnEnter(
      On.EntityStates.BrotherMonster.SpellChannelState.orig_OnEnter orig,
      SpellChannelState self)
    {
      SpellChannelState.maxDuration = 5f;
      TeamComponent[] objectsOfType = UnityEngine.Object.FindObjectsOfType<TeamComponent>();
      for (int index = 0; index < objectsOfType.Length; ++index)
      {
        if (objectsOfType[index].teamIndex == TeamIndex.Player)
          objectsOfType[index].GetComponent<CharacterBody>().AddBuff(RoR2Content.Buffs.TeamWarCry);
      }
      orig(self);
    }

    private void SpellChannelExitState_OnEnter(
      On.EntityStates.BrotherMonster.SpellChannelExitState.orig_OnEnter orig,
      SpellChannelExitState self)
    {
      UmbralMithrix.finishedItemSteal = true;
      self.characterBody.gameObject.GetComponent<P4Controller>().finishedItemSteal = true;
      bool killedAllies = false;
      foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
      {
        if (cm.teamIndex == TeamIndex.Player)
        {
          CharacterBody cb = cm.GetBody();
          if (cb && !cb.isPlayerControlled && cb.healthComponent)
          { cb.healthComponent.Suicide(); }
        }
      }
      if (killedAllies)
      {
        Chat.SendBroadcastChat(new Chat.SimpleChatMessage()
        {
          baseToken = "<color=#c6d5ff><size=120%>Mithrix: Perish.</color></size>"
        });
      }
      orig(self);
    }

    private void StaggerEnter_OnEnter(On.EntityStates.BrotherMonster.StaggerEnter.orig_OnEnter orig, StaggerEnter self)
    {
      self.duration = 0.0f;
      if ((bool)PhaseCounter.instance && PhaseCounter.instance.phase == 4)
      {
        self.outer.SetNextState(new SpellChannelEnterState());
      }
      else
      {
        if ((bool)PhaseCounter.instance && PhaseCounter.instance.phase == 3 && !UmbralMithrix.spawnedClone)
        {
          UmbralMithrix.spawnedClone = true;
          DirectorPlacementRule placementRule = new DirectorPlacementRule();
          placementRule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
          placementRule.minDistance = 3f;
          placementRule.maxDistance = 20f;
          placementRule.position = new Vector3(-88.5f, 491.5f, -0.3f);
          Xoroshiro128Plus rng = RoR2Application.rng;
          DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(UmbralMithrix.mithrixCard, placementRule, rng);
          directorSpawnRequest.summonerBodyObject = self.gameObject;
          directorSpawnRequest.onSpawnedServer += spawnResult =>
          {
            CharacterMaster master = spawnResult.spawnedInstance.GetComponent<CharacterMaster>();
            master.GetBody().AddBuff(RoR2Content.Buffs.Immune);
            foreach (BaseAI baseAI in master.GetComponents<BaseAI>())
            {
              if (baseAI)
              {
                baseAI.fullVision = true;
                baseAI.neverRetaliateFriendlies = true;
              }
            }
          };
          DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
        }
        orig(self);
      }
    }

    private void TrueDeathState_OnEnter(On.EntityStates.BrotherMonster.TrueDeathState.orig_OnEnter orig, TrueDeathState self)
    {
      TrueDeathState.dissolveDuration = 3f;
      if (!UmbralMithrix.practiceModeEnabled)
      {
        Vector3 velocity = Vector3.up * 40f + Vector3.forward * 2f;
        PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(UmbralMithrix.UmbralItem.itemIndex), self.characterBody.footPosition + Vector3.up * 1.5f, velocity);
      }
      UmbralMithrix.practiceModeEnabled = false;
      orig(self);
    }
  }
}
