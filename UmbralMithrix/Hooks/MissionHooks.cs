using EntityStates.Missions.BrotherEncounter;
using RoR2;
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
    }

    private void Phase1OnEnter(On.EntityStates.Missions.BrotherEncounter.Phase1.orig_OnEnter orig, Phase1 self)
    {
      UmbralMithrix.spawnedClone = false;
      UmbralMithrix.p2ThresholdReached = false;
      UmbralMithrix.p3ThresholdReached = false;
      UmbralMithrix.finishedItemSteal = false;
      UmbralMithrix.AdjustBaseSkills();
      UmbralMithrix.AdjustBaseStats();

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

    private void BrotherEncounterPhaseBaseStateOnEnter(
      On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_OnEnter orig,
      BrotherEncounterPhaseBaseState self)
    {
      orig(self);

      if (!NetworkServer.active || self.phaseScriptedCombatEncounter == null || !(bool)PhaseCounter.instance)
        return;
      self.childLocator = self.GetComponent<ChildLocator>();
      Transform child1 = self.childLocator.FindChild("ArenaWalls");
      Material material = Addressables.LoadAssetAsync<Material>((object)"RoR2/Base/InvadingDoppelganger/matDoppelganger.mat").WaitForCompletion();
      if ((bool)child1)
      {
        child1.gameObject.SetActive(self.shouldEnableArenaWalls);
        if (ModConfig.purpleArena.Value)
        {
          foreach (Renderer componentsInChild in child1.GetComponentsInChildren<Renderer>())
            componentsInChild.material = material;
        }
      }
      self.phaseBossGroup.bestObservedName = "Umbral Mithrix";
      self.phaseBossGroup.bestObservedSubtitle = "<sprite name=\"CloudLeft\"> The Collective <sprite name=\"CloudRight\">";
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
      if (PhaseCounter.instance.phase == 4)
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
        spawnInfo1.spawnCard = UmbralMithrix.mithrixHurtCard;
        ScriptedCombatEncounter.SpawnInfo spawnInfo3 = spawnInfo1;
        self.phaseScriptedCombatEncounter.spawns = new ScriptedCombatEncounter.SpawnInfo[1]
        {
          spawnInfo3
        };
      }
    }
  }
}
