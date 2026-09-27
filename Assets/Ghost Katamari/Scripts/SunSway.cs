using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunSway : MonoBehaviour
{
    [SerializeField] float amplitude = 5f;
    [SerializeField] float frequency = 6f;
    
    Vector3 startingRotation = Vector3.zero;

    private void Start()
    {
        startingRotation = transform.eulerAngles;
    }

    private void Update()
    {
        var currentOffset = amplitude * Mathf.Sin(frequency * Time.time);

        transform.rotation = Quaternion.Euler(startingRotation.x + currentOffset, startingRotation.y, startingRotation.z);
    }
}
