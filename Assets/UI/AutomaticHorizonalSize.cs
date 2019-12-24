using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticHorizonalSize : MonoBehaviour
{
    // Start is called before the first frame update
    public float childWidth=100f;
    void Start()
    {
        AdjustSize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AdjustSize(){
        Vector2 size=this.GetComponent<RectTransform>().sizeDelta;
        size.x=this.transform.childCount *childWidth;
        size.y=50f;
        this.GetComponent<RectTransform>().sizeDelta=size;
    }
}
