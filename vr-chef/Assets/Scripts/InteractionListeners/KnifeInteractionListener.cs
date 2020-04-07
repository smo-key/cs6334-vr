using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class KnifeInteractionListener : InteractionListener
{
    static Color DefaultOutlineColor = Color.white;
    static Color SelectedOutlineColor = new Color(255.0f / 255.0f, 228.0f / 255.0f, 0.0f / 255.0f);
    static float SelectedOutlineMultiplier = 3.0f;
    static Vector3 RotationBias = new Vector3(0, 0, 0);

    static float MIN_ENTRANCE_VELOCITY = 0.4f;
    static float MIN_EXIT_VELOCITY = 0.4f;

    public GameObject DefaultColliders;
    public GameObject HeldBoundaryColliders;
    public Collider ChopCollider;
    public MeshRenderer MeshRenderer;

    float defaultOutlineWidth;
    bool isGrabbed = false;
    Rigidbody rigidBody;
    AudioSource audioData;

    HashSet<GameObject> objectsChopping = new HashSet<GameObject>();

    public void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        audioData = gameObject.GetComponent<AudioSource>();
        defaultOutlineWidth = MeshRenderer.material.GetFloat("_OutlineWidth");

        UpdateMaterial(false);
    }

    void UpdateMaterial(bool isNearHand)
    {
        foreach (var material in MeshRenderer.materials)
        {
            material.SetColor("_Tint", isNearHand ? SelectedOutlineColor : DefaultOutlineColor);
            material.SetColor("_OutlineColor", isNearHand ? SelectedOutlineColor : DefaultOutlineColor);
            material.SetFloat("_OutlineWidth", isNearHand ? defaultOutlineWidth * SelectedOutlineMultiplier : defaultOutlineWidth);
        }

        MeshRenderer.UpdateGIMaterials();
    }

    void UpdateColliders()
    {
        DefaultColliders.SetActive(!isGrabbed);
        HeldBoundaryColliders.SetActive(isGrabbed && objectsChopping.Count == 0);
        ChopCollider.enabled = isGrabbed;
    }

    public void OnEdgeCollisionEnter(Collider collider, ColliderListenerAction listener)
    {
        if (!isGrabbed) return;

        //print("Collided with " + collider.gameObject.name + " at " + listener.CurrentVelocity.magnitude);
        if (listener.CurrentVelocity.y >= 0) return;    // ensure that we are cutting down
        if (listener.CurrentVelocity.magnitude < MIN_ENTRANCE_VELOCITY) return;

        //ensure item is choppable
        var foodListener = collider.gameObject.GetComponent<FoodInteractionListener>();
        if (!foodListener) return;

        collider.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        objectsChopping.Add(collider.gameObject);
        UpdateColliders();
    }

    public void OnEdgeCollisionFrame(Collider collider, ColliderListenerAction listener)
    {

    }

    public void OnEdgeCollisionExit(Collider collider, ColliderListenerAction listener)
    {
        if (!isGrabbed) return;
        if (!objectsChopping.Contains(collider.gameObject)) return;
        objectsChopping.Remove(collider.gameObject);
        UpdateColliders();

        //print("Exited " + collider.gameObject.name + " at " + listener.CurrentVelocity.magnitude);
        collider.gameObject.GetComponent<Rigidbody>().isKinematic = false;

        if (listener.CurrentVelocity.magnitude < MIN_EXIT_VELOCITY) return;
        collider.gameObject.GetComponent<FoodInteractionListener>().OnChopped();
        audioData.Play(0);
    }

    public override void OnFrame(InteractionController controller)
    {
        //attach item position to hand mesh's position and rotation every frame if grabbed
        if (isGrabbed)
        {
            gameObject.transform.position = controller.Target.transform.position;
            Vector3 euler = controller.Target.transform.rotation.eulerAngles + RotationBias;
            gameObject.transform.rotation = Quaternion.Euler(euler);
        }

        //attach food item position to hand mesh's position and rotation every frame if grabbed
        /*if (isGrabbed)
        {
            var hand = controller.GetHand();
            var handRenderer = hand.GetComponent<MeshRenderer>();
            gameObject.transform.position = controller.Target.transform.position;
            gameObject.transform.rotation = controller.Target.transform.rotation;
            handRenderer.enabled = false;
            //print("Knife: " + gameObject.transform.rotation);
            //print("Hand: " + controller.Target.transform.rotation);
            if (controller.Name.Equals("Left"))
            {
                gameObject.transform.Rotate(controller.transform.rotation.x + 45, controller.transform.rotation.y + 45, controller.transform.rotation.z + 45);
            }
            else if (controller.Name.Equals("Right"))
            {
                gameObject.transform.Rotate(controller.transform.rotation.x + 45, controller.transform.rotation.y - 45, controller.transform.rotation.z + 45);
            }
            //print("Knife: " + gameObject.transform.position);
            timeSpan = System.DateTime.UtcNow - startTime;
            //print("Elasped MS: " + timeSpan.TotalMilliseconds);
            double timeThreshold = 1000;
            if(timeSpan.TotalMilliseconds >= timeThreshold)
            {
                currentY = gameObject.transform.position.y;
                double diff = currentY - prevY;
                print("DIFF: " + diff.ToString());
                objectRenderer.materials = OriginalMaterials;
                IsChopping = false;
                if (diff <= -0.15)
                {
                    print("Chopped Down");
                    audioData.Play(0);
                    IsChopping = true;
                    Material[] materials2 = new Material[objectRenderer.materials.Length];
                    for (int i = 0; i < materials2.Length; i++)
                    {
                        materials2[i] = ChoppedMaterial;
                    }
                    objectRenderer.materials = materials2;
                }
                prevY = currentY;
                startTime = System.DateTime.UtcNow;
            }
        }*/
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

        //make the hand mesh invisible
        isGrabbed = true;
        handRenderer.enabled = false;
        rigidBody.isKinematic = true;

        //switch colliders
        UpdateColliders();

        //update materisl
        UpdateMaterial(false);
    }

    public override void OnDrop(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();

        //remove knife item
        isGrabbed = false;
        handRenderer.enabled = true;
        rigidBody.isKinematic = false;
        objectsChopping.Clear();

        //switch colliders
        UpdateColliders();

        //update materisl
        UpdateMaterial(false);
    }
}
