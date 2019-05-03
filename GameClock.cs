using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class GameClock : MonoBehaviour {

    public static GameClock instance;

    public float timeBetweenTicks;

    float lastUpdate;
    float _timeRemaining;
    bool _pauseTimer;

    public bool repeatOnElapse = true;

    [ShowInInspector]
    public float timeRemaining {
        get { return _timeRemaining; }
        set {

            if (value < 0)
                value = 0;

            _timeRemaining = value;

            if (value > 0)
                return;
                                
            tick.Invoke();

            if(repeatOnElapse)
                restart();

        }
    }
    
    [ShowInInspector]
    public bool pauseTimer {
        get { return _pauseTimer; }
        set {
            _pauseTimer = value;
            if (!value)
                lastUpdate = Time.time;
        }
    }

    [System.Serializable]
    public class Tick : UnityEvent { }
    [HideInInspector] public UnityEvent tick = new Tick();

    void Awake() {
        instance = this;        
    }

	// Use this for initialization
	void Start () {
        restart();	
	}
	
	// Update is called once per frame
	void Update () {        

        float elapsed = Time.time - lastUpdate;
        lastUpdate = Time.time;

        if (pauseTimer)
            return;

        if (timeRemaining > 0)
            timeRemaining -= elapsed;

    }

    public void restart() {
        timeRemaining = timeBetweenTicks;
        lastUpdate = Time.time;
        pauseTimer = false;
    }

}
