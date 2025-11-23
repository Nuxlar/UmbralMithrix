using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using EntityStates;
using System.Collections.Generic;

namespace UmbralMithrix.EntityStates;

public class ChannelUmbralUlt : BaseState
{
    public static GameObject waveProjectileLeftPrefab = UmbralMithrix.leftUltLine;
    public static GameObject waveProjectileRightPrefab = UmbralMithrix.rightUltLine;
    public static int waveProjectileCount = ModConfig.UltimateWaves.Value;
    public static float waveProjectileDamageCoefficient = 9f;
    public static float waveProjectileForce = 300f;
    public static int totalWaves = ModConfig.UltimateCount.Value;
    public static float maxDuration = ModConfig.UltimateDuration.Value;
    public static GameObject channelBeginMuzzleflashEffectPrefab = UmbralMithrix.umbralUltMuzzleFlash;
    public static GameObject channelEffectPrefab;
    public static string enterSoundString = "Play_moonBrother_blueWall_active_loop";
    public static string exitSoundString = "Stop_moonBrother_blueWall_active_loop";
    private GameObject channelEffectInstance;
    public static SkillDef replacementSkillDef = UmbralMithrix.ultDef;
    private int wavesFired;

    public override void OnEnter()
    {
        base.OnEnter();
        Util.PlaySound(ChannelUmbralUlt.enterSoundString, this.gameObject);
        Transform modelChild = this.FindModelChild("MuzzleUlt");
        if (modelChild && ChannelUmbralUlt.channelEffectPrefab)
            this.channelEffectInstance = Object.Instantiate(ChannelUmbralUlt.channelEffectPrefab, modelChild.position, Quaternion.identity, modelChild);
        if (!ChannelUmbralUlt.channelBeginMuzzleflashEffectPrefab)
            return;
        EffectManager.SimpleMuzzleFlash(ChannelUmbralUlt.channelBeginMuzzleflashEffectPrefab, this.gameObject, "MuzzleUlt", false);
    }

    private void FireWave()
    {
        ++this.wavesFired;

        float num = 360f / ModConfig.UltimateWaves.Value;
        Vector3 normalized;
        GameObject prefab;

        if (PhaseCounter.instance)
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

            if (PhaseCounter.instance.phase == 2 && playerBodies.Count > 0)
            {
                float distance = 50f;

                Vector3 vector3 = Vector3.ProjectOnPlane(this.inputBank.aimDirection, Vector3.up);
                Vector3 center = playerBodies[Random.Range(0, playerBodies.Count)].footPosition with
                {
                    y = 491f
                };

                Vector3 point1 = center + new Vector3(-distance, 0f, -distance);
                Vector3 point2 = center + new Vector3(distance, 0f, -distance);
                Vector3 point3 = center + new Vector3(-distance, 0f, distance);
                Vector3 point4 = center + new Vector3(distance, 0f, distance);
                Vector3[] points = [point1, point2, point3, point4];

                for (int idx = 0; idx < 4; ++idx)
                {
                    float offset = Random.Range(-10f, 10f);

                    for (int index = 0; index < ModConfig.UltimateWaves.Value; ++index)
                    {
                        Vector3 forward = Quaternion.AngleAxis((num + offset) * index, Vector3.up) * vector3;
                        ProjectileManager.instance.FireProjectile(UmbralMithrix.staticUltLine, points[idx], Util.QuaternionSafeLookRotation(forward), this.gameObject, this.characterBody.damage * ChannelUmbralUlt.waveProjectileDamageCoefficient, ChannelUmbralUlt.waveProjectileForce, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
                    }
                }
            }

            if (PhaseCounter.instance.phase == 3)
            {
                int count = playerBodies.Count;
                int num1 = ModConfig.UltimateWaves.Value;
                float num2 = 360f / num1;
                normalized = Vector3.ProjectOnPlane(UnityEngine.Random.onUnitSphere, Vector3.up).normalized;
                prefab = UmbralMithrix.leftUltLine;
                if (UnityEngine.Random.value <= 0.5)
                    prefab = UmbralMithrix.rightUltLine;

                Vector3 center = playerBodies[Random.Range(0, playerBodies.Count)].footPosition with
                {
                    y = 491f
                };
                Vector3[] vector3Array = [
                    new Vector3(center.x, this.characterBody.footPosition.y, center.z) + new Vector3(UnityEngine.Random.Range(-45f, -15f), 0.0f, UnityEngine.Random.Range(-45f, -15f)),
                        new Vector3(center.x, this.characterBody.footPosition.y, center.z) + new Vector3(UnityEngine.Random.Range(15f, 45f), 0.0f, UnityEngine.Random.Range(15f, 45f))
                ];

                for (int index1 = 0; index1 < 2; ++index1)
                {
                    for (int index2 = 0; index2 < num1; ++index2)
                    {
                        Vector3 forward = Quaternion.AngleAxis(num2 * index2, Vector3.up) * normalized;
                        ProjectileManager.instance.FireProjectile(prefab, vector3Array[index1], Util.QuaternionSafeLookRotation(forward), this.gameObject, this.characterBody.damage * ChannelUmbralUlt.waveProjectileDamageCoefficient, ChannelUmbralUlt.waveProjectileForce, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
                    }
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!this.isAuthority)
            return;
        if (Mathf.CeilToInt(this.fixedAge / ChannelUmbralUlt.maxDuration * ChannelUmbralUlt.totalWaves) > this.wavesFired)
            this.FireWave();
        if ((double)this.fixedAge <= ChannelUmbralUlt.maxDuration)
            return;
        this.outer.SetNextState(new ExitUmbralUlt());
    }

    public override void OnExit()
    {
        Util.PlaySound(ChannelUmbralUlt.exitSoundString, this.gameObject);
        if ((bool)this.channelEffectInstance)
            EntityState.Destroy(this.channelEffectInstance);
        base.OnExit();
    }
}
