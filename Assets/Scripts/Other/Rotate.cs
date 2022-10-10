using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{


    [SerializeField, Header("In degrees per second")]
    private float speed;

    [SerializeField]
    private Vector3 axis;

    private void Update()
    {
        transform.Rotate(axis * speed * Time.deltaTime);
    }

}
