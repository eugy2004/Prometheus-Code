using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotatingElement : MonoBehaviour
{
    [SerializeField]
    Vector3 rotationDirection;
    void Update()
    {
        transform.eulerAngles += rotationDirection * Time.deltaTime;
    }
}
