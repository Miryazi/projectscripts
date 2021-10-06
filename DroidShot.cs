using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MixedUp
{
#if USING_OBSERVABLE
    public class DroidShot : MonoBehaviourPun, IPunObservable
#else
    public class DroidShot : MonoBehaviourPun
#endif
    {
        enum HitType { LightSabre, Shield, Nothing }

        public float maxError = 0.001f;
        public AudioSource hitAudio = null;

        //USING BELOW
        public float oobTolerance = 0.2f;
        private Transform _turret = null;

        ParticleSystem particle = null;
        TrailRenderer bulletTrail = null;


        private void Awake()
        {
            _turret = transform.parent;
            particle = GetComponent<ParticleSystem>();
            DroidTurret.RegisterShot(this);
            GetComponent<Rigidbody>().isKinematic = true;
            bulletTrail = GetComponent<TrailRenderer>();
            bulletTrail.enabled = false;
            
        }

        bool _isFlying = false;
       public bool IsFlying
        {
            get { return _isFlying; }
            private set { _isFlying = value;  }
        }


        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Droid")
            {
                Debug.Log("Bullet dont do anything");
                return;
            }
            else if(collision.gameObject.tag == "Bullet")
            {
                Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), collision.collider);
                return;
            }
            else
            {
                hitAudio.Play();
                GetComponent<MeshRenderer>().enabled = false;
                particle.Play();
                StartCoroutine("homeDelay");

#if WORK_IN_PROGRESS
                if ( collision.gameObject.GetComponent<Shield>() ) {

                    PhotonView clientView = collision.gameObject.GetComponent<PhotonView>();
                    if (clientView != null)
                    {

                        /// WHat did I REALLY hit
                        /// 

                        clientView.RPC("ShieldHit", RpcTarget.All);
                    }
                }
#endif
                // }

            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Droid")
            {
                Debug.Log("OntriggerEntered");
                return;
            }
            else          
            {
                hitAudio.Play();
                particle.Play();
                GetComponent<MeshRenderer>().enabled = false;
                StartCoroutine("homeDelay");
            }
            if (other.gameObject.tag == "Bullet")
            {
                Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), other);
                return;
            }
        }

#if WORK_IN_PROGRESS
        [PunRPC]
        void BulletHit( int viewId, HitType hittype )
        {
            PhotonView clientView = PhotonNetwork.GetPhotonView(viewId);
            //clientView.transform
        }
#endif

#if USING_OBSERVABLE
        Vector3 initShotPos = Vector3.zero;
        Vector3 initShotVel = Vector3.zero;
        float initTime = 0f;
#endif

        [PunRPC]
        void BulletShot( Vector3 worldPos, Vector3 worldVelocity)
        {
            //clientView.transform
            //Debug.Log("I'm being shot! I'm no longer kinematic!!!");

            transform.SetParent(null);
            transform.position = worldPos;

            if (true)
            {
                Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);
                Quaternion dq = Quaternion.FromToRotation(Vector3.up, localVelocity);
                transform.localRotation = transform.localRotation * dq;
                GetComponent<Rigidbody>().freezeRotation = true;
            }
            else
            {
                Vector3 worldYDir = transform.up;
                Quaternion dq = Quaternion.FromToRotation(worldYDir, worldVelocity);
                transform.rotation = dq * transform.rotation;
            }

            //Vector3 rotatedDir = dq * worldYDir;

            GetComponent<Rigidbody>().isKinematic = false;
            GetComponent<Rigidbody>().velocity = worldVelocity;
            GetComponent<CapsuleCollider>().enabled = true;
            bulletTrail.enabled = true;

#if USING_OBSERVABLE
            initShotPos = worldPos;
            initShotVel = worldVelocity;
            initTime = Time.fixedTime;
#endif
            IsFlying = true;
            Debug.Log("Shot Bullet!");
        }

        public void Shoot( Vector3 worldPos,  Vector3 worldVelocity, float delay )
        {
            photonView.RPC("BulletShot", RpcTarget.All, worldPos,  worldVelocity);
            StartCoroutine(DelayedShot (worldPos, worldVelocity, delay));
        }

        void BulletGoHome()
        {
            //put the bullet back home   
            bulletTrail.enabled = false;
            Debug.Log("I'm going home!");
            transform.SetParent( _turret );

            GetComponent<Rigidbody>().freezeRotation = false;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().isKinematic = true;
            transform.localPosition = Vector3.zero;

            GetComponent<CapsuleCollider>().enabled = false;
            IsFlying = false;
        }

        //USING BELOW
        IEnumerator DelayedShot(Vector3 startPos, Vector3 velocity, float delay)
        {
            //checks if it is no longer falling or if it is wayyyy far away
            yield return new WaitForSeconds(delay);
            BulletShot(startPos, velocity);
            
            while (IsFlying)
            {

                yield return new WaitForSeconds(1f);
                if (transform.position.sqrMagnitude > 144f)
                {
                    GetComponent<MeshRenderer>().enabled = false;
                    particle.Play();
                    StartCoroutine("homeDelay");
                }
            }

        }

        IEnumerator homeDelay()
        {
            yield return new WaitForSeconds(.50f);
            BulletGoHome();
            GetComponent<MeshRenderer>().enabled = true;
        }

#if USING_OBSERVABLE
        //---------------------------------------------------------------------------------
        //OBSERVABLE
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                Vector3 ghostPos = initShotPos + (initShotVel * (Time.fixedTime - initTime));
                Vector3 dpos = transform.position - ghostPos;
                if ( Vector3.Dot(dpos, dpos) > maxError )
                {
                    stream.SendNext(transform.position);
                    stream.SendNext(GetComponent<Rigidbody>().velocity);
                }

            }
            else
            {
                transform.position = (Vector3)stream.ReceiveNext();
                GetComponent<Rigidbody>().velocity = (Vector3)stream.ReceiveNext();
            }
        } 
#endif
    }
}
