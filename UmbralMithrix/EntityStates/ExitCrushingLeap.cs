using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace UmbralMithrix
{
    public class ExitCrushingLeap : BaseSkillState
    {
        private float duration;
        // private float baseDuration = 0.5f;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = ExitSkyLeap.baseDuration / this.attackSpeedStat;
            Util.PlaySound(ExitSkyLeap.soundString, this.gameObject);
            this.PlayAnimation("Body", "ExitSkyLeap", "SkyLeap.playbackRate", this.duration);
            this.PlayAnimation("FullBody Override", "BufferEmpty");
            this.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, ExitSkyLeap.baseDuration);
            AimAnimator aimAnimator = this.GetAimAnimator();
            if ((bool)aimAnimator)
                aimAnimator.enabled = true;
            float num2 = 360f / ExitSkyLeap.waveProjectileCount;
            Vector3 vector3 = Vector3.ProjectOnPlane(this.inputBank.aimDirection, Vector3.up);
            Vector3 footPosition = this.characterBody.footPosition;
            for (int index = 0; index < ExitSkyLeap.waveProjectileCount; ++index)
            {
                Vector3 forward = Quaternion.AngleAxis(num2 * (float)index, Vector3.up) * vector3;
                ProjectileManager.instance.FireProjectile(ExitSkyLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.characterBody.damage * ExitSkyLeap.waveProjectileDamageCoefficient, ExitSkyLeap.waveProjectileForce, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
            }
            if (!(bool)PhaseCounter.instance)
                return;
            if (PhaseCounter.instance.phase == 1)
                return;
            GenericSkill special = (bool)this.skillLocator ? this.skillLocator.special : null;
            if (!(bool)special)
                return;
            UltChannelState.replacementSkillDef.activationState = new SerializableEntityStateType(typeof(UltEnterState));
            special.SetSkillOverride(this.outer, UltChannelState.replacementSkillDef, GenericSkill.SkillOverridePriority.Contextual);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!this.isAuthority || (double)this.fixedAge <= (double)this.duration)
                return;
            this.outer.SetNextStateToMain();
        }
    }
}
