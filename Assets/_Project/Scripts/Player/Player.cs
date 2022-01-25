using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeteorGame
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }

        #region Variables
        
        [SerializeField] private Inventory inventory;

        public Inventory Inv => inventory;

        public float currency = 0;

        [SerializeField] private SpellSlot spellSlot1 = new SpellSlot(1);
        [SerializeField] private SpellSlot spellSlot2 = new SpellSlot(2);

        private Tween currencyTween;
        private Camera cameraObj;


        public Animator shootTest;



        #endregion

        #region Unity Methods

        public SpellSlot SpellSlot(int i)
        {
            if (i == 1)
            {
                return spellSlot1;
            }

            if (i == 2)
            {
                return spellSlot2;
            }

            return null;
        }

        private void Awake()
        {
            spellSlot1.UnlockSpellSlot();
            spellSlot1.IncreaseMaxLinks();

            inventory = new Inventory();
            //inventory.GemAdded += OnGemAddedToInventory;
            Instance = this;

            cameraObj = GetComponentInChildren<Camera>();

            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 144;

            spellSlot1.GemLinkedOrRemoved += OnGemAddedOrRemoved;
            spellSlot1.SpellChanged += OnSpellChanged;

            spellSlot2.GemLinkedOrRemoved += OnGemAddedOrRemoved;
            spellSlot2.SpellChanged += OnSpellChanged;
        }

        private void OnEnable()
        {
            Instance = this;
        }



        private void DebugAddAllGemsToInv()
        {
            foreach (GemSO gemSO in GameManager.Instance.AllGems.All)
            {
                GemItem gem = new GemItem(gemSO, level: 0);
                inventory.AddGem(gem);
            }
        }

        private void OnSpellChanged(SpellSlot slot, SpellItem spell)
        {
            GameManager.Instance.TabMenuManager.RebuildInventoryUI();
        }

        private void OnGemAddedOrRemoved(SpellSlot slot, GemItem gem)
        {
            GameManager.Instance.TabMenuManager.RebuildInventoryUI();
        }

        private void Start()
        {
            DebugAddAllGemsToInv();

            //GameManager.Instance.test();

            //AddLinked(inventory.gems.First());
            //AddLinked(inventory.gems.Last());

            //SetActiveSpell(inventory.Spells.First(s => s.Name == "Fireball"));
            //AddLinked(inventory.gems.First(g => g.gemSO.name == "Greater Multiple Projectiles"));

            spellSlot1.Equip(inventory.Spells.First(s => s.Name == "Fireball").Gem);
        }

        private void Update()
        {


        }


        #endregion

        #region Methods

        internal void CollidedWithDroppedGold(GoldCoinDrop goldCoinDrop)
        {
            ChangeCurrency(goldCoinDrop.amount);
        }

        public Vector3 AimingAt(out Enemy hitEnemy)
        {
            hitEnemy = null;
            Vector3 destination;

            int layer_mask = LayerMask.GetMask(new string[]{"Enemies", "Arena"});

            Ray ray = cameraObj.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, 900f, layer_mask))
            {
                if (hit.transform.gameObject.TryGetComponent<Enemy>(out var e))
                {
                    hitEnemy = e;
                }

                destination = hit.point;
            }
            else
            {
                destination = ray.GetPoint(1000);
            }

            return destination;
        }


        private void PlayGoldPickup()
        {

        }

        internal void ChangeCurrency(int amount)
        {
            if (currencyTween != null && currencyTween.active)
            {
                currencyTween.Complete();
                currencyTween.Kill();
            }

            var curr = currency;
            var target = curr + amount;
            currencyTween = DOTween.To(() => currency, x => currency = x, target, 0.5f).SetUpdate(true);
        }

        public bool CanAfford(int amount)
        {
            return currency >= amount;
        }

        #endregion
    }
}
