using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Globals : MonoBehaviour
{
    public static Globals Instance;

    void Awake()
    {
        Instance = this;
    }

    int CurrentLevel = 1;


    public int GetCurrentLevel()
    {
        return CurrentLevel;

    }

    public Texture2D GetCurrentTargetTexture()
    {
        Texture2D targetTexture = Resources.Load<Texture2D>($"Levels/{GetCurrentLevel()}");

        return targetTexture;
    }

    public Texture2D GetCurrentTargetScene()
    {
        Texture2D targetTexture = Resources.Load<Texture2D>($"Scenes/{GetCurrentLevel()}");

        return targetTexture;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
