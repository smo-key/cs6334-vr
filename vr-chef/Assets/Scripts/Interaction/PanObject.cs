﻿using UnityEngine;
using System.Collections;
using Assets.Scripts.Interaction.Generic;
using Assets.Scripts;

namespace Assets.Scripts
{
    public class PanObject : GrabbableObject
    {
        protected override float SelectedOutlineMultiplier => 2.0f;

        public GameObject dialObject;

        GameObject ingredient;

        private void OnCollisionEnter(Collision collision)
        {
           
            if (collision.gameObject.CompareTag("RecipeIngredient"))
            {
                print("COLLISION AHPEPEND");
                this.GetComponent<InteractableObject>().MaterialTintOverride = Color.red;
                print("Collision detected in " + this.gameObject.name);
                print("Collided with " + collision.gameObject.name);
                if (gameObject.transform.position.y < collision.transform.position.y)
                {
                    print(gameObject.name + " is below " + collision.gameObject.name);
                    this.ingredient = collision.gameObject;
                    RecipeItemObject recipeInteraction = this.ingredient.GetComponent<RecipeItemObject>();
                    recipeInteraction.ChangeIngredientColor(50);
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("RecipeIngredient"))
            {
                this.GetComponent<InteractableObject>().MaterialTintOverride = null;
                if (this.ingredient == collision.gameObject)
                {
                    print(this.ingredient.name + " is no longer above " + this.gameObject.name);
                    this.ingredient = null;
                }
            }
        }
    }
}