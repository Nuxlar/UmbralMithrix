using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace UmbralMithrix.EntityStates
{
    public class GlassOrbAttack : BaseState
    {
        public static float duration = 0.2f;
        public static float waitDuration = 3f;
        public static float damageCoefficient = 3.75f;
        public static float force = 1000f;
        public static string soundString = "Play_moonBrother_blueWall_slam_start";
        public static GameObject projectilePrefab = UmbralMithrix.cloneTrackingOrb;
        public static GameObject muzzleFlashEffect = UmbralMithrix.umbralUltMuzzleFlash;

        public override void OnEnter()
        {
            base.OnEnter();
            this.PlayCrossfade("Body", "UltEnter", "Ult.playbackRate", GlassOrbAttack.duration, 0.1f);
            Util.PlaySound(GlassOrbAttack.soundString, this.gameObject);
            if (this.isAuthority)
            {
                Ray aimRay = this.GetAimRay();
                EffectManager.SimpleMuzzleFlash(GlassOrbAttack.muzzleFlashEffect, this.gameObject, "MuzzleUlt", false);
                ProjectileManager.instance.FireProjectile(GlassOrbAttack.projectilePrefab, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), this.gameObject, this.characterBody.damage * GlassOrbAttack.damageCoefficient, GlassOrbAttack.force, Util.CheckRoll(this.critStat, this.characterBody.master), speedOverride: 0.0f);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if ((double)this.fixedAge <= GlassOrbAttack.waitDuration)
                return;
            this.outer.SetNextState(new GlassOrbAttackExit());
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
