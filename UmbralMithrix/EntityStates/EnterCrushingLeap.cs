using EntityStates;
using EntityStates.BrotherMonster;
using RoR2;
using UnityEngine;

namespace UmbralMithrix
{
    public class EnterCrushingLeap : BaseSkillState
    {
        public static float baseDuration = 0.5f;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(EnterSkyLeap.soundString, this.gameObject);
            this.duration = EnterCrushingLeap.baseDuration / this.attackSpeedStat;
            this.PlayAnimation("Body", "EnterSkyLeap", "SkyLeap.playbackRate", this.duration);
            this.PlayAnimation("FullBody Override", "BufferEmpty");
            this.characterDirection.moveVector = this.characterDirection.forward;
            AimAnimator aimAnimator = this.GetAimAnimator();
            if (!(bool)aimAnimator)
                return;
            aimAnimator.enabled = true;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!this.isAuthority || (double)this.fixedAge <= this.duration)
                return;
            this.outer.SetNextState(new AimCrushingLeap());
        }
    }
}
