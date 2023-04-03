using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class SceneObject : MonoBehaviour
{
    /// <summary>
    /// Game Manager is our stored manager that conrols the game
    /// </summary>
    public GameManager gameManager;

    /// <summary>
    /// verts3 is a list of vector 3 positions that make up the verticies of the shape
    /// </summary>
    List<Vector3> verts3 = new List<Vector3>();
    /// <summary>
    /// Tris3 is a list of verticie indexes in int form
    /// </summary>
    List<int> tris3 = new List<int>();
    /// <summary>
    /// Verts 2 is a list of Vector2 positions that make up the shape
    /// </summary>
    List<Vector2> verts2 = new List<Vector2>();
    /// <summary>
    /// Tris 2 is a list of ushorts of the indicis for the shape
    /// </summary>
    List<ushort> tris2 = new List<ushort>();

    /// <summary>
    /// Current Color is the color of the shape to be used
    /// </summary>
    public Color CurrentColor = Color.white;
    /// <summary>
    /// Mesh collider is the collider used for onmouse down that takes the mesh we build
    /// </summary>
    public MeshCollider meshCollider;
    /// <summary>
    /// interaction handler is a component that passes up On Mouse down when used on the collider
    /// </summary>
    public InteractionHandler interactionHandler;

    /// <summary>
    /// mask is the sprite mask used to mask our object with the sprite we build
    /// </summary>
    public SpriteMask mask;
    /// <summary>
    /// image sprite is the texture sprite for our object
    /// </summary>
    public SpriteRenderer imageSprite;
    /// <summary>
    /// main sprite material is the material grabbed off our sprite that we animate
    /// </summary>
    private Material mainSpriteMaterial;

    /// <summary>
    /// current sprite is the stored sprite that we made with our verts and tris
    /// </summary>
    private Sprite currentSprite;

    /// <summary>
    /// Anim is the animator tied to this object we can call animation clips on
    /// </summary>
    public Animator anim;

    /// <summary>
    /// point count is the value that represents how many points our shape will have
    /// </summary>
    public int pointCount = 5;
    /// <summary>
    /// Obejct size sets the scaled width of the new shape
    /// </summary>
    public float ObjectSize = 5;

    /// <summary>
    /// Texture index is the int value storing which texture were using from the pool on the game manager
    /// </summary>
    public int TextureIndex =0;
    /// <summary>
    /// is player is a bool set to track if this button is the players custom button 
    /// </summary>
    public bool isPlayer = false;
    /// <summary>
    /// is builder is the bool used to track if this scene object is the main building object for the user
    /// </summary>
    public bool isBuilder = false;

    /// <summary>
    /// Scene button index is the stored value 0-3 telling which button of the four this one is
    /// </summary>
    public int SceneButtonIndex = 0;

    /// <summary>
    /// is visible stores the bool for if this object is currently visiable or not
    /// used for animating In or Out
    /// </summary>
    bool isVisible = false;

    /// <summary>
    /// Full swipe speed is the time it takes to swipe and object out and then back in with a change
    /// </summary>
    private float FullSwipeSpeed = .5f;

    /// <summary>
    /// Last Click time is used to detect double clicks by comparing the last time a button was clicked with the current time at click
    /// </summary>
    private float LastClickTime = 0;

    /// <summary>
    /// In change state is a bool tracking if the object is currently being animated or not
    /// </summary>
    bool inChangeState = false;


    private void Awake()
    {
        mainSpriteMaterial = imageSprite.material;

    }
    void Start()
    {
        mainSpriteMaterial.SetFloat("_SwipeAmount", 0);

        if (!isPlayer)
        {
            Setup(Random.Range(3, 10), Color.white);
        }
        interactionHandler.OnClick += Handleclick;
    }


    #region Animations
    /// <summary>
    /// Set Visiable uses a bool to set the object visiable of not via the shader
    /// this sets objects up to be swiped in or out on scene change
    /// </summary>
    /// <param name="isVis">the bool stating if the object should be visible or not</param>
    public void SetVisible(bool isVis)
    {
        isVisible = isVis;
        mainSpriteMaterial.SetFloat("_SwipeAmount", isVis ? 0:1) ;
        mainSpriteMaterial.SetInteger("_InvertSwipe", 1);
    }

    /// <summary>
    /// Click animate is called when the object is clicked on
    /// Pop Animation is played on the animator
    /// an audio sound is played
    /// </summary>
    public void ClickAnimate()
    {
        anim.Play("Base Layer.PopAnimation");
        if (isBuilder)
            gameManager.PlayGivenSound(gameManager.ButtonSounds[Random.Range(0, gameManager.ButtonSounds.Count)]);
        else
            gameManager.PlayCurrentSoundForIndex(SceneButtonIndex);
    }

    /// <summary>
    /// Start animate si used when animating a full swipe of an object
    /// </summary>
    /// <param name="OnChange"> The action to be called wheen the object is not visiable during the switch</param>
    /// <param name="OnComplete">the end action to be called once done animating</param>
    void StartAnimate( Action OnChange=null,Action OnComplete = null)
    {
        StartCoroutine(AnimationRoutine(FullSwipeSpeed, OnChange, OnComplete));
    }

    /// <summary>
    /// Animtion routine is the timed process of hiding an object and then returning it with new changes
    /// </summary>
    /// <param name="duration">length of animation</param>
    /// <param name="OnChange">Action to be called mid change</param>
    /// <param name="OnComplete">Action to be called once animation is done</param>
    /// <returns></returns>
    IEnumerator AnimationRoutine(float duration, Action OnChange, Action OnComplete)
    {
        float time = 0;
        float halfTime = duration / 2f;
        mainSpriteMaterial.SetFloat("_SwipeAmount", 0);
        mainSpriteMaterial.SetInteger("_InvertSwipe", 0);
        float normalTime = 0;
        while (time < halfTime)
        {

            time += 0.01f;
            normalTime = (time / (duration / 2f));
            mainSpriteMaterial.SetFloat("_SwipeAmount", normalTime);
            yield return new WaitForSeconds(0.01f);

        }
       if(OnChange!=null) OnChange.Invoke();
       
        mainSpriteMaterial.SetInteger("_InvertSwipe", 1);

        while (time < duration)
        {

            time += 0.01f;
            normalTime = ((time - halfTime) / (duration - halfTime));
            mainSpriteMaterial.SetFloat("_SwipeAmount", normalTime);


            yield return new WaitForSeconds(0.01f);

        }
        mainSpriteMaterial.SetFloat("_SwipeAmount", 0);
        mainSpriteMaterial.SetInteger("_InvertSwipe", 0);
        if(OnComplete !=null) OnComplete.Invoke();
    }

    /// <summary>
    /// Start Swipe is used when revealing or hiding objects
    /// </summary>
    /// <param name="isSwipeOut">bool used to tell if we are revealing or hiding an object</param>
    /// <param name="OnComplete">Action to be called once completed</param>
    public void StartSwipe(bool isSwipeOut, Action OnComplete = null)
    {
        if (isSwipeOut) StartCoroutine(SwipeOut(.5f, OnComplete));
        else StartCoroutine(SwipeIn(FullSwipeSpeed/2f, OnComplete));
    }
    /// <summary>
    /// Swipe out is the timed routine usd for hding a visible object
    /// </summary>
    /// <param name="duration">Leangth of time routine takes</param>
    /// <param name="OnComplete">Action to be called when done animating</param>
    /// <returns></returns>
    IEnumerator SwipeOut(float duration, Action OnComplete = null)
    {
        float time = 0;
        mainSpriteMaterial.SetFloat("_SwipeAmount", 0);
        mainSpriteMaterial.SetInteger("_InvertSwipe", 0);
        while (time < duration)
        {
            time += 0.01f;
            mainSpriteMaterial.SetFloat("_SwipeAmount", time / duration);
            yield return new WaitForSeconds(0.01f);
        }

        if (OnComplete != null) OnComplete();
    }

    /// <summary>
    /// Swipe in is the timed routine used for revealing a hidden object
    /// </summary>
    /// <param name="duration">Leangth of time routine takes</param>
    /// <param name="OnComplete">Action to be called when done animating</param>
    /// <returns></returns>
    IEnumerator SwipeIn(float duration, Action OnComplete = null)
    {
        float time = 0;
        mainSpriteMaterial.SetFloat("_SwipeAmount", 0);
        mainSpriteMaterial.SetInteger("_InvertSwipe", 1);
        while (time < duration)
        {
            time += 0.01f;
            mainSpriteMaterial.SetFloat("_SwipeAmount",( time / duration));
            yield return new WaitForSeconds(0.01f);
        }
        mainSpriteMaterial.SetFloat("_SwipeAmount", 0);
        mainSpriteMaterial.SetInteger("_InvertSwipe", 0);
        if (OnComplete != null) OnComplete();

    }
    #endregion
    #region Inputs
    /// <summary>
    /// Handle click is called any time the mouse down happens on this object
    /// if double clicked on the builder obj then we change it color
    /// if in game and player turn then we add a turn for the player
    /// </summary>
    void Handleclick()
    {
        if (isBuilder)
        {
            if (Time.time - LastClickTime < .25f)
            {
                CurrentColor = (Random.ColorHSV(0f, 1f, 1f, 1f, 0.25f, .75f) + Color.white) / 2f;
                imageSprite.color = CurrentColor;
                ClickAnimate();
            }
            LastClickTime = Time.time;
        }

        if (!isBuilder && gameManager != null && gameManager.currentGameState  == GameState.PlayerTurn)
        {
            gameManager.OnButtonClick(SceneButtonIndex);
            ClickAnimate();
        }

    }


    /// <summary>
    /// Increase sides add 1 to the point count of the shape and rebuilds 
    /// Animate the change
    /// </summary>
    public void IncreaseSides()
    {
        if (inChangeState) return;
        inChangeState = true;

        pointCount++;
        Rebuild(pointCount, CurrentColor);


        StartCoroutine(ActionAfterDelay(FullSwipeSpeed, () =>
        {
            inChangeState = false;
        }));
    }
    /// <summary>
    /// DecreaseSides sides removes 1 to the point count of the shape and rebuilds 
    /// Animate the change
    /// </summary>
    public void DecreaseSides()
    {

        if (inChangeState) return;
        inChangeState = true;

        if (pointCount > 3)
        {
            pointCount--;
            Rebuild(pointCount, CurrentColor);
        }

        StartCoroutine(ActionAfterDelay(FullSwipeSpeed, () =>
        {
            inChangeState = false;
        }));
    }

    /// <summary>
    /// Increase Texture selectes the next textrure in the pool and applies it to this object
    /// Animates
    /// </summary>
    public void IncreaseTexture()
    {
        if (inChangeState) return;
        inChangeState = true;

        if (TextureIndex < gameManager.ObjectSprites.Count()-1)
            TextureIndex++;
        else
        {
            TextureIndex = 0;
        }
        StartAnimate(OnChange, OnComplete);


        void OnChange()
        {
            imageSprite.sprite = gameManager.ObjectSprites[TextureIndex];
        }
        void OnComplete()
        {
            inChangeState = false;
        }
    }
    /// <summary>
    /// Decrease Texture selectes the previouse textrure in the pool and applies it to this object
    /// Animates
    /// </summary>
    public void DecreaseTexture()
    {
        if (inChangeState) return;
        inChangeState = true;
        if (TextureIndex > 0)
            TextureIndex--;
        else
        {
            TextureIndex = gameManager.ObjectSprites.Count() - 1;
        }


        StartAnimate(OnChange, OnComplete);


        void OnChange()
        {
            imageSprite.sprite = gameManager.ObjectSprites[TextureIndex];
        }
        void OnComplete()
        {
            inChangeState = false;
        }
    }
    #endregion


    #region Building
    /// <summary>
    /// Setup builds the object shape and sprites
    /// </summary>
    /// <param name="_pointCount">Amount of points on the new shape</param>
    /// <param name="clr">The color of the shape</param>
    public void Setup(int _pointCount, Color clr)
    {
        pointCount = _pointCount;
        CurrentColor = clr;
        verts3.Clear();
        tris3.Clear();
        BuildShapeV3(ref verts3, ref tris3);
        verts2.Clear();
        tris2.Clear();
        BuildShapeV2(ref verts2, ref tris2);


        DrawMesh(verts3.ToArray(), tris3.ToArray());
        currentSprite = CreateSprite(verts2.ToArray(), tris2.ToArray());

        imageSprite.color = CurrentColor;

        mask.sprite = currentSprite;
    }

    /// <summary>
    /// Rebuild builds the sprite with shape and color 
    /// but it then animates the object to show the change
    /// </summary>
    /// <param name="_pointCount"></param>
    /// <param name="clr"></param>
    public void Rebuild(int _pointCount, Color clr)
    {
        Setup(_pointCount, clr);

        if (isVisible)
            StartAnimate(OnChange);
        else
        {
            OnChange();
            StartCoroutine(SwipeIn(FullSwipeSpeed / 2f));

        }

        void OnChange()
        {
            mask.sprite = currentSprite;
            imageSprite.color = CurrentColor;
        }

    }

    /// <summary>
    /// Build Shape v3 builds a vector 3 list of points with the PointCount var
    /// </summary>
    /// <param name="verts">referenced list of vector 3 positions that gets set to the new shapes points</param>
    /// <param name="tris">referenced list of ints for the indices of the new shape</param>
    void BuildShapeV3(ref List<Vector3> verts, ref List<int> tris)
    {
        verts.Add(new Vector3(0, 0, 0));


        for (int i = 0; i < pointCount; i++)
        {
            float theta = Mathf.Deg2Rad * ((float)i / (float)(pointCount)) * 360f;
            float x = ObjectSize * Mathf.Sin(theta);
            float y = ObjectSize * Mathf.Cos(theta);

            verts.Add(new Vector3(x, y, 0));

            if (i < pointCount - 1)
            {
                tris.Add(0);
                tris.Add((i + 1));
                tris.Add((i + 2));
            }
            else// if (i == pointCount - 2)
            {
                tris.Add(0);
                tris.Add((i + 1));
                tris.Add((1));
            }
        }


        float lx = Mathf.Infinity, ly = Mathf.Infinity;
        foreach (Vector3 vi in verts)
        {
            if (vi.x < lx)
                lx = vi.x;
            if (vi.y < ly)
                ly = vi.y;
        }
        Vector3[] localv = new Vector3[verts.Count];
        for (int i = 0; i < verts.Count; i++)
        {
            localv[i] = verts[i] - new Vector3(lx, ly, 0);
        }

        verts = localv.ToList();
    }

    /// <summary>
    /// Build Shape V2 builds a shape using pointCount to dicate the shape and sets the refereence lists to the new points
    /// </summary>
    /// <param name="verts">referenced list of vector 2 positions that gets set to the new shapes points</param>
    /// <param name="tris">referenced list of ushorts for the nindices of the new shape</param>
    void BuildShapeV2(ref List<Vector2> verts, ref List<ushort> tris)
    {
        verts.Add(new Vector3(0, 0, 0));

        for (int i = 0; i < pointCount; i++)
        {
            float theta = Mathf.Deg2Rad * ((float)i / (float)(pointCount)) * 360f;
            float x = ObjectSize * 2f * Mathf.Sin(theta);
            float y = ObjectSize * 2f * Mathf.Cos(theta);

            verts.Add(new Vector3(x, y, 0));


            if (i < pointCount - 1)
            {
                tris.Add(0);
                tris.Add((ushort)(i + 1));
                tris.Add((ushort)(i + 2));
            }
            else// if (i == pointCount - 2)
            {
                tris.Add(0);
                tris.Add((ushort)(i + 1));
                tris.Add((1));
            }
        }

        float lx = Mathf.Infinity, ly = Mathf.Infinity;
        foreach (Vector2 vi in verts)
        {
            if (vi.x < lx)
                lx = vi.x;
            if (vi.y < ly)
                ly = vi.y;
        }
        Vector2[] localv = new Vector2[verts.Count];
        for (int i = 0; i < verts.Count; i++)
        {
            localv[i] = verts[i] - new Vector2(lx, ly);
        }

        verts = localv.ToList();

    }

    /// <summary>
    /// Draw a mesh with the passed in point
    /// </summary>
    /// <param name="vertices">Verticies for new Mesh</param>
    /// <param name="triangles">Triangle indices for new Mesh</param>
    void DrawMesh(Vector3[] vertices, int[] triangles)
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        meshCollider.sharedMesh = mesh;
    }

    /// <summary>
    /// Given verts, tris and a color, return a sprite 
    /// </summary>
    /// <param name="vertices">Verticies for new sprite</param>
    /// <param name="triangles">Triangle indices for new sprite</param>
    /// <returns></returns>
    Sprite CreateSprite(Vector2[] vertices, ushort[] triangles)
    {
        Sprite sp = Sprite.Create(new Texture2D(1024, 1024), new Rect(0.0f, 0.0f, 1024, 1024), Vector3.zero, 1);

        sp.OverrideGeometry(vertices, triangles);
        return sp;

    }

    #endregion

    #region Util
    /// <summary>
    /// Action after delay is a utility to simple call an action after a delay
    /// </summary>
    /// <param name="delay">time to delay befor action</param>
    /// <param name="action">Action to call</param>
    /// <returns></returns>
    IEnumerator ActionAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
    #endregion
}
