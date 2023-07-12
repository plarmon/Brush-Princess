using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintObjTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PaintObjs"))
        {
            other.gameObject.GetComponent<PaintObj>().StartTransition();
        }
    }
}
