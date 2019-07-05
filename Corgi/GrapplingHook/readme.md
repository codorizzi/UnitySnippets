# Corgi Rope / Grappling Hook

## GrapplingHook
The GrapplingHook class includes a method for deploying rope pieces (intended to be GO with Distance Joint 2D) between itself and a target player.

### Setup
GrappleHook Game Object
* Sprite Renderer
* RigidBody 2D
* Grapple Hook component
    * Rope Piece prefab
    * Distance Between = appropriate for sprite used as rope piece
    * Source = Character or other source (default is to grab first GameObject with player tag if not set)
    * Has Grip = Bool that will add a grip on last piece if True

Rope Piece
* Sprite Renderer
* Rigid Body 2D
* Distance Joint 2D
* Box Collider 2D
* Ladder
* Rotation Constant (optional)

See attached package for examples

## GrappleAbility
This was mainly a quick test method for attaching the GrappleHook to a Character for easy trigger. It's currently just a hollowed out version of the Dash ability. More work likely required before production readiness.

## Sample Project
https://searle.itch.io/platformer2

# Corgi Lurch Flying Enemy

## CharacterFlyLurch
Override of CharacterFly that allows a flying in a lurch fashion (e.g. jellyfish type movement)

## AIActionFlyRandom
Similar to AIActionFlyTowardTarget, except the direction is chosen at random upon decision.

## Sample Project
https://searle.itch.io/platformer4

# Wallwalk

## CharacterGravityOverride
Allows any AI / Player character to wall walk in 360 degrees. Requires CharacterControllerOverride adn CODebug.

## Sample Project
https://searle.itch.io/platformer4
