using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveMeThere : MonoBehaviour
{
    private TempScript tmp;
    public GameObject test;
    // Start is called before the first frame update
    void Start(){
        
    }

    // Update is called once per frame
    void Update()
    {
        tmp = test.GetComponent<TempScript>();
        transform.position = new Vector3(tmp.getX(), tmp.getY(), tmp.getZ());
    }
}
