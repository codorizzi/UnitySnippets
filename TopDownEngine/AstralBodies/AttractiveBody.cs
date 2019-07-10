using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractiveBody : MonoBehaviour {
	
	public static List<AttractiveBody> Attractors;

	public float mass;

	void OnEnable () {
		if (Attractors == null)
			Attractors = new List<AttractiveBody>();

		Attractors.Add(this);
	}

	void OnDisable () {
		Attractors.Remove(this);
	}

}