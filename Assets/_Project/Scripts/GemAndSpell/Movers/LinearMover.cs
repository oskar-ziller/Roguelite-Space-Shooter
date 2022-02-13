using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class LinearMover : IMover
    {
        private ProjectileBase _moveable;

        public LinearMover(ProjectileBase obj)
        {
            _moveable = obj;
        }


        public void Move()
        {
            List<Vector3> path = new List<Vector3>();
            var rigidBody = _moveable.Rigidbody;
            var projDir = (_moveable.spawnInfo.AimingAt - rigidBody.position).normalized;


            var total = _moveable.CastProjCount;
            var current = _moveable.spawnInfo.ProjID;

            var len = 2f;
            var angle = 60f;

            if (total > 1)
            {
                // rotate transform.right randomly and get a random direction on that plane
                Vector3 customAxis = Quaternion.AngleAxis(360 * current / total, rigidBody.transform.forward) * rigidBody.transform.right;

                // rotate angle degrees around our original direction, using customAxis as our rotation axis
                Vector3 dirToUse = Quaternion.AngleAxis(angle, customAxis) * rigidBody.transform.forward;

                Vector3 pos = rigidBody.position + len * dirToUse;
                path.Add(pos);
            }


            if (path.Count > 0)
            {
                rigidBody.isKinematic = true;
                rigidBody.DOPath(path.ToArray(), 0.1f, PathType.CatmullRom, PathMode.Ignore, 2).SetEase(Ease.Linear).onComplete += OnTweenEnded;
            }
            else
            {
                OnTweenEnded();
            }
        }

        private void OnTweenEnded()
        {
            // move ball towards original aimtarget
            _moveable.Rigidbody.isKinematic = false;
            _moveable.Rigidbody.velocity = Vector3.zero;

            var origDir = (_moveable.spawnInfo.AimingAt - _moveable.StartedMovingFrom).normalized;
            _moveable.Rigidbody.velocity = origDir * _moveable.spawnInfo.CastBy.Modifiers.ProjectileSpeedCalcd;
        }
    }
}
