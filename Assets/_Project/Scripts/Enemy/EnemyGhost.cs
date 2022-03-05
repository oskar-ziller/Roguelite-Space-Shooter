using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame.Enemies
{
    public class EnemyGhost : MonoBehaviour
    {

        #region Variables
        private float fadeInDur = 0.4f;
        private float fadeOutDur = 0.5f;

        private MeshFilter meshFilter;
        private MeshRenderer renderer;
        private Enemy owner;
        private LineRenderer lineRenderer;

        private Tween alphaTween;
        private float alphaTweening = 1f; // used for tween

        private WaitForSeconds waitcached = new WaitForSeconds(5f);

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            if (renderer == null)
            {
                renderer = GetComponent<MeshRenderer>();
            }

            if (owner == null)
            {
                owner = GetComponentInParent<Enemy>();
                owner.Spawned += OnSpawn;
            }

            waitcached = new WaitForSeconds(fadeInDur);
            SetAlpha(0f);
        }

        private void OnSpawn(Enemy obj)
        {
            SetMesh(owner.Mesh);
            StartCoroutine(FadeInAndOut());
        }

        private void Start()
        {
        
        }

        private void Update()
        {
        
        }

        #endregion

        #region Methods


        private void SetAlpha(float newAlpha)
        {
            renderer.material.SetFloat("_alpha", newAlpha);
        }

        public void SetMesh(Mesh newMesh)
        {
            meshFilter.mesh = newMesh;
        }

        private Vector3[] GetLinePoints()
        {
            var toreturn = new List<Vector3>();

            var portal = owner.PortalTransform;
            var selfPos = transform.position;

            Animator a;

            Plane p = new Plane(portal.forward, Vector3.Distance(portal.transform.position, Vector3.zero));


            var startingPoint = p.ClosestPointOnPlane(selfPos);
            var dist = Vector3.Distance(startingPoint, selfPos) - (owner.Radi);
            var dir = (selfPos - startingPoint).normalized;


            //Vector3 up = owner.PortalTransform.up;
            //var randDir = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), owner.PortalTransform.forward) * up;
            //var randDist = UnityEngine.Random.Range(0, owner.PortalTransform.localScale.x * 2f);

            //Vector3 randomPos = owner.PortalTransform.position + (randDir * randDist);

            //var dist = Vector3.Distance(transform.position, randomPos);
            //dist = Mathf.Ceil(dist);

            //var dir = (transform.position - randomPos).normalized;

            for (int i = 0; i < dist / 2; i++)
            {
                toreturn.Add(startingPoint + (dir * i * 2));
            }

            return toreturn.ToArray();
        }

        private IEnumerator FadeInAndOut()
        {
            var points = GetLinePoints();
            var randomDelay = UnityEngine.Random.Range(0f, 2f);

            SetAlpha(0f);
            yield return new WaitForSeconds(randomDelay);
            StartFade(start: 0f, target: 1f, dur: fadeInDur);

            var len = lineRenderer.widthCurve.keys.Length;
            var curve = lineRenderer.widthCurve;

            Keyframe[] keys = lineRenderer.widthCurve.keys;
            Keyframe keyFrame = keys[len - 1];
            keyFrame.value = owner.Radi * 2.5f / 100f;

            curve.RemoveKey(len - 1);
            curve.AddKey(keyFrame);

            lineRenderer.widthCurve = curve;


            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.green, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.1f, 0.0f), new GradientAlphaKey(0.1f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );


            lineRenderer.colorGradient = gradient;
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);

            yield return new WaitForSeconds(0.2f);

            lineRenderer.positionCount = 0;

            yield return new WaitForSeconds(fadeInDur);
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 6f));

            owner.FadeIn(fadeOutDur * 0.6f);
            StartFade(start: 1f, target: 0f, dur: fadeOutDur);

            gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.1f, 0.0f), new GradientAlphaKey(0.1f, 0.8f), new GradientAlphaKey(0f, 1f) }
            );


            lineRenderer.colorGradient = gradient;

            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);

            yield return new WaitForSeconds(0.2f);

            lineRenderer.positionCount = 0;


        }

        internal void StartFade(float start, float target, float dur)
        {
            if (alphaTween != null && alphaTween.active)
            {
                alphaTween.Kill(complete: true);
            }

            alphaTweening = start;
            SetAlpha(start);
            alphaTween = DOTween.To(() => alphaTweening, x => alphaTweening = x, target, dur);
            alphaTween.onUpdate += StepComplete;
        }

        private void StepComplete()
        {
            SetAlpha(alphaTweening);
        }

        #endregion

    }
}
