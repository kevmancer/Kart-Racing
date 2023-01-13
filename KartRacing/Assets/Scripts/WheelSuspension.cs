using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSuspension : MonoBehaviour
{
    public float springConstant = 3500f;
    public float damperConstant = 2000f;
    public float restLength = 0.5f;
    public float compression = 0f;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, restLength + compression))
        {
            compression = hit.distance - restLength;

            Debug.Log(compression);

            Vector3 springForce = -springConstant * compression * transform.up;
            Vector3 damperForce = -damperConstant * (compression - (transform.position - hit.point).magnitude) * (transform.position - hit.point).normalized;
            transform.position = transform.position + springForce + damperForce;
        }
        else
        {
            compression = 0f;
        }
    }
}
