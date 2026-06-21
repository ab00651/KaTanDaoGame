  using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Levelloader : MonoBehaviour
{
    public Animator transition;

    public float transitionTime = 1f;
    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetMouseButtonDown(0)) {
            LoadNextLevel();
        }*/
    }

    private int targetLevelIndex;

    public void LoadNextLevel()
    {
        targetLevelIndex = SceneManager.GetActiveScene().buildIndex + 1;
        MapVisualizer.LoadFromSaveOnStart = false;
        transition.SetTrigger("End");
        Invoke("DoLoadLevel", transitionTime);
    }

    void DoLoadLevel()
    {
        SceneManager.LoadScene(targetLevelIndex);
    }
}

