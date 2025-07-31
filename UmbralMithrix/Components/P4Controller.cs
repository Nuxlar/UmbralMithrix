using EntityStates.BrotherMonster;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UmbralMithrix
{
    public class P4Controller : MonoBehaviour
    {
        public bool finishedItemSteal = false;
        private CharacterBody body;
        private float shockwaveStopwatch = 0f;
        private float pizzaStopwatch = 0f;
        private float missileStopwatch = 0f;
        private float shockwaveInterval = 3.75f;
        private float pizzaInterval = 1.5f;
        private float missileInterval = 1.25f;

        readonly List<CharacterBody> _trackedPlayers = new List<CharacterBody>();

        private void Start()
        {
            body = GetComponent<CharacterBody>();
        }

        void OnEnable()
        {
            foreach (PlayerCharacterMasterController playerMasterController in PlayerCharacterMasterController.instances)
            {
                if (playerMasterController && playerMasterController.isConnected && playerMasterController.master)
                {
                    CharacterBody playerBody = playerMasterController.master.GetBody();

                    if (playerBody)
                    {
                        handlePlayerBody(playerBody);
                    }
                }
            }

            CharacterBody.onBodyStartGlobal += onBodyStartGlobal;
            CharacterBody.onBodyDestroyGlobal += onBodyDestroyGlobal;
        }

        void OnDisable()
        {
            CharacterBody.onBodyStartGlobal -= onBodyStartGlobal;
            CharacterBody.onBodyDestroyGlobal -= onBodyDestroyGlobal;

            foreach (CharacterBody playerBody in _trackedPlayers)
            {
                playerBody.RemoveBuff(RoR2Content.Buffs.TeamWarCry);
            }

            _trackedPlayers.Clear();
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active)
                return;

            if (finishedItemSteal && body.healthComponent && body.healthComponent.alive)
            {
                missileStopwatch += Time.deltaTime;
                pizzaStopwatch += Time.deltaTime;
                shockwaveStopwatch += Time.deltaTime;

                if (missileStopwatch >= missileInterval)
                {
                    missileStopwatch %= 1;
                    int num1 = 8;
                    int num2 = 8;
                    Vector3 vector3_1 = body.inputBank ? body.inputBank.aimDirection : body.transform.forward;
                    float num3 = 180f / num1;
                    float num4 = (float)(3.0 + ((int)body.radius * 1.0));
                    float num5 = body.damage * 0.5f;
                    Quaternion quaternion = Util.QuaternionSafeLookRotation(vector3_1);
                    for (int index = 0; index < num1; ++index)
                    {
                        Vector3 vector3_2 = body.aimOrigin + Quaternion.AngleAxis((float)(((num2 - 1) * (double)num3) - ((double)num3 * (num1 - 1) / 2.0)), vector3_1) * Vector3.up * num4;
                        ProjectileManager.instance.FireProjectile(new FireProjectileInfo()
                        {
                            projectilePrefab = UmbralMithrix.lunarMissile,
                            position = vector3_2,
                            rotation = quaternion,
                            owner = gameObject,
                            damage = num5,
                            force = 100f
                        });
                        --num2;
                    }
                }
            }

            if (pizzaStopwatch >= pizzaInterval)
            {
                pizzaStopwatch %= 1;
                Vector3 vector3_3 = Vector3.ProjectOnPlane(body.inputBank.aimDirection, Vector3.up);
                float num = 40f;
                Vector3 vector3_4 = new Vector3(UnityEngine.Random.Range(-45, -25), 0.0f, UnityEngine.Random.Range(-45, -25));
                if ((double)UnityEngine.Random.value <= 0.5)
                    vector3_4 = new Vector3(UnityEngine.Random.Range(25, 45), 0.0f, UnityEngine.Random.Range(25, 45));
                GameObject prefab = UmbralMithrix.leftP4Line;
                if ((double)UnityEngine.Random.value <= 0.5)
                    prefab = UmbralMithrix.rightP4Line;

                Vector3 pizzaSpawnPosition = body.footPosition;
                if (_trackedPlayers.Count > 0)
                {
                    pizzaSpawnPosition = _trackedPlayers[UnityEngine.Random.Range(0, _trackedPlayers.Count)].footPosition with
                    {
                        y = 491f
                    };
                }

                pizzaSpawnPosition += vector3_4;
                for (int index2 = 0; index2 < 9; ++index2)
                {
                    Vector3 forward = Quaternion.AngleAxis(num * index2, Vector3.up) * vector3_3;
                    ProjectileManager.instance.FireProjectile(prefab, pizzaSpawnPosition, Util.QuaternionSafeLookRotation(forward), gameObject, body.damage * (UltChannelState.waveProjectileDamageCoefficient * 0.5f), UltChannelState.waveProjectileForce / 8f, Util.CheckRoll(body.crit, body.master));
                }
            }

            if (shockwaveStopwatch >= shockwaveInterval)
            {
                shockwaveStopwatch %= 1;
                int num6 = (int)Util.PlaySound(ExitSkyLeap.soundString, gameObject);
                float num7 = 360f / ExitSkyLeap.waveProjectileCount;
                Vector3 vector3 = Vector3.ProjectOnPlane(body.inputBank.aimDirection, Vector3.up);
                Vector3 footPosition = body.footPosition;
                for (int index = 0; index < ExitSkyLeap.waveProjectileCount; ++index)
                {
                    Vector3 forward = Quaternion.AngleAxis(num7 * index, Vector3.up) * vector3;
                    ProjectileManager.instance.FireProjectile(ExitSkyLeap.waveProjectilePrefab, footPosition, Util.QuaternionSafeLookRotation(forward), gameObject, body.damage * (ExitSkyLeap.waveProjectileDamageCoefficient * 0.75f), ExitSkyLeap.waveProjectileForce / 4f, Util.CheckRoll(body.crit, body.master));
                }
            }
        }

        void onBodyStartGlobal(CharacterBody body)
        {
            if (body.isPlayerControlled)
            {
                handlePlayerBody(body);
            }
        }

        void onBodyDestroyGlobal(CharacterBody body)
        {
            _trackedPlayers.Remove(body);
        }

        void handlePlayerBody(CharacterBody playerBody)
        {
            if (!_trackedPlayers.Contains(playerBody))
            {
                _trackedPlayers.Add(playerBody);
                if (NetworkServer.active)
                    playerBody.AddBuff(RoR2Content.Buffs.TeamWarCry);
            }
        }
    }
}