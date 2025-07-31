using EntityStates.BrotherMonster;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.Networking;

namespace UmbralMithrix
{
    public class MithrixMiscHooks
    {
        public MithrixMiscHooks()
        {
            On.EntityStates.BrotherMonster.SkyLeapDeathState.OnEnter += SkyLeapDeathState_OnEnter;
            On.EntityStates.BrotherMonster.SpellChannelExitState.OnEnter += SpellChannelExitState_OnEnter;
            On.EntityStates.BrotherMonster.StaggerEnter.OnEnter += StaggerEnter_OnEnter;
            On.EntityStates.BrotherMonster.TrueDeathState.OnEnter += TrueDeathState_OnEnter;
        }

        private void SkyLeapDeathState_OnEnter(On.EntityStates.BrotherMonster.SkyLeapDeathState.orig_OnEnter orig, SkyLeapDeathState self)
        {
            if (self.characterBody.name == "BrotherGlassBody(Clone)" && PhaseCounter.instance && PhaseCounter.instance.phase == 2)
            {
                self.DestroyModel();

                if (!NetworkServer.active)
                    return;

                self.DestroyBodyAsapServer();
            }
            else
            {
                orig(self);
            }
        }

        private void SpellChannelExitState_OnEnter(On.EntityStates.BrotherMonster.SpellChannelExitState.orig_OnEnter orig, SpellChannelExitState self)
        {
            if (UmbralMissionController.instance)
                UmbralMissionController.instance.finishedItemSteal = true;

            self.characterBody.gameObject.GetComponent<P4Controller>().finishedItemSteal = true;

            bool killedAllies = false;
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
            {
                if (cm.teamIndex == TeamIndex.Player)
                {
                    CharacterBody cb = cm.GetBody();
                    if (cb && !cb.isPlayerControlled && cb.healthComponent)
                    {
                        cb.healthComponent.Suicide();
                    }
                }
            }

            if (killedAllies)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "UMBRAL_KILLED_ALL_ALLIES_MESAAGE"
                });
            }

            orig(self);
        }

        private void StaggerEnter_OnEnter(On.EntityStates.BrotherMonster.StaggerEnter.orig_OnEnter orig, StaggerEnter self)
        {
            self.duration = 0.0f;

            if (PhaseCounter.instance && PhaseCounter.instance.phase == 4)
            {
                self.outer.SetNextState(new SpellChannelEnterState());
            }

            if (UmbralMissionController.instance && PhaseCounter.instance && PhaseCounter.instance.phase == 3 && UmbralMissionController.instance && !UmbralMissionController.instance.spawnedClone)
            {
                UmbralMissionController.instance.spawnedClone = true;
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
                    master.GetBody().AddBuff(RoR2Content.Buffs.Intangible);
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

        private void TrueDeathState_OnEnter(On.EntityStates.BrotherMonster.TrueDeathState.orig_OnEnter orig, TrueDeathState self)
        {
            TrueDeathState.dissolveDuration = 3f;
            if (UmbralMissionController.instance)
            {
                if (!UmbralMissionController.instance.practiceModeEnabled)
                {
                    Vector3 velocity = (Vector3.up * 40f) + (Vector3.forward * 2f);
                    PickupDropletController.CreatePickupDroplet(PickupCatalog.FindPickupIndex(UmbralMithrix.UmbralItem.itemIndex), self.characterBody.footPosition + (Vector3.up * 1.5f), velocity);
                }

                UmbralMissionController.instance.practiceModeEnabled = false;
            }
            orig(self);
        }
    }
}
