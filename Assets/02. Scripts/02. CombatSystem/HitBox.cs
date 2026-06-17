using System.Collections.Generic;
using UnityEngine;

namespace Arcanist
{
    // HitBox
        // - 게임에서 캐릭터나 사물의 위치, 충돌, 피탄, 피격 판정을 계산하기 위해 설정한 "가상의 상자"
        // - HitBox(공격 판정) <-> HurtBox(피격 판정) 구조로 보통 작성 
        //   (HurtBox는 일반 콜라이더로도 구현 가능하긴함)
        // - 상자를 밀어내거나 부수는 판정도 포함

    public class Hitbox : MonoBehaviour
    {
        [field: SerializeField] public AttackInfo AttackInfo { get; private set; }
        
        // 이 히트박스의 주인, 부모 계층에서 찾아 보관
        private CombatEntity Owner { get; set; }

        // 충돌한 HurtBox의 정보들을 담아둔다.
        // 연속 공격을 반지하기 위해 HashSet 사용, List 사용해도 가능
        private HashSet<HurtBox> checkList = new HashSet<HurtBox>();

        private void Awake()
        {
            Owner = GetComponentInParent<CombatEntity>();
            if (Owner == null)
            {
                Debug.LogError($"{name}: 부모 계층에서 CombatEntity를 찾지 못했습니다. 오브젝트를 삭제합니다.");
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            // 히트박스가 다시 켜질 때 (재사용 될 때 checkList를 비워줌)
            checkList.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // HurtBox가 없을 경우 종료 (Physics Layer 설정으로 아예 안 들어오게 하는게 좋음)
            if (collision.TryGetComponent<HurtBox>(out HurtBox other) == false) return;

            // 현재 HashSet에 해당 HurtBox가 존재할 경우 종료
            if (checkList.Contains(other)) return;

            // 중복 체크 방지를 위해 HashSet에 해당 HurtBox를 추가
            checkList.Add(other);

            // 데미지 처리하는 로직
            // 미리 준비해놓고
            CombatEntity sender = Owner;
            CombatEntity receiver = other.Owner;
            int damage = AttackInfo.RollDamage();

            // 담아주고
            CombatEvent @event;
            @event.EventType = CombatEventType.DamageEvent;
            @event.Amount = damage;
            @event.Position = collision.ClosestPoint(sender.transform.position);

            // 보낸다
            CombatSystem.Instance.To(sender, receiver, @event);
        }
    }   
}
