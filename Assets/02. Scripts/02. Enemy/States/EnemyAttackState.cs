using UnityEngine;

namespace Arcanist
{
    public class EnemyAttackState : EnemyStateBase
    {
        private float attackTimer;
        private bool attackProcessed;

        public EnemyAttackState(EnemyController owner) : base(owner)
        {
        }

        public override void Enter()
        {
            attackTimer = 0.0f;
            attackProcessed = false;
            Owner.StopMove();
        }

        public override void UpdateState(AnimatorStateInfo stateInfo)
        {
            Owner.StopMove();

            if (Owner.Target != null)
                Owner.LookAt(Owner.Target.position);

            attackTimer += Time.deltaTime;

            if (attackProcessed == false && attackTimer >= Owner.AttackHitDelay)
            {
                attackProcessed = true;
                Owner.ProcessAttack();
            }
        }

        public override void Exit()
        {
            Owner.StopMove();
        }
    }
}
