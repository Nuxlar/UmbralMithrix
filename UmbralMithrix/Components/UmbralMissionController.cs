using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace UmbralMithrix
{
    public class UmbralMissionController : MonoBehaviour
    {
        public static UmbralMissionController instance;

        public CharacterBody currentPhaseBody;
        public CharacterBody p3CloneBody;
        public bool practiceModeEnabled = false;
        public bool spawnedClone = false;
        public bool finishedItemSteal = false;
        public bool p2ThresholdReached = false;
        public bool p3ThresholdReached = false;
        public List<GameObject> timeCrystals = new List<GameObject>();

        private void Start()
        {
            instance = this;
        }
    }
}