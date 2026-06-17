using UnityEngine;

namespace Arcanist
{
    public enum AttackKey
    {
        None = 0, Combo1, Combo2, Combo3, JumpAttack
    }
    [System.Serializable]
    public struct AttackInfo
    {
        public AttackKey Key;
        public int MinDamage;
        public int MaxDamage;
        public AnimationCurve damageCurve;

        // 구조체도 메소드를 가질 수 있음
        // "이 데이터를 어떻게 해석(데미지 계산 공식)하는가?"
        // 데이터 곁에 두는 것이 응집도가 좋음
        // 기능이 많아지면 분리(확장함수)하는 것이 좋지만, 
        // 몇 개 없으면 구조체 내부에 구현하고 사용해도 됨
        
        public int RollDamage()
        {
            // Curve에 t 역할을 검색할 랜덤한 값을 추출함
            // e.g. 0~99까지의 랜덤한 숫자를 뽑음
            float randomRoll = Random.Range(0f, 1f);

            // 커브를 통해 가중치(t) 평가 = .Evaluate()를 이용해 y축 값 추출
            float evaluation = damageCurve.Evaluate(randomRoll);

            float finalDamage = Mathf.Lerp(MinDamage, MaxDamage, evaluation);

            // Mathf.RoundToInt(): float 값을 반올림
            return Mathf.RoundToInt(finalDamage);
        }
    }
}
