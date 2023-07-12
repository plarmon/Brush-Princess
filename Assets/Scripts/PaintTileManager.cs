using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintTileManager : MonoBehaviour
{
    [SerializeField] private GameObject[] tileList;
    [SerializeField] private GameObject[] doorObjectList;
    private GameManager gm;
    private List<PaintTile> paintTileList;
    private List<PaintDoor> doorList;
    private PaintTile currentTile;
    private bool active;
    private int tileCount = 0;
    private float currPercentFilled = 0.0f;

    private void Awake()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        // Initializes lists
        paintTileList = new List<PaintTile>();
        doorList = new List<PaintDoor>();

        // Gets a list of all the paint tiles
        tileList = GameObject.FindGameObjectsWithTag("PaintTile");

        // Gets a list of all the paint doors
        doorObjectList = GameObject.FindGameObjectsWithTag("PaintDoor");
    }

    private void Start()
    {
        // initializes all of the paint tiles
        foreach (GameObject pt in tileList)
        {
            PaintTile tile = pt.GetComponent<PaintTile>();
            tile.initialize(tileCount);
            paintTileList.Add(tile);
            tileCount++;
        }

        // initializes all of the paint doors
        foreach (GameObject go in doorObjectList)
        {
            PaintDoor pd = go.GetComponent<PaintDoor>();
            doorList.Add(pd);
        }
        StartCoroutine(CheckPercentage());
    }

    /*
     * Sets the current Tile active to a specified PaintTile
     * 
     * @param pt - the paintTile to set the currentTile to
     */
    public void SetActiveTile(PaintTile pt)
    {
        currentTile = pt;
        ToggleActive(true);
    }

    /*
     * Toggles the active variable to a certain values
     * 
     * @param status - the value to set active to
     */
    public void ToggleActive(bool status)
    {
        active = status;
    }

    private void Update()
    {
        // Updates how much of all of the tiles in the scene are filled
        float checkPercentSum = 0.0f;
        foreach (PaintTile pt in paintTileList)
        {
            checkPercentSum += pt.GetPercentFilled();
        }
        currPercentFilled = Mathf.Max(currPercentFilled, checkPercentSum / tileCount);

        // Checks if any doors should open
        if (doorList.Count > 0)
        {
            foreach (PaintDoor pd in doorList)
            {
                if (currPercentFilled > pd.GetLimit() && !pd.opened)
                {
                    // Opens the given door
                    pd.OpenDoor();
                    gm.player.Pause(true);
                }
            }
        }
    }

    /*
     * Checks the percentage of the active tile on a 1 second interval
     */
    private IEnumerator CheckPercentage()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (active && currentTile != null)
            {
                // Check how much of the current tile is active
                currentTile.CheckPercentFilled();
            }
        }
    }

    public float GetPercentage()
    {
        return currPercentFilled;
    }
}
