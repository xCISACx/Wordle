using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public WordleManager WordleManager;
    public GameObject Keyboard;

    public string InputString
    {
        get { return _inputString; }
        set => _inputString = value;
    }
    [SerializeField] private string _inputString = "";

    [Header("Input Variables")]

    public KeyCode[] AcceptedKeys =
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
        KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
        KeyCode.U, KeyCode.V, KeyCode.W,
        KeyCode.X, KeyCode.Y, KeyCode.Z
    };

    [SerializeField] public string[] KeyboardKeys =
    {
        "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X", "C",
        "V", "B", "N", "M"
    };

    private void Awake()
    {
        //SetKeyBoardKeys();
        WordleManager = GetComponent<WordleManager>();
        _inputString = "";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (WordleManager.AnswerLength > -1 && WordleManager.AnswerLength <= WordleManager.RequiredAnswerLength - 1)
        {
            var key = ReadKeyInput();

            // System.Char.IsLetter(key.ToString(), 0) does not work since it prints "Mouse" when the mouse is clicked
		    
            if (Input.anyKeyDown && AcceptedKeys.Contains(key))
            {
                TypeKey(key.ToString());
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            DeleteLastChar();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (WordleManager.Solution != "")
            {
                WordleManager.CheckAnswer();   
            }
            else
            {
                //Debug.Log("no solution");
            }
        }
    }

    private KeyCode ReadKeyInput()
    {
        foreach (KeyCode keyValue in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyValue))
            {
                if (keyValue != KeyCode.Return)
                {
                    return keyValue;
                }
            }
        }

        return KeyCode.KeypadPeriod;
    }
    
    public void DeleteLastChar()
    {
        if (WordleManager.Answer.Length >= 1)
        {
            WordleManager.Answer = WordleManager.Answer.Substring(0, WordleManager.Answer.Length - 1);
            WordleManager.CurrentChar--;
            WordleManager.AnswerLength--;
            WordleManager.CurrentChar = Mathf.Clamp(WordleManager.CurrentChar, 0, 4);

            var chars = WordleManager.CurrentUIRow.GetComponentsInChildren<Char>();
            
            WordleManager.CurrentUIColumn = chars[WordleManager.CurrentChar];
		    
            WordleManager.CurrentUIColumn.GetComponentInChildren<TMP_Text>().text = "";
		    
            WordleManager.SetTileColours(chars, WordleManager.DefaultColour);
        }
    }

    public void TypeKey(string key)
    {
        var chars = WordleManager.CurrentUIRow.GetComponentsInChildren<Char>();
	    
        if (WordleManager.Answer.Length <= WordleManager.RequiredAnswerLength - 1)
        {
            var s = key;
            _inputString = s;
			
            WordleManager.CurrentUIColumn = chars[WordleManager.CurrentChar];

            WordleManager.CurrentUIColumn.GetComponentInChildren<TMP_Text>().text = s;

            WordleManager.Answer += _inputString;
            WordleManager.CurrentChar++;
            WordleManager.CurrentChar = Mathf.Clamp(WordleManager.CurrentChar, 0, 5);
            WordleManager.AnswerLength++;
            _inputString = "";   
		    
            WordleManager.PunchTarget(WordleManager.CurrentUIColumn.transform, WordleManager.PunchForce, WordleManager.PunchDuration);
        }

        if (!WordleManager.AllowedWordsHashSet.Contains(WordleManager.Answer))
        {
            for (int i = 0; i < WordleManager.Answer.Length; i++)
            {
                if (WordleManager.Answer.Length == WordleManager.RequiredAnswerLength)
                {
                    var currentPanel = chars[i].gameObject;
				    
                    WordleManager.ShakeTarget(currentPanel.transform, WordleManager.InvalidPunchForce, WordleManager.InvalidPunchDuration);
				    
                    WordleManager.SetTileColours(chars, Color.red);
                }
            }
        }
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
