using UnityEngine;

public class FPSMgr : MonoBehaviour {
    public float f_UpdateInterval = 0.5F;

    private float f_LastInterval;

    private int i_Frames = 0;

    public float Fps { get; set; }

	void Awake()
	{
		Application.targetFrameRate = 60;
	}

    // Use this for initialization
    void Start () {
        f_LastInterval = Time.realtimeSinceStartup;

        i_Frames = 0;
    }
	
	// Update is called once per frame
	void Update () {
        ++i_Frames;

        if (Time.realtimeSinceStartup > f_LastInterval + f_UpdateInterval)
        {
            Fps = i_Frames / (Time.realtimeSinceStartup - f_LastInterval);
            //f_Fps = i_Frames / time;

            i_Frames = 0;

            f_LastInterval = Time.realtimeSinceStartup;
        }


    }
}
