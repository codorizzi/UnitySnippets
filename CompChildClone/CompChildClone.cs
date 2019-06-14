using System.Collections.Generic;
using UnityEngine;
using Utils;

public class CompChildClone : MonoBehaviour {
    
    public List<Component> ignore;

    void Start() {
        foreach(Transform child in transform) {
            foreach (Component component in gameObject.GetComponents(typeof(Component))) {

                if (ignore.Contains(component))
                    continue;
                
                ComponentUtil.CopyComponent(component, child.gameObject);
            }
        }
    }
    
}