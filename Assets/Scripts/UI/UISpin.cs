using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UISpin : MonoBehaviour
{

    [SerializeField, Header("In degrees per second")]
    private float speed;

    private RectTransform rectTransform;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        rectTransform.Rotate(Vector3.forward * speed * Time.deltaTime);
    }

}
