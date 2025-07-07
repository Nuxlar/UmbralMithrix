using EntityStates;
using RoR2.Projectile;
using RoR2;
using UnityEngine;

namespace UmbralMithrix.EntityStates
{
    public class FireUmbralShards : BaseSkillState
    {
        public static float baseDuration = 0.11f;
        public float damageCoefficient = 0.05f;
        public static GameObject projectilePrefab = UmbralMithrix.shardProjectile;
        public static float recoilAmplitude = 1.5f;
        public static float spreadBloomValue = 0.4f;
        public static string muzzleString = "MuzzleLeft";
        public static GameObject muzzleFlashEffectPrefab;
        public static string fireSound = "Play_moonBrother_m1_laser_shoot";
        public float maxSpread = 2f;
        public float spreadYawScale = 5f;
        public float spreadPitchScale = 1f;
        private float duration;
        private static int FireLunarShardsStateHash = Animator.StringToHash("FireLunarShards");

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FireUmbralShards.baseDuration / this.attackSpeedStat;
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                Transform transform = base.FindModelChild(FireUmbralShards.muzzleString);
                if (transform)
                {
                    aimRay.origin = transform.position;
                }
                aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, this.maxSpread, this.spreadYawScale, this.spreadPitchScale, 0f, 0f);
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
                fireProjectileInfo.crit = base.characterBody.RollCrit();
                fireProjectileInfo.damage = base.characterBody.damage * this.damageCoefficient;
                fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                fireProjectileInfo.owner = base.gameObject;
                fireProjectileInfo.procChainMask = default(ProcChainMask);
                fireProjectileInfo.force = 0f;
                fireProjectileInfo.useFuseOverride = false;
                fireProjectileInfo.useSpeedOverride = false;
                fireProjectileInfo.target = null;
                fireProjectileInfo.projectilePrefab = FireUmbralShards.projectilePrefab;
                for (int index = 0; index < ModConfig.LunarShardAdd.Value; ++index)
                {
                    ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    aimRay.direction = Util.ApplySpread(aimRay.direction, 0.0f, this.maxSpread * (float)(1.0 + 0.449999988079071 * index), this.spreadYawScale * (float)(1.0 + 0.449999988079071 * index), this.spreadPitchScale * (float)(1.0 + 0.449999988079071 * index));
                    fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
                }
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            this.PlayAnimation("Gesture, Additive", FireUmbralShards.FireLunarShardsStateHash);
            this.PlayAnimation("Gesture, Override", FireUmbralShards.FireLunarShardsStateHash);
            base.AddRecoil(-0.4f * FireUmbralShards.recoilAmplitude, -0.8f * FireUmbralShards.recoilAmplitude, -0.3f * FireUmbralShards.recoilAmplitude, 0.3f * FireUmbralShards.recoilAmplitude);
            base.characterBody.AddSpreadBloom(FireUmbralShards.spreadBloomValue);

            if (muzzleFlashEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireUmbralShards.muzzleFlashEffectPrefab, base.gameObject, FireUmbralShards.muzzleString, false);
            }

            Util.PlaySound(FireUmbralShards.fireSound, base.gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
