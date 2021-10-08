using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Parabox.CSG;
using ConcaveHull;
   public class CarveScript : MonoBehaviour, IDragHandler,IEndDragHandler,IBeginDragHandler
{
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private GameObject DummyObject;
    [SerializeField] private GameObject CarveObject;
    [SerializeField] private GameObject Knife;
    [SerializeField] private Texture2D PumpkinTexture;
    [SerializeField] private MeshRenderer PumpkinRenderer;
    [SerializeField] private Texture2D CarveDetailTexture;

    [Header("Point Detection")]
    [SerializeField] private Transform RayLimitUp;
    [SerializeField] private Transform RayLimitDown;
    [SerializeField] private float RayStep;
    Dictionary<Vector2,Vector2> UvPoints = new Dictionary<Vector2, Vector2>();



    public float Concavity = -0.5f;
    public int ScaleFactor = 1;


    List<Vector3> DrawedVertices = new List<Vector3>();



    Vector3 LastKnifePosition = Vector3.zero;
    int TotalPoint = 0;

    Texture2D TmpTexture;

    #region Hull
    List<Vector2> SortPoints(Vector2 center, List<Vector2> rawPoints)
    {
        List<Vector2> sortedList = new List<Vector2>();

        for(int i=0; i<rawPoints.Count; i++)
        {
            sortedList.Add(new Vector2(rawPoints[i].x - center.x,rawPoints[i].y - center.y));
        }

        List<Vector2> newPoints = BubbleSort(center,rawPoints);

        return newPoints;
    }

    List<Vector2> BubbleSort(Vector2 center,List<Vector2> points)
    {
        List<Vector2> returnList = new List<Vector2>();
        Vector2[] sortedPoints =new Vector2[points.Count];
        Vector2 tmpValue;
        sortedPoints = points.ToArray();

        for (int i = 0; i <= sortedPoints.Length - 1; i++)
        {
            for (int j = 1; j <= sortedPoints.Length - 1; j++)
            {
                if (!ComparePoints(center,sortedPoints[j - 1],sortedPoints[j]))
                {
                    tmpValue = sortedPoints[j - 1];
                    sortedPoints[j - 1] = sortedPoints[j];
                    sortedPoints[j] = tmpValue;
                }
            }
        }

        for(int i=0; i< sortedPoints.Length; i++)
        {
            returnList.Add(sortedPoints[i]);
        }

        return returnList;
    }

    bool ComparePoints(Vector2 center,Vector2 pt1,Vector2 pt2)
    {
        float angle1 = GetAngle(center,pt1);
        float angle2 = GetAngle(center,pt2);

        if(angle1 < angle2)
        {
            return true;
        }

        float dist1 = GetDistance(center,pt1);
        float dist2 = GetDistance(center,pt2);

        if((angle1 == angle2) && (dist1 < dist2))
        {
            return true;
        }

        return false;
    }

    float GetAngle(Vector2 center, Vector2 pt)
    {
        float angle = 0;
        float x = pt.x - center.x;
        float y = pt.y - center.y;
        angle = Mathf.Atan2(y,x);

        if(angle <= 0)
        {
            angle += 2*Mathf.PI;
        }
        return angle;
    }

    float GetDistance(Vector2 pt1, Vector2 pt2)
    {
        float distance = 0;
        float x = pt1.x - pt2.x;
        float y = pt1.y - pt2.y;

        distance = Mathf.Sqrt((x * x) + (y * y));

        return distance;
    }

    #endregion

