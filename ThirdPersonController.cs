using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using Cinemachine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.Animations;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviourPunCallbacks
    {
        #region References & Variables
        [Header("Player")]
        [Tooltip("Reference to the main player holder")]
        [SerializeField]
        private GameObject Player;
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Magnitude that determines when the player strafes, rotation-wise")]
        public float maxTurnForAnimation = 1.5f;
        [Tooltip("Magintude that determines when the player's walk animations to get triggered")]
        public float maxMagnitudeToAnimateMove = 0.1f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;        

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Header("Player Crouching")]
        [Tooltip("If the character is crouching")]
        public bool Crouching = false;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDCrouch;
        private int _animIDStand;
        private int _animIDStrafeSpeed;
        private int _animIDStrafe;
        private int _animIDWalkDirection;
        private int _animIDIsHoldingWeapon;

        // multiplayer
        private PhotonView _playerView, _playerBodyView;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        [SerializeField]
        private GameObject _mainCamera;
        [SerializeField]
        private CinemachineBrain cBrain;
        [SerializeField]
        private Inventory _inventory;


        // animatiom
        private const float _threshold = 0.01f;
        private bool _hasAnimator;

        // focusing
        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera, _virtualAimCam;
        private bool isAiming, wantsToToggleAim;
        private float aimTransitionTime = 0.2f, deathTransitionTime = 1;

        // Health
        private Health playerHealth;
        public bool isDead;
        private LookAtConstraint[] lookAtConstraints;

        // Vecna resistance
        private bool hasCassettePlayer;

        // bike riding
        [SerializeField]
        private bool isRidingBike;
        BikeController bikeController;

        // crouch
        private float initialColliderHeight, crouchColliderHeight = 1.2f;
        private Vector3 initialColliderCenter, crouchColliderCenter = new Vector3(0, 0.6f, 0);

        // Game Management
        GameManager gameManager;
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDCrouch = Animator.StringToHash("Crouch");
            _animIDStand = Animator.StringToHash("Stand");
            _animIDStrafeSpeed = Animator.StringToHash("StrafeSpeed");
            _animIDStrafe = Animator.StringToHash("Strafe");
            _animIDWalkDirection = Animator.StringToHash("WalkDirection");
            _animIDIsHoldingWeapon = Animator.StringToHash("WeaponType");
        }
        #endregion

        #region Unity Basic Functions
        private void Awake()
        {
            _playerView = Player.GetComponent<PhotonView>();
            _playerBodyView = GetComponent<PhotonView>();
            playerHealth = new Health(1000, HealthType.Player);
            lookAtConstraints = GetComponentsInChildren<LookAtConstraint>();
            gameManager = FindObjectOfType<GameManager>();
        }

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            if (MyPlayer())
            {
                _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

                
                _controller = GetComponent<CharacterController>();
                // set collider details
                initialColliderCenter = _controller.center;
                initialColliderHeight = _controller.height;

                _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
                _playerInput = GetComponent<PlayerInput>();
#else
			    Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

                AssignAnimationIDs();

                // reset our timeouts on start
                _jumpTimeoutDelta = JumpTimeout;
                _fallTimeoutDelta = FallTimeout;
            }
        }

        private void Update()
        {
            if (playerHealth.currentHealth < 0)
                Die();
            if (isDead)
            {
                _animator.SetTrigger("Die");
                return;
            }
            if (MyPlayer())
            {
                _hasAnimator = TryGetComponent(out _animator);

                Crouch();
                FocusAim();
                JumpAndGravity();
                GroundedCheck();
                Move();

            }
        }

        private void LateUpdate()
        {
            if (MyPlayer() && !isDead)
                CameraRotation();
        }
        #endregion

        #region Player Movememnt
        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);

            // also rotate player according to Camera view
            transform.forward = new Vector3(CinemachineCameraTarget.transform.forward.x, transform.forward.y, CinemachineCameraTarget.transform.forward.z);
            // while rotating player, player rotation animation
            if (!_input.sprint && (_input.look.x > maxTurnForAnimation || _input.look.x < -maxTurnForAnimation))
            {
                _animator.SetBool(_animIDStrafe, true);
                _animator.SetFloat(_animIDStrafeSpeed, (_input.look.x / Mathf.Abs(_input.look.x)));
            }
            else
                _animator.SetBool(_animIDStrafe, false);
        }

        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.sprint)
            {
                inputDirection.x = 0;
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(transform.TransformDirection(inputDirection) * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);

                float currentValue, time;
                currentValue = _animator.GetFloat(_animIDWalkDirection);
                time = Time.deltaTime * 5;
                // walk forwards
                if (_input.move.y > maxMagnitudeToAnimateMove)
                    _animator.SetFloat(_animIDWalkDirection, Mathf.Lerp(currentValue, 0, time));
                else if (!_input.sprint)
                {
                    // walk backwards
                    if (_input.move.y < -maxMagnitudeToAnimateMove)
                        _animator.SetFloat(_animIDWalkDirection, Mathf.Lerp(currentValue, -1, time));
                    // walk right
                    if (_input.move.x > maxMagnitudeToAnimateMove)
                        _animator.SetFloat(_animIDWalkDirection, Mathf.Lerp(currentValue, 0.5f, time));
                    // walk left
                    else if (_input.move.x < -maxMagnitudeToAnimateMove)
                        _animator.SetFloat(_animIDWalkDirection, Mathf.Lerp(currentValue, -0.5f, time));
                }

                //Check if the character is not holding any weapon, so they don't just hang their hand in the air
                if (_inventory.weaponManager.isHoldingWeapon())
                    _animator.SetFloat(_animIDIsHoldingWeapon, 0);
                else
                    _animator.SetFloat(_animIDIsHoldingWeapon, 1);
                
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                    // if falling, stand after
                    Stand();
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }


        //Crouch
        private void Crouch()
        {
            if (Grounded && _input.crouch == true)
            {
                if (!Crouching)
                {
                    // update animator to crouch
                    if (_hasAnimator)
                    {
                        _animator.SetTrigger(_animIDCrouch);
                        _animator.SetBool(_animIDStand, false);
                        _controller.height = crouchColliderHeight;
                        _controller.center = crouchColliderCenter;
                        Crouching = true;
                    }
                }
                else
                {
                    Stand();
                }
                _input.crouch = false;
            }

            //If player is sprinting, stop crouching
            if (_animationBlend > 2)
            {
                Stand();
            }
        }
        private void Stand()
        {
            // update animator to crouch
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDStand, true);
                _controller.height = initialColliderHeight;
                _controller.center = initialColliderCenter;
                Crouching = false;
            }
        }

        private void FocusAim()
        {
            if (Input.GetMouseButtonDown(1))
            {
                wantsToToggleAim = true;
                isAiming = !isAiming;
                switch (isAiming)
                {
                    case true:
                        _virtualCamera.Priority = 9;
                        _virtualAimCam.Priority = 10;
                        cBrain.m_DefaultBlend.m_Time = aimTransitionTime;
                        break;
                    case false:
                        _virtualAimCam.Priority = 9;
                        _virtualCamera.Priority = 10;
                        cBrain.m_DefaultBlend.m_Time = aimTransitionTime;
                        break;
                }
                //StartCoroutine();
            }
        }

        private System.Collections.IEnumerator Wait()
        {
            yield return new WaitForSeconds(3);
            wantsToToggleAim = false;
        }

        #endregion

        #region PUN RPC Functions
        [PunRPC]
        private void PickUp(int itemPV_ID, bool isWeapon)
        {
            GameObject item = PhotonView.Find(itemPV_ID).gameObject;
            Debug.Log("Attempted picking");
            if (item)
            {
                Debug.LogError(item.name + "WAS TRIED TO BE PICKED UP BY " + gameObject.name);
                _inventory.PickUpItem(item, isWeapon);
            }
        }

        private void InteractWithDoor(int viewID)
        {
            DoorController door = PhotonView.Find(viewID).GetComponent<DoorController>();
            door.InteractWithDoor();
        }

        private void OpenChest(int viewID, int itemIndex, bool spawnItem)
        {
            ChestController chest = PhotonView.Find(viewID).GetComponent<ChestController>();
            chest.OpenChest(itemIndex, spawnItem);
        }
        #endregion

        #region Collision Checks
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Eggos"))
            {
                other.GetComponent<Pickable>().PickedUp();
                // do animation
                playerHealth.BoostHealth(0, true);
            }
            else if (other.CompareTag("Ammo"))
            {
                other.GetComponent<Pickable>().PickedUp();
                _inventory.weaponManager.RefillAmmo();
            }
            else if (other.CompareTag("CassettePlayer"))
            {
                other.GetComponent<Pickable>().PickedUp(); 
                hasCassettePlayer = true;
                
            }
            else if (other.CompareTag("Coke"))
            {
                other.GetComponent<Pickable>().PickedUp();
                playerHealth.BoostHealth(playerHealth.maxHealth/2, false);

            }
            else if (other.CompareTag("Pizza"))
            {
                other.GetComponent<Pickable>().PickedUp();
                playerHealth.BoostHealth(playerHealth.maxHealth / 4, false);

            }
            else if (other.CompareTag("Hellfire Tee"))
            {
                other.GetComponent<Pickable>().PickedUp();
                //Cosmetic

            }
        }


        private void OnTriggerStay(Collider other)
        {
            RaycastHit hit;
            if (MyPlayer())
            {
                if (other.CompareTag("Weapon") && other.GetComponent<WeaponController>().isPickable)
                {               
                    // tell player they can pick it up here
                    if (_input.otherAction)
                    {
                        int viewID = other.GetComponent<PhotonView>().ViewID;
                        //_playerBodyView.RPC("PickUp", RpcTarget.AllBuffered, viewID, true);
                        PickUp(viewID, true);
                        if (MyPlayer())
                        {
                            Hashtable hash = new Hashtable();
                            hash.Add("ViewID", viewID);
                            hash.Add("IsWeapon", true);
                            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                        }
                        _input.otherAction = false;
                    }
                }
                // to open chest
                else if (other.CompareTag("Chest") && Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit))
                {
                    Debug.Log("Can see chest");
                    if (!hit.collider.CompareTag("Chest"))
                       return;
                    // tell player they can pick it up here
                    if (_input.otherAction)
                    {
                        ChestController chest = other.GetComponent<ChestController>();
                        int chestViewID = other.GetComponent<PhotonView>().ViewID;
                        int rand = chest.GetRandomIntFromChest();
                        Debug.Log("Tried to open chest");
                        OpenChest(chestViewID, rand, true);
                        //other.gameObject.GetComponent<ChestController>().OpenChest();
                        _input.otherAction = false;

                        if (MyPlayer())
                        {
                            Hashtable hash = new Hashtable();
                            hash.Add("ChestController", chestViewID);
                            hash.Add("ChestControllerItemID", rand);
                            hash.Add("SpawnItem", false);
                            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                        }
                    }
                }
                if (other.CompareTag("Door") && other.GetComponent<DoorController>() != null)
                {
                    // tell player they can open door
                    if (_input.otherAction)
                    {
                        DoorController door = other.GetComponent<DoorController>();
                        int doorViewID = door.GetComponent<PhotonView>().ViewID;
                        InteractWithDoor(doorViewID);
                       // other.GetComponent<DoorController>().InteractWithDoor();
                        _input.otherAction = false;

                        if (MyPlayer())
                        {
                            Hashtable hash = new Hashtable();
                            hash.Add("InteractWithDoor", doorViewID);
                            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                        }
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(MyPlayer())
                _input.otherAction = false;
        }
        #endregion

        #region Miscellaneous
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (MyPlayer() && animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (MyPlayer() && animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }

        public Health GetHealth() 
        {
            return this.playerHealth;
        }

        public void Die()
        {
            //Destroy(gameObject.transform.parent.gameObject);
            isDead = true;
            _animator.SetTrigger("Die");
            cBrain.m_DefaultBlend.m_Time = deathTransitionTime;
            if (_virtualCamera)
                _virtualCamera.GetComponent<PlayerCameraShake>().DieAnimation();
            _animator.SetTrigger("Die");
            foreach(LookAtConstraint constraint in lookAtConstraints)
            {
                constraint.enabled = false;
            }
            Debug.Log("YOU DED");
            gameManager.UpdateDeadPlayers();
        }

        public void SetNewThirdPersonProperties(ThirdPersonProps newThirdPerson)
        {
            this.Player = newThirdPerson.player;
            this.CinemachineCameraTarget = newThirdPerson.cinemachineCameraTarget;
            this._mainCamera = newThirdPerson.mainCamera;
            this.cBrain = newThirdPerson.cBrain;
            this._inventory = newThirdPerson.inventory;
            this._virtualCamera = newThirdPerson.virtualCamera;
            this._virtualAimCam = newThirdPerson.virtualAimCam;
        }

        public ThirdPersonProps GetThirdPersonProperties() 
        {
            ThirdPersonProps tp;
            tp.player = this.Player;
            tp.cinemachineCameraTarget = this.CinemachineCameraTarget;
            tp.mainCamera = this._mainCamera;
            tp.cBrain = this.cBrain;
            tp.inventory = this._inventory;
            tp.virtualCamera = this._virtualCamera;
            tp.virtualAimCam = this._virtualAimCam;

            return tp;
        }

        public struct ThirdPersonProps
        {
            public GameObject player;
            public GameObject cinemachineCameraTarget; 
            public GameObject mainCamera;
            public CinemachineBrain cBrain;
            public Inventory inventory;
            public CinemachineVirtualCamera virtualCamera;
            public CinemachineVirtualCamera virtualAimCam;
        }
        #endregion

        public bool MyPlayer()
        {
            if(_playerView.IsMine) 
                return true;
            else 
                return false;
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            //Debug.LogError("Player Properties Updated");
            if(!_playerView.IsMine && targetPlayer == photonView.Owner)
            {
                if(changedProps.ContainsKey("ViewID") && changedProps.ContainsKey("IsWeapon"))
                {
                    // pick up weapons update around server
                    PickUp((int)changedProps["ViewID"], (bool)changedProps["IsWeapon"]);
                    Debug.LogError("Tried Picking Up!");
                }
                if (changedProps.ContainsKey("InteractWithDoor"))
                {
                    // pick up weapons update around server
                    InteractWithDoor((int)changedProps["InteractWithDoor"]);
                    Debug.LogError("Tried Opening Door!");
                }
                if (changedProps.ContainsKey("ChestController"))
                {
                    // pick up weapons update around server
                    OpenChest((int)changedProps["ChestController"], (int)changedProps["ChestControllerItemID"], (bool)changedProps["SpawnItem"]);
                    Debug.LogError("Tried Opening Chest!");
                }

            }
        }
    }
}