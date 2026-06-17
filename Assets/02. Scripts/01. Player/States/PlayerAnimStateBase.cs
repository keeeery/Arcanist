using UnityEngine;

namespace Arcanist
{
    // PlayerAnimStateBase
    // - 플레이어 애니메이션 상태 클래스들의 공통 부모 (상태 패턴)
    // - 매 프레임 "지금 어떤 태그의 상태인가"를 관찰해서 그에 맞는 행동 실행
    // - 다른 상태 패턴과는 다르게 전이(Transition)를 결정하지 않음 (전이는 애니메이터가)
    // - 보스 상태 패턴과는 다르게 순수 C# 클래스로 작성

    public abstract class PlayerAnimStateBase
    {
        // 추상 클래스(Abstract Class)
        // - 객체 지향 프로그래밍에서 "미완성된 베이스 설계도" 역할을 하는 중요한 개념
        // - 스스로 완벽히 실재하는 객체가 될 수는 없지만, 
        //   다른 클래스들이 상속받아 완성할 수 있도록 시스템의 공통적인 특징과 견고한 뼈대를 제공

        // 핵심 특징
        // - 인스턴스화 불가: new 키워드를 사용하여 직접 객체(인스턴스)를 생성할 수 없음
        //   오직 상속을 위한 부모 클래스로만 존재
        // - 추상 메소드(Abstract Methods): 선언만 있고 구현부(Body)가 없는 메소드
        //   자식 클래스에서 오버라이딩하여 구체적인 로직을 구현하는 "강제적 계약" 부여
        // - 일반 멤버 보유: 인터페이스(Interface)와 달리 
        //   완전히 구현된 일반 메소드나 멤버변수(필드), 프로퍼티를 가질 수 있음
        // - 단일 상속: C#에서는 다중 상속을 지원하지 않으므로 
        //   하나의 자식 클래스는 단 하나의 추상 클래스만 상속 가능

        // 사용해서 얻을 수 있는 이점
        // - 코드 재사용성 극대화
        // - 다형성의 올바른 구현
        // - 확장성(OCP)의 확보: 새로운 기능이나 객체 타입이 필요할 때 기존 코드를 거의 수정하지 않고, 
        //   추상 클래스를 상속받는 새로운 자식 클래스를 추가하는 방식으로 시스템을 유연하고 안전하게 확장 가능
        
        public const float INPUT_RESET_TIME = 0.3f;
        public const float COMBO_INPUT_END_TIME = 0.9f;

        protected PlayerController Owner { get; }
        protected Animator Animator => Owner.Animator;

        protected PlayerAnimStateBase(PlayerController owner)
        {
            Owner = owner;
        }

        // 이 상태로 진입한 순간 한 번 호출
        public virtual void Enter() { }
        // 이 상태에 머무는 동안 매 프레임 호출 (자식이 반드시 구현)
        public abstract void UpdateState(AnimatorStateInfo stateInfo);
        // 이 상태에서 떠나는 순간 한 번 호출
        public virtual void Exit() { }

        // 함수의 선언부에 사용하는 virtual 키워드와 abstract 키워드
        // virtual
        // - 메소드, 프로퍼티, 이벤트 선언 등에 사용되어 자식 클래스에서 
        //   해당 멤버의 구현을 오버라이드할 수 있도록 허용하는 권한을 부여하는 키워드
        //   객체 지향에서 다형성을 구현하는 요소 중 하나
        // - 보통 부모 클래스의 함수도 사용해야 할때 사용

        // abstract
        // - 선언만 있고 구현부(Body)가 없는 메소드
        //   자식 클래스에서 오버라이딩하여 구체적인 로직을 구현하는 "강제적 계약" 부여
        // - 자식 클래스에서 구현을 강제해야 할 때 사용

        // 두 키워드의 차이
        // - virtual은 구현부가 있어서, 자식 클래스에서 재정의하더라도 부모 클래스의 구현부 사용 가능
        // - abstract는 구현부가 없기 때문에 무조건 자식 클래스에서 재정의해야함
    }
}
