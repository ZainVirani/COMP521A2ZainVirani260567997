using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class windBehaviour : MonoBehaviour {

    float windSpeed;
    public float maxSpeed = 1f;
    public GameObject arrow;
    public GameObject head;
    // Use this for initialization
    void Start () {
        windSpeed = 0; //init
        arrow = GameObject.FindGameObjectWithTag("arrow");
        head = GameObject.FindGameObjectWithTag("head");
        InvokeRepeating("randomWind", 0f, 0.5f); //randomize wind starting now, repeating every half a second
    }

    void randomWind()
    {
        windSpeed = Random.Range(-maxSpeed, maxSpeed + 0.001f); //min is inclusive, max exclusive so we add a minimal amount to account for it
        arrow.transform.localScale = new Vector3((windSpeed) / maxSpeed, 1, 1); //arrow represents wind direction and speed, scale accordingly
        head.transform.localScale = new Vector3(.45f, .3f, 1);
    }

    public float getWind() //public get
    {
        return windSpeed;
    }
}
