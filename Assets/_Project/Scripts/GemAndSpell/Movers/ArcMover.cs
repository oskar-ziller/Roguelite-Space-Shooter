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

            if (_moveable.TotalProjectiles == 1)
            {
                OnTweenEnded();
                return;
            }


            List<Vector3> path = new List<Vector3>();
            var rigidBody = _moveable.Rigidbody;
            var projDir = _moveable.Rigidbody.transform.forward;

            var dist = Vector3.Distance(_moveable.MovingTowards, rigidBody.position);

            var total = _moveable.TotalProjectiles;
            var currentID = _moveable.ProjectileID;

            int curveCount = 1;

            var len = Math.Min(dist / 2f, 20f);

            var centerPos = _moveable.CastPos;
            var currDirectionFromCenter = (_moveable.MainMesh.position - centerPos).normalized;

            var moveVector = (5 * currDirectionFromCenter) + (len * rigidBody.transform.forward);

            Vector3 pos = rigidBody.transform.position + moveVector;
            path.Add(pos);


            rigidBody.isKinematic = true;
            rigidBody.transform.DOMove(pos, _moveable.ScaleDur/2).SetEase(Ease.InSine).onComplete += OnTweenEnded;
        }

        private void OnTweenEnded()
        {
            // move ball towards direction of movement
            _moveable.Rigidbody.isKinematic = false;
            _moveable.Rigidbody.velocity = Vector3.zero;
            //_moveable.Rigidbody.transform.LookAt(_moveable.MovingTowards);

            //var origDir = (_moveable.MovingTowards - _moveable.RigidBodyObj.position).normalized;
            _moveable.Rigidbody.velocity = _moveable.Rigidbody.transform.forward * _moveable.StartingSpeed;


            //_moveable.Rigidbody.velocity = (_moveable.MovingTowards - _moveable.Rigidbody.position).normalized * _moveable.StartingSpeed;

        }


    }
}
