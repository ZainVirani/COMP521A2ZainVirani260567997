using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cannonBallBehaviour : MonoBehaviour {

    public float gravity = -0.4f;
    public float airResistanceX = 0.1f;
    public float airResistanceY = 0.1f;
    public float velX;
    public float velY;
    public float initialVelocity = 15f;
    public float restitution = 0.5f;
    int angle;
    float posX = 2.496f;
    float posY = 2.075f;
    float nextPosX;
    float nextPosY;
    public Vector2 nextPos;
    float windSpeed;
    windBehaviour wind;
    float lerp = 0.005f;
    bool updateOff = false;
    float bounceCoeff = 1.2f;
    bool windOff = false;

    // Use this for initialization
    void Start () {
        transform.position = new Vector3(posX, posY, 0);
        nextPosX = posX;
        nextPosY = posY;
        wind = GameObject.FindGameObjectWithTag("cannons").GetComponent<windBehaviour>();
        windSpeed = 0;
        angle = GameObject.FindGameObjectWithTag("cannons").GetComponent<cannonBehaviour>().getCannonAngle(); //find firing angle

        //set appropriate initial velocity
        velX = initialVelocity * Mathf.Cos(Mathf.Deg2Rad * angle);
        velY = initialVelocity * Mathf.Sin(Mathf.Deg2Rad * angle);
    }
	
	// Update is called once per frame
	void Update() {

        if (transform.position.x >= 14 && transform.position.x <= 16 && transform.position.y <= 6.384f) //handles mountain penetration by bouncing upwards
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            bounce(new Vector3(0,0,0), new Vector3(1,0,0), 23);
        }

        nextPos = new Vector2(nextPosX, nextPosY);
        if (!updateOff)
        {
            move();
        }
        posX = nextPosX;
        posY = nextPosY;

        //Destroy if not moving or out of bounds
        if (posX == nextPosX && posY == nextPosY) {
            InvokeRepeating("destroyIn", 3, 1);
        }
        if (transform.position.x <= -0.25 || transform.position.x >= 30.25 || transform.position.y <= -0.25)
        {
            Destroy(gameObject);
        }
        if (transform.position.y <= 1.385 && (transform.position.x <= 10.2f || transform.position.x >= 19.8f))
        {
            Destroy(gameObject);
        }

    }

    void destroyIn()
    {
        if (true)//posX == nextPosX && posY == nextPosY) //TODO: FIX WEIRDNESS
        {
            Destroy(gameObject);
        }
    }
    
    public void bounce(Vector3 A, Vector3 B, int i)
    {
        windOff = true;
        updateOff = true;
        posX = nextPosX;
        posY = nextPosY;
        Vector3 AB = (B - A).normalized; //find the line
        
        bool top = false;
        //figure out where we hit and adjust the x velocity considering bounce and restitution accordingly
        if (i >= 22 && i <= 33 && velX > 0)
        {
            Debug.Log("HIT TOP");
            top = true;
            velX += -AB.y * restitution + velX * 4 * bounceCoeff;
        }
        else if (i >= 0 && i <= 21 && velX > 0)
        {
            Debug.Log("HIT LEFT");
            velX = -AB.y * restitution;
            
        }
        else
        {
            Debug.Log("HIT RIGHT");
            velX = -AB.y * restitution;
        }

        velY = AB.x * restitution; //apply y velocity restitution

        //apply a bounce in the direction of the normal
        nextPosX = transform.position.x - (AB.y * bounceCoeff)/2; // * velX * restitution;
        nextPosY = transform.position.y + (AB.x * bounceCoeff)/2;// * velY * restitution;
        moveUnrestricted();
        nextPosX = transform.position.x - (AB.y * bounceCoeff) / 2; // * velX * restitution;
        nextPosY = transform.position.y + (AB.x * bounceCoeff) / 2;// * velY * restitution;
        moveUnrestricted();
        if (bounceCoeff * restitution >= 0.00385f && !top) bounceCoeff *= restitution;
        else bounceCoeff *= restitution;
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        updateOff = false;

    }

    void moveUnrestricted() //used when applying a bounce (all forces deactivated for one frame)
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(nextPosX, nextPosY, 0), 1);
    }

    void move()
    {
        posX = nextPosX;
        posY = nextPosY;

        if (!windOff) windSpeed = wind.getWind(); //find wind
        else windSpeed = 0;

        //Apply air resistance to velocity
        if (velX < 0)
        {
            velX = velX + (velX * airResistanceX);
        }
        else {
            velX = velX - (velX * airResistanceX);
        }
        if (velY < 0)
        {
            velY = velY + (velY * airResistanceY);
        }
        else {
            velY = velY - (velY * airResistanceY);
        }

        //Apply gravity to velocity with v = v' + at where a=gravity 
        velY = velY + gravity;

        //adjust next positions
        nextPosX = nextPosX + velX + windSpeed;
        nextPosY = nextPosY + velY;

        //transform in direction of next positions (lerp smooths movement quite nicely)
        transform.position = Vector3.Lerp(transform.position, new Vector3(nextPosX, nextPosY, 0), lerp);
    }
}
