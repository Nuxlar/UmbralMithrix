using RoR2;
using RoR2.Projectile;
using EntityStates.BrotherMonster;
using System.Collections.Generic;
using UnityEngine;

namespace UmbralMithrix
{
  public class P4Controller : MonoBehaviour
  {
    public bool finishedItemSteal = false;
    private CharacterBody body;
    private List<CharacterBody> playerBodies = new();
    private float shockwaveStopwatch = 0f;
    private float pizzaStopwatch = 0f;
    private float missileStopwatch = 0f;
    private float shockwaveInterval = 3.75f;
    private float pizzaInterval = 1.5f;
    private float missileInterval = 1.5f;

    private void Start()
    {
      this.body = this.gameObject.GetComponent<CharacterBody>();
      foreach (CharacterMaster cm in UnityEngine.Object.FindObjectsOfType<CharacterMaster>())
      {
        if (cm.teamIndex == TeamIndex.Player)
        {
          CharacterBody cb = cm.GetBody();
          if (cb && cb.isPlayerControlled)
            playerBodies.Add(cb);
        }
      }
    }

    private void FixedUpdate()
    {
      if (finishedItemSteal && this.body.healthComponent && this.body.healthComponent.alive)
      {
        this.missileStopwatch += Time.deltaTime;
        this.pizzaStopwatch += Time.deltaTime;
        this.shockwaveStopwatch += Time.deltaTime;

        if (this.missileStopwatch >= missileInterval)
        {
          this.missileStopwatch %= 1;
          int num1 = 8;
          int num2 = 8;
          Vector3 vector3_1 = (bool)this.body.inputBank ? this.body.inputBank.aimDirection : this.body.transform.forward;
          float num3 = 180f / num1;
          float num4 = (float)(3.0 + (int)this.body.radius * 1.0);
          float num5 = this.body.damage * 0.3f;
          Quaternion quaternion = Util.QuaternionSafeLookRotation(vector3_1);
          for (int index = 0; index < num1; ++index)
          {
            Vector3 vector3_2 = this.body.aimOrigin + Quaternion.AngleAxis((float)((num2 - 1) * (double)num3 - (double)num3 * (num1 - 1) / 2.0), vector3_1) * Vector3.up * num4;
            ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
            {
              projectilePrefab = UmbralMithrix.lunarMissile,
              position = vector3_2,
              rotation = quaternion,
              owner = this.gameObject,
              damage = num5,
              force = 100f
            });
            --num2;
          }
        }
      }
      if (this.pizzaStopwatch >= pizzaInterval)
      {
        this.pizzaStopwatch %= 1;
        Vector3 vector3_3 = Vector3.ProjectOnPlane(this.body.inputBank.aimDirection, Vector3.up);
        float num = 40f;
        Vector3 vector3_4 = new Vector3(UnityEngine.Random.Range(-45, -25), 0.0f, UnityEngine.Random.Range(-45, -25));
        if ((double)UnityEngine.Random.value <= 0.5)
          vector3_4 = new Vector3(UnityEngine.Random.Range(25, 45), 0.0f, UnityEngine.Random.Range(25, 45));
        GameObject prefab = UmbralMithrix.leftP4Line;
        if ((double)UnityEngine.Random.value <= 0.5)
          prefab = UmbralMithrix.rightP4Line;
        Vector3 footPosition = playerBodies[UnityEngine.Random.Range(0, playerBodies.Count)].footPosition with
        {
          y = 491f
        };
        footPosition += vector3_4;
        for (int index2 = 0; index2 < 9; ++index2)
        {
          Vector3 forward = Quaternion.AngleAxis(num * index2, Vector3.up) * vector3_3;
          ProjectileManager.instance.FireProjectile(prefab, footPosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.body.damage * (UltChannelState.waveProjectileDamageCoefficient * 0.25f), UltChannelState.waveProjectileForce / 8f, Util.CheckRoll(this.body.crit, this.body.master));
        }
      }
      if (this.shockwaveStopwatch >= shockwaveInterval)
      {
        this.shockwaveStopwatch %= 1;
        int num6 = (int)Util.PlaySound(ExitSkyLeap.soundString, this.gameObject);
        float num7 = 360f / (float)ExitSkyLeap.waveProjectileCount;
        Vector3 vector3 = Vector3.ProjectOnPlane(this.body.inputBank.aimDirection, Vector3.up);
        Vector3 footPosition = this.body.footPosition;
        for (int index = 0; index < ExitSkyLeap.waveProjectileCount; ++index)
        {
          Vector3 forward = Quaternion.AngleAxis(num7 * (float)index, Vector3.up) * vector3;
          ProjectileManager.instance.FireProjectile(ExitSkyLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.body.damage * (ExitSkyLeap.waveProjectileDamageCoefficient * 0.5f), ExitSkyLeap.waveProjectileForce / 4f, Util.CheckRoll(this.body.crit, this.body.master));
        }
      }
    }
  }
}