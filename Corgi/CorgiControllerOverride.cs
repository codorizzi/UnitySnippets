using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

/*
 *
 * - Fixed defect -    Downward raycasts made too close (or in) to a platform mask with fail to detect a collide, and
 *                     would prevent additional raycasts from triggering.
 * 
 */

namespace MoreMountains.CorgiEngine {
    public class CorgiControllerOverride : CorgiController {
        
        protected bool InvalidPosition = false;
        private Vector3 SafePosition;

        protected override void EveryFrame() {
            
            ApplyGravity();
            FrameInitialization();

            // we initialize our rays
            SetRaysParameters();
            HandleMovingPlatforms();

            // we store our current speed for use in moving platforms mostly
            ForcesApplied = _speed;

            // we cast rays on all sides to check for slopes and collisions

            DetermineMovementDirection();
            if (CastRaysOnBothSides) {
                CastRaysToTheLeft();
                CastRaysToTheRight();
            }
            else {
                if (_movementDirection == -1) {
                    CastRaysToTheLeft();
                }
                else {
                    CastRaysToTheRight();
                }
            }

            CastRaysBelow();
            CastRaysAbove();

            // safety check to see if the new transform position is actually safe, no collision == safe
            RaycastHit2D rc = MMDebug.RayCast(_newPosition, Vector2.down, 0.0001f, PlatformMask, Color.black, false);
            
            // we move our transform to its next position
            if (!InvalidPosition && rc.collider == null) {
                _transform.Translate(_newPosition, Space.Self);
                SafePosition = _transform.position;
            }
            // currently sitting in an invalid position, move to last safe
            else {
                _transform.position = SafePosition;
                InvalidPosition = false;
            }

            SetRaysParameters();
            ComputeNewSpeed();
            SetStates();

            _externalForce.x = 0;
            _externalForce.y = 0;

            FrameExit();
        }


