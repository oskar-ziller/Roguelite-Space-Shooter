using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace MeteorGame
{
    public class InventoryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {

        [Tooltip("TMP object of name")]
        [SerializeField] private TextMeshProUGUI nameTMP;

        [Tooltip("TooltipTrigger attached to this inventory item so that we can fill gem info")]
        [SerializeField] private TooltipTrigger trigger;

        public GameObject circleShape, rectShape;
        public Image circleIcon, rectIcon;

        public Color defaultTextColor;
        public Color enterTextColor;

        private RectTransform rectTransform;

        private GemItem gem;
        private TabMenuManager tabMenuManager;


        internal void Init(GemItem g, RectTransform holder)
        {
            gem = g;
            UpdateUI();
            SetParent(holder);
            tabMenuManager = GetComponentInParent<TabMenuManager>();
        }

        private void UpdateUI()
        {
            UpdateTexts();
            UpdateIconColor();
            DisableUnusedShape();
        }

        internal void SetParent(RectTransform holder)
        {
            rectTransform.SetParent(holder);
            rectTransform.localScale = Vector3.one;
        }

        private void DisableUnusedShape()
        {
            if (gem.HasSpell)
            {
                circleShape.gameObject.SetActive(false);
            }
            else
            {
                rectShape.gameObject.SetActive(false);
            }
        }

        private void UpdateIconColor()
        {
            circleIcon.color = gem.Color;
            rectIcon.color = gem.Color;
        }

        private void UpdateTexts()
        {
            nameTMP.text = gem.Name;
            trigger.SetupGemInfoTooltip(gem);
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                OnLeftClick();
            else if (eventData.button == PointerEventData.InputButton.Middle)
                OnMiddleClick();
            else if (eventData.button == PointerEventData.InputButton.Right)
                OnRightClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            nameTMP.color = enterTextColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            nameTMP.color = defaultTextColor;
        }

        private void OnLeftClick()
        {
            StartCoroutine(ChangeColorOnClick());
            TryEquip(1);
        }

        private void OnRightClick()
        {
            StartCoroutine(ChangeColorOnClick());
            TryEquip(2);
        }

        private void OnMiddleClick()
        {
            StartCoroutine(ChangeColorOnClick());
            TryEquip(3);
        }

        IEnumerator ChangeColorOnClick()
        {
            nameTMP.color = defaultTextColor;
            yield return new WaitForSecondsRealtime(0.1f);
            nameTMP.color = enterTextColor;
        }

        private void TryEquip(int slot)
        {
            var s = Player.Instance.SpellSlot(slot);

            if (!s.IsUnlocked)
            {
                tabMenuManager.DisplayError(UIError.SpellSlotNeedsUnlock);
                return;
            }

            if (!gem.HasSpell)
            {
                if (s.CanLinkMore)
                {
                    s.Equip(gem);
                }
                else
                {
                    tabMenuManager.DisplayError(UIError.CantLink);
                }
            }
            else
            {
                s.Equip(gem);
            }
        }
    }
}
