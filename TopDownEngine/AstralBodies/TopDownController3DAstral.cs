using System.Collections;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class TopDownController3DAstral : TopDownController3D {
    
    protected Vector3 Inertia;
    protected Vector3 LastPosition;

    [Header("Gravitational Attraction")]
    public LayerMask AttractiveLayers;

    protected override void ProcessUpdate() {
        
        if (_transform == null)
            return;

        if (!FreeMovement)
            return;
        
        _newVelocity = Velocity;
        _positionLastFrame = _transform.position;

        AddInput();
        AddAttractiveForces();
        AddGravity();
        AddInertia();
        MoveWithPlatform();
        ComputeVelocityDelta();
        MoveCharacterController();
        DetectNewMovingPlatform();
        ComputeNewVelocity();
        StickToTheGround();
        StoreInteria();
    }

    protected void AddInertia() {
        _newVelocity.x += Inertia.x;
        //_newVelocity.y += _inertia.y; // currently only interested in 2D
        _newVelocity.z += Inertia.z;
    }

    protected void StoreInteria() {
        
        Inertia = _newVelocity;

        if (LastPosition.x == transform.position.x)
            Inertia.x = 0;
        
        // only interested in 2D
//        if (LastPosition.y == transform.position.y) 
//            Inertia.y = 0;

        if (LastPosition.z == transform.position.z) 
            Inertia.z = 0;

        LastPosition = transform.position;
        
    }

    protected void AddAttractiveForces() {

        foreach (AttractiveBody attractor in AttractiveBody.Attractors) {

            // ignore attractors not in mask
            if (((1 << attractor.gameObject.layer) & AttractiveLayers) == 0)
                continue;

            Vector3 direction = attractor.transform.position - transform.position;
            float distance = direction.magnitude;

            if (distance == 0f)
                continue;
            
            float forceMagnitude = attractor.mass / Mathf.Pow(distance, 2);
            Vector3 force = direction.normalized * forceMagnitude;

            AddedForce += force;

        }
        
    }

}
