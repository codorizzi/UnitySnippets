using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine {
    /// <summary>
    /// Requires a CharacterFly ability. Makes the character fly up to the specified MinimumDistance in the direction of the target. That's how the RetroGhosts move.
    /// </summary>
    [RequireComponent(typeof(CharacterFly))]
    public class AIActionFlyRandom : AIAction {
        /// the minimum distance from the target this Character can reach.
        public float MinimumDistance = 1f;

        protected CharacterFly _characterFly;
        protected int _numberOfJumps = 0;

        /// <summary>
        /// On init we grab our CharacterFly ability
        /// </summary>
        protected override void Initialization() {
            _characterFly = gameObject.GetComponent<CharacterFly>();
        }

        /// <summary>
        /// On PerformAction we fly
        /// </summary>
        public override void PerformAction() {
            Fly();
        }

        /// <summary>
        /// Moves the character towards the target if needed
        /// </summary>
        protected virtual void Fly() {

            bool vert,
                 horz;

            vert = Random.Range(1, 100) < 50;
            horz = Random.Range(1, 100) < 50; 
            
            
            if (horz)
                _characterFly.SetHorizontalMove(1f);
            else 
                _characterFly.SetHorizontalMove(-1f);
            
            if (vert) 
                _characterFly.SetVerticalMove(1f);
            else 
                _characterFly.SetVerticalMove(-1f);
            
        }

        /// <summary>
        /// On exit state we stop our movement
        /// </summary>
        public override void OnExitState() {
            base.OnExitState();

            _characterFly.SetHorizontalMove(0f);
            _characterFly.SetVerticalMove(0f);
        }
    }
}