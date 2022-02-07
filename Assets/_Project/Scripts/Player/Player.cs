using DG.Tweening;
using System;
using System.Collections;
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
        public float TweeningCurrency => currencyTweening;

        private float currencyTweening = 0; // used to tween and display currency

        [SerializeField] private float currency = 0;
        [SerializeField] private SpellSlot spellSlot1 = new SpellSlot(1);
        [SerializeField] private SpellSlot spellSlot2 = new SpellSlot(2);

        private Tween currencyTween;
        private Camera cameraObj;

        private bool isSetup = false;

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
            //QualitySettings.vSyncCount = 1;
            //Application.targetFrameRate = 144;

            Instance = this;

        }

        private void Start()
        {
            Setup();
        }


        private void Setup()
        {
            inventory = new Inventory();
            cameraObj = GetComponentInChildren<Camera>();

            spellSlot1.GemLinkedOrRemoved += OnGemAddedOrRemoved;
            spellSlot1.SpellChanged += OnSpellChanged;

            spellSlot2.GemLinkedOrRemoved += OnGemAddedOrRemoved;
            spellSlot2.SpellChanged += OnSpellChanged;

            DebugAddAllGemsToInv();
            spellSlot1.UnlockSpellSlot();
            spellSlot1.IncreaseMaxLinks();
            spellSlot1.Equip(inventory.Spells.First(s => s.Name == "Fireball").Gem);

            //GameManager.Instance.TabMenuManager.RebuildTabMenu();

            currencyTweening = currency;

            isSetup = true;
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
            if (isSetup)
            {
                //GameManager.Instance.TabMenuManager.RebuildTabMenu();
            }
        }

        private void OnGemAddedOrRemoved(SpellSlot slot, GemItem gem)
        {
            if (isSetup)
            {
                //GameManager.Instance.TabMenuManager.RebuildTabMenu();
            }
        }

        #endregion

        #region Methods

        private float pitch = 1f;
        Coroutine pitchResetCoroutine;
        private float pitchResetDurInSeconds = 1.5f;

        IEnumerator PitchResetCoroutine()
        {
            yield return new WaitForSeconds(pitchResetDurInSeconds);
            pitch = 1f;
        }

        internal void CollidedWithDroppedGold(GoldCoinDrop goldCoinDrop)
        {
            ChangeCurrency(goldCoinDrop.goldAmount);
            goldCoinDrop.PlayAuidoWithPitch(pitch);
            pitch += 0.05f;

            if (pitchResetCoroutine != null)
            {
                StopCoroutine(pitchResetCoroutine);
            }

            pitchResetCoroutine = StartCoroutine(PitchResetCoroutine());
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
            currency = target;
            currencyTween = DOTween.To(() => currencyTweening, x => currencyTweening = x, target, 0.5f).SetUpdate(true);
        }

        public bool CanAfford(int amount)
        {
            return currency >= amount;
        }



        #endregion
    }
}
