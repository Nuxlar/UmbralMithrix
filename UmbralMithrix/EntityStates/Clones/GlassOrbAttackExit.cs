using EntityStates;
using EntityStates.LunarWisp;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace UmbralMithrix.EntityStates
{
    public class GlassOrbAttackExit : BaseState
    {
        public static float duration = 1f;

        private static int exitStateHash = Animator.StringToHash("UltExit");
        private static int exitParamHash = Animator.StringToHash("Ult.playbackRate");

        public override void OnEnter()
        {
            base.OnEnter();
            this.PlayAnimation("Body", GlassOrbAttackExit.exitStateHash, GlassOrbAttackExit.exitParamHash, GlassOrbAttackExit.duration);

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if ((double)this.fixedAge <= GlassOrbAttackExit.duration)
                return;
            this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
