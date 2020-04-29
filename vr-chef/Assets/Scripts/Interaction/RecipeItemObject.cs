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

    Material currentMaterial;

    private void OnCollisionEnter(Collision collision)
    {
        var ingredient = collision.gameObject.GetComponent<RecipeItemObject>();
        if (ingredient)
        {
            print(this.gameObject.name + " collided with " + collision.gameObject.name);
            if (objectAbove == null)
            {
                print("No object above");
            }
            else
            {
                print("above " + gameObject.name + " is " + objectAbove.name);
            }
            if (objectBelow == null)
            {
                print("No object below");
            }
            else
            {
                print("below " + gameObject.name + " is " + objectBelow.name);
            }

            //print(this.gameObject.transform.position);
            //print(collision.gameObject.transform.position);
            if (objectAbove == null && gameObject.transform.position.y < collision.transform.position.y)
            {
                print(gameObject.name + " is below " + collision.gameObject.name);
                this.objectAbove = collision.gameObject;
            }
            else if(objectBelow == null)
            {
                print(collision.gameObject.name + " is below " + gameObject.name);
                this.objectBelow = collision.gameObject;

                if(gameObject.name.Contains("cheese"))
                {
                    // Validate burger stack
                    List<string> recipe = new List<string>();
                    recipe.Add("bottomBun");
                    recipe.Add("patty");
                    recipe.Add("cheese");
                    //recipe.Add("slicedOnion");
                    //recipe.Add("slicedTomato");
                    //recipe.Add("topBun");
                    print("Validating burger");
                
                    GameObject plate = validateBurger(this.gameObject, this.optionalPlate, recipe.Count-1, recipe);
                    if (plate != null)
                    {
                        print("PLate is not null");
                        InteractableObject interactableObject = plate.GetComponent<InteractableObject>();
                        interactableObject.MaterialTintOverride = Color.green;

                        //run success animation
                        var effect = GameObject.Find("FinishedEffect");
                        var newEffect = GameObject.Instantiate(effect);
                        newEffect.transform.position = plate.transform.position;
                        newEffect.SetActive(true);
                        newEffect.GetComponent<ParticleSystem>().Play();
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
        print("Item on stack is " + recipeItem.name);
        if (!recipeItem.name.Contains(recipe[index]))
        {
            return null;
        }

        RecipeItemObject recipeInteraction = recipeItem.GetComponent<RecipeItemObject>();
        return validateBurger(recipeInteraction.objectBelow, recipeInteraction.optionalPlate, index - 1, recipe);
    }

    private void OnCollisionExit(Collision collision)
    {
        var ingredient = collision.gameObject.GetComponent<RecipeItemObject>();
        if (ingredient)
        {
            print(gameObject.name + " exiting from " + collision.gameObject.name);
            this.objectAbove = null;
            this.objectBelow = null;
        }
    }

    public override void Start()
    {
        base.Start();
        if (name.Contains("patty"))
        {
            ObjectRenderer.material = Resources.Load("Materials/PattyRaw") as Material;
            currentMaterial = Resources.Load("Materials/PattyRaw") as Material;
        }
        Vector3 scale = transform.localScale;
        float scaleF = 1.5f;
        transform.localScale = new Vector3(scale.x * scaleF, scale.y * scaleF, scale.z * scaleF);
    }

    public override void OnFrame(InteractionController controller)
    {
        base.OnFrame(controller);
        if (startCooking && name.Contains("patty"))
        {
            if (timer >= 100)
            {
                secondsCooked += 1;
                timer = 0;
            }
            timer += 1;
            if(secondsCooked < 5)
            {
                ObjectRenderer.material = Resources.Load("Materials/PattyRaw") as Material;
                currentMaterial = Resources.Load("Materials/PattyRaw") as Material; 
            }
            else if(secondsCooked < 9)
            {
                ObjectRenderer.material = Resources.Load("Materials/PattyCooked") as Material;
                currentMaterial = Resources.Load("Materials/PattyCooked") as Material;
            }
            else if (secondsCooked < 12)
            {
                ObjectRenderer.material = Resources.Load("Materials/PattyOvercooked") as Material;
                currentMaterial = Resources.Load("Materials/PattyOvercooked") as Material;
                stopCookingIngredient();
            }
        }
        if (currentMaterial != null)
        {
            ObjectRenderer.material = currentMaterial;
        }
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
