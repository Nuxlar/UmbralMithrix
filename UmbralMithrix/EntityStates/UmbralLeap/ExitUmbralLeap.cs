using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using UnityEngine;
using EntityStates;
using EntityStates.BrotherMonster;

namespace UmbralMithrix.EntityStates;

public class ExitUmbralLeap : BaseState
{
    public static float baseDuration = 3f;
    public static string soundString = "Play_moonBrother_phaseJump_land_impact";
    public static GameObject waveProjectilePrefab = UmbralMithrix.umbralLeapWave;
    public static int waveProjectileCount = ModConfig.JumpWaveCount.Value;
    public static float waveProjectileDamageCoefficient = 4f;
    public static float waveProjectileForce = 400f;
    // public static SkillDef replacementSkillDef;
    private float duration;
    private static int ExitUmbralLeapStateHash = Animator.StringToHash(nameof(ExitSkyLeap));
    private static int BufferEmptyStateHash = Animator.StringToHash("BufferEmpty");
    private static int SkyLeapParamHash = Animator.StringToHash("SkyLeap.playbackRate");

    public override void OnEnter()
    {
        base.OnEnter();
        this.duration = ExitUmbralLeap.baseDuration / this.attackSpeedStat;
        Util.PlaySound(ExitUmbralLeap.soundString, this.gameObject);
        this.PlayAnimation("Body", ExitUmbralLeap.ExitUmbralLeapStateHash, ExitUmbralLeap.SkyLeapParamHash, this.duration);
        this.PlayAnimation("FullBody Override", ExitUmbralLeap.BufferEmptyStateHash);
        this.characterBody.AddTimedBuff(RoR2Content.Buffs.ArmorBoost, ExitUmbralLeap.baseDuration);
        AimAnimator aimAnimator = this.GetAimAnimator();
        if ((bool)aimAnimator)
            aimAnimator.enabled = true;
        if (this.isAuthority)
            this.FireRingAuthority();
        if (PhaseCounter.instance && PhaseCounter.instance.phase == 1)
            return;
        /* saving for later
    for (int index = 0; index < ExitUmbralLeap.cloneCount; ++index)
    {
        SpawnCard spawnCard = LegacyResourcesAPI.Load<SpawnCard>("SpawnCards/CharacterSpawnCards/cscBrotherGlass");
        DirectorPlacementRule placementRule = new DirectorPlacementRule();
        placementRule.placementMode = DirectorPlacementRule.PlacementMode.Approximate;
        placementRule.minDistance = 3f;
        placementRule.maxDistance = 20f;
        placementRule.spawnOnTarget = this.gameObject.transform;
        Xoroshiro128Plus rng = RoR2Application.rng;
        DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(spawnCard, placementRule, rng);
        directorSpawnRequest.summonerBodyObject = this.gameObject;
        directorSpawnRequest.onSpawnedServer += spawnResult => spawnResult.spawnedInstance.GetComponent<Inventory>().GiveItem(RoR2Content.Items.HealthDecay, ExitUmbralLeap.cloneDuration);
        DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
    }
    */
        GenericSkill special = (bool)this.skillLocator ? this.skillLocator.special : null;
        if (!(bool)special)
            return;

        special.SetSkillOverride(this.outer, ChannelUmbralUlt.replacementSkillDef, GenericSkill.SkillOverridePriority.Contextual);
    }

    private void FireRingAuthority()
    {
        float num = 360f / ExitUmbralLeap.waveProjectileCount;
        Vector3 vector3 = Vector3.ProjectOnPlane(this.inputBank.aimDirection, Vector3.up);
        Vector3 footPosition = this.characterBody.footPosition;
        for (int index = 0; index < ExitUmbralLeap.waveProjectileCount; ++index)
        {
            Vector3 forward = Quaternion.AngleAxis(num * index, Vector3.up) * vector3;
            if (this.isAuthority)
                ProjectileManager.instance.FireProjectileWithoutDamageType(ExitUmbralLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), this.gameObject, this.characterBody.damage * ExitUmbralLeap.waveProjectileDamageCoefficient, ExitUmbralLeap.waveProjectileForce, Util.CheckRoll(this.characterBody.crit, this.characterBody.master));
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!this.isAuthority)
            return;
        if ((double)this.fixedAge <= (double)this.duration)
            return;
        this.outer.SetNextStateToMain();
    }
}
