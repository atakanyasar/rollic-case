using UnityEngine;
namespace Gameplay.View
{
    public class ExitParticle : MonoBehaviour
    {
        [SerializeField] private ParticleSystem Particles;
        [SerializeField] private ParticleSystemRenderer Renderer;

        public void Prepare(Material material, float widthScale)
        {
            Renderer.sharedMaterial = material;
            var shape = Particles.shape;
            var scale = shape.scale;
            scale.x *= widthScale;
            shape.scale = scale;
        }

        public void Reset()
        {
            Particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Play()
        {
            Particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Particles.Play();
        }
    }
}
