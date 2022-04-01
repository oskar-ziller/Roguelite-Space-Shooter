using DG.Tweening;
using MeteorGame.Enemies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace MeteorGame
{

    [Serializable]
    public struct ProjectileSpawnInfo
    {
        //public void Setup(SpellSlot castBySlot, Vector3 aimingAt, Enemy hitEnemy, int castID, int projectileID, Vector3 castPos)
        public SpellSlot CastBy;
        public Vector3 AimingAt;
        public Enemy HitEnemy;
        public int CastID;
        public int ProjID;
        public Vector3 CastPos;
    }

    public class SpellCaster : MonoBehaviour
    {
        public static SpellCaster Instance { get; private set; }

        [Tooltip("Transform to hold the new projectile until move")]
        [SerializeField] private Transform dummyHolder1, dummyHolder2;

        [Tooltip("Transform to hold the new projectile after move")]
        [SerializeField] private Transform projectileHolder;

        // keep track of chilling areas so we can remove oldest when we reach spell limit
        private List<ChillingArea> creepingFrostChillingAreas = new();
        private int castID = 0;

        [Tooltip("After how many projectileCount the dummy scale should be halved")]
        private float dummyHalfScaleProjectileCount = 8f;

        [Tooltip("Dummy scale max")]
        [SerializeField] private float dummyScaleMax = 1f;

        [Tooltip("Dummy scale min")]
        [SerializeField] private float dummyScaleMin = 0.5f;

        // have a list for 2 slots and combine into dictionary
        private Dictionary<SpellSlot, List<ProjectileDummy>> dummyDict = new();
        private Dictionary<SpellSlot, List<ProjectileBase>> projDict = new();
        private Dictionary<SpellSlot, Transform> holderDict = new();

        private Dictionary<SpellSO, ObjectPool<ProjectileBase>> projPool = new();
        private Dictionary<SpellSO, ObjectPool<ProjectileDummy>> dummyPool = new();
        private Dictionary<SpellSO, ObjectPool<Explosion>> explosionPool = new();


        WandAnim wandAnim1, wandAnim2;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance.IsGamePaused)
            {
                return;
            }

            if (Input.GetKey(KeyCode.Mouse0))
            {
                Player.Instance.SpellSlot(1).Cast();
            }

            if (Input.GetKey(KeyCode.Mouse1))
            {
                Player.Instance.SpellSlot(2).Cast();
            }
        }

        public void Setup()
        {
            SpellSlot slot1 = Player.Instance.SpellSlot(1);
            SpellSlot slot2 = Player.Instance.SpellSlot(2);

            slot1.SpellChanged += OnSpellChanged;
            slot1.GemLinkedOrRemoved += OnGemAddedRemoved;

            slot2.SpellChanged += OnSpellChanged;
            slot2.GemLinkedOrRemoved += OnGemAddedRemoved;


            var wandAnims = Player.Instance.GetComponentsInChildren<WandAnim>().ToList();

            wandAnim1 = wandAnims.First(w => w.belongsToSlot == 1);
            wandAnim2 = wandAnims.First(w => w.belongsToSlot == 2);

            dummyDict.Add(slot1, new List<ProjectileDummy>());
            dummyDict.Add(slot2, new List<ProjectileDummy>());

            projDict.Add(slot1, new List<ProjectileBase>());
            projDict.Add(slot2, new List<ProjectileBase>());

            holderDict.Add(slot1, dummyHolder1);
            holderDict.Add(slot2, dummyHolder2);


            SpawnDummies(slot1);
            ScaleDummies(slot1, 0.4f);
        }

        private void OnGemAddedRemoved(SpellSlot slot, GemItem gem)
        {
            OnSpellChanged(slot, slot.Spell);
        }


        private void ClearDummyDict(SpellSlot slot)
        {
            for (int i = dummyDict[slot].Count - 1; i >= 0; i--)
            {
                Destroy(dummyDict[slot][i].gameObject);
            }

            dummyDict[slot].Clear();
        }

        private void OnSpellChanged(SpellSlot slot, SpellItem spell)
        {
            ClearDummyDict(slot);
            

            if (spell != null)
            {
                SpawnDummies(slot);
                ScaleDummies(slot, 1f);
            }

        }

        private void SpawnDummies(SpellSlot slot)
        {
            List<ProjectileDummy> dummies = dummyDict[slot];

            // starts at this and rotates counter clockwise as a whole group
            float randomStartDeg = UnityEngine.Random.Range(0, 360);

            for (int i = 0; i < slot.ProjectileCount; i++)
            {
                var dummy = SpawnDummy(slot);
                dummy.SetSpinnerVals(i, slot.ProjectileCount, randomStartDeg);
                dummies.Add(dummy);
            }
        }


        private float CalculateDummyScale(int dummyCount)
        {
            switch (dummyCount)
            {
                case 1:
                    return 0.1f;
                case 2:
                    return 0.08f;
                case 3:
                    return 0.08f;
                case 4:
                    return 0.07f;
                case 5:
                    return 0.06f;
                case 6:
                    return 0.05f;
                case 7:
                    return 0.05f;
                case 8:
                    return 0.04f;
                case 9:
                    return 0.04f;
                case 10:
                    return 0.03f;
                case 11:
                    return 0.03f;
                case 12:
                    return 0.02f;
                default:
                    return 0.02f;
            }
        }


        /// <summary>
        /// Scales dummies after spawning. The more dummies the smaller they scale to. 
        /// </summary>
        /// <param name="slot"></param>
        private void ScaleDummies(SpellSlot slot, float dur)
        {
            var dummies = dummyDict[slot];

            var calculatedScale = CalculateDummyScale(dummies.Count);

            foreach (var item in dummies)
            {
                item.transform.DOScale(calculatedScale, dur);
            }
        }

        private ProjectileDummy SpawnDummy(SpellSlot slot)
        {
            var parent = holderDict[slot];
            var dummy = Instantiate(slot.Spell.dummyPrefab);
            dummy.transform.SetParent(parent);
            dummy.transform.localPosition = Vector3.zero;
            dummy.transform.localScale = Vector3.zero;

            return dummy;
        }

        public void Cast(SpellSlot slot)
        {
            var spell = slot.Spell;

            if (spell == null || !spell.CanCast())
            {
                return;
            }

            var dur = slot.Modifiers.CastTimeCalcd;

            if (slot.slotNo == 1)
            {
                wandAnim1.Shoot(dur);
            }
            else
            {
                wandAnim2.Shoot(dur);
            }

            SpawnProjectilesFromDummes(slot);
            MoveProjectiles(slot);
            ClearProjectilesDict(slot);

            spell.Cast();
            castID++;

            ClearDummyDict(slot);
            SpawnDummies(slot);
            ScaleDummies(slot, dur);
        }




        private void ClearProjectilesDict(SpellSlot slot)
        {
            projDict[slot].Clear();
        }

        private void MoveProjectiles(SpellSlot slot)
        {
            var list = projDict[slot];

            foreach (var proj in list)
            {
                proj.Move();
            }
        }


        private ProjectileBase SpawnProjectile(SpellSlot slot)
        {
            var parent = projectileHolder;
            var p = Instantiate(slot.Spell.projPrefab);
            p.transform.SetParent(parent);

            return p;
        }


        private void SpawnProjectilesFromDummes(SpellSlot slot)
        {
            List<ProjectileDummy> dummies = dummyDict[slot];

            if (dummies.Count == 0)
            {
                return;
            }

            List<ProjectileBase> projList = projDict[slot];
            Vector3 aimingAt = Player.Instance.AimingAt(out Enemy hitEnemy);

            for (int i = 0; i < dummies.Count; i++)
            {
                var dummy = dummies[i];

                var p = SpawnProjectile(slot);
                p.transform.position = dummy.transform.position;
                p.transform.localScale = dummy.transform.localScale;

                var info = new ProjectileSpawnInfo
                {
                    CastBy = slot,

                    AimingAt = aimingAt,
                    HitEnemy = hitEnemy,

                    CastID = castID,
                    ProjID = i,

                    CastPos = holderDict[slot].position
                };

                p.Setup(info);

                projList.Add(p);
            }
        }



        #region chillingareastuff

        public void AddCreepingFrostChillingArea(ChillingArea area)
        {
            creepingFrostChillingAreas.Add(area);
        }

        public int CreepingFrostChillingAreaCount()
        {
            return creepingFrostChillingAreas.Count;
        }

        public ChillingArea GetOldestCreepingFrostChillingArea()
        {
            return creepingFrostChillingAreas[0];
        }

        public void RemoveCreepingFrostChillingArea(ChillingArea a)
        {
            creepingFrostChillingAreas.Remove(a);
        }

        #endregion
    }
}
