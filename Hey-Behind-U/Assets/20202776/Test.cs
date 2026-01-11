using UnityEngine;
using System.Collections.Generic;

public class CircleColliderModifier : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float radius = 7.5f;
    public int segments = 100;

    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        lineRenderer.positionCount = segments + 1;
        Draw();
    }
    void Update()
    {
        this.gameObject.GetComponent<Rigidbody>().linearVelocity = new Vector3(1.5f, 0, 0);
    }

    void Draw()
    {
        points.Clear();
        float angleStep = 360f / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * angleStep * i;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            points.Add(new Vector3(x, 0, y));
        }
        lineRenderer.SetPositions(points.ToArray());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Box"))
        {
            ModifyCircle(other.bounds);
        }
    }

    void ModifyCircle(Bounds boxBounds)
    {
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[i];
            if (boxBounds.Contains(point))
            {
                points[i] = new Vector3(boxBounds.center.x, boxBounds.center.y, 0); // 충돌한 부분을 박스 중심으로 이동
            }
        }
        lineRenderer.SetPositions(points.ToArray());
    }
}