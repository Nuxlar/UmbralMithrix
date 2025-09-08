using RoR2.Projectile;
using UnityEngine;

namespace UmbralMithrix
{
    public class OrbDelayController : MonoBehaviour
    {
        private float delay = 3f;
        private float stopwatch = 0f;
        private ProjectileSimple projectileSimple;

        private void Start()
        {
            projectileSimple = GetComponent<ProjectileSimple>();
        }

        private void FixedUpdate()
        {
            stopwatch += Time.deltaTime;
            if (stopwatch >= delay)
            {
                projectileSimple.desiredForwardSpeed = 50f;
                Destroy(this);
            }
        }
        /*
        if (this.characterBody.name == "BrotherGlassBody(Clone)")
        {
            foreach (ProjectileController projectileController in InstanceTracker.GetInstancesList<ProjectileController>())
            {
                if (projectileController.name == "LunarWispTrackingBomb(Clone)" && projectileController.TryGetComponent(out ProjectileSimple projectileSimple))
                {
                    projectileSimple.desiredForwardSpeed = 50f;
                }
            }
        }
*/
    }
}