using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class KeyboardKey : MonoBehaviour
{
    public bool Typeable;
    public string Key;
    public WordleManager WordleManager;
    public List<string> AcceptedKeysStringList;

    private void Awake()
    {
        for (int i = 0; i < WordleManager.AcceptedKeys.Length; i++)
        {
            var keyString = WordleManager.AcceptedKeys[i].ToString();
            AcceptedKeysStringList.Add(keyString);
        }
    }

    public void TypeKey()
    {
        Debug.Log(Key);

        if (AcceptedKeysStringList.Contains(Key))
        {
            Debug.Log("valid key " + Key);
            
            WordleManager.TypeKey(Key);   
        }
        else
        {
            if (Key == "Backspace")
            {
                WordleManager.DeleteLastChar();
            }
            
            if (Key == "Enter")
            {
                WordleManager.CheckAnswer();
            }
                
        }
    }
}
