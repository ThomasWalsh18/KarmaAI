using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void loadMainScene()
    {
        SceneManager.LoadScene("Karma");
    }
    public void loadInstuctions()
    {
        SceneManager.LoadScene("Instructions");
    }

    public void loadAIWin()
    {
        SceneManager.LoadScene("AIWon");
    }

    public void loadPlayerWin()
    {
        SceneManager.LoadScene("PlayerWon");
    }
}
