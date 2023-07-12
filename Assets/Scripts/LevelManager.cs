using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Level Information")]
    public string levelName;
    public int finished = 0; //if the finished value is 0 the level has not been completeded. If 1 it is. 

    [Space(2)]
    [Header("Gameplay Stuff")]
    [SerializeField] public PaintTileManager ptm;
    public float score = 0; //this goes up by doing tricks and moving
    public float levelTime; //count down timmer for the level
    private float originalFogValue = 0.0f;

    [Space(2)]
    [Header("Reference Stuff")]
    [SerializeField] private PlayerController princess = null; //need this for some conditions
    [SerializeField] private GameManager theGMan = null; //need this for checking level completion stuff. 

    private void Awake()
    {
        theGMan = GameObject.Find("GameManager").GetComponent<GameManager>();
        if(ptm == null)
        {
            ptm = GameObject.Find("PaintTileManager").GetComponent<PaintTileManager>();
        }

        switch (RenderSettings.fogMode)
        {
            case FogMode.Linear:
                originalFogValue = RenderSettings.fogEndDistance;
                break;
            case FogMode.Exponential:
                originalFogValue = RenderSettings.fogDensity;
                break;
            case FogMode.ExponentialSquared:
                originalFogValue = RenderSettings.fogDensity;
                break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        theGMan.player.Pause(true);
        StartCoroutine(countDown());
        theGMan.ui.UpdateScore(score);
    }

    // Update is called once per frame
    void Update()
    {
        #region Player related stuff
        if (princess.state == PlayerController.States.TWIRLING || princess.state == PlayerController.States.GRINDING)
        {
            score += 20f * Time.deltaTime; //add the time score stuff
            theGMan.ui.UpdateScore(score);
        }
        #endregion

        switch (RenderSettings.fogMode) {
            case FogMode.Linear:
                RenderSettings.fogEndDistance = originalFogValue * (1 - ptm.GetPercentage());
                break;
            case FogMode.Exponential:
                RenderSettings.fogDensity = originalFogValue * (1 - ptm.GetPercentage());
                break;
            case FogMode.ExponentialSquared:
                RenderSettings.fogDensity = originalFogValue * (1 - ptm.GetPercentage());
                break;
        }
    }

    public void AddToScore(float points)
    {
        Debug.Log("points : " + points);
        score += points;
        theGMan.ui.UpdateScore(score);
    }

    #region Level Count Down
    IEnumerator countDown()
    {
        theGMan.ui.UpdateTime(Mathf.Max(levelTime, 0));
        yield return new WaitForSeconds(1f);
        levelTime -= 1f;
        StartCoroutine(countDown());

    }
    #endregion

}
