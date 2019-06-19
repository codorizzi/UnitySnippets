using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// This component allows your character to fly by moving gravity-free on both x and y axis. Here you can define the flight speed, as well as whether or not the character is always flying (in which case you don't have to press a button to fly). Important note : slope ceilings are not supported for now.
    /// </summary>
    public class CharacterFlyLurch : CharacterFly {

        public float MaxLurchSpeed = 6f;
        public float MinLurchSpeed;
        
        public float LurchSlowdown = 1f;

        protected override void HandleMovement() {

            if (FlySpeed == MinLurchSpeed)
                FlySpeed = MaxLurchSpeed;
            else {
                FlySpeed = FlySpeed - LurchSlowdown;
                if (FlySpeed < 0)
                    FlySpeed = 0;
            }
            
            base.HandleMovement();
        }
    }

}
