using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPositionController : MonoBehaviour
{


    // CAMERA
    //x = 0, y = 0, z = 0, x = 45°
    public GameObject camera;
    // Tilt at the begining (editor)
    public bool tiltSwitch;
    // Movement speed of camera
    public float smooth = 5.0f;

    // BIM MODEL
    /* Starting model position:
     * Position: x = -0.5, y = -1.4, z = 0.85
     * Size: x,y,z = 0.075
     */
    public GameObject bimModel;
    public GameObject surfaceModel;

    public GameObject[] subModelsArray;

    public Vector3 fullViewObjectPosition;
    public Vector3 fullViewObjectSize;
    //walthrough object position will always be 0,0,0
    public Vector3 walkthroughObjectSize;
    private bool initRoomPos = true;

    public MenuAdapter mAdapter;

    //List of spaces and list of room position
    public List<GameObject> spaces;
    private Dictionary<string, Vector3> roomPositionsDict;
    private Dictionary<string, List<string>> spacesByDepth;
    private Dictionary<string, Dictionary<string, List<string>>> subModelDict;

    public Material seeThroughMaterial;

    public Vector3 bimObjectCenter;

    // Start is called before the first frame update
    void Start() {

        // Creating, filling and forwarding dict of all spaces/rooms in surface model for camera manipulation
        spacesByDepth = new Dictionary<string, List<string>>();
        GetAllSpaces(surfaceModel);
        mAdapter.SetRoomNamesDict(spacesByDepth);

        // Creating, filling and forwarding dict of all sub models for highlighting on demand
        subModelDict = new Dictionary<string, Dictionary<string, List<string>>>();
        // Loop through array of all sub models and create dict
        foreach (GameObject sModel in subModelsArray)
        {
            subModelDict.Add(sModel.name, ParseSubModels(sModel));
        }
        mAdapter.SetSubModelDict(subModelDict);

        // Init values and game objects
        // 1. Set position of the surface model 
        SaveFullViewBimSize();
        // 3. Set size of surface model for (full size) walkthrough

        SaveFullViewBimPos();
        // 2. Set size of surface model for full view
        
        SaveWalkthroughBimSize();

        // Tilt camera to see the model on start (irrelevant for VR and AR)
        if (tiltSwitch) {
            StartCoroutine(SetInitCameraPos());
        }

        

        // Get coordinates of center of every room
        GetPositionOfAllSpaces();

        // Set full model view for begining of app
        SetFullModelView();


        //PrintRoomPositionDict();
        //PrintRoomNamesDict();
        //MoveCameraToRoom("A103");
    }
    
    /*
    public void Update() {
        float camDistance = Vector3.Distance(camera.transform.position, bimModel.transform.position);
        Debug.Log(camDistance);
    }
    */
    
    // Init functions
    public void SaveFullViewBimPos() {
        float posX = bimObjectCenter.x * modelDistanceWeight * modelCameraDistance * radius;

        //Storing initial position of bimModel
        fullViewObjectPosition = new Vector3(-posX, -1.0f, 1.0f);
    }

    public float modelDistanceWeight = 0.00177335f;
    public float modelCameraDistance = 1.41f;
    public float radius;

    public void SaveFullViewBimSize() {

        // First find a center for your bounds.
        bimObjectCenter = Vector3.zero;

        foreach (GameObject child in spaces)
        {
            bimObjectCenter += child.GetComponent<Renderer>().bounds.center;
        }
        bimObjectCenter /= spaces.Count; //center is average center of children

        //Now you have a center, calculate the bounds by creating a zero sized 'Bounds', 
        Bounds bounds = new Bounds(bimObjectCenter, Vector3.zero);

        foreach (GameObject child in spaces)
        {
            bounds.Encapsulate(child.GetComponent<Renderer>().bounds);
        }

        radius = bounds.extents.magnitude;

        float camDistance = Vector3.Distance(camera.transform.position, bimModel.transform.position);

        Debug.Log(radius);
        Debug.Log(camDistance);
        float fullViewSize = radius * modelDistanceWeight * modelCameraDistance;
        fullViewObjectSize = new Vector3(fullViewSize, fullViewSize, fullViewSize);
    }
    public void SaveWalkthroughBimSize() {
        walkthroughObjectSize = new Vector3(1.0f, 1.0f, 1.0f);
    }
    public void GetAllSpaces(GameObject parent) {

        foreach (Transform child in parent.transform) {
            if (child.gameObject.name.Contains("IfcSpace")) {
                // List of all rooms on current level
                List<string> roomsOnThisFloor = new List<string>();
                foreach (Transform space in child)
                {
                    spaces.Add(space.gameObject);
                    Renderer rend = space.gameObject.GetComponent<Renderer>();
                    rend.GetComponent<Renderer>().material = seeThroughMaterial;
                    rend.enabled = false;
                    roomsOnThisFloor.Add(space.gameObject.name);
                }
                // Add list of all rooms on currenlty level under level name key
                spacesByDepth.Add(parent.gameObject.name, roomsOnThisFloor);
            }
            GetAllSpaces(child.gameObject);
        }
    }

    public Dictionary<string, List<string>> ParseSubModels(GameObject parent) {

        foreach (Transform child in parent.transform)
        {
            // Catch parent object of all levels
            if (parent.transform.childCount > 1)
            {

                Dictionary<string, List<string>> levels = new Dictionary<string, List<string>>();

                // Loop through each level
                foreach (Transform level in parent.transform)
                {
                    // List of all sub elements of level like all pipes, all vents etc.
                    List<string> subElements = new List<string>();

                    foreach (Transform subElement in level)
                    {

                        if (!subElement.gameObject.name.Contains("IfcSpace"))
                        {
                            foreach(Transform subElementChild in subElement)
                            {
                                Renderer rend = subElementChild.gameObject.GetComponent<Renderer>();
                                rend.GetComponent<Renderer>().material = seeThroughMaterial;
                                rend.enabled = false;
                            }

                            string elementName = subElement.gameObject.name;

                            int start = elementName.IndexOf("Ifc", 0) + 3;

                            elementName = elementName.Substring(start);
                            elementName = elementName.Substring(0, elementName.Length - 4);

                            subElements.Add(elementName);
                        }
                        else
                        {
                            foreach (Transform subElementChild in subElement)
                            {
                                Renderer rend = subElementChild.gameObject.GetComponent<Renderer>();
                                rend.enabled = false;
                            }
                        }
                    }

                    levels.Add(level.gameObject.name, subElements);

                    // Print 
                    Debug.Log("Key: " + level.gameObject.name + "Value: " + subElements);
                    foreach (string name in subElements)
                    {
                        Debug.Log(name);
                    }
                }
                return levels;
            }
            return ParseSubModels(child.gameObject);
        }
        Debug.Log("Incompatible model hierarchy.");
        return null;
    }

    // Coroutine for setting initial camera poz (for Editor)
    IEnumerator SetInitCameraPos()
    {
        yield return new WaitForSeconds(0.05f);
        Debug.Log("Tilting");
        //Set position
        camera.transform.position = new Vector3(0, 0, 0);
        //Set rotation
        Quaternion target = Quaternion.Euler(90.0f, 0, 0);
        camera.transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * smooth);
    }

    // Full view
    public void SetFullModelView() {
        MoveCameraForFullView();
        ResizeAndMoveModelForFullView();
    }
    public void MoveCameraForFullView() {
        camera.transform.position = new Vector3(0, 0, 0);
    }
    public void ResizeAndMoveModelForFullView() {
        bimModel.transform.localScale = fullViewObjectSize;
        bimModel.transform.position = fullViewObjectPosition;
    }

    // Walkthrough
    public void GetPositionOfAllSpaces() {
        /* Resize and move model before getting positions of spaces
         * to save correct room position
         */
        ResizeAndMoveModelForWalkthrough();
        roomPositionsDict = new Dictionary<string, Vector3>();
        foreach (GameObject space in spaces) {
            Renderer rend = space.GetComponent<Renderer>();
            Vector3 center = rend.bounds.center;
            roomPositionsDict.Add(space.name, center);
        }
    }
    public void ResizeAndMoveModelForWalkthrough() {
        bimModel.transform.localScale = walkthroughObjectSize;
        bimModel.transform.position = new Vector3(0f, 0f, 0f);
    }
    public void MoveCameraToRoom(string room) {
        // Get positions of rooms only the first time
        if (initRoomPos) {
            GetPositionOfAllSpaces();
            initRoomPos = false;
        }
        else {
            ResizeAndMoveModelForWalkthrough();
        }
        // Get position of passed room from dict
        Vector3 selectedRoomPoz = roomPositionsDict[room];
        // Move bimModel so that the camera is in the selected room
        Vector3 modelTranslationVector = camera.transform.position - selectedRoomPoz;
        bimModel.transform.position += modelTranslationVector;
    }

    // For menu
    public Dictionary<string, List<string>> GetRoomNamesDict() {
        return spacesByDepth;
    }

    // For Debuging
    public void PrintRoomPositionDict() {
        foreach (KeyValuePair<string, Vector3> kvp in roomPositionsDict) {
            Debug.Log(kvp.Key + " : " + kvp.Value);
        }
    }
    public void PrintRoomNamesDict() {
        foreach (KeyValuePair<string, List<string>> kvp in spacesByDepth)
        {
            Debug.Log(kvp.Key + " : " + kvp.Value.ToString());
            foreach (string name in kvp.Value) {
                Debug.Log(name);
            }
        }
    }
}
