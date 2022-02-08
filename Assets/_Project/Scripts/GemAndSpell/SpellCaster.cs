using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
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
        private Dictionary<int, List<ProjectileDummy>> dummyDict = new Dictionary<int, List<ProjectileDummy>>();

        private Dictionary<int, List<ProjectileBase>> projDict = new Dictionary<int, List<ProjectileBase>>();

        private Dictionary<int, Transform> holderDict = new Dictionary<int, Transform>();

        //public float scaleDur = 0.4f;


        WandAnim wandAnim1, wandAnim2;

        private void Awake()
        {
            Player.Instance.SpellSlot(1).SpellChanged += OnSpellChanged;
            Player.Instance.SpellSlot(1).GemLinkedOrRemoved += OnGemAddedRemoved;

            Player.Instance.SpellSlot(2).SpellChanged += OnSpellChanged;
            Player.Instance.SpellSlot(2).GemLinkedOrRemoved += OnGemAddedRemoved;


            var wandAnims = Player.Instance.GetComponentsInChildren<WandAnim>().ToList();

            wandAnim1 = wandAnims.First(w => w.belongsToSlot == 1);
            wandAnim2 = wandAnims.First(w => w.belongsToSlot == 2);

            dummyDict.Add(1, new List<ProjectileDummy>());
            dummyDict.Add(2, new List<ProjectileDummy>());


            projDict.Add(1, new List<ProjectileBase>());
            projDict.Add(2, new List<ProjectileBase>());


            holderDict.Add(1, dummyHolder1);
            holderDict.Add(2, dummyHolder2);


            Instance = this;
        }

        private void OnGemAddedRemoved(SpellSlot slot, GemItem gem)
        {
            OnSpellChanged(slot, slot.Spell);
        }


        private static void ClearDummyDict(int slotNo)
        {
            for (int i = Instance.dummyDict[slotNo].Count - 1; i >= 0; i--)
            {
                Destroy(Instance.dummyDict[slotNo][i].gameObject);
            }

            Instance.dummyDict[slotNo].Clear();
        }

        private void OnSpellChanged(SpellSlot slot, SpellItem spell)
        {
            ClearDummyDict(slot.slotNo);
            

            if (spell != null)
            {
                SpawnDummies(slot);
                ScaleDummies(slot, 1f);
            }

        }

        private static void SpawnDummies(SpellSlot slot)
        {
            List<ProjectileDummy> dummies = Instance.dummyDict[slot.slotNo];

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

            var dummies = Instance.dummyDict[slot.slotNo];

            var calculatedScale = Helper.Map(dummies.Count, 1, Instance.dummyHalfScaleProjectileCount, 1, 0.5f);

            foreach (var item in dummies)
            {
                item.transform.DOScale(calculatedScale, dur).SetUpdate(true);
                //item.transform.DOScale(1f/ dummies.Count, dur);
            }
        }

        private static ProjectileDummy SpawnDummy(SpellSlot slot)
        {
            var parent = Instance.holderDict[slot.slotNo];
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

            var dur = (spell.MsBetweenCasts + spell.CastTimeMs) / 1000f;

            if (slot.slotNo == 1)
            {
                Instance.wandAnim1.Shoot(dur);
            }
            else
            {
                Instance.wandAnim2.Shoot(dur);
            }


            SpawnProjectilesFromDummes(slot);

            MoveProjectiles(slot.slotNo);
            ClearProjectilesDict(slot.slotNo);

            spell.Cast();
            Instance.castID++;

            ClearDummyDict(slot.slotNo);
            SpawnDummies(slot);
            ScaleDummies(slot, dur);
        }




        private static void ClearProjectilesDict(int slotNo)
        {
            Instance.projDict[slotNo].Clear();
        }

        private static void MoveProjectiles(int slotNo)
        {
            var list = Instance.projDict[slotNo];

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
            List<ProjectileDummy> dummies = Instance.dummyDict[slot.slotNo];

            if (dummies.Count == 0)
            {
                return;
            }

            var projList = Instance.projDict[slot.slotNo];
            var aimingAt = Player.Instance.AimingAt(out var hitEnemy);

            for (int i = 0; i < dummies.Count; i++)
            {
                var dummy = dummies[i];

                var p = Instance.SpawnProjectile(slot);
                p.transform.position = dummy.transform.position;
                p.transform.localScale = dummy.transform.localScale;
                //p.ScaleDur = Instance.scaleDur;
                p.Setup(slot, aimingAt, hitEnemy, Instance.castID, i, Instance.holderDict[slot.slotNo].position);
                //p.ScaleDur = (50f * 8f) / p.StartingSpeed;
                //p.ScaleDur = 4f;
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
