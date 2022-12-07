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
	[SerializeField] private int _wordLength = 0;
	[SerializeField] private List<string> _allowedWordsList;
	[SerializeField] private List<string> _possibleWordsList;
	[SerializeField] private List<string> _guessedLettersList;
	[SerializeField] private List<CharContainer> _uiRows;
	[SerializeField] private List<Char> _uiColumns;

	[SerializeField] private string _inputString = "";
	[SerializeField] private int _currentRound = 0;
	[SerializeField] private int _maxRounds = 5;
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
    [SerializeField] private TMP_Text _solutionTextWin;
    [SerializeField] private TMP_Text _solutionTextLose;
    
    [SerializeField] private GameObject _winCanvas; 
    [SerializeField] private TMP_Text _attemptsText;

    private Dictionary<string, int> _letterFrequencyDictionary = new Dictionary<string, int>();
    private Dictionary<string, int> _guessLetterFrequencyDictionary = new Dictionary<string, int>();
    
    [SerializeField] List<string> _previousLetters = new List<string>();

    private void Awake()
    {
	    var seed = DateTime.Now.Ticks;
	    UnityEngine.Random.InitState((int) seed);
	    InitialiseValues();
	    UpdateRound();
	    AddWordsToList("AllowedWords", _allowedWordsList);
	    Debug.LogWarning(_allowedWordsList.Count);
	    AddWordsToList("PossibleWords", _possibleWordsList);
	    Debug.LogWarning(_possibleWordsList.Count);
	    PickRandomWord();
	    _currentUIRow = _uiRows[_currentRound].gameObject;
	    _currentUIColumn = _uiColumns[_currentChar].gameObject;
	    SetKeyBoardKeys();
	    InitGuessFrequencyDictionary();

	    foreach (var @char in _uiColumns)
	    {
		    @char.GetComponent<Animator>().enabled = true;
	    }
    }

    private void InitialiseValues()
    {
		_inputString = "";
		_wordLength = 0;
		_currentRound = 0;
		_currentChar = 0;
    }
    
    [ContextMenu("Init Guess Frequency")]
    void InitGuessFrequencyDictionary()
    {
	    _guessLetterFrequencyDictionary.Clear();

	    foreach (var key in _keyboardKeys)
	    {
		    _guessLetterFrequencyDictionary.Add(key, 0);
	    }

	    foreach (var pair in _guessLetterFrequencyDictionary)
	    {
		    Debug.Log("Letter: " + pair.Key.ToString() + ", Frequency: " + pair.Value.ToString());
	    }

	    Debug.Log("init guess frequency");
    }

    [ContextMenu("Update Guess Frequency")]
    void UpdateGuessFrequencyDictionary(string answer)
    {
	    _guessLetterFrequencyDictionary.Clear();

	    foreach (var key in _keyboardKeys)
	    {
		    _guessLetterFrequencyDictionary.Add(key, 0);
	    }

	    for (int i = 0; i < answer.Length; i++)
	    {
		    Debug.Log( _guessLetterFrequencyDictionary[answer[i].ToString()]);
		    _guessLetterFrequencyDictionary[answer[i].ToString()] += 1;
	    }

	    foreach (var pair in _guessLetterFrequencyDictionary)
	    {
		    Debug.Log("Letter: " + pair.Key.ToString() + ", Frequency: " + pair.Value.ToString());
	    }
	    
	    Debug.Log("updated frequency");
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
	    
	    Debug.LogWarning(_answer);
	    
	    if (!_allowedWordsList.Contains(_answer))
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
	    if (_allowedWordsList.Contains(_answer))
	    {
		    Debug.Log("valid answer");
		    
		    var correctCount = 0;
		    _guessedLettersList.Clear();
		    var correctLetterCount = 0;

		    UpdateGuessFrequencyDictionary(_answer);

		    if (_answer.Length == 5)
		    {
			    string remaining = _answer;
			    
			    for (int i = 0; i < _answer.Length; i++)
			    {
				    if (!_guessedLettersList.Contains(_answer[i].ToString()))
				    {
					    _guessedLettersList.Add(_answer[i].ToString());
				    }
				    
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
				    
				    // get the letters before the current letter

				    for (int j = i; j > 0; j--)
				    {
					    _previousLetters.Add(_answer[j].ToString());
				    }

				    _currentUIColumn = chars[i].gameObject;

				    // if the current letter matches the position of the same letter in the solution

				    if (_answer[i].ToString() == _solution[i].ToString())
				    {
					    // we need to disable the animator to change colours because if we don't it overrides the colour change...
					    
					    charAnim.enabled = false;
					    
					    //make panel green
					    //make text black
					    
					    currentColumnImage.color = Color.green;

					    currentKeyboardLetterImage.color = Color.green;
					    currentKeyboardLetterText.color = Color.white;
					    
					    _currentUIColumn.GetComponent<Char>().state = Char.tileState.Correct;

					    remaining = remaining.Remove(i, 1);
					    remaining = remaining.Insert(i, " ");
					    
					    correctCount++;
					    correctLetterCount++;
					    
					    Debug.Log("green " + i);
				    }
				    
				    // if the current letter does not match the position of the same letter in the solution and does not exist in the solution
				    
				    else if (!_solution.Contains(_answer[i].ToString()))
				    {
					    // we need to disable the animator to change colours because if we don't it overrides the colour change...
					    
					    /*charAnim.enabled = false;
					    
					    //make panel grey
					    //make text white
					    
					    currentColumnImage.color = Color.grey;
					    currentKeyboardLetterImage.color = Color.grey;*/

					    _currentUIColumn.GetComponent<Char>().state = Char.tileState.Incorrect;
					    
					    Debug.Log("grey " + i);
				    }
			    }
			    
			    for (int j = 0; j < _answer.Length; j++)
			    {
				    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[j].gameObject;
				    
				    //get keyboard letter to light up
				    
				    GameObject currentKeyboardLetter = _keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == _answer[j].ToString()).ToList()[0].gameObject;

				    var currentColumnImage = _currentUIColumn.GetComponentInChildren<Image>();
				    var currentColumnText = _currentUIColumn.GetComponentInChildren<TMP_Text>();
				    
				    var currentKeyboardLetterImage = currentKeyboardLetter.GetComponentInChildren<Image>();
				    var currentKeyboardLetterText = currentKeyboardLetter.GetComponentInChildren<TMP_Text>();
				    
				    Char currentChar = _currentUIRow.GetComponentsInChildren<Char>()[j];
				    
				    var charAnim = _currentUIColumn.GetComponent<Animator>();

				    if (currentChar.state != Char.tileState.Correct && currentChar.state != Char.tileState.Incorrect)
				    {
					    if (remaining.Contains(_answer[j]))
					    {
						    currentChar.state = Char.tileState.WrongPlace;

						    int index = remaining.IndexOf(_answer[j]);
						    remaining = remaining.Remove(index, 1);
						    remaining = remaining.Insert(index, " ");
						    
						    Debug.Log("yellow " + j);
						    
						    // we need to disable the animator to change colours because if we don't it overrides the colour change...
					    
						    /*charAnim.enabled = false;
						    
						    /*_currentUIRow.GetComponentsInChildren<Char>()[index].gameObject.GetComponentInChildren<Image>().color = Color.yellow;
						    _currentUIRow.GetComponentsInChildren<Char>()[index].gameObject.GetComponentInChildren<TMP_Text>().color = Color.black;#1#
						    
						    Debug.Log(currentColumnImage);

						    currentColumnImage.color = Color.yellow;
						    currentColumnText.color = Color.black;*/

						    if (currentKeyboardLetterImage.color != Color.green)
						    {
							    currentKeyboardLetterImage.color = Color.yellow;
							    currentKeyboardLetterText.color = Color.black;
						    
							    Debug.Log("Making keyboard " + _answer[j] + " YELLOW since it's not green");
						    }
						    
					    }
					    else
					    {
						    currentChar.state = Char.tileState.Incorrect;
						    
						    /*// we need to disable the animator to change colours because if we don't it overrides the colour change...
					    
						    charAnim.enabled = false;
						    
						    currentColumnImage.color = Color.grey;
						    currentColumnText.color = Color.white;*/
						    
						    Debug.Log("grey LAST " + j);
					    }
				    }
			    }
		    }

		    /*for (int i = 0; i < _answer.Length; i++)
		    {
			    _previousLetters.Clear();
			    
			    if (_answer.Length == 5)
			    {
				    if (!_guessedLettersList.Contains(_answer[i].ToString()))
				    {
					    _guessedLettersList.Add(_answer[i].ToString());
				    }
				    
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
				    
				    // get the letters before the current letter

				    for (int j = i; j > 0; j--)
				    {
					    _previousLetters.Add(_answer[j].ToString());
				    }

				    _currentUIColumn = chars[i].gameObject;
					    
				    Debug.Log(_currentUIColumn);

				    // if the current letter does not match the position of the same letter in the solution but exists in the solution
				    
				    if (_answer[i].ToString() != _solution[i].ToString() && _solution.Contains(_answer[i].ToString()))
				    {
					    // we need to disable the animator to change colours because if we don't it overrides the colour change...

					    charAnim.enabled = false;
					    
					    //make panel yellow
					    //make text black
					    
					    currentColumnImage.color = Color.yellow;
					    currentColumnText.color = Color.black;
					    
					    Debug.Log("Making char " + i + " YELLOW since it's not in the right place");

					    // if the current letter is already in the word in another place, mark it grey instead of yellow

					    Debug.Log("Char " + i);
					    Debug.Log(_answer.Contains(_answer[i].ToString()));
					    Debug.Log(_guessedLettersList.Contains(_answer[i].ToString()));
					    Debug.Log(_guessLetterFrequencyDictionary[_answer[i].ToString()] > 1);
					    Debug.Log(_previousLetters.Contains(_answer[i].ToString()));

					    if (_solution.Contains(_answer[i].ToString()) && _guessedLettersList.Contains(_answer[i].ToString()) 
					        && _guessLetterFrequencyDictionary[_answer[i].ToString()] > 1 && _previousLetters.Contains(_answer[i].ToString()))
					    {
						    currentColumnImage.color = Color.grey;
						    currentColumnText.color = Color.white;

						    Debug.Log("Making char " + i + " GRAY since there's another instance of it in the word in the right place");
					    }
					    
					    // if the current letter is not already marked green on the keyboard, mark it yellow, otherwise leave it be

					    if (currentKeyboardLetterImage.color != Color.green)
					    {
						    currentKeyboardLetterImage.color = Color.yellow;
						    currentKeyboardLetterText.color = Color.black;
						    
						    Debug.Log("Making keyboard " + _answer[i] + " YELLOW since it's not green");
					    }
				    }
				    
				    // if the current letter does not match the position of the same letter in the solution and does not exist in the solution
				    
				    else if (!_solution.Contains(_answer[i].ToString()))
				    {
					    // we need to disable the animator to change colours because if we don't it overrides the colour change...
					    
					    charAnim.enabled = false;
					    
					    //make panel grey
					    //make text white
					    
					    currentColumnImage.color = Color.grey;
					    currentKeyboardLetterImage.color = Color.grey;
					    
					    Debug.Log("grey " + i);
				    }
				    
				    // if the current letter matches the position of the same letter in the solution

				    if (_answer[i].ToString() == _solution[i].ToString())
				    {
					    // we need to disable the animator to change colours because if we don't it overrides the colour change...
					    
					    charAnim.enabled = false;
					    
					    //make panel green
					    //make text black
					    
					    currentColumnImage.color = Color.green;

					    currentKeyboardLetterImage.color = Color.green;
					    currentKeyboardLetterText.color = Color.white;
					    
					    correctCount++;
					    correctLetterCount++;
					    
					    Debug.Log("green " + i);
				    }
			    }
		    }*/
	    
		    Debug.Log("correct letters: " + correctCount);

		    if (correctCount == 5)
		    {
			    Debug.Log("Winner");
			    _winCanvas.SetActive(true);
			    _solutionTextWin.text = _solution.ToUpper();
			    _attemptsText.text = (_currentRound + 1).ToString() + " attempt(s)";
		    }
		    else
		    {
			    if (_currentRound < _maxRounds)
			    {
				    _currentRound++;
				    UpdateRound();   
			    }
			    else
			    {
				    Debug.Log("Loser");
				    _defeatCanvas.SetActive(true);
				    _solutionTextLose.text = _solution.ToUpper();
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
	    var file = Resources.Load<TextAsset>(path);

	    var fileContent = file.text;

	#if UNITY_WEBGL

	    var fileWords = fileContent.Split("\r\n", StringSplitOptions.None);
	    
	#endif

	#if !UNITY_WEBGL
	
		  var fileWords = fileContent.Split(Environment.NewLine, StringSplitOptions.None);
	    
	#endif

	    foreach (var word in fileWords)
	    {
		    list.Add(word.ToUpper());
	    }
	    
	    //list = new List<string>(fileWords);

	    /*var words = File.ReadLines(path);
		    
	    StreamReader reader = new StreamReader(path);

	    var index = 1;
	    
	    while (!reader.EndOfStream)
	    {
		    var line = reader.ReadLine();
		    list.Add(line);
		    index++;
	    }
	    reader.Close();*/
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
