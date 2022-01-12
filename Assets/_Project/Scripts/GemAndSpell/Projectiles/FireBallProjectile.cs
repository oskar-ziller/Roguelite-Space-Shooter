using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace MeteorGame
{
    public class FireBallProjectile : ProjectileBase
    {
        [Tooltip("Mesh to scale to size of AoE")]
        [SerializeField] private Transform explosionMesh;

        public override void Awake()
        {
            base.Awake();
            ProjectileMover = new ArcMover(this);
        }

        public override bool HandleEnemyCollision()
        {
            collidingWith.TakeHit(CastBy);

            if (base.HandleEnemyCollision())
            {
                return true;
            }

            DoExplode();
            return false;
        }

        public void DoExplode()
        {
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.isKinematic = true;

            HideBody();
            DisableCollider();

            var totalRadi = CastBy.Spell.ExplosionRadius;

            foreach (Enemy e in EnemyManager.Instance.EnemiesInRange(transform.position, totalRadi, fromShell: true))
            {
                if (e == collidingWith)
                {
                    continue;
                }

                e.TakeHit(CastBy, applyAilment: false);
            }

            float dur = 0.25f;
            Transform mesh = SpawnExplosionMesh();

            float fadetime = 0.1f;

            var rend = mesh.GetComponent<MeshRenderer>();
            //var startAlpha = rend.material.GetFloat("_maxalpha");

            mesh.DOScale(totalRadi * 2, dur).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                Destroy(mesh.gameObject, dur);
                DestroySelfSoft();
            });

            StartCoroutine(FadeOut(rend, dur/2, dur/2));
        }

        private IEnumerator FadeOut(MeshRenderer rend, float delay, float duration)
        {
            yield return new WaitForSeconds(delay);

            float time = 0;
            float startValue = rend.material.GetFloat("_maxalpha");

            while (time < duration)
            {
                float val = Mathf.Lerp(startValue, 0, time / duration);

                if (val < 0.03f)
                {
                    rend.material.SetFloat("_maxalpha", 0);
                    yield break;
                }

                rend.material.SetFloat("_maxalpha", val);
                time += Time.deltaTime;
                yield return null;
            }

        }

        private Transform SpawnExplosionMesh()
        {
            explosionMesh.parent = null;
            explosionMesh.gameObject.SetActive(true);
            return explosionMesh;
        }
    }
}
