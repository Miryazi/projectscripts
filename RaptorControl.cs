using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace GoalTech
{

    public class RaptorControl : MonoBehaviour
    {

        bool isAllowed = true;

        public GameObject goalBall;
        public Transform ball;
        public Transform home;
        public GameObject secretBone;
        public Transform ballHome;
        public Transform rapHead = null;

        public GameObject headNub;
        private BoxCollider hnCollider;
        public enum States { Home, GoingHome, GoingToGoal, AtGoal };
        public States currentState = States.Home;
        Animator animControl;
        NavMeshAgent agent;

        AudioSource audioSource;


        public AudioSource audioSourceTalk;
        public AudioSource audioSourceRoar;
        public AudioClip[] audioClipTalk;
        public AudioClip[] audioClipRoar;

        BoxCollider ballCollider = null;
        public bool boneInPlay = false;
        bool hitDetected = false;

        float floorHeight = 4.05f;
        float heightAllowance = 0f;

        bool redundant = false;

        public Animator anim;

        static public bool startFacts = false;

   
        void Start()
        {
            animControl = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            ballCollider = goalBall.GetComponent<BoxCollider>();
            hnCollider = headNub.GetComponent<BoxCollider>();

            hnCollider.enabled = false;
            agent.isStopped = true;
            boneInPlay = false;

            audioSource = GetComponent<AudioSource>();
        }


        void Update()
        {
            switch (currentState)
            {
                case States.Home:
                    {
                        startFacts = false;
                        redundant = false;
                        hnCollider.enabled = false;
                        //Debug.Log("stopped at home");
                        animControl.SetBool("isRunning", false);
                        agent.isStopped = true;
                        animControl.SetBool("isIdle", true);
                        float speed = 2.0f;
                        var targetRotation = Quaternion.LookRotation(home.transform.position - transform.position);

                        Vector3 dirFromAtoB = (home.transform.position - transform.position).normalized;
                        float dotProd = Vector3.Dot(dirFromAtoB, transform.forward);
                        //Debug.Log("dotProd: " + dotProd);

                        if (dotProd > 0.998f) // results range is .996 to .999
                        {

                            boneInPlay = false;
                        }

                        // Smoothly rotate towards the target point.
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed * Time.deltaTime);

                        if (hitDetected == true)
                        {
                        
                            heightAllowance = 5f;
                            boneInPlay = true;
                            hitDetected = false;
                        }
                        if ((ball.transform.position.y < heightAllowance) && boneInPlay && isAllowed)
                        {
                            isAllowed = false;
                            StartCoroutine(AllowedTimer());
                           
                            currentState = States.GoingToGoal;
                            break;
                        }

                        StartCoroutine(WaitCoroutine());

                        break;
                    }
                case States.GoingHome:
                    {
                        //Debug.Log("STATE: GOING HOME");
                        boneInPlay = false;
                      
                        animControl.SetBool("isIdle", false);
                        animControl.SetBool("isRunning", true);
                        agent.destination = home.position;
                        
                        agent.isStopped = false;

                        if (!agent.pathPending)
                        {
                            if (agent.remainingDistance <= agent.stoppingDistance)
                            {
                                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                                {
                                    animControl.SetBool("isRunning", false);
                                    animControl.SetBool("isIdle", true);
                                    //Debug.Log("Should detect if home");
                                    currentState = States.Home;

                                    startFacts = true;
                                    TextController.isAlreadyDone = false;
                                }
                            }
                        }
                        //check if he has made it home, if so move to states.Home
                        break;
                    }
                case States.GoingToGoal:
                    {
                        TestuBC.atHome = false;

                        animControl.SetBool("isIdle", false);
                        animControl.SetBool("isRunning", true);
                        if(redundant == false)
                        {
                            agent.destination = ball.position;
                            redundant = true;
                        }
                        agent.isStopped = false;
                        hnCollider.enabled = true;
                   

                        if (!agent.pathPending)
                        {
                            if (agent.remainingDistance <= (agent.stoppingDistance + 2f))
                            {
                                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0)
                                {
                                    //Debug.Log("close enough to goal");
                                    StartCoroutine(atGoal());
                                }
                            }
                        }


                        break;
                    }
                case States.AtGoal:
                    {
                        break;
                    }
                default:
                    {
                       
                        break;
                    }
            }
        }

        IEnumerator atGoal()
        {
            //Debug.Log("Started Coroutine at timestamp : " + Time.time);
            animControl.SetBool("isRunning", false);
            animControl.SetBool("atGoal", true);

            yield return new WaitForSeconds(.5f);

            //Debug.Log("Finished Coroutine at timestamp : " + Time.time);
            //Debug.Log("Have Bone");
            animControl.SetBool("atGoal", false);
            animControl.SetBool("isRunning", true);
            currentState = States.GoingHome;
        }

        void MessageAccept()
        {
            //Debug.Log("ACCEPTED MESSAGE");
            hitDetected = true;
        }

       
        public void GrabBone()
        {
            Debug.Log("********************************************* I made it to GrabBone");
            BroadcastMessage("TurnOn");

          
            secretBone.transform.parent = rapHead;
            secretBone.transform.localRotation = Quaternion.Euler(80f, 0, 0);
            secretBone.transform.localPosition = Vector3.zero;

        
            Rigidbody rb2 = goalBall.GetComponent<Rigidbody>();
            rb2.isKinematic = true;
            goalBall.transform.position = ballHome.position;
            MeshRenderer mr2 = goalBall.GetComponent<MeshRenderer>();
            mr2.enabled = false;
            MeshRenderer mr4 = secretBone.GetComponent<MeshRenderer>();
            mr4.enabled = false;

        }

        public void Step()
        {
            Debug.Log("FootStep");
            audioSource.Play(0);
        }


        public void RaptorTalk()
        {
            audioSourceTalk.clip = audioClipTalk[Random.Range(0, audioClipTalk.Length)];
            audioSourceTalk.Play();
        }

        public void RaptorRoar()
        {
            audioSourceRoar.clip = audioClipRoar[Random.Range(0, audioClipRoar.Length)];
            audioSourceRoar.Play();
        }

        IEnumerator WaitCoroutine()
        {
            yield return new WaitForSeconds(5);

            TestuBC.atHome = true;
        }

        IEnumerator AllowedTimer()
        {
            yield return new WaitForSeconds(10);

            isAllowed = true;
        }

    }

}