using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Object Variables")]
    [SerializeField] private PlayerController player;
    private GameManager gm;

    // Bounce variables
    [Header("Text Bounce")]
    [SerializeField] private float bounceSpeed;
    [SerializeField] private float bounceDamper;

    // Trick list variables
    [Header("Trick List")]
    [SerializeField] private float listRefreshRate;
    [SerializeField] private int trickListLength;
    private List<string> trickList;
    private float listTime;

    [Header("Blackout Variables")]
    [SerializeField] private Image blackout;
    [SerializeField] private float blackoutInc;

    [Header("Score Variables")]
    [SerializeField] public TextMeshProUGUI scoreText;
    [SerializeField] public TextMeshProUGUI scoreTextLabel;
    [SerializeField] public TextMeshProUGUI timeText;
    [SerializeField] public TextMeshProUGUI timeTextLabel;

    [Header("Final Score Variables")]
    [SerializeField] private GameObject finalScorePanel;
    [SerializeField] private Text timeScore;
    [SerializeField] private Text percentageFilled;
    [SerializeField] private Text percentageScore;
    [SerializeField] private Text trickScore;
    [SerializeField] private Text totalScore;
    [SerializeField] private Text letterGrade;
    private bool endOfLevel = false;
    private string nextScene;

    [Header("Grind Variables")]
    [SerializeField] private GameObject balance;
    [SerializeField] private GameObject balanceTracker;
    [SerializeField] private float falloffPoint;

    // UI variables
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI trickText;
    [SerializeField] private GameObject outOfTimePanel;
    private Vector3 originalScale;

    private bool outOfTime = false;

    private void Awake()
    {
        // Initialize trickList 
        trickList = new List<string>();
        originalScale = trickText.transform.localScale;
        UpdateScore(0);

        // Initialize PlayerController
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    private void Update()
    {
        if(endOfLevel)
        {
            if (player.player.GetButtonDown("Jump"))
            {
                StartCoroutine(FadeOutScene(nextScene));
            }
        }
    }

    /*
     * Updates the ui element which displays the score
     */
    public void UpdateScore(float score)
    {
        scoreText.text = Mathf.Round(score).ToString();
    }

    /*
    * Updates the ui element which displays the time
    */
    public void UpdateTime(float time)
    {
        timeText.text = Mathf.Round(time).ToString();
        if(time <= 0 && !endOfLevel)
        {
            outOfTimePanel.SetActive(true);
            nextScene = gm.lm.levelName;
            endOfLevel = true;
            gm.player.Pause(false);
            outOfTime = true;
        }
    }

    /*
     * A public method to add a trick to the trickList
     * 
     * @param the name of the trick being added to the list
     */
    public void AddTrick(Trick trick)
    {
        // Add trick to trick list
        trickList.Add(trick.GetName());

        // Reset the listTime varaible
        listTime = listRefreshRate;

        // Stops the coroutine if it's already executing then starts it again
        StopAllCoroutines();
        trickText.transform.localScale = originalScale;
        StartCoroutine(DisplayTricks());
    }

    /*
     * Coroutine for displaying the current list of tricks
     */
    IEnumerator DisplayTricks()
    {
        trickText.text = "";
        // Adds all tricks in the list to the text
        for(int i = 0; i < trickListLength; i++)
        {
            if (i < trickList.Count)
            {
                // Add trick to the text
                trickText.text += (i != trickList.Count - 1) ? trickList[i] + " + " : trickList[i];
            }
        }
        // initializes the bounce timer
        float bounceTimer = 0;

        // increment bounceTimer up to PI
        while (bounceTimer < Mathf.PI)
        {
            // Modify the scale of the text box to bounce;
            trickText.transform.localScale = originalScale * ((Mathf.Sin(bounceTimer) / bounceDamper) + 1);

            // Increment the bounceTimer variable
            bounceTimer += bounceSpeed * Time.deltaTime;
            yield return null;
        }

        // Reset the scale of the text box
        trickText.transform.localScale = originalScale;

        // Keeps the text displayed until timer runs out
        while (listTime > 0)
        {
            // Increment the listTime variable
            listTime -= Time.deltaTime;
            yield return null;
        }

        // Reset the text value and the list
        trickText.text = "";
        trickList = new List<string>();
    }
    
    /*
     * Plays the fade in animation
     */
    public IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(1);
        float blackoutAlpha = blackout.color.a;
        while (blackoutAlpha > 0)
        {
            blackoutAlpha -= blackoutInc;
            blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, blackoutAlpha);
            yield return null;
        }
        blackoutAlpha = 0;
        blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, blackoutAlpha);
        if (player.paused)
        {
            player.Pause(false);
        }
    }

    /*
     * Plays the fade out animation and transitions to a given scene
     * 
     * @param sceneName - the name of the scene to transition to
     */
    public IEnumerator FadeOutScene(string sceneName)
    {
        float blackoutAlpha = blackout.color.a;
        while (blackoutAlpha < 1)
        {
            blackoutAlpha += blackoutInc;
            blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, blackoutAlpha);
            yield return null;
        }
        blackoutAlpha = 1;
        blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, blackoutAlpha);
        gm.SceneTransition(sceneName);
    }

    /*
     * Plays the fade out animation and respawns the player to a given point
     * 
     * @param respawnPoint - the point to respawn the player to
     */
    public IEnumerator FadeOutRespawn(Transform respawnPoint)
    {
        float blackoutAlpha = blackout.color.a;
        while(blackoutAlpha < 1)
        {
            blackoutAlpha += blackoutInc;
            blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, blackoutAlpha);
            yield return null;
        }
        blackoutAlpha = 1;
        blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, blackoutAlpha);
        player.Respawn(respawnPoint);
        yield return null;
    }

    /*
     * Shows the balance bar or not
     * 
     * @param display - determines whether to display it or not
     */
    public void DisplayGrind(bool display)
    {
        balance.SetActive(display);
    }

    /*
     * Updates the position of the baalance tracker in the balance bar
     */
    public void UpdateBalance(float balance)
    {
        // Updates the balance UI
        balanceTracker.transform.localPosition = new Vector3(falloffPoint * balance, balanceTracker.transform.localPosition.y, balanceTracker.transform.localPosition.z);
    }

    public void FinalScoreInitialize(float timeRemaining, float trickScore, float percentFilled, string sceneName)
    {
        scoreText.gameObject.SetActive(false);
        scoreTextLabel.gameObject.SetActive(false);
        timeText.gameObject.SetActive(false);
        timeTextLabel.gameObject.SetActive(false);

        timeScore.text = (timeRemaining * 10).ToString();
        this.trickScore.text = Mathf.Round(trickScore).ToString();
        this.percentageFilled.text = (Mathf.Round(percentFilled * 1000) / 10).ToString();
        float percentScore = percentFilled * 10000;
        this.percentageScore.text = Mathf.Round(percentScore).ToString();
        float totalScore = (timeRemaining * 10) + trickScore + percentScore;
        this.totalScore.text = Mathf.Round(totalScore).ToString();

        if(totalScore > 5000)
        {
            letterGrade.text = "S";
        } else if(totalScore > 4000)
        {
            letterGrade.text = "A";
        }
        else if(totalScore > 3000)
        {
            letterGrade.text = "B";
        }
        else if(totalScore > 2000)
        {
            letterGrade.text = "C";
        }
        else if(totalScore > 1000)
        {
            letterGrade.text = "D";
        }
        else
        {
            letterGrade.text = "F";
        }

        finalScorePanel.SetActive(true);

        nextScene = sceneName;
        endOfLevel = true;
    }
}
