using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using Screen = UnityEngine.Screen;

/// <summary>
/// Game Sate is the states the game baord can be in from start to each persons turn untill gameover
/// </summary>
public enum GameState
{
    Pregame,
    Starting,
    PlayerTurn,
    BotTurn,
    GameOver
}


/// <summary>
/// Game Manager handles all the games logic between buttons and game state
/// </summary>
public class GameManager : MonoBehaviour
{

    /// <summary>
    /// Current Game State is the state the game is currently in
    /// </summary>
    public GameState currentGameState = GameState.Pregame;
    /// <summary>
    /// Player is the in-game scene object that takes the shape the player build at the start
    /// </summary>
    public SceneObject Player;
    /// <summary>
    /// Build object is the scene object at the start that the user uses to build out one of the buttons
    /// </summary>
    public SceneObject BuildObject;

    /// <summary>
    /// gameObjects array contains the 4 buttons in the scene used for the game
    /// </summary>
    public List<SceneObject> gameObjects = new List<SceneObject>();

    /// <summary>
    /// Start Canvas is the canvas preset when the user is building their object and the game is not in play
    /// </summary>
    public GameObject StartCanvas;
    /// <summary>
    /// countdown text is the tmpro text used to countdown at the start of the game
    /// </summary>
    public TextMeshProUGUI countdownText;
    /// <summary>
    /// top Text is a text that is at the top of the screen during gameplay
    /// </summary>
    public TextMeshProUGUI topText;
    /// <summary>
    /// Bottom Text is the text at the bottom of the screen during gameplay
    /// </summary>
    public TextMeshProUGUI bottomText;

    /// <summary>
    /// Object Sprites ia  pool of usable sprites the user can cycle through when building 
    /// As well as the scene buttons select from here when picking thier textures
    /// </summary>
    public List<Sprite> ObjectSprites = new List<Sprite>();
    /// <summary>
    /// Game board is the main object the game board and everything included is contained under
    /// </summary>
    public GameObject gameBoard;

    /// <summary>
    /// Turns is the list of positions the game uses during gameplay
    /// every turn another int is added to turns
    /// </summary>
    public List<int> Turns = new List<int>();

    /// <summary>
    /// player turns are the current turns the player has played each round
    /// used for tracking if the player is correct or not in the sequence
    /// </summary>
    public List<int> playerTurns = new List<int>();

    /// <summary>
    /// Audio source prefab is the prefab spawned for each noise and contains an audio source
    /// </summary>
    public GameObject audioSourcePrefab;

    /// <summary>
    /// Button sounds is a pool of random sounds that the buttons pick from at start
    /// each round the sounds are different
    /// </summary>
    public List<AudioClip> ButtonSounds = new List<AudioClip>();
    /// <summary>
    /// Current sounds is the list of sound chosen for the given round
    /// </summary>
    List<AudioClip> CurrentSounds = new List<AudioClip>();

    /// <summary>
    /// Succes sound is the sound played when the round was completed succesfully
    /// </summary>
    public AudioClip SuccessSound;
    /// <summary>
    /// Loss sound is the sound played when the user loses the game
    /// </summary>
    public AudioClip LoseSound;
    /// <summary>
    /// Begin sound is the sound played at the start of the roun
    /// </summary>
    public AudioClip BeginSound;

    /// <summary>
    /// Cam is a copy of the main Camera
    /// </summary>
    Camera cam;

    /// <summary>
    /// Back Color is the current color of the background
    /// </summary>
    Color backColor = Color.white;
    /// <summary>
    /// New Back Color is the upcoming color of the background
    /// </summary>
    Color newbackColor = Color.blue;

    /// <summary>
    /// High score is the stored value of the highest score acieved this play session
    /// </summary>
    int HighScore = 0;
    /// <summary>
    /// Current score is the score for each gameplay in progress
    /// </summary>
    int currentScore = 0;

    /// <summary>
    /// Background color change time is the last time the background was changed
    /// </summary>
    float backgroundColorChangetime = 0;
    /// <summary>
    /// Background Color change speed is a time for how oftten the background cycles new colors
    /// </summary>
    float backgroundColorChangespeed = 10;



    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        countdownText.gameObject.SetActive(false);
        newbackColor = (Random.ColorHSV(0f, 1f, 1f, 1f, 0.25f, .75f) + Color.white + Color.white) / 3f;
        topText.gameObject.SetActive(false);
        topText.text = "";

        gameBoard.SetActive(false);

        for(int i =0;i< gameObjects.Count; i ++)
        {
            gameObjects[i].SceneButtonIndex = i;
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        UpdateBackground();
    }

    /// <summary>
    /// Update background slowly rotates through random colors so the background is slightly animated
    /// </summary>
    void UpdateBackground()
    {
        if (Time.time - backgroundColorChangetime > backgroundColorChangespeed)
        {
            backgroundColorChangetime = Time.time;
            backColor = newbackColor;
            newbackColor = (Random.ColorHSV(0f, 1f, 1f, 1f, 0.25f, .75f) + Color.white + Color.white) / 3f;
        }

        cam.backgroundColor = Color.Lerp(backColor, newbackColor, (Time.time - backgroundColorChangetime) / backgroundColorChangespeed); ;

    }

