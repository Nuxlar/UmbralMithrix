using System;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace UmbralMithrix.EntityStates
{
    public class UmbralHammerSlam : BaseState
    {
        public static float duration = 3.5f;
        public static float damageCoefficient = 4f;
        public static float forceMagnitude = 5000f;
        public static float upwardForce = 1000f;
        public static float radius = 12f;
        public static string attackSoundString = "Play_moonBrother_swing_vertical";
        public static string muzzleString = "SlamZone";
        public static GameObject slamImpactEffect = UmbralMithrix.umbralSlamImpact;
        public static float durationBeforePriorityReduces = 1.2f;
        public static GameObject orbProjectilePrefab = UmbralMithrix.umbralSlamProjectile;
        public static GameObject waveProjectilePrefab = UmbralMithrix.umbralLeapWave;
        public static float waveProjectileArc = 120f;
        public static int waveProjectileCount = 3;
        public static float waveProjectileDamageCoefficient = 2f;
        public static float waveProjectileForce = 400f;
        public static float weaponDamageCoefficient = 3f;
        public static float weaponForce = 5000f;
        public static GameObject pillarProjectilePrefab = UmbralMithrix.umbralSlamPillar;
        public static float pillarDamageCoefficient = 3f;
        public static GameObject weaponHitEffectPrefab = UmbralMithrix.umbralSlamHitEffect;
        public static NetworkSoundEventDef weaponImpactSound = UmbralMithrix.umbralSlamHitSound;
        private BlastAttack blastAttack;
        private OverlapAttack weaponAttack;
        private Animator modelAnimator;
        private Transform modelTransform;
        private bool hasDoneBlastAttack;

        public override void OnEnter()
        {
            base.OnEnter();
            UmbralMithrix.hasfired = false;
            this.modelAnimator = base.GetModelAnimator();
            this.modelTransform = base.GetModelTransform();
            Util.PlayAttackSpeedSound(UmbralHammerSlam.attackSoundString, base.gameObject, this.attackSpeedStat);
            base.PlayCrossfade("FullBody Override", "WeaponSlam", "WeaponSlam.playbackRate", UmbralHammerSlam.duration, 0.1f);
            if (base.characterDirection)
            {
                base.characterDirection.moveVector = base.GetAimRay().direction;
            }
            if (this.modelTransform)
            {
                AimAnimator component = this.modelTransform.GetComponent<AimAnimator>();
                if (component)
                {
                    component.enabled = true;
                }
            }
            if (base.isAuthority)
            {
                OverlapAttack overlapAttack = new OverlapAttack();
                overlapAttack.attacker = base.gameObject;
                overlapAttack.damage = UmbralHammerSlam.damageCoefficient * this.damageStat;
                overlapAttack.damageColorIndex = DamageColorIndex.Default;
                overlapAttack.damageType = DamageType.Generic;
                overlapAttack.hitEffectPrefab = UmbralHammerSlam.weaponHitEffectPrefab;
                overlapAttack.hitBoxGroup = Array.Find<HitBoxGroup>(this.modelTransform.GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "WeaponBig");
                overlapAttack.impactSound = UmbralHammerSlam.weaponImpactSound.index;
                overlapAttack.inflictor = base.gameObject;
                overlapAttack.procChainMask = default(ProcChainMask);
                overlapAttack.pushAwayForce = UmbralHammerSlam.weaponForce;
                overlapAttack.procCoefficient = 1f;
                overlapAttack.teamIndex = base.GetTeam();
                this.weaponAttack = overlapAttack;
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.inputBank && base.skillLocator && base.skillLocator.utility.IsReady() && base.inputBank.skill3.justPressed)
            {
                base.skillLocator.utility.ExecuteIfReady();
                return;
            }
            if (this.modelAnimator)
            {
                if (base.isAuthority && this.modelAnimator.GetFloat("weapon.hitBoxActive") > 0.5f)
                {
                    this.weaponAttack.Fire(null);
                }
                if (this.modelAnimator.GetFloat("blast.hitBoxActive") > 0.5f && !this.hasDoneBlastAttack)
                {
                    this.hasDoneBlastAttack = true;
                    EffectManager.SimpleMuzzleFlash(UmbralHammerSlam.slamImpactEffect, base.gameObject, UmbralHammerSlam.muzzleString, false);
                    if (base.isAuthority)
                    {
                        if (!UmbralMithrix.hasfired)
                        {
                            UmbralMithrix.hasfired = true;
                            if (PhaseCounter.instance)
                            {
                                int num1 = ModConfig.SlamOrbProjectileCount.Value;
                                float num2 = 360f / num1;
                                Vector3 vector3 = Vector3.ProjectOnPlane(this.characterDirection.forward, Vector3.up);
                                Vector3 position = this.FindModelChild(UmbralHammerSlam.muzzleString).position;
                                for (int index = 0; index < num1; ++index)
                                {
                                    Vector3 forward = Quaternion.AngleAxis(num2 * index, Vector3.up) * vector3;
                                    ProjectileManager.instance.FireProjectile(UmbralHammerSlam.orbProjectilePrefab, position, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.characterBody.damage * (UmbralHammerSlam.waveProjectileDamageCoefficient * 0.75f), UmbralHammerSlam.waveProjectileForce, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
                                }
                            }
                        }
                        if (base.characterDirection)
                        {
                            base.characterDirection.moveVector = base.characterDirection.forward;
                        }
                        if (this.modelTransform)
                        {
                            Transform transform = base.FindModelChild(UmbralHammerSlam.muzzleString);
                            if (transform)
                            {
                                this.blastAttack = new BlastAttack();
                                this.blastAttack.attacker = base.gameObject;
                                this.blastAttack.inflictor = base.gameObject;
                                this.blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
                                this.blastAttack.baseDamage = this.damageStat * UmbralHammerSlam.damageCoefficient;
                                this.blastAttack.baseForce = UmbralHammerSlam.forceMagnitude;
                                this.blastAttack.position = transform.position;
                                this.blastAttack.radius = UmbralHammerSlam.radius;
                                this.blastAttack.bonusForce = new Vector3(0f, UmbralHammerSlam.upwardForce, 0f);
                                this.blastAttack.Fire();
                            }
                        }
                        if (PhaseCounter.instance && PhaseCounter.instance.phase == 3)
                        {
                            Transform transform2 = base.FindModelChild(UmbralHammerSlam.muzzleString);
                            float num = UmbralHammerSlam.waveProjectileArc / (float)UmbralHammerSlam.waveProjectileCount;
                            Vector3 vector = Vector3.ProjectOnPlane(base.characterDirection.forward, Vector3.up);
                            Vector3 vector2 = base.characterBody.footPosition;
                            if (transform2)
                            {
                                vector2 = transform2.position;
                            }
                            for (int i = 0; i < UmbralHammerSlam.waveProjectileCount; i++)
                            {
                                Vector3 vector3 = Quaternion.AngleAxis(num * ((float)i - (float)UmbralHammerSlam.waveProjectileCount / 2f), Vector3.up) * vector;
                                ProjectileManager.instance.FireProjectileWithoutDamageType(UmbralHammerSlam.waveProjectilePrefab, vector2, Util.QuaternionSafeLookRotation(vector3), base.gameObject, base.characterBody.damage * UmbralHammerSlam.waveProjectileDamageCoefficient, UmbralHammerSlam.waveProjectileForce, Util.CheckRoll(base.characterBody.crit, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                            }
                            ProjectileManager.instance.FireProjectileWithoutDamageType(UmbralHammerSlam.pillarProjectilePrefab, vector2, Quaternion.identity, base.gameObject, base.characterBody.damage * UmbralHammerSlam.pillarDamageCoefficient, 0f, Util.CheckRoll(base.characterBody.crit, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                        }
                    }
                }
            }
            if (base.fixedAge >= UmbralHammerSlam.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (base.fixedAge <= UmbralHammerSlam.durationBeforePriorityReduces)
            {
                return InterruptPriority.PrioritySkill;
            }
            return InterruptPriority.Skill;
        }
    }
}
