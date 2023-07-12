using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private float floatSpeed;
    private float floatDamper;
    private Camera theCam;
    private float startyPos;
    private float floatInc;

    // Start is called before the first frame update
    void Start()
    {
        theCam = Camera.main;
        startyPos = transform.position.y;
        floatInc = 0;
        floatSpeed = 2.0f;
        floatDamper = 10.0f;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(theCam.transform);
        transform.Rotate(new Vector3(0, 180, 0), Space.Self);

        transform.position = new Vector3(transform.position.x, startyPos + Mathf.Cos(floatInc) / floatDamper, transform.position.z);

        floatInc += floatSpeed * Time.deltaTime;
    }
}
