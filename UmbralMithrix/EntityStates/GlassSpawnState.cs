using EntityStates;
using EntityStates.LunarWisp;
using RoR2;

namespace UmbralMithrix
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
      Util.PlaySound(SpawnState.spawnSoundString, this.gameObject);
    }

    public override void FixedUpdate()
    {
      base.FixedUpdate();
      if ((double)this.fixedAge >= SpawnState.spawnEffectsDelay && !this.spawnEffectsTriggered)
      {
        this.spawnEffectsTriggered = true;
        EffectManager.SimpleMuzzleFlash(SpawnState.spawnEffectPrefab, this.gameObject, "MuzzleCenter", false);
      }
      if ((double)this.fixedAge < SpawnState.duration || !this.isAuthority)
        return;
      this.outer.SetNextStateToMain();
    }

    public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Death;
  }
}
