using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Sirenix.OdinInspector;

[RequireComponent(typeof(SortingGroup))]
public class DottedLine : MonoBehaviour {

    // start of line
    [OnValueChanged("renderLine")]
    public Vector2 point1;

    // end of line    
    [OnValueChanged("renderLine")]
    public Vector2 point2;    
	
	// dot used for line
	public Sprite dotSprite;
    
    [OnValueChanged("renderLine")]
    public float size;            
    
    [OnValueChanged("renderLine")]
    public float spacing;    
    
    [OnValueChanged("renderLine")]
    public Color32 color = new Color32(255, 255, 255, 255);    
	
	// cached and active dots for line
	List<GameObject> dotsCache = new List<GameObject>();
	List<GameObject> dotsActive = new List<GameObject>();
	
	Vector3 rotation; // internally used to store direction dots should point

    void Start() {        
        renderLine();
    }

	// add dot to game view
	private void addDot(Vector2 point) {
		
		GameObject dot;
		if (dotsCache.Count == 0) 
			dot = createNewDot();
		else
			dot = getCachedDot();
		
		dot.transform.position = point;			
		dot.transform.localScale = Vector3.one * size;
		dot.GetComponent<SpriteRenderer>().color = color;        

		dot.transform.eulerAngles = rotation;
		
	}
	
	// create a brand new dot GameObject
	private GameObject createNewDot() {
		
		GameObject dot = new GameObject();

		dot.transform.parent = transform;
		dot.AddComponent<SpriteRenderer>().sprite = dotSprite;		
        dot.hideFlags = HideFlags.HideInHierarchy;
        dot.name = "dot";

        dotsActive.Add(dot);
		
		return dot;
		
	}
		
	// fetch an already created dot from cache
	private GameObject getCachedDot() {
		
		GameObject dot = dotsCache[0];
		dotsCache.Remove(dot);
		dot.SetActive(true);
		
		dotsActive.Add(dot);
		
		return dot;
		
	}
	
	// deactivate and store created dot into cache
	private void cacheDot(GameObject dot) {
		dot.SetActive(false);
		dotsActive.Remove(dot);
		dotsCache.Add(dot);
	}
	
	// clear existing line and replace dots
	private void renderLine() {
        
        if(!Application.isPlaying)
            return;

        clearLine();

        setRotation();
		
		Vector2 direction = (point2 - point1).normalized;
		Vector2 currentPoint = point1;
		
		while ((point2 - point1).magnitude > (currentPoint - point1).magnitude) {
			addDot(currentPoint);
			currentPoint += (direction * spacing);			
		}
		
	}
	
	// clear existing line
	private void clearLine() {		

		while(dotsActive.Count > 0)
			cacheDot(dotsActive[0]);		
	}
	
	// set the direction that all dots should point
	private void setRotation() {

        Vector3 heading = (point2 - point1);
        float z = Quaternion.FromToRotation(Vector3.up, heading).eulerAngles.z;
        rotation = new Vector3(0, 0, z);
	
	}
	
}
