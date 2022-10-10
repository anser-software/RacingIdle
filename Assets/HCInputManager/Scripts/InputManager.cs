using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Baracuda.Monitoring;
using UnityEngine.EventSystems;
public class InputManager : MonoBehaviour
{

    #region Singleton
    private void Awake()
    {
        Global.Register<InputManager>(this);
    }
    #endregion

    #region Pointer Position

    public Vector2 PointerPos { get; private set; }

    public Vector2 PointerInitPosOnTap { get; private set; }

    public Vector2 PointerDisplacementFromInitPos => PointerPos - PointerInitPosOnTap;

    public Vector2 PointerPosViewport => new Vector2(PointerPos.x / Screen.width, PointerPos.y / Screen.height);

    public Vector2 PointerDelta { get; private set; }

    public Vector2 PointerDeltaViewport => new Vector2(PointerDelta.x / Screen.width, PointerDelta.y / Screen.height);

    public Vector2 PointerMoveDirection => PointerDelta.normalized;

    private Vector2 lastPointerPos;

    #endregion

    #region Events
    public event Action OnTap;

    public event Action OnClick;

    public event Action<HoldData> OnHoldEnd;

    public event Action<SwipeData> OnSwipe;

    public event Action<Collider> OnObjectTapped;
    #endregion

    #region Data
    public bool IsHolding => TimeHoldingPointerDown > MinHoldingTimeForOnHold;

    public float TimeHoldingPointerDown { get; private set; }

    public float MaxHoldingTimeForClick = 0.2F;

    public float MinHoldingTimeForOnHold = 0.3F;

    public float MinPointerDisplacementForSwipe = 10F;
    #endregion

    #region Transformations

    public Vector3 TransformPointerPosToWorldPlane(Camera camera, float zDepth)
    {
        var frustumHeight = zDepth * MathF.Tan(camera.fieldOfView * 0.5F * Mathf.Deg2Rad);

        var frustumWidth = frustumHeight * camera.aspect;

        var pointerPos = (PointerPosViewport - Vector2.one * 0.5F) * 2F;

        var x = pointerPos.x * frustumWidth;

        var y = pointerPos.y * frustumHeight;

        var cameraTransform = camera.transform;

        return cameraTransform.position + x * cameraTransform.right + y * cameraTransform.up + zDepth * cameraTransform.forward;
    }

    public Vector3 TransformPointerPosToWorldPlane(Transform orientation, float zDepth, float scale)
    {
        var pointerPos = (PointerPosViewport - Vector2.one * 0.5F) * scale;

        return orientation.position + pointerPos.x * orientation.right + pointerPos.y * orientation.up + zDepth * orientation.forward;
    }

    public Vector3 TransformPointerPosToWorldPlane(Vector3 normal, Vector3 up, Vector3 origin, float scale)
    {
        var horizontal = Vector3.Cross(normal, up);

        var pointerPos = (PointerPosViewport - Vector2.one * 0.5F) * scale;

        return origin + pointerPos.x * horizontal + pointerPos.y * up;
    }

    #endregion


    private Camera mainCam;

    public T TryGetObjectTappedOn<T>()
    {
        if (Physics.Raycast(mainCam.ScreenPointToRay(PointerPos), out RaycastHit hitInfo))
        {
            var hit = hitInfo.transform.GetComponent<T>();

            if (!hit.Equals(default(T)))
            {
                return hit;
            }
        }

        return default(T);
    }

    private void Start()
    {
        lastPointerPos = PointerPos = Vector2.zero;

        TimeHoldingPointerDown = 0F;

        mainCam = Camera.main;
    }

    private void Update()
    {
        PointerPos = Input.mousePosition;

        PointerDelta = PointerPos - lastPointerPos;

        lastPointerPos = PointerPos;

        if(Input.GetMouseButtonDown(0))
        {
            PointerInitPosOnTap = PointerPos;

            if(!EventSystem.current.IsPointerOverGameObject())
                OnTap?.Invoke();

            CheckTap();
        }
        if (Input.GetMouseButton(0))
        {
            TimeHoldingPointerDown += Time.deltaTime;
        } else if(Input.GetMouseButtonUp(0))
        {
            PointerUp();
        }
    }

    private void CheckTap()
    {
        var go = TryGetObjectTappedOn<Collider>();

        if (go != null)
        {
            OnObjectTapped?.Invoke(go);
        }
    }

    private void PointerUp()
    {
        var displacementMagnitude = PointerDisplacementFromInitPos.magnitude;

        if (TimeHoldingPointerDown < MaxHoldingTimeForClick)
        {
            OnClick?.Invoke();
        }
        if (TimeHoldingPointerDown > MinHoldingTimeForOnHold)
        {
            var holdData = new HoldData(TimeHoldingPointerDown, PointerInitPosOnTap, displacementMagnitude);

            OnHoldEnd?.Invoke(holdData);
        }
        if (displacementMagnitude > MinPointerDisplacementForSwipe)
        {
            var swipeData = new SwipeData(TimeHoldingPointerDown, PointerDisplacementFromInitPos);

            OnSwipe?.Invoke(swipeData);
        }

        TimeHoldingPointerDown = 0F;
    }

}

public struct HoldData
{
    public float TimeHolding;
    public Vector2 InitialTapPosScreen;
    public float PointerDisplacementMagnitude;

    public HoldData(float timeHolding, Vector2 initialTapPosScreen, float pointerDisplacement)
    {
        TimeHolding = timeHolding;
        InitialTapPosScreen = initialTapPosScreen;
        PointerDisplacementMagnitude = pointerDisplacement;
    }
}

public struct SwipeData
{
    public float TimeHolding;

    public float SwipeAngleRad;

    public float SwipeDistance;

    public Vector2 SwipeDisplacement;

    public SwipeData(float timeHolding, Vector2 swipeDisplacement)
    {
        TimeHolding = timeHolding;
        SwipeDisplacement = swipeDisplacement;
        SwipeDistance = SwipeDisplacement.magnitude;

        var swipeDirection = SwipeDisplacement.normalized;

        SwipeAngleRad = Mathf.Atan2(swipeDirection.y, swipeDirection.x);
    }
}
