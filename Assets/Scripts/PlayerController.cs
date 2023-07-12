using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class PlayerController : MonoBehaviour
{
    // Component and Object Vairables
    [Header("Object Variables")]
    [SerializeField] public Rigidbody rb;
    [SerializeField] private Transform shoulders;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private LayerMask ground;
    [SerializeField] public Player player;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject driftPaint;
    [SerializeField] private Transform spawnPoint;
    private ParticleSystem.EmissionModule driftPaintModule;
    private ParticleSystem.MainModule driftPaintMain;
    private GameManager gm;
    public Animator anim; // the character animator. lmao

    // Particle Effects
    [Header ("Particles")]
    [SerializeField] private ParticleSystem PaintTrail;
    [SerializeField] private GameObject landingEffectPrefab;

    // Basic Movement Variables
    [Header("Movement")]
    [SerializeField] private float maxWalkSpeed;
    [SerializeField] private float maxGlideSpeed;
    [SerializeField] private float speed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float dragForce;
    [SerializeField] private float playerCurrentSpeed; //the actual speed of the player gameObject.
    [HideInInspector]
    public bool paused = false;

    // Animation variables
    [Header("Animation")]
    [SerializeField] private Animator animator;

    // Jump Variables
    [Header("Jump")]
    private float currJumpForce = 0.0f;
    [SerializeField] private float jumpForceInc = 5.0f;
    [SerializeField] private float minJumpForce;
    [SerializeField] private float maxJumpForce;

    // Camera Variables
    [Header("Camera")]
    [SerializeField] private float lookSensitivity;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera mainCam;

    // Push variables
    [Header("Push")]
    [SerializeField] private int pushStartupTime;
    [SerializeField] private float pushForce;
    [SerializeField] private float pushDirOffset;
    [SerializeField] private float landingBoostForce;

    // Drift Variables
    [Header("Drift")]
    [SerializeField] private float driftForce;
    [SerializeField] private float driftSensitivity;
    [SerializeField] private int driftDir;
    [SerializeField] private float maxDriftTime;
    private float driftTime;

    // Ground Detection Variables
    [Header("Ground Detection")]
    private bool grounded;
    private Vector3 groundNormal;

    // Spinning Variables
    [Header("Spinning")]
    [SerializeField] int spinInputTime;
    [SerializeField] float maxSpinTime;
    private float prevSpinDir;
    private float spinTime;
    private Queue<Vector2> spinDirList; 
    private Vector2 spinVector;
    private int spinDirection;
    private int spinningHash;
    private int clockwiseHash;

    //Trick Variables
    [Header("Tricks")]
    [SerializeField] private float listRefreshRate;
    [SerializeField] private int trickListSize;
    private List<Trick> trickList;
    private int trickOneHash;
    private int trickTwoHash;
    private bool justGrinded;

    [HideInInspector]
    public PaintTile currTile;

    //Sound Variables
     [Header("Sound FX")]
     public AudioSource jumpSound;
     public AudioSource pushSound;
     public AudioSource landSound;

    //Pause Menu Variables
    [Header("Pause Menu")]
    [SerializeField] public GameObject pauseMenuUI;

    // State Variables
    [HideInInspector] 
    public enum States
    {
        IDLE = 0,
        PUSHING = 1,
        AIRBORN = 2,
        TWIRLING = 3,
        DRIFTING = 4,
        TRICKY = 5,
        GRINDING = 6
    };
    [HideInInspector] public States state;

    // Other Variables
    [Header("Other")]
    [SerializeField] private int playerID = 0;
    [SerializeField] private float halfHeight;

    #region Awake
    private void Awake()
    {
        // Initializes component & object variables
        rb = GetComponent<Rigidbody>();
        player = ReInput.players.GetPlayer(playerID);
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        driftPaintModule = driftPaint.GetComponent<ParticleSystem>().emission;
        driftPaintModule.enabled = false;
    }
    #endregion

    #region Start
    // Start is called before the first frame update
    void Start()
    {
        // Sets initial values for variables
        currJumpForce = minJumpForce;
        spinVector = Vector2.zero;
        spinDirList = new Queue<Vector2>(spinInputTime);
        prevSpinDir = 0;
        spinningHash = Animator.StringToHash("Spinning");
        clockwiseHash = Animator.StringToHash("Clockwise");
        state = States.IDLE;
        trickList = new List<Trick>(trickListSize);
        for(int i = 0; i < trickListSize; i++)
        {
            trickList.Add(new Trick());
        }
        StartCoroutine(CycleTrickList(listRefreshRate));
    }
    #endregion

    #region FixedUpdate
    private void FixedUpdate()
    {
        if (state != States.GRINDING)
        {
            // Sets the rotation of the player
            CheckRotation();
        } else
        {
            // cameraController.SetFreeze(false);
        }
    }
    #endregion

    #region Update
    private void Update()
    {
        CheckPlayerSpeed();
        TrailSize();

       if (!paused)
       {
            if (state != States.GRINDING)
            {
                // Sets spinVector to the current input of the player on the left joystick
                spinVector = new Vector2(player.GetAxis("Move Horizontal"), player.GetAxis("Move Vertical"));

                // Checks if the player is grounded
                grounded = CheckGrounded();

                // Sets drag force depending on if the player is grounded or not
                if (grounded)
                {
                    rb.drag = dragForce;
                    if (PaintTrail.isPaused)
                    {
                        PaintTrail.Play();
                    }
                }
                else
                {
                    rb.drag = 0;
                    if (PaintTrail.isPlaying)
                    {
                        PaintTrail.Pause();
                    }
                }

                // If the player is inputing something on the left stick then continue
                if (spinVector != Vector2.zero && (state == States.IDLE || state == States.AIRBORN || state == States.TWIRLING))
                {
                    // Gets the angle of the joystick
                    float angle = (spinVector.x > 0) ? Vector2.Angle(Vector2.up, spinVector) : 360 - Vector2.Angle(Vector2.up, spinVector);

                    // If the angle doesn't equal the same as the previous angle then add to the list
                    if (prevSpinDir != Mathf.Round(angle))
                    {
                        spinDirList.Enqueue(spinVector);
                        prevSpinDir = Mathf.Round(angle);
                    }

                    // Keeps the list of inputs at a set size
                    if (spinDirList.Count > spinInputTime) spinDirList.Dequeue();

                    // Check if the player has input a spin
                    int spinDirectionTemp = SpinCheck(ref spinDirList);

                    // If a spin was input and they are not currently spinning then start spinning
                    if (spinDirectionTemp != 0)
                    {
                        if (spinDirectionTemp == 1)
                        {
                            Trick cwtTrick = new Trick("Clockwise Twirl", 0.5f, 50.0f);
                            AddToEnd<Trick>(cwtTrick, ref trickList);
                            gm.ui.AddTrick(cwtTrick);
                        }
                        else if (spinDirectionTemp == -1)
                        {
                            Trick ccwtTrick = new Trick("Counter-Clockwise Twirl", 0.5f, 50.0f);
                            AddToEnd<Trick>(ccwtTrick, ref trickList);
                            gm.ui.AddTrick(ccwtTrick);
                        }
                        if (state != States.TWIRLING)
                        {
                            StartCoroutine(Spin(spinDirectionTemp));
                        }
                    }
                }
                else
                {
                    // Resent the spin input list
                    spinDirList = new Queue<Vector2>(spinInputTime);
                }

                // Push left
                if (player.GetButtonDown("Push Left") && state == States.IDLE)
                {
                    anim.SetTrigger("Push Left");
                    StartCoroutine(Push(-1, pushStartupTime));
                }
                // Push right
                else if (player.GetButtonDown("Push Right") && state == States.IDLE)
                {
                    anim.SetTrigger("Push Right");
                    StartCoroutine(Push(1, pushStartupTime));
                }

                // Halts the players velocity over time
                if (player.GetButtonSinglePressHold("Break") && grounded && state == States.IDLE)
                {
                    if (rb.velocity.magnitude > 0)
                    {
                        StartCoroutine(Break());
                        rb.AddForce(-rb.velocity * 2);
                        if (rb.velocity.magnitude < 0) rb.velocity = Vector3.zero;
                    }
                }

                // Adds to the jumpforce if the jump button is held
                if (player.GetButtonSinglePressHold("Jump") && grounded)
                {
                    if (currJumpForce <= maxJumpForce)
                    {
                        currJumpForce += jumpForceInc * Time.deltaTime;
                    }
                }

                // When the jump button goes up, add the built up jump force to the up direction
                if (player.GetButtonUp("Jump") && grounded)
                {
                    Jump(currJumpForce);
                }

                if (player.GetButtonDown("Trick1") && (state == States.AIRBORN || (state == States.TWIRLING && !grounded)))
                {
                    Trick trick1 = new Trick("ATrick1", 0.5f, 100.0f);
                    anim.SetTrigger("ATrick1");
                    AddToEnd<Trick>(trick1, ref trickList);
                    gm.ui.AddTrick(trick1);
                    StartCoroutine(PerformTrick(trick1));
                }

                if (player.GetButtonDown("Trick2") && (state == States.AIRBORN || (state == States.TWIRLING && !grounded)))
                {
                    Trick trick2 = new Trick("ATrick2", 0.5f, 100.0f);
                    anim.SetTrigger("ATrick2");
                    AddToEnd<Trick>(trick2, ref trickList);
                    gm.ui.AddTrick(trick2);
                    StartCoroutine(PerformTrick(trick2));
                }

                #region Animation Update
                anim.SetBool("Grounded", grounded);
                #endregion
            }
        }

        if (player.GetButtonDown("Game Pause"))
        {
            Pause(paused);
            pauseMenuUI.SetActive(paused);
        }
    }
    #endregion

    #region Jump Stuff
    /*
     * Makes the player jump with a given force
     * 
     * @param jumpForce - the force to make the player jump
     */
    public void Jump(float jumpForce)
    {
         jumpSound.Play();
        state = States.AIRBORN;
        // Adds trick to the trick list list
        Trick jumpTrick = new Trick("Jump", 0.0f, 20.0f);
        AddToEnd<Trick>(jumpTrick, ref trickList);
        gm.ui.AddTrick(jumpTrick);
        Vector3 jumpDir;
        if (rb.velocity.magnitude != 0)
        {
            jumpDir = (Vector3.up * jumpForce) + (transform.forward * (jumpForce / 3));
        }
        else
        {
            jumpDir = Vector3.up * jumpForce;
        }
        rb.AddForce(jumpDir);
        currJumpForce = minJumpForce;
        StartCoroutine(JumpCam());
    }

    /*
     * Sets the distance of the camera from the player based on the players distance from the ground.
     */
    IEnumerator JumpCam()
    {
        // Gets the origianl distance of the camrea from the player
        float originalDistance = mainCam.GetCinemachineComponent<Cinemachine.Cinemachine3rdPersonFollow>().CameraDistance;

        while (grounded)
        {
            yield return null;
        }
        while (!grounded)
        {
            // Gets the distance of the player from the ground
            float distance;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit, 100, ground))
            {
                // increases the distance of the camera from the player if the distance to the ground is greater
                float currentDistance = mainCam.GetCinemachineComponent<Cinemachine.Cinemachine3rdPersonFollow>().CameraDistance;
                distance = Mathf.Lerp(currentDistance, (hit.distance / 3.0f) + originalDistance, 0.1f);
                mainCam.GetCinemachineComponent<Cinemachine.Cinemachine3rdPersonFollow>().CameraDistance = distance;
            }
            yield return null;
        }
        // Sets the camera distance back to its original values
        mainCam.GetCinemachineComponent<Cinemachine.Cinemachine3rdPersonFollow>().CameraDistance = originalDistance;
    }
    #endregion

    /*
     * Pauses or unpauses the game
     */
    #region Pause
    public void Pause(bool pause)
    {
        paused = pause;

        if (paused)
        {
            Debug.Log("Hit pause 1");
            // pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            paused = false;
            Debug.Log("Hit pause 2");
        }
        else
        {
            //StopAllCoroutines();
            // pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            paused = true;
        }
    }
    #endregion

    /* Coroutine for pushing the character in a specified direction over a certain period of time
    * 
    * @param foot - Determines which direction the push will be in
    * @param pushStartupTime - Sets how long it will take to get to max speed in the push
    */
    #region Push
    private IEnumerator Push(int foot, int pushStartupTime)
    {
         pushSound.Play();
        state = States.PUSHING;
        driftDir = foot;
        float currPushForce = pushForce / pushStartupTime;
        while (currPushForce < pushForce && grounded)
        {
            if (rb.velocity.magnitude <= maxGlideSpeed)
            {
                // Gets the perpendicular vector from the normal vector of the ground
                Vector3 slopeDir = Vector3.Cross(groundNormal, -transform.right);
                Vector3 pushDir = slopeDir + (transform.right * pushDirOffset * foot);

                // Adds force in the push direction
                rb.AddForce(pushDir * currPushForce, ForceMode.Acceleration);

                // Increments the push force
                currPushForce += (pushForce / pushStartupTime);
            }
            yield return null;
        }

        // Drifting
        state = States.DRIFTING;
        driftTime = maxDriftTime;
        // GameObject driftPaintInst = Instantiate(driftPaint, PaintTrail.gameObject.transform);
        // driftPaintModule = driftPaintInst.GetComponent<ParticleSystem>().emission;
        driftPaintModule.enabled = true;
        driftPaint.transform.right = -transform.forward;
        while((player.GetButtonSinglePressHold("Push Right") || player.GetButtonSinglePressHold("Push Left")) && driftTime > 0 && player.GetButtonSinglePressHold("Break"))
        {
            // Gets the perpendicular vector from the normal vector of the ground
            Vector3 slopeDir = Vector3.Cross(groundNormal, -transform.right);
            Vector3 pushDir = slopeDir + (transform.right * pushDirOffset * foot);

            // Adds force in the drift direction
            rb.AddForce(pushDir * driftForce, ForceMode.Acceleration);

            // Increments the driftTime varaible
            driftTime -= 1 * Time.deltaTime;
            yield return null;
        }
        // Sets drifting to false
        driftPaintModule.enabled = false;
        driftDir = 0;
        state = States.IDLE;
    }
    #endregion

    /*
    * Spins the player in either the clockwise or counter clockwise direction
    * 
    * @param direction details which direction the player is spinning in [clockwise(1), counter-clockwise(-1)]
    */
    #region Spin
    private IEnumerator Spin(int direction)
    {
        state = States.TWIRLING;
        spinDirection = direction;

        // Lets the animator know the player is spinning
        anim.SetBool(spinningHash, true);

        // Clockwise Spin
        if (direction == 1)
        {
            anim.SetBool(clockwiseHash, true);
        }
        // Counter-clockwise Spin
        else
        {
            anim.SetBool(clockwiseHash, false);
        }

        spinTime = maxSpinTime;

        // While the player keeps spinning then spin
        while (spinTime > 0)
        {
            // This was some code trying to make the twirling happen by rotating the player model
            // The problem with this is that it will mess up all the other animations

            // float angle = (spinTime / maxSpinTime) * 360;
            // if(direction == 1)
            // {
            //     playerModel.transform.Rotate(0, (360 / maxSpinTime) * Time.deltaTime, 0, Space.Self);
            // } else if (direction == -1)
            // {
            //     playerModel.transform.Rotate(0, (-360 / maxSpinTime) * Time.deltaTime, 0, Space.Self);
            // }

            // incrementally decrease the time left on the spin
            spinTime -= 1 * Time.deltaTime;
            yield return null;
        }
        // This was also for rotating the player model
        // playerModel.transform.rotation = shoulders.rotation;

        spinTime = maxSpinTime;
        spinDirection = 0;
        animator.speed = 1;
        anim.SetBool(spinningHash, false);
        state = States.IDLE;
    }
    #endregion

    /*
     * Checks if the player is spinning in a clockwise or counter clockwise direction based on a list of the joystick inputs
     * 
     * @param spinDirList a reference to the list of inputs from the left joystick
     * 
     * @return an integer representing spinning in the clockwise (1), counter-clockwise (-1), or neither (0) direction
     */
    #region SpinCheck
    private int SpinCheck(ref Queue<Vector2> spinDirList)
    {
        // spinCheck represents the 4 regions the joystick has to pass through in order to be considered a spin
        List<bool> spinCheck = new List<bool>() { false, false, false, false };
        int checkCount = 0;

        // the current min an max angle range for a certain section
        int minRange = 0;
        int maxRange = 90;

        // Clockwise Check
        foreach (Vector2 dir in spinDirList)
        {
            // Gets the angle of the joystick input
            float angle = (dir.x > 0) ? Vector2.Angle(Vector2.up, dir) : 360 - Vector2.Angle(Vector2.up, dir);

            // If it's within the region then continue
            if (angle >= minRange && angle < maxRange)
            {
                // Check a region in the spinCheck list
                spinCheck[checkCount] = true;
                checkCount++;

                // Increment the angle range
                minRange += 90;
                maxRange += 90;
            }
            // If all regions are checked then retun clockwise
            if (!spinCheck.Contains(false))
            {
                // Set the spin time to the max if either not spinning or spinning clockwise
                if (spinDirection != -1)
                {
                    spinTime = maxSpinTime;
                }
                // Reset the list so it doesn't read the same values again
                spinDirList = new Queue<Vector2>(spinInputTime);
                return 1;
            }
        }

        // Reset variables for checking counter-clockwise input
        minRange = 270;
        maxRange = 360;
        spinCheck = new List<bool>() { false, false, false, false };
        checkCount = 0;

        // Counter-Clockwise Check
        foreach (Vector2 dir in spinDirList)
        {
            // Gets the angle of the joystick input
            float angle = (dir.x > 0) ? Vector2.Angle(Vector2.up, dir) : 360 - Vector2.Angle(Vector2.up, dir);

            // If it's within the region then continue
            if (angle >= minRange && angle < maxRange)
            {
                // Check a region in the spinCheck list
                spinCheck[checkCount] = true;
                checkCount++;

                // Increment the angle range
                minRange -= 90;
                maxRange -= 90;
            }
            // If all regions are checked then retun counter-clockwise
            if (!spinCheck.Contains(false))
            {
                // Set the spin time to the max if either not spinning or spinning counter-clockwise
                if (spinDirection != 1)
                {
                    spinTime = maxSpinTime;
                }
                // Reset the list so it doesn't read the same values again
                spinDirList = new Queue<Vector2>(spinInputTime);
                return -1;
            }
        }
        return 0;
    }
    #endregion

    #region Grounded Functions
    /*
    * Determines if the player is grounded or not and returns the results
    * 
    * @return whether the player is grounded
    */
    private bool CheckGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, halfHeight, ground))
        {
            Vector3 prevGround = groundNormal;
            groundNormal = hit.normal;

            if(!grounded)
            {
                state = States.IDLE;
                if (state != States.TWIRLING)
                {
                    Vector3 slopeDir = Vector3.Cross(groundNormal, -transform.right);

                    // Landing Boost
                    landSound.Play();
                    rb.AddForce(slopeDir * landingBoostForce, ForceMode.Impulse);

                    // Landing Effect Instantiation
                    GameObject landingInstance = Instantiate(landingEffectPrefab, transform.position, Quaternion.Euler(Vector3.forward));
                    landingInstance.transform.up = transform.up;
                    Cinemachine.CinemachineImpulseSource source = landingInstance.GetComponent<Cinemachine.CinemachineImpulseSource>();
                    source.GenerateImpulse(Camera.main.transform.forward);
                } else
                {
                    // Crash
                    rb.velocity /= 2;
                }
            }
            
            return true;
        } else
        {
            if (state == States.IDLE)
            {
                state = States.AIRBORN;
            }
            return false;
        }
    }

    /*
    * Gets if the player is grounded or not
    * 
    * @return boolean value of if the player is grounded
    */
    public bool GetGrounded()
    {
        return grounded;
    }
    #endregion

    /*
    * Sets the rotation of the Player
    */
    #region CheckRotation
    private void CheckRotation()
    {
        if (state != States.DRIFTING)
        {
            // Gets horizontal look input from the player
            float lookHorizontal = player.GetAxis("LookHorizontal");
            transform.Rotate(0, lookHorizontal * lookSensitivity, 0, Space.Self);
        } else
        {
            if (driftTime > 0)
            {
                transform.Rotate(0, driftDir * driftSensitivity, 0, Space.Self);
            }
        }
    }
    #endregion

    /*
     * Stops the player in their place and starts the beark particle effect
     */
    #region Break
    IEnumerator Break()
    {
        driftPaint.transform.right = transform.forward;
        driftPaintModule.enabled = true;
        while(!(rb.velocity.magnitude > -0.1 && rb.velocity.magnitude < 0.1))
        {
            yield return null;
        }
        driftPaintModule.enabled = false;
    }
    #endregion

    /*
     * Respawns the player at a given respawn point
     * 
     * @param respawnPoint - the point the player should be respawned at
     */
    #region Respawn
    public void Respawn(Transform respawnPoint)
    {
        Debug.Log("Respawn");
        state = States.IDLE;
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
        gm.ui.StartCoroutine(gm.ui.FadeIn());
    }
    #endregion

    /*
     * Performs a certain trick
     * 
     * @param trick - the trick to be performed
     */
    #region PerformTrick
    IEnumerator PerformTrick(Trick trick)
    {
        gm.lm.AddToScore(trick.GetPoints());
        float trickTimer = 0;
        state = States.TRICKY;
        while (trickTimer < trick.GetTime())
        {
            trickTimer += Time.deltaTime;
            yield return null;
        }
        state = (grounded) ? States.IDLE : States.AIRBORN;
        if(grounded)
        {
            state = States.IDLE;
        } else
        {
            state = States.AIRBORN;
        }
    }
    #endregion

    /*
     * Adds an element to the end of a list and shifts everything ofer towards the front of the List.
     * This method is used to maintain the local list of tricks that are being performed and will come in
     * handy when we start dealing with combo moves
     * 
     * @param value the element that is being added to the back of the list
     * @param list the list that the element is being pushed to
     */
    #region AddToEnd
    private void AddToEnd<T>(T value, ref List<T> list)
    {
        T prevValue = list[list.Count - 1];
        T currValue = prevValue;
        list[list.Count - 1] = value;
        for(int i = list.Count - 2; i >= 0; i--)
        {
            currValue = list[i];
            list[i] = prevValue;
            prevValue = currValue;
        }
    }
    #endregion

    /*
     * Cycles through the local list of tricks being performed at a defined refresh rate. Once the refresh
     * rate is hit with the defined timer then all elements are cycled towards the front of the list. This
     * method doesn't achieve anything in game at the moment but will be usefull when we start doing combo
     * moves.
     * 
     * @param refreshTime the rate at which the list is refreshed
     */
    #region CycleTrickList
    IEnumerator CycleTrickList(float refreshTime)
    {
        float refreshTimer = refreshTime;
        while (true)
        {
            if (refreshTimer <= 0)
            {
                Trick t = new Trick();
                AddToEnd<Trick>(t,ref trickList);
                refreshTimer = refreshTime;
            }
            else
            {
                refreshTimer -= Time.deltaTime;
            }
            yield return null;
        }
    }
    #endregion

    /*
     * Get the rigidbody velocity and save it to a variable. 
     * set the a speed variable to the velocity. mangitude.
     * Use this new data to tell adjust the scale of the trail paritlces to make the trail bigger.
     */
    #region Player Speed Tracker
    public void CheckPlayerSpeed()
    {
        var vel = rb.velocity;
        playerCurrentSpeed = vel.magnitude;
    }

    public void TrailSize()
    {
        if (playerCurrentSpeed > 0) {
            PaintTrail.startSize = playerCurrentSpeed / 1f;
        }
       //if (playerCurrentSpeed >= 20)
       //    PaintTrail.startSize = 20f;
    }

    #endregion

    /*
     * Completes the grind of the player by setting them to the correct state and pushes them off the rail
     * 
     * @param forceDir - the direction the player should be pushed off of the rail
     */
    #region Finishing a Grind
    public void CompleteGrind(Vector3 forceDir)
    {
        rb.useGravity = true;
        if (grounded)
        {
            state = PlayerController.States.IDLE;
        }
        else
        {
            state = PlayerController.States.AIRBORN;
        }

        // Add Force
        rb.AddForce(forceDir * 15.0f, ForceMode.Impulse);
        StartCoroutine(FinishGrind());
    }

    /*
     * Makes sure that the player doesn't start grinding on the smae rail right after the grind is completed
     */
    private IEnumerator FinishGrind()
    {
        justGrinded = true;
        yield return new WaitForSeconds(1);
        justGrinded = false;
    }

    /*
     * Returns if the player just finished grinding
     */
    public bool GetJustGrinded()
    {
        return justGrinded;
    }
    #endregion

    

    /*
     * Prints all of the vcalues in trickList
     */
    // private void PrintList()
    // {
    //     string output = "";
    //     foreach(Trick t in trickList)
    //     {
    //         output += t.GetName() + " ";
    //     }
    //     Debug.Log(output + " Count: " + trickList.Count);
    // }

}
