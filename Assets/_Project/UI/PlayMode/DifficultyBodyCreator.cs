using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace MeteorGame
{
    public class DifficultyBodyCreator : MonoBehaviour
    {

        #region Variables

        public Transform holder;

        public float w, h;

        public TextMeshProUGUI textPrefab;

        public Sprite normalBar;
        public Sprite checkpointBar;

        #endregion

        #region Unity Methods

        private void Awake()
        {

        }

        private void Start()
        {



            float totalX = 0f;

            for (int level = 0; level < GameManager.Instance.MaxGameLevel; level++)
            {
                var im = CreateImageObject(level);
                //im.anchoredPosition = new Vector2(totalX, 0);
                //im.pivot = new Vector2(0, 0.5f);
                //totalX += im.sizeDelta.x + 5f;
            }





        }


        private void Update()
        {

        
        }

        #endregion

        #region Methods

        void ResetSizeAndPosition(RectTransform rt)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector3.zero;
            rt.sizeDelta = new Vector2(w, h);
        }


        public Gradient gradient;

        RectTransform CreateImageObject(int level)
        {
            GameObject barHolder = new GameObject(level.ToString());
            var br = barHolder.AddComponent<RectTransform>();

            //ContentSizeFitter f = barHolder.AddComponent<ContentSizeFitter>();
            //f.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textObj = Instantiate(textPrefab);
            textObj.text = level.ToString();

            GameObject imageObj = new GameObject(); 
            Image im = imageObj.AddComponent<Image>();
            im.sprite = normalBar; 

            im.color = gradient.Evaluate(level / GameManager.Instance.MaxGameLevel);

            if (level % GameManager.Instance.checkpointLevelInterval == 0 && level > 0)
            {
                //im.color = Color.white;
                im.sprite = checkpointBar;
            }

            RectTransform rt = imageObj.GetComponent<RectTransform>();

            

            imageObj.SetActive(true); 
            ResetSizeAndPosition(rt);
            Resize(rt, level);


            imageObj.transform.SetParent(barHolder.transform);
            textObj.transform.SetParent(barHolder.transform);
            barHolder.transform.SetParent(holder);

            br.localScale = Vector3.one;
            br.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y);
            br.anchoredPosition = Vector3.zero;

            return rt;
        }

        int debugTotal = 0;


        RectTransform Resize(RectTransform r, int level)
        {
            int widthPerSecond = 5;

            int secondsToNextLevel = 0;

            var minsToMax = GameManager.Instance.minutesToHitMaxGameLevel;
            var secondsToMax = minsToMax * 60;
            var maxLevel = GameManager.Instance.MaxGameLevel;
            var curve = GameManager.Instance.DifficultyCurve;


            for (int i = 1; i < secondsToMax; i++)
            {
                var secondsPercentage = (float)(i / secondsToMax);
                var eval = Mathf.Floor(curve.Evaluate(secondsPercentage) * maxLevel);


                if (eval < level)
                {
                    continue;
                }

                if (eval > level)
                {
                    break;
                }


                secondsToNextLevel++;
                debugTotal++;
            }

            r.sizeDelta = new Vector2((secondsToNextLevel * widthPerSecond) - 5, h);


            //var minutePercentagePrev = (float)((index - 1) / GameManager.Instance.minutesToHitMaxGameLevel);
            //var minutePercentage = (float)(index / GameManager.Instance.minutesToHitMaxGameLevel);

            //var evalPrev = GameManager.Instance.difficultyCurve.Evaluate(minutePercentagePrev) * GameManager.Instance.maxGameLevel;
            //var eval = GameManager.Instance.difficultyCurve.Evaluate(minutePercentage) * GameManager.Instance.maxGameLevel;

            //var evalDiff = eval - evalPrev;

            //float result = w / evalDiff;
            ////eval = Mathf.Ceil(eval);
            //print($"i: {index} - eval: {eval} - evalDiff: {evalDiff} - width: {result}");

            //r.sizeDelta = new Vector2(result, h);
            return r;
        }


        #endregion

    }
}
