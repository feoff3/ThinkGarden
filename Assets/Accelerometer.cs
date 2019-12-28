using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Accelerometer : MonoBehaviour {

    //The lower this value, the less smooth the value is and faster Accel is updated. 30 seems fine for this
    const float updateSpeed = 30.0f;

    float AccelerometerUpdateInterval = 1.0f / updateSpeed;
    float LowPassKernelWidthInSeconds = 1.0f;
    float LowPassFilterFactor = 0;
    Vector3 lowPassValue = Vector3.zero;
    float x = 0;
    float y = 0;
    float y_nograv = 0;
    float z = 0;
    float m = 0;
    public float seconds_for_nod_measure = 1.5f;
    float[] nod_measure;
    int last_nod_measure_index = 0;
    int nods_total = 0;
    public float nod_m_threshold = 6;
    public float nod_cooldown = 1;
    float nod_cooldown_end = 0; // time in seconds when to end nod cooldown
    string help_text = "TO WALK:\nLOOK to the place on the ground\nwhere you want to step to,\nquickly NOD by your head,\nor TOUCH phone screen,\nor PRESS cardboard button\nDo a couple of steps to end this tutorial!";
    public bool display_debug_text = false;
    public bool display_help_text = true;

    System.Collections.ArrayList gesture_subscribers = null; // objects that subscribed to gestures
   

	// Use this for initialization
	void Start () {
        if (gesture_subscribers == null)
            gesture_subscribers = new ArrayList();
        LowPassFilterFactor = AccelerometerUpdateInterval / LowPassKernelWidthInSeconds;
        lowPassValue = Input.acceleration;
        GetComponent<TextMesh>().text = "Bye World";
        float nod_measure_size = seconds_for_nod_measure / Time.deltaTime;
        nod_measure = new float[(int)(nod_measure_size)];
        for (int i = 0; i < nod_measure.GetLength(0); i++)
            nod_measure[i] = 0;
	}


    public abstract class IGestureSubscriber : MonoBehaviour
    {
        public abstract void NotifyGesture(string gesture_name, System.Collections.Generic.Dictionary<string, object> parms);
    };

    // subscribe to gesture events. must implement IGestureSubscriber interface
    public void SubscribeGestures(IGestureSubscriber subscriber_object)
    {
        Debug.Log("New subscriber");
        if (gesture_subscribers == null)
            Start();
        gesture_subscribers.Add(subscriber_object);
    }

    public void UnsubscribeGestures(IGestureSubscriber subscriber_object)
    {
        gesture_subscribers.Remove(subscriber_object);
    }

    void NotifyGesture(string gesture_name)
    {
        System.Collections.Generic.Dictionary<string, object> parms = new System.Collections.Generic.Dictionary<string, object>();
        NotifyGesture(gesture_name, parms);
    }

    void NotifyGesture(string gesture_name, System.Collections.Generic.Dictionary<string, object> parms)
    {
        IEnumerator enumerator = gesture_subscribers.GetEnumerator();
        while (enumerator.MoveNext())
        {
            (enumerator.Current as IGestureSubscriber).NotifyGesture(gesture_name, parms);
        }
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 rawAccelValue = Input.acceleration;
        x += Time.deltaTime * Time.deltaTime / 2 * rawAccelValue.x;
        y += Time.deltaTime * Time.deltaTime / 2 * rawAccelValue.y;
        y_nograv += Time.deltaTime * Time.deltaTime / 2 * (rawAccelValue.y + 0.98f);
        z += Time.deltaTime * Time.deltaTime / 2 * rawAccelValue.z;
        float delta_m_filtered = 0;
        if ((rawAccelValue.magnitude - 1.0f) > 0.05f)
            delta_m_filtered = (rawAccelValue.magnitude - 1.0f);
        m += delta_m_filtered;

        // measure nod gesture
        if (last_nod_measure_index >= nod_measure.GetLength(0))
            last_nod_measure_index = 0;
        nod_measure[last_nod_measure_index] = delta_m_filtered;
        last_nod_measure_index++;
        
        float m_in_seconds = 0; // how much m changed since for last seconds_for_nod_measure seconds
        // calc sum
        for (int i = 0; i < nod_measure.GetLength(0); i++)
            m_in_seconds += nod_measure[i];

        if ((m_in_seconds > nod_m_threshold || (Input.GetMouseButtonDown(0)) ) && nod_cooldown_end < Time.time)
        {
            // count nod
            nods_total++;
            // timeout till next one can be detected
            nod_cooldown_end = Time.time + nod_cooldown;
            NotifyGesture("Nod"); // TODO: send raycast info 
        }
        if (nods_total > 2)
            display_help_text = false;

        // print out
        string text = "";
        if (display_help_text) 
            text = help_text;
        if (display_debug_text)
        {
            text = text + "\nFPS: " + (1.0f / Time.deltaTime) +
            "\nMagnitude: " + rawAccelValue.magnitude +
            "\nIntegral M: " + m +
            "\nLast seconds: " + m_in_seconds +
             "\nNods: " + nods_total;
        }
        //Debug.Log(text);
        GetComponent<TextMesh>().text = text;
	}

}

public class NodHandler : MonoBehaviour
{
    protected bool registered = false;
    protected GameObject target_game_object = null;
    protected void Register(GameObject game_object = null)
    {
        target_game_object = game_object;
        if (game_object == null)
            target_game_object = gameObject;
        NodHandlerDelegate handler = new NodHandlerDelegate(this);
        Accelerometer script = GameObject.Find("AccelText").GetComponent<Accelerometer>();
        script.SubscribeGestures(handler);
        registered = true;
        Debug.Log(this.name + ":" + this.GetType().Name + " registered for gestures");
    }

    class NodHandlerDelegate : Accelerometer.IGestureSubscriber
    {
        private NodHandler Obj;
        public NodHandlerDelegate(NodHandler obj) { Obj = obj; }
        public override void NotifyGesture(string gesture_name, System.Collections.Generic.Dictionary<string, object> parms)
        {
            //Debug.Log("Gesture detected " + gesture_name);
            if (gesture_name == "Nod")
            {
                Obj.HandleNod();
            }
        }
    }

    protected void SetTargetObject(GameObject game_object)
    {
        target_game_object = game_object;
    }

    void HandleNod()
    {
        RaycastHit hit;
        GameObject player = GameObject.Find("Player");
        Camera cam = player.GetComponentInChildren<Camera>();
        bool ray_hit = Physics.Raycast(cam.transform.position, cam.transform.forward, out hit);
        //Debug.Log(this.name + ":" + this.GetType().Name + " testing for nod");
        if (target_game_object == null || ray_hit)
        {
            if (target_game_object == null || (ray_hit && hit.collider.gameObject == target_game_object))
            {
                //Debug.Log(this.name + ":" + this.GetType().Name + " transfer nod");
                OnNod();
            }
        }
    }

    protected virtual void OnNod() { }
}