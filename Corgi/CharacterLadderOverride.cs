using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using UnityEngine;

/*
 *
 * - Fixed defect -    If discrete ladder GO were close together, race condition occurred, such that the last ladder
 *                     piece was the one to set the variables (e.g. TouchingLadder, etc.). Used layer check instead. 
 *
 * - Fixed defect -    Replaced four instances where Character transform was being set without collision checks
 * 
 */

namespace MoreMountains.CorgiEngine {

    public class CharacterLadderOverride : CharacterLadder {

        private BoxCollider2D _boxCollider;
        public LayerMask _ladderMask;
        private ContactFilter2D _ladderFilter;

        public new bool LadderColliding {
            get {
                return _boxCollider.IsTouchingLayers(_ladderMask);
            }
        }

        /// the ladder the character is currently on
        public new Ladder CurrentLadder {
            get {

                if (!_boxCollider.IsTouchingLayers(_ladderMask))
                    return null;

                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(_ladderMask);

                List<Collider2D> colliders = new List<Collider2D>();
                _boxCollider.OverlapCollider(filter, colliders);

                // should always have an item, but just in case
                if(colliders.Count > 0)
                    return colliders[0].GetComponent<Ladder>();
                
                return null;

            }
        }

        const float _climbingDownInitialYTranslation = 0.1f;
        const float _ladderTopSkinHeight = 0.01f;

        protected CorgiControllerOverride _corgiController;

        private void Awake() {
            _corgiController = GetComponent<CorgiControllerOverride>();
        }

        protected override void Initialization() {
            base.Initialization();

            _boxCollider = GetComponent<BoxCollider2D>();
            _ladderFilter = new ContactFilter2D();
            _ladderFilter.SetLayerMask(_ladderMask);

        }

        protected override void StartClimbingDown() {
            SetClimbingState();
            _controller.CollisionsOff();
            _controller.ResetColliderSize();

            // we rotate our character if requested
            if (ForceRightFacing) {
                _character.Face(Character.FacingDirections.Right);
            }

            if (_characterGravity != null) {
                if (_characterGravity.ShouldReverseInput()) {
                    if (CurrentLadder.CenterCharacterOnLadder) {
                        _corgiController.SetTransform(new Vector2(CurrentLadder.transform.position.x,
                            transform.position.y + _climbingDownInitialYTranslation));
                    }
                    else {
                        _corgiController.SetTransform(new Vector2(transform.position.x,
                            transform.position.y + _climbingDownInitialYTranslation));
                    }

                    return;
                }
            }

            // we force its position to be a bit lower 
            if (CurrentLadder.CenterCharacterOnLadder) {
                _corgiController.SetTransform(new Vector2(CurrentLadder.transform.position.x,
                    transform.position.y - _climbingDownInitialYTranslation));
            }
            else {
                _corgiController.SetTransform(new Vector2(transform.position.x,
                    transform.position.y - _climbingDownInitialYTranslation));
            }
        }

        protected override void StartClimbing() {

            if (CurrentLadder.LadderPlatform != null) {
                if (AboveLadderPlatform()) {
                    return;
                }
            }

            // we rotate our character if requested
            if (ForceRightFacing) {
                _character.Face(Character.FacingDirections.Right);
            }

            SetClimbingState();

            // we set collisions
            _controller.CollisionsOn();

            if (CurrentLadder.CenterCharacterOnLadder) {
                _corgiController.SetTransform(new Vector2(CurrentLadder.transform.position.x,
                    _controller.transform.position.y));
            }
        }
        
        // Override because LadderColliding and CurrentLadder are not virtual
        protected override void HandleLadderClimbing() {
            
            if (!AbilityPermitted
                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal &&
                    _condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement)) {
                return;
            }

            // if the character is colliding with a ladder
            if (LadderColliding) {
                if ((_movement.CurrentState ==
                     CharacterStates.MovementStates.LadderClimbing) // if the character is climbing
                    && _controller.State.IsGrounded) // and is grounded
                {
	                // we make it get off the ladder
	                GetOffTheLadder();
                }

                if (_inputManager == null) {
	                return;
                }

                if (_verticalInput > _inputManager.Threshold.y // if the player is pressing up
                    && (_movement.CurrentState !=
                        CharacterStates.MovementStates.LadderClimbing
                    ) // and we're not climbing a ladder already
                    && (_movement.CurrentState !=
                        CharacterStates.MovementStates.Gliding) // and we're not gliding
                    && (_movement.CurrentState != CharacterStates.MovementStates.Jetpacking)
                ) // and we're not jetpacking
                {
	                // then the character starts climbing
	                StartClimbing();
                }

                // if the character is climbing the ladder (which means it previously connected with it)
                if (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing) {
	                Climbing();
                }

                // if the current ladder does have a ladder platform associated to it
                if (CurrentLadder.LadderPlatform != null) {
	                if ((_movement.CurrentState ==
	                     CharacterStates.MovementStates.LadderClimbing) // if the character is climbing
	                    && AboveLadderPlatform()) // and is above the ladder platform
	                {
		                // we make it get off the ladder
		                GetOffTheLadder();
	                }
                }

                if (CurrentLadder.LadderPlatform != null) {
	                if ((_movement.CurrentState !=
	                     CharacterStates.MovementStates.LadderClimbing) // if the character is climbing
	                    && (_verticalInput < -_inputManager.Threshold.y) // and is pressing down
	                    && (AboveLadderPlatform()) // and is above the ladder's platform
	                    && _controller.State.IsGrounded) // and is grounded
	                {
		                // we make it get off the ladder
		                StartClimbingDown();
	                }
                }
            }
            else {
                // if we're not colliding with a ladder, but are still in the LadderClimbing state
                if (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing) {
	                GetOffTheLadder();
                }
            }

            // we stop our sounds if needed
            if (_movement.CurrentState != CharacterStates.MovementStates.LadderClimbing &&
                _abilityInProgressSfx != null) {
                // we play our exit sound
                StopAbilityUsedSfx();
                PlayAbilityStopSfx();
            }
        }
        
        // Override because LadderColliding and CurrentLadder are not virtual
        protected override void Climbing()
        {
            // we disable the gravity
            _controller.GravityActive(false);

            if (CurrentLadder.LadderPlatform != null)
            {
                if (!AboveLadderPlatform())
                {
                    _controller.CollisionsOn();
                }
            }
            else
            {
                _controller.CollisionsOn();
            }				
			
            // we set the force according to the ladder climbing speed
            if (CurrentLadder.LadderType == Ladder.LadderTypes.Simple)
            {
                _controller.SetVerticalForce(_verticalInput * LadderClimbingSpeed);
                // we set the climbing speed state.
                CurrentLadderClimbingSpeed=Mathf.Abs(_verticalInput ) * transform.up;	
            }
            if (CurrentLadder.LadderType == Ladder.LadderTypes.BiDirectional)
            {
                _controller.SetHorizontalForce(_horizontalInput * LadderClimbingSpeed);
                _controller.SetVerticalForce(_verticalInput * LadderClimbingSpeed);
                CurrentLadderClimbingSpeed = Mathf.Abs(_horizontalInput ) * transform.right;	
                CurrentLadderClimbingSpeed += Mathf.Abs(_verticalInput ) * (Vector2)transform.up;	
            }

            if ((CurrentLadderClimbingSpeed != Vector2.zero) && (_abilityInProgressSfx == null))
            {
                PlayAbilityUsedSfx ();
            }
            if ((CurrentLadderClimbingSpeed == Vector2.zero) && (_abilityInProgressSfx != null))
            {
                StopAbilityUsedSfx ();
            }

        }
    }
}