using Unity.VisualScripting;
using UnityEngine;

namespace Arcanist
{
    public class PlayerStat : BaseStat
    {
        
    }

    public class Player : CombatEntity
    {
        public static Player LocalPlayer { get; set; }

        public override BaseStat BaseStat => Stat;

        private PlayerStat Stat { get; set; } 

        public AttackInfo AttackInfo;

        private void Awake()
        {
            LocalPlayer = this;
        }

        public override void TakeDamage(int damage)
        {
            
        }

        public override void TakeHeal(int heal)
        {
            
        }
    }
}

