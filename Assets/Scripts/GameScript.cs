using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameScript : MonoBehaviour
{
    [SerializeField] private Light InsideLight;

    [SerializeField] private RenderTexture CurrentView;
    [SerializeField] private Camera CurrentViewCam;
    [SerializeField] private Button BtnSave;
    [SerializeField] private Button BtnCompare;
    [SerializeField] private GameObject DebugMenu;

    [SerializeField] private MeshRenderer PumpkinRenderer;

    [SerializeField] private ParticleSystem EndParticle;
    [SerializeField] private GameObject SimilarityPopup;
    [SerializeField] private GameObject MainPopup;
    [SerializeField] private Image SimilarityProgress;
    [SerializeField] private Button BtnSend;

    [SerializeField] private RawImage TargetScene;

    bool DebugMenuStatus = false;

    void Start()
    {
        //InvokeRepeating("StartLightAnimation",0,1);
        StartLightAnimation();

        BtnSave.onClick.AddListener(SaveCurrentScene);
        BtnCompare.onClick.AddListener(ComparePumpkin);
        BtnSend.onClick.AddListener(SendPumpkin);

        TargetScene.texture = Globals.Instance.GetCurrentTargetScene();

    }

    void StartLightAnimation()
    {
        iTween.ValueTo(gameObject,iTween.Hash("from",0.1f,"to",0.3f,"time",1f,"looptype",iTween.LoopType.pingPong,"onupdate","SetLightValue","onupdatetarget",gameObject));
    }

    void SetLightValue(float newLightValue)
    {
        InsideLight.intensity = newLightValue;
    }

    #region  ImageCompare

    int[,] Image1Values = new int[3,255];
    int[,] Image2Values = new int[3,255];

    void ResetHistograms()
    {
        for(int i=0; i<3; i++)
        {
            for(int j=0; j<255; j++)
            {
                Image1Values[i,j] = 0;
                Image2Values[i,j] = 0;
            }
        }
    }

    public int Compare()
    {
        ResetHistograms();
        int similarityRate = 0;

        Texture2D currentTex = (Texture2D)PumpkinRenderer.material.GetTexture("_mainTexture");
        //currentTex.Resize(512,512);
        //currentTex.Apply();

        Texture2D targetTex = Globals.Instance.GetCurrentTargetTexture();

        int[,] tempHistogram1 = GetImageHistogram(currentTex);
        int[,] tempHistogram2 = GetImageHistogram(targetTex);


        similarityRate = CompareHistograms(tempHistogram1,tempHistogram2);

        return similarityRate;
    }

    Texture2D RenderToTexture2D(RenderTexture renderTex)
    {
        CurrentViewCam.Render();
        Texture2D tex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.ARGB32, false);
        RenderTexture.active = renderTex;
        tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tex.Apply();
        Destroy(tex);
        return tex;
    }

    int[,] GetImageHistogram(Texture2D image)
    {
        int[,] histogram = new int[3,255];

        for(int i=0; i<3; i++)
        {
            for(int j=0; j<255; j++)
            {
                histogram[i,j] = 0;
    
            }
        }

        int width = image.width;
        int height = image.height;


        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                Color pixelColor = image.GetPixel(x,y);

                int r = (int)(pixelColor.r * 254);
                int g = (int)(pixelColor.g * 254);
                int b = (int)(pixelColor.b * 254);

                

                histogram[0,r]++;
                histogram[1,g]++;
                histogram[2,b]++;
            }
        }

        return histogram;
    }

    int CompareHistograms(int[,] histogram1, int[,] histogram2)
    {
        int matchValue = 0;

        for(int channel=0; channel<3; channel++)
        {
            for(int value=0; value<255; value++)
            {
                matchValue += Mathf.Abs(histogram1[channel,value] - histogram2[channel,value]);
            }
        }

        return matchValue;
    }

    #endregion


    #region CreateTarget

    void SaveTextureToFile (Texture2D texture,string path) 
    {
        
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
    }

    void SaveCurrentScene()
    {
        Texture2D sceneImage = RenderToTexture2D(CurrentView);

        Texture2D pumpkinTexture = Instantiate((Texture2D)PumpkinRenderer.material.GetTexture("_mainTexture"));
        //pumpkinTexture.Resize(512,512);
        //pumpkinTexture.Apply();
        SaveTextureToFile(pumpkinTexture,$"{Application.dataPath}/Resources/Levels/level.png");
        SaveTextureToFile(sceneImage,$"{Application.dataPath}/Resources/Scenes/level.png");
        //SaveTextureToFile(RenderToTexture2D(CurrentView),$"{Application.dataPath}/Resources/Levels/level.png");
    }


    #endregion

    void ComparePumpkin()
    {
        int similarity = Compare();
        Debug.Log(similarity);
    }

    void SendPumpkin()
    {
        iTween.MoveTo(MainPopup,MainPopup.transform.position + Vector3.left * 1000,0.3f);
        iTween.MoveTo(SimilarityPopup,SimilarityPopup.transform.position + Vector3.up * 500,0.3f);
        Invoke("LateCompare",0.3f);
        
    }

    void LateCompare()
    {
        int similarity = Compare();
        Debug.Log(similarity);
        float ratio = 1 - (similarity / 200000f);
        if(ratio < 0) ratio  = 0;
        iTween.ValueTo(gameObject,iTween.Hash("from",0,"to",ratio,"time",1f,"onupdate","changeProgressBarValue"));
        Invoke("StartCompareEffects",1);
    }

    void changeProgressBarValue(float val)
    {
        SimilarityProgress.fillAmount = val;
    }

    void StartCompareEffects()
    {
        EndParticle.Play();
    }


    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
            DebugMenuStatus = !DebugMenuStatus;

            if(DebugMenuStatus)
            {
                iTween.ScaleTo(DebugMenu,Vector3.one,0.3f);
            }
            else
            {
                iTween.ScaleTo(DebugMenu,Vector3.zero,0.3f);
            }
        }
    }


}
