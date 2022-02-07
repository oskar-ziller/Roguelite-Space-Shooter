using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeteorGame
{
    public class ExplosionHandler : MonoBehaviour
    {

        private SphereCollider col;
        private ParticleSystem ps;

        private SpellSlot castBy;
        

        ParticleSystem.Particle[] m_Particles;

        private void Awake()
        {
            ps = GetComponentInChildren<ParticleSystem>();
            col = GetComponentInChildren<SphereCollider>();
            m_Particles = new ParticleSystem.Particle[ps.main.maxParticles];
        }

        private void Start()
        {
        }

        private void FixedUpdate()
        {
            SetColliderSizeToParticleSize();
        }


        internal void Setup(SpellSlot castBy)
        {
            this.castBy = castBy;
        }


        private void SetColliderSizeToParticleSize()
        {
            if (ps.isPlaying)
            {
                int numParticlesAlive = ps.GetParticles(m_Particles);

                if (numParticlesAlive > 0)
                {
                    var particle = m_Particles[0];
                    var size = particle.GetCurrentSize(ps);
                    col.radius = size / 2f;
                }
            }
        }



        private void OnParticleSystemStopped()
        {
            Destroy(gameObject);
        }


        public void DoExplode()
        {
            // Set the particle starting size to radi * 2 and some variation

            // Particle uses vertex streams to communicate with the shader
            // to fade out over time
            ParticleSystem.MainModule main = ps.main;
            main.startSize = castBy.ExpRadi;

            ps.Play();
        }


        private void OnTriggerEnter(Collider other)
        {
            Enemy e = other.gameObject.GetComponent<Enemy>();
            e.TakeHit(castBy, applyAilment: false);
        }
    }
}
