using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera endingScreenCamera;
    [SerializeField] private CinemachineVirtualCamera introCamera;

    [SerializeField] BallController ballController;

    [SerializeField] GameObject ghostSprite;

    [SerializeField] Timer timer;

    [SerializeField] public TextMeshProUGUI collectablesText = null;

    [SerializeField] GameObject replayCanvas = null;

    public static GameManager instance;

    public int currentLevel = 1;

    public CanvasGroup canvasGroup;

    private void Awake()
    {
        instance = this;
        ObjectPickup.totalObjectsOnMap = 0;
        ObjectPickup.objectsCollected = 0;
    }
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Level 2")
            currentLevel = 2;

        if (SceneManager.GetActiveScene().name == "Level 3")
            currentLevel = 3;

        StartCoroutine(PlayIntro());

        EnableDebrisCollisions();

        normalFov = endingScreenCamera.m_Lens.FieldOfView;
        dashFov = normalFov * 0.7f;
    }

    IEnumerator PlayIntro()
    {
        CinemachineManualFreeLook.canMoveCamera = false;

        yield return new WaitForSeconds(1f);

        StartCoroutine(FadeRoutine(0f));

        yield return new WaitForSeconds(2f);

        introCamera.Priority = -50;

        yield return new WaitForSeconds(1f);

        ghostSprite.GetComponent<Animator>().SetTrigger("Play Intro");
        ballController.GetComponent<Animator>().SetTrigger("Play Intro");

        yield return new WaitForSeconds(3);

        AudioManager.instance.Play("pumpkin_ghost");

        yield return new WaitForSeconds(2);

        ballController.GetComponent<Animator>().enabled = false;
        ghostSprite.SetActive(false);

        if (GameObject.Find("Beaten the game") != null)
        {
            timer.GetComponent<TextMeshProUGUI>().enabled = true;
            collectablesText.GetComponent<TextMeshProUGUI>().enabled = true;
            //replayCanvas.SetActive(true);
        }

        CinemachineManualFreeLook.canMoveCamera = true;

        timer.StartTimer();
    }

    public void EndLevel()
    {
        timer.StopTimer();

        ballController.canMove = false;

        ballController.rb.isKinematic = true;

        ballController.GetComponent<SphereCollider>().enabled = false;

        //foreach (Collider c in GetComponents<Collider>()) {
        //    DestroyImmediate(c);
        //}

        DisableDebrisCollisions();

        endingScreenCamera.Priority = 100;

        StartCoroutine(TransitionToNextLevel());

        
    }

    private void DisableDebrisCollisions()
    {
        int debrisLayer = LayerMask.NameToLayer("Debris");

        if (debrisLayer == -1)
        {
            Debug.LogError("Layer 'Debris' not found. Make sure it exists in Project Settings > Tags and Layers.");
            return;
        }

        // Loop through all possible layers (Unity supports up to 31 layers)
        for (int i = 0; i < 32; i++) {
            if (i == debrisLayer) continue; // skip self to self collisions

            Physics.IgnoreLayerCollision(debrisLayer, i, true);
        }

        // Also disable debris-vs-debris collisions
        Physics.IgnoreLayerCollision(debrisLayer, debrisLayer, true);
    }

    private void EnableDebrisCollisions()
    {
        int debrisLayer = LayerMask.NameToLayer("Debris");

        if (debrisLayer == -1)
        {
            Debug.LogError("Layer 'Debris' not found. Make sure it exists in Project Settings > Tags and Layers.");
            return;
        }

        // Loop through all possible layers (Unity supports up to 31 layers)
        for (int i = 0; i < 32; i++)
        {
            if (i == debrisLayer) continue; // skip self to self collisions

            Physics.IgnoreLayerCollision(debrisLayer, i, false);
        }

        // Also disable debris-vs-debris collisions
        Physics.IgnoreLayerCollision(debrisLayer, debrisLayer, false);
    }

    IEnumerator TransitionToNextLevel()
    {
        Vector3 point = GameObject.Find("Level End Trigger").transform.position;

        ballController.transform.DOMove(point + Vector3.up*4f, 0.5f).SetEase(Ease.InExpo);

        yield return new WaitForSeconds(2f);

        ballController.transform.DOMove(GameObject.Find("Fall Point").transform.position + Vector3.down * 4f, 1.3f).SetEase(Ease.InOutBack);

        yield return new WaitForSeconds(1f);

        DoDash();

        AudioManager.instance.Play("explosion");

        yield return new WaitForSeconds(2f);

        StartCoroutine(FadeRoutine(1f));

        yield return new WaitForSeconds(1f);

        Destroy(ballController.gameObject);

        yield return new WaitForSeconds(.5f);

        GameObject.Find("Music").GetComponent<AudioSource>().Stop();

        yield return new WaitForSeconds(1.5f);

        if (currentLevel == 1) {

            StartCoroutine(HoursLater());

            yield return new WaitForSeconds(8f);

            SceneManager.LoadScene("Level 2");


        }
        else if (currentLevel == 2)
        {

            var a = new GameObject("Beaten the game");
            DontDestroyOnLoad(a);

            yield return null;
            yield return null;

            yield return new WaitForSeconds(7f);

            SceneManager.LoadScene("Level 1");

        }
        //else if (currentLevel == 3)
        //    SceneManager.LoadScene("Level 1");
    }

    public float normalFov = 80f;
    public float dashFov = 50f;
    public float fovDurationIn = 0.15f;
    public float fovDurationOut = 0.8f;

    public void DoDash()
    {
        DOVirtual.Float(normalFov, dashFov, fovDurationIn, v =>
        {
            endingScreenCamera.m_Lens.FieldOfView = v;
        }).SetEase(Ease.OutQuint).OnComplete(() =>
        {
            DOVirtual.Float(dashFov, normalFov, fovDurationOut, v =>
            {
                endingScreenCamera.m_Lens.FieldOfView = v;
            }).SetEase(Ease.OutSine);
        });
    }


    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / 1f);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    [SerializeField] private TextMeshProUGUI hoursLaterText = null;
    private IEnumerator HoursLater()
    {
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime;
            hoursLaterText.alpha = Mathf.Lerp(0f, 1f, time / 1f);
            yield return null;
        }

        time = 0f;

        while (time < 3.5f)
        {
            time += Time.deltaTime;
            yield return null;
        }

        time = 0f;

        while (time < 1.5f)
        {
            time += Time.deltaTime;
            hoursLaterText.alpha = Mathf.Lerp(1f, 0f, time / 1.5f);
            yield return null;
        }
    }
}
