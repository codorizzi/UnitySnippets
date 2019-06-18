using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine {
    /// <summary>
    /// Add this class to a character and it'll be able to perform a grapple
    /// </summary>
    [AddComponentMenu("Corgi Engine/Character/Abilities/Character Dash")]
    public class GrappleAbility : CharacterAbility {
        

        [Header("Dash")]
        /// length of grapple
        public float grappleDistance = 8f;

        /// if this is true, forces will be reset on dash exit (killing inertia)
        public bool hasGrip;

        public GameObject grapplePrefab;

        [Header("Direction")]
        public MMAim aim;
        public bool autoCorrectTrajectory = true;

        /// the minimum amount of input required to apply a direction to the dash
        public float minimumInputThreshold = 0.1f;

        /// if this is true, the character will flip when dashing and facing the dash's opposite direction
        public bool flipCharacterIfNeeded = true;

        [Header("Cooldown")]
        public float grappleCooldown = 1f;

        protected float CooldownTimeStamp = 0;
        protected Vector2 GrappleDirection;

        /// <summary>
        /// Initializes our aim instance
        /// </summary>
        protected override void Initialization() {
            base.Initialization();
            aim.Initialization();
        }

        /// <summary>
        /// At the start of each cycle, we check if we're pressing the dash button. If we
        /// </summary>
        protected override void HandleInput() {
            if (_inputManager.DashButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) {
                StartGrapple();
            }
        }

        /// <summary>
        /// Causes the character to dash or dive (depending on the vertical movement at the start of the dash)
        /// </summary>
        public virtual void StartGrapple() {
            // if the Dash action is enabled in the permissions, we continue, if not we do nothing
            if (!AbilityPermitted
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
                || (_movement.CurrentState == CharacterStates.MovementStates.LedgeHanging)
                || (_movement.CurrentState == CharacterStates.MovementStates.Gripping))
                return;

            // If the user presses the dash button and is not aiming down
            if (_verticalInput < -_inputManager.Threshold.y)
                return;

            // if the character is allowed to dash
            if (CooldownTimeStamp <= Time.time) {
                InitiateGrapple();
            }
        }

        /// <summary>
        /// initializes all parameters prior to a dash and triggers the pre dash feedbacks
        /// </summary>
        public virtual void InitiateGrapple() {

            // we start our sounds
            PlayAbilityStartSfx();
            PlayAbilityUsedSfx();

            // we initialize our various counters and checks
            CooldownTimeStamp = Time.time + grappleCooldown;

            ComputeGrappleDirection();
            CheckFlipCharacter();

            RaycastHit2D hit = MMDebug.RayCast(transform.position, GrappleDirection, grappleDistance, _controller.PlatformMask, Color.blue, true);

            if (hit.collider == null)
                return;

            GameObject go = Instantiate(grapplePrefab, hit.point, new Quaternion(), hit.transform.parent);
            GrapplingHook hook = go.GetComponent<GrapplingHook>();
            hook.hasGrip = hasGrip;
            hook.source = gameObject;
            hook.DeployHook();
            
            // we play our exit sound
            StopAbilityUsedSfx();
            PlayAbilityStopSfx();

        }

        /// <summary>
        /// Computes the dash direction based on the selected options
        /// </summary>
        protected virtual void ComputeGrappleDirection() {
            // we compute our direction
            aim.PrimaryMovement = _character.LinkedInputManager.PrimaryMovement;
            aim.SecondaryMovement = _character.LinkedInputManager.SecondaryMovement;
            aim.CurrentPosition = this.transform.position;
            GrappleDirection = aim.GetCurrentAim();

            CheckAutoCorrectTrajectory();

            if (GrappleDirection.magnitude < minimumInputThreshold) {
                GrappleDirection = _character.IsFacingRight ? Vector2.right : Vector2.left;
            }
            else {
                GrappleDirection = GrappleDirection.normalized;
            }
        }

        /// <summary>
        /// Prevents the character from dashing into the ground when already grounded and if AutoCorrectTrajectory is checked
        /// </summary>
        protected virtual void CheckAutoCorrectTrajectory() {
            if (autoCorrectTrajectory && _controller.State.IsCollidingBelow && (GrappleDirection.y < 0f)) {
                GrappleDirection.y = 0f;
            }
        }

        /// <summary>
        /// Checks whether or not a character flip is required, and flips the character if needed
        /// </summary>
        protected virtual void CheckFlipCharacter() {
            // we flip the character if needed
            if (flipCharacterIfNeeded && (Mathf.Abs(GrappleDirection.x) > 0f)) {
                if (_character.IsFacingRight != (GrappleDirection.x > 0f)) {
                    _character.Flip();
                }
            }
        }
    }
}