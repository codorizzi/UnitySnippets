using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace MoreMountains.CorgiEngine {
    
    public enum HorizontalMovement {
        None,
        Left = 1,
        Right = -1
    }
    
    public class CharacterGravityFixed : CharacterGravity {
        
        [Header("Fixed Gravity Settings")]
        public bool wallWalking;

        private float ForceThreshold = 0.01f; // amount of force required to trigger direction
        private float _gravityDirection;

        private float _boundsHeight;
        private float _boundsWidth;
        private float _boundsDiff;

        // how far forward from center to check for cliff
        private float ledgeCheckRayOffset = 0.08f;
        // how far vertically from center to draw the ledge check line
        public float rayOffset = 0f;
        
        private CorgiControllerOverride controller;
        
        public Vector2 GravityDirectionVector { get { return MMMaths.RotateVector2 (Vector2.down, _gravityDirection);	}}

        public HorizontalMovement MovementDirection {

            get {

                HorizontalMovement direction;
                
                if (Mathf.Abs(_controller.ForcesApplied.x) < ForceThreshold)
                    return HorizontalMovement.None;

                direction = _controller.ForcesApplied.x > 0 ? HorizontalMovement.Right : HorizontalMovement.Left;

                // reverse if on the ceiling
                if (Mathf.FloorToInt(_gravityDirection) == 180)
                    return (HorizontalMovement)(-(float)direction);

                return direction;
                
            }
            
        }

        public override void Update() {
            
            if(wallWalking)
                HandleWallWalking();
            
            base.Update();
            
        }

        public virtual void HandleWallWalking() {

            HorizontalMovement direction = MovementDirection;
            
            if(controller == null) {
                controller = (CorgiControllerOverride) _controller;
                _boundsWidth = controller.BoundsWidth;
                _boundsHeight = controller.BoundsHeight;
                _boundsDiff = Mathf.Abs(_boundsWidth - _boundsHeight);
            }

            if(direction == HorizontalMovement.Left && _controller.State.IsCollidingLeft) {
                
                // shift position by difference between height / width to accommodate for pivot rotation
                controller.SetTransform(transform.position + transform.up * Mathf.Abs(_boundsDiff));

                RotateGravity(direction);
                
                // no longer colliding due to rotational shift
                _controller.State.IsCollidingLeft = false;
            
            } else if(direction == HorizontalMovement.Right && _controller.State.IsCollidingRight) {

                // shift position by difference between height / width to accommodate for pivot rotation
                controller.SetTransform(transform.position + -transform.up * Mathf.Abs(_boundsDiff));
                
                RotateGravity(direction);
                
                // no longer colliding due to rotational shift
                _controller.State.IsCollidingRight = false;
            
            } else if(IsOverLedge()) {
                
                // whether player is on vertical or horizontal plane
                //bool isVertical = _defaultGravityAngle == 90 || _defaultGravityAngle == 270;

                RotateGravity((HorizontalMovement)(-(float)direction));

                Transition(true, GravityDirectionVector);

                // get forward direction depending which direction character is facing
                Vector3 forward = _character.IsFacingRight ? transform.right : -transform.right;

                Vector3 forwardAmount = (_boundsWidth * 0.5f * forward) + (_boundsWidth * ledgeCheckRayOffset) * forward + _boundsDiff * forward;
                Vector3 downAmount = (_boundsWidth * 0.5f * -transform.up) + (_boundsWidth * ledgeCheckRayOffset * -transform.up);

                Vector3 newPosition =  transform.position +  downAmount + forwardAmount;
                
                // check whether rotation / position shift is safe
                Collider2D hit = CODebug.OverlapBox(newPosition, controller.BoxCollider.size, _defaultGravityAngle, controller.PlatformMask, Color.cyan);
                
                // if safe, perform shift
                if (hit == null) {
                    transform.localEulerAngles = new Vector3(0, 0, _defaultGravityAngle);
                
                    // shift character position by overhang extent
                    controller.SetTransform(newPosition);
                }
                else {
                    CODebug.DebugDrawBox(newPosition, controller.BoxCollider.size, _defaultGravityAngle,  Color.white, 5f);
                    // reverse gravity change
                    RotateGravity((HorizontalMovement)((float)direction));
                    
                }

            }

        }

        void RotateGravity(HorizontalMovement direction) {

            if (direction == HorizontalMovement.Left)
                _defaultGravityAngle -= 90;
            else if (direction == HorizontalMovement.Right)
                _defaultGravityAngle += 90;
            
            // normalize between 0 and 360
            _defaultGravityAngle = (_defaultGravityAngle % 360 + 360) % 360;

        }

        bool IsOverLedge() {

            if (!_controller.State.IsGrounded)
                return false; 
            
            // cast ray a little forward than center to see if overhang is > 50%
            Vector3 forward = _character.IsFacingRight ? transform.right : -transform.right;
            Vector2 origin = Vector2.MoveTowards(transform.position, transform.position + forward, ledgeCheckRayOffset);
            float rayLength = ((CorgiControllerOverride)_controller).BoundsHeight / 2 + 0.05f + rayOffset;
            RaycastHit2D rc = MMDebug.RayCast(origin, -transform.up, rayLength, _controller.PlatformMask, Color.magenta, true);

            if (rc.collider == null)
                return true;
            
            return false;
        }

        public override void ResetGravityToDefault() {
            
            // TODO - update to allow different start gravity
            _gravityDirection = 0f;
            _defaultGravityAngle = 0f;
            
            base.ResetGravityToDefault();
        }
    }
}