using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum BrushTags
{
    CarveBrush,
    HoleBrush,
}


[System.Serializable]
public class Brush 
{
    private Dictionary<Vector2,Color> BrushValues = new Dictionary<Vector2, Color>();
    public Texture2D BrushTexture;
    public BrushTags BrushName;

    private bool IsInitialized = false;

    public Dictionary<Vector2,Color> GetBrushValues()
    {
        if(IsInitialized)
        {
            return BrushValues;
        }

        IsInitialized = true;

        int width = BrushTexture.width;
        int height = BrushTexture.height;
        for(int i =0; i<width ; i++)
        {
            for(int j =0; j < height ; j++)
            {
                Color pixelColor = BrushTexture.GetPixel(i,j);
                Color tmpColor = new Color(pixelColor.r,pixelColor.g,pixelColor.b,pixelColor.r);
                BrushValues.Add(new Vector2(i - (width/2),j-(height/2)),tmpColor);
            }
        }

        return BrushValues;

        

    }


}
