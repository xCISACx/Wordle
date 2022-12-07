using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Char : MonoBehaviour
{
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
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        Image = GetComponentInChildren<Image>();
        Text = GetComponentInChildren<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case tileState.Correct:

                GetComponent<Animator>().enabled = false;
                Image.color = Color.green;
                Text.color = Color.black;
                break;
            case tileState.Incorrect:
                
                GetComponent<Animator>().enabled = false;
                Image.color = Color.gray;
                Text.color = Color.white;
                break;
            
            case tileState.WrongPlace:
                
                GetComponent<Animator>().enabled = false;
                Image.color = Color.yellow;
                Text.color = Color.black;
                break;
        }
    }
}
