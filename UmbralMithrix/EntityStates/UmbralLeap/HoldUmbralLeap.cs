using RoR2;
using UnityEngine;
using EntityStates;
using EntityStates.BrotherMonster;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace UmbralMithrix.EntityStates;

public class HoldUmbralLeap : BaseState
{
    public static float duration = ModConfig.CrushingLeap.Value;
    private CharacterModel characterModel;
    private HurtBoxGroup hurtboxGroup;
    private int originalLayer;

    public override void OnEnter()
    {
        base.OnEnter();
        Transform modelTransform = this.GetModelTransform();
        if ((bool)modelTransform)
        {
            this.characterModel = modelTransform.GetComponent<CharacterModel>();
            this.hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
        }
        if ((bool)this.characterModel)
            ++this.characterModel.invisibilityCount;
        if ((bool)this.hurtboxGroup)
            ++this.hurtboxGroup.hurtBoxesDeactivatorCounter;
        this.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
        Util.PlaySound("Play_moonBrother_phaseJump_land_preWhoosh", this.gameObject);
        this.originalLayer = this.gameObject.layer;
        this.gameObject.layer = LayerIndex.GetAppropriateFakeLayerForTeam(this.teamComponent.teamIndex).intVal;
        this.characterMotor.Motor.RebuildCollidableLayers();

        if (this.isAuthority)
        {
            List<CharacterBody> playerBodies = new();
            foreach (CharacterMaster cm in CharacterMaster.readOnlyInstancesList)
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
                if (Physics.Raycast(new Ray(target, Vector3.down), out RaycastHit hit, 500f, (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                {
                    this.characterMotor.Motor.SetPositionAndRotation(hit.point + new Vector3(0, 10, 0), Quaternion.identity);
                }
                else
                {
                    this.characterMotor.Motor.SetPositionAndRotation(target, Quaternion.identity);
                }
            }

            GameObject workPls = GameObject.Instantiate(UmbralMithrix.leapIndicatorPrefab, this.characterBody.footPosition, Quaternion.identity);
            float radius = this.characterBody.radius / 2;
            workPls.transform.localScale = new Vector3(radius, radius, radius);
            workPls.AddComponent<SelfDestructController>();
            UmbralMithrix.leapIndicator = workPls;
            if (NetworkServer.active)
                NetworkServer.Spawn(workPls);
        }

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!this.isAuthority || (double)this.fixedAge <= (double)HoldSkyLeap.duration)
            return;
        this.outer.SetNextState(new ExitUmbralLeap());
    }

    public override void OnExit()
    {
        if ((bool)this.characterModel)
            --this.characterModel.invisibilityCount;
        if ((bool)this.hurtboxGroup)
            --this.hurtboxGroup.hurtBoxesDeactivatorCounter;
        this.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
        this.gameObject.layer = this.originalLayer;
        this.characterMotor.Motor.RebuildCollidableLayers();
        base.OnExit();
    }
}
