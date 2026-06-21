using UnityEngine;

public class MapMove : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 0.3f;
    [SerializeField] private float maxZoom = 3f;
    [SerializeField] private float dragSpeed = 0.5f;

    private Camera cam;
    private Vector3 dragOrigin;

    private Transform MapRoot => transform;

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
            Debug.LogError("MapMove: 未找到主相机");
    }

    void Update()
    {
        if (cam == null) return;

        HandleZoom();
        HandlePan();
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        float oldScale = MapRoot.localScale.x;
        float newScale = oldScale + scroll * zoomSpeed;
        newScale = Mathf.Clamp(newScale, minZoom, maxZoom);
        if (Mathf.Approximately(oldScale, newScale)) return;

        // 鼠标指向的世界坐标
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);

        // 该点在 MapRoot 本地空间中的位置
        Vector3 localMouse = (mouseWorld - MapRoot.position) / oldScale;

        MapRoot.localScale = Vector3.one * newScale;

        // 调整位置使同一世界点保持在鼠标下
        MapRoot.position = mouseWorld - localMouse * newScale;
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            return;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 currentWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = (currentWorld - dragOrigin) * dragSpeed;
            MapRoot.position += delta;
            dragOrigin = currentWorld;
        }
    }
}
