using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Arcanist
{

    public class PlayerController : MonoBehaviour 
    {
        private const string ANIM_TAG_ATTACK = "Attack";
        private const string ANIM_TAG_ATTACK_END = "Attack_End";
        private const string ANIM_TAG_MOVEMENT = "Movement";
        private const string ANIM_TAG_JUMP = "Jump";
        private const string ANIM_TAG_PARRY = "Parry";

        public static readonly int MOVEMENT = Animator.StringToHash("Movement");
        public static readonly int IS_ATTACK = Animator.StringToHash("IsAttack");
        public static readonly int IS_GROUNDED = Animator.StringToHash("IsGrounded");
        public static readonly int JUMP = Animator.StringToHash("Jump");
        public static readonly int DOUBLE_JUMP = Animator.StringToHash("DoubleJump");
        public static readonly int DESCENDING = Animator.StringToHash("Descending");
        public static readonly int DASH = Animator.StringToHash("Dash");
        public static readonly int PARRY = Animator.StringToHash("Parry");


        public Animator Animator { get; private set; }
        private SpriteRenderer SpriteRenderer { get; set; }
        private CharacterController2D Controller2D { get; set; }

        public int FacingDirection { get; private set; } = 1;

        [Header("Dash")]
        [SerializeField] private float dashPower = 10.0f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.35f;

        [Header("FX")]
        [SerializeField] private GameObject doubleJumpFxPrefab;
        [SerializeField] private GameObject dashFxPrefab;
        [SerializeField] private Transform fxSpawnPoint;

        private float nextDashTime;
        private Dictionary<int, PlayerAnimStateBase> StateDic { get; set; } = new();

        public bool IsParrying { get; set; } = false;
        public bool IsParryWindowOpen { get; private set; } = false;

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
            StateDic.Add(Animator.StringToHash(ANIM_TAG_PARRY), new ParryState(this));
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

            if (SimpleInput.GetKeyDown(Key.D))
                Animator.SetBool(IS_ATTACK, true);

            if (SimpleInput.GetKeyDown(Key.S))
                Animator.SetTrigger(PARRY);
        }

        public void HandleMovementWithoutAnimation()
        {
            float horizontal = 0.0f;

            if (Keyboard.current.leftArrowKey.isPressed)
                horizontal = -0.75f;
            else if (Keyboard.current.rightArrowKey.isPressed)
                horizontal = 0.75f;

            if (Mathf.Abs(horizontal) > 0.0f)
            {
                FacingDirection = horizontal < 0 ? -1 : 1;

                Vector3 scale = Animator.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * FacingDirection;
                Animator.transform.localScale = scale;
            }

            Controller2D.SetMoveInput(horizontal);
        }

        public void StopMovement()
        {
            Controller2D.SetMoveInput(0.0f);
        }

        public void OpenParryWindow()
        {
            IsParryWindowOpen = true;
        }

        public void CloseParryWindow()
        {
            IsParryWindowOpen = false;
        }

        public void EndParry()
        {
            IsParrying = false;
            CloseParryWindow();
        }

        private void UpdateJumpInput()
        {
            if (IsParrying)
                return;

            if (SimpleInput.GetKeyDown(Key.Space) == false)
                return;

            bool wasGrounded = Controller2D.IsGrounded;
            bool jumpAccepted = Controller2D.RequestJump();

            if (jumpAccepted && wasGrounded == false)
            {
                Animator.SetTrigger(DOUBLE_JUMP);
                SpawnFx(doubleJumpFxPrefab);
            }
        }

        private void UpdateDashInput()
        {
            if (IsParrying)
                return;

            if (SimpleInput.GetKeyDown(Key.LeftShift) == false)
                return;

            if (Time.time < nextDashTime)
                return;

            bool dashAccepted = Controller2D.RequestDash(FacingDirection, dashPower, dashDuration);

            if (dashAccepted == false)
                return;

            nextDashTime = Time.time + dashCooldown;
            Animator.SetTrigger(DASH);
            SpawnFx(dashFxPrefab);
        }

        private void SpawnFx(GameObject prefab)
        {
            if (prefab == null)
                return;

            Transform spawnPoint = fxSpawnPoint != null ? fxSpawnPoint : transform;
            GameObject fx = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

            Vector3 scale = fx.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * FacingDirection;
            fx.transform.localScale = scale;
        }

        private bool prevIsGrounded = false;

        private void UpdateGroundedAnimation()
        {
            bool isGrounded = Controller2D.IsGrounded;
            Animator.SetBool(IS_GROUNDED, isGrounded);

            if (IsParrying)
            {
                prevIsGrounded = isGrounded;
                return;
            }

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