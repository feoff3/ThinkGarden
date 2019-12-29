using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float initial_speed = 4f;
    bool initial_animation_started = false;
    bool initial_animation_ended = false;
    public GameObject initial_target = null;
    public float initial_stop_distance = 1.8f;
    public float initial_shrink_speed = 0.0f;
    float angle_speed = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // initial animation - move towards the initial_target (player)
        if (initial_animation_started && !initial_animation_ended && initial_target != null)
        {
            Transform me = transform;
            Transform target = initial_target.transform;
            transform.localScale -= transform.localScale * initial_shrink_speed;
            if (System.Math.Abs(Utility.getAngleToObject(me, target)) > 1.0f)
            {
                Debug.Log(Utility.getAngleToObject(me, target));
                Utility.RotateTowards(me, target, angle_speed * Time.deltaTime);
            }
            Utility.MoveTowards(me, target, initial_speed*Time.deltaTime);
            if (Utility.getDistanceToObject(me, target) < initial_stop_distance)
            {
                initial_animation_ended = true;
                if (initial_shrink_speed > 0)
                    transform.localScale = Vector3.zero;
            }
        }
        // TODO: floating animation
    }

    public void Activate()
    {
        if (initial_target != null)
        {
            Transform me = transform;
            Transform target = initial_target.transform;
            angle_speed = System.Math.Abs(2 / ((Utility.getDistanceToObject(me, target) - initial_stop_distance) / initial_speed));
            Debug.Log("Angle Speed: " + angle_speed);
            initial_animation_started = true;
        }
    }
}
