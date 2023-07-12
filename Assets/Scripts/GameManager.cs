using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    [Header("Manager Scripts")]
    [SerializeField] public UIManager ui;
    [SerializeField] public PlayerController player;
    [SerializeField] public LevelManager lm;

    [Header("Progress Tracking")]
    //the following conditions are used to track the progress of the game's completion 
    //THIS IS ALL SUBJECT TO CHANGED!
    public int[] finishedLevels; //amont of levels finished
    public int[] unlockedWorlds; //what worlds are done.
    

    private void Awake()
    {
        // Locks the frame rate at a specified value
        QualitySettings.vSyncCount = 0; // VSync must be disabled.
        Application.targetFrameRate = 60;
    }

    /*
     * Ends the current level and transitions to the next one
     * 
     * @param toPosition - the position the player will be launched to after completing the level
     * @param launchSpeed - the speed the player will be launched at
     * @param sceneName - the name of the scene to tranision to
     */
    public void EndLevel(Transform toPosition, float launchSpeed, string sceneName)
    {
        // Ends the Level
        Debug.Log("Hit End Level 1");
        player.Pause(true);
        player.rb.velocity = Vector3.zero;
        Debug.Log("Hit End Level 2");
        StartCoroutine(playerEndLaunch(toPosition, launchSpeed));
        ui.FinalScoreInitialize(lm.levelTime, lm.score, lm.ptm.GetPercentage(), sceneName);
    }


    /*
     * Launches the player to a specific position for the end of the level
     * 
     * @param toPosition - the position to launch to
     * @param launchSpeed - the speed the player will launch at
     * @parm sceneName - the scene to transition to after done launching
     */
    private IEnumerator playerEndLaunch(Transform toPosition, float launchSpeed)
    {
        yield return new WaitForSeconds(1);
        float lerpInc = 0.0f;
        Vector3 startPosition = player.transform.position;
        while(true)
        {
            lerpInc += launchSpeed * Time.deltaTime;
            player.transform.position = Vector3.Lerp(startPosition, toPosition.position, lerpInc);
            yield return null;
        }
    }

    /*
     * Transitions to a new scene
     * 
     * @param sceneName - the scene to transition to
     */
    public void SceneTransition(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
