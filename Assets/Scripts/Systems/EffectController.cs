using Unity.Properties;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    [SerializeField] private bool destroyEffect = true;
    [SerializeField] private float lifeTime = 0.5f;

    private void OnEnable()
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particles.Length; ++i)
        {
            particles[i].Play();
        }

        if (destroyEffect == true)
        {
            Destroy(gameObject, lifeTime);
        }
        else
        {
            Invoke("HideEffect", lifeTime);
        }
    }

    void HideEffect()
    {
        gameObject.SetActive(false);
    }
}
