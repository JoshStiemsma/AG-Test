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


public enum GameState
{
    Pregame,
    Starting,
    PlayerTurn,
    BotTurn,
    GameOver
}
public class GameManager : MonoBehaviour
{
    public GameState currentGameState = GameState.Pregame;
    public SceneObject Player;
    public SceneObject BuildObject;
    public GameObject SceneObjectPrefab;

    public float spawnSpeed = .1f;
     float lastSpawnTime = 0;


    public List<SceneObject> gameObjects = new List<SceneObject>();

    public GameObject StartCanvas;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI topText;
    public TextMeshProUGUI bottomText;

    public List<Sprite> ObjectSprites = new List<Sprite>();
    public GameObject gameBoard;

    public List<int> Turns = new List<int>();
    public List<int> playerTurns = new List<int>();

    public GameObject audioSourcePrefab;
    public List<AudioClip> ButtonSounds = new List<AudioClip>();
    List<AudioClip> CurrentSounds = new List<AudioClip>();
    public AudioClip SuccessSound;
    public AudioClip LoseSound;
    public AudioClip BeginSound;


    Camera cam;
    Color backColor = Color.white;
    Color newbackColor = Color.blue;


    int HighScore = 0;
    int currentScore = 0;
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

    public void OnButtonClick(int buttonIndex)
    {
        Debug.Log("OnButtonClick "+ buttonIndex);
        if (currentGameState != GameState.PlayerTurn) return;

        playerTurns.Add(buttonIndex);
        CheckLastTurn();
    }


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

    IEnumerator ActionAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
    public void PlayCurrentSoundForIndex(int index)
    {
        PlayGivenSound(CurrentSounds[index]);

    }
    public void PlayGivenSound(AudioClip clip, float volume = 1)
    {
        GameObject obj = Instantiate(audioSourcePrefab);
        AudioSource sc = obj.GetComponent<AudioSource>();
        sc.clip = clip;
        sc.volume = volume;
        sc.Play();
        Destroy(obj, sc.clip.length);

    }
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

    public void IncreaseSides()
    {
        BuildObject.IncreaseSides();
    }
    public void DecreaseSides()
    {
        BuildObject.DecreaseSides();

    }
    public void IncreaseTexture()
    {
        BuildObject.IncreaseTexture();
    }
    public void DecreaseTexture()
    {
        BuildObject.DecreaseTexture();

    }

    float colorChangetime=0;
    float colorChangespeed = 10;
    // Update is called once per frame
    void Update()
    {
        if(Time.time - colorChangetime > colorChangespeed)
        {
            colorChangetime = Time.time;
            backColor = newbackColor;
            newbackColor = (Random.ColorHSV(0f, 1f, 1f, 1f, 0.25f, .75f) + Color.white + Color.white) / 3f;
        }
      
        cam.backgroundColor = Color.Lerp(backColor, newbackColor, (Time.time - colorChangetime) / colorChangespeed); ;

    }

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
    IEnumerator CountDownRoutine(Action OnFinish)
    {
        Debug.Log("Start routine B ");
        float duration = 3;
        float time = 0;
        countdownText.gameObject.SetActive(true);
        while (time < duration-1)
        {

            time += 0.01f;
            countdownText.text = (duration - time).ToString("00");
            yield return new WaitForSeconds(0.01f);
        }



        countdownText.text = "Go!";
        while (time < duration+1)
        {
            time += 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
       // countdownText.gameObject.SetActive(false);
        OnFinish.Invoke();
    }
    public int FindAvailableSprite()
    {

        int index = Random.Range(0, ObjectSprites.Count);

        if (index == Player.TextureIndex) return FindAvailableSprite();

        foreach (SceneObject obj in gameObjects)
        {
         //   Debug.Log($"compre {index} and {obj.TextureIndex}");
            if (index == obj.TextureIndex) return FindAvailableSprite();
        }
        return index;
    }
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


    public void GameOver()
    {
        PlayGivenSound(LoseSound,.25f);
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
            ClearGame();

        }));
    }
    public void SetupGame()
    {

    }
    public void ClearGame()
    {
        Turns.Clear();
        topText.gameObject.SetActive(false);
        
    }




}
