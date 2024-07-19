using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Gameplay;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Animator))]
    public class MovementRigidbody : NetworkBehaviour
    {
        [SerializeField] float m_MoveSpeedMultiplier = 1f;
        [SerializeField] float m_AnimSpeedMultiplier = 1f;
        [SerializeField] ParticleSystem fireball;


        public Transform m_FollowTarget;
        public Transform m_LookTarget;
        private Transform m_Cam;
        private Vector3 m_Move;
        private CinemachineVirtualCameraBase m_VCam;
        private Teammate teammate;
        Vector3 m_GroundNormal;
        bool m_IsGrounded;

        public Rigidbody m_Rigidbody { get; set; }
        Animator m_Animator;
        Vector3 move;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Cursor.lockState = CursorLockMode.Locked;
            if (!IsClient || !IsOwner)
            {
                enabled = false;
                return;
            }
            m_Cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
            teammate = GetComponent<Teammate>();
            m_VCam = m_Cam.GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCameraBase>(); // this is fine since it only happens once.
            m_VCam.Follow = m_FollowTarget;
            m_VCam.LookAt = m_LookTarget;
            m_Animator = GetComponent<Animator>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
                if (m_Animator.GetInteger("Atack") != 1)
                {
                    if (fireball.isPlaying)
                        fireball.Stop();
                    m_Animator.SetInteger("Atack", 1);
                }

            if (Input.GetMouseButton(1))
                if (m_Animator.GetInteger("Atack") != 2f && teammate.Ulta >= 99)
                {
                    if (fireball.isPlaying)
                        fireball.Stop();
                    m_Animator.SetInteger("Atack", 2);
                }
        }

        private void FixedUpdate()
        {
            m_Move.x = Input.GetAxis("Horizontal");
            m_Move.z = Input.GetAxis("Vertical");

            m_Move = m_Move.z * m_Cam.forward + m_Move.x * m_Cam.right;

            m_Move *= 0.5f;
            if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 2f;
            Move(m_Move.x, m_Move.z);
        }
        
        public void Move(float moveX, float moveZ)
        {
            move.x = moveX;
            move.z = moveZ;
            m_Animator.applyRootMotion = true;

            if (move.magnitude > 1.1f)
                m_AnimSpeedMultiplier = 1.15f;
            else
                m_AnimSpeedMultiplier = 1f;

            move = transform.InverseTransformDirection(move);
            move = Vector3.ProjectOnPlane(move, m_GroundNormal);
            transform.rotation = Quaternion.Euler(0, m_VCam.transform.eulerAngles.y, 0);
            CheckGroundStatus();

            if (m_Rigidbody.velocity.magnitude > 7f && fireball.isPlaying)
                fireball.Stop();
            else if (m_Rigidbody.velocity.magnitude < 7f && !fireball.isPlaying)
                fireball.Play();

            if (!m_IsGrounded)
                HandleAirborneMovement();

            UpdateAnimator(move);

        }


        void UpdateAnimator(Vector3 move)
        {
            m_Animator.SetFloat("Horizontal", move.x, 0.1f, Time.deltaTime * 0.5f);
            m_Animator.SetFloat("Vertical", move.z, 0.1f, Time.deltaTime * 0.5f);
            m_Animator.speed = m_AnimSpeedMultiplier;
        }

        public void OnAnimatorMove()
        {
            if (Time.deltaTime > 0)
            {
                Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;
                v.y = 0;
                m_Rigidbody.velocity = v;
            }
        }

        void HandleAirborneMovement()
        {
            Vector3 extraGravityForce = (Physics.gravity * 5f) - Physics.gravity;
            m_Rigidbody.AddForce(extraGravityForce);
        }

        void CheckGroundStatus()
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, 0.01f))
            {
                m_GroundNormal = hitInfo.normal;
                m_IsGrounded = true;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundNormal = Vector3.up;
            }
        }

        public void IncreaseSpeed(float value) => m_MoveSpeedMultiplier = value > 1 ? value : m_MoveSpeedMultiplier;
    }
}