using UnityEngine;

/// <summary>
/// 카메라 흔들림 효과를 처리하는 역할.
/// </summary>
public class CameraShake : MonoBehaviour
{
    [SerializeField] private float shakeTimer = 0.0f;
    [SerializeField] private float currentStrength = 0.0f;

    private Vector3 originalLocalPosition = Vector3.zero;

    private void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(shakeTimer > 0.0f)
        {
            ApplyShakePosition();
            shakeTimer -= Time.deltaTime;
            return;
        }

        shakeTimer = 0.0f;
        transform.localPosition = originalLocalPosition;
    }

    public void Shake(float duration, float strength)
    {
        shakeTimer = duration;
        currentStrength = strength;
    }

    void ApplyShakePosition()
    {
        float randomX = Random.Range(-currentStrength, currentStrength);
        float randomY = Random.Range(-currentStrength, currentStrength);

        Vector3 shakeOffset = new Vector3(randomX, randomY, 0.0f);

        transform.localPosition = originalLocalPosition + shakeOffset;
    }
}
