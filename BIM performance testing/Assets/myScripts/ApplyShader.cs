using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyShader : MonoBehaviour
{
	public Transform parentObject;
	public Material mat;
    // Start is called before the first frame update
    void Start()
    {
        setMaterial(parentObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	
	void setMaterial(Transform pObject){
		foreach(Transform child in pObject){
			if(child.GetComponent<Renderer>() != null){
				child.GetComponent<Renderer>().material = mat;
			}
			setMaterial(child);
		}
	}
}
