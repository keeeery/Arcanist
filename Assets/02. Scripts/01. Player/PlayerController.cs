using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Arcanist
{

    public class PlayerController : MonoBehaviour 
    {
        // 애니메이션 상태 Tag
        // - 애니메이터의 상태마다 지정할 수 있는 stirng Tag
        // - 컴포넌트에서는 해당 정보를 읽어서 현재 애니메이션 상태 추측

        private const string ANIM_TAG_ATTACK = "Attack";
        private const string ANIM_TAG_ATTACK_END = "Attack_End";
        private const string ANIM_TAG_MOVEMENT = "Movement";
        private const string ANIM_TAG_FIRE = "Fire";
        private const string ANIM_TAG_JUMP = "Jump";

        public static readonly int MOVEMENT = Animator.StringToHash("Movement");
        public static readonly int IS_ATTACK = Animator.StringToHash("IsAttack");
        public static readonly int IS_GROUNDED = Animator.StringToHash("IsGrounded");
        public static readonly int JUMP = Animator.StringToHash("Jump");
        public static readonly int DESCENDING = Animator.StringToHash("Descending");
        public static readonly int IS_FIRE = Animator.StringToHash("IsFire");
        public static readonly int DASH = Animator.StringToHash("Dash");

        public Animator Animator { get; private set; }
        private SpriteRenderer SpriteRenderer { get; set; }
        private CharacterController2D Controller2D { get; set; }

        public int FacingDirection { get; private set; } = 1;

        [Header("Dash")]
        [SerializeField] private float dashDistance = 1.5f;
        [SerializeField] private float dashCooldown = 0.35f;

        private float nextDashTime;

        private Dictionary<int, PlayerAnimStateBase> StateDic { get; set; } = new();
        private PlayerAnimStateBase defaultState;
        private PlayerAnimStateBase currentState;

        private void Awake()
        {
            Animator = GetComponentInChildren<Animator>();
            SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            Controller2D = GetComponent<CharacterController2D>();

            defaultState = new MovementState(this);
            StateDic.Add(Animator.StringToHash(ANIM_TAG_MOVEMENT), defaultState);
            StateDic.Add(Animator.StringToHash(ANIM_TAG_ATTACK), new AttackState(this));
            StateDic.Add(Animator.StringToHash(ANIM_TAG_ATTACK_END), new AttackEndState(this));
            StateDic.Add(Animator.StringToHash(ANIM_TAG_JUMP), new JumpState(this));
        }

        private void Update()
        {
            UpdateAnimState();
            UpdateJumpInput();
            UpdateDashInput();
            UpdateGroundedAnimation();
        }

        private void UpdateAnimState()
        {
            AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

            if (StateDic.TryGetValue(stateInfo.tagHash, out PlayerAnimStateBase nextState) == false)
                nextState = defaultState;
            
            if (currentState != nextState)
            {
                currentState?.Exit();
                currentState = nextState;
                nextState.Enter();
            }

            currentState.UpdateState(stateInfo);
        }

        public void HandleMovement()
        {
            Vector2 inputVector = SimpleInput.GetMoveAxisRaw();
            float absMovement = Mathf.Abs(inputVector.x);
            Animator.SetFloat(MOVEMENT, absMovement);

            if (absMovement > 0)
            {
                FacingDirection = (inputVector.x < 0 ? -1 : 1);

                Vector3 scale = Animator.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * FacingDirection;
                Animator.transform.localScale = scale;
            }

            Controller2D.SetMoveInput(inputVector.x);

            // ARCANIST 기획안 기준: D = 기본 공격
            if (SimpleInput.GetKeyDown(Key.D))
                Animator.SetBool(IS_ATTACK, true);
        }

        public void StopMovement()
        {
            Controller2D.SetMoveInput(0.0f);
        }

        private void UpdateJumpInput()
        {
            if (SimpleInput.GetKeyDown(Key.Space))
                Controller2D.RequestJump();
        }

        private void UpdateDashInput()
        {
            // ARCANIST 기획안 기준: Left Shift = 대시
            if (SimpleInput.GetKeyDown(Key.LeftShift) == false)
                return;

            if (Time.time < nextDashTime)
                return;

            nextDashTime = Time.time + dashCooldown;
            Controller2D.AddExternalMovement(Vector3.right * FacingDirection * dashDistance);
            Animator.SetTrigger(DASH);
        }

        private bool prevIsGrounded = false;

        private void UpdateGroundedAnimation()
        {
            bool isGrounded = Controller2D.IsGrounded;
            Animator.SetBool(IS_GROUNDED, isGrounded);

            if (prevIsGrounded && isGrounded == false)
            {
                if (Controller2D.VerticalVelocity > 0.0f)
                    Animator.SetTrigger(JUMP);
                else 
                    Animator.SetTrigger(DESCENDING);
            }

            prevIsGrounded = isGrounded;
        }
    }
}
