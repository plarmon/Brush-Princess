using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rail : MonoBehaviour
{
    [SerializeField] private float grindSpeed;

    [SerializeField] public List<GameObject> points;
    private List<GrindPoint> grindPoints;

    // Start is called before the first frame update
    void Start()
    {
        // Initializes the grindPoints List
        grindPoints = new List<GrindPoint>(points.Capacity);
        for(int i = 0; i < points.Capacity; i++)
        {
            GrindPoint gp = points[i].GetComponent<GrindPoint>();
            if (gp != null)
            {
                grindPoints.Add(gp);
                gp.SetPoint(points[i].transform);
                if (i != 0)
                {
                    gp.SetPrevPoint(grindPoints[i - 1]);
                    grindPoints[i - 1].SetNextPoint(gp);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Only checks if the collider is the Player
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Rail Hit Player");
            // Only starts if the player is higher than the collision area and they are not grinding
            if (other.gameObject.GetComponent<PlayerController>().state != PlayerController.States.GRINDING && other.gameObject.transform.position.y > transform.position.y)
            {
                if (!other.gameObject.GetComponent<PlayerController>().GetJustGrinded())
                {
                    // Finds the closest grind point on the rail
                    GrindPoint closestPoint = null;
                    float closestDist = float.MaxValue;

                    foreach (GrindPoint grind in grindPoints)
                    {
                        float dist = Vector3.Magnitude(other.gameObject.transform.position - grind.GetPoint().position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestPoint = grind;
                        }
                    }

                    if (closestPoint != null)
                    {
                        // Starts the grind for the player on the closest grind point
                        other.gameObject.GetComponent<Grind>().StartGrind(other.gameObject.transform, closestPoint, grindSpeed);
                    }
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Only checks if the collider is the Player
        if (collision.collider.gameObject.CompareTag("Player"))
        {
            // Only starts if the player is higher than the collision area and they are not grinding
            if (collision.collider.gameObject.GetComponent<PlayerController>().state != PlayerController.States.GRINDING && collision.collider.gameObject.transform.position.y > transform.position.y)
            {
                if (!collision.gameObject.GetComponent<PlayerController>().GetJustGrinded())
                {
                    // Finds the closest grind point on the rail
                    GrindPoint closestPoint = null;
                    float closestDist = float.MaxValue;

                    foreach (GrindPoint grind in grindPoints)
                    {
                        float dist = Vector3.Magnitude(collision.collider.gameObject.transform.position - grind.GetPoint().position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestPoint = grind;
                        }
                    }

                    if (closestPoint != null)
                    {
                        // Starts the grind for the player on the closest grind point
                        collision.collider.gameObject.GetComponent<Grind>().StartGrind(collision.collider.gameObject.transform, closestPoint, grindSpeed);
                    }
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Rail Hit Player");
    }
}
