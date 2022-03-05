using DG.Tweening;
using MeteorGame.Enemies;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace MeteorGame
{

    public class ProjectileSpawnInfo
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
        private List<ChillingArea> creepingFrostChillingAreas = new List<ChillingArea>();
        private int castID = 0;

        // have a list for 2 slots and combine into dictionary
        private Dictionary<SpellSlot, List<ProjectileDummy>> dummyDict = new Dictionary<SpellSlot, List<ProjectileDummy>>();
        private Dictionary<SpellSlot, List<ProjectileBase>> projDict = new Dictionary<SpellSlot, List<ProjectileBase>>();
        private Dictionary<SpellSlot, Transform> holderDict = new Dictionary<SpellSlot, Transform>();


        public Dictionary<SpellSO, ObjectPool<ProjectileBase>> projPool = new Dictionary<SpellSO, ObjectPool<ProjectileBase>>();
        public Dictionary<SpellSO, ObjectPool<ProjectileDummy>> dummyPool = new Dictionary<SpellSO, ObjectPool<ProjectileDummy>>();
        public Dictionary<SpellSO, ObjectPool<Explosion>> explosionPool = new Dictionary<SpellSO, ObjectPool<Explosion>>();


        WandAnim wandAnim1, wandAnim2;

        private void Awake()
        {


            Instance = this;
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
        }

        private void OnGemAddedRemoved(SpellSlot slot, GemItem gem)
        {
            OnSpellChanged(slot, slot.Spell);
        }


        private static void ClearDummyDict(SpellSlot slot)
        {
            for (int i = Instance.dummyDict[slot].Count - 1; i >= 0; i--)
            {
                Destroy(Instance.dummyDict[slot][i].gameObject);
            }

            Instance.dummyDict[slot].Clear();
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

        private static void SpawnDummies(SpellSlot slot)
        {
            List<ProjectileDummy> dummies = Instance.dummyDict[slot];

            // starts at this and rotates counter clockwise as a whole group
            float randomStartDeg = Random.Range(0, 350);

            for (int i = 0; i < slot.ProjectileCount; i++)
            {
                var dummy = SpawnDummy(slot);
                dummy.SetSpinnerVals(i, slot.ProjectileCount, randomStartDeg);
                dummies.Add(dummy);
            }
        }

        [Tooltip("After how many projectileCount the dummy scale should be halved")]
        public float dummyHalfScaleProjectileCount = 8f;

        /// <summary>
        /// Scales dummies at start from 0->1
        /// </summary>
        /// <param name="slot"></param>
        private static void ScaleDummies(SpellSlot slot, float dur)
        {
            var dummies = Instance.dummyDict[slot];

            var calculatedScale = Helper.Map(dummies.Count, 1, Instance.dummyHalfScaleProjectileCount, 1, 0.5f);

            foreach (var item in dummies)
            {
                item.transform.DOScale(calculatedScale, dur).SetUpdate(true);
                //item.transform.DOScale(1f/ dummies.Count, dur);
            }
        }

        private static ProjectileDummy SpawnDummy(SpellSlot slot)
        {
            var parent = Instance.holderDict[slot];
            var d = Instantiate(slot.Spell.dummyPrefab);
            d.transform.SetParent(parent);
            d.transform.localPosition = Vector3.zero;
            d.transform.localScale = Vector3.zero;

            return d;
        }

        public static void Cast(SpellSlot slot)
        {
            if (GameManager.Instance.IsGamePaused)
            {
                return;
            }

            var spell = slot.Spell;

            if (spell == null || !spell.CanCast())
            {
                return;
            }

            var dur = slot.Modifiers.CastTimeCalcd;

            if (slot.slotNo == 1)
            {
                Instance.wandAnim1.Shoot(dur);
            }
            else
            {
                Instance.wandAnim2.Shoot(dur);
            }


            SpawnProjectilesFromDummes(slot);
            MoveProjectiles(slot);
            ClearProjectilesDict(slot);

            spell.Cast();
            Instance.castID++;

            ClearDummyDict(slot);
            SpawnDummies(slot);
            ScaleDummies(slot, dur);
        }




        private static void ClearProjectilesDict(SpellSlot slot)
        {
            Instance.projDict[slot].Clear();
        }

        private static void MoveProjectiles(SpellSlot slot)
        {
            var list = Instance.projDict[slot];

            foreach (var proj in list)
            {
                proj.Move();
            }
        }


        private ProjectileBase SpawnProjectile(SpellSlot slot)
        {
            var parent = Instance.projectileHolder;
            var p = Instantiate(slot.Spell.projPrefab);
            p.transform.SetParent(parent);

            return p;
        }


        private static void SpawnProjectilesFromDummes(SpellSlot slot)
        {
            List<ProjectileDummy> dummies = Instance.dummyDict[slot];

            if (dummies.Count == 0)
            {
                return;
            }

            var projList = Instance.projDict[slot];
            var aimingAt = Player.Instance.AimingAt(out var hitEnemy);

            for (int i = 0; i < dummies.Count; i++)
            {
                var dummy = dummies[i];

                var p = Instance.SpawnProjectile(slot);
                p.transform.position = dummy.transform.position;
                p.transform.localScale = dummy.transform.localScale;

                var info = new ProjectileSpawnInfo();
                info.CastBy = slot;

                info.AimingAt = aimingAt;
                info.HitEnemy = hitEnemy;

                info.CastID = Instance.castID;
                info.ProjID = i;

                info.CastPos = Instance.holderDict[slot].position;

                p.Setup(info);

                projList.Add(p);
            }
        }



        #region chillingareastuff

        public static void AddCreepingFrostChillingArea(ChillingArea area)
        {
            Instance.creepingFrostChillingAreas.Add(area);
        }

        public static int CreepingFrostChillingAreaCount()
        {
            return Instance.creepingFrostChillingAreas.Count;
        }

        public static ChillingArea GetOldestCreepingFrostChillingArea()
        {
            return Instance.creepingFrostChillingAreas[0];
        }

        public static void RemoveCreepingFrostChillingArea(ChillingArea a)
        {
            Instance.creepingFrostChillingAreas.Remove(a);
        }

        #endregion
    }
}
