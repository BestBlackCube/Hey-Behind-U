using Mono.Cecil.Cil;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Script : MonoBehaviour
{
    [Header("PlayerCheckLength")]
    public bool PlayerAttack = false;
    public float viewAngle;
    public float viewDistance;
    public float DirectPlayerRay;

    [Header("Field Select name & Type")]
    public int walkType;
    public int walkLoop;

    public GameObject PlayerChar;
    public GameObject Block; // this test Object

    [Header("nomalEnemy(1~10)")]
    public string LoopName;

    [Header("PlayerLookEnemy(15, 16)")]

    [Header("PlayerSoundFollow(20)")]
    public int SoundCount = 3;

    [Header("RushEnemy(30)")]
    public float RushRayLength = 0f;
    public bool SetmovePoint = true;
    public Vector3 lastPoint = Vector3.zero;
    public int RayMaxCount = 0;
    

    [Header("followPlayerEnemy(50)")]
    public int FollowCount = 9;

    [Header("Enemy Setting")]
    public bool SetmoveChange = false;
    public LineRenderer SelectLoop;
    Rigidbody rigid;
    Animator animator;
    NavMeshAgent agent;

    
    
    float ResetTimer = 0f;
    
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if(walkType == 30) RushRayLength = 0;
            rigid.isKinematic = true;
            agent.enabled = true;
            SetmoveChange = false;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        switch(walkType)
        {
            case 1:
                viewAngle = 270f;
                viewDistance = 20f;
                break;
            case 15:
            case 16:
                viewAngle = 270f;
                viewDistance = 50f;
                break;
            case 20:
                viewAngle = 310f;
                viewDistance = 30f;
                break;
            case 30:
                viewAngle = 10f;
                viewDistance = 40f;
                break;
            case 50:
                viewAngle = 360f;
                viewDistance = 15f;
                break;
            case 51:
                viewAngle = 0f;
                viewDistance = 0f;
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SelectLoop == null) FindLoopObject(); // Enemy Object move
        //walkTimer();
        float TargetDistance = Vector3.Distance(PlayerChar.transform.position, transform.position);
        float targetAngle = Vector3.Angle(transform.forward, PlayerChar.transform.position - transform.position);

        if (Input.GetKey(KeyCode.H)) Block.SetActive(false);
        else if (Input.GetKeyUp(KeyCode.H)) Block.SetActive(true);

        DirectPlayerRay = Vector3.Distance(PlayerChar.transform.position, transform.position); // Enemy To Player Distance
        Ray directRay = new Ray(transform.position, PlayerChar.transform.position - transform.position); // Player Check Ray
        RaycastHit[] directName = Physics.RaycastAll(directRay, DirectPlayerRay); // All layer
        foreach (RaycastHit playerhit in directName)
        {
            if(TargetDistance < viewDistance) // Player Enemy Distance in
            {
                if (targetAngle < viewAngle / 2f) // Player Enemy Distance in, Angle in
                {
                    Debug.DrawRay(directRay.origin, directRay.direction * DirectPlayerRay, Color.red);
                    if (DirectPlayerRay <= 2.5f) PlayerAttack = true;
                    else PlayerAttack = false;

                    if (playerhit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                    {
                        SetmoveChange = true;
                        break;
                    }
                    else // Not Player
                    {
                        SetmoveChange = true;
                        break;
                    }
                }
                else // Player Enemy Distance in, Angle out
                {
                    Debug.DrawRay(directRay.origin, directRay.direction * DirectPlayerRay, Color.yellow);
                    if (walkType != 30) SetmoveChange = false;
                }
            }
            else // Player Enemy Distacne out
            {
                Debug.DrawRay(directRay.origin, directRay.direction * DirectPlayerRay, Color.green);
                if (walkType != 30) SetmoveChange = false;
            }
        }

        if (agent.velocity == Vector3.zero && ResetTimer < 3f && SetmoveChange) ResetTimer += Time.deltaTime;
        else if (ResetTimer >= 3f)
        {
            SetmoveChange = false;
            ResetTimer = 0f;
        } 

        switch (walkType) 
        {
            case 1: // nomal Enemy
                animator.SetLayerWeight(1, 1);
                if (!animator.GetBool("walk")) animator.SetBool("walk", true);
                if(PlayerAttack)
                {
                    agent.velocity = Vector3.zero;
                    if(!animator.GetBool("attack")) animator.SetBool("attack", true);
                }
                else
                {
                    if (SetmoveChange) agent.SetDestination(PlayerChar.transform.position);
                    else
                    {
                        float LoopTransform = Vector3.Distance(transform.position, SelectLoop.GetPosition(walkLoop));
                        agent.SetDestination(SelectLoop.GetPosition(walkLoop));
                        if (LoopTransform < 1.5f && walkLoop != SelectLoop.positionCount - 1) walkLoop++;
                        else if (LoopTransform < 1.5f && walkLoop == SelectLoop.positionCount - 1) walkLoop = 0;
                    }
                }
                break;
            case 15: // Player Look front Enemy
                animator.SetLayerWeight(2, 1);
                if(TargetDistance < viewDistance) // distance in
                {
                    float PlayerLookEnemy = Vector3.Dot(PlayerChar.transform.forward, PlayerChar.transform.position - transform.position);
                    float EnemyLookPlayer = Vector3.Dot(transform.forward, PlayerChar.transform.position - transform.position);

                    if (PlayerLookEnemy > 0) // player not Look me
                    {
                        if (targetAngle < viewAngle / 2f)
                        {
                            agent.SetDestination(PlayerChar.transform.position);
                            if (agent.isStopped) agent.isStopped = false; // move true
                        }
                    }
                    else // player Look me
                    {
                        RaycastHit[] LookhitName = Physics.RaycastAll(directRay, DirectPlayerRay); // All layer
                        if (EnemyLookPlayer >= 0 || EnemyLookPlayer < 0) // Enemy 360 Player Looking
                        {
                            agent.destination = PlayerChar.transform.position;
                            foreach (RaycastHit WallHit in LookhitName)
                            {
                                if (WallHit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                                {
                                    if (agent.isStopped) agent.isStopped = false; // Destination Wall transform move
                                    break;
                                }
                                if (WallHit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                                {
                                    if (!agent.isStopped) agent.isStopped = true; // move false
                                    break;
                                }
                            }
                        }
                    }
                }
                break;
            case 16: // Player not Look front Enemy;
                animator.SetLayerWeight(2, 1);
                if (TargetDistance < viewDistance) // distance in
                {
                    float PlayerLookEnemy = Vector3.Dot(PlayerChar.transform.forward, PlayerChar.transform.position - transform.position);
                    float EnemyLookPlayer = Vector3.Dot(transform.forward, PlayerChar.transform.position - transform.position);

                    if (PlayerLookEnemy > 0) 
                        if (targetAngle < viewAngle / 2f)
                        {
                            RaycastHit[] LookhitName = Physics.RaycastAll(directRay, DirectPlayerRay); // All layer
                            if (EnemyLookPlayer >= 0 || EnemyLookPlayer < 0) // Enemy 360 Player Looking
                            {
                                agent.destination = PlayerChar.transform.position;
                                foreach (RaycastHit WallHit in LookhitName)
                                {
                                    if (WallHit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                                    {
                                        if (agent.isStopped) agent.isStopped = false;
                                        break;
                                    }
                                    if (WallHit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                                    {
                                        if (!agent.isStopped) agent.isStopped = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else agent.SetDestination(PlayerChar.transform.position); // player Look me
                }
                break;
            case 20:
                // walk Sound Enemy
                // player walk sound check
                // Enemy sound transform move
                // N radian in player ? -> follow player

                //if (!animator.GetBool("walk")) animator.SetBool("walk", true);
                //if(SetmoveChange)
                //{
                //    if(SoundCount > 0)
                //    {
                //        if (PlayerChar.GetComponent<Player_test>().PlayerDot[SoundCount] != Vector3.zero)
                //        {
                //            agent.SetDestination(PlayerChar.GetComponent<Player_test>().PlayerDot[SoundCount]);
                //            float SoundDistance = Vector3.Distance(transform.position, PlayerChar.GetComponent<Player_test>().PlayerDot[SoundCount]);
                //            if (SoundDistance < 0.3f) SoundCount--;
                //        }
                //    }
                //    else if(SoundCount == 0)
                //    {
                //        if (TargetDistance < 10f) agent.SetDestination(PlayerChar.transform.position);
                //        else agent.SetDestination(PlayerChar.GetComponent<Player_test>().PlayerDot[SoundCount]);
                //    }
                //}
                //else
                //{
                //    // move walkLoop
                //}


                    break;
            case 30: // forward rush Enemy
                animator.SetLayerWeight(4, 1);
                if (!animator.GetBool("walk")) animator.SetBool("walk", true);
                if (SetmoveChange)
                {
                    Ray ray = new Ray(transform.position, transform.forward); // PlayerCheckRay
                    if (SetmovePoint)
                    {
                        transform.LookAt(PlayerChar.transform); // Player Object Look
                        if (agent.enabled)
                        {
                            RushRayLength = 0;
                            agent.enabled = false; 
                        }

                        RushRayLength += 35f * Time.deltaTime; 
                        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 1,
                            transform.position.z), transform.forward * RushRayLength, Color.blue); 
                        RaycastHit[] hitName = Physics.RaycastAll(ray, RushRayLength); 
                        if(RushRayLength >= 100f)
                        {
                            RayMaxCount++;
                            RushRayLength = 0;
                            if (RayMaxCount >= 5)
                            {
                                lastPoint = PlayerChar.transform.position;
                                SetmovePoint = false;
                            }
                        }
                        foreach (RaycastHit WallHit in hitName) // Wallhit(ray) == hitName(ObjectName)?
                            if (WallHit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                            {
                                SetmovePoint = false;
                                break;
                            }
                    }
                    else if (!SetmovePoint && RayMaxCount < 5)
                    {
                        rigid.isKinematic = false; 
                        GetComponent<Rigidbody>().linearVelocity = ray.direction * 30f; 
                    } 
                    else if(!SetmovePoint && RayMaxCount >= 5)
                        transform.position = Vector3.Slerp(transform.position, lastPoint, Time.deltaTime * 10f);
                }
                else
                {
                    if (!agent.enabled) agent.enabled = true; 
                    if (!SetmovePoint) SetmovePoint = true;   
                    float forwardTransform = Vector3.Distance(transform.position, SelectLoop.GetPosition(walkLoop));
                    agent.SetDestination(SelectLoop.GetPosition(walkLoop)); 
                    if (forwardTransform < 1.5f && walkLoop != SelectLoop.positionCount - 1) walkLoop++;
                    else if (forwardTransform < 1.5f && walkLoop == SelectLoop.positionCount - 1) walkLoop = 0;
                }
                break;
            case 50:
                animator.SetLayerWeight(5, 1);
                if (!animator.GetBool("walk")) animator.SetBool("walk", true);
                if (SetmoveChange) agent.SetDestination(PlayerChar.transform.position);
                else
                {
                    float DotTransform = Vector3.Distance(transform.position, PlayerChar.GetComponent<Player_test>().PlayerDot[FollowCount]);
                    if (PlayerChar.GetComponent<Player_test>().PlayerDot[FollowCount] != Vector3.zero)
                        agent.SetDestination(PlayerChar.GetComponent<Player_test>().PlayerDot[FollowCount]);
                    if (DotTransform < 1.5f && FollowCount > 0) FollowCount--;
                }
                break;
            case 51:
                animator.SetLayerWeight(5, 1);
                if (!animator.GetBool("walk")) animator.SetBool("walk", true);
                agent.SetDestination(PlayerChar.transform.position);
                break;
            default:
                this.gameObject.SetActive(false);
                break;
        }
    }
    void FindLoopObject()
    {
        if (LoopName != "") SelectLoop = GameObject.Find(LoopName).GetComponent<LineRenderer>();
        if (PlayerChar == null) PlayerChar = GameObject.Find("Player");
    }
    int walkTimer()
    {
        if (walkType == 0) walkType = Random.Range(1, 2);
        return walkType;
    }
    void OnDrawGizmosSelected()
    {
        if (SetmoveChange) Gizmos.color = Color.red;
        else Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftBoundary * viewDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * viewDistance);
    }
}