        protected override void CastRaysBelow() {
            _friction = 0;

            if (_newPosition.y < -_smallValue) {
                State.IsFalling = true;
            }
            else {
                State.IsFalling = false;
            }

            if ((Parameters.Gravity > 0) && (!State.IsFalling)) {
                State.IsCollidingBelow = false;
                return;
            }

            float rayLength = _boundsHeight / 2 + RayOffset;

            if (State.OnAMovingPlatform) {
                rayLength *= 2;
            }

            if (_newPosition.y < 0) {
                rayLength += Mathf.Abs(_newPosition.y);
            }

            _verticalRayCastFromLeft = (_boundsBottomLeftCorner + _boundsTopLeftCorner) / 2;
            _verticalRayCastToRight = (_boundsBottomRightCorner + _boundsTopRightCorner) / 2;
            _verticalRayCastFromLeft += (Vector2) transform.up * RayOffset;
            _verticalRayCastToRight += (Vector2) transform.up * RayOffset;
            _verticalRayCastFromLeft += (Vector2) transform.right * _newPosition.x;
            _verticalRayCastToRight += (Vector2) transform.right * _newPosition.x;

            if (_belowHitsStorage.Length != NumberOfVerticalRays) {
                _belowHitsStorage = new RaycastHit2D[NumberOfVerticalRays];
            }

            _raysBelowLayerMaskPlatforms = PlatformMask;

            _raysBelowLayerMaskPlatformsWithoutOneWay =
                PlatformMask & ~MidHeightOneWayPlatformMask & ~OneWayPlatformMask &
                ~MovingOneWayPlatformMask;
            _raysBelowLayerMaskPlatformsWithoutMidHeight = _raysBelowLayerMaskPlatforms & ~MidHeightOneWayPlatformMask;

            // if what we're standing on is a mid height oneway platform, we turn it into a regular platform for this frame only
            if (StandingOnLastFrame != null) {
                _savedBelowLayer = StandingOnLastFrame.layer;
                if (MidHeightOneWayPlatformMask.Contains(StandingOnLastFrame.layer)) {
                    StandingOnLastFrame.layer = LayerMask.NameToLayer("Platforms");
                }
            }

            // if we were grounded last frame, and not on a one way platform, we ignore any one way platform that would come in our path.
            if (State.WasGroundedLastFrame) {
                if (StandingOnLastFrame != null) {
                    if (!MidHeightOneWayPlatformMask.Contains(StandingOnLastFrame.layer)) {
                        _raysBelowLayerMaskPlatforms = _raysBelowLayerMaskPlatformsWithoutMidHeight;
                    }
                }
            }

            float smallestDistance = float.MaxValue;
            int smallestDistanceIndex = 0;
            bool hitConnected = false;

            for (int i = 0; i < NumberOfVerticalRays; i++) {
                Vector2 rayOriginPoint = Vector2.Lerp(_verticalRayCastFromLeft, _verticalRayCastToRight,
                    (float) i / (float) (NumberOfVerticalRays - 1));

                if ((_newPosition.y > 0) && (!State.WasGroundedLastFrame)) {
                    _belowHitsStorage[i] = MMDebug.RayCast(rayOriginPoint, -transform.up, rayLength,
                        _raysBelowLayerMaskPlatformsWithoutOneWay, Color.blue, Parameters.DrawRaycastsGizmos);
                }
                else {
                    _belowHitsStorage[i] = MMDebug.RayCast(rayOriginPoint, -transform.up, rayLength,
                        _raysBelowLayerMaskPlatforms, Color.blue, Parameters.DrawRaycastsGizmos);
                }

                float distance = MMMaths.DistanceBetweenPointAndLine(_belowHitsStorage[smallestDistanceIndex].point,
                    _verticalRayCastFromLeft, _verticalRayCastToRight);

                if (_belowHitsStorage[i]) {
                    if (_belowHitsStorage[i].collider == _ignoredCollider) {
                        continue;
                    }

                    hitConnected = true;
                    State.BelowSlopeAngle = Vector2.Angle(_belowHitsStorage[i].normal, transform.up);
                    _crossBelowSlopeAngle = Vector3.Cross(transform.up, _belowHitsStorage[i].normal);
                    if (_crossBelowSlopeAngle.z < 0) {
                        State.BelowSlopeAngle = -State.BelowSlopeAngle;
                    }

                    if (_belowHitsStorage[i].distance < smallestDistance) {
                        smallestDistanceIndex = i;
                        smallestDistance = _belowHitsStorage[i].distance;
                    }
                }


                // moved -- it's previous location prevented hitConnected from being set (defect)
                if (distance < _smallValue)
                    break;
            }

            if (hitConnected) {
                StandingOn = _belowHitsStorage[smallestDistanceIndex].collider.gameObject;
                StandingOnCollider = _belowHitsStorage[smallestDistanceIndex].collider;

                // if the character is jumping onto a (1-way) platform but not high enough, we do nothing
                if (
                    !State.WasGroundedLastFrame
                    && (smallestDistance < _boundsHeight / 2)
                    && (
                        OneWayPlatformMask.Contains(StandingOn.layer)
                        ||
                        MovingOneWayPlatformMask.Contains(StandingOn.layer)
                    )
                ) {
                    State.IsCollidingBelow = false;
                    return;
                }

                State.IsFalling = false;
                State.IsCollidingBelow = true;


                // if we're applying an external force (jumping, jetpack...) we only apply that
                if (_externalForce.y > 0 && _speed.y > 0) {
                    _newPosition.y = _speed.y * Time.deltaTime;
                    State.IsCollidingBelow = false;
                }
                // if not, we just adjust the position based on the raycast hit
                else {
                    float distance = MMMaths.DistanceBetweenPointAndLine(_belowHitsStorage[smallestDistanceIndex].point,
                        _verticalRayCastFromLeft, _verticalRayCastToRight);

                    _newPosition.y = -distance
                                     + _boundsHeight / 2
                                     + RayOffset;
                }

                if (!State.WasGroundedLastFrame && _speed.y > 0) {
                    _newPosition.y += _speed.y * Time.deltaTime;
                }

                if (Mathf.Abs(_newPosition.y) < _smallValue)
                    _newPosition.y = 0;

                // we check if whatever we're standing on applies a friction change
                _frictionTest = _belowHitsStorage[smallestDistanceIndex].collider.gameObject
                    .GetComponentNoAlloc<SurfaceModifier>();
                if (_frictionTest != null) {
                    _friction = _belowHitsStorage[smallestDistanceIndex].collider.GetComponent<SurfaceModifier>()
                        .Friction;
                }

                // we check if the character is standing on a moving platform
                _movingPlatformTest = _belowHitsStorage[smallestDistanceIndex].collider.gameObject
                    .GetComponentNoAlloc<MMPathMovement>();
                if (_movingPlatformTest != null && State.IsGrounded) {
                    _movingPlatform = _movingPlatformTest.GetComponent<MMPathMovement>();
                }
                else {
                    DetachFromMovingPlatform();
                }
            }
            else {
                State.IsCollidingBelow = false;
                if (State.OnAMovingPlatform) {
                    DetachFromMovingPlatform();
                }
            }

            if (StickToSlopes) {
                StickToSlope();
            }
        }

        public void SetTransform(Vector2 position) {

            Vector2 heading = position - (Vector2)transform.position;
            float distance = heading.magnitude;
            Vector2 direction = heading / distance;
            float angle = 0f;

            // cast box of same size as Character box collider towards the new transform, looking for Platform collisions
            RaycastHit2D hits = Physics2D.BoxCast(transform.position, _boxCollider.size, angle, direction, distance,
                PlatformMask);

            if (hits.collider == null)
                transform.position = position;

        }
        
    }
}