using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoalTech
{
    public class ThrowBall : MonoBehaviour
    {
        Rigidbody rb = null;
        Transform cannonParent = null;
        public Transform secretBone = null;
        public Transform rapHead = null;
        public Transform home = null;

        public bool isGoingHome = false;
        public bool isReadyForPickup = false;

        public float boneDropOff = 1.4f;
        // Start is called before the first frame update
        void Start()
        {
            cannonParent = transform.parent;
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyUp("space"))
            {
                print("space key was pressed");
                rb.useGravity = true;
                rb.isKinematic = false;
                transform.parent = null;
                rb.AddForce(new Vector3(1, 15, -20) * Random.Range(10, 17));
            }

            if (Input.GetKeyUp("r"))
            {
                transform.parent = cannonParent;
                this.transform.localPosition = Vector3.zero;
            }

            if (Vector3.Distance(transform.position, home.position) > 2f)
            {
                isGoingHome = true;
            }

            if (isGoingHome == true)
            {
                if (Vector3.Distance(transform.position, home.position) < boneDropOff)
                {
                    Debug.Log("bone should go back to home");
                    transform.parent = home;
                    transform.localPosition = Vector3.zero;


                    isGoingHome = false;
                }
            }

            if (transform.parent == home && Vector3.Distance(transform.position, rapHead.position) < 1.5f)
            {
                GetComponent<MeshRenderer>().enabled = true;
            }
        }

        public void goHomeBone()
        {
            transform.parent = home;
            transform.localRotation = Quaternion.Euler(80, 0, 0);
            transform.localPosition = Vector3.zero;
        }

        private void OnTriggerEnter(Collider other)
        {

            if (other.tag == "ground")
            {
                rb.isKinematic = true;
            }
            if (other.tag == "raptor" && isGoingHome)
            {
                Debug.Log("Uhm");
                secretBone.transform.parent = rapHead;
                secretBone.transform.localRotation = Quaternion.Euler(80f, 0, 0);
                secretBone.transform.localPosition = Vector3.zero;

                goHomeBone();
                GetComponent<MeshRenderer>().enabled = false;
            }
        }

    }
}