    #region Turn Handling

    /// <summary>
    /// Check last turn is called after a players turn and checks if the turn was correct
    /// if player is on last button in sequence then we switch to bot turn
    /// </summary>
    void CheckLastTurn()
    {

       // Debug.Log($"Compare Totals  player: {playerTurns.Count}    Total:{Turns.Count}");
        //Debug.Log($"Compare turns players Last: {playerTurns[playerTurns.Count - 1]}    Current:{Turns[playerTurns.Count - 1]}");

        if (playerTurns[playerTurns.Count-1] != Turns[playerTurns.Count-1]) GameOver();
        if (playerTurns.Count == Turns.Count)
        {
            PlayGivenSound(SuccessSound);
            currentScore++;
            bottomText.text = $"Score:{currentScore}";
            if (currentScore > HighScore) HighScore = currentScore;

            StartCoroutine(ActionAfterDelay(1,()=> UpdateTurn(GameState.BotTurn))) ;
           
        }
      

    }
    /// <summary>
    /// Update turn sets the current game state then handles changes based off of that
    /// </summary>
    /// <param name="inTurn">The new Game State</param>
    public void UpdateTurn(GameState inTurn)
    {

        currentGameState = inTurn;
        //topText.text = currentGameState.ToString();
        switch (currentGameState)
        {
            case GameState.Pregame:
                break;
            case GameState.Starting:
                break;
            case GameState.PlayerTurn:
                topText.text = "Your Turn";
                break;
            case GameState.BotTurn:
                topText.text = "Simon's Turn";

                Turns.Add(Random.Range(0, 3));
                StartCoroutine(PlayTurns());
                break;
            case GameState.GameOver:

                break;

        }
    }

    /// <summary>
    /// Play Turns is the routine of playing back the games current sequence
    /// Every Second we animate the next button on the list untill done
    /// then return to players turn
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayTurns()
    {
        bool done = false;
        int index = 0;
        while (!done)
        {
            if (index >= Turns.Count)
            {
                done = true;
                yield return new WaitForSeconds(.1f);
            }
            else
            {
                // Debug.Log($"Animating button {Turns[index]}  {Turns.Count} {index}");
                gameObjects[Turns[index]].ClickAnimate();
                index++;
                yield return new WaitForSeconds(1f);

            }

        }
        // Debug.Log("done Animating turns" );


        UpdateTurn(GameState.PlayerTurn);
        playerTurns = new List<int>();

    }

    /// <summary>
    /// Game over set the board into the game over game state
    /// Then it clears the board and returnt he user to the start builder scene
    /// </summary>
    public void GameOver()
    {
        PlayGivenSound(LoseSound, .25f);
        currentGameState = GameState.GameOver;
        StartCanvas.SetActive(true);
        topText.text = "Game Over";
        bottomText.text = $"High Score: {HighScore}";
        foreach (SceneObject obj in gameObjects)
        {
            obj.StartSwipe(true);
        }
        StartCoroutine(ActionAfterDelay(1, () =>
        {
            gameBoard.gameObject.SetActive(false);
            BuildObject.gameObject.SetActive(true);
            BuildObject.StartSwipe(false, null);

            Turns.Clear();
            topText.gameObject.SetActive(false);

        }));
    }

    #endregion


    #region Audio
    /// <summary>
    /// Relative to the 4 game buttons, play the sound of the button
    /// </summary>
    /// <param name="index">which button to play 1-4</param>
    public void PlayCurrentSoundForIndex(int index)
    {
        PlayGivenSound(CurrentSounds[index]);
    }

    /// <summary>
    /// Play audio clip passed in by spawning sudio prefab and passing in clip
    /// </summary>
    /// <param name="clip">The clip to play</param>
    /// <param name="volume">volume amount</param>
    public void PlayGivenSound(AudioClip clip, float volume = 1)
    {
        GameObject obj = Instantiate(audioSourcePrefab);
        AudioSource sc = obj.GetComponent<AudioSource>();
        sc.clip = clip;
        sc.volume = volume;
        sc.Play();
        Destroy(obj, sc.clip.length);

    }
    /// <summary>
    /// Setup rand sounds picks four sounds from our sound bank and relates them to one of the four button 
    /// so each round the sounds are different
    /// </summary>
    void SetupRandomSounds()
    {
        List<AudioClip> available = new List<AudioClip>(ButtonSounds);
        CurrentSounds = new List<AudioClip>();
        for (int i=0; i < 4; i++)
        {
            int index = Random.Range(0, available.Count);
            CurrentSounds.Add(available[index]);
            available.RemoveAt(index);
        }

    }

    #endregion

    #region Button Handling

