using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : Usable
{
    bool animation_started = false;
    bool animation_ended = false;
    public float distance_to_appear = 0.5f;
    public float distance_to_appear_y = 0.02f;
    public float delay = 1.0f;
    public float speed = 1.0f;
    public GameObject ghost = null;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (animation_started && !animation_ended)
        {
            ghost.transform.position += ghost.transform.forward * Time.deltaTime * speed;
            if (ghost.transform.position.y - transform.position.y < distance_to_appear_y)
                ghost.transform.position += Vector3.up * Time.deltaTime * speed;
            if ((ghost.transform.position - transform.position).magnitude > distance_to_appear)
            {
                animation_ended = true;
                Ghost ghost_script = null;
                if (ghost.TryGetComponent<Ghost>(out ghost_script))
                    ghost_script.Activate();
            }
        }
    }

    public void StartAnimation()
    {
        ghost.transform.position = transform.position;
        animation_started = true;
    }


    public override void OnUse()
    {
        //Debug.Log("OnUse() got by instructor");
        if (ghost == null)
        {
            ghost = GameObject.Find("Ghost");
        }
        if (ghost != null)
        {
            this.MakeUnusable();
            Invoke("StartAnimation", delay);
        }
    }
}
