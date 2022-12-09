using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public GameObject Keyboard;

    [SerializeField] public string[] KeyboardKeys =
    {
        "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X", "C",
        "V", "B", "N", "M"
    };

    private void Awake()
    {
        //SetKeyBoardKeys();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // This method attributes a letter to each keyboard key so that letter can be typed when it's clicked on
    
    [ContextMenu("Set Keyboard Keys")]
    public void SetKeyBoardKeys()
    {
        var keyboardKeys = Keyboard.GetComponentsInChildren<KeyboardKey>();
        var typeableKeyboardKeys = Keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Typeable).ToList();

        for (int i = 0; i < typeableKeyboardKeys.Count; i++)
        {
            var key = typeableKeyboardKeys[i];
		    
            key.Key = KeyboardKeys[i];
			    
            var textComponent = key.GetComponentInChildren<TMP_Text>();
        
            if (textComponent)
            {
                textComponent.text = key.Key;
            }
        }

        var backSpaceKey = keyboardKeys.Where(x => x.Key == "Backspace").ToList();
        backSpaceKey[0].GetComponentInChildren<TMP_Text>().text = "<-";
	    
        var enterKey = keyboardKeys.Where(x => x.Key == "Enter").ToList();
        enterKey[0].GetComponentInChildren<TMP_Text>().text = "Go";
    }
}