    #region  TextureOperations
    void InitUvPoints()
    {
        int xStep = Mathf.RoundToInt(Mathf.Abs(RayLimitUp.position.x - RayLimitDown.position.x) / RayStep);
        int yStep = Mathf.RoundToInt(Mathf.Abs(RayLimitUp.position.y - RayLimitDown.position.y) / RayStep);

        RaycastHit hit;

        for(int x=0; x<xStep; x++)
        {
            for(int y=0; y<yStep; y++)
            {
                Vector3 startPosition = new Vector3(RayLimitUp.position.x + (x * RayStep),RayLimitUp.position.y - (y * RayStep),RayLimitUp.position.z);
                Ray ray = new Ray(startPosition,Vector3.forward);

                //Debug.DrawRay(startPosition,Vector3.forward,Color.red,3f);
                if (Physics.Raycast(ray, out hit,1f)) 
                {
                    //Debug.Log("eklendi");
                    Vector2 pixelUV = hit.textureCoord;
                
                    int xUv = Mathf.RoundToInt(pixelUV.x * TmpTexture.width);
                    int yUv = Mathf.RoundToInt(pixelUV.y * TmpTexture.height);

                    UvPoints.Add(new Vector2(x,y),new Vector2(xUv,yUv));
                }    
            }
        }
    }

    Vector2 GetTextureCoordinate(Vector3 worldPos)
    {
        Vector2 coord = new Vector2(-1000,-1000);

        int xPos = Mathf.RoundToInt(Mathf.Abs(worldPos.x - RayLimitUp.position.x) / RayStep);
        int yPos = Mathf.RoundToInt(Mathf.Abs(worldPos.y - RayLimitUp.position.y) / RayStep);
        Vector2 pos = new Vector2(xPos,yPos);

        if(UvPoints.ContainsKey(pos))
        {
            coord = UvPoints[pos];
        }

        return coord;
    }


    void ApplyCarvingDetail(Vector2 pos,bool setTexture)
    {
        Dictionary<Vector2,Color> detail = new Dictionary<Vector2, Color>();
        int width = CarveDetailTexture.width;
        int height = CarveDetailTexture.height;
        for(int i =0; i<width ; i++)
        {
            for(int j =0; j < height ; j++)
            {
                Color tmpColor = new Color(0,0,0,CarveDetailTexture.GetPixel(i,j).r);
                detail.Add(new Vector2(i - (width/2),j-(height/2)),tmpColor);
            }
        }

        int x = (int)pos.x;
        int y = (int)pos.y;

        foreach(Vector2 detailPos in detail.Keys)
        {
            int newX = x + (int)detailPos.x;
            int newY = y + (int)detailPos.y;

            if(newX >= 0 && newX < TmpTexture.width && newY >= 0 && newY < TmpTexture.height)
            {
                //TmpTexture.GetPixel(newX,newY).a + detail[detailPos].a
                Color tmpColor = new Color(1,1,1,TmpTexture.GetPixel(newX,newY).a - (1 - detail[detailPos].a));
                TmpTexture.SetPixel(newX,newY,tmpColor);
            }
        }

        if(setTexture) TmpTexture.Apply();
        
    }

    #endregion


    #region PointZoneControl

    public bool PointInPolygon(List<Vector2> Points ,float X, float Y)
    {
        // Get the angle between the point and the
        // first and last vertices.
        int max_point = Points.Count - 1;
        float total_angle = GetAngle(
            Points[max_point].x, Points[max_point].y,
            X, Y,
            Points[0].x, Points[0].y);

        // Add the angles from the point
        // to each other pair of vertices.
        for (int i = 0; i < max_point; i++)
        {
            total_angle += GetAngle(Points[i].x, Points[i].y, X, Y, Points[i + 1].x, Points[i + 1].y);
        }

        // The total angle should be 2 * PI or -2 * PI if
        // the point is in the polygon and close to zero
        // if the point is outside the polygon.
        // The following statement was changed. See the comments.
        //return (Math.Abs(total_angle) > 0.000001);
        return (Mathf.Abs(total_angle) > 1);
    }

    float GetAngle(float Ax, float Ay,float Bx, float By, float Cx, float Cy)
    {
        // Get the dot product.
        float dot_product = DotProduct(Ax, Ay, Bx, By, Cx, Cy);

        // Get the cross product.
        float cross_product = CrossProductLength(Ax, Ay, Bx, By, Cx, Cy);

        // Calculate the angle.
        return (float)Mathf.Atan2(cross_product, dot_product);
    }

