using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cannonBehaviour : MonoBehaviour {

    public bool leftCannon = true;
    GameObject cannonBall;
    GameObject cannon;
    GameObject goatLauncher;
    GameObject goat;
    public int minRot = 10;
    public int maxRot = 60;
    public int cannonAngle;
    public int launcherAngle;

	// Use this for initialization
	void Start () { //load appropriate resources
        cannon = GameObject.FindGameObjectWithTag("cannon") as GameObject;
        goatLauncher = GameObject.FindGameObjectWithTag("goatlauncher") as GameObject;
        cannonBall = Resources.Load("Cannon Ball") as GameObject;
        goat = Resources.Load("Goat") as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Tab)) //switch cannons on tab
        {
            Debug.Log("Switching Cannons!");
            switch(leftCannon){
                case true:
                    leftCannon = false;
                    break;
                case false:
                    leftCannon = true;
                    break;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space)) //shoot on space
        {
            switch (leftCannon)
            {
                case true:
                    fireBall();
                    break;
                case false:
                    fireGoat();
                    break;
            }
        }
	}

    public int getCannonAngle() //public get
    {
        return cannonAngle;
    }

    public int getLauncherAngle() //public get
    {
        return launcherAngle;
    }

    void fireBall() //fire cannonball (simply instantiate the object and it's scripts will handle the rest)
    {
        cannonAngle = Random.Range(minRot, maxRot + 1);
        cannon.transform.localRotation = Quaternion.Euler(0, 0, cannonAngle);
        GameObject ball = Instantiate(cannonBall) as GameObject;
        Debug.Log("Cannon ball firing!");
    }
    void fireGoat() //fire goat (simply instantiate the object and it's scripts will handle the rest)
    {
        launcherAngle = Random.Range(minRot, maxRot + 1);
        goatLauncher.transform.localRotation = Quaternion.Euler(0, -180, launcherAngle);
        GameObject gt = Instantiate(goat) as GameObject;
        Debug.Log("Goat firing!");
    }
}
