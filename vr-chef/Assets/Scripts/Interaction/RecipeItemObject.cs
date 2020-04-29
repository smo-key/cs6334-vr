using UnityEngine;
using System.Collections;
using Assets.Scripts.Interaction.Generic;
using Assets.Scripts;
using System.Collections.Generic;

public class RecipeItemObject : GrabbableObject
{
    protected override float SelectedOutlineMultiplier => 2.0f;

    public GameObject objectAbove;
    public GameObject objectBelow;

    public GameObject optionalPlate;

    public bool startCooking = false;
    public float timer = 0;
    public float secondsCooked = 0;
    Renderer rend;

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("RecipeIngredient"))
        {
            print("Collision detected in " + this.gameObject.name);
            print("Collided with " + collision.gameObject.name);
            print(this.gameObject.transform.position);
            print(collision.gameObject.transform.position);
            if (gameObject.transform.position.y < collision.transform.position.y)
            {
                print(gameObject.name + " is below " + collision.gameObject.name);
                this.objectAbove = collision.gameObject;
            }
            else
            {
                print(collision.gameObject.name + " is below " + gameObject.name);
                this.objectBelow = collision.gameObject;

                if(gameObject.name == "patty")
                {
                    // Validate burger stack
                    List<string> recipe = new List<string>();
                    recipe.Add("bottomBun");
                    recipe.Add("patty");
                    //recipe.Add("slicedOnion");
                    //recipe.Add("slicedTomato");
                    //recipe.Add("topBun");
                    print("Validating burger");
                    GameObject plate = validateBurger(this.gameObject, this.optionalPlate, 1, recipe);
                    if (plate != null)
                    {
                        print("PLate is not null");
                        InteractableObject interactableObject = plate.GetComponent<InteractableObject>();
                        interactableObject.MaterialTintOverride = Color.green;
                    }
                }
            }
        }

    }

    private GameObject validateBurger(GameObject recipeItem, GameObject plate, int index, List<string> recipe)
    {
        if (index < 0)
        {
            return plate;
        }
        if (recipeItem == null)
        {
            return null;
        }
        if (recipe[index] != recipeItem.name)
        {
            return null;
        }
        print("Item on stack is " + recipeItem.name);
        RecipeItemObject recipeInteraction = recipeItem.GetComponent<RecipeItemObject>();
        return validateBurger(recipeInteraction.objectBelow, recipeInteraction.optionalPlate, index - 1, recipe);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("RecipeIngredient"))
        {
            if (this.objectBelow == collision.gameObject)
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
        if (name == "patty")
        {
            this.rend = GetComponent<Renderer>();
            if (rend != null)
            {
                this.rend.material = Resources.Load("Materials/PattyRaw") as Material;
            }
        }

    }

    public override void OnFrame(InteractionController controller)
    {
        base.OnFrame(controller);
        if (startCooking && name == "patty")
        {
            //Resources.Load("Materials/PattyRaw");
            if (timer >= 100)
            {
                secondsCooked += 1;
                timer = 0;
            }
            timer += 1;
            if(secondsCooked < 5)
            {
                this.rend.material = Resources.Load("Materials/PattyRaw") as Material;
            }
            else if(secondsCooked < 9)
            {
                this.rend.material = Resources.Load("Materials/PattyCooked") as Material;
            }
            else if (secondsCooked < 12)
            {
                this.rend.material = Resources.Load("Materials/PattyOvercooked") as Material;
                stopCookingIngredient();
            }

            print("TIMER IS: " + secondsCooked);
        }
        //print(gameObject.name + " Position " + gameObject.transform.position.y);

    }

    public void startCookingIngredient()
    {
        startCooking = true;
        timer = 0;
        //secondsCooked = 0;
    }

    public void stopCookingIngredient()
    {
        startCooking = false;
        timer = 0;
        //secondsCooked = 0;
        //InteractableObject interactableObject = this.GetComponent<InteractableObject>();
        //interactableObject.MaterialTintOverride = null;
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
