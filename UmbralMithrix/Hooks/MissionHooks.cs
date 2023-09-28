using EntityStates.Missions.BrotherEncounter;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UmbralMithrix
{
  public class MissionHooks
  {
    public MissionHooks()
    {
      On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter += BrotherEncounterPhaseBaseStateOnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase1.OnEnter += Phase1OnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase2.OnEnter += Phase2OnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase3.OnEnter += Phase3OnEnter;
      On.EntityStates.Missions.BrotherEncounter.Phase4.OnEnter += Phase4OnEnter;
      On.EntityStates.Missions.BrotherEncounter.BossDeath.OnEnter += BossDeathOnEnter;
    }

    private void Phase1OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, Phase1 self)
    {
      UmbralMithrix.spawnedClone = false;
      UmbralMithrix.p2ThresholdReached = false;
      UmbralMithrix.p3ThresholdReached = false;
      UmbralMithrix.finishedItemSteal = false;
      UmbralMithrix.AdjustBaseSkills();
      UmbralMithrix.AdjustBaseStats();
      GameObject enabled = GameObject.Find("BonfireEnabled");
      if (enabled)
      {
        if (UmbralMithrix.bonfireRunTime == 0.0)
        {
          this.SaveAllies();
          UmbralMithrix.bonfireRunTime = Run.instance.time;
        }
        foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
        {
          if (cm.teamIndex == TeamIndex.Player)
          {
            CharacterBody cb = cm.GetBody();
            if (cb && cb.isPlayerControlled)
            {
              if (!UmbralMithrix.bonfireInventory.ContainsKey(cm) && !UmbralMithrix.persistentBuffs.ContainsKey(cm) && !UmbralMithrix.bonfireGold.ContainsKey(cm))
              {
                UmbralMithrix.bonfireGold.Add(cm, cm.money);
                GameObject gameObject = new GameObject();
                gameObject.AddComponent<CharacterMaster>();
                Inventory inventory = gameObject.AddComponent<Inventory>();
                inventory.AddItemsFrom(cm.inventory, _ => true);
                inventory.CopyEquipmentFrom(cm.inventory);
                UmbralMithrix.bonfireInventory.Add(cm, inventory);
                Dictionary<BuffIndex, int> dictionary = new Dictionary<BuffIndex, int>();
                int buffCount1 = cb.GetBuffCount(RoR2Content.Buffs.PermanentCurse.buffIndex);
                if (buffCount1 > 0)
                  dictionary.Add(RoR2Content.Buffs.PermanentCurse.buffIndex, buffCount1);
                int buffCount2 = cb.GetBuffCount(RoR2Content.Buffs.BanditSkull.buffIndex);
                if (buffCount2 > 0)
                  dictionary.Add(RoR2Content.Buffs.BanditSkull.buffIndex, buffCount2);
                if (dictionary.Count > 0)
                {
                  UmbralMithrix.persistentBuffs[cm] = dictionary;
                  Debug.Log(string.Format("Saved buffs for player `{0}` : Curse={1}, BanditSkulls={2}", cm.name, buffCount1, buffCount2));
                }
              }
            }
          }
        }
      }

      GameObject gameObject1 = GameObject.Find("EscapeSequenceController");
      if ((bool)gameObject1)
      {
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject1.transform.GetChild(0).GetChild(8).gameObject, new Vector3(-88.5f, 491.5f, -0.3f), Quaternion.identity);
        gameObject2.transform.eulerAngles = new Vector3(270f, 0.0f, 0.0f);
        gameObject2.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        gameObject2.AddComponent<NetworkIdentity>();
        NetworkServer.Spawn(gameObject2);
      }
      orig(self);
    }

    private void Phase2OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase2.orig_OnEnter orig, Phase2 self)
    {
      self.KillAllMonsters();
      UmbralMithrix.AdjustPhase2Stats();
      orig(self);
    }

    private void Phase3OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase3.orig_OnEnter orig, Phase3 self)
    {
      self.KillAllMonsters();
      UmbralMithrix.AdjustPhase3Stats();
      orig(self);
    }

    private void Phase4OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase4.orig_OnEnter orig, Phase4 self)
    {
      UmbralMithrix.AdjustPhase4Stats();
      orig(self);
    }

    private void BossDeathOnEnter(On.EntityStates.Missions.BrotherEncounter.BossDeath.orig_OnEnter orig, BossDeath self)
    {
      orig(self);
      TeamComponent[] objectsOfType = UnityEngine.Object.FindObjectsOfType<TeamComponent>();
      for (int index = 0; index < objectsOfType.Length; ++index)
      {
        if (objectsOfType[index].teamIndex == TeamIndex.Player)
          objectsOfType[index].GetComponent<CharacterBody>().RemoveBuff(RoR2Content.Buffs.TeamWarCry);
      }
    }

    private void BrotherEncounterPhaseBaseStateOnEnter(
      On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_OnEnter orig,
      BrotherEncounterPhaseBaseState self)
    {
      orig(self);

      if (!NetworkServer.active || self.phaseScriptedCombatEncounter == null || !(bool)PhaseCounter.instance)
        return;
      ScriptedCombatEncounter.SpawnInfo spawnInfo1;
      if (PhaseCounter.instance.phase == 2)
      {
        Transform transform = new GameObject()
        {
          transform = {
            position = new Vector3(-88.5f, 491.5f, -0.3f),
            rotation = Quaternion.identity
          }
        }.transform;
        spawnInfo1 = new ScriptedCombatEncounter.SpawnInfo();
        spawnInfo1.explicitSpawnPosition = transform;
        spawnInfo1.spawnCard = UmbralMithrix.mithrixCard;
        ScriptedCombatEncounter.SpawnInfo spawnInfo2 = spawnInfo1;
        self.phaseScriptedCombatEncounter.spawns = new ScriptedCombatEncounter.SpawnInfo[1]
        {
          spawnInfo2
        };
      }
      if (PhaseCounter.instance.phase == 3)
      {
        Transform transform = new GameObject()
        {
          transform = {
            position = new Vector3(-88.5f, 491.5f, -0.3f),
            rotation = Quaternion.identity
          }
        }.transform;
        spawnInfo1 = new ScriptedCombatEncounter.SpawnInfo();
        spawnInfo1.explicitSpawnPosition = transform;
        spawnInfo1.spawnCard = UmbralMithrix.mithrixHurtP3Card;
        ScriptedCombatEncounter.SpawnInfo spawnInfo3 = spawnInfo1;
        self.phaseScriptedCombatEncounter.spawns = new ScriptedCombatEncounter.SpawnInfo[1]
        {
          spawnInfo3
        };
      }
    }

    private void SaveAllies()
    {
      int num = 0;
      TeamIndex teamIndex = TeamIndex.Player;
      foreach (CharacterMaster characterMaster in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
      {
        if (characterMaster.teamIndex == teamIndex)
        {
          CharacterBody body = characterMaster.GetBody();
          if ((bool)body && (bool)body.healthComponent && (body.name.Contains("Drone") || body.name.Contains("Turret")))
          {
            UmbralMithrix.bonfireAllies.Add(body.name);
            if (body.name.Contains("EquipmentDrone"))
              UmbralMithrix.droneEquips.Add(body.inventory.GetEquipmentIndex());
            ++num;
          }
        }
      }
      if (num <= 0)
        return;
      Debug.Log(string.Format("Added {0} allies.", num));
    }
  }
}
