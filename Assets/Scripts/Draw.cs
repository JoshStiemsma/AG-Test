using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Draw : MonoBehaviour
{
    public GameObject polygon;
    public SpriteRenderer sr;
    public GameObject node;

    public int pointCount = 5;


    public Button Increase;
    public Button Decrease;
    public Button Polybutton;


    List<Vector3> verts3 = new List<Vector3>();
    List<int> tris3 = new List<int>();

    List<Vector2> verts2 = new List<Vector2>();
    List<ushort> tris2 = new List<ushort>();


    Color CurrentColor = Color.red;

    public MeshCollider meshCollider;
    public InteractionHandler interactionHandler;


    public SpriteMask mask;
    public SpriteRenderer imageSprite;
    private Material mainSpriteMaterial;



    private Sprite currentSprite;

    public Animator anim;
    // public an
    void Start()
    {
        mainSpriteMaterial = imageSprite.material;
        mainSpriteMaterial.SetFloat("_SwipeAmount", 0);

        interactionHandler.OnClick += (Handleclick);

        Increase.onClick.AddListener(IncreaseSides);
        Decrease.onClick.AddListener(DecreaseSides);

        Initiate();
    }

    private void Update()
    {
        
    }


    void StartAnimate()
    {
        Debug.Log("StartAnimate");

         anim.Play("Base Layer.PopAnimation 1");
        StartCoroutine(AnimationRoutine(1));
    }
    IEnumerator AnimationRoutine(float duration)
    {
        float time = 0;
        float halfTime = duration / 2f;
      //  loadInMaterial.SetFloat("_SwipeAmount", 0);
        mainSpriteMaterial.SetFloat("_SwipeAmount", 0);
        mainSpriteMaterial.SetInteger("_InvertSwipe", 0);
        float normalTime = 0;
        while (time < halfTime)
        {

            time += 0.01f ;
            normalTime = (time / (duration / 2f));
            mainSpriteMaterial.SetFloat("_SwipeAmount", normalTime);
            yield return new WaitForSeconds(0.01f);

        }
        mask.sprite = currentSprite;
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

    }

    void Handleclick() {


        Debug.Log("Handleclick");

        CurrentColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        //material.SetColor("_Color", CurrentColor);
        //imageSprite.color = CurrentColor;
       // StartAnimate();

    }


    public void IncreaseSides()
    {
        Debug.Log("IncreaseSides");
        pointCount++;
        Rebuild();

    }
    public void DecreaseSides()
    {
        if(pointCount > 3)
        {
            pointCount--;
            Debug.Log("DecreaseSides");

            Rebuild();

        }
    }

    void Initiate()
    {
        verts3.Clear();
        tris3.Clear();
        BuildShapeV3(ref verts3, ref tris3);





        verts2.Clear();
        tris2.Clear();
        BuildShapeV2(ref verts2, ref tris2);




        DrawMesh(verts3.ToArray(), tris3.ToArray(), CurrentColor);
        currentSprite = CreateSprite(verts2.ToArray(), tris2.ToArray(), CurrentColor);

        mask.sprite = currentSprite;


    }

    void Rebuild()
    {
        verts3.Clear();
        tris3.Clear();

        verts2.Clear();
        tris2.Clear();

        BuildShapeV3(ref verts3, ref tris3);

        BuildShapeV2(ref verts2, ref tris2);

        DrawMesh(verts3.ToArray(), tris3.ToArray(), CurrentColor);

        currentSprite = CreateSprite(verts2.ToArray(), tris2.ToArray(), CurrentColor);

        StartAnimate();


    }

    public float size = 5;
    void BuildShapeV3(ref List<Vector3> verts, ref List<int> tris)
    {
        verts.Add(new Vector3(0,0, 0));


        for (int i = 0; i < pointCount; i++)
        {
            float theta = Mathf.Deg2Rad * ((float)i / (float)(pointCount)) * 360f;
            float x = size * Mathf.Sin(theta);
            float y = size * Mathf.Cos(theta);

            verts.Add(new Vector3(x, y,0));

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
            localv[i] = verts[i] - new Vector3(lx, ly,0);
        }

        verts = localv.ToList();
    }
    void BuildShapeV2(ref List<Vector2> verts, ref List<ushort> tris)
    {
        verts.Add(new Vector3(0,0, 0));

        for (int i = 0; i < pointCount; i++)
        {
            float theta = Mathf.Deg2Rad * ((float)i / (float)(pointCount)) * 360f;
            float x = size * 2f * Mathf.Sin(theta);
            float y = size * 2f * Mathf.Cos(theta);

            verts.Add(new Vector3(x, y, 0));


            if (i < pointCount - 1)
            {
                tris.Add(0);
                tris.Add((ushort) (i + 1));
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
       // meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }


    Sprite CreateSprite(Vector2[] vertices, ushort[] triangles, Color color)
    {
        Sprite sp = Sprite.Create(new Texture2D(1024, 1024), new Rect(0.0f, 0.0f, 1024, 1024), Vector3.zero,1);

        sp.OverrideGeometry(vertices , triangles);
        return sp;

    }
}
