using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

public class DamageOnTouchOverride : DamageOnTouch {

    [Header("TileMap")] 
    public int radius = 1;
    public LayerMask PlatformMask;

    protected override void Colliding(Collider2D collider) {
        
        checkTilemapCollision(collider);
        base.Colliding(collider);
    }

    void checkTilemapCollision(Collider2D collider) {
        
        DestructibleTilemap dtm = collider.gameObject.GetComponent<DestructibleTilemap>();

        if (dtm == null)
            return;

        Vector3 origin = _boxCollider2D.transform.position;

        RaycastHit2D hit = Physics2D.BoxCast(_boxCollider2D.transform.position, _boxCollider2D.size, transform.rotation.eulerAngles.z,
            transform.right, 0.01f, PlatformMask);

        dtm.DamageTiles(hit.point, DamageCaused, radius);

    }
    
}
