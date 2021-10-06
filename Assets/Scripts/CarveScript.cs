using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Parabox.CSG;

   public class CarveScript : MonoBehaviour, IDragHandler,IEndDragHandler,IBeginDragHandler
{
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private GameObject DummyObject;
    [SerializeField] private GameObject CarveObject;



    List<Vector3> DrawedVertices = new List<Vector3>();
    List<Vector3> DrawedNormals = new List<Vector3>();
    List<Vector2> DrawedUv = new List<Vector2>();

  
    int TotalPoint = 0;
    public void OnBeginDrag(PointerEventData eventData)
    {
        //DrawedVertices.Add(new Vector3(0,0,0));
        //DrawedUv.Add(Vector2.zero);
        //DrawedNormals.Add(Vector3.forward);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RaycastHit hit;
        Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit)) 
        {

            if(TotalPoint > 0 && Vector3.Distance(DrawedVertices[TotalPoint],hit.point) > 0.01f)
            {
                TotalPoint += 2;
                DrawedVertices.Add(hit.point);
                DrawedUv.Add(Vector2.zero);
                DrawedNormals.Add(Vector3.up);

                Vector3 paralelPoint = hit.point;
                paralelPoint.z += 0.2f;
                DrawedVertices.Add(paralelPoint);
                DrawedUv.Add(Vector2.zero);
                DrawedNormals.Add(-Vector3.up);

                Debug.DrawRay(hit.point,hit.normal * 3,Color.red,5f);
            }


        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        Debug.Log(TotalPoint);
        Mesh mesh = new Mesh();

        Mesh tmpMesh = DummyObject.GetComponent<MeshFilter>().mesh;
/*
                    TotalPoint += 2;
                    DrawedVertices.Add(hit.point);
                    DrawedUv.Add(Vector2.zero);
                    DrawedNormals.Add(Vector3.up);
*/
        int cnt = DrawedVertices.Count;
        int[] triArray = new int[((cnt ) * 3) + (((cnt/2)-2) * 3) ];
//int[] triArray = new int[(cnt + ((cnt/2)-2)) * 3 ];
        for(int i=0; i<cnt ; i++)
        {
            triArray[(i*3)] = i;

            if(i%2 == 0)
            {
                triArray[(i*3)+1] = (i+2) % cnt;
                triArray[(i*3)+2] = (i+1) % cnt;
            }
            else
            {
                triArray[(i*3)+1] = (i+1) % cnt;
                triArray[(i*3)+2] = (i+2) % cnt;
            }
        }

        int step = 0;
        for(int i=0; i < cnt/2 - 2; i++)
        {
            triArray[(cnt*3)+(i*3)] = 1;

            if(i < 0)
            {
                triArray[(cnt*3)+(i*3)+2] = (2*i) +3;
                triArray[(cnt*3)+(i*3)+1] = (2*i) + 5;
            }
            else
            {
                triArray[(cnt*3)+(i*3)+2] = (2*i) + 5;
                triArray[(cnt*3)+(i*3)+1] = (2*i) + 3;
            }

                
                /*

            if(step%2 == 0)
            {
                triArray[(cnt*3)+(i*3)+1] = (2*i) + 3;
                triArray[(cnt*3)+(i*3)+2] = (2*i) +5;
            }
            else
            {
                triArray[(cnt*3)+(i*3)+1] = (2*i) + 5;
                triArray[(cnt*3)+(i*3)+2] = (2*i) + 3;

            }
            step++;
            */
        }

    



        mesh.vertices = DrawedVertices.ToArray();
        mesh.normals = DrawedNormals.ToArray();

        //mesh.uv = DrawedUv.ToArray();
mesh.triangles = triArray;

        

        mesh.RecalculateNormals();
        DummyObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        //CarverMaskObject.GetComponent<MeshFilter>().mesh = mesh;
        //DummyObject.GetComponent<MeshRenderer>().enabled = true;


        DrawedNormals.Clear();
        DrawedVertices.Clear();
        DrawedUv.Clear();
        TotalPoint = 0;
        
        Invoke("Carve",0.2f);

    }

    void Carve()
    {
        CarveObject.GetComponent<MeshFilter>().mesh.subMeshCount = 1;
        
        Model result = CSG.Subtract(CarveObject,DummyObject);

        //Material[] mats = CarveObject.GetComponent<MeshRenderer>().materials;

        //result.mesh.RecalculateNormals();
        // Create a gameObject to render the result
        var composite = new GameObject();
        CarveObject.GetComponent<MeshFilter>().mesh = result.mesh;
        //CarveObject.GetComponent<MeshRenderer>().materials = mats;
        Debug.Log("last");
        //DummyObject.GetComponent<MeshFilter>().mesh = tmpMesh;
        DrawedVertices.Clear();
        TotalPoint = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerCamera = Camera.main;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
 

