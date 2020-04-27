using UnityEngine;
using System.Collections;
using Assets.Scripts.Interaction.Generic;
using Assets.Scripts;

public class RecipeItemObject : GrabbableObject
{
    protected override float SelectedOutlineMultiplier => 2.0f;

    GameObject objectAbove;
    GameObject objectBelow;

    public void ChangeIngredientColor(float percent)
    {
        Material material = ObjectRenderer.material;
        percent = Mathf.Clamp01(percent);
        material.color = new Color(material.color.r * (1 - percent), material.color.g * (1 - percent), material.color.b * (1 - percent), material.color.a);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("RecipeIngredient"))
        {
            print("Collision detected in " + this.gameObject.name);
            print("Collided with " + collision.gameObject.name);
            if(gameObject.transform.position.y < collision.transform.position.y)
            {
                print(gameObject.name + " is below " + collision.gameObject.name);
                this.objectAbove = collision.gameObject;
            }
            else
            {
                print(collision.gameObject.name + " is below " + gameObject.name);
                this.objectBelow = collision.gameObject;
            }
        }
        
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("RecipeIngredient"))
        {
            if(this.objectBelow == collision.gameObject)
            {
                print(this.objectBelow.name + " is no longer below " + this.gameObject.name);
                this.objectBelow = null;
            }
            else if (this.objectAbove == collision.gameObject)
            {
                print(this.objectAbove.name + " is no longer above " + this.gameObject.name);
                this.objectAbove = null;
            }
        }
    }

    public override void Start()
    {
        base.Start();
       
    }

    public override void OnFrame(InteractionController controller)
    {
        base.OnFrame(controller);
    }

    public override void OnGrab(InteractionController controller)
    {
        base.OnGrab(controller);
    }

    public override void OnDrop(InteractionController controller)
    {
        base.OnDrop(controller);

        //TODO snap to item area
    }
}
