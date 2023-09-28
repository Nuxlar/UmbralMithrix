using RoR2;
using UnityEngine;

namespace UmbralMithrix
{
  public class DeathZoneController : MonoBehaviour
  {
    private SphereZone zone;
    private float stopwatch = 0f;
    private float interval = 1f;
    private float zoneRadius = 100f;

    private void Start()
    {
      this.zone = this.gameObject.GetComponent<SphereZone>();
    }

    private void FixedUpdate()
    {
      this.stopwatch += Time.deltaTime;
      if ((double)this.stopwatch < this.interval)
        return;
      this.stopwatch %= this.interval;
      if ((double)this.zone.Networkradius > zoneRadius)
        this.zone.Networkradius -= 6f;
    }

  }
}