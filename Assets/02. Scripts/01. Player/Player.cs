using Unity.VisualScripting;
using UnityEngine;

namespace Arcanist
{
    public class PlayerStat : BaseStat
    {
        
    }
    
    // 스탯, 전투 관련 기능
    // 어떤 개체에서든 Player를 찾을 수 있는 기능
    // CombatEntity를 상속받았기 때문에 "전투에 참여하는 개체"의 
    // 공통부(Pivot, TakeDamege 등 순수가상함수 계약)는 물려받고, 플레이어 고유의 것만 여기 남음

    public class Player : CombatEntity
    {
        public static Player LocalPlayer { get; set; }

        public override BaseStat BaseStat => Stat;

        private PlayerStat Stat { get; set; } 

        public AttackInfo AttackInfo;

        public override void TakeDamage(int damage)
        {
            
        }

        public override void TakeHeal(int heal)
        {
            
        }
    }
}

