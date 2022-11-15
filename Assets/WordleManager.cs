using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WordleManager : MonoBehaviour
{
	[SerializeField] private string Solution;
	public string Answer;
	[SerializeField] private TextAsset AllowedWords;
	[SerializeField] private TextAsset PossibleWords;
	[SerializeField] private int WordLength = 0;
	[SerializeField] private List<string> AllowedWordsList;
	[SerializeField] private List<string> PossibleWordsList;
	[SerializeField] private List<CharContainer> UIRows;
	[SerializeField] private List<Char> UIColumns;

	[SerializeField] private string InputString = "";
	[SerializeField] private int InputCount = 0;
	[SerializeField] private int CurrentRound = 0;
	[SerializeField] private int CurrentChar = 0;

	[SerializeField] private bool Flashing = false;

	public KeyCode[] AcceptedKeys =
	{
		KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
		KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
		KeyCode.U, KeyCode.V, KeyCode.W,
		KeyCode.X, KeyCode.Y, KeyCode.Z
	};

	[SerializeField] private GameObject Keyboard;

	[SerializeField] private string[] KeyboardKeys =
	{
		"Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X", "C",
		"V", "B", "N", "M"
	};

	[SerializeField] private GameObject CurrentUIRow;
    [SerializeField] private GameObject CurrentUIColumn;

    [SerializeField] private GameObject DefeatCanvas; 
    [SerializeField] private TMP_Text SolutionText;
    
    [SerializeField] private GameObject WinCanvas; 
    [SerializeField] private TMP_Text AttemptsText;

    private void Awake()
    {
	    var seed = DateTime.Now.Ticks;
	    UnityEngine.Random.InitState((int) seed);
	    InitialiseValues();
	    UpdateRound();
	    AddWordsToList(AssetDatabase.GetAssetPath(AllowedWords), AllowedWordsList);
	    AddWordsToList(AssetDatabase.GetAssetPath(PossibleWords), PossibleWordsList);
	    PickRandomWord();
	    CurrentUIRow = UIRows[CurrentRound].gameObject;
	    CurrentUIColumn = UIColumns[CurrentChar].gameObject;
	    SetKeyBoardKeys();
	    
	    foreach (var _char in UIColumns)
	    {
		    _char.GetComponent<Animator>().enabled = true;
	    }
    }

    private void InitialiseValues()
    {
		InputString = "";
		WordLength = 0;
		InputCount = 0;
		CurrentRound = 0;
		CurrentChar = 0;
    }

    private void OnValidate()
    {
	    UIRows = GetComponentsInChildren<CharContainer>().ToList();
	    UIColumns = GetComponentsInChildren<Char>().ToList();
    }

    [ContextMenu("Set Keyboard Keys")]
    private void SetKeyBoardKeys()
    {
	    var keyboardKeys = Keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Typeable).ToList();

	    for (int i = 0; i < keyboardKeys.Count; i++)
	    {
		    var key = keyboardKeys[i];
		    
		    key.Key = KeyboardKeys[i];
			    
		    var textComponent = key.GetComponentInChildren<TMP_Text>();
        
		    if (textComponent)
		    {
			    textComponent.text = key.Key;
		    }
	    }

	    var backSpaceKey = Keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == "Backspace").ToList();
	    backSpaceKey[0].GetComponentInChildren<TMP_Text>().text = "<-";
	    
	    var enterKey = Keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == "Enter").ToList();
	    enterKey[0].GetComponentInChildren<TMP_Text>().text = "Go";
    }

    private void Update()
    {
	    if (WordLength > -1 && WordLength <= 4)
	    {
		    var key = ReadKeyInput();
		    
		    //Debug.Log("reading key " + key);
		    
		    if (Input.anyKeyDown && AcceptedKeys.Contains(key))
		    {
			    TypeKey(key.ToString());
			    //Debug.Log(Answer);
		    }
	    }

	    if (Input.GetKeyDown(KeyCode.Backspace))
	    {
		    DeleteLastChar();
	    }

	    if (Input.GetKeyDown(KeyCode.Return))
	    {
		    if (Solution != "")
		    {
			    CheckAnswer();   
		    }
		    else
		    {
			    Debug.Log("no solution");
		    }
	    }

	    //Debug.Log(InputString.Length);
	    //Debug.Log(Answer.Length);
    }

    public void DeleteLastChar()
    {
	    if (Answer.Length >= 1)
	    {
		    Answer = Answer.Substring(0, Answer.Length - 1);
		    CurrentChar--;
		    WordLength--;

		    var chars = CurrentUIRow.GetComponentsInChildren<Char>();
		    CurrentUIColumn = chars[CurrentChar].gameObject;
		    
		    CurrentUIColumn.GetComponentInChildren<TMP_Text>().text = "";
	    }
    }

    public void TypeKey(string key)
    {
	    var s = key;
	    Debug.Log("typing " + key);
	    InputString = s;

	    var chars = CurrentUIRow.GetComponentsInChildren<Char>();
	    CurrentUIColumn = chars[CurrentChar].gameObject;
	    
	    CurrentUIColumn.GetComponentInChildren<TMP_Text>().text = s;
	    Answer += InputString;
	    CurrentChar++;
	    WordLength++;
	    InputString = "";
	    
	    if (!AllowedWordsList.Contains(Answer.ToLower()))
	    {
		    for (int i = 0; i < Answer.Length; i++)
		    {
			    if (Answer.Length == 5)
			    {
				    var currentPanel = chars[i].gameObject;
				    
				    var currentPanelAnim = currentPanel.GetComponent<Animator>();
				    currentPanelAnim.SetTrigger(Animator.StringToHash("Flash Red"));
				    
				    //currentPanel.GetComponent<Animator>().ResetTrigger(Animator.StringToHash("Flash Red"));
			    }
		    }
	    }
    }

    public void CheckAnswer()
    {
	    if (AllowedWordsList.Contains(Answer.ToLower()))
	    {
		    Debug.Log("valid answer");
		    
		    var correctCount = 0;
	    
		    for (int i = 0; i < Answer.Length; i++)
		    {
			    if (Answer.Length == 5)
			    {
				    CurrentUIColumn = CurrentUIRow.GetComponentsInChildren<Char>()[i].gameObject;
				    
				    //get keyboard letter to light up
				    
				    GameObject currentKeyboardLetter = Keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == Answer[i].ToString()).ToList()[0].gameObject;
				    
				    Debug.Log(currentKeyboardLetter);
				    
				    var currentColumnImage = CurrentUIColumn.GetComponentInChildren<Image>();
				    var currentColumnText = CurrentUIColumn.GetComponentInChildren<TMP_Text>();
				    
				    var currentKeyboardLetterImage = currentKeyboardLetter.GetComponentInChildren<Image>();
				    var currentKeyboardLetterText = currentKeyboardLetter.GetComponentInChildren<TMP_Text>();
				    
				    var chars = CurrentUIRow.GetComponentsInChildren<Char>();
				    
				    var charAnim = CurrentUIColumn.GetComponent<Animator>();
				    
				    CurrentUIColumn = chars[i].gameObject;
					    
				    Debug.Log(CurrentUIColumn);

				    if (Answer[i].ToString().ToLower() == Solution[i].ToString().ToLower())
				    {
					    // we need to disable the animator to change colours because if we don't it overrides the colour change...
					    
					    charAnim.enabled = false;
					    
					    //make panel green
					    //make text black
					    
					    currentColumnImage.color = Color.green;

					    currentKeyboardLetterImage.color = Color.green;
					    currentKeyboardLetterText.color = Color.white;
					    
					    correctCount++;
					    
					    Debug.Log("green " + i);
				    }
				    
				    else if (Answer[i].ToString().ToLower() != Solution[i].ToString().ToLower() && Solution.ToLower().Contains(Answer[i].ToString().ToLower()))
				    {
					    // we need to disable the animator to change colours because if we don't it overrides the colour change...

					    charAnim.enabled = false;
					    
					    //make panel yellow
					    //make text black

					    currentColumnImage.color = Color.yellow;
					    currentColumnText.color = Color.black;

					    currentKeyboardLetterImage.color = Color.yellow;
					    currentKeyboardLetterText.color = Color.black;
					    
					    Debug.Log("YELLOW " + i);
				    }
				    
				    else if (!Solution.ToLower().Contains(Answer[i].ToString().ToLower()))
				    {
					    // we need to disable the animator to change colours because if we don't it overrides the colour change...
					    
					    charAnim.enabled = false;
					    
					    //make panel grey
					    //make text white
					    
					    currentColumnImage.color = Color.grey;
					    currentKeyboardLetterImage.color = Color.grey;
					    
					    Debug.Log("grey " + i);
				    }
			    }
		    }
	    
		    Debug.Log("correct letters: " + correctCount);

		    if (correctCount == 5)
		    {
			    Debug.Log("Winner");
			    WinCanvas.SetActive(true);
			    AttemptsText.text = (CurrentRound + 1).ToString() + " attempts";
		    }
		    else
		    {
			    if (CurrentRound < 4)
			    {
				    CurrentRound++;
				    UpdateRound();   
			    }
			    else
			    {
				    Debug.Log("Loser");
				    DefeatCanvas.SetActive(true);
				    SolutionText.text = Solution.ToUpper();
				    //show defeat canvas
			    }
		    }
	    }
    }

    void UpdateRound()
    {
	    InputString = "";
	    Answer = "";
	    WordLength = 0;
	    CurrentChar = 0;
	    CurrentUIRow = UIRows[CurrentRound].gameObject;
	    CurrentUIColumn = CurrentUIRow.GetComponentsInChildren<Char>()[CurrentChar].gameObject;
	    //current_char_name = "Container/WordsContainer/Label" + str(current_round) + "/Char"
    }

    void AddWordsToList(string path, List<string> list)
    {
	    StreamReader reader = new StreamReader(path);

	    var index = 1;
	    
	    while (!reader.EndOfStream)
	    {
		    var line = reader.ReadLine();
		    list.Add(line);
		    index++;
	    }
	    reader.Close();
    }

    void PickRandomWord()
    {
	    var num = UnityEngine.Random.Range(0, PossibleWordsList.Count - 1);
	    Solution = PossibleWordsList[num];
    }

    private KeyCode ReadKeyInput()
    {
        foreach(KeyCode vkey in System.Enum.GetValues(typeof(KeyCode)))
        {
	        if(Input.GetKeyDown(vkey))
	        {
		        if (vkey != KeyCode.Return)
		        {
			        return vkey;
		        }
	        }
        }

        return KeyCode.KeypadPeriod;
    }

    public void RestartGame()
    {
	    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
