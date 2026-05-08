using UnityEngine;
using UnityEngine.Rendering;

public class EnemyRagdoll : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private Rigidbody[] rigidBodies;

    private void Awake()
    {
        rigidBodies = GetComponentsInChildren<Rigidbody>();
        DisableRagdoll();
    }

    public void EnableRagdoll()
    {
        animator.enabled = false;

        for(int i=0; i<rigidBodies.Length; ++i)
        {
            rigidBodies[i].isKinematic = false;
        }
    }

    public void DisableRagdoll()
    {
        animator.enabled = true;

        for (int i = 0; i < rigidBodies.Length; ++i)
        {
            rigidBodies[i].isKinematic = true;
        }
    }
}
