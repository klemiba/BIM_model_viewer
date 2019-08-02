using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempScript : MonoBehaviour
{

    private float worldX;
    private float worldY;
    private float worldZ;

    // Start is called before the first frame update
    void Start()
    {
        worldX = transform.position.x;
        worldY = transform.position.y;
        worldZ = transform.position.z;
        Debug.Log(worldX + " " + worldY + " " + worldZ);
        Renderer rend = transform.GetComponent<Renderer>();
        Debug.Log(rend.bounds.center);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public float getX() {
        return worldX;
    }
    public float getY() {
        return worldY;
    }
    public float getZ() {
        return worldZ;
    }
}
