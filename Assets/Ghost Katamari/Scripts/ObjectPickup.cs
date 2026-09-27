using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    [SerializeField] public bool removeCollisionsOnPickup = true;

    [Header("If set to 0, will calculate automatically as x*y*z")]
    [SerializeField] public float objectVolume = 0f;
    [SerializeField] public int tier = 0; // Tier 0 - desktop size objects, tier 1 - room size objects, tier 2 - city size objects.
    [SerializeField] public float minVolumeToPickUp = 0f;

    [SerializeField] private string pickupSound = null;

    [SerializeField] public bool dontScaleCollider = false;
    [SerializeField] public bool dontOffset = false;

    [SerializeField] public bool ignoreSuck = false;

    public static int totalObjectsOnMap = 0;
    public static int objectsCollected = 0;

    private Vector3 colliderSize;

    private void Start()
    {
        totalObjectsOnMap += 1;

        //print(GameManager.instance);

        GameManager.instance.collectablesText.text = objectsCollected + " / " + totalObjectsOnMap;

        colliderSize = GetComponent<BoxCollider>().bounds.size;

        gameObject.tag = "Prop";

        gameObject.layer = LayerMask.NameToLayer("Debris");

        if (objectVolume == 0f)
            objectVolume = colliderSize.x * colliderSize.y * colliderSize.z;
    }
    public void OnPickup(BallController ball)
    {
        objectsCollected += 1;
        GameManager.instance.collectablesText.text = objectsCollected + " / " + totalObjectsOnMap;

        if (pickupSound != null)
            AudioManager.instance.Play(pickupSound);

        if(removeCollisionsOnPickup)
            gameObject.GetComponent<Collider>().enabled = false;

        if(objectVolume < 100f && !dontOffset)
            transform.position += (ball.transform.position - transform.position).normalized * colliderSize.magnitude * 0.1f * objectVolume;

        transform.parent = ball.transform;
    }
}
