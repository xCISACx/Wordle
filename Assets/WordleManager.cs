using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WordleManager : MonoBehaviour
{
	public InputManager InputManager;
	
	[Header("Tile Settings")]
	
	public Color DefaultColour = Color.black;
	public Color CorrectColour = new Color(108,169,101, 255);
	public Color WrongPlaceColour = new Color(200,182,83, 255);
	public Color IncorrectColour = new Color(120,124,127, 255);
	public Vector3 PunchForce = new Vector3(0, 10f, 0);
	public Vector3 InvalidPunchForce = new Vector3(10f, 0, 0);
	public float PunchDuration = 0.5f;
	public float InvalidPunchDuration = 0.1f;

	[Header("Match Variables")]
	
	[SerializeField] private string _solution;
	[SerializeField] private string _answer;
	[SerializeField] private int _wordLength;
	private HashSet<string> _possibleWordsHashSet = new HashSet<string>();
	private HashSet<string> _allowedWordsHashSet = new HashSet<string>();
	[SerializeField] private List<CharContainer> _uiRows;
	[SerializeField] private List<Char> _uiColumns;

	[SerializeField] private string _inputString = "";
	[SerializeField] private int _currentRound;
	[SerializeField] private int _maxRounds = 5;
	[SerializeField] private int _currentChar;
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

	[SerializeField] private CharContainer _currentUIRow;
    [SerializeField] private Char _currentUIColumn;
    
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

	    AddWordsToHashSet("AllowedWords", _allowedWordsHashSet);
	    
	    AddWordsToHashSet("PossibleWords", _possibleWordsHashSet);
	    
	    //Debug.Log(_possibleWordsHashSet.Count);
	    //Debug.Log(_allowedWordsHashSet.Count);
	    
	    PickRandomWord();
	    
	    _uiRows = GetComponentsInChildren<CharContainer>().ToList();
	    _uiColumns = GetComponentsInChildren<Char>().ToList();
	    
	    _currentUIRow = _uiRows[_currentRound];
	    _currentUIColumn = _uiColumns[_currentChar];

	    InputManager = GetComponent<InputManager>();
	    
	    InputManager.SetKeyBoardKeys();
	    
		UpdateFrequencyDictionary(_guessLetterFrequencyDictionary, _answer);
	    UpdateFrequencyDictionary(_solutionLetterFrequencyDictionary, _solution);
    }

    private void InitialiseValues()
    {
		_inputString = "";
		_wordLength = 0;
		_currentRound = 0;
		_currentChar = 0;
    }

    private void OnValidate()
    {
	    _uiRows = GetComponentsInChildren<CharContainer>().ToList();
	    _uiColumns = GetComponentsInChildren<Char>().ToList();
    }

    private void Update()
    {
	    if (_wordLength > -1 && _wordLength <= 4)
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
		    if (_solution != "")
		    {
			    CheckAnswer();   
		    }
		    else
		    {
			    //Debug.Log("no solution");
		    }
	    }
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
		    _currentUIColumn = chars[_currentChar];
		    
		    _currentUIColumn.GetComponentInChildren<TMP_Text>().text = "";
		    
		    SetTileColours(chars, DefaultColour);
	    }
    }

    void SetTileColours(Char[] tiles, Color color)
    {
	    for (int i = 0; i < 5; i++)
	    {
		    var currentPanel = tiles[i].gameObject;
		    var currentPanelImage = currentPanel.GetComponentInChildren<Image>();
			    
		    currentPanelImage.color = color;
		    //Debug.Log("reset tile " + i + "'s colour");
	    }
    }

    public void TypeKey(string key)
    {
	    var chars = _currentUIRow.GetComponentsInChildren<Char>();
	    
	    if (_answer.Length <= 4)
	    {
		    var s = key;
			_inputString = s;
			
			_currentUIColumn = chars[_currentChar];

			_currentUIColumn.GetComponentInChildren<TMP_Text>().text = s;

		    _answer += _inputString;
		    _currentChar++;
		    _currentChar = Mathf.Clamp(_currentChar, 0, 5);
		    _wordLength++;
		    _inputString = "";   
		    
		    PunchTarget(_currentUIColumn.transform, PunchForce, PunchDuration);
	    }

	    if (!_allowedWordsHashSet.Contains(_answer))
	    {
		    for (int i = 0; i < _answer.Length; i++)
		    {
			    if (_answer.Length == 5)
			    {
				    var currentPanel = chars[i].gameObject;
				    
				    ShakeTarget(currentPanel.transform, InvalidPunchForce, InvalidPunchDuration);
				    
				    SetTileColours(chars, Color.red);
			    }
		    }
	    }
    }

    public void CheckAnswer()
    {
	    if (!_allowedWordsHashSet.Contains(_answer))
	    {
		    //Debug.Log("valid answer");
		    return;
	    }

	    int correctCount = 0;

	    UpdateFrequencyDictionary(_guessLetterFrequencyDictionary, _answer);

	    if (_answer.Length == 5)
	    {
		    _remaining = _answer;

		    for (int i = 0; i < _answer.Length; i++)
		    {
			    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[i];
			    
			    //get keyboard letter to light up

			    GameObject currentKeyboardLetter = GetCurrentKeyboardLetter(_answer, i);
			    KeyboardKey currentKeyboardKey = currentKeyboardLetter.GetComponent<KeyboardKey>();
			    
			    //Debug.Log(currentKeyboardLetter);

			    Char[] chars = _currentUIRow.GetComponentsInChildren<Char>();
			    
			    _currentUIColumn = chars[i];

			    PunchTarget(chars[i].transform, new Vector3(0, 3f, 0), PunchDuration);
			    
			    // if the current letter matches the position of the same letter in the solution
			    
			    if (LetterIsInRightPlace(i, currentKeyboardKey))
			    {
				    correctCount++;
				    Debug.Log(correctCount);
			    }

			    // if the current letter does not match the position of the same letter in the solution and does not exist in the solution

			    else if (LetterIsNotInSolution(i, currentKeyboardKey))
			    {
				    currentKeyboardKey.state = KeyboardKey.tileState.Incorrect;
				    _currentUIColumn.GetComponent<Char>().State = Char.TileState.Incorrect;

				    //Debug.Log("grey " + i);
			    }

			    /*else if (_answer[i].ToString() == _solution[i].ToString())
			    {
				    currentKeyboardKey.state = KeyboardKey.tileState.Correct;
				    
				    _currentUIColumn.GetComponent<Char>().State = Char.TileState.Correct;

				    _remaining = _remaining.Remove(i, 1);
				    _remaining = _remaining.Insert(i, " ");
				    
				    correctCount++;

				    //Debug.Log("green " + i);
			    }*/

			    // if the current letter does not match the position of the same letter in the solution and does not exist in the solution
			    
			    /*else if (!_solution.Contains(_answer[i].ToString()))
			    {
				    currentKeyboardKey.state = KeyboardKey.tileState.Incorrect;
				    _currentUIColumn.GetComponent<Char>().State = Char.TileState.Incorrect;
				    
				    //Debug.Log("grey " + i);
			    }*/
		    }

		    bool multipleCount = false;
		    
		    for (int j = 0; j < _answer.Length; j++)
		    {
			    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[j];
			    
			    //get keyboard letter to light up

			    GameObject currentKeyboardLetter = GetCurrentKeyboardLetter(_answer, j);
			    KeyboardKey currentKeyboardKey = currentKeyboardLetter.GetComponent<KeyboardKey>();

			    Char currentChar = _currentUIRow.GetComponentsInChildren<Char>()[j];

			    if (currentChar.State != Char.TileState.Correct && currentChar.State != Char.TileState.Incorrect)
			    {
				    if (!_remaining.Contains(_answer[j]))
				    {
					    currentChar.State = Char.TileState.Incorrect;

					    //Debug.Log("grey LAST " + j);
					    return;
				    }
				    
				    // if the solution only has one instance of the letter and the guess has two or more:
				    // mark the first as wrong place and the rest incorrect

				    if (_solutionLetterFrequencyDictionary[_answer[j].ToString()] < 2 && _guessLetterFrequencyDictionary[_answer[j].ToString()] > 1 && !multipleCount)
				    {
					    int index = _remaining.IndexOf(_answer[j]);
					    _remaining = _remaining.Remove(index, 1);
					    _remaining = _remaining.Insert(index, " ");
					    currentChar.State = Char.TileState.WrongPlace;
					    
					    //Debug.Log("yellow " + j);
					    
					    multipleCount = true;
				    }
				    else
				    {
					    if (_solutionLetterFrequencyDictionary[_answer[j].ToString()] >=
					             _guessLetterFrequencyDictionary[_answer[j].ToString()])
					    {
						    currentChar.State = Char.TileState.WrongPlace;
						    
						    //Debug.Log("YELLOW LAST " + j);
					    }
					    else
					    {
						    currentChar.State = Char.TileState.Incorrect;

						    //Debug.Log("grey LAST " + j);   
					    }
				    }
				    
				    // check how many of a letter are in the answer and guess that have been marked as correct

				    if (_guessLetterFrequencyDictionary[_answer[j].ToString()] > _solutionLetterFrequencyDictionary[_answer[j].ToString()])
				    {
					    for (int i = 0; i < _answer.Length; i++)
					    {
						    Char newCurrentCharI = _currentUIRow.GetComponentsInChildren<Char>()[i];
						    Char newCurrentCharJ = _currentUIRow.GetComponentsInChildren<Char>()[j];

						    if (newCurrentCharI.State == Char.TileState.Correct && _answer[i] == _answer[j])
						    {
							    // we already have an instance of that letter that is correct, so mark all others as incorrect
							    newCurrentCharJ.State = Char.TileState.Incorrect;
							    
							    //Debug.Log("There's already another instance of " + _answer[j] + " that is in the right place so all others are incorrect");
						    }
					    }
				    }

				    if (currentKeyboardKey.state != KeyboardKey.tileState.Correct)
				    {
					    currentKeyboardKey.state = KeyboardKey.tileState.WrongPlace;

					    //Debug.Log("Making keyboard " + _answer[j] + " YELLOW since it's not green");
				    }
				    
				    /*if (_remaining.Contains(_answer[j]))
				    {
					    // if the solution only has one instance of the letter and the guess has two or more:
					    // mark the first as wrong place and the rest incorrect

					    if (_solutionLetterFrequencyDictionary[_answer[j].ToString()] < 2 && _guessLetterFrequencyDictionary[_answer[j].ToString()] > 1 && !multipleCount)
					    {
						    int index = _remaining.IndexOf(_answer[j]);
						    _remaining = _remaining.Remove(index, 1);
						    _remaining = _remaining.Insert(index, " ");
						    currentChar.State = Char.TileState.WrongPlace;
						    
						    //Debug.Log("yellow " + j);
						    
						    multipleCount = true;
					    }
					    else
					    {
						    if (_solutionLetterFrequencyDictionary[_answer[j].ToString()] >=
						             _guessLetterFrequencyDictionary[_answer[j].ToString()])
						    {
							    currentChar.State = Char.TileState.WrongPlace;
							    
							    //Debug.Log("YELLOW LAST " + j);
						    }
						    else
						    {
							    currentChar.State = Char.TileState.Incorrect;

							    //Debug.Log("grey LAST " + j);   
						    }
					    }
					    
					    // check how many of a letter is in the answer and guess that have been marked as correct

					    if (_guessLetterFrequencyDictionary[_answer[j].ToString()] > _solutionLetterFrequencyDictionary[_answer[j].ToString()])
					    {
						    for (int i = 0; i < _answer.Length; i++)
						    {
							    Char newCurrentCharI = _currentUIRow.GetComponentsInChildren<Char>()[i];
							    Char newCurrentCharJ = _currentUIRow.GetComponentsInChildren<Char>()[j];

							    if (newCurrentCharI.State == Char.TileState.Correct && _answer[i] == _answer[j])
							    {
								    // we already have an instance of that letter that is correct, so mark all others as incorrect
								    newCurrentCharJ.State = Char.TileState.Incorrect;
								    
								    //Debug.Log("There's already another instance of " + _answer[j] + " that is in the right place so all others are incorrect");
							    }
						    }
					    }

					    if (currentKeyboardKey.state != KeyboardKey.tileState.Correct)
					    {
						    currentKeyboardKey.state = KeyboardKey.tileState.WrongPlace;

						    //Debug.Log("Making keyboard " + _answer[j] + " YELLOW since it's not green");
					    }*/
				    /*}
				    else
				    {
					    currentChar.State = Char.TileState.Incorrect;

					    //Debug.Log("grey LAST " + j);
				    }*/
			    }
		    }
	    }

	    //Debug.Log("correct letters: " + correctCount);
	    
	    CheckForWinOrLoss(correctCount);
    }
    
    private bool LetterIsInRightPlace(int i, KeyboardKey currentKeyboardKey)
    {
	    if (_answer[i].ToString() == _solution[i].ToString())
	    {
		    currentKeyboardKey.state = KeyboardKey.tileState.Correct;
				    
		    _currentUIColumn.GetComponent<Char>().State = Char.TileState.Correct;

		    _remaining = _remaining.Remove(i, 1);
		    _remaining = _remaining.Insert(i, " ");
		    
		    return true;

		    //Debug.Log("green " + i);
	    }

	    return false;
    }

    private bool LetterIsNotInSolution(int i, KeyboardKey currentKeyboardKey)
    {
	    return (!_solution.Contains(_answer[i].ToString()));
    }
    
    private void CheckForWinOrLoss(int correctCount)
    {
	    if (correctCount == 5)
	    {
		    //Debug.Log("Winner");
		    _winCanvas.SetActive(true);
		    _solutionTextWin.text = _solution.ToUpper();
		    _attemptsText.text = (_currentRound + 1) + " attempt(s)";
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
			    //Debug.Log("Loser");
			    _defeatCanvas.SetActive(true);
			    _solutionTextLose.text = _solution.ToUpper();
			    //show defeat canvas
		    }
	    }
    }

    GameObject GetCurrentKeyboardLetter(string word, int index)
    {
	    return InputManager.Keyboard.GetComponentsInChildren<KeyboardKey>().Where(x => x.Key == word[index].ToString()).ToList()[0].gameObject;
    }

    private void PunchTarget(Transform target, Vector3 force, float duration)
    {
	    target.DOPunchPosition(force, duration).OnComplete( () => target.DORewind());
    }
    
    private void ShakeTarget(Transform target, Vector3 force, float duration)
    {
	    target.DOShakePosition(duration, force, 10, 0f)
		    .SetRecyclable(true)
		    .SetAutoKill(false)
		    .OnComplete( () => target.DORewind());
    }

    void UpdateRound()
    {
	    _inputString = "";
	    _answer = "";
	    _wordLength = 0;
	    _currentChar = 0;
	    _currentUIRow = _uiRows[_currentRound];
	    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[_currentChar];
    }

    void AddWordsToHashSet(string path, HashSet<string> set)
    {
	    TextAsset file = Resources.Load<TextAsset>(path);

	    string fileContent = file.text;

#if UNITY_WEBGL

	    string[] fileWords = fileContent.Split("\r\n");
	    
#endif

#if !UNITY_WEBGL
	
		  string[] fileWords = fileContent.Split(Environment.NewLine);
	    
#endif

	    foreach (string word in fileWords)
	    {
		    set.Add(word.ToUpper());
	    }
    }

    void PickRandomWord()
    {
	    int num = UnityEngine.Random.Range(0, _possibleWordsHashSet.Count - 1);
	    _solution = _possibleWordsHashSet.ElementAt(num);
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
    
    [ContextMenu("Init Solution Frequency UI")]
    void InitSolutionFrequencyDictionary()
    {
	    _solutionLetterFrequencyDictionary.Clear();

	    foreach (string key in InputManager.KeyboardKeys)
	    {
		    _solutionLetterFrequencyDictionary.Add(key, 0);
	    }

	    foreach (KeyValuePair<string, int> pair in _solutionLetterFrequencyDictionary)
	    {
		    //Debug.Log("Letter: " + pair.Key + ", Frequency: " + pair.Value);
	    }

	    //Debug.Log("init solution frequency");
    }

    [ContextMenu("Update Solution Frequency UI")]
    void UpdateSolutionFrequencyDictionary()
    {
	    _solutionLetterFrequencyDictionary.Clear();

	    foreach (string key in InputManager.KeyboardKeys)
	    {
		    _solutionLetterFrequencyDictionary.Add(key, 0);
	    }

	    foreach (char letter in _solution)
	    {
		    //Debug.Log( _solutionLetterFrequencyDictionary[letter.ToString()]);
		    _solutionLetterFrequencyDictionary[letter.ToString()] += 1;
	    }

	    foreach (KeyValuePair<string, int> pair in _solutionLetterFrequencyDictionary)
	    {
		    //Debug.Log("Letter: " + pair.Key + ", Frequency: " + pair.Value);
	    }
	    
	    //Debug.Log("updated solution frequency");
    }
    
    void UpdateFrequencyDictionary(Dictionary<string, int> dictionary, string word)
    {
	    dictionary.Clear();

	    foreach (string key in InputManager.KeyboardKeys)
	    {
		    dictionary.Add(key, 0);
	    }

	    foreach (char letter in word)
	    {
		    //Debug.Log( dictionary[letter.ToString()]);
		    dictionary[letter.ToString()] += 1;
	    }

	    foreach (KeyValuePair<string, int> pair in dictionary)
	    {
		    //Debug.Log("Letter: " + pair.Key + ", Frequency: " + pair.Value);
	    }
	    
	    //Debug.Log("updated frequency of " + dictionary.GetType().GetProperty(name));
    }

    public void RestartGame()
    {
	    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
