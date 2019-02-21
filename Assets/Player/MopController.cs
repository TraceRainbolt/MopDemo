using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MopController : MonoBehaviour {


    enum Fluid { Acid, Ferro, Glue };

    private Color colorAcid = new Color(0.2f, 0.8f, 0.3f);
    private Color colorFerro = new Color(0.2f, 0.2f, 0.2f);
    private Color colorGlue = new Color(1, 1, 1);

    private Fluid fluidType;
    private bool hasLiquid;

    private SpriteRenderer mop;

    private void Start()
    {
        mop = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Vector3 center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            center.z = 0;

            LiquidControl.instance.Create(center);
        }

        if (Input.GetButton("Fire1"))
        {
            Vector3 center = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            center.z = 0;

            LiquidControl.instance.Redraw(center);
        }
    }

    void UpdateFluidMaterial(GameObject fluidObject)
    {
        SpriteRenderer fluidSprite = fluidObject.GetComponent<SpriteRenderer>();

        switch (fluidType)
        {
            case Fluid.Acid:
                fluidSprite.color = colorAcid;
                break;
            case Fluid.Ferro:
                fluidSprite.color = colorFerro;
                break;
            case Fluid.Glue:
                fluidSprite.color = colorGlue;
                break;
        }
    }
}
