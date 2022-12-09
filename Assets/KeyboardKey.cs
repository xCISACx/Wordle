using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardKey : MonoBehaviour
{
    public InputManager InputManager;
    
    public enum tileState
    {
        None,
        Correct,
        Incorrect,
        WrongPlace
    };

    public tileState state;
    
    public Image Image;
    public TMP_Text Text;
    
    private Color _defaultColour; 
    private Color _correctColour;
    private Color _wrongPlaceColour;
    private Color _incorrectColour;
    
    public bool Typeable;
    public string Key;
    public WordleManager WordleManager;
    public List<string> AcceptedKeysStringList;


    private void Awake()
    {
        Image = GetComponentInChildren<Image>();
        Text = GetComponentInChildren<TMP_Text>();

        WordleManager = FindObjectOfType<WordleManager>();
        InputManager = FindObjectOfType<InputManager>();
        
        _defaultColour = WordleManager.DefaultColour; 
        _correctColour = WordleManager.CorrectColour;
        _wrongPlaceColour = WordleManager.WrongPlaceColour;
        _incorrectColour = WordleManager.IncorrectColour;
        
        for (int i = 0; i < InputManager.AcceptedKeys.Length; i++)
        {
            var keyString = InputManager.AcceptedKeys[i].ToString();
            AcceptedKeysStringList.Add(keyString);
        }
    }
    
    private void Update()
    {
        switch (state)
        {
            case tileState.Correct:
                
                Image.color = _correctColour;
                Text.color = Color.white;
                break;
            
            case tileState.Incorrect:
                
                Image.color = _incorrectColour;
                Text.color = Color.white;
                break;
            
            case tileState.WrongPlace:
                
                Image.color = _wrongPlaceColour;
                Text.color = Color.white;
                break;
        }
    }

    public void TypeKey()
    {
        //Debug.Log(Key);

        if (AcceptedKeysStringList.Contains(Key))
        {
            //Debug.Log("valid key " + Key);
            
            InputManager.TypeKey(Key);   
        }
        else
        {
            if (Key == "Backspace")
            {
                InputManager.DeleteLastChar();
            }
            
            if (Key == "Enter")
            {
                WordleManager.CheckAnswer();
            }
                
        }
    }
}
