using System.Collections.Generic;
using UnityEngine;

namespace Arcanist
{
    public class HitBox : MonoBehaviour
    {
        [field: SerializeField] public AttackInfo AttackInfo { get; private set; }

        private CombatEntity Owner { get; set; }

        private HashSet<HurtBox> checkList = new HashSet<HurtBox>();

        private void Awake()
        {
            Owner = GetComponentInParent<CombatEntity>();
            
            if(Owner == null)
            {
                Debug.LogError($"{name} : 부모 계층에서 CombatEntity를 찾지 못했습니다." +
                    $"오브젝트를 삭제합니다");
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            checkList.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            HurtBox hurtBox = CombatSystem.Instance.GetHurtBoxOrNull(collision);
            if (hurtBox == null) return;
            
            if (checkList.Contains(hurtBox)) return;

            checkList.Add(hurtBox);

            CombatEntity sender = Owner;
            CombatEntity receiver = hurtBox.Owner;

            int damage = AttackInfo.RollDamage();

            CombatEvent @event;
            @event.EventType = CombatEventType.DamageEvent;
            @event.Amount = damage;
            @event.Position = collision.ClosestPoint(sender.transform.position);

            CombatSystem.Instance.To(sender, receiver, @event);
        }
    }
}
