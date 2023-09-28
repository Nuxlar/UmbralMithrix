using EntityStates.BrotherMonster;
using EntityStates.BrotherMonster.Weapon;
using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine;

namespace UmbralMithrix
{
  public class PrimaryHooks
  {
    public PrimaryHooks()
    {
      On.EntityStates.BrotherMonster.WeaponSlam.OnEnter += WeaponSlamOnEnter;
      On.EntityStates.BrotherMonster.WeaponSlam.FixedUpdate += WeaponSlamFixedUpdate;
      On.EntityStates.BrotherMonster.Weapon.FireLunarShards.OnEnter += FireLunarShardsOnEnter;
    }

    private void WeaponSlamOnEnter(On.EntityStates.BrotherMonster.WeaponSlam.orig_OnEnter orig, WeaponSlam self)
    {
      UmbralMithrix.hasfired = false;
      orig(self);
    }

    private void WeaponSlamFixedUpdate(On.EntityStates.BrotherMonster.WeaponSlam.orig_FixedUpdate orig, WeaponSlam self)
    {
      if (self.isAuthority && self.hasDoneBlastAttack && (bool)self.modelTransform && !UmbralMithrix.hasfired)
      {
        UmbralMithrix.hasfired = true;
        if ((bool)PhaseCounter.instance)
        {
          int num1 = ModConfig.SlamOrbProjectileCount.Value;
          float num2 = 360f / num1;
          Vector3 vector3 = Vector3.ProjectOnPlane(self.characterDirection.forward, Vector3.up);
          Vector3 position = self.FindModelChild(WeaponSlam.muzzleString).position;
          for (int index = 0; index < num1; ++index)
          {
            Vector3 forward = Quaternion.AngleAxis(num2 * index, Vector3.up) * vector3;
            ProjectileManager.instance.FireProjectile(FistSlam.waveProjectilePrefab, position, Util.QuaternionSafeLookRotation(forward), self.gameObject, self.characterBody.damage * (FistSlam.waveProjectileDamageCoefficient * 0.75f), FistSlam.waveProjectileForce, Util.CheckRoll(self.characterBody.crit, self.characterBody.master));
          }
        }
      }
      orig(self);
    }

    private void FireLunarShardsOnEnter(On.EntityStates.BrotherMonster.Weapon.FireLunarShards.orig_OnEnter orig, FireLunarShards self)
    {
      if (!(self is FireLunarShardsHurt))
      {
        Ray aimRay = self.GetAimRay();
        Transform modelChild = self.FindModelChild(FireLunarShards.muzzleString);
        if ((bool)(UnityEngine.Object)modelChild)
          aimRay.origin = modelChild.position;
        FireProjectileInfo fireProjectileInfo = new FireProjectileInfo();
        fireProjectileInfo.position = aimRay.origin;
        fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
        fireProjectileInfo.crit = self.characterBody.RollCrit();
        fireProjectileInfo.damage = self.characterBody.damage * self.damageCoefficient;
        fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
        fireProjectileInfo.owner = self.gameObject;
        fireProjectileInfo.procChainMask = new ProcChainMask();
        fireProjectileInfo.force = 0.0f;
        fireProjectileInfo.useFuseOverride = false;
        fireProjectileInfo.useSpeedOverride = false;
        fireProjectileInfo.target = (GameObject)null;
        fireProjectileInfo.projectilePrefab = FireLunarShards.projectilePrefab;
        for (int index = 0; index < ModConfig.LunarShardAdd.Value; ++index)
        {
          ProjectileManager.instance.FireProjectile(fireProjectileInfo);
          aimRay.direction = Util.ApplySpread(aimRay.direction, 0.0f, self.maxSpread * (float)(1.0 + 0.449999988079071 * (double)index), self.spreadYawScale * (float)(1.0 + 0.449999988079071 * (double)index), self.spreadPitchScale * (float)(1.0 + 0.449999988079071 * (double)index));
          fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
        }
      }
      orig(self);
    }
  }
}
