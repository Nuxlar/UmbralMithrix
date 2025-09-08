using EntityStates;
using EntityStates.BrotherMonster;
using EntityStates.LunarWisp;
using RoR2;

namespace UmbralMithrix.EntityStates
{
    public class GlassSpawnState : BaseState
    {
        public static float duration;
        public static string spawnSoundString;
        public static float spawnEffectsDelay;
        private bool spawnEffectsTriggered;

        public override void OnEnter()
        {
            base.OnEnter();

            Util.PlaySound(SpawnState.spawnSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= SpawnState.spawnEffectsDelay && !spawnEffectsTriggered)
            {
                spawnEffectsTriggered = true;
                EffectManager.SimpleMuzzleFlash(SpawnState.spawnEffectPrefab, gameObject, "MuzzleCenter", false);
            }

            if (fixedAge >= SpawnState.duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
