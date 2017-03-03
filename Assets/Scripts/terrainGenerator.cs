using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainGenerator : MonoBehaviour {

    [Range(0.1f, 5f)]
    public float heightScale = 3.0f; //a higher heightScale results in less height variance
    [Range(0.1f, 5f)]
    public float detailScale = 3.0f; //a higher detailScale results in less detail variance
    public GameObject empty;
    private GameObject temp;
    private Mesh slope; //used to update the mesh renderer
    private Vector3[] vertices; //list of 121 vertices on the plane
    

    void Start()
    {
        //acquire appropriate meshes
        slope = GetComponent<MeshFilter>().mesh;
        vertices = slope.vertices;
        if (tag != "top") GenerateTerrain(); //keep the top flat
        else adjustVertices(); //adjust the vertex positions appropriately for the top (for the other planes this is handled in GenerateTerrain())
    }

    void adjustVertices()
    {
        /*
            the reason i need to do this is because when the planes are rotated, its vertices aren't taken with it (i.e. it remains as a representation of a flat surface in a 3D sense)
            I run midpoint bisection and adjust the location of these after (the meshes update properly)
            I end up using these vertices for collision detection later, so that's why its necessary
        */
        //create empty at 000, move relevant vertices inside, rotate and transform to this.rotate/transform
        empty = Instantiate(Resources.Load("Empty") as GameObject); //used to maintain global position coordinates
        temp = Instantiate(Resources.Load("Empty") as GameObject); //used for rotation
        temp.transform.position = new Vector3(0, 0, 0);
        for (int i = 0; i < 11; i++)
        {
            Vector3 toAssign = slope.vertices[i];
            GameObject newD = Instantiate(Resources.Load("Dot") as GameObject); //visual component for testing
            for (int j = i + 11; j < vertices.Length; j += 11)
            {
                if (slope.vertices[j].z > toAssign.z)
                {
                    toAssign = slope.vertices[j];
                }
            }
            newD.transform.parent = temp.transform;
            newD.name = i.ToString();
            newD.transform.position = toAssign;
            if (tag == "top")
            {
                newD.transform.localScale = new Vector3(.05f, .025f, 0.1f);
            }
        }
        temp.transform.rotation = transform.rotation;
        temp.transform.position = transform.position;
        temp.transform.localScale = transform.localScale;
        empty.transform.position = new Vector3(0, 0, 0);
        
        for (int i = 0; i < 11; i++) //transfer adjusted vertices from temp to empty
        {
            //Debug.Log("accessing child " + i);
            temp.transform.FindChild(i.ToString()).parent = empty.transform;
        }
        Destroy(temp);
        
    }
    
    void GenerateTerrain()
    {
        MidpointBisection(vertices, 0, 10); //run the recursive algorithm
        slope.vertices = vertices;
        slope.RecalculateBounds();
        slope.RecalculateNormals(); //adjust the plane mesh
        adjustVertices(); //adjust vertex locations

        //code for collider, just used for testing vertex locations because it has a nice visual component, not actually needed
        /*Destroy(gameObject.GetComponent<MeshCollider>());
        MeshCollider coll = gameObject.AddComponent<MeshCollider>();
        coll.sharedMesh = null;
        coll.sharedMesh = slope;*/

        
        
    }
    
    void MidpointBisection(Vector3[] relevantVertices, int start, int stop)
    {
        int midpoint = (stop + start) / 2;
        if (midpoint != start && midpoint != stop)
        {
            //Displace the vector at midpoint
            float maxOffset = Mathf.Abs(relevantVertices[stop].x - relevantVertices[start].x) / heightScale;
            float offset = (float)Random.Range(-maxOffset-1, maxOffset+1);
            float length = Mathf.Abs((relevantVertices[midpoint].x - relevantVertices[0].x) / detailScale);
            //Debug.Log("Offset " + (offset* length));
            relevantVertices[midpoint].z += offset * length; //if we move it down, adjust the vertices below it down as well
            if(offset < 0)
            {
                for(int i = midpoint + 11; i<vertices.Length; i += 11)
                {
                    relevantVertices[i].z += offset * length;
                }
            }
            //Re-balancing and recursive call
            if (start < midpoint)
            {
                for (int i = midpoint - 1; i >= start; i--) //adjust the vertices around it to more realistically depict the change in the midpoint
                {
                    relevantVertices[i].z += offset * length * (i - start) / (midpoint - start);
                    if (offset < 0)
                    {
                        for (int j = i; j < vertices.Length; j += 11)
                        {
                            relevantVertices[j].z += offset * length * (i - start) / (midpoint - start);
                        }
                    }
                }
                MidpointBisection(relevantVertices, start, midpoint); //recursive call
            }
            if (midpoint < stop)
            {
                for (int i = midpoint + 1; i <= stop; i++) //adjust the vertices around it to more realistically depict the change in the midpoint
                {
                    relevantVertices[i].z += offset * length * (stop - i) / (stop - midpoint);
                    if (offset < 0)
                    {
                        for (int j = i; j < vertices.Length; j += 11)
                        {
                            relevantVertices[j].z += offset * length * (stop - i) / (stop - midpoint);
                        }
                    }
                }
                MidpointBisection(relevantVertices, midpoint, stop); //recursive call
            }
        }
    }

}
