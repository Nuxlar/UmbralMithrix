using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UmbralMithrix
{
  public class CloneController : MonoBehaviour
  {
    private List<CharacterBody> playerBodies = new();
    private SpawnCard cloneCard = UmbralMithrix.mithrixGlassCard;
    private float stopwatch = 0f;
    private float interval = 8f;
    private int lifetime = 12;

    private void Start()
    {
      foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
      {
        if (cm.teamIndex == TeamIndex.Player)
        {
          CharacterBody cb = cm.GetBody();
          if (cb && cb.isPlayerControlled)
            playerBodies.Add(cb);
        }
      }
      if (PhaseCounter.instance)
      {
        if (PhaseCounter.instance.phase == 2)
          lifetime = 6;
        else if (PhaseCounter.instance.phase == 3)
          lifetime = 4;
      }
    }

    private void FixedUpdate()
    {
      this.stopwatch += Time.deltaTime;
      if ((double)this.stopwatch < this.interval)
        return;
      foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
      {
        if (cm.teamIndex == TeamIndex.Player)
        {
          CharacterBody cb = cm.GetBody();
          if (cb && cb.isPlayerControlled)
            playerBodies.Add(cb);
        }
      }
      this.stopwatch %= this.interval;
      DirectorPlacementRule placementRule = new DirectorPlacementRule();
      placementRule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
      placementRule.minDistance = 12f;
      placementRule.maxDistance = 20f;
      placementRule.position = playerBodies[UnityEngine.Random.Range(0, playerBodies.Count)].corePosition;
      Xoroshiro128Plus rng = RoR2Application.rng;
      DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(cloneCard, placementRule, rng)
      {
        summonerBodyObject = this.gameObject,
        onSpawnedServer = (Action<SpawnCard.SpawnResult>)(spawnResult => spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, lifetime))
      });
    }

  }
}