using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyController : MonoBehaviour
{
    private const float INTELLIGENCE_MULTIPLIER = 4, INTELLIGENCE_MAX = 1000,
                        ACTION_TIMER_THRESOLD_MIN = 5.0f, ACTION_TIMER_THRESOLD_MAX = 10.0f,
                        TURN_COLLISION_DISTANCE = 0.3f, PLAYER_DETECTION_RADIUS_MULTIPLIER = 20f,
                        SPEED_FACTOR = 75f, ATTACK_SPEED_FACTOR = 3.5f, ROTATION_FACTOR = 45f,
                        ATTACK_DISTANCE = .65f, ATTACK_DISTANCE_MAX = 2f, GIVE_UP_ATTACK_DISTANCE_MULTIPLIER = 20f,
                        SLOPE_ANGLE_MAX = 60f;

    [SerializeField]
    private EnemyData enemyData;
    private Enemy enemyClass;

    [SerializeField]
    private Health enemyHealth;

    [SerializeField]
    private CharacterController enemyController;

    private Animator enemyAnimator;
    private Rigidbody rigidbody;

    private enum NPCActivity
    {
        Idle = 0, Walk = 1, Turn = 2, Attack, Die
    }

    private NPCActivity npcActivity;

    private bool isAttacking,
                 isBlocked,
                 hasTurned;

    private float actionTimer, actionThresold,
                  turnDetectionDistance,
                  playerDetectionRadius,
                  giveUpFarness; //what distance from player should I give up attacking?

    
    //Misc
    private GameObject playerAttacking;
    private Animator playerAttackingAnimator;
    private Health playerAttackingHealth;
    private PlayerCameraShake playerCameraShake, playerAimCameraShake;
    private StarterAssets.ThirdPersonController playerAttackingThirdPersonController;




    private void Start()
    {
        Initialilize();
    }

    private void Initialilize() 
    {
        //classes
        enemyClass = new Enemy(enemyData.speed, enemyData.damage, enemyData.intelligence, enemyData.attackDistance, enemyData.maxHealth, enemyData.enemyType);
        enemyHealth = new Health(enemyClass.maxHealth, HealthType.Enemy);
        enemyController = GetComponent<CharacterController>();
        enemyAnimator = GetComponentInChildren<Animator>();
        rigidbody = GetComponent<Rigidbody>();

        //timer stuff
        actionThresold = INTELLIGENCE_MULTIPLIER * (enemyClass.intelligence / INTELLIGENCE_MAX) + Random.Range(ACTION_TIMER_THRESOLD_MIN, ACTION_TIMER_THRESOLD_MAX);
        actionTimer = 0;

        //turn data
        turnDetectionDistance = transform.localScale.z + TURN_COLLISION_DISTANCE * enemyClass.intelligence ; // INTELLIGENCE_MAX);

        //randomize first action
        npcActivity = (NPCActivity)Random.Range(0, 2);

        //find player data
        playerDetectionRadius = enemyClass.intelligence * PLAYER_DETECTION_RADIUS_MULTIPLIER;
        giveUpFarness = enemyClass.intelligence * ATTACK_DISTANCE * GIVE_UP_ATTACK_DISTANCE_MULTIPLIER; //distance from player were enemy gives up attacking
    }

    public void TakeDamage(float _damage)
    {
        enemyHealth.TakeDamage(_damage);
        Debug.Log("Life left is" + enemyHealth.currentHealth);
    }

    private void Update()
    {
        //if enemy health is finished, die
        if(enemyHealth != null && enemyHealth.currentHealth <= 0)
        {
            Die();
            return;
        }

        //do enemy shit
        AIRandomizer();

        //attack
        if (isAttacking && playerAttacking != null)
            Attack(playerAttacking);
    }

    private void LateUpdate()
    {
        //always look for a player to attack
        FindPlayer();
        TargetOntopOrBelowMonster();
        StandUpright();
    }

    public void AIRandomizer() 
    {
        if (!isAttacking) 
        {
            if(actionTimer >= actionThresold) 
            {
                actionTimer = 0;
                SwitchActivity();
            }
            actionTimer += Time.deltaTime;
            Act();
        }
    }

    private void Act() 
    {
        switch (npcActivity) 
        {
            case NPCActivity.Walk:
                Move();
                break;
            case NPCActivity.Turn:
                Turn();
                break;
            case NPCActivity.Idle:
                Idle();
                break;
            default:
                break;
        }    
    }
    private void SwitchActivity() 
    {
        int decision = Random.Range(0, 6);
        switch (decision) 
        {
            case 0: case 1: case 2:
                npcActivity = NPCActivity.Walk;
                break;
            case 3: case 4:
                npcActivity = NPCActivity.Turn;
                hasTurned = false;
                break;
            case 5:
                npcActivity = NPCActivity.Idle;
                break;
            default:
                break;
        }
    }
    public void Move(bool inAttackMode = false)
    {
        isBlocked = IsBlocked();
        float zMove;

        //Just use regular walking and then detect colliders to stop and then turn away from collider

        //walk normally
        if (!inAttackMode) 
        {
            enemyAnimator.SetTrigger("GoWalk");
            zMove = (enemyClass.speed /SPEED_FACTOR) * Time.deltaTime;
        }

        else//run
        {
            enemyAnimator.SetTrigger("GoRunning");            
            zMove = (enemyClass.speed /SPEED_FACTOR) * ATTACK_SPEED_FACTOR * Time.deltaTime; //attack speed
        } 

        if (!isBlocked)
            transform.position += transform.TransformDirection(0, 0, zMove);
        else
        {
            if (isAttacking)
            {
                GoRoundObstacle(playerAttacking.transform, transform.forward);
            }
        }

        //for bats exclusive
        if(enemyClass.enemyType == EnemyType.Demobats) 
        {
            float yWave = 2f * (Mathf.Sin(Time.deltaTime));
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + yWave, transform.localPosition.z);
        }


        npcActivity = NPCActivity.Walk;
    }

    public void Turn(float angle = 0)
    {
        if(angle != 0 && !hasTurned)
        {
            float yRot = angle * ROTATION_FACTOR * Time.deltaTime;
            //turn
            transform.Rotate(0, yRot, 0);
            hasTurned = true;
        }
        if (!hasTurned) 
        {
            npcActivity = NPCActivity.Turn;
            do
            {
                float yRot = Random.Range(-360.0f, 360.0f) * ROTATION_FACTOR * Time.deltaTime;
                //turn
                transform.Rotate(0, yRot, 0);
                isBlocked = IsBlocked();

            }while (isBlocked);
                hasTurned = true;
        }

        //When turning play Idle animation
        npcActivity = NPCActivity.Idle;
        enemyAnimator.SetFloat("IdleState", Random.Range(0, 2));
        enemyAnimator.SetTrigger("GoIdle");
    }

    private void Idle() 
    {
        npcActivity = NPCActivity.Idle;
        enemyAnimator.SetFloat("IdleState", Random.Range(0, 2));
        enemyAnimator.SetTrigger("GoIdle");   
    }

    private void Attack(GameObject playerToAttack)
    {
        if (playerAttackingThirdPersonController.isDead)
        {
            LeaveDeadBody();
            return;
        }
        isAttacking = true;
        npcActivity = NPCActivity.Attack;

        //look at player on y axis
        float xRot, zRot;
        xRot = transform.localEulerAngles.x;
        zRot = transform.localEulerAngles.z;

        if (!isBlocked)
        {
            transform.LookAt(playerToAttack.transform);
            transform.localEulerAngles = new Vector3(xRot, transform.localEulerAngles.y, zRot);
        }

        //move with greater speed towards them
        //stop moving in an offset infront of them
        float offset = 0.75f;
        bool xOffsetReached = Mathf.Abs(transform.position.x - playerToAttack.transform.position.x) < ATTACK_DISTANCE + offset + enemyClass.attackDistance;
        bool zOffsetReached = Mathf.Abs(transform.position.z - playerToAttack.transform.position.z) < ATTACK_DISTANCE + offset + enemyClass.attackDistance;

        if (!xOffsetReached || !zOffsetReached)
            Move(true);
        else
        {
            Attack(playerAttackingThirdPersonController);
        }

        /*//for bats exclusive
        if (enemyClass.enemyType == EnemyType.Demobats && transform.position.y > (playerToAttack.transform.position.y + (playerToAttack.transform.localScale.y /2)))
        {
            transform.localPosition = new Vector3(transform.localPosition.x, playerToAttack.transform.localPosition.y - (playerToAttack.transform.localScale.y / 2), transform.localPosition.z);
        }*/
    }

    private void Attack(StarterAssets.ThirdPersonController playerToAttack) //This attack method refers to the action of attack, for instance a hit on the player
    {
        //play animation
        enemyAnimator.SetTrigger("GoAttack");
        enemyAnimator.ResetTrigger("GoRunning");
        Debug.Log("Attacking!!!");
        //play sfx        

        //for bats exclusive
        if (enemyClass.enemyType == EnemyType.Demobats)
        {
            float zWave = 0.8f * (Mathf.Tan(Time.deltaTime));
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + zWave);
        }
    }

    public void DamagePlayer()
    {
        if (!playerAttacking)
            return;
        Debug.Log("ATTACKED!");
        playerAttackingAnimator.SetFloat("DamageRand", Random.Range(0, 2));
        playerAttackingAnimator.SetTrigger("TakeDamage");
        playerAttackingHealth.TakeDamage(enemyClass.damage);
        if(playerCameraShake)
            playerCameraShake.Shake(enemyClass.damage % 7, enemyClass.damage % 2);
        if (playerAimCameraShake)
            playerAimCameraShake.Shake(enemyClass.damage % 10, enemyClass.damage % 2);

    }

    private void FindPlayer() 
    {
        bool isPlayerObject;
        if (!isAttacking && playerAttacking == null) 
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, playerDetectionRadius);
            foreach(Collider collider in colliders) 
            {  
                var c = collider.gameObject;
                isPlayerObject = c.CompareTag("Player") && !c.GetComponent<StarterAssets.ThirdPersonController>().isDead;
                if (isPlayerObject) 
                {
                    enemyAnimator.SetTrigger("GoFindPlayer");
                    //play vfx
                    playerAttacking = c;
                    playerAttackingAnimator = playerAttacking.GetComponent<Animator>();
                    playerAttackingHealth = playerAttacking.GetComponent<StarterAssets.ThirdPersonController>().GetHealth();
                    playerCameraShake = playerAttacking.GetComponentInParent<PlayerController>().GetCameraPlayerShake();
                    playerAimCameraShake = playerAttacking.GetComponentInParent<PlayerController>().GetAimCameraPlayerShake();
                    playerAttackingThirdPersonController = playerAttacking.GetComponent<StarterAssets.ThirdPersonController>();
                    Attack(c.gameObject);
                    return;
                }
            }
        }
        GiveUpAttack();
    }

    private void GiveUpAttack()
    {
        if(playerAttacking != null)
        {
            float xFarness, zFarness;
            xFarness = transform.position.x - playerAttacking.transform.position.x;
            zFarness = transform.position.z - playerAttacking.transform.position.z;

            if(Mathf.Abs(xFarness) > giveUpFarness || Mathf.Abs(zFarness) > giveUpFarness) 
            {
                Debug.Log("Gave Up!");
                isAttacking = false;
                playerAttacking = null;
                playerAttackingAnimator = null;
                playerAttackingHealth = null;
                playerCameraShake = null;
                playerAimCameraShake = null;
                playerAttackingThirdPersonController = null;
            }
        }
    }

    private void LeaveDeadBody()
    {
        if (isAttacking)
        {
            Debug.Log("Gave Up!");
            isAttacking = false;
            playerAttacking = null;
            playerAttackingAnimator = null;
            playerAttackingHealth = null;
            playerCameraShake = null;
            playerAimCameraShake = null;
            playerAttackingThirdPersonController = null;
            isAttacking = false;
            Move();
        }
    }

    private bool IsBlocked() 
    {
        if (enemyClass.enemyType == EnemyType.Mindflayer)
            return false;
        if (isAttacking) 
        {
            Vector3 origin = transform.position - new Vector3(0, transform.localScale.y * 0.8f, 0);
            RaycastHit hit;
            Debug.DrawRay(origin, transform.TransformDirection(Vector3.forward) * turnDetectionDistance, Color.green);
            if (Physics.Raycast(origin, transform.TransformDirection(Vector3.forward), out hit, turnDetectionDistance, LayerMask.NameToLayer("Ground"), QueryTriggerInteraction.Ignore))
            {

                if (hit.collider != null) 
                {
                    Debug.Log("Called!");
                    if (!hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Weapon") && !hit.collider.CompareTag("Chest"))
                    {
                        Turn(180);
                        return true;
                    }
                    else 
                    {
                        Debug.Log("Blocked!");
                        return true;
                    }
                }
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * turnDetectionDistance, Color.red);
                Debug.Log("Not Blocked!");
                return false;
            }
        }
        return false;
    }

    private void StandUpright()
    {
        Debug.Log("Checking if Upright");
        bool x, y, z;
        x = transform.localEulerAngles.x > SLOPE_ANGLE_MAX || transform.localEulerAngles.x < -SLOPE_ANGLE_MAX;
        y = transform.localEulerAngles.y > SLOPE_ANGLE_MAX || transform.localEulerAngles.y < -SLOPE_ANGLE_MAX;
        z = transform.localEulerAngles.z > SLOPE_ANGLE_MAX || transform.localEulerAngles.z < -SLOPE_ANGLE_MAX;

        if (x)
            transform.localEulerAngles = new Vector3(CloserTo(transform.localEulerAngles.x, -SLOPE_ANGLE_MAX, SLOPE_ANGLE_MAX), transform.localEulerAngles.y, transform.localEulerAngles.z);
        if (z)
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, CloserTo(transform.localEulerAngles.z, -SLOPE_ANGLE_MAX, SLOPE_ANGLE_MAX));
    }

    private void GoRoundObstacle(Transform target, Vector3 startDir)
    {
        transform.Rotate(0, 10, 0);
    }

    private void TargetOntopOrBelowMonster() //if player tries to jump and stay over monster
    {
        if (isAttacking)
        {
            //check if the player is within a range of the monster's x and z axes (nearly the same pos)
            bool px, nx, pz, nz;
            px = (playerAttacking.transform.position.x < (transform.position.x + (transform.localScale.x/2)));
            nx = (playerAttacking.transform.position.x > (transform.position.x - (transform.localScale.x/2)));
            pz = (playerAttacking.transform.position.z < (transform.position.z + (transform.localScale.z / 2)));
            nz = (playerAttacking.transform.position.z > (transform.position.z - (transform.localScale.z / 2)));

            if (px && nx && pz && nz) 
            {
                rigidbody.AddForce(0, 0, 5f * enemyClass.intelligence, ForceMode.Impulse); //repulse the monster away from the player
            }
        }
    }

    public void Die()
    {
        Debug.Log("I died!");
        //Destroy(gameObject);

        //if Vecna dies, win game
        if (enemyClass.enemyType == EnemyType.Vecna)
            FindObjectOfType<WaveManager>().VecnaKilled();
        SpawnManager.DestroyNetworkObject(gameObject);
    }

    private float CloserTo(float num, float min, float max)
    {
        if (Mathf.Abs(min - num) < Mathf.Abs(max - num))
            return min;
        else
            return max;
    }


}
