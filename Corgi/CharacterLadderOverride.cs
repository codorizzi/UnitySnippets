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

public class CharacterLadderOverride : CharacterLadder {
    
    private BoxCollider2D _boxCollider;
    public LayerMask _ladderMask;
    private ContactFilter2D _ladderFilter;
    
    public new  bool LadderColliding => _boxCollider.IsTouchingLayers(_ladderMask);

    /// the ladder the character is currently on
    public new Ladder CurrentLadder {
        get {
            
            if (!_boxCollider.IsTouchingLayers(_ladderMask)) 
                return null;
            
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(_ladderMask);
				
            List<Collider2D> colliders = new List<Collider2D>();
            _boxCollider.OverlapCollider(filter, colliders);

            return colliders[0].GetComponent<Ladder>();
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
    
    protected override void StartClimbingDown()
    {
        SetClimbingState();
        _controller.CollisionsOff();
        _controller.ResetColliderSize ();

        // we rotate our character if requested
        if (ForceRightFacing)
        {
            _character.Face(Character.FacingDirections.Right);
        }

        if (_characterGravity != null)
        {
            if (_characterGravity.ShouldReverseInput ())
            {
                if (CurrentLadder.CenterCharacterOnLadder)
                {
                    _corgiController.SetTransform(new Vector2(CurrentLadder.transform.position.x, transform.position.y + _climbingDownInitialYTranslation));
                }
                else {
                    _corgiController.SetTransform(new Vector2(transform.position.x, transform.position.y + _climbingDownInitialYTranslation));
                }
                return;
            }
        }

        // we force its position to be a bit lower 
        if (CurrentLadder.CenterCharacterOnLadder)
        {
            _corgiController.SetTransform(new Vector2(CurrentLadder.transform.position.x, transform.position.y - _climbingDownInitialYTranslation));
        }
        else
        {
            _corgiController.SetTransform(new Vector2(transform.position.x, transform.position.y - _climbingDownInitialYTranslation));
        }
    }
    
    protected override void StartClimbing() {

        if (CurrentLadder.LadderPlatform != null)
        {
            if (AboveLadderPlatform()) 
            {
                return;
            }
        }

        // we rotate our character if requested
        if (ForceRightFacing)
        {
            _character.Face(Character.FacingDirections.Right);
        }

        SetClimbingState();

        // we set collisions
        _controller.CollisionsOn();

        if (CurrentLadder.CenterCharacterOnLadder)
        {
            _corgiController.SetTransform(new Vector2(CurrentLadder.transform.position.x,_controller.transform.position.y));
        }
    }
    
}
