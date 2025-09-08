using RoR2;
using UnityEngine;
using EntityStates;
using EntityStates.BrotherMonster;
using RoR2.Projectile;

namespace UmbralMithrix.EntityStates;

public class UmbralBash : BasicMeleeAttack
{
    public static float durationBeforePriorityReduces = 0.5f;

    public override void PlayAnimation()
    {
        this.PlayCrossfade("FullBody Override", nameof(SprintBash), "SprintBash.playbackRate", this.duration, 0.05f);
    }

    public override void OnEnter()
    {
        baseDuration = 4f;
        damageCoefficient = 2f;
        hitBoxGroupName = "Weapon";
        hitEffectPrefab = UmbralMithrix.umbralSlamHitEffect;
        procCoefficient = 1f;
        pushAwayForce = 6000f;
        forceVector = new Vector3(0f, 2000f, 0f);
        hitPauseDuration = 0.1f;
        swingEffectPrefab = UmbralMithrix.umbralSwingEffect;
        swingEffectMuzzleString = "MuzzleSprintBash";
        mecanimHitboxActiveParameter = "weapon.hitBoxActive";
        shorthopVelocityFromHit = 0f;
        beginStateSoundString = "Play_moonBrother_swing_horizontal";
        forceForwardVelocity = true;
        impactSound = UmbralMithrix.umbralSlamHitSound;
        forwardVelocityCurve = new AnimationCurve();

        base.OnEnter();
        if (this.isAuthority)
        {
            Ray aimRay = this.GetAimRay();
            if (this.characterBody.name == "BrotherBody(Clone)")
            {
                for (int index = 0; index < 6; ++index)
                {
                    Util.PlaySound(FireUmbralShards.fireSound, this.gameObject);
                    ProjectileManager.instance.FireProjectile(FireUmbralShards.projectilePrefab, aimRay.origin, Quaternion.LookRotation(aimRay.direction), this.gameObject, (float)((double)this.characterBody.damage * 0.100000001490116 / 12.0), 0.0f, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
                }

                if (PhaseCounter.instance && PhaseCounter.instance.phase != 1 && ModConfig.addShockwave.Value)
                {
                    Vector3 vector3 = Vector3.ProjectOnPlane(this.inputBank.aimDirection, Vector3.up);
                    Vector3 footPosition = this.characterBody.footPosition;
                    Vector3 forward = Quaternion.AngleAxis(0.0f, Vector3.up) * vector3;
                    ProjectileManager.instance.FireProjectile(WeaponSlam.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.characterBody.damage * WeaponSlam.waveProjectileDamageCoefficient, WeaponSlam.waveProjectileForce, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
                }
            }
        }
        AimAnimator aimAnimator = this.GetAimAnimator();
        if ((bool)aimAnimator)
            aimAnimator.enabled = true;
        if (!(bool)this.characterDirection)
            return;
        this.characterDirection.forward = this.inputBank.aimDirection;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!this.isAuthority || !(bool)this.inputBank || !(bool)this.skillLocator || !this.skillLocator.utility.IsReady() || !this.inputBank.skill3.justPressed)
            return;
        this.skillLocator.utility.ExecuteIfReady();
    }

    public override void OnExit()
    {
        Transform modelChild = this.FindModelChild("SpinnyFX");
        if ((bool)modelChild)
            modelChild.gameObject.SetActive(false);
        this.PlayCrossfade("FullBody Override", "BufferEmpty", 0.1f);
        base.OnExit();
    }

    public override InterruptPriority GetMinimumInterruptPriority()
    {
        return (double)this.fixedAge <= SprintBash.durationBeforePriorityReduces ? InterruptPriority.PrioritySkill : InterruptPriority.Skill;
    }
}
