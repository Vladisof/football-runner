using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartLoRunPlayer : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float speed = 2.0f;

    private Vector3 currentTarget;

    void Start()
    {
        currentTarget = endPoint.position;
    }

    void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, currentTarget, step);

        if (transform.position == currentTarget)
        {
            if (currentTarget == startPoint.position)
            {
                currentTarget = endPoint.position;
            }
            else
            {
                transform.position = startPoint.position;
            }
        }
    
    }
}
