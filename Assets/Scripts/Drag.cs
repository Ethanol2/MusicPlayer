using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Drag : MonoBehaviour
{
    public bool dragging = false;
    [Space]
    public bool lockX = false;
    public bool lockY = false;
    public bool lockToBounds = false;
    public Bounds bounds = new Bounds();
    public UnityEvent onDragStart = new UnityEvent();
    public UnityEvent onDrag = new UnityEvent();
    public UnityEvent onDragEnd = new UnityEvent();

    Vector2 cursorPos;

    void OnMouseDown()
    {
        dragging = true;
        cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        onDragStart.Invoke();
    }
    void OnMouseDrag()
    {
        Vector2 current = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dif = current - cursorPos;

        Vector2 newPos = this.transform.localPosition + new Vector3( lockX ? 0f : dif.x, lockY ? 0f : dif.y, 0f);
        if (lockToBounds)
        {
            if (!bounds.Contains(newPos))
            {
                newPos = this.transform.localPosition;
            }
        }
        this.transform.localPosition = newPos;
        cursorPos = current;
        onDrag.Invoke();
    }
    void OnMouseUp()
    {
        dragging = false;
        onDragEnd.Invoke();
    }
}
