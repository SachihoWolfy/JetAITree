using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideOnKeypress : MonoBehaviour
{
    public bool beingCalled;
    public bool isVisible;
    public List<GameObject> gameObjects;

    public KeyCode key;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            ToggleVisibility();
        }
    }
    void ToggleVisibility()
    {
        foreach (GameObject obj in gameObjects)
        {
            obj.SetActive(gameObjects[0].activeSelf);
        }
        foreach (GameObject obj in gameObjects)
        {
            obj.SetActive(!obj.activeSelf);
        }
    }
}
