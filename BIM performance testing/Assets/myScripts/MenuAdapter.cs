using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuAdapter : MonoBehaviour {

    public GameObject content;
    public Button buttonPrefab;
    public Button backButton;
    public Material seeThroughMaterial;

    public Text menuTitle;

    private GameObject selectionBuffer;
    private GameObject selectionBufferSubModel;
    private Button buttonBuffer;
    private string bufferedButtonName;

    public Dictionary<string, List<string>> roomsNamesDict;
    Dictionary<string, Dictionary<string, List<string>>> subModelDict;

    public string surfaceModel = "Surface Model";

    public CameraPositionController cameraController;


    // Set up back button according to current location in menu
    public void SetBackButtonListener(string depth) {

        backButton.GetComponentInChildren<Text>().enabled = true;
        backButton.interactable = true;

        if (depth == "one")
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GenerateMainMenu);
        }
        else if (depth == "surface")
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(GenerateSurfaceLevelMenu);
        }
        else {
            
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(delegate { GenerateSubModelLevelMenu(depth); });
        }

    }

    // Hides back button in Main menu
    public void HideBackButton()
    {
        backButton.GetComponentInChildren<Text>().enabled = false;
        backButton.interactable = false;
    }

    //Get dict of rooms from CameraPositionController
    public void SetRoomNamesDict(Dictionary<string, List<string>> roomsNamesDict) {
        this.roomsNamesDict = roomsNamesDict;
        Debug.Log("MenuAdapter: Printing received dict");
        PrintRoomNamesDict();

        //GenerateFirstLevelMenu();
    }

    //Get dict of sub models from CameraPositionController
    public void SetSubModelDict(Dictionary<string, Dictionary<string, List<string>>> subModelDict) {
        this.subModelDict = subModelDict;
        Debug.Log("MenuAdapter: Sub Model dict received -> making menu now");
        GenerateMainMenu();
        //do smthin
    }

    public void GenerateMainMenu() {
        ClearMenu();
        HideBackButton();
        // TODO: Adjust backbutton func
        // Maybe put a full model view button here

        // Generate surface model as first button
        Button btn = Instantiate(buttonPrefab);
        btn.onClick.AddListener(delegate { GenerateSurfaceLevelMenu(); });
        btn.GetComponentInChildren<Text>().text = surfaceModel;
        btn.transform.SetParent(content.transform, false);

        List<string> keyList = new List<string>(subModelDict.Keys);
        foreach (string subModel in keyList)
        {
            btn = Instantiate(buttonPrefab);
            btn.onClick.AddListener(delegate { GenerateSubModelLevelMenu(subModel); });
            btn.GetComponentInChildren<Text>().text = subModel;
            btn.transform.SetParent(content.transform, false);
        }

        menuTitle.text = "Main Menu";
    }

    public void GenerateSurfaceLevelMenu() {
        ClearMenu();

        SetBackButtonListener("one");

        // turn off highlighting
        if (selectionBuffer != null) {
            ClearMaterial(selectionBuffer.name);
        }
        

        // Setting first button as full model view
        Button btn = Instantiate(buttonPrefab);
        btn.onClick.AddListener(delegate { cameraController.SetFullModelView(); });
        btn.GetComponentInChildren<Text>().text = "Full model view";
        btn.transform.SetParent(content.transform, false);

        menuTitle.text = "Levels";

        List<string> keyList = new List<string>(roomsNamesDict.Keys);
        foreach (string level in keyList) {
            btn = Instantiate(buttonPrefab);
            btn.onClick.AddListener(delegate { GenerateSurfaceRoomMenu(level); });
            btn.GetComponentInChildren<Text>().text = level;
            btn.transform.SetParent(content.transform, false);
        }
    }

    public void GenerateSurfaceRoomMenu(string parentLevel) {
        //TODO: Add movement of camera and highlight of entire level
        ClearMenu();
        SetBackButtonListener("surface");

        List<string> valueList = roomsNamesDict[parentLevel];
        foreach (string room in valueList) {
            Button btn = Instantiate(buttonPrefab);
            btn.onClick.AddListener(delegate { SurfaceLeafButtonActions(room, btn); });
            btn.GetComponentInChildren<Text>().text = room;
            btn.transform.SetParent(content.transform, false);
        }

        menuTitle.text = "Rooms";
    }

    public void GenerateSubModelLevelMenu(string subModelName) {
        ClearMenu();
        // turn off highlighting
        ClearMaterialInChildren();
        SetBackButtonListener("one");

        menuTitle.text = subModelName;

        List<string> keyList = new List<string>(subModelDict[subModelName].Keys);

        foreach (string element in keyList)
        {
            Button btn = Instantiate(buttonPrefab);
            btn.onClick.AddListener(delegate { GenerateSubModelElementMenu(element, subModelName); });
            btn.GetComponentInChildren<Text>().text = element;
            btn.transform.SetParent(content.transform, false);
        }
    }

    public void GenerateSubModelElementMenu(string subModelLevel, string subModelName) {
        ClearMenu();
        SetBackButtonListener(subModelName);

        menuTitle.text = subModelLevel;

        List<string> keyList = subModelDict[subModelName][subModelLevel];

        foreach (string element in keyList)
        {
            Button btn = Instantiate(buttonPrefab);
            btn.onClick.AddListener(delegate { SubModelLeafButtonActions(element, subModelLevel, subModelName, btn); });
            btn.GetComponentInChildren<Text>().text = element;
            btn.transform.SetParent(content.transform, false);
        }
    }

    public void ClearMenu() {
        foreach (Transform child in content.transform) {
            GameObject.Destroy(child.gameObject);
        }
    }

    // Surface button actions

    public void SurfaceLeafButtonActions(string room, Button btn) {
        if (buttonBuffer != null) {
            buttonBuffer.GetComponentInChildren<Text>().text = bufferedButtonName;
            ResetSurfaceButton(buttonBuffer, bufferedButtonName);
        }
        buttonBuffer = btn;
        ApplySeeThroughMaterial(room);
        string roomName = btn.GetComponentInChildren<Text>().text;
        btn.GetComponentInChildren<Text>().text = "Enter";
        bufferedButtonName = roomName;
        // Orange
        btn.GetComponentInChildren<Image>().color = new Color32(255, 153, 51, 255);
        btn.onClick.AddListener(delegate { MoveToRoom(roomName); });
    }

    public void ResetSurfaceButton(Button bufferBtn, string name)
    {
        bufferBtn.GetComponentInChildren<Text>().text = name;
        // Blue
        bufferBtn.GetComponentInChildren<Image>().color = new Color32(134, 175, 204, 255);
        bufferBtn.onClick.RemoveAllListeners();
        bufferBtn.onClick.AddListener(delegate { SurfaceLeafButtonActions(name, bufferBtn); });
    }

    public void MoveToRoom(string roomName)
    {
        ClearMaterial(roomName);
        cameraController.MoveCameraToRoom(roomName);
    }

    public void ApplySeeThroughMaterial(string room)
    {
        // Clear previous selection
        if (selectionBuffer != null)
        {
            ClearMaterial(selectionBuffer.name);
        }
        GameObject space = GameObject.Find(room);
        selectionBuffer = space;
        space.GetComponent<Renderer>().enabled = true;
        //space.GetComponent<Renderer>().material = seeThroughMaterial;
    }

    public void ClearMaterial(string room)
    {
        GameObject space = GameObject.Find(room);
        space.GetComponent<Renderer>().enabled = false;
        //Material m = space.GetComponent<Renderer>().material;
        //Destroy(m);
    }

    // Sub model button actions

    public void SubModelLeafButtonActions(string element, string level, string name, Button btn)
    {
        
        if (buttonBuffer != null)
        {
            ResetSubModelButton(buttonBuffer);
        }
        buttonBuffer = btn;
        
        ApplySeeThroughMaterialToChildren(element, level, name);
        
        // Orange
        btn.GetComponentInChildren<Image>().color = new Color32(255, 153, 51, 255);
    }

    public void ResetSubModelButton(Button bufferBtn) {
        bufferBtn.GetComponentInChildren<Image>().color = new Color32(134, 175, 204, 255);
    }

    public void ApplySeeThroughMaterialToChildren(string element, string level, string name)
    {
        Debug.Log(element + " " + level + " " + name);
        GameObject subModel = GameObject.Find(name);
        Transform targetLevel = RecursiveFind(subModel, level);
        GameObject target = null;
        Debug.Log(targetLevel);
        Debug.Log(targetLevel.GetType());

        foreach (Transform group in targetLevel) {

            if (group.gameObject.name.Contains(element)) {
                target = group.gameObject;
            }
        }

        // Clear previous selection
        if (selectionBufferSubModel != null)
        {
            ClearMaterialInChildren();
        }

        selectionBufferSubModel = target;

        foreach (Transform child in target.transform) {
            child.gameObject.GetComponent<Renderer>().material = seeThroughMaterial;
            child.gameObject.GetComponent<Renderer>().enabled = true;
        }
    }

    public Transform RecursiveFind(GameObject obj, string level) {
        Debug.Log(obj.name);
        Transform targetLevel = obj.transform.Find(level);
        if (targetLevel == null) {
            return RecursiveFind(obj.transform.GetChild(0).gameObject, level);
        }
        Debug.Log(targetLevel.gameObject.name);
        return targetLevel;
    }

    public void ClearMaterialInChildren()
    {
        if (selectionBufferSubModel != null) { 
            foreach (Transform child in selectionBufferSubModel.transform)
            {
                child.gameObject.GetComponent<Renderer>().enabled = false;
            }
        }
        //Material m = space.GetComponent<Renderer>().material;
        //Destroy(m);
    }

    public void PrintRoomNamesDict()
    {
        foreach (KeyValuePair<string, List<string>> kvp in roomsNamesDict)
        {
            Debug.Log(kvp.Key + " : " + kvp.Value.ToString());
            foreach (string name in kvp.Value)
            {
                Debug.Log(name);
            }
        }
    }

}
