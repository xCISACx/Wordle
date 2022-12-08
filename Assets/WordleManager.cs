using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WordleManager : MonoBehaviour
{
	[Header("Tile Settings")]
	
	public Color DefaultColour = Color.black;
	public Color CorrectColour = new Color(108,169,101, 255);
	public Color WrongPlaceColour = new Color(200,182,83, 255);
	public Color IncorrectColour = new Color(120,124,127, 255);
	public float ColourChangeDuration = 0.3f;
	public Vector3 PunchForce = new Vector3(0, 10f, 0);
	public float PunchDuration = 0.5f;
	
	[Header("Match Variables")]
	
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
	[SerializeField] private string _remaining;
    private readonly Dictionary<string, int> _solutionLetterFrequencyDictionary = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _guessLetterFrequencyDictionary = new Dictionary<string, int>();
	
	[Header("Input Variables")]

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
    
    [Header("Game Over Variables")]

    [SerializeField] private GameObject _defeatCanvas; 
    [SerializeField] private TMP_Text _solutionTextWin;
    [SerializeField] private TMP_Text _solutionTextLose;
    
    [SerializeField] private GameObject _winCanvas; 
    [SerializeField] private TMP_Text _attemptsText;

    
    private void Awake()
    {
	    var seed = DateTime.Now.Ticks;
	    
	    UnityEngine.Random.InitState((int) seed);
	    
	    InitialiseValues();
	    UpdateRound();
	    
	    AddWordsToList("AllowedWords", _allowedWordsList);
	    //Debug.LogWarning(_allowedWordsList.Count);
	    AddWordsToList("PossibleWords", _possibleWordsList);
	    //Debug.LogWarning(_possibleWordsList.Count);
	    
	    PickRandomWord();
	    
	    _currentUIRow = _uiRows[_currentRound].gameObject;
	    _currentUIColumn = _uiColumns[_currentChar].gameObject;
	    
	    SetKeyBoardKeys();
	    
	    InitGuessFrequencyDictionary();
	    InitSolutionFrequencyDictionary();
	    
	    UpdateSolutionFrequencyDictionary();

	    /*foreach (var @char in _uiColumns)
	    {
		    @char.GetComponent<Animator>().enabled = true;
	    }*/
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
    
    [ContextMenu("Init Solution Frequency")]
    void InitSolutionFrequencyDictionary()
    {
	    _solutionLetterFrequencyDictionary.Clear();

	    foreach (var key in _keyboardKeys)
	    {
		    _solutionLetterFrequencyDictionary.Add(key, 0);
	    }

	    foreach (var pair in _solutionLetterFrequencyDictionary)
	    {
		    Debug.Log("Letter: " + pair.Key.ToString() + ", Frequency: " + pair.Value.ToString());
	    }

	    Debug.Log("init solution frequency");
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
    
    [ContextMenu("Update Solution Frequency")]
    void UpdateSolutionFrequencyDictionary()
    {
	    _solutionLetterFrequencyDictionary.Clear();

	    foreach (var key in _keyboardKeys)
	    {
		    _solutionLetterFrequencyDictionary.Add(key, 0);
	    }

	    for (int i = 0; i < _solution.Length; i++)
	    {
		    Debug.Log( _solutionLetterFrequencyDictionary[_solution[i].ToString()]);
		    _solutionLetterFrequencyDictionary[_solution[i].ToString()] += 1;
	    }

	    foreach (var pair in _solutionLetterFrequencyDictionary)
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

    private void TweenToRed(Transform target, float duration)
    {
	    target.GetComponentInChildren<Image>().DOColor(Color.red, duration);
    }

    private void PunchTarget(Transform target, Vector3 force, float duration)
    {
	    target.DOPunchPosition(force, duration);
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
		    _currentChar = Mathf.Clamp(_currentChar, 0, 4);

		    var chars = _currentUIRow.GetComponentsInChildren<Char>();
		    _currentUIColumn = chars[_currentChar].gameObject;
		    
		    _currentUIColumn.GetComponentInChildren<TMP_Text>().text = "";
		    
		    for (int i = 0; i < _answer.Length + 1; i++)
		    {
			    var currentPanel = chars[i].gameObject;
			    var currentPanelImage = currentPanel.GetComponentInChildren<Image>();
			    
			    currentPanelImage.color = DefaultColour;
		    }
	    }
    }

    public void TypeKey(string key)
    {
	    var chars = _currentUIRow.GetComponentsInChildren<Char>();
	    
	    if (_answer.Length <= 4)
	    {
		    var s = key;
			_inputString = s;
			
			_currentUIColumn = chars[_currentChar].gameObject;

			_currentUIColumn.GetComponentInChildren<TMP_Text>().text = s;

		    _answer += _inputString;
		    _currentChar++;
		    _currentChar = Mathf.Clamp(_currentChar, 0, 5);
		    _wordLength++;
		    _inputString = "";   
		    
		    PunchTarget(_currentUIColumn.transform, new Vector3(0, 5f, 0), PunchDuration);
		    
		    //_currentUIColumn.transform.DOPunchPosition(new Vector3(0, 5f, 0), PunchDuration);
	    }

	    if (!_allowedWordsList.Contains(_answer))
	    {
		    for (int i = 0; i < _answer.Length; i++)
		    {
			    if (_answer.Length == 5)
			    {
				    var currentPanel = chars[i].gameObject;
				    
				    PunchTarget(currentPanel.transform, PunchForce, PunchDuration);
				    
				    //currentPanel.transform.DOPunchPosition(PunchForce, PunchDuration);
				    
				    TweenToRed(currentPanel.transform, ColourChangeDuration);

				    //var currentPanelAnim = currentPanel.GetComponent<Animator>();
				    //currentPanelAnim.SetTrigger(Animator.StringToHash("Flash Red"));
				    
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

		    UpdateGuessFrequencyDictionary(_answer);

		    if (_answer.Length == 5)
		    {
			    _remaining = _answer;
			    
			    for (int i = 0; i < _answer.Length; i++)
			    {
				    if (!_guessedLettersList.Contains(_answer[i].ToString()))
				    {
					    _guessedLettersList.Add(_answer[i].ToString());
				    }
				    
				    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[i].gameObject;
				    
				    //get keyboard letter to light up
				    
				    GameObject currentKeyboardLetter = _keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == _answer[i].ToString()).ToList()[0].gameObject;
				    KeyboardKey currentKeyboardKey = currentKeyboardLetter.GetComponent<KeyboardKey>();
				    
				    Debug.Log(currentKeyboardLetter);

				    var chars = _currentUIRow.GetComponentsInChildren<Char>();
				    
				    _currentUIColumn = chars[i].gameObject;

				    PunchTarget(chars[i].transform, new Vector3(0, 3f, 0), PunchDuration);
				    
				    // if the current letter matches the position of the same letter in the solution

				    if (_answer[i].ToString() == _solution[i].ToString())
				    {
					    currentKeyboardKey.state = KeyboardKey.tileState.Correct;
					    
					    _currentUIColumn.GetComponent<Char>().State = Char.TileState.Correct;

					    _remaining = _remaining.Remove(i, 1);
					    _remaining = _remaining.Insert(i, " ");
					    
					    correctCount++;

					    Debug.Log("green " + i);
				    }

				    // if the current letter does not match the position of the same letter in the solution and does not exist in the solution
				    
				    else if (!_solution.Contains(_answer[i].ToString()))
				    {
					    currentKeyboardKey.state = KeyboardKey.tileState.Incorrect;
					    _currentUIColumn.GetComponent<Char>().State = Char.TileState.Incorrect;
					    
					    Debug.Log("grey " + i);
				    }
			    }

			    bool multipleCount = false;
			    
			    for (int j = 0; j < _answer.Length; j++)
			    {
				    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[j].gameObject;
				    
				    //get keyboard letter to light up
				    
				    GameObject currentKeyboardLetter = _keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == _answer[j].ToString()).ToList()[0].gameObject;
				    KeyboardKey currentKeyboardKey = currentKeyboardLetter.GetComponent<KeyboardKey>();

				    Char currentChar = _currentUIRow.GetComponentsInChildren<Char>()[j];

				    if (currentChar.State != Char.TileState.Correct && currentChar.State != Char.TileState.Incorrect)
				    {
					    if (_remaining.Contains(_answer[j]))
					    {
						    // if the solution only has one instance of the letter and the guess has two or more:
						    // mark the first as wrong place and the rest incorrect

						    if (_solutionLetterFrequencyDictionary[_answer[j].ToString()] < 2 && _guessLetterFrequencyDictionary[_answer[j].ToString()] > 1 && !multipleCount)
						    {
							    int index = _remaining.IndexOf(_answer[j]);
							    _remaining = _remaining.Remove(index, 1);
							    _remaining = _remaining.Insert(index, " ");
							    currentChar.State = Char.TileState.WrongPlace;
							    
							    Debug.Log("yellow " + j);
							    
							    multipleCount = true;
						    }
						    else
						    {
							    if (_solutionLetterFrequencyDictionary[_answer[j].ToString()] >=
							             _guessLetterFrequencyDictionary[_answer[j].ToString()])
							    {
								    currentChar.State = Char.TileState.WrongPlace;
								    
								    Debug.Log("YELLOW LAST " + j);
							    }
							    else
							    {
								    currentChar.State = Char.TileState.Incorrect;

								    Debug.Log("grey LAST " + j);   
							    }
						    }
						    
						    // check how many of a letter is in the answer and guess that have been marked as correct

						    if (_guessLetterFrequencyDictionary[_answer[j].ToString()] > _solutionLetterFrequencyDictionary[_answer[j].ToString()])
						    {
							    for (int i = 0; i < _answer.Length; i++)
							    {
								    var newCurrentCharI = _currentUIRow.GetComponentsInChildren<Char>()[i];
								    var newCurrentCharJ = _currentUIRow.GetComponentsInChildren<Char>()[j];

								    if (newCurrentCharI.State == Char.TileState.Correct && _answer[i] == _answer[j])
								    {
									    // we already have an instance of that letter that is correct, so mark all others as incorrect
									    newCurrentCharJ.State = Char.TileState.Incorrect;
									    
									    Debug.Log("There's already another instance of " + _answer[j] + " that is in the right place so all other are incorrect");
								    }
							    }
						    }

						    if (currentKeyboardKey.state != KeyboardKey.tileState.Correct)
						    {
							    currentKeyboardKey.state = KeyboardKey.tileState.WrongPlace;

							    Debug.Log("Making keyboard " + _answer[j] + " YELLOW since it's not green");
						    }
					    }
					    else
					    {
						    currentChar.State = Char.TileState.Incorrect;

						    Debug.Log("grey LAST " + j);
					    }
				    }
			    }
		    }

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
