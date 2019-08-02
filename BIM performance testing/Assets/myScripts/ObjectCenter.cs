using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCenter : MonoBehaviour
{
    private Renderer rend;
    private Vector3 center;

    void Start() {
        rend = GetComponent<Renderer>();
        center = rend.bounds.center;
    }

    public Vector3 getObjectCenter() {
        return center;
    }
}

/* Draws a wireframe sphere in the Scene view, fully enclosing
// the object.
void OnDrawGizmosSelected(){
    Debug.Log(center);
    Draws a sphere around the location
    float radius = rend.bounds.extents.magnitude;
    Gizmos.color = Color.white;
    Gizmos.DrawWireSphere(center, radius);
}*/

