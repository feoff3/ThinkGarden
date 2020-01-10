using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Use : NodHandler
{
    public float max_look_distance = 15.0f;
    GvrReticlePointer pointer;
    GvrPointerInputModule input;
    GameObject current_object = null;
    GameObject last_object = null;
    UnityEngine.UI.Text question = null;
    public float forget_object_delay = 1.0f;
    public float outline_radius = 10.0f;
    public float outline_width = 3.0f;
    public float default_time_to_read = 4.0f;
    float read_start_time = 0;
    bool read_complete = false;
    string new_text = "";
    ArrayList objects_around;
    // Start is called before the first frame update
    void Start()
    {
        pointer = GameObject.Find("GvrReticlePointer").GetComponent<GvrReticlePointer>();
        question = GameObject.Find("Question").GetComponent<UnityEngine.UI.Text>();
        //if (question != null)
        //    question.text = Screen.currentResolution.ToString();
        base.Register();
        base.SetTargetObject(null);
        if (outline_radius > 0)
        {
            var boxCollider = gameObject.AddComponent<SphereCollider>();
            boxCollider.isTrigger = true;
            boxCollider.radius = outline_radius;
            var rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
        }
        objects_around = new ArrayList();
    }

    bool IsObjectUsable(GameObject obj)
    {
        return obj.tag == "Usable" || obj.tag == "Readable" || obj.GetComponent<Usable>() != null;
    }

    bool IsObjectReadable(GameObject obj)
    {
        return obj.tag == "Readable" || obj.GetComponent<Readable>() != null;
    }

    float GetTimeToRead(GameObject obj)
    {
        Readable r = obj.GetComponent<Readable>();
        if (r != null)
        {
            return r.time_to_read;
        }
        return default_time_to_read;
    }

    void OnRead(GameObject obj)
    {
        Readable r = obj.GetComponent<Readable>();
        if (r != null)
        {
            r.OnUse();
        }
        else
        {
            // TODO: check settings if to play subtitles
            AudioSource clip = obj.GetComponentInChildren<AudioSource>();
            if (clip && clip.isPlaying == false)
                clip.Play();
            new_text = GetObjectText(obj, true);
        }
    }

    // outline object which in collider
    private void OnTriggerEnter(Collider other)
    {
        if (objects_around.Contains(other.gameObject))
            return;
        if (IsObjectUsable(other.gameObject) == false)
            return;
        Debug.Log("New object in collider " + other.gameObject.name);
        Usable usable;
        if (other.gameObject.TryGetComponent<Usable>(out usable))
        {
            if (usable.outline == false)
                return;
        }
        var outline = other.gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = other.gameObject.AddComponent<Outline>();
            outline.enabled = false;
        }
        if (outline.enabled == false)
        {
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.enabled = true;
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 0.3f;
        }
        objects_around.Add(other);

    }
    private void OnTriggerStay(Collider other)
    {
        if (IsObjectUsable(other.gameObject) == false)
            return;
        var outline = other.gameObject.GetComponent<Outline>();
        if (outline == null)
            return;
        if (outline.OutlineWidth < outline_width)
            outline.OutlineWidth += 0.1f;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsObjectUsable(other.gameObject) == false)
            return;
        var outline = other.gameObject.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
        objects_around.Remove(other.gameObject);
    }

    string GetObjectText(GameObject obj, bool subtitles = false)
    {
        TextMesh[] texts = obj.GetComponentsInChildren<TextMesh>();
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name == "Subtitles" && subtitles)
                return texts[i].text;
            if (texts[i].name != "Subtitles")
                return texts[i].text;
        }
        return "";
    }

    // Update is called once per frame
    void Update()
    {
        if (!registered)
            Start();
        RaycastHit hit;
        bool end_pointer = false;
        // manage rectile pointer
        if (Physics.Raycast(transform.position, transform.forward, out hit, max_look_distance))
        {
            if (IsObjectUsable(hit.collider.gameObject) == false)
            {
                end_pointer = true;
            }
            else
            {
                if (hit.collider.gameObject != current_object)
                {
                    pointer.OnPointerEnter(GvrPointerInputModule.CurrentRaycastResult, true);
                    current_object = hit.collider.gameObject;
                    new_text = GetObjectText(current_object);
                }
                else
                {
                    pointer.OnPointerHover(GvrPointerInputModule.CurrentRaycastResult, true);
                    if (question.text.Length != new_text.Length && read_start_time == 0)
                        question.text = new_text.Substring(0, question.text.Length + 1);
                    // Gaze base interaction for read
                    else
                    {
                        if (IsObjectReadable(hit.collider.gameObject) && read_complete == false)
                        {
                            // play gaze progress animation
                            if (read_start_time == 0)
                                read_start_time = Time.time;

                            float time_to_read = GetTimeToRead(hit.collider.gameObject);
                            if (Time.time - read_start_time > time_to_read)
                            {
                                // end gaze animation
                                OnRead(hit.collider.gameObject);
                                read_complete = true;
                                read_start_time = 0;
                            }
                            else
                            {
                                float percent = ((Time.time - read_start_time) / time_to_read) * 100.0f;
                                //string percent_str = ((int)percent).ToString();
                                string percent_str = "";
                                int num_percent = (int)percent / 8;
                                for (int i = 0; i < 8; i++)
                                {
                                    if (i < num_percent)
                                        percent_str += '\u2588';
                                }
                                if (percent_str != "")
                                    question.text = new_text + " " + percent_str;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            end_pointer = true;
        }
        if (current_object != null && end_pointer)
        {
            last_object = current_object;
            pointer.OnPointerExit(current_object);
            Invoke("ForgetObject", forget_object_delay);
        }
    }

    void ForgetObject()
    {
        if (last_object == current_object)
        {
            current_object = null;
            question.text = "";
            // gaze stop animation
            read_start_time = 0;
            read_complete = false;
        }
    }

    protected override void OnNod()
    {
        if (current_object != null)
        {
            Usable usable = null;
            Debug.Log("Got nod message Use()");
            if (current_object.TryGetComponent<Usable>(out usable))
            {
                usable.OnUse();
            }
            else
                Debug.Log("Object not usable...");
        }
    }
}

public class Usable : MonoBehaviour
{
    public bool outline = true;

    void Awake()
    {
        if (this.outline)
        {
            var outline = gameObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 3f;
            outline.enabled = false;
        }
    }

    // note: it is a good idea to add Outline on awake here...
    public virtual void OnUse() { }

    public virtual void MakeUnusable() 
    {
        this.tag = "Untagged";
        var outline = gameObject.GetComponent<Outline>();
        if (outline)
            outline.enabled = false;
    }
}

public class Readable : Usable
{
    public float time_to_read = 3.0f;

    public virtual void OnUse() 
    {
        // TODO: read from config if to read subtitles
    }
}
