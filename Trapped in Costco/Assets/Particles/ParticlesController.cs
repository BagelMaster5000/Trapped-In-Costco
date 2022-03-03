using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    [SerializeField] ParticleSystem smashParticles;
    [SerializeField] ParticleSystem eatParticles;
    [SerializeField] ParticleSystem sparkleParticles;
    [SerializeField] ParticleSystem boneParticles;

    private void Awake()
    {
        GameController.staticReference.OnSmash += () => { smashParticles.Play(); };
        GameController.staticReference.OnTryMoveWhileFreeSamples += () => { eatParticles.Play(); };
        GameController.staticReference.OnGotCorrectItem += () => { sparkleParticles.Play(); };
        GameController.staticReference.OnClearedBlockage += (Location dummy) => { boneParticles.Play(); };

        GameController.staticReference.OnArrivedAtLocation += (Location dummy) =>
        {
            smashParticles.Clear();
            eatParticles.Clear();
            sparkleParticles.Clear();
            boneParticles.Clear();
        };
    }
}
