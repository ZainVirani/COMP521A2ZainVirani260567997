using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goat : MonoBehaviour {

    float posX = 26.48f;
    float posY = 1.4f;
    public Transform[] points;
    LineRenderer[] lines;
    Transform head;
    Transform body;

    public float gravity = -0.032f;
    public float airResistanceX = -0.01f;
    public float airResistanceY = 0.01f;
    public float initialVelocity = -15f;
    public float restitution = 0.5f;
    Vector3[] currentPos;
    Vector3[] previousPos;
    public float[] lengthConstraints; //length to vertex i + 1
    float lengthTolerance;
    public float[] angleConstraints; //angle i-1, i, i+1
    float angleTolerance;
    public float[] velX;
    public float[] velY;
    int angle;
    float windSpeed;
    windBehaviour wind;
    public bool updateOff = true;
    bool windOff = true;
    bool firstMove = true;
    public bool collision = false;

    Vector3[] mountain;

    // Use this for initialization
    void Start () {
        transform.position = new Vector3(posX, posY, 0);
        points = new Transform[20];
        lines = new LineRenderer[19];
        currentPos = new Vector3[20];
        previousPos = new Vector3[20];
        lengthConstraints = new float[20];
        lengthTolerance = 0.1f;
        angleConstraints = new float[20];
        angleTolerance = 0.1f;
        velX = new float[20]; //this does not store velocity, rather it is just used to calculate inertia for each point
        velY = new float[20];
        head = transform.Find("Head");
        body = transform.Find("Body");
        mountain = GameObject.FindGameObjectWithTag("mountain").GetComponent<mountainVertices>().getMountain();
        draw(true);
        buildConstraints();
        wind = GameObject.FindGameObjectWithTag("cannons").GetComponent<windBehaviour>();
        windSpeed = 0;
        angle = GameObject.FindGameObjectWithTag("cannons").GetComponent<cannonBehaviour>().getLauncherAngle();
        updateOff = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (!updateOff)
        {
            moveHead(); //apply initial velocity and then inertia to head
            firstMove = false;
            
        }
        //Destroy if not moving
        for (int i = 0; i < 20; i++)
        {
            checkCollision(i);
            if (points[i].position.x <= -2 || points[i].position.x >= 30 || points[i].position.y <= -1)
            {
                Destroy(gameObject);
            }
            if (points[i].position.y <= 1 && (points[i].position.x <= 9 || points[i].position.x >= 19))
            {
                Destroy(gameObject);
            }
        }
        if (!updateOff)
        {
            //move body verlets to satisfy constraints
            updateBody();
        }
        draw(false);
	}

    public void cannonBallCollision(int i, float cVelX, float cVelY)
    {
        //move the hit verlet in the direction of cannonball velocity, then move it back to satisfy constraints (the draw happens in the next frame)
        Vector3 temp = points[i].position;
        points[i].position += new Vector3(cVelX/50, cVelY/50, 0);
        draw(false);
        while(Vector3.Distance(points[i].position, temp) > 0.001f)
        {
            
            points[i].position = Vector3.MoveTowards(points[i].position, temp, 0.0001f);
            //draw(false);
        }
    }

    void reformShape(int closest)
    {
        //move head to appropriate position (this is what updateBody should base its constraints on)
        Vector3 AB = (mountain[closest] - mountain[closest + 2]).normalized;
        Vector3 halfNormal = new Vector3(AB.y * 0.5f, -AB.x * 0.5f, 0);
        points[10].position = points[17].position + halfNormal;
        points[11].position = points[18].position + halfNormal;
        float angle = Vector3.Angle(Vector3.left, AB);
        if (points[17].position.x < 14)
        {
            angle *= -1;
            angle += 90;
        }
        else if (angle == 0);
        else angle -= 90;
        head.rotation = Quaternion.Euler(0, 0, angle);
        Vector3 zlocation = points[18].position + new Vector3(AB.x * 0.7f, AB.y * 0.7f, 0) + new Vector3 (AB.y * .5f, -AB.x * .5f, 0);
        head.position += (zlocation - points[0].position);
        for (int i = 0; i < 20; i++)
        {
            velX[i] = 0;
            velY[i] = 0;
        }

        //updateBody(); //for some reason this doesn't work here.... if it did the constraints would be satisfied
        //instead i attempt to reform some shapes below
        float distance = Vector3.Distance(points[7].position, points[17].position);
        if (distance > 1)
        {
            while (distance > 1)
            {
                points[7].position = Vector3.MoveTowards(points[7].position, points[17].position, 0.01f);
                points[6].position = Vector3.MoveTowards(points[7].position, points[17].position, 0.01f);
                points[8].position = Vector3.MoveTowards(points[7].position, points[17].position, 0.01f);
                points[9].position = Vector3.MoveTowards(points[7].position, points[17].position, 0.01f);
                distance = Vector3.Distance(points[7].position, points[17].position);
            }
            
        }
        distance = Vector3.Distance(points[12].position, points[11].position);
        if (distance > 0.2)
        {
            while (distance > 0.2)
            {
                points[12].position = Vector3.MoveTowards(points[12].position, points[11].position, 0.01f);
                distance = Vector3.Distance(points[12].position, points[11].position);
            }

        }

    }

    bool checkCollision(int j) //check collision with mountain vertices
    {
        for (int i = 0; i < 55; i++)
        {
            if (Vector3.Distance(points[j].position, mountain[i]) <= 0.1f)
            {
                updateOff = true;
                if(!collision) updateBody();
                handleCollision();
                return true;
            }
        }
        return false;
    }

    void handleCollision() //on collision, stop movement and move feet to closest mountain position
    {
        int closest = 0;
        for (int i = 0; i < 55; i++)
        {
            if (Vector3.Distance(points[18].position, mountain[i]) <= Vector3.Distance(points[18].position, mountain[closest]))
            {
                closest = i;
            }
        }
        if (closest == 54) closest = 52;
        points[18].position = mountain[closest];
        points[17].position = mountain[closest + 2];
        if (!collision) reformShape(closest);
        collision = true;
    }

    void moveHead()
    {
        //Debug.Log("move head called");
        for(int i = 0; i < 20; i++)
        {
            if (i == 6) i = 13;
            if (i == 17) i = 19;
            //Debug.Log("MOVING VERTEX " + i);

            //apply initial velocity on first movement, inertia afterwards
            if(!firstMove) velX[i] = points[i].position.x - previousPos[i].x;
            else velX[i] = initialVelocity * Mathf.Cos(Mathf.Deg2Rad * angle) / 20;
            if (!firstMove) velY[i] = points[i].position.y - previousPos[i].y;
            else velY[i] = -initialVelocity * Mathf.Sin(Mathf.Deg2Rad * angle) / 20;

            if (!windOff) windSpeed = wind.getWind()/100; //get wind
            else windSpeed = 0;

            //Apply air resistance to velocity
            if (velX[i] < 0)
            {
                velX[i] = velX[i] + (velX[i] * airResistanceX);
            }
            else {
                velX[i] = velX[i] - (velX[i] * airResistanceX);
            }
            if (velY[i] < 0)
            {
                velY[i] = velY[i] + (velY[i] * airResistanceY);
            }
            else {
                velY[i] = velY[i] - (velY[i] * airResistanceY);
            }

            //Apply gravity to velocity with v = v' + at where a=gravity 
            velY[i] = velY[i] + gravity;
            
            //verlet formulas
            currentPos[i].x = points[i].position.x + velX[i] + windSpeed; //+ accX * timestepSq;
            currentPos[i].y = points[i].position.y + velY[i]; //+ accY * timestepSq;
            
            previousPos[i].x = points[i].position.x;
            previousPos[i].y = points[i].position.y;
            

            //points[i].position = Vector3.Lerp(points[i].position, new Vector3(currentPos[i].x, currentPos[i].y, 0), lerp);
            points[i].position = new Vector3(currentPos[i].x, currentPos[i].y, 0);
        }
        //Debug.Log(velX[0] + " " + velY[0]);
    }

    void updateBody()
    {
        //move body verlets to satisfy constraints based on movement of head verlets
        for (int i = 5; i < 7; i++) //top half
        {
            float maxL = lengthConstraints[i] * (1 + lengthTolerance);
            float minL = lengthConstraints[i] * (1 - lengthTolerance);

            float distance = (points[i].position - points[i + 1].position).magnitude;
            //adjust lengths
            if (distance < minL)
            {
                while (distance < minL)
                {
                    points[i + 1].position = Vector3.MoveTowards(points[i + 1].position, points[i].position, -0.01f);
                    distance = (points[i].position - points[i + 1].position).magnitude;
                }
            }
            else if (distance > maxL)
            {
                while (distance > maxL)
                {
                    points[i + 1].position = Vector3.MoveTowards(points[i + 1].position, points[i].position, 0.01f);
                    distance = (points[i].position - points[i + 1].position).magnitude;
                }
            }

            float maxA = angleConstraints[i] * (1 + angleTolerance);
            float minA = angleConstraints[i] * (1 - angleTolerance);
            float angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
            //rotate on z axis through its neighbour to satisfy angle constraints
            if (angle < minA)
            {
                //Debug.Log("STUCK ON ITERATION " + i);
                while (angle < minA)
                {
                    points[i + 1].RotateAround(points[i].position, Vector3.forward, 1);
                    angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
                }
            }
            else if (angle > maxA)
            {
                //Debug.Log("STUCK ON ITERATION " + i);
                while (angle > maxA)
                {
                    points[i + 1].RotateAround(points[i].position, Vector3.forward, 1);
                    angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
                }
            }
            //adjust rotation based on direction of movement
            if (velY[0] >= 0)
            {
                //points[i + 1].RotateAround(points[i].position, Vector3.forward, 0);
            }
            else
            {
                points[i + 1].RotateAround(points[i].position, Vector3.forward, 270);
            }
                
        }

        //the next two for loops work exactly the same as the previous, just adjusted for different verlet locations in relation to the head

        for (int i = 13; i > 6; i--) //bottom half
        {
            float maxL = lengthConstraints[i - 1] * (1 + lengthTolerance);
            float minL = lengthConstraints[i - 1] * (1 - lengthTolerance);
            float distance = (points[i].position - points[i - 1].position).magnitude;
            if (distance < minL)
            {
                while (distance < minL)
                {
                    points[i - 1].position = Vector3.MoveTowards(points[i - 1].position, points[i].position, -0.01f);
                    distance = (points[i].position - points[i - 1].position).magnitude;
                }
            }
            else if (distance > maxL)
            {
                while (distance > maxL)
                {
                    points[i - 1].position = Vector3.MoveTowards(points[i - 1].position, points[i].position, 0.01f);
                    distance = (points[i].position - points[i - 1].position).magnitude;
                }
            }

            float maxA = angleConstraints[i] * (1 + angleTolerance);
            float minA = angleConstraints[i] * (1 - angleTolerance);
            float angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
            if (angle < minA)
            {
                while (angle < minA)
                {
                    points[i - 1].RotateAround(points[i].position, Vector3.forward, 1);
                    angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
                }
            }
            else if (angle > maxA)
            {
                while (angle > maxA)
                {
                    points[i - 1].RotateAround(points[i].position, Vector3.forward, 1);
                    angle = Vector3.Angle(points[i - 1].position - points[i].position, points[i + 1].position - points[i].position);
                }
            }
            if (velY[0] >= 0)
            {
                if (i == 13) points[i - 1].RotateAround(points[i].position, Vector3.forward, 180);
                if (i == 8) points[i - 1].RotateAround(points[i].position, Vector3.forward, -90);
            }
            else
            {
                if (i == 13) points[i - 1].RotateAround(points[i].position, Vector3.forward, -90);
                if (i == 8) points[i - 1].RotateAround(points[i].position, Vector3.forward, 225);
            }
            
        }


        for (int i = 17; i < 19; i++) //legs
        {
            float maxL = lengthConstraints[i] * (1 + lengthTolerance);
            float minL = lengthConstraints[i] * (1 - lengthTolerance);
            float distance = (points[i].position - points[i - 7].position).magnitude;
            if (distance < minL)
            {
                while (distance < minL)
                {
                    points[i].position = Vector3.MoveTowards(points[i].position, points[i - 7].position, -0.01f);
                    distance = (points[i].position - points[i - 7].position).magnitude;
                }
            }
            else if (distance > maxL)
            {
                while (distance > maxL)
                {
                    points[i].position = Vector3.MoveTowards(points[i].position, points[i - 7].position, 0.01f);
                    distance = (points[i].position - points[i - 7].position).magnitude;
                }
            }
            float maxA = angleConstraints[i] * (1 + angleTolerance);
            float minA = angleConstraints[i] * (1 - angleTolerance);
            float angle = Vector3.Angle(points[i].position - points[i - 7].position, points[i - 8].position - points[i - 7].position);
            if (angle < minA)
            {
                while (angle < minA)
                {
                    points[i].RotateAround(points[i - 7].position, Vector3.forward, 1);
                    angle = Vector3.Angle(points[i].position - points[i - 7].position, points[i - 8].position - points[i - 7].position);
                }
            }
            else if (angle < maxA)
            {
                while (angle > maxA)
                {
                    points[i].RotateAround(points[i - 7].position, Vector3.forward, 1);
                    angle = Vector3.Angle(points[i].position - points[i - 7].position, points[i - 8].position - points[i - 7].position);
                }
            }
            if (velY[0] > 0)
            {
                points[i].RotateAround(points[i - 7].position, Vector3.forward, 180);
            }
            else
            {
                points[i].RotateAround(points[i - 7].position, Vector3.forward, -90);
            }
            
        }
            
    }
    

    void destroyIn()
    {
        if (true)//posX == nextPosX && posY == nextPosY) //TODO: FIX WEIRDNESS
        {
            Destroy(gameObject);
        }
    }

    void draw(bool init) //draws lines between points
    {
        //if (init) Debug.Log("INIT GOAT");
        for (int i = 0; i < 17; i++)
        {
            
            if (init) //init is true if this is the first time we are drawing. in this case we find all the points and positions
            {
                if (i >= 6 && i <= 12) points[i] = body.Find("" + i);
                else points[i] = head.Find("" + i);
                currentPos[i] = points[i].position;
                previousPos[i] = points[i].position;
            }
            
            if (i > 0)
            {
                if (init) lines[i - 1] = points[i - 1].gameObject.AddComponent<LineRenderer>(); //if init then give each point a line renderer
                lines[i - 1].startColor = Color.black;
                lines[i - 1].startWidth = 0.05f;
                lines[i - 1].endColor = Color.black;
                lines[i - 1].endWidth = 0.05f;
                lines[i - 1].numPositions = 2;
                lines[i - 1].SetPosition(0, points[i - 1].position);
                lines[i - 1].SetPosition(1, points[i].position);
                lines[i - 1].material.color = Color.black;
                lines[i - 1].numCapVertices = 1;
            }
        }

        //special cases for last head line and also legs
        if (init) lines[16] = points[16].gameObject.AddComponent<LineRenderer>();
        lines[16].startColor = Color.black;
        lines[16].startWidth = 0.05f;
        lines[16].endColor = Color.black;
        lines[16].endWidth = 0.05f;
        lines[16].numPositions = 2;
        lines[16].SetPosition(0, points[16].position);
        lines[16].SetPosition(1, points[0].position);
        lines[16].material.color = Color.black;
        lines[16].numCapVertices = 1;

        
        
        if (init) //init legs and eye
        {
            points[17] = body.Find("leg1");
            points[18] = body.Find("leg2");
            points[19] = head.Find("eye");
            for (int i = 17; i < 20; i++)
            {
                currentPos[i] = points[i].position;
                previousPos[i] = points[i].position;
            }            
        }

        if (init) lines[17] = points[17].gameObject.AddComponent<LineRenderer>();
        lines[17].startColor = Color.black;
        lines[17].startWidth = 0.05f;
        lines[17].endColor = Color.black;
        lines[17].endWidth = 0.05f;
        lines[17].numPositions = 2;
        lines[17].SetPosition(0, points[17].position);
        lines[17].SetPosition(1, points[10].position);
        lines[17].material.color = Color.black;
        lines[17].numCapVertices = 1;

        if (init) lines[18] = points[18].gameObject.AddComponent<LineRenderer>();
        lines[18].startColor = Color.black;
        lines[18].startWidth = 0.05f;
        lines[18].endColor = Color.black;
        lines[18].endWidth = 0.05f;
        lines[18].numPositions = 2;
        lines[18].SetPosition(0, points[18].position);
        lines[18].SetPosition(1, points[11].position);
        lines[18].material.color = Color.black;
        lines[18].numCapVertices = 1;
    }

    void buildConstraints()
    {
        int h = 0;
        for(int i = 0; i < 20; i++)
        {
            if (i == 0) h = 16;
            else h = i - 1;

            //special cases for legs and eye
            if (i == 17)
            {
                lengthConstraints[i] = (points[i].GetComponent<LineRenderer>().GetPosition(0) - points[10].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
                angleConstraints[i] = 90;
                continue;
            }

            if (i == 18)
            {
                lengthConstraints[i] = (points[i].GetComponent<LineRenderer>().GetPosition(0) - points[11].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
                angleConstraints[i] = 90;
                continue;
            }
            
            if (i == 19)
            {
                lengthConstraints[i] = (points[i].position - points[1].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
                angleConstraints[i] = 90;
                continue;
            }

            //set its length to next neighbour, and angle based on surrounding verlets
            lengthConstraints[i] = (points[i].GetComponent<LineRenderer>().GetPosition(1) - points[i].GetComponent<LineRenderer>().GetPosition(0)).magnitude;
            angleConstraints[i] = Vector3.Angle(points[h].GetComponent<LineRenderer>().GetPosition(0) - points[h].GetComponent<LineRenderer>().GetPosition(1), 
                points[i].GetComponent<LineRenderer>().GetPosition(1) - points[i].GetComponent<LineRenderer>().GetPosition(0));
        }
    }

}
