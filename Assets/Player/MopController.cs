using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MopController : MonoBehaviour {

    public int maxFluid = 10;
    public GameObject fluid;

    enum Fluid { Acid, Ferro, Glue };

    private Color colorAcid = new Color(0.2f, 0.8f, 0.3f);
    private Color colorFerro = new Color(0.2f, 0.2f, 0.2f);
    // private Color colorGlue = new Color(0.9f, 0.9f, 0.99f);
    private Color colorGlue = new Color(1, 1, 1);

    private Fluid fluidType;
    private int fluidAmount;

    private GameObject fluids;
    private SpriteRenderer mop;

    private void Start()
    {
        fluids = new GameObject("FluidContainer");
        mop = GetComponent<SpriteRenderer>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag.Contains("Fluid") && Input.GetButton("Fire2"))
        {
            fluidAmount = maxFluid;
            Destroy(other.gameObject);
            if (other.tag.Contains("Ferro"))
            {
                mop.color = colorFerro;
                fluidType = Fluid.Ferro;
            }
            if (other.tag.Contains("Acid"))
            {
                mop.color = colorAcid;
                fluidType = Fluid.Acid;
            }
            if (other.tag.Contains("Glue"))
            {
                mop.color = colorGlue;
                fluidType = Fluid.Glue;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Input.GetButton("Fire1") && collision.gameObject.CompareTag("MoppableSurface") && fluidAmount > 0)
        {
            Vector3 fluidPosition = transform.position - new Vector3(0.2f, 0.58f, 0);
            fluidPosition.z = 0;

            GameObject newFluid = Instantiate(fluid, fluidPosition, Quaternion.identity);
            Bounds moppableBounds = collision.gameObject.GetComponent<Collider2D>().bounds;
            Bounds fluidBounds = newFluid.GetComponent<Collider2D>().bounds;

            if (!fluidBounds.Intersects(moppableBounds))
            {
                Destroy(newFluid);
            }
            UpdateFluidMaterial(newFluid);
            newFluid.transform.parent = fluids.transform;
            fluidAmount--;
        }

        if (fluidAmount == 0)
        {
            mop.color = new Color(1, 1, 1);
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
