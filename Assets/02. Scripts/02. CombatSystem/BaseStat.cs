using UnityEngine;

namespace Arcanist
{
    // 모든 캐릭터(플레이어, 적, 보스) 스탯의 공통 부모

    // 단순히 데이터만 담지 않고, "HP가 변하는 규칙(클램프, 사망 등)"까지
    // 이 클래스가 책임 => 데이터가 변경의 규칙까지 책임진다는 의미

    // 에디터에 노출시키기 위해 일부러 직렬화 선언 (원래는 숨기는 게 맞음)
    [System.Serializable]
    public class BaseStat
    {
        [field: SerializeField] public int MaxHp { get; private set; }
        public int Hp { get; private set; }
        public bool isDead => (Hp <= 0);

        /// <summary>
        /// 특정 개체에게 데미지를 적용하고 죽었는지를 반환하는 함수
        /// </summary>
        /// <param name="damage"></param>
        public bool ApplyDamage(int damage)
        {
            Hp = Mathf.Clamp(Hp - damage, 0, MaxHp);
            return isDead;
        }

        public void ApplyHeal(int heal)
        {
            Hp = Mathf.Clamp(Hp + heal, 0, MaxHp);
        }

        /// <summary>
        /// 체력을 최대치로 되돌리는 함수
        /// </summary>
        public void ResetToFull()
        {
            Hp = MaxHp;
        }
    }
}
