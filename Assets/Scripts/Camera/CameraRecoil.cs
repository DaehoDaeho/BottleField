using UnityEngine;

/// <summary>
/// 총 발사 시 카메라 반동을 처리하는 역할.
/// </summary>
public class CameraRecoil : MonoBehaviour
{
    [SerializeField] private float currentPitchRecoil = 0.0f;
    [SerializeField] private float currentReturnSpeed = 14.0f;

    // Update is called once per frame
    void Update()
    {
        ApplyRecoilRotation();
        RecoverRecoil();
    }

    public void AddRecoil(float recoilPitch, float returnSpeed)
    {
        currentPitchRecoil += recoilPitch;
        currentPitchRecoil = Mathf.Min(currentPitchRecoil, 20.0f);
        currentReturnSpeed = returnSpeed;
    }

    void ApplyRecoilRotation()
    {
        Quaternion recoilRotation = Quaternion.Euler(-currentPitchRecoil, 0.0f, 0.0f);
        transform.localRotation = recoilRotation;
    }

    void RecoverRecoil()
    {
        currentPitchRecoil = Mathf.Lerp(currentPitchRecoil, 0.0f,
            currentReturnSpeed * Time.deltaTime);

        if(currentPitchRecoil < 0.01f)
        {
            currentPitchRecoil = 0.0f;
        }
    }
}
