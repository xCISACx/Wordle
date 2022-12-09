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
	public string Solution => _solution;

	public int RequiredAnswerLength => _requiredAnswerLength;
	private int _requiredAnswerLength = 5;
	
	public HashSet<string> PossibleWordsHashSet => _possibleWordsHashSet;
	private HashSet<string> _possibleWordsHashSet = new HashSet<string>();
	
	public HashSet<string> AllowedWordsHashSet => _allowedWordsHashSet;
	private HashSet<string> _allowedWordsHashSet = new HashSet<string>();
	
	public CharContainer CurrentUIRow
	{
		get
		{
			return _currentUIRow;
		}

		set => _currentUIRow = value;
	}
	
	[SerializeField] private CharContainer _currentUIRow;
	
	
	public Char CurrentUIColumn
	{
		get
		{
			return _currentUIColumn;
		}

		set => _currentUIColumn = value;
	}
	
	[SerializeField] private Char _currentUIColumn;
    
	public List<CharContainer> UIRows => _uiRows;
	[SerializeField] private List<CharContainer> _uiRows;
	
	public List<Char> UIColumns => _uiColumns;
	[SerializeField] private List<Char> _uiColumns;
	public string Answer
	{
		get
		{
			return _answer;
		}

		set => _answer = value;
	}
	
	[SerializeField] private string _answer;
	
	public int AnswerLength
	{
		get
		{
			return _answerLength;
		}

		set => _answerLength = value;
	}

	[SerializeField] private int _answerLength;
	
	public int CurrentChar
	{
		get
		{
			return _currentChar;
		}

		set => _currentChar = value;
	}
	
	[SerializeField] private int _currentChar;
	
	public int CurrentRound => _currentRound;
	[SerializeField] private int _currentRound;
	
	[SerializeField] private int _maxRounds = 5;
	
	[SerializeField] private string _remainingLetters;
    private readonly Dictionary<string, int> _solutionLetterFrequencyDictionary = new Dictionary<string, int>();
    private readonly Dictionary<string, int> _guessLetterFrequencyDictionary = new Dictionary<string, int>();

    [Header("Game Over Variables")]

    [SerializeField] private GameObject _defeatCanvas; 
    [SerializeField] private TMP_Text _solutionTextWin;
    [SerializeField] private TMP_Text _solutionTextLose;
    
    [SerializeField] private GameObject _winCanvas; 
    [SerializeField] private TMP_Text _attemptsText;

    
    private void Awake()
    {
	    InputManager = GetComponent<InputManager>();
	    
	    var seed = DateTime.Now.Ticks;
	    
	    UnityEngine.Random.InitState((int) seed);
	    
	    InitialiseValues();
	    UpdateRound();

	    AddWordsToHashSet("AllowedWords", _allowedWordsHashSet);
	    
	    AddWordsToHashSet("PossibleWords", _possibleWordsHashSet);
	    
	    Debug.Log(PossibleWordsHashSet.Count);
	    Debug.Log(AllowedWordsHashSet.Count);
	    
	    _uiRows = GetComponentsInChildren<CharContainer>().ToList();
	    _uiColumns = GetComponentsInChildren<Char>().ToList();
	    
	    _currentUIRow = _uiRows[_currentRound];
	    _currentUIColumn = _uiColumns[_currentChar];
	    
	    PickRandomWord();

	    InputManager.SetKeyBoardKeys();
	    
		UpdateFrequencyDictionary(_guessLetterFrequencyDictionary, _answer);
	    UpdateFrequencyDictionary(_solutionLetterFrequencyDictionary, _solution);
    }

    private void InitialiseValues()
    {
	    _answerLength = 0;
		_currentRound = 0;
		_currentChar = 0;
    }
    
    public void UpdateRound()
    {
	    InputManager.InputString = "";
	    _answer = "";
	    AnswerLength = 0;
	    _currentChar = 0;
	    _currentUIRow = _uiRows[_currentRound];
	    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[_currentChar];
    }

    public void SetTileColours(Char[] tiles, Color color)
    {
	    for (int i = 0; i < 5; i++)
	    {
		    var currentPanel = tiles[i].gameObject;
		    var currentPanelImage = currentPanel.GetComponentInChildren<Image>();
			    
		    currentPanelImage.color = color;
		    //Debug.Log("reset tile " + i + "'s colour");
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

	    if (_answer.Length != _requiredAnswerLength)
	    {
		    return;
	    }

	    _remainingLetters = _answer;
		    
	    // first pass to check for correct and incorrect letters 

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
	    }
	    
	    // second pass to check for wrong place letters and edge cases

	    bool multipleCount = false;
	    
	    for (int j = 0; j < _answer.Length; j++)
	    {
		    _currentUIColumn = _currentUIRow.GetComponentsInChildren<Char>()[j];
		    
		    //get keyboard letter to light up

		    GameObject currentKeyboardLetter = GetCurrentKeyboardLetter(Answer, j);
		    KeyboardKey currentKeyboardKey = currentKeyboardLetter.GetComponent<KeyboardKey>();

		    Char currentChar = CurrentUIRow.GetComponentsInChildren<Char>()[j];

		    if (currentChar.State != Char.TileState.Correct && currentChar.State != Char.TileState.Incorrect)
		    {
			    // if the remaining letters don't contain the current answer letter being checked, mark the tile as incorrect
			    
			    if (!_remainingLetters.Contains(Answer[j]))
			    {
				    currentChar.State = Char.TileState.Incorrect;

				    //Debug.Log("grey LAST " + j);
				    return;
			    }
			    
			    // if the solution only has one instance of the letter and the guess has two or more,
			    // mark the first as wrong place and the rest incorrect so the player won't wrongly
			    // think there are 2 of the same letter in the solution

			    if (_solutionLetterFrequencyDictionary[Answer[j].ToString()] < 2 && _guessLetterFrequencyDictionary[_answer[j].ToString()] > 1 && !multipleCount)
			    {
				    int index = _remainingLetters.IndexOf(Answer[j]);
				    
				    _remainingLetters = _remainingLetters.Remove(index, 1);
				    _remainingLetters = _remainingLetters.Insert(index, " ");
				    
				    currentChar.State = Char.TileState.WrongPlace;
				    
				    //Debug.Log("yellow " + j);
				    
				    multipleCount = true;
			    }
			    else
			    {
				    // if the solution has the same or higher frequency of the letter as the guess, mark the tile as wrong place
				    
				    if (_solutionLetterFrequencyDictionary[Answer[j].ToString()] >= _guessLetterFrequencyDictionary[Answer[j].ToString()])
				    {
					    currentChar.State = Char.TileState.WrongPlace;
					    
					    //Debug.Log("YELLOW LAST " + j);
				    }
				    
				    // otherwise, mark the tile as incorrect as the guessed letter is not in the solution
				    
				    else
				    {
					    currentChar.State = Char.TileState.Incorrect;

					    //Debug.Log("grey LAST " + j);   
				    }
			    }
			    
			    // if the guess letter frequency is higher than the solution frequency
			    // check how many of a letter are in the answer and guess that have been marked as correct
			    // this way we can mark the rest as incorrect since only one instance of that letter is in the solution

			    if (_guessLetterFrequencyDictionary[Answer[j].ToString()] > _solutionLetterFrequencyDictionary[Answer[j].ToString()])
			    {
				    for (int i = 0; i < Answer.Length; i++)
				    {
					    Char[] uiColumns = CurrentUIRow.GetComponentsInChildren<Char>();
					    
					    Char newCurrentCharI = uiColumns[i];
					    Char newCurrentCharJ = uiColumns[j];
					    
					    // if the first pass's answer has a correct letter already and the letter is the same for both passes

					    if (newCurrentCharI.State == Char.TileState.Correct && Answer[i] == Answer[j])
					    {
						    // we already have an instance of that letter that is correct, so mark all others as incorrect
						    
						    newCurrentCharJ.State = Char.TileState.Incorrect;
						    
						    //Debug.Log("There's already another instance of " + _answer[j] + " that is in the right place so all others are incorrect");
					    }
				    }
			    }
			    
			    // if the corresponding keyboard key wasn't previously marked as correct, mark it as wrong place

			    if (currentKeyboardKey.state != KeyboardKey.tileState.Correct)
			    {
				    currentKeyboardKey.state = KeyboardKey.tileState.WrongPlace;

				    //Debug.Log("Making keyboard " + _answer[j] + " YELLOW since it's not green");
			    }
		    }
	    }

	    //Debug.Log("correct letters: " + correctCount);
	    
	    CheckForWinOrLoss(correctCount);
    }
    
    private bool LetterIsInRightPlace(int i, KeyboardKey currentKeyboardKey)
    {
	    if (Answer[i].ToString() == _solution[i].ToString())
	    {
		    currentKeyboardKey.state = KeyboardKey.tileState.Correct;
				    
		    CurrentUIColumn.GetComponent<Char>().State = Char.TileState.Correct;

		    _remainingLetters = _remainingLetters.Remove(i, 1);
		    _remainingLetters = _remainingLetters.Insert(i, " ");
		    
		    return true;

		    //Debug.Log("green " + i);
	    }

	    return false;
    }

    private bool LetterIsNotInSolution(int i, KeyboardKey currentKeyboardKey)
    {
	    return (!_solution.Contains(Answer[i].ToString()));
    }

    private void CheckForWinOrLoss(int correctCount)
    {
	    if (correctCount == _requiredAnswerLength)
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

    public void PunchTarget(Transform target, Vector3 force, float duration)
    {
	    target.DOPunchPosition(force, duration).OnComplete( () => target.DORewind());
    }
    
    public void ShakeTarget(Transform target, Vector3 force, float duration)
    {
	    target.DOShakePosition(duration, force, 10, 0f)
		    .SetRecyclable(true)
		    .SetAutoKill(false)
		    .OnComplete( () => target.DORewind());
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
