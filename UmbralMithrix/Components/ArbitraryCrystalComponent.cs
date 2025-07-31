using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UmbralMithrix
{
    public class ArbitraryCrystalComponent : MonoBehaviour
    {
        private BullseyeSearch bullseyeSearch;

        private void Start()
        {
            TetherVfxOrigin tetherVfxOrigin = this.gameObject.AddComponent<TetherVfxOrigin>();
            tetherVfxOrigin.tetherPrefab = UmbralMithrix.tether;

            this.bullseyeSearch = new();

            this.bullseyeSearch.searchOrigin = this.transform.position;
            this.bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
            this.bullseyeSearch.teamMaskFilter.RemoveTeam(TeamIndex.Player);
            this.bullseyeSearch.maxDistanceFilter = 200f;
            this.bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
            this.bullseyeSearch.filterByLoS = false;
            this.bullseyeSearch.filterByDistinctEntity = true;
            this.bullseyeSearch.RefreshCandidates();
            List<HurtBox> hurtBoxList = this.bullseyeSearch.GetResults().ToList();

            for (int index = 0; index < hurtBoxList.Count; ++index)
            {
                HurtBox hurtBox = hurtBoxList[index];
                if (hurtBox && hurtBox.healthComponent)
                {
                    CharacterBody body = hurtBox.healthComponent.body;
                    if (body && PhaseCounter.instance)
                    {
                        string bodyName;
                        if (NetworkServer.active)
                            bodyName = PhaseCounter.instance.phase == 2 ? "BrotherBody(Clone)" : "BrotherHurtBodyP3(Clone)";
                        else
                            bodyName = PhaseCounter.instance.phase == 3 ? "BrotherBody(Clone)" : "BrotherHurtBodyP3(Clone)";
                        if (body.name == bodyName)
                        {
                            tetherVfxOrigin.AddTether(body.transform);
                        }
                    }
                }
            }
        }
    }
}