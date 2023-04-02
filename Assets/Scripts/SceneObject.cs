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



    List<Vector3> verts3 = new List<Vector3>();
    List<int> tris3 = new List<int>();

    List<Vector2> verts2 = new List<Vector2>();
    List<ushort> tris2 = new List<ushort>();


    public Color CurrentColor = Color.white;


    public MeshCollider meshCollider;
    public InteractionHandler interactionHandler;


    public SpriteMask mask;
    public SpriteRenderer imageSprite;
    private Material mainSpriteMaterial;

    private Sprite currentSprite;

    public Animator anim;
    public int pointCount = 5;

    public GameManager gameManager;
    public int TextureIndex =0;
    public bool isPlayer = false;
    public bool isBuilder = false;
    public int SceneButtonIndex = 0;

    bool isVisible = false;

    private float FullSwipeSpeed = .5f;
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


        DrawMesh(verts3.ToArray(), tris3.ToArray(), CurrentColor);
        currentSprite = CreateSprite(verts2.ToArray(), tris2.ToArray(), CurrentColor);
       
        imageSprite.color = CurrentColor;

        mask.sprite = currentSprite;
    }

    public void Initiate(GameManager _gameMananger, bool isPlayer =false)
    {
        gameManager = _gameMananger;

        if (!isPlayer)
        {
            Setup(Random.Range(3,10), Color.white);
            this.transform.localScale = Vector3.one / 4f;

        }

    }
    public void Rebuild(int _pointCount, Color clr)
    {
        pointCount = _pointCount;
        CurrentColor = clr;
        verts3.Clear();
        tris3.Clear();

        verts2.Clear();
        tris2.Clear();

        BuildShapeV3(ref verts3, ref tris3);

        BuildShapeV2(ref verts2, ref tris2);

        DrawMesh(verts3.ToArray(), tris3.ToArray(), CurrentColor);

        currentSprite = CreateSprite(verts2.ToArray(), tris2.ToArray(), CurrentColor);
       

        if(isVisible)
         StartAnimate(OnChange);
        else
        {
            OnChange();
            StartCoroutine(SwipeIn(FullSwipeSpeed/2f));

        }

        void OnChange()
        {
            mask.sprite = currentSprite;
            imageSprite.color = CurrentColor;
        }

    }
    public void SetVisible(bool isVis)
    {
        isVisible = isVis;
        mainSpriteMaterial.SetFloat("_SwipeAmount", isVis ? 0:1) ;
        mainSpriteMaterial.SetInteger("_InvertSwipe", 1);
    }

    public void ClickAnimate()
    {
        //Debug.Log("Animate click");
        anim.Play("Base Layer.PopAnimation");
        if (isBuilder)
            gameManager.PlayGivenSound(gameManager.ButtonSounds[Random.Range(0, gameManager.ButtonSounds.Count)]);
        else
            gameManager.PlayCurrentSoundForIndex(SceneButtonIndex);
    }
    void StartAnimate( Action OnChange=null,Action OnComplete = null)
    {
        //Debug.Log("StartAnimate");

        StartCoroutine(AnimationRoutine(FullSwipeSpeed, OnChange, OnComplete));
    }
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


    public void StartSwipe(bool isSwipeOut, Action OnComplete = null)
    {
        if (isSwipeOut) StartCoroutine(SwipeOut(.5f, OnComplete));
        else StartCoroutine(SwipeIn(FullSwipeSpeed/2f, OnComplete));
    }
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

    void Handleclick()
    {

        Debug.Log("Handleclick");
        if (isBuilder)
        {
            CurrentColor = (Random.ColorHSV(0f, 1f, 1f, 1f, 0.25f, .75f) +Color.white)/2f;
            imageSprite.color = CurrentColor;
            ClickAnimate();

        }

        if (!isBuilder && gameManager != null && gameManager.currentGameState  == GameState.PlayerTurn)
        {
            gameManager.OnButtonClick(SceneButtonIndex);
            ClickAnimate();
        }

    }


    bool inChangeState = false;
    IEnumerator ActionAfterDelay(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }

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
        // Debug.Log("IncreaseTexture " + TextureIndex);

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

    public float ObjectSize = 5;
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
    void DrawMesh(Vector3[] vertices, int[] triangles, Color color)
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        meshCollider.sharedMesh = mesh;
    }


    Sprite CreateSprite(Vector2[] vertices, ushort[] triangles, Color color)
    {
        Sprite sp = Sprite.Create(new Texture2D(1024, 1024), new Rect(0.0f, 0.0f, 1024, 1024), Vector3.zero, 1);

        sp.OverrideGeometry(vertices, triangles);
        return sp;

    }
}
