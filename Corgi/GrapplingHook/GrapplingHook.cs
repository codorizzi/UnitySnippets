using System.Collections.Generic;
using System.Linq;
using MoreMountains.CorgiEngine;
using Sirenix.OdinInspector;
using UnityEngine;

public class GrapplingHook : MonoBehaviour {
    
    public GameObject ropePiece;
    public float distanceBetweenPieces;
    public GameObject source;
    
    private List<GameObject> _pieces;

    public bool hasGrip;

    private void Awake() {
        _pieces = new List<GameObject>();
    }

    [Button]
    
    public void DeployHook() {

        if(source == null)
            source = GameObject.FindGameObjectWithTag("Player");

        Vector2 target = source.transform.position;
        Vector2 step = Vector2.MoveTowards(transform.position, target, distanceBetweenPieces);
        
        // initial connection
        AddPiece(step, gameObject);

        BoxCollider2D sourceCollider = source.GetComponent<BoxCollider2D>();
        
        while (!step.Equals(target) && !sourceCollider.bounds.Contains(step)) {
            step = Vector2.MoveTowards(step, target, distanceBetweenPieces);
            AddPiece(step, _pieces.Last());
        }

        if (hasGrip)
            _pieces.Last().AddComponent<Grip>();

    }

    private void AddPiece(Vector2 position, GameObject connection) {
        
        GameObject piece = Instantiate(ropePiece, position, new Quaternion(), transform);
            
        DistanceJoint2D joint = piece.GetComponent<DistanceJoint2D>();
        joint.distance = distanceBetweenPieces;

        Rigidbody2D lastPiece = connection.GetComponent<Rigidbody2D>();

        joint.connectedBody = lastPiece;
        _pieces.Add(piece);
            
    }

    
}
