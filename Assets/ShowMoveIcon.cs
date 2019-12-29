using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMoveIcon : MonoBehaviour
{
    public GameObject icon = null;
    public float max_look_distance = 20.0f;
    private Vector3 original_scale = Vector3.zero;
    public float icon_growth_speed = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        original_scale = icon.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (original_scale == Vector3.zero)
            Start();
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, max_look_distance))
        {
            if (hit.collider.gameObject.tag == "Walkable")
            {
                if ((icon.transform.localScale - original_scale).magnitude > 0.1f)
                    icon.transform.localScale += (original_scale - icon.transform.localScale) * icon_growth_speed;
                return;
            }
        }
        icon.transform.localScale -= original_scale * 0.1f;
        if (icon.transform.localScale.x < 0)
            icon.transform.localScale = Vector3.zero;
    }
}
