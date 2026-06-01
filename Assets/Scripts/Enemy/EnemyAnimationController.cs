using UnityEngine;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private EnemyStateMachine stateMachine;

    public void OnAttack()
    {
        if(stateMachine != null)
        {
            stateMachine.ApplyDamageToPlayer();
        }
    }

    public void OnRangedAttack()
    {
        if (stateMachine != null)
        {
            stateMachine.ApplyToRangedDamageToPlayer();
        }
    }
}
