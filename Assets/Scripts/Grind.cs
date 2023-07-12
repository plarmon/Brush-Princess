using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grind : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private float distCutoff;
    [SerializeField] private float turnSpeed;
    [SerializeField] private float balanceInc;
    private GameManager gm;
    private GrindPoint currentPoint;
    private float setGrindSpeed;
    private float balance = 0;
    private bool reverse = false;

    private void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    /*
     * Initializes and starts a players grind
     * 
     * @param start - the transform from where the player starts the grind
     * @param to - the grind point the player is grinding to
     * @param grindSpeed - determines whether particle effects are playing for this grind
     */
    #region StartGrind
    public void StartGrind(Transform start, GrindPoint to, float grindSpeed)
    {
        // Determine which direction to grind in
        if(to.GetPrevPoint() == null && to.GetNextPoint() != null)
        {
            reverse = false;
        } else if(to.GetNextPoint() == null && to.GetPrevPoint() != null)
        {
            reverse = true;
        } else
        {
            // If it's not the start or end point then firure out the closest point based on direction
            Vector3 dirToPrev = Vector3.Normalize(to.GetPrevPoint().GetPoint().position - start.position);
            Vector3 dirToNext = Vector3.Normalize(to.GetNextPoint().GetPoint().position - start.position);
            float prevAngle = Vector3.Angle(transform.forward, dirToPrev);
            float nextAngle = Vector3.Angle(transform.forward, dirToNext);

            if(nextAngle <= prevAngle)
            {
                reverse = false;
            } else
            {
                reverse = true;
            }
        }

        // Sets the initial value of your balance
        balance = 0;
        gm.ui.DisplayGrind(true);

        // Changes the players state
        player.state = PlayerController.States.GRINDING;

        // Starts the grind with the correct speed
        setGrindSpeed = grindSpeed;
        StartCoroutine(Grinding(start, to, 5.0f, false));
    }
    #endregion

    /*
     * Continues a grind to the next grind point
     * 
     * @param start - the transform from where the player starts the grind
     * @param to - the grind point the player is grinding to
     * @param grindsSpeed - the speed the player should grind th rail
     * @param effects - determines whether particle effects are playing for this grind
     */
    #region NextGrind
    private void NextGrind(Transform start, GrindPoint to, float grindSpeed, bool effects)
    {
        player.state = PlayerController.States.GRINDING;
        StartCoroutine(Grinding(start, to, grindSpeed, effects));
    }
    #endregion

    /*
     * Enumerator for grinding which runs while the player is grinding
     * 
     * @param start - The transform from where the player starts the grind
     * @param to - The grind point the player is grinding to
     * @param grindSpeed - the speed the player should grind the rail
     * @param effects - determines whether particle effects are playing for this grind
     */
    #region Grinding
    public IEnumerator Grinding(Transform start, GrindPoint to, float grindSpeed, bool effects)
    {
        // If the current grind point has effects then play them
        if (effects)
        {
            currentPoint = to.GetPrevPoint();
            currentPoint.PlayEffect();
        }

        // Changes some values in the players rigidbody
        player.gameObject.GetComponent<Rigidbody>().useGravity = false;
        player.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

        // Variable initialization
        Vector3 startPosition = start.position;
        float lerpInc = 0.0f;

        // While the player is a certain distance away from the next grind point, continue
        while (Vector3.Magnitude(transform.position - to.GetPoint().position) > distCutoff) {
            // Modifies the balance
            balance += to.GetBalanceModif() * Time.deltaTime;
            gm.ui.UpdateBalance(balance);

            // Lerps the position of the player
            lerpInc = ((Time.deltaTime * grindSpeed) + lerpInc) % 1f;
            transform.position = Vector3.Lerp(startPosition, to.GetPoint().position, lerpInc);

            if (!reverse)
            {
                // Rotates the player towards the rotation of the current grindpoint if it exists
                if (to.GetPrevPoint() != null)
                {
                    float rotateTo = Mathf.LerpAngle(transform.rotation.eulerAngles.y, to.GetPrevPoint().GetPoint().eulerAngles.y, turnSpeed);
                    transform.localEulerAngles = new Vector3(transform.rotation.x, rotateTo, transform.rotation.z);
                }
            } else
            {
                if (to.GetNextPoint() != null)
                {
                    float rotateTo = Mathf.LerpAngle(transform.rotation.eulerAngles.y, to.GetNextPoint().GetPoint().eulerAngles.y + 180, turnSpeed);
                    transform.localEulerAngles = new Vector3(transform.rotation.x, rotateTo, transform.rotation.z);
                }
            }

            // If effects are present then set the position of the particle effect ot the position of the player on the grind rail
            if (effects)
            {
                to.GetPrevPoint().grindEffect.transform.position = new Vector3(transform.position.x, to.GetPrevPoint().grindEffect.transform.position.y, transform.position.z);
            }

            yield return null;
        }

        // If there were effects then pause the effects
        if (effects)
        {
            to.GetPrevPoint().PauseEffect();
        }

        if (!reverse)
        {
            if (to.GetNextPoint() != null)
            {
                NextGrind(transform, to.GetNextPoint(), setGrindSpeed, to.GetEffect());
            }
            // If there is another grind point, continue the grind towards that points, else end the grind
            else
            {
                EndGrind(to.GetPoint().forward);
            }
        } else
        {
            if (to.GetPrevPoint() != null)
            {
                NextGrind(transform, to.GetPrevPoint(), setGrindSpeed, to.GetEffect());
            }
            // If there is another grind point, continue the grind towards that points, else end the grind
            else
            {
                EndGrind(-to.GetPoint().forward);
            }
        }
    }

    #endregion

    private void FixedUpdate()
    {
        if (player.state == PlayerController.States.GRINDING)
        {
            // Check for trigger inputs for balancing
            if (player.player.GetButtonSinglePressHold("Push Left"))
            {          
                balance -= balanceInc * Time.deltaTime;
            } else if (player.player.GetButtonSinglePressHold("Push Right"))
            {
                balance += balanceInc * Time.deltaTime;
            }

            // Fall Right
            if (balance >= 1.0f)
            {
                if (currentPoint != null)
                {
                    currentPoint.PauseEffect();
                }
                StopAllCoroutines();
                EndGrind(transform.right);
            }
            // Fall Left
            else if (balance <= -1.0f)
            {
                if (currentPoint != null)
                {
                    currentPoint.PauseEffect();
                }
                StopAllCoroutines();
                EndGrind(-transform.right);
            }

            // Update the balance UI
            gm.ui.UpdateBalance(balance);
        }
    }

    /*
     * Ends the grind and pushes the player in a certain direction
     * 
     * @param direction the direction the player should be pushed
     */
    #region EndGrind
    private void EndGrind(Vector3 direction)
    {
        player.CompleteGrind(direction);
        gm.ui.DisplayGrind(false);
    }
    #endregion
}
