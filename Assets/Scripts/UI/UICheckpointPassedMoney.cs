using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UICheckpointPassedMoney : MonoBehaviour
{

    [SerializeField]
    private float distance, duration;

    [SerializeField]
    private AnimationCurve animationCurve;

    [SerializeField]
    private TextMeshProUGUI textMesh;

    private float t;

    private Vector3 initialPos;

    public void Init(float money)
    {
        textMesh.text = "+" + money.ToString();
    }

    private void Start()
    {
        initialPos = transform.position;
    }

    private void Update()
    {
        t += Time.deltaTime / duration;

        transform.position = initialPos + Vector3.up * animationCurve.Evaluate(t) * distance;

        var c = textMesh.color;

        c.a = 1F - animationCurve.Evaluate(t);

        textMesh.color = c;

        if(t >= 1F)
        {
            Destroy(gameObject);
        }
    }

}
