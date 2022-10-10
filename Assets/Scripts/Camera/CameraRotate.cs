using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{

    [SerializeField]
    private Vector3 offset;
    
    private Vector3 pivot;

    [SerializeField, Range(0F, 1F)]
    private float angleAboveGround;

    [SerializeField]
    private float distance, rotationSensitivity, rotationAcceleration, distancePerSegment;

    [SerializeField]
    private bool facePivot;

    private InputManager InputManager => Global.Get<InputManager>();

    private Vector3 targetDisplacement;

    private float targetAngleAround, currentAngleAround;

    private float cosAG, sinAG;

    private void Start()
    {
        pivot = Global.Get<PathManager>().GetPathAverageWorldPos() + offset;

        currentAngleAround = targetAngleAround = Random.Range(0, Mathf.PI * 2F);

        cosAG = Mathf.Cos(angleAboveGround * 0.5F * Mathf.PI);
        sinAG = Mathf.Sin(angleAboveGround * 0.5F * Mathf.PI);

        targetDisplacement = new Vector3(
            Mathf.Cos(currentAngleAround * 0.5F * Mathf.PI) * sinAG,
            cosAG, Mathf.Sin(currentAngleAround * 0.5F * Mathf.PI) * sinAG) * distance;

        transform.position = pivot + targetDisplacement;

        Global.Get<PathManager>().OnPathChanged += UpdatePivot;
    }

    private void UpdatePivot()
    {
        pivot = Global.Get<PathManager>().GetPathAverageWorldPos(true) + offset;
    }

    private void Update()
    {
        if(InputManager.IsHolding)
        {
            targetAngleAround -= Time.deltaTime * rotationSensitivity * InputManager.PointerDelta.x;       
        }

        currentAngleAround = Mathf.Lerp(currentAngleAround, targetAngleAround, Time.deltaTime * rotationAcceleration);

        targetDisplacement = new Vector3(
            Mathf.Cos(currentAngleAround * 0.5F * Mathf.PI) * sinAG,
            cosAG, Mathf.Sin(currentAngleAround * 0.5F * Mathf.PI) * sinAG) * (distance + distancePerSegment * Global.Get<PathManager>().SegmentsAppendedCount);

        transform.position = pivot + targetDisplacement;

        if (facePivot)
        {
            transform.rotation = Quaternion.LookRotation((pivot - transform.position).normalized, Vector3.up);
        }
    }

}
