using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Move : MonoBehaviour
{
    public float max_look_distance = 38.0f;
    public float max_move_distance = 3.5f;
    public float Height = 5.0f;
    public float Speed = 2.5f;
    public float HeightOscillation = 0.02f;
    private bool registered = false;
    private Vector3 target_pos;
    private Vector3 start_pos;
    private float start_time;

    IEnumerator SwitchToVR()
    {
        // Device names are lowercase, as returned by `XRSettings.supportedDevices`.
        string desiredDevice = "cardboard"; // Or "cardboard".

        // Some VR Devices do not support reloading when already active, see
        // https://docs.unity3d.com/ScriptReference/XR.XRSettings.LoadDeviceByName.html
        if (System.String.Compare(UnityEngine.XR.XRSettings.loadedDeviceName, desiredDevice, true) != 0)
        {
            UnityEngine.XR.XRSettings.LoadDeviceByName(desiredDevice);

            // Must wait one frame after calling `XRSettings.LoadDeviceByName()`.
            yield return null;
        }

        // Now it's ok to enable VR mode.
        UnityEngine.XR.XRSettings.enabled = true;
    }


    // Start is called before the first frame update
    void Start()
    {
        NodHandler handler = new NodHandler(this);
        Accelerometer script = GameObject.Find("AccelText").GetComponent<Accelerometer>();
        script.SubscribeGestures(handler);
        registered = true;
        GameObject player = GameObject.Find("Player");
        target_pos = player.transform.position;

        StartCoroutine(SwitchToVR());
    }

    // Update is called once per frame
    void Update()
    {
        if (!registered)
            Start();
        GameObject player = GameObject.Find("Player");
        if (player.transform.position != target_pos)
        {
            float distance_walked = (start_pos - player.transform.position).magnitude;
            float t = Time.time - start_time;
            float a = (float)(System.Math.Pow(max_move_distance * 6 , 0.3)); // most probably the movement will end somewhere there
            float current_speed = Speed * t * (a - t);
            Vector3 intermediate_pos = Vector3.MoveTowards(player.transform.position, target_pos, current_speed * Time.deltaTime);
            // a liitle oscillation of y
            //intermediate_pos.y = target_pos.y + Height * HeightOscillation * ((float)System.Math.Sin(3.14f * t / a));
            //intermediate_pos.z = intermediate_pos.z + 0.01f * Height * HeightOscillation * ((float)System.Math.Cos(3.14f * t / a));
            if (distance_walked > max_move_distance) // stop moving if we go too far
            {
                target_pos = intermediate_pos; // setting it as an end of the trajectory if distance is too far
            }
            player.transform.position = intermediate_pos;
        }
    }

    class NodHandler : Accelerometer.IGestureSubscriber
    {
        private Move MoveObj;
        public NodHandler(Move move_obj) { MoveObj = move_obj; }
        public override void NotifyGesture(string gesture_name, System.Collections.Generic.Dictionary<string, object> parms)
        {
            Debug.Log("Gesture detected " + gesture_name);
            if (gesture_name == "Nod")
            {
                MoveObj.MovePlayer();
            }
        }
    }

    void MovePlayer()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, max_look_distance))
        {
            if (hit.collider.gameObject.tag != "Walkable")
            {
                Debug.Log("Wrong object " + hit.collider.gameObject.name);
                return;
            }
            GameObject player = GameObject.Find("Player");
            Debug.Log("HIT " + hit.point.ToString() + " Magnitude " + (player.transform.position - hit.point).magnitude);
            start_pos = player.transform.position;
            start_time = Time.time;
            Vector3 new_pos = hit.point;
            new_pos.y += Height;
            target_pos = new_pos;
        }
        else
        {
            Debug.Log("Raycast miss");
        }
    }
}
