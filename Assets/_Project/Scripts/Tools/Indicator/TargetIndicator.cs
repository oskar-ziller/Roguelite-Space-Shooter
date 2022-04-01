using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MeteorGame
{
    public class TargetIndicator : MonoBehaviour
    {
        [SerializeField] private Image targetIndicatorPrefab;
        //[SerializeField] private Image coreIndicatorPrefab;
        [SerializeField] private Camera cam;

        private RectTransform rt;
        private Dictionary<EnemyPack, RectTransform> indicators;

        //private Image coreIndicator;
        //private RectTransform coreIndRect;

        float outOfSightOffest = 20;


        void Start()
        {
            indicators = new Dictionary<EnemyPack, RectTransform>();
            rt = GetComponent<RectTransform>();

            //coreIndicator = Instantiate(coreIndicatorPrefab);
            //coreIndicator.transform.SetParent(transform, true);

            //coreIndRect = coreIndicator.GetComponent<RectTransform>();
        }

        void Update()
        {
            var packs = EnemyManager.Instance.AlivePacks;

            foreach (var e in packs)
            {
                if (!indicators.ContainsKey(e) && !e.IsVisible)
                {
                    CreateIndicator(e);
                }
            }

            UpdateIndicators();
        }


        private List<EnemyPack> toRemove = new();

        private void UpdateIndicators()
        {
            // check if we need to remove any
            foreach (var item in indicators)
            {
                if (item.Key.IsVisible || !EnemyManager.Instance.AlivePacks.Contains(item.Key))
                {
                    toRemove.Add(item.Key);
                    Destroy(item.Value.gameObject);
                }
            }

            // remove
            foreach (var item in toRemove)
            {
                indicators.Remove(item);
            }

            toRemove.Clear();

            // update remaining
            foreach (var item in indicators)
            {
                SetIndicatorPos(item.Key.Position, item.Value);
            }

            // update core
            //SetIndicatorPos(Vector3.zero, coreIndRect);
        }



        private void CreateIndicator(EnemyPack e)
        {
            var ind = Instantiate(targetIndicatorPrefab);
            ind.transform.SetParent(transform, true);

            indicators.Add(e, ind.GetComponent<RectTransform>());
        }


        private void SetIndicatorPos(Vector3 worldPos, RectTransform im)
        {
            //Get the position of the target in relation to the screenSpace 
            Vector3 indicatorPosition = cam.WorldToScreenPoint(worldPos);
            //Debug.Log("GO: "+ gameObject.name + "; slPos: " + indicatorPosition + "; cvWidt: " + canvasRect.rect.width + "; cvHeight: " + canvasRect.rect.height);

            //In case the target is both in front of the camera and within the bounds of its frustrum
            if (indicatorPosition.z >= 0f & indicatorPosition.x <= rt.rect.width * rt.localScale.x
             & indicatorPosition.y <= rt.rect.height * rt.localScale.x & indicatorPosition.x >= 0f & indicatorPosition.y >= 0f)
            {

                //Set z to zero since it's not needed and only causes issues (too far away from Camera to be shown!)
                indicatorPosition.z = 0f;
            }

            //In case the target is in front of the cam, but out of sight
            else if (indicatorPosition.z >= 0f)
            {
                //Set indicatorposition and set targetIndicator to outOfSight form.
                indicatorPosition = OutOfRangeindicatorPositionB(indicatorPosition);
            }
            else
            {
                //Invert indicatorPosition! Otherwise the indicator's positioning will invert if the target is on the backside of the camera!
                indicatorPosition *= -1f;

                //Set indicatorposition and set targetIndicator to outOfSight form.
                indicatorPosition = OutOfRangeindicatorPositionB(indicatorPosition);

            }

            //Set the position of the indicator
            im.position = indicatorPosition;
        }


        //protected void SetIndicatorPosition(EnemyPack target, RectTransform im)
        //{

        //    if (target.Enemies.First().IsVisible())
        //    {
        //        indicators.Remove(target);
        //        Destroy(im.gameObject);
        //        return;
        //    }


        //    //Get the position of the target in relation to the screenSpace 
        //    Vector3 indicatorPosition = cam.WorldToScreenPoint(target.Position);
        //    //Debug.Log("GO: "+ gameObject.name + "; slPos: " + indicatorPosition + "; cvWidt: " + canvasRect.rect.width + "; cvHeight: " + canvasRect.rect.height);

        //    //In case the target is both in front of the camera and within the bounds of its frustrum
        //    if (indicatorPosition.z >= 0f & indicatorPosition.x <= rt.rect.width * rt.localScale.x
        //     & indicatorPosition.y <= rt.rect.height * rt.localScale.x & indicatorPosition.x >= 0f & indicatorPosition.y >= 0f)
        //    {

        //        //Set z to zero since it's not needed and only causes issues (too far away from Camera to be shown!)
        //        indicatorPosition.z = 0f;
        //    }

        //    //In case the target is in front of the ship, but out of sight
        //    else if (indicatorPosition.z >= 0f)
        //    {
        //        //Set indicatorposition and set targetIndicator to outOfSight form.
        //        indicatorPosition = OutOfRangeindicatorPositionB(indicatorPosition);
        //    }
        //    else
        //    {
        //        //Invert indicatorPosition! Otherwise the indicator's positioning will invert if the target is on the backside of the camera!
        //        indicatorPosition *= -1f;

        //        //Set indicatorposition and set targetIndicator to outOfSight form.
        //        indicatorPosition = OutOfRangeindicatorPositionB(indicatorPosition);

        //    }

        //    //Set the position of the indicator
        //    im.position = indicatorPosition;
        //}




        private Vector3 OutOfRangeindicatorPositionB(Vector3 indicatorPosition)
        {
            //Set indicatorPosition.z to 0f; We don't need that and it'll actually cause issues if it's outside the camera range
            indicatorPosition.z = 0f;

            //Calculate Center of Canvas and subtract from the indicator position to have indicatorCoordinates from the Canvas Center instead the bottom left!
            Vector3 canvasCenter = new Vector3(rt.rect.width / 2f, rt.rect.height / 2f, 0f) * rt.localScale.x;
            indicatorPosition -= canvasCenter;

            //Calculate if Vector to target intersects (first) with y border of canvas rect or if Vector intersects (first) with x border:
            //This is required to see which border needs to be set to the max value and at which border the indicator needs to be
            //moved (up & down or left & right)
            float divX = (rt.rect.width / 2f - outOfSightOffest) / Mathf.Abs(indicatorPosition.x);
            float divY = (rt.rect.height / 2f - outOfSightOffest) / Mathf.Abs(indicatorPosition.y);

            //In case it intersects with x border first, put the x-one to the border and adjust the y-one accordingly (Trigonometry)
            if (divX < divY)
            {
                float angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
                indicatorPosition.x = Mathf.Sign(indicatorPosition.x) * (rt.rect.width * 0.5f - outOfSightOffest) * rt.localScale.x;
                indicatorPosition.y = Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.x;
            }

            //In case it intersects with y border first, put the y-one to the border and adjust the x-one accordingly (Trigonometry)
            else
            {
                float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition, Vector3.forward);

                indicatorPosition.y = Mathf.Sign(indicatorPosition.y) * (rt.rect.height / 2f - outOfSightOffest) * rt.localScale.y;
                indicatorPosition.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.y;
            }

            //Change the indicator Position back to the actual rectTransform coordinate system and return indicatorPosition
            indicatorPosition += canvasCenter;
            return indicatorPosition;
        }








    }
}
