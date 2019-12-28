using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instructor : Usable
{
    /*
     * Text
     * 
     * 
     * 
     
     */
    bool speaking;
    
    // Start is called before the first frame update
    // The target rotation marker
    GameObject turn_towards = null;

    // Angular speed in radians per sec for turning towards player.
    public float speed = 1.0f;

    // which state to play after response on nod
    public string state_to_play = "salute 0";

    // when to start autorotate
    public float autorotate_distance = 5.0f;

    void RotateComplete()
    {
        
    }

    void Respond()
    {
        // here we start some animation
        Animator animator = GetComponentInParent<Animator>();
        if (animator == null)
            return;
        animator.Play(state_to_play);
    }

    float getAngleToObject(GameObject obj)
    {
        Transform target = obj.transform;
        return Utility.getAngleToObject(transform , target);
    }

    float getDistanceToObject(GameObject obj)
    {
        Transform target = obj.transform;
        return Utility.getDistanceToObject(transform, target);
    }

    void Update()
    {
        GameObject player = GameObject.Find("Player");
        if (getDistanceToObject(player) < autorotate_distance)
            turn_towards = player;
        if (turn_towards == null)
            return;

        Utility.RotateTowards(transform, turn_towards.transform, speed * Time.deltaTime, true);
        
        if (System.Math.Abs(getAngleToObject(turn_towards)) < 10.0f)
        {
            //Debug.Log("Finish rotating...");
            turn_towards = null;
            RotateComplete();
        }      
    }

    public override void OnUse()
    {
        //Debug.Log("OnUse() got by instructor");
        GameObject player = GameObject.Find("Player");
        if (System.Math.Abs(getAngleToObject(player)) < 20.0f)
        {
            Respond();
        }
        turn_towards = player;
    }


}

// just a class with some static studd
public class Utility
{
    public static float getAngleToObject(Transform me, Transform target)
    {
        // Determine which direction to rotate towards
        Vector3 my_position = me.position;
        Vector3 target_vec = target.position;
        target_vec.y = my_position.y;
        Vector3 targetDirection = target_vec - my_position;
        return Vector3.SignedAngle(me.forward, targetDirection, Vector3.up);
    }

    public static float getDistanceToObject(Transform me, Transform target)
    {
        // Determine which direction to rotate towards
        Vector3 my_position = me.position;
        Vector3 target_vec = target.position;
        return (target_vec - my_position).magnitude;
    }
    // rotate towards preserving y, on the fraction of existing angle between objects
    public static void RotateTowards(Transform me, Transform target, float fraction, bool rotate_parent = false)
    {
        // Determine which direction to rotate towards
        Vector3 my_position = me.transform.position;
        Vector3 target_vec = target.position;
        target_vec.y = my_position.y;
        Vector3 targetDirection = target_vec - my_position;
        // The step size is equal to speed times frame time.
        Transform transform = me;
        if (rotate_parent)
            transform = transform.parent;
        transform.RotateAround(my_position, Vector3.up, getAngleToObject(me, target) * fraction);
    }
    // move towards preserving y, negative distance to move away
    public static void MoveTowards(Transform me, Transform target, float distance, bool move_parent = false)
    {
        // Determine which direction to rotate towards
        Vector3 my_position = me.transform.position;
        Vector3 target_vec = target.position;
        target_vec.y = my_position.y;
        // The step size is equal to speed times frame time.
        Transform transform = me;
        if (move_parent)
            transform = transform.parent;
        Vector3 new_pos = Vector3.MoveTowards(transform.position, target_vec, distance);
        transform.position = new_pos;
    }
};
