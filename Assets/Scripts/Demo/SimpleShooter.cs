using UnityEngine;

public class SimpleShooter : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameFlowDemoManager gameFlowDemoManager;

    [SerializeField] private float damageAmount = 10.0f;
    [SerializeField] private float shootRange = 100.0f;
    [SerializeField] private float shootInterval = 0.25f;

    private float nextShootTime = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0) == true)
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        if(Time.time < nextShootTime)
        {
            return;
        }

        nextShootTime = Time.time + shootInterval;

        // 광선을 쏠 위치와 방향을 생성.
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hitInfo;

        if(Physics.Raycast(ray, out hitInfo, shootRange) == true)
        {
            IDamageable damageableTarget = hitInfo.collider.GetComponent<IDamageable>();
            if(damageableTarget != null)
            {
                damageableTarget.TakeDamage(damageAmount);
            }
        }
    }
}
