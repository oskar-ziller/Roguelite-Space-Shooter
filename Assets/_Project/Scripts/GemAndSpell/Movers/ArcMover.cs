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
            //var rigidBody = _moveable.Rigidbody;

            //Debug.Log(rigidBody.position + " - " + rigidBody.transform.position);
            //rigidBody.DOMove(new Vector3(40, 40, 40), _moveable.PathDur).SetEase(Ease.Linear);


            //OnTweenEnded();
            //return;

            var rigidBody = _moveable.Rigidbody;
            rigidBody.isKinematic = true;

            var projDir = Player.Instance.transform.forward;

            var dist = Vector3.Distance(_moveable.spawnInfo.AimingAt, rigidBody.position);
            dist = 100;

            var total = _moveable.CastProjCount;
            var currentID = _moveable.spawnInfo.ProjID;

            var len = Math.Min(dist / 2f, _moveable.PathLen);

            len = _moveable.PathLen;

            if (_moveable.CastProjCount == 1 || _moveable.spawnInfo.ProjID == 0)
            {
                var moveVector = (len * rigidBody.transform.forward);

                Vector3 pos = rigidBody.transform.position + moveVector;

                rigidBody.transform.DOMove(pos, _moveable.PathDur)
                    .SetEase(Ease.Linear)
                    .SetUpdate(UpdateType.Fixed)
                    .onComplete += OnTweenEnded;
            }
            else
            {
                var centerPos = _moveable.spawnInfo.CastPos;
                var currDirectionFromCenter = (_moveable.MainMesh.position - centerPos).normalized;

                var moveVector = (_moveable.SpreadFromCenter * currDirectionFromCenter) + (len * rigidBody.transform.forward);

                Vector3 pos = rigidBody.transform.position + moveVector;

                rigidBody.transform.DOMove(pos, _moveable.PathDur)
                    .SetEase(Ease.Linear)
                    .SetUpdate(UpdateType.Fixed)
                    .onComplete += OnTweenEnded;
            }
        }

        private void OnTweenEnded()
        {
            // move ball towards direction of movement
            _moveable.Rigidbody.isKinematic = false;
            _moveable.Rigidbody.velocity = _moveable.Rigidbody.transform.forward * _moveable.spawnInfo.CastBy.Modifiers.ProjectileSpeedCalcd;
        }


    }
}
