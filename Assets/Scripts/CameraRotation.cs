using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        float rotX = Input.GetAxis("Mouse Y");
        float angle = this.gameObject.transform.localEulerAngles.x - rotX;

        if (angle > 180) angle = angle - 360;
        angle = Mathf.Clamp(angle, -70f, 70f);

        this.gameObject.transform.localEulerAngles = new Vector3(angle, 0.0f, 0.0f);
    }

}
