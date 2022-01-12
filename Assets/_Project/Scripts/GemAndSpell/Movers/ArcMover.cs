using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class ArcMover : IMover
    {
        private ProjectileBase _moveable;

        public ArcMover(ProjectileBase obj)
        {
            _moveable = obj;
        }

        public void Move()
        {
            List<Vector3> path = new List<Vector3>();
            var rigidBody = _moveable.Rigidbody;
            var projDir = (_moveable.MovingTowards - rigidBody.position).normalized;

            var dist = Vector3.Distance(_moveable.MovingTowards, rigidBody.position);

            if (dist > 40)
            {
                dist = 40;
            }

            if (dist < 9)
            {
                OnTweenEnded();
                return;
            }

            float totalLength = 0f;

            var prevPos = rigidBody.position;

            var total = _moveable.TotalProjectiles;
            var current = _moveable.ProjectileID;

            int curveCount = UnityEngine.Random.Range(1, 3);
            var len = dist / (curveCount * 2 * UnityEngine.Random.Range(0.7f, 1.4f));

            var angle = UnityEngine.Random.Range(12f, 25f);
            var randomAxisAngle = UnityEngine.Random.Range(0f, 360f);

            for (int i = 0; i < curveCount; i++)
            {
                if (i % 2 == 0)
                {
                    angle = -angle;
                }

                // rotate transform.right randomly and get a random direction on that plane
                Vector3 customAxis = Quaternion.AngleAxis(randomAxisAngle, rigidBody.transform.forward) * rigidBody.transform.right;

                // rotate angle degrees around our original direction, using customAxis as our rotation axis
                Vector3 dirToUse = Quaternion.AngleAxis(angle, customAxis) * rigidBody.transform.forward;
                Vector3 dirToUseBack = Quaternion.AngleAxis(-angle, customAxis) * rigidBody.transform.forward;

                var randomExtraLength = UnityEngine.Random.value * 2;

                if (_moveable.AimingAtEnemy != null) // dont randomly spread if we are aiming at enemy
                {
                    randomExtraLength = 0f;
                }

                Vector3 pos = prevPos + (len / 2 * dirToUse * (1+ randomExtraLength));
                path.Add(pos);

                Vector3 pos2 = pos + len/2 * dirToUseBack;
                path.Add(pos2);

                totalLength += Vector3.Distance(prevPos, pos);
                totalLength += Vector3.Distance(pos, pos2);

                prevPos = pos2;
            }


            if (path.Count > 0)
            {
                rigidBody.isKinematic = true;
                rigidBody.DOPath(path.ToArray(), totalLength/(_moveable.StartingSpeed * 2), PathType.CatmullRom, PathMode.Ignore, 2).SetEase(Ease.Linear).onComplete += OnTweenEnded;
            }
            else
            {
                OnTweenEnded();
            }

        }

        private void OnTweenEnded()
        {
            // move ball towards direction of movement
            _moveable.Rigidbody.isKinematic = false;
            _moveable.Rigidbody.velocity = Vector3.zero;

            //var origDir = (_moveable.MovingTowards - _moveable.RigidBodyObj.position).normalized;
            _moveable.Rigidbody.velocity = _moveable.Rigidbody.transform.forward * _moveable.StartingSpeed;
        }


    }
}
