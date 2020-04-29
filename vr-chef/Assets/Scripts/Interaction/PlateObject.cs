using UnityEngine;
using System.Collections;
using Assets.Scripts.Interaction.Generic;
using Assets.Scripts;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class PlateObject : GrabbableObject
    {
        protected override float SelectedOutlineMultiplier => 2.0f;

        GameObject ingredient;

        private void OnCollisionEnter(Collision collision)
        {

            if (this.ingredient == null && collision.gameObject.CompareTag("RecipeIngredient"))
            {
                this.GetComponent<InteractableObject>().MaterialTintOverride = Color.red;
                if (gameObject.transform.position.y < collision.transform.position.y)
                {
                    print(gameObject.name + " is below " + collision.gameObject.name);

                    this.ingredient = collision.gameObject;
                

                    RecipeItemObject recipeInteraction = this.ingredient.GetComponent<RecipeItemObject>();
                    recipeInteraction.optionalPlate = this.gameObject;


                    if(this.ingredient.name != "bottomBun")
                    {
                        InteractableObject interactableObject = this.GetComponent<InteractableObject>();
                        interactableObject.MaterialTintOverride = Color.red;
                    }
                    else if (this.ingredient.name == "bottomBun")
                    {
   
                            InteractableObject interactableObject = this.GetComponent<InteractableObject>();
                            interactableObject.MaterialTintOverride = Color.blue;
                        
                    }

                }
            }
        }

        private bool validateBurger(GameObject recipeItem, int index, List<string> recipe)
        {
            if(index >= recipe.Count)
            {
                return true;
            }
            if(recipeItem == null)
            {
                return false;
            }
            if(recipe[index] != recipeItem.name)
            {
                return false;
            }
            RecipeItemObject recipeInteraction = recipeItem.GetComponent<RecipeItemObject>();
            return validateBurger(recipeInteraction.objectAbove, index + 1, recipe);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (this.ingredient != null && collision.gameObject.CompareTag("RecipeIngredient"))
            {
                if (this.ingredient == collision.gameObject)
                {
                    print(this.ingredient.name + " is no longer above " + this.gameObject.name);
                    
                    InteractableObject interactableObject = this.GetComponent<InteractableObject>();
                    interactableObject.MaterialTintOverride = null;
                    this.ingredient = null;
                    
                }
            }
        }
    }
}