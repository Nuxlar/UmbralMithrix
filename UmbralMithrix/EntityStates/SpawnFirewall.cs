using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using EntityStates;
using EntityStates.BrotherMonster;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

namespace UmbralMithrix
{
  public class SpawnFirewall : EntityStates.BaseState
  {
    public static float baseDuration = 3.5f;
    public static float damageCoefficient = 4f;
    public static float forceMagnitude = 16f;
    public static float upwardForce;
    public static float radius = 3f;
    public static string attackSoundString;
    public static string muzzleString;
    public static float healthCostFraction;
    public static GameObject chargeEffectPrefab;
    public static GameObject slamImpactEffect;
    public static GameObject waveProjectilePrefab;
    public static int waveProjectileCount;
    public static float waveProjectileDamageCoefficient;
    public static float waveProjectileForce;
    private BlastAttack attack;
    private Animator modelAnimator;
    private Transform modelTransform;
    private bool hasAttacked;
    private float duration;
    private GameObject chargeInstance;
    private GameObject indicatorPrefab = UmbralMithrix.p4Indicator;
    private List<GameObject> indicatorInstances = new();

    public override void OnEnter()
    {
      base.OnEnter();
      this.modelAnimator = this.GetModelAnimator();
      this.modelTransform = this.GetModelTransform();
      this.duration = FistSlam.baseDuration / this.attackSpeedStat;
      Util.PlayAttackSpeedSound(FistSlam.attackSoundString, this.gameObject, this.attackSpeedStat);
      // RoR2/DLC1/VoidRaidCrab/MultiBeamRayIndicator.prefab
      // RoR2/Base/Mage/FirewallAreaIndicator.prefab
      this.PlayCrossfade("FullBody Override", nameof(FistSlam), "FistSlam.playbackRate", this.duration, 0.1f);
      if ((bool)this.characterDirection)
        this.characterDirection.moveVector = this.characterDirection.forward;
      if ((bool)this.modelTransform)
      {
        AimAnimator component = this.modelTransform.GetComponent<AimAnimator>();
        if ((bool)component)
          component.enabled = true;
      }
      Transform modelChild = this.FindModelChild("MuzzleRight");
      if (!(bool)modelChild || !(bool)FistSlam.chargeEffectPrefab)
        return;
      if ((bool)(Object)this.modelTransform)
      {
        Transform fistMuzzle = this.FindModelChild(FistSlam.muzzleString);
        if ((bool)(Object)fistMuzzle)
        {
          int num1 = 8;
          float num2 = 360f / 8;
          Vector3 normalized = Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, Vector3.up).normalized;
          GameObject prefab = this.indicatorPrefab;
          for (int index2 = 0; index2 < num1; ++index2)
          {
            Vector3 forward = Quaternion.AngleAxis(num2 * index2, Vector3.up) * normalized;
            GameObject instance = GameObject.Instantiate(prefab, fistMuzzle.position, Quaternion.LookRotation(forward));
            indicatorInstances.Add(instance);
            instance.transform.GetChild(0).gameObject.SetActive(false);
            RayAttackIndicator indicator = instance.GetComponent<RayAttackIndicator>();
            indicator.attackRange = 500f;
            indicator.attackRay = new Ray(fistMuzzle.position, forward);
          }
        }
      }
      this.chargeInstance = Object.Instantiate<GameObject>(FistSlam.chargeEffectPrefab, modelChild.position, modelChild.rotation);
      this.chargeInstance.transform.parent = modelChild;
      ScaleParticleSystemDuration component1 = this.chargeInstance.GetComponent<ScaleParticleSystemDuration>();
      if (!(bool)component1)
        return;
      component1.newDuration = this.duration / 2.8f;
    }

    public override void OnExit()
    {
      if ((bool)this.chargeInstance)
        EntityState.Destroy(this.chargeInstance);
      this.PlayAnimation("FullBody Override", "BufferEmpty");
      this.characterBody.gameObject.GetComponent<P4Controller>().spawnedWall = true;
      base.OnExit();
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((bool)this.modelAnimator && (double)this.modelAnimator.GetFloat("fist.hitBoxActive") > 0.5 && !this.hasAttacked)
      {
        if ((bool)this.chargeInstance)
          EntityState.Destroy(this.chargeInstance);
        if (this.indicatorInstances.Count > 0)
        {
          foreach (GameObject indicatorInstance in this.indicatorInstances)
            EntityState.Destroy(indicatorInstance);
          this.indicatorInstances.Clear();
        }
        EffectManager.SimpleMuzzleFlash(FistSlam.slamImpactEffect, this.gameObject, FistSlam.muzzleString, false);
        if (this.isAuthority)
        {
          if ((bool)this.modelTransform)
          {
            Transform modelChild = this.FindModelChild(FistSlam.muzzleString);
            if ((bool)modelChild)
            {
              int num1 = 8;
              float num2 = 360f / 8;
              Vector3 normalized = Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, Vector3.up).normalized;
              GameObject prefab = UmbralMithrix.fireWall;
              for (int index2 = 0; index2 < num1; ++index2)
              {
                Vector3 forward = Quaternion.AngleAxis(num2 * index2, Vector3.up) * normalized;
                ProjectileManager.instance.FireProjectile(prefab, modelChild.position, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.characterBody.damage * 3, 0, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
              }
            }
          }
        }
        this.hasAttacked = true;
      }
      if ((double)this.fixedAge < (double)this.duration || !this.isAuthority)
        return;
      this.outer.SetNextStateToMain();
    }
  }
}