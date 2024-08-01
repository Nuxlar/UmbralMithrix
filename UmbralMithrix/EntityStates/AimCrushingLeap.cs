using EntityStates;
using EntityStates.Huntress;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UmbralMithrix
{
    public class AimCrushingLeap : BaseSkillState
    {
        public static GameObject areaIndicatorPrefab = UmbralMithrix.leapIndicatorPrefab;
        public float maxDuration = ModConfig.CrushingLeap.Value;
        private float stopwatch;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;
        private GameObject areaIndicatorInstance;
        private PlayerCharacterMasterController targetPlayer;
        private static Material awShellExpolsionMat = UmbralMithrix.crushingLeapDangerIndicatorMat;

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
            Util.PlaySound("Play_voidRaid_snipe_shoot_final", this.gameObject);
            this.gameObject.layer = LayerIndex.fakeActor.intVal;
            this.characterMotor.Motor.RebuildCollidableLayers();
            this.characterMotor.velocity = Vector3.zero;
            this.characterMotor.Motor.SetPosition(new Vector3(this.characterMotor.transform.position.x, this.characterMotor.transform.position.y + 25f, this.characterMotor.transform.position.z));
            if (!(bool)AimCrushingLeap.areaIndicatorPrefab)
                return;
            this.areaIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(AimCrushingLeap.areaIndicatorPrefab);
            this.areaIndicatorInstance.transform.localScale = new Vector3(ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius);
            this.targetPlayer = PlayerCharacterMasterController.instances[UnityEngine.Random.Range(0, PlayerCharacterMasterController.instances.Count - 1)];
        }

        private void UpdateAreaIndicator()
        {
            if (!(bool)this.areaIndicatorInstance)
                return;
            RaycastHit hitInfo;
            if (Physics.Raycast(new Ray(this.targetPlayer.body.footPosition, Vector3.down), out hitInfo, 200f, (int)LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                this.areaIndicatorInstance.transform.position = hitInfo.point;
            else
                this.areaIndicatorInstance.transform.position = this.targetPlayer.body.footPosition;
        }

        public void HandleFollowupAttack()
        {
            this.characterMotor.Motor.SetPosition(new Vector3(this.areaIndicatorInstance.transform.position.x, this.areaIndicatorInstance.transform.position.y + 1f, this.areaIndicatorInstance.transform.position.z));
            this.outer.SetNextState(new ExitCrushingLeap());
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;
            if (this.stopwatch < Mathf.Clamp(this.maxDuration - 1f, 0.25f, 10f))
                this.UpdateAreaIndicator();
            if (this.stopwatch >= Mathf.Clamp(this.maxDuration - 1f, 0.1f, 10f))
                this.areaIndicatorInstance.transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = AimCrushingLeap.awShellExpolsionMat;
            if (!this.isAuthority || !(bool)this.inputBank || (double)this.fixedAge < this.maxDuration)
                return;
            this.HandleFollowupAttack();
        }

        public override void OnExit()
        {
            if ((bool)this.characterModel)
                --this.characterModel.invisibilityCount;
            if ((bool)this.hurtboxGroup)
                --this.hurtboxGroup.hurtBoxesDeactivatorCounter;
            this.gameObject.layer = LayerIndex.defaultLayer.intVal;
            this.characterMotor.Motor.RebuildCollidableLayers();
            if ((bool)this.areaIndicatorInstance)
                EntityState.Destroy(this.areaIndicatorInstance.gameObject);
            base.OnExit();
        }
    }
}
