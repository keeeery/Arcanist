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
        public static readonly int IS_SPRINT = Animator.StringToHash("IsSprint");
        public static readonly int IS_ATTACK = Animator.StringToHash("IsAttack");
        public static readonly int IS_GROUNDED = Animator.StringToHash("IsGrounded");
        public static readonly int JUMP = Animator.StringToHash("Jump");
        public static readonly int DOUBLE_JUMP = Animator.StringToHash("DoubleJump");
        public static readonly int DESCENDING = Animator.StringToHash("Descending");
        public static readonly int DASH = Animator.StringToHash("Dash");
        public static readonly int PARRY = Animator.StringToHash("Parry");
        public static readonly int PARRY_SUCCESS = Animator.StringToHash("ParrySuccess");
        public static readonly int IS_HURT = Animator.StringToHash("IsHurt");
        public static readonly int DEATH = Animator.StringToHash("Death");


        public Animator Animator { get; private set; }
        private SpriteRenderer SpriteRenderer { get; set; }
        private CharacterController2D Controller2D { get; set; }

        public int FacingDirection { get; private set; } = 1;

        [Header("Dash")]
        [SerializeField] private float dashPower = 10.0f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 0.8f;

        [Header("Parry")]
        [SerializeField] private float parryCooldown = 1.2f;

        [Header("Sprint")]
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float staminaCostPerSec = 20.0f;
        [SerializeField] private float staminaRecoveryPerSec = 10.0f;
        [SerializeField] private float staminaRecoveryDelay = 0.6f;
        [SerializeField] private float sprintInputGraceTime = 0.08f;

        [Header("Hurt")]
        [SerializeField] private float hurtDuration = 0.15f;

        [Header("FX")]
        [SerializeField] private GameObject doubleJumpFxPrefab;
        [SerializeField] private GameObject dashFxPrefab;
        [SerializeField] private GameObject sprintLoopFxPrefab;
        [SerializeField] private GameObject sprintEndFxPrefab;
        [SerializeField] private Transform fxSpawnPoint;
        [SerializeField] private Transform sprintFxSpawnPoint;

        private float nextDashTime;
        private float nextParryTime;
        private float hurtEndTime;
        private float staminaRecoveryStartTime;
        private float sprintInputGraceEndTime;
        private bool isSprint;
        private GameObject sprintLoopFx;
        private bool isHurt;
        private bool isDead;
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
            UpdateCooldownUI();

            if (isDead) return;

            if (isHurt)
            {
                if (Time.time >= hurtEndTime) EndHurt();
                else return;
            }

            UpdateAnimState();
            UpdateJumpInput();
            UpdateDashInput();
            UpdateGroundedAnimation();
            UpdateStaminaRecovery();
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
            if (isHurt || isDead)
            {
                StopMovement();
                return;
            }

            Vector2 inputVector = SimpleInput.GetMoveAxisRaw();
            float absMovement = Mathf.Abs(inputVector.x);

            bool shiftPressed = Keyboard.current.leftShiftKey.isPressed;
            bool hasMoveInput = absMovement > 0.0f;

            if (shiftPressed && hasMoveInput)
                sprintInputGraceEndTime = Time.time + sprintInputGraceTime;

            bool wantsSprint = Controller2D.IsGrounded && shiftPressed && (hasMoveInput || Time.time < sprintInputGraceEndTime);
            bool canSprint = TrySprint(wantsSprint);

            Animator.SetFloat(MOVEMENT, canSprint ? 2.0f : absMovement);
            SetSprintState(canSprint);

            if (absMovement > 0)
            {
                FacingDirection = (inputVector.x < 0 ? -1 : 1);

                Vector3 scale = Animator.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * FacingDirection;
                Animator.transform.localScale = scale;

                UpdateSprintLoopFxDirection();
            }

            float moveInput = inputVector.x;

            if (canSprint) moveInput *= sprintMultiplier;

            Controller2D.SetMoveInput(moveInput);

            if (SimpleInput.GetKeyDown(Key.D))
                Animator.SetBool(IS_ATTACK, true);

            if (SimpleInput.GetKeyDown(Key.S))
            {
                if (Time.time >= nextParryTime)
                {
                    nextParryTime = Time.time + parryCooldown;
                    IsParrying = true;
                    StopMovement();
                    OpenParryWindow();
                    Animator.SetTrigger(PARRY);
                    Debug.Log("[Parry] Start");
                }
            }
        }

        public void HandleMovementWithoutAnimation(float moveScale = 0.75f)
        {
            if (isHurt || isDead)
            {
                StopMovement();
                return;
            }

            float horizontal = 0.0f;

            if (Keyboard.current.leftArrowKey.isPressed)
                horizontal = -moveScale;
            else if (Keyboard.current.rightArrowKey.isPressed)
                horizontal = moveScale;

            if (Mathf.Abs(horizontal) > 0.0f)
            {
                FacingDirection = horizontal < 0 ? -1 : 1;

                Vector3 scale = Animator.transform.localScale;
                scale.x = Mathf.Abs(scale.x) * FacingDirection;
                Animator.transform.localScale = scale;

                UpdateSprintLoopFxDirection();
            }

            bool wantsSprint = Controller2D.IsGrounded && Mathf.Abs(horizontal) > 0.0f && Keyboard.current.leftShiftKey.isPressed;
            bool canSprint = TrySprint(wantsSprint);

            SetSprintState(canSprint);

            if (canSprint) horizontal *= sprintMultiplier;

            Controller2D.SetMoveInput(horizontal);
        }

        public void StopMovement()
        {
            Controller2D.SetMoveInput(0.0f);
            SetSprintState(false);
        }

        public void OpenParryWindow()
        {
            if (IsParryWindowOpen) return;

            IsParryWindowOpen = true;
            Debug.Log("[Parry] Window Open");
        }

        public void CloseParryWindow()
        {
            if (IsParryWindowOpen == false) return;

            IsParryWindowOpen = false;
            Debug.Log("[Parry] Window Close");
        }

        public void EndParry()
        {
            IsParrying = false;
            CloseParryWindow();
            Debug.Log("[Parry] End");
        }

        public void SuccessParry()
        {
            IsParrying = false;
            CloseParryWindow();

            Animator.ResetTrigger(PARRY);
            Animator.SetTrigger(PARRY_SUCCESS);

            Debug.Log("패링 성공 애니메이션");
        }

        public void Hurt()
        {
            if (isDead) return;

            isHurt = true;
            hurtEndTime = Time.time + hurtDuration;
            IsParrying = false;
            CloseParryWindow();
            StopMovement();
            Animator.SetBool(IS_HURT, true);
        }

        private void EndHurt()
        {
            isHurt = false;
            Animator.SetBool(IS_HURT, false);
        }

        public void Dead()
        {
            if (isDead) return;

            isDead = true;
            isHurt = false;
            IsParrying = false;
            CloseParryWindow();
            StopMovement();
            Animator.SetBool(IS_HURT, false);
            Animator.SetTrigger(DEATH);
        }

        private void UpdateJumpInput()
        {
            if (IsParrying || isHurt || isDead)
                return;

            if (SimpleInput.GetKeyDown(Key.Space) == false)
                return;

            bool wasGrounded = Controller2D.IsGrounded;
            if (wasGrounded)
                SetSprintState(false, false);

            bool jumpAccepted = Controller2D.RequestJump();

            if (jumpAccepted && wasGrounded == false)
            {
                Animator.SetTrigger(DOUBLE_JUMP);
                SpawnFx(doubleJumpFxPrefab);
            }
        }

        private void UpdateDashInput()
        {
            if (IsParrying || isHurt || isDead)
                return;

            if (SimpleInput.GetKeyDown(Key.A) == false)
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

        private void UpdateCooldownUI()
        {
            if (Player.LocalPlayer == null)
                return;

            Player.LocalPlayer.UpdateParryCooldownUI(
                Mathf.Max(0.0f, nextParryTime - Time.time),
                parryCooldown);
        }

        private bool TrySprint(bool wantsSprint)
        {
            if (wantsSprint == false || Player.LocalPlayer == null)
                return false;

            float staminaCost = staminaCostPerSec * Time.deltaTime;
            staminaRecoveryStartTime = Time.time + staminaRecoveryDelay;

            if (Player.LocalPlayer.HasStamina(staminaCost) == false)
            {
                sprintInputGraceEndTime = 0.0f;
                SetSprintState(false);
                return false;
            }

            if (Player.LocalPlayer.TryUseStamina(staminaCost) == false)
            {
                sprintInputGraceEndTime = 0.0f;
                SetSprintState(false);
                return false;
            }

            //isSprint = true;
            return true;
        }

        private void UpdateStaminaRecovery()
        {
            if (isSprint || Player.LocalPlayer == null)
                return;

            if (Time.time < staminaRecoveryStartTime)
                return;

            Player.LocalPlayer.RecoverStamina(staminaRecoveryPerSec * Time.deltaTime);
        }

        private void UpdateSprintLoopFxDirection()
        {
            if (sprintLoopFx == null) return;

            Vector3 scale = sprintLoopFx.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * FacingDirection;
            sprintLoopFx.transform.localScale = scale;
        }

        private void SetSprintState(bool value, bool playEndFx = true)
        {
            if (isSprint == value)
            {
                Animator.SetBool(IS_SPRINT, value);
                return;
            }

            isSprint = value;
            Animator.SetBool(IS_SPRINT, value);

            if (value)
            {
                if (sprintLoopFx == null)
                {
                    Transform spawnPoint = sprintFxSpawnPoint != null ? sprintFxSpawnPoint : transform;
                    sprintLoopFx = Instantiate(sprintLoopFxPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
                    
                    UpdateSprintLoopFxDirection();
                }

                return;
            }

            if (sprintLoopFx != null)
            {
                Destroy(sprintLoopFx);
                sprintLoopFx = null;
            }

            if (playEndFx && Controller2D.IsGrounded)
                SpawnFx(sprintEndFxPrefab, sprintFxSpawnPoint);
        }

        private void SpawnFx(GameObject prefab)
        {
            SpawnFx(prefab, fxSpawnPoint);
        }

        private void SpawnFx(GameObject prefab, Transform customSpawnPoint)
        {
            if (prefab == null)
                return;

            Transform spawnPoint = customSpawnPoint != null ? customSpawnPoint : transform;
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

            if (IsParrying || isHurt || isDead)
            {
                prevIsGrounded = isGrounded;
                return;
            }

            if (prevIsGrounded && isGrounded == false)
            {
                SetSprintState(false, false);

                if (Controller2D.VerticalVelocity > 0.0f)
                    Animator.SetTrigger(JUMP);
                else 
                    Animator.SetTrigger(DESCENDING);
            }

            prevIsGrounded = isGrounded;
        }
    }
}