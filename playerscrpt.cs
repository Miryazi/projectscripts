using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class playerscrpt : MonoBehaviour
{
    // Use this for initialization
    public static playerscrpt instance;
    public Transform Model;
    public Rigidbody hab;

    public float smoothness = 1.0f;
    Vector3 home = Vector3.zero;
    public float speed = 5;

    public float rollMax = 45;
    public float pitchMax = 45;
    public float disEP = 50f;
    public GameObject endPoint = null;

    public Text coinCounter;
    int coinAm = 0;



    public Slider slider;
    public Slider oSlider;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        hab = GetComponent<Rigidbody>();
        PlatformController.instance.wake();

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 nV = VectInput();
        Debug.DrawLine(Model.transform.position, Model.transform.position + nV.normalized * 10);
        Debug.DrawLine(Model.transform.position, Model.transform.position + nV.normalized * 10);
        nV.Normalize();
        slider.value = nV.z;
        oSlider.value = -nV.x;

        //Quaternion tarRot = Quaternion.Euler(v - home);
        Quaternion tarRot = Quaternion.Euler(pitchMax * nV.x, 0, rollMax * nV.z);
        Model.transform.rotation = Quaternion.RotateTowards(Model.transform.rotation, tarRot, smoothness * Time.deltaTime);


        if (Input.GetKeyDown(KeyCode.Space))
        {
            home.y = BasicSerialThread.instance.currentVector.x; //Yaw, Good.
            home.z = -(BasicSerialThread.instance.currentVector.y); //not good
            home.x = -(BasicSerialThread.instance.currentVector.z);
        }

 

        if (Vector3.Distance(this.transform.position, endPoint.transform.position) < disEP)
        {
            endPoint.SetActive(true);
        }
    }

    private void FixedUpdate()
    {

        Vector3 mV = VectInput();
        float xVal = mV.z;

        float zVal = -(mV.x);
        float step = speed * Time.deltaTime;
        hab.AddForce(xVal * step, 0, zVal * step, ForceMode.Acceleration);

    }
    public float MapRange(float val, float min, float max, float newMin, float newMax)
    {
        return ((val - min) / (max - min) * (newMax - newMin) + newMin);
        // or Y = (X-A)/(B-A) * (D-C) + C
    }

    public Vector3 VectInput()
    {
        Vector3 v = Vector3.zero;

        v.y = 0;// BasicSerialThread.instance.currentVector.x; 
        v.z = -(BasicSerialThread.instance.currentVector.y);
        v.x = -(BasicSerialThread.instance.currentVector.z);

        v.z = Mathf.Clamp(v.z, -rollMax, rollMax);
        v.z = MapRange(v.z, -rollMax, rollMax, -1, 1);

        v.x = Mathf.Clamp(v.x, -pitchMax, pitchMax);
        v.x = MapRange(v.x, -rollMax, rollMax, -1, 1);


        return v;
    }

    public void addCoins()
    {
        coinAm++;
        coinCounter.text = "Coins: " + coinAm.ToString();
    }

    public int retCoinAmount()
    {
        return coinAm;
    }

}
