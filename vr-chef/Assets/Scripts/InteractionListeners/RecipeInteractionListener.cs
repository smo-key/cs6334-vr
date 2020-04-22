using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class RecipeInteractionListener : InteractionListener
{
    public static Color DefaultOutlineColor = Color.white;
    public static Color SelectedOutlineColor = new Color(255.0f / 255.0f, 228.0f / 255.0f, 0.0f / 255.0f);
    public static Color ChoppedOutlineColor = new Color(0.0f / 255.0f, 0.0f / 255.0f, 0.0f / 255.0f, 0.5f);
    public static float SelectedOutlineMultiplier = 2.0f;

    Rigidbody rb;
    Renderer renderer;
    bool isGrabbed = false;
    float defaultOutlineWidth;
    Vector3 originalRotationOnGrab;
    int chopCount = 0;
    GameObject objectAbove;
    GameObject objectBelow;

    void UpdateMaterial(bool isNearHand)
    {
        foreach (var material in renderer.materials)
        {
            if (chopCount > 0)
            {
                material.SetColor("_Tint", ChoppedOutlineColor);
                material.SetColor("_OutlineColor", ChoppedOutlineColor);
                material.SetFloat("_OutlineWidth", isNearHand ? defaultOutlineWidth * SelectedOutlineMultiplier : defaultOutlineWidth);
            }
            else
            {
                material.SetColor("_Tint", isNearHand ? SelectedOutlineColor : DefaultOutlineColor);
                material.SetColor("_OutlineColor", isNearHand ? SelectedOutlineColor : DefaultOutlineColor);
                material.SetFloat("_OutlineWidth", isNearHand ? defaultOutlineWidth * SelectedOutlineMultiplier : defaultOutlineWidth);
            }
        }

        renderer.UpdateGIMaterials();
    }

    public void changeIngredientColor(float percent)
    {
        Material material = this.GetComponent<MeshRenderer>().material;
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

    public void Start()
    {
        renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        rb = gameObject.GetComponent<Rigidbody>();
        defaultOutlineWidth = renderer.material.GetFloat("_OutlineWidth");

        //update material
        UpdateMaterial(false);
    }

    public override void OnFrame(InteractionController controller)
    {
        //attach food item position to hand mesh's position and rotation every frame if grabbed
        if (isGrabbed)
        {
            var hand = controller.GetHand();
            var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();
            gameObject.transform.position = controller.Target.transform.position;

            Vector3 newAngles = controller.Target.transform.rotation.eulerAngles - originalRotationOnGrab + controller.RotationBias;
            gameObject.transform.rotation = Quaternion.Euler(newAngles);
            handRenderer.enabled = false;
        }
    }

    public override void OnEnterClosest(InteractionController controller)
    {
        //don't do anything if there's an object in the hand
        if (controller.ControlledObject) return;

        UpdateMaterial(true);
    }

    public override void OnLeaveClosest(InteractionController controller)
    {
        UpdateMaterial(false);
    }

    public override void OnGrab(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();

        originalRotationOnGrab = controller.Target.transform.rotation.eulerAngles;

        //make the hand mesh invisible
        isGrabbed = true;
        handRenderer.enabled = false;
        rb.isKinematic = true;

        UpdateMaterial(false);
    }

    public override void OnDrop(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();

        //remove food item
        isGrabbed = false;
        handRenderer.enabled = true;
        rb.isKinematic = false;
    }
}
