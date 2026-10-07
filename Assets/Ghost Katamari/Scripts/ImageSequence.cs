using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageSequence : MonoBehaviour
{

    public Texture[] frames;              // Array of textures (sequence frames)
    public float framesPerSecond = 24f;   // Speed of playback

    [SerializeField] Material targetMaterial;

    private int currentFrame = 0;
    private float timer = 0f;

    void Update()
    {
        if (frames == null || frames.Length == 0 || targetMaterial == null)
            return;

        // Advance timer
        timer += Time.deltaTime;

        // Time between frames
        float frameDuration = 1f / framesPerSecond;

        if (timer >= frameDuration)
        {
            // Advance frame
            currentFrame = (currentFrame + 1) % frames.Length;

            // Apply texture
            targetMaterial.mainTexture = frames[currentFrame];

            // Reset timer
            timer -= frameDuration;
        }
    }
}
