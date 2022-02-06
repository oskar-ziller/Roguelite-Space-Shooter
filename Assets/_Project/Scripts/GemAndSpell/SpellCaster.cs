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

        [SerializeField] private ProjectileBase fireballProjectile;
        [SerializeField] private ProjectileBase ballLightningProjectile;
        [SerializeField] private ProjectileBase creepingFrostProjectile;

        [Tooltip("Transform to hold the new projectile until move")]
        [SerializeField] private Transform playerProjectileHolder1, playerProjectileHolder2;

        [Tooltip("Transform to hold the new projectile after move")]
        [SerializeField] private Transform projectileHolder;

        // keep track of chilling areas so we can remove oldest when we reach spell limit
        private List<ChillingArea> creepingFrostChillingAreas = new List<ChillingArea>();
        private int castID = 0;

        private List<ProjectileBase> dummyProjectiles = new List<ProjectileBase>();
        private List<ProjectileBase> dummyProjectiles2 = new List<ProjectileBase>();

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

            Instance = this;
        }

        private void OnGemAddedRemoved(SpellSlot slot, GemItem gem)
        {
            OnSpellChanged(slot, null);
        }

        private void OnSpellChanged(SpellSlot slot, SpellItem spell)
        {
            List<ProjectileBase> listToUse;

            if (slot.slotNo == 1)
            {
                listToUse = dummyProjectiles;
            }
            else
            {
                listToUse = dummyProjectiles2;
            }

            if (listToUse.Count > 0)
            {
                foreach (var item in listToUse)
                {
                    Destroy(item.gameObject);
                }

                listToUse.Clear();
            }

            if (slot.Spell != null) // spell is set to null when unequipped
            {
                SpawnDummiesWithEffect(slot);
            }

        }

        private void SpawnDummies(SpellSlot spellSlot)
        {
            List<ProjectileBase> listToUse;

            if (spellSlot.slotNo == 1)
            {
                listToUse = dummyProjectiles;
            }
            else
            {
                listToUse = dummyProjectiles2;
            }

            for (int i = 0; i < spellSlot.ProjectileCount; i++)
            {
                var dummy = SpawnProjectile(spellSlot);
                
                if (spellSlot.slotNo == 1)
                {
                    dummy.transform.SetParent(playerProjectileHolder1);
                }

                if (spellSlot.slotNo == 2)
                {
                    dummy.transform.SetParent(playerProjectileHolder2);
                }

                dummy.SetCastBy(spellSlot);
                dummy.MakeDummy();

                listToUse.Add(dummy);
                dummy.SetProjectileID(i);
                //dummy.EnableSpinner();
            }
        }

        private ProjectileBase SpawnProjectile(SpellSlot castBy)
        {
            Vector3 spawnPos;
            
            if (castBy.slotNo == 1)
            {
                spawnPos = playerProjectileHolder1.position;
            }
            else
            {
                spawnPos = playerProjectileHolder2.position;
            }

            ProjectileBase spellProjectile = null;

            if (castBy.Spell.Name == "Fireball")
            {
                spellProjectile = fireballProjectile;
            }

            if (castBy.Spell.Name == "BallLightning")
            {
                spellProjectile = ballLightningProjectile;
            }

            if (castBy.Spell.Name == "CreepingFrost")
            {
                spellProjectile = creepingFrostProjectile;
            }

            if (spellProjectile == null)
            {
                throw new System.Exception("something went wrong spawning spell projectile");
            }

            return Instantiate(spellProjectile, spawnPos, Quaternion.identity);
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

            var rand = UnityEngine.Random.Range(0, 360);
            var aimingAt = Player.Instance.AimingAt(out var hitEnemy);


            List<ProjectileBase> listToUse;

            var dur = (spell.MsBetweenCasts + spell.CastTimeMs) / 1000f;

            if (slot.slotNo == 1)
            {
                listToUse = Instance.dummyProjectiles;
                Instance.wandAnim1.Shoot(dur);
            }
            else
            {
                listToUse = Instance.dummyProjectiles2;
                Instance.wandAnim2.Shoot(dur);
            }

            for (int i = 0; i < listToUse.Count; i++)
            {
                var proj = listToUse[i];
                proj.MakeNormal();
                proj.transform.SetParent(Instance.projectileHolder);
                proj.Setup(slot, aimingAt, hitEnemy, Instance.castID, i);
                proj.transform.Rotate(new Vector3(0, 0, rand), Space.Self);
                proj.Move();
            }

            listToUse.Clear();

            spell.Cast();
            Instance.castID++;

            Instance.SpawnDummiesWithEffect(slot);
        }


        private void SpawnDummiesWithEffect(SpellSlot spellSlot)
        {
            var spell = spellSlot.Spell;
            var castTime = spell.CastTimeMs / 1000f;
            var msBetween = spell.MsBetweenCasts / 1000f;
            var dur = castTime + msBetween;

            SpawnDummies(spellSlot);

            //List<ProjectileBase> listToUse;

            //if (spellSlot.slotNo == 1)
            //{
            //    listToUse = dummyProjectiles;
            //}
            //else
            //{
            //    listToUse = dummyProjectiles2;
            //}

            ////var defaultScale = 0.15f;
            ////var minScale = 0.01f;
            ////var maxCount = 8f;
            ////var currCount = listToUse.Count;

            ////foreach (var item in listToUse)
            ////{
            ////    item.transform.localScale = Vector3.zero;

            ////    var scale = defaultScale - (currCount * ((defaultScale - minScale) / maxCount));

            ////    item.transform.DOScale(scale, dur);
            //}
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