    /// <summary>
    /// On Button click is called anytime an in game button is pressed, if in game state then it is added to the palyers turns and checked
    /// </summary>
    /// <param name="buttonIndex"></param>
    public void OnButtonClick(int buttonIndex)
    {
        if (currentGameState != GameState.PlayerTurn) return;

        playerTurns.Add(buttonIndex);
        CheckLastTurn();
    }


    /// <summary>
    /// UI Button handleing for increasing shape size that passes call to main build object
    /// </summary>
    public void IncreaseSides()
    {
        BuildObject.IncreaseSides();
    }

    /// <summary>
    /// UI Button handleing for decreasing shape size that passes call to main build object
    /// </summary>
    public void DecreaseSides()
    {
        BuildObject.DecreaseSides();

    }

    /// <summary>
    /// UI Button handleing for changing texture that passes call to main build object
    /// </summary>
    public void IncreaseTexture()
    {
        BuildObject.IncreaseTexture();
    }

    /// <summary>
    /// UI Button handleing for changing texture that passes call to main build object
    /// </summary>
    public void DecreaseTexture()
    {
        BuildObject.DecreaseTexture();
    }

    #endregion



 

    #region Board Setup
    /// <summary>
    /// Start Game kicks off the building of the game board and hiding of build object
    /// once everything is initialized for a new game the build object animates out and the boardd gets built
    /// </summary>
    public void StartGame()
    {
        currentGameState = GameState.Starting;
        PlayGivenSound(BeginSound, .5f);
        StartCanvas.SetActive(false);

        SetupRandomSounds();
        topText.gameObject.SetActive(true);
        topText.text = "";
        currentScore = 0;
        bottomText.text = $"Score: {currentScore}";


        BuildObject.StartSwipe(true, () =>
        {
            BuildObject.gameObject.SetActive(false);
            BuildBoard();
        });
    }

    /// <summary>
    /// Build board is called at the end of start game and sets all the buttons to new objects with new skins
    /// The users built options go to the nearest button and the rest are random and avoid the users options
    /// Start countdown
    /// </summary>
    void BuildBoard()
    {
        Player.TextureIndex = BuildObject.TextureIndex;
        Player.imageSprite.sprite = ObjectSprites[Player.TextureIndex];

        gameBoard.SetActive(true);

        Player.Rebuild(BuildObject.pointCount, BuildObject.CurrentColor);

        foreach (SceneObject obj in gameObjects)
        {
            if (obj == Player) break;
            obj.SetVisible(false);
            int index = FindAvailableSprite();
            obj.TextureIndex = index;
            obj.imageSprite.sprite = ObjectSprites[index];

            int pCount = FindAvailableShape();
            obj.pointCount = pCount;

            obj.Rebuild(obj.pointCount, obj.CurrentColor);

        }

        //Debug.Log("Start routine A");

        StartCoroutine(CountDownRoutine(() =>
        {
            countdownText.gameObject.SetActive(false);
            UpdateTurn(GameState.BotTurn);
        }));

    }

    /// <summary>
    /// Find available sprite is used when building the board so each button can grab a uniqie sprite
    /// it is recursive and attempts to grab a new sprite then check if any other has that sprite yet, if so we grab another
    /// </summary>
    /// <returns></returns>
    public int FindAvailableSprite()
    {

        int index = Random.Range(0, ObjectSprites.Count);

        if (index == Player.TextureIndex) return FindAvailableSprite();

        foreach (SceneObject obj in gameObjects)
        {
            if (index == obj.TextureIndex) return FindAvailableSprite();
        }
        return index;
    }

    /// <summary>
    /// Find available shape is used when building the board and its buttons, each button grabs a shape but keeps finding a 
    /// new shape until it has a unique one fromt he others
    /// </summary>
    /// <returns></returns>
    public int FindAvailableShape()
    {
        int index = Random.Range(3, 10);

        if (index == Player.pointCount) return FindAvailableShape();

        foreach (SceneObject obj in gameObjects)
        {
            if (index == obj.pointCount) return FindAvailableShape();
        }
        return index;
    }



    /// <summary>
    /// The countdown routine is the routine that counts from 3 to 0 then starts the game
    /// </summary>
    /// <param name="OnFinish">The action to call once the countdown is done, usually start game</param>
    /// <returns></returns>
    IEnumerator CountDownRoutine(Action OnFinish)
    {
        float duration = 4;
        float time = 0;
        countdownText.gameObject.SetActive(true);
        while (time < duration-1)
        {

            time += 1;
            countdownText.text = (duration - time).ToString("00");
            yield return new WaitForSeconds(1);
        }
        countdownText.text = "Go!";
         yield return new WaitForSeconds(1);
        
        OnFinish.Invoke();
    }

    #endregion
    


    /// <summary>
    /// Basic delayed routine is a utilitiy that calls action on end
    /// </summary>
    /// <param name="delay">Time to wait</param>
    /// <param name="action">action to call after delay</param>
    /// <returns></returns>
    IEnumerator ActionAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }



}
