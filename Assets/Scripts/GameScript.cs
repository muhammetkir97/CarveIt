using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour
{
    [SerializeField] private Light InsideLight;
    void Start()
    {
        //InvokeRepeating("StartLightAnimation",0,1);
        StartLightAnimation();
    }

    void StartLightAnimation()
    {
        iTween.ValueTo(gameObject,iTween.Hash("from",0.1f,"to",0.3f,"time",1f,"looptype",iTween.LoopType.pingPong,"onupdate","SetLightValue","onupdatetarget",gameObject));
    }

    void SetLightValue(float newLightValue)
    {
        InsideLight.intensity = newLightValue;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
