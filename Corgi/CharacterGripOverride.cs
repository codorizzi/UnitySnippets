using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
 
 namespace MoreMountains.CorgiEngine {
     public class CharacterGripOverride : CharacterGrip {
         
         protected CorgiControllerOverride _corgiController;

         private void Awake() {
             _corgiController = GetComponent<CorgiControllerOverride>();
         }
         
         protected override void Grip() {
             // if we're gripping to something, we disable the gravity
             if (_movement.CurrentState == CharacterStates.MovementStates.Gripping) {
                 _controller.SetForce(Vector2.zero);
                 _controller.GravityActive(false);
                 if (_characterJump != null) {
                     _characterJump.ResetNumberOfJumps();
                 }
 
                 _corgiController.SetTransform(_gripTarget.transform.position + _gripTarget.GripOffset);
             }
         }
     }
 }