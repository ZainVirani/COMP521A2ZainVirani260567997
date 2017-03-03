using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collision : MonoBehaviour {

    //Vector3[] ballVertices;
    Vector3[] mountain;
    float r = 0.75f;
    public float tolerance = 0;
    cannonBallBehaviour cBB;
    GameObject[] goats;
	// Use this for initialization
	void Start () {
        //updateBallVertices();
        cBB = gameObject.GetComponent<cannonBallBehaviour>();
        mountain = GameObject.FindGameObjectWithTag("mountain").GetComponent<mountainVertices>().getMountain();
        goats = GameObject.FindGameObjectsWithTag("goat");
    }
	
	// Update is called once per frame
	void Update () {
        if (checkIntersections())
        {
            Debug.Log("WE GOTTA BOUNCE");
        }
        checkGoat();
    }

    void checkGoat() //goat collision
    {
        foreach(GameObject g in goats) //find all goats
        {
            Transform[] points = g.GetComponent<goat>().points;
            for (int i = 0; i < points.Length; i++) //check all goat points against the ball position
            {
                if(Vector3.Distance(transform.position, points[i].position) <= Mathf.Abs(r / 2 + tolerance))
                {
                    g.GetComponent<goat>().cannonBallCollision(i, cBB.velX, cBB.velY); //handle collision
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }

    public bool checkIntersections()
    {
        //updateBallVertices();
        //this attempts to use 8 fake vertices on the ball and tests for line intersections with lines between adjacent mountain vertices (doesn't work at all though)
        /*if (transform.position.x >= 9.6f)
        {
            for (int i = 0; i < 8; i++) //check each line on the ball
            {
                for (int j = 0; j < 32; j++) //against each line on the mountain
                {
                    if (i == 7)
                    {
                        if (checkLines(ballVertices[i], ballVertices[0], mountain[j], mountain[j + 1]))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (checkLines(ballVertices[i], ballVertices[i + 1], mountain[j], mountain[j + 1]))
                        {
                            return true;
                        }
                    }
                }
            }
        }*/

        //this attempts to check distances between the coordinates of the ball and lines between adjacent mountain vertices
        if(transform.position.x >= 9.6f)
        {
            for(int i = 0; i < 55; i++)
            {
                //float distance = Mathf.Abs(distanceToLineSegment(mountain[i], mountain[i + 1], transform.position));
                //Debug.Log(mountain[i].ToString() + " " + mountain[i+1].ToString() + " " + transform.position.ToString()); //purely for debugging ONLY ONE fire unless you want to slow your machine down
                //if (Mathf.Abs(cBB.velY) >= 40) tolerance = 0.1f;
                if (Vector3.Distance(transform.position, mountain[i]) <= Mathf.Abs(r/2 + tolerance)) //indicates collision
                {
                    //Debug.Log(mountain[i].ToString() + " " + mountain[i+1].ToString() + " " + transform.position.ToString());
                    //Debug.Log(distance);

                    //find the correct line to represent the bounce normal
                    if (i == 0)
                    {
                        cBB.bounce(mountain[i], mountain[i + 1], i);
                    }
                    else if (i == 54)
                    {
                        cBB.bounce(mountain[i - 1], mountain[i], i);
                    }
                    else if (i != 0 &&Vector3.Distance(transform.position, mountain[i + 1]) < Vector3.Distance(transform.position, mountain[i - 1]))
                    {
                        //avoids issues with corners
                        if (i == 10) i = 11;
                        if (i == 21) i = 22;
                        if (i == 32) i = 33;
                        if (i == 43) i = 44;
                        cBB.bounce(mountain[i], mountain[i + 1], i);
                    }
                    else
                    {
                        //avoids issues with corners
                        if (i == 11) i = 10;
                        if (i == 22) i = 21;
                        if (i == 33) i = 32;
                        if (i == 44) i = 43;
                        cBB.bounce(mountain[i - 1], mountain[i], i);
                    }
                    
                    return true;
                }
            }
        }
        return false;
    }
    
    //stuff that i tried but didn't work, including point to line distance, and line intersection + some linear algebra

    /*float distanceToLineSegment(Vector3 A, Vector3 B, Vector3 P)
    {
        Vector3 AP = P - A;       //Vector from A to P   
        Vector3 AB = B - A;       //Vector from A to B  

        float magnitudeAB = AB.sqrMagnitude;     //Magnitude of AB vector (it's length squared)     
        float ABAPproduct = Vector3.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
        float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  
        return distance;
    }

    private bool checkLines(Vector3 pointA1, Vector3 pointA2, Vector3 pointB1, Vector3 pointB2)
    {
        // Get A,B,C of first line - points : pointA1 to pointA2
        float A1 = pointA2.y - pointA1.y;
        float B1 = pointA1.x - pointA2.x;
        float C1 = A1 * pointA1.x + B1 * pointA1.y;

        // Get A,B,C of second line - points : pointB1 to pointB2
        float A2 = pointB2.y - pointB1.y;
        float B2 = pointB1.x - pointB2.x;
        float C2 = A2 * pointB1.x + B2 * pointB1.y;

        // Get delta and check if the lines are parallel
        float delta = A1 * B2 - A2 * B1;
        if (delta == 0) return false;// return Vector3.zero;//lines don't intersect

        // Return the Vector3 intersection point
        return isPointOnLine(pointA1, pointA2, new Vector3((B2 * C1 - B1 * C2) / delta, (A1 * C2 - A2 * C1) / delta, 0));
        //return new Vector3((B2 * C1 - B1 * C2) / delta, (A1 * C2 - A2 * C1) / delta);
    }

    bool isPointOnLine(Vector3 pointA1, Vector3 pointA2, Vector3 point)
    {
        if (pointA1.x < pointA2.x)
        {
            if (point.x >= pointA1.x - tolerance && pointA2.x + tolerance >= point.x)
            {
                return true;
            }
            else
            {
                //Debug.Log("no int");
                return false;
            }
        }
        else
        {
            if (point.x <= pointA1.x + tolerance && pointA2.x - tolerance <= point.x)
            {
                return true;
            }
            else
            {
                //Debug.Log("no int");
                return false;
            }
        }
    }

    void updateBallVertices() //unit circle calculations for 8 vertices N, NE, E, SE, S, SW, W, NW
    {
        ballVertices = new Vector3[8];
        ballVertices[0] = new Vector3(transform.position.x, transform.position.y + r, 0);
        ballVertices[1] = new Vector3(transform.position.x + r / Mathf.Sqrt(2), transform.position.y + r / Mathf.Sqrt(2), 0);
        ballVertices[2] = new Vector3(transform.position.x + r, transform.position.y, 0);
        ballVertices[3] = new Vector3(transform.position.x + r / Mathf.Sqrt(2), transform.position.y - r / Mathf.Sqrt(2), 0);
        ballVertices[4] = new Vector3(transform.position.x, transform.position.y - r, 0);
        ballVertices[5] = new Vector3(transform.position.x - r / Mathf.Sqrt(2), transform.position.y - r / Mathf.Sqrt(2), 0);
        ballVertices[6] = new Vector3(transform.position.x - r, transform.position.y, 0);
        ballVertices[7] = new Vector3(transform.position.x - r / Mathf.Sqrt(2), transform.position.y + r / Mathf.Sqrt(2), 0);
    }*/


}