    float CrossProductLength(float Ax, float Ay,float Bx, float By, float Cx, float Cy)
    {
        // Get the vectors' coordinates.
        float BAx = Ax - Bx;
        float BAy = Ay - By;
        float BCx = Cx - Bx;
        float BCy = Cy - By;

        // Calculate the Z coordinate of the cross product.
        return (BAx * BCy - BAy * BCx);
    }

    float DotProduct(float Ax, float Ay,float Bx, float By, float Cx, float Cy)
    {
        // Get the vectors' coordinates.
        float BAx = Ax - Bx;
        float BAy = Ay - By;
        float BCx = Cx - Bx;
        float BCy = Cy - By;

        // Calculate the dot product.
        return (BAx * BCx + BAy * BCy);
    }
    #endregion



    public void OnBeginDrag(PointerEventData eventData)
    {

    }

    public void OnDrag(PointerEventData eventData)
    {
        RaycastHit hit;
        Ray ray = PlayerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit)) 
        {

            Vector2 texCoord = GetTextureCoordinate(hit.point);

            if(texCoord != new Vector2(-1000,-1000))
            {
                ApplyCarvingDetail(texCoord,true);
                
                int x = (int)(Mathf.Abs((hit.point - RayLimitUp.position).x / RayStep));
                int y = (int)(Mathf.Abs((hit.point - RayLimitUp.position).y / RayStep));
                AddCarvePoint(new Vector3(x,y,0) );
            }
            

            SetKnifePosition(hit.point,hit.normal);
        }
    }



    public void OnEndDrag(PointerEventData eventData)
    {

        SetKnifePosition(Vector3.one * -99,Vector3.zero);

        GetConcaveHull();

    }


    List<Node> CarvePoints = new List<Node>();
    void AddCarvePoint(Vector3 pos)
    {
        CarvePoints.Add(new Node(pos.x, pos.y,TotalPoint));
        TotalPoint++;
    }

    void GetConcaveHull()
    {
        Hull.setConvexHull(CarvePoints);
        Hull.setConcaveHull(Concavity, ScaleFactor);

        Invoke("SetCarve",0.2f);
    }

    void SetCarve()
    {
        List<Vector2> limitPoints = new List<Vector2>();


        int cnt = 0;
        for (int i = 0; i < Hull.hull_concave_edges.Count; i++) 
        {
            float xPos =(float)Hull.hull_concave_edges[i].nodes[0].x;
            float yPos =(float)Hull.hull_concave_edges[i].nodes[0].y;

            limitPoints.Add(new Vector2(xPos,yPos));
            cnt++;
        }

        foreach(Vector2 pos in limitPoints)
        {
            Debug.Log(pos);
        }

        foreach(Vector2 pos in UvPoints.Keys)
        {
            Debug.Log("carve kontrol");
            
            if(PointInPolygon(limitPoints,pos.x,pos.y))
            {
                Debug.Log("carve eklendi");
                ApplyCarvingDetail(UvPoints[pos],false);
            }
        }

        TmpTexture.Apply();


        
        CarvePoints.Clear();
        Hull.hull_edges.Clear();
        Hull.hull_concave_edges.Clear();
        Hull.unused_nodes.Clear();
        TotalPoint = 0;

    }


    void SetKnifePosition(Vector3 pos,Vector3 norm)
    {
        Knife.transform.position = pos;
        Knife.transform.rotation = Quaternion.LookRotation(norm);

        Vector3 angleVector = pos - LastKnifePosition;
        float cutAngle = Vector3.SignedAngle(angleVector.normalized,-Vector3.up,-Vector3.forward);
        //Debug.Log(cutAngle);
        iTween.RotateTo(Knife.transform.GetChild(0).gameObject,iTween.Hash("rotation",new Vector3(0,0,-cutAngle),"time",0.1f,"islocal",true,"ignoretimescale",true));
        //Knife.transform.GetChild(0).localRotation = ;
        LastKnifePosition = pos;
        //Knife.transform.LookAt()
    }


    // Start is called before the first frame update
    void Start()
    {
        PlayerCamera = Camera.main;
        TmpTexture = Instantiate(PumpkinTexture);
        PumpkinRenderer.material.mainTexture = TmpTexture;

        InitUvPoints();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
 

