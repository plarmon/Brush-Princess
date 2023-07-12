using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PaintTile : MonoBehaviour
{
    [SerializeField] private MeshRenderer plane;
    [SerializeField] private Shader paintShader;
    [SerializeField] private Texture2D trailMap;
    [SerializeField] private Texture2D mainTexture;
    [SerializeField] private Texture2D trailTexture;
    [SerializeField] private Texture2D trailNormal;
    [SerializeField] private Texture2D mainNormal;
    [SerializeField] private LayerMask mask;
    [SerializeField] private Camera paintCam;
    [SerializeField] private int textureTiling;
    [SerializeField] private Color mainTextureColor;
    [SerializeField] private Color paintColor;
    [SerializeField] private int resolutionMult;
    [SerializeField] private int checkPointsPerRow;
    private PaintTileManager ptm;
    private bool alreadyEnabled = false;
    private RenderTexture rt;
    private bool inside = false;
    private PlayerController player;
    private Material paintMat;
    private bool playerIn = false;
    private float percentFilled = 0.0f;
    private Texture2D tex;
    private int textureWidth;
    private int textureHeight;
    private double totalPixels;
    private double filledPixels;

    private void Start()
    {
        // initializes the Paint Tile Manager and Player Controller objects
        ptm = GameObject.Find("PaintTileManager").GetComponent<PaintTileManager>();
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        plane = GetComponent<MeshRenderer>();
    }

    /*
     * Creates the material and applies the shader that will be on the tile
     */
    public void initialize(int index)
    {
        paintMat = new Material(paintShader);
        paintMat.name = "PaintMat" + index;
        paintMat.SetTexture("TrailMap", trailMap);
        paintMat.SetFloat("TrailRadius", 0.6f);
        paintMat.SetFloat("Height Multiplier", -0.04f);
        paintMat.SetTexture("MainTexture", mainTexture);
        paintMat.SetTexture("TrailTexture", trailTexture);
        paintMat.SetColor("MainTextureColor", mainTextureColor);
        paintMat.SetColor("TrailColor", paintColor);
        paintMat.SetFloat("TrailBlendingCap", 1.0f);
        paintMat.SetTexture("TrailNormal", trailNormal);
        paintMat.SetTexture("MainNormal", mainNormal);
        paintMat.SetFloat("TrailTiling", textureTiling);
        paintMat.SetFloat("TrailGloss", 0);
        paintMat.SetFloat("SnowGloss", 2);
        paintMat.enableInstancing = true;
        gameObject.GetComponent<Renderer>().material = paintMat;
    }

    /*
     * Enables the renderTexture for the tile which will track the paint trail
     */
    public void enable()
    {
        // Creates the render texture
        textureWidth = (int)(plane.bounds.size.x + Mathf.Abs(transform.rotation.z * Mathf.Rad2Deg) + 
                                Mathf.Abs(transform.rotation.y * Mathf.Rad2Deg * (transform.localScale.x - transform.localScale.z)));
        textureHeight = (int)(plane.bounds.size.z + Mathf.Abs(transform.rotation.x * Mathf.Rad2Deg));
        rt = new RenderTexture(textureWidth * resolutionMult, textureHeight * resolutionMult, 1, RenderTextureFormat.R16);
        rt.name = gameObject.name + " RenderTexture";
        paintCam.gameObject.SetActive(true);
        paintCam.targetTexture = rt;

        // Sets the texture in the shader program
        paintMat.SetTexture("TrailMap", paintCam.targetTexture);

        // makes sure it wont be enabled again
        alreadyEnabled = true;
        player.currTile = this;
    }
    
    /*
     * Disables the camera while the player is not on the tile
     */
    public void pause()
    {
        // paintCam.enabled = false;
        paintCam.gameObject.SetActive(false);
        player.currTile = null;
        ptm.ToggleActive(false);
    }
    
    /*
     * enables the camera while the player is on the tile
     */
    public void unPause()
    {
        if (alreadyEnabled)
        {
            paintCam.gameObject.SetActive(true);
            player.currTile = this;
        } else
        {
            enable();
        }
    }

    /*
     * When the player enters the tile, then enable the camera, or initialize the render texture
     */
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerIn = true;
            if (!alreadyEnabled)
            {
                enable();
            }
            else
            {
                unPause();
            }
            ptm.SetActiveTile(this);
        }
    }

    /*
     * When the player exits the tile, then pause the camera
     */
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerIn = false;
            pause();
        }
    }

    /*
     * Starts the couroutine which temporarily enables the tiles for a certain amount of time
     * 
     * @param time - the amount of time to enable the tile
     */
    public void StartTempEnable(float time)
    {
        StartCoroutine(TempEnable(time));
    }

    /*
     * Temporarily enables the tile for a certain amount of time to record a paint splash
     * 
     * @param time - the time in seconds to keep the tile enabled
     */
    public IEnumerator TempEnable(float time)
    {
        if (!alreadyEnabled)
        {
            enable();
        }
        else
        {
            unPause();
        }
        yield return new WaitForSeconds(time);
        if (!playerIn)
        {
            pause();
        }
    }

    /*
     * Returns the percent of the tile that's filled
     * 
     * @return the precent that's filled
     */
    public float GetPercentFilled()
    {
        return percentFilled;
    }

    /*
     * Checks how much of the tile is filled with paint
     */
    public void CheckPercentFilled()
    {
        StartCoroutine(CheckSnapshot());       
    }

    /*
     * Gets the percentage of the tile that is filled
     */
    private IEnumerator CheckSnapshot()
    {
        // Wait till the end of the frame so that we can create a texture2d of the render texture
        yield return new WaitForEndOfFrame();

        // Maybe make a copy of the trailMap texture to loop throuhg all pixels
        tex = new Texture2D(textureWidth * resolutionMult, textureHeight * resolutionMult, TextureFormat.R16, false);
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, textureWidth * resolutionMult, textureHeight * resolutionMult), 0, 0);
        tex.Apply();
        var colorData = tex.GetPixelData<Color32>(0);
        filledPixels = 0.0;
        totalPixels = 0.0;
        int rowWidth = (textureWidth * resolutionMult);

        // Loops throught each row of the texture
        for (int i = 0; i < colorData.Length; i += rowWidth)
        {
            // Selects a certain number of points in the row to check
            int rand = Random.Range(0, rowWidth / checkPointsPerRow);
            for (int n = 1; n <= checkPointsPerRow; n++)
            {
                if (i + (rowWidth / n) + rand < colorData.Length)
                {
                    // Checks if the given pixel is red (meaning it is painted over)
                    if (colorData[i + (rowWidth / n) + rand].r != 0)
                    {
                        // The pixel is red
                        filledPixels++;
                    }
                    totalPixels++;
                }
            }
        }
        // Sets the percent filled to the max value between the percent calculated and the previous recorded value
        percentFilled = Mathf.Max((float)(filledPixels / totalPixels), percentFilled);
    }
}
