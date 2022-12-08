using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Char : MonoBehaviour
{
    public Vector3 DefaultPosition;
    public WordleManager WordleManager;
    public enum TileState
    {
        None,
        Correct,
        Incorrect,
        WrongPlace
    };

    public TileState State;

    public Image Image;
    public TMP_Text Text;

    private Color _defaultColour; 
    private Color _correctColour;
    private Color _wrongPlaceColour;
    private Color _incorrectColour;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Awake()
    {
        Image = GetComponentInChildren<Image>();
        Text = GetComponentInChildren<TMP_Text>();
        WordleManager = FindObjectOfType<WordleManager>();

        DefaultPosition = transform.GetComponent<RectTransform>().anchoredPosition;

        _defaultColour = WordleManager.DefaultColour; 
        _correctColour = WordleManager.CorrectColour;
        _wrongPlaceColour = WordleManager.WrongPlaceColour;
        _incorrectColour = WordleManager.IncorrectColour;
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case TileState.Correct:

                //GetComponent<Animator>().enabled = false;
                Image.color = _correctColour;
                Text.color = Color.white;
                break;
            
            case TileState.Incorrect:
                
                //GetComponent<Animator>().enabled = false;
                Image.color = _incorrectColour;
                Text.color = Color.white;
                break;
            
            case TileState.WrongPlace:
                
                //GetComponent<Animator>().enabled = false;
                Image.color = _wrongPlaceColour;
                Text.color = Color.white;
                break;
        }
    }
}
