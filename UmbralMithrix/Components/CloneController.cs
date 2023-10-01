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
    }

    private void FixedUpdate()
    {
      this.stopwatch += Time.deltaTime;
      if ((double)this.stopwatch < this.interval)
        return;
      this.stopwatch %= this.interval;
      DirectorPlacementRule placementRule = new DirectorPlacementRule();
      placementRule.placementMode = DirectorPlacementRule.PlacementMode.NearestNode;
      placementRule.minDistance = 3f;
      placementRule.maxDistance = 10f;
      placementRule.position = playerBodies[UnityEngine.Random.Range(0, playerBodies.Count)].corePosition;
      Xoroshiro128Plus rng = RoR2Application.rng;
      DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(cloneCard, placementRule, rng)
      {
        summonerBodyObject = this.gameObject,
        onSpawnedServer = (Action<SpawnCard.SpawnResult>)(spawnResult => spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, 6))
      });
    }

  }
}