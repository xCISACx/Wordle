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
	[SerializeField] private string _solution;
	[SerializeField] private string _answer;
	[SerializeField] private TextAsset _allowedWords;
	[SerializeField] private TextAsset _possibleWords;
	[SerializeField] private int _wordLength = 0;
	[SerializeField] private List<string> _allowedWordsList;
	[SerializeField] private List<string> _possibleWordsList;
	[SerializeField] private List<CharContainer> _uiRows;
	[SerializeField] private List<Char> _uiColumns;

	[SerializeField] private string _inputString = "";
	[SerializeField] private int _inputCount = 0;
	[SerializeField] private int _currentRound = 0;
	[SerializeField] private int _currentChar = 0;

	[SerializeField] private bool _flashing = false;

	public KeyCode[] AcceptedKeys =
	{
		KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J,
		KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T,
		KeyCode.U, KeyCode.V, KeyCode.W,
		KeyCode.X, KeyCode.Y, KeyCode.Z
	};

	[SerializeField] private GameObject _keyboard;

	[SerializeField] private string[] _keyboardKeys =
	{
		"Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P", "A", "S", "D", "F", "G", "H", "J", "K", "L", "Z", "X", "C",
		"V", "B", "N", "M"
	};

	[SerializeField] private GameObject _currentUIRow;
    [SerializeField] private GameObject _currentUIColumn;

    [SerializeField] private GameObject _defeatCanvas; 
    [SerializeField] private TMP_Text _solutionText;
    
    [SerializeField] private GameObject _winCanvas; 
    [SerializeField] private TMP_Text _attemptsText;

    private void Awake()
    {
	    var seed = DateTime.Now.Ticks;
	    UnityEngine.Random.InitState((int) seed);
	    InitialiseValues();
	    UpdateRound();
	    AddWordsToList(AssetDatabase.GetAssetPath(_allowedWords), _allowedWordsList);
	    AddWordsToList(AssetDatabase.GetAssetPath(_possibleWords), _possibleWordsList);
	    PickRandomWord();
	    _currentUIRow = _uiRows[_currentRound].gameObject;
	    _currentUIColumn = _uiColumns[_currentChar].gameObject;
	    SetKeyBoardKeys();
	    
	    foreach (var @char in _uiColumns)
	    {
		    @char.GetComponent<Animator>().enabled = true;
	    }
    }

    private void InitialiseValues()
    {
		_inputString = "";
		_wordLength = 0;
		_inputCount = 0;
		_currentRound = 0;
		_currentChar = 0;
    }

    private void OnValidate()
    {
	    _uiRows = GetComponentsInChildren<CharContainer>().ToList();
	    _uiColumns = GetComponentsInChildren<Char>().ToList();
    }

    [ContextMenu("Set Keyboard Keys")]
    private void SetKeyBoardKeys()
    {
	    var keyboardKeys = _keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Typeable).ToList();

	    for (int i = 0; i < keyboardKeys.Count; i++)
	    {
		    var key = keyboardKeys[i];
		    
		    key.Key = _keyboardKeys[i];
			    
		    var textComponent = key.GetComponentInChildren<TMP_Text>();
        
		    if (textComponent)
		    {
			    textComponent.text = key.Key;
		    }
	    }

	    var backSpaceKey = _keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == "Backspace").ToList();
	    backSpaceKey[0].GetComponentInChildren<TMP_Text>().text = "<-";
	    
	    var enterKey = _keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == "Enter").ToList();
	    enterKey[0].GetComponentInChildren<TMP_Text>().text = "Go";
    }

    private void Update()
    {
	    if (_wordLength > -1 && _wordLength <= 4)
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
		    if (_solution != "")
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
	    if (_answer.Length >= 1)
	    {
		    _answer = _answer.Substring(0, _answer.Length - 1);
		    _currentChar--;
		    _wordLength--;

		    var chars = _currentUIRow.GetComponentsInChildren<Char>();
		    _currentUIColumn = chars[_currentChar].gameObject;
		    
		    _currentUIColumn.GetComponentInChildren<TMP_Text>().text = "";
	    }
    }

    public void TypeKey(string key)
    {
	    var s = key;
	    Debug.Log("typing " + key);
	    _inputString = s;

	    var chars = _currentUIRow.GetComponentsInChildren<Char>();
	    _currentUIColumn = chars[_currentChar].gameObject;
	    
	    _currentUIColumn.GetComponentInChildren<TMP_Text>().text = s;
	    _answer += _inputString;
	    _currentChar++;
	    _wordLength++;
	    _inputString = "";
	    
	    if (!_allowedWordsList.Contains(_answer.ToLower()))
	    {
		    for (int i = 0; i < _answer.Length; i++)
		    {
			    if (_answer.Length == 5)
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
	    if (_allowedWordsList.Contains(_answer.ToLower()))
	    {
		    Debug.Log("valid answer");
		    
		    var correctCount = 0;
	    
		    for (int i = 0; i < _answer.Length; i++)
		    {
			    if (_answer.Length == 5)
			    {
				    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[i].gameObject;
				    
				    //get keyboard letter to light up
				    
				    GameObject currentKeyboardLetter = _keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == _answer[i].ToString()).ToList()[0].gameObject;
				    
				    Debug.Log(currentKeyboardLetter);
				    
				    var currentColumnImage = _currentUIColumn.GetComponentInChildren<Image>();
				    var currentColumnText = _currentUIColumn.GetComponentInChildren<TMP_Text>();
				    
				    var currentKeyboardLetterImage = currentKeyboardLetter.GetComponentInChildren<Image>();
				    var currentKeyboardLetterText = currentKeyboardLetter.GetComponentInChildren<TMP_Text>();
				    
				    var chars = _currentUIRow.GetComponentsInChildren<Char>();
				    
				    var charAnim = _currentUIColumn.GetComponent<Animator>();
				    
				    _currentUIColumn = chars[i].gameObject;
					    
				    Debug.Log(_currentUIColumn);

				    if (_answer[i].ToString().ToLower() == _solution[i].ToString().ToLower())
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
				    
				    else if (_answer[i].ToString().ToLower() != _solution[i].ToString().ToLower() && _solution.ToLower().Contains(_answer[i].ToString().ToLower()))
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
				    
				    else if (!_solution.ToLower().Contains(_answer[i].ToString().ToLower()))
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
			    _winCanvas.SetActive(true);
			    _attemptsText.text = (_currentRound + 1).ToString() + " attempts";
		    }
		    else
		    {
			    if (_currentRound < 4)
			    {
				    _currentRound++;
				    UpdateRound();   
			    }
			    else
			    {
				    Debug.Log("Loser");
				    _defeatCanvas.SetActive(true);
				    _solutionText.text = _solution.ToUpper();
				    //show defeat canvas
			    }
		    }
	    }
    }

    void UpdateRound()
    {
	    _inputString = "";
	    _answer = "";
	    _wordLength = 0;
	    _currentChar = 0;
	    _currentUIRow = _uiRows[_currentRound].gameObject;
	    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[_currentChar].gameObject;
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
	    var num = UnityEngine.Random.Range(0, _possibleWordsList.Count - 1);
	    _solution = _possibleWordsList[num];
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
