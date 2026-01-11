using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UIElements;
using TMPro;
using NUnit.Framework;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UI;
using Unity.VisualScripting;

public class Player_test : MonoBehaviour
{
    // player inGame Status
    public int PlayerHp = 100;
    public int runPow = 1;
    // player Component & variable
    public Rigidbody rigid;
    public Animator animator;

    public GameObject playerCamera;
    public Vector3[] PlayerDot;
    public bool ground = true; //2단 점프 방지 변수

    float jumpHeight = 300f;
    public float DotTimer = 0;
    public int DotCount = 0;

    Vector3 movepw = new Vector3(5f, 10f, 5f); //! Vector3 물리 속도값

    RaycastHit hit;

    // player Ladder

    public LayerMask GroundlayerMask;
    public LayerMask StairlayerMask;
    public Transform groundcheck;     //! 플레이어 바닥 인식
    public float maxSlopeAngle = 45f; //최대 경사면 각도

    //----------스태미나 관련 변수-----------------//
    public float maxStamina = 200f;     //최대 스태미나 
    public float NowStamina;        //현재 사용중인 스태미나나
    public float drainStamina = 20f; //? 달릴때 초당 스태미너 소모
    public float drainLadder = 10f; //? 사다리 탈 경우 스태미나 소모
    public float recoveryStamina = 10f; //초당 스태미나 회복
    public float delayStamina = 2f; //스태미나 회복대기 시간
    private float timerStamina = 0f; // 회복 되기 까지 지연시간 타이머
    public UnityEngine.UI.Slider staminaSlider;    //Slider 지정
    //----------스태미나 관련 변수-----------------//
    public int Keycount = 0;
    public int TotalKeys = 5;
    public Text KeyCountText;
    public TMP_Text gameClearTMPText;   //game clear text
    private bool isGameClear = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //private void OnCollisionStay(Collision collision)
    //{
    //    foreach(ContactPoint contact in collision.contacts)
    //    {
    //        RaycastHit hit;
    //        if(Physics.Raycast(transform.position, Vector3.down, out hit, 1.5f))
    //        {
    //            float Sangle = Vector3.SignedAngle(Vector3.up, hit.normal, transform.forward);
    //            Debug.Log("Player Angle : " + Sangle);

    //            float slopeMultiplier = Mathf.Clamp01(1f - (Sangle / 90f));
    //            if (Sangle > 0) PlayerMovemass(0, slopeMultiplier, 0);
    //            else if (Sangle < 0) PlayerMovemass(0, -slopeMultiplier, 0);
    //                Debug.Log("Y Move : " + 5f * slopeMultiplier);

    //        }
    //    }
    //}
    void Start()
    {
        GroundlayerMask = LayerMask.GetMask("Ground", "Default", "Stair", "Player");         //?바닥으로 지정할 이름설정
        StairlayerMask = LayerMask.GetMask("Stair");                     //?계단으로 지정할 레이어마크스
        //Ladder = LayerMask.GetMask("Ladder");                       //? 사다리 지정 레이어마스크
        NowStamina = maxStamina;                // 현재 스태미나를 최대 스태미나로 초기화

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = NowStamina;
        }

        if (gameClearTMPText != null) gameClearTMPText.enabled = false;

    }

    // Update is called once per frame

    void Update()
    {
        playerDead();
        //TODO 캐릭터 점프, 애니메이션 관련 레이케스트 코드 
        Debug.DrawRay(transform.position, Vector3.down * 0.2f, Color.red); //! 레이저시각화
        //! 레이케스트 구현
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.2f, GroundlayerMask, QueryTriggerInteraction.UseGlobal))
        {
            //Debug.Log("Raycast 충돌 오브젝트 : " + hit.collider.gameObject.name);
            //Debug.Log("충돌위치 :" + hit.point);

            ground = false;
            animator.SetBool("jump", false);  //!레이저가 Layermask에 닿으면 점프 애니메이션 종료
            animator.SetBool("Fall", false);  //! 레이저가 닿으면 폴 애니메이션 종료
            animator.SetTrigger("Land"); //! 착지는 한번 신호 보내고 끝
        }
        else
        {
            ground = true;  //공중에 떠있다면 ground는 참
            animator.ResetTrigger("Land");
        }
        UpdateKeyUI();


        //TODO 플레이어 계단 인식 코드
        Vector3 Groundleft = (Vector3.down + Vector3.left).normalized; //왼쪽아래 대각 방향 백터
        Vector3 Groundright = (Vector3.down + Vector3.right).normalized; // 오른쪽아래 대각 방향 백터
        Debug.DrawRay(transform.position, Groundleft * 0.7f, Color.blue);   //왼쪽 대각 시각화
        Debug.DrawRay(transform.position, Groundright * 0.7f, Color.blue);  //오른쪽 대각 시각화
        if (Physics.Raycast(transform.position, Groundleft, out hit, 0.7f, StairlayerMask) ||
            Physics.Raycast(transform.position, Groundright, out hit, 0.7f, StairlayerMask))
        {
            //Debug.Log("Stair detection!");
            rigid.useGravity = false;
        }
        else
        {
            rigid.useGravity = true;
        }

        //! CheckGround(); // 2단 점프 금지 구현
        if (Input.GetKeyDown(KeyCode.Space) && !ground)
        {
            ground = true;
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpHeight);

            animator.SetBool("jump", true); //점프 애니메이션 적용
            animator.ResetTrigger("Land");    // 착지 애니메이션 초기화
        }

        if (rigid.linearVelocity.y <= 0 && ground)    // 플레이어가 하강 중이면 fall 애니메이션 실행
        {
            animator.SetBool("Fall", true);
        }

        //TODO--------------------------- 달리기 코드 // 스태미나 코드------------------------------------
        bool canRun = NowStamina > 0f;  //달릴수 있는지 확인하기 위한 변수

        //Debug.Log($"스테미나 확인 : {NowStamina}");
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) // 애니메이션 코드
        { // W, S, A, D를 눌렀을때 "걷기" 애니메이션이 비활성화 일경우 -> 활성화
            if (Input.GetKey(KeyCode.LeftShift) && canRun)
            {
                if (!animator.GetBool("run")) animator.SetBool("run", true);
                if (animator.GetBool("walk")) animator.SetBool("walk", false);
                runPow = 2;

                //스태미나 소모
                NowStamina -= drainStamina * Time.deltaTime;    //달리때 매 초마다 스태미나 소모
                NowStamina = Mathf.Clamp(NowStamina, 0f, maxStamina);
                timerStamina = 0f;      //플레이어가 달릴때 스태미나가 회복되면 안됨 0f로 초기화   
            }
            else
            {
                if (!animator.GetBool("walk")) animator.SetBool("walk", true);
                if (animator.GetBool("run")) animator.SetBool("run", false);
                runPow = 1;

                //스태미나 회복 // 회복 타이머
                timerStamina += Time.deltaTime;
                if (timerStamina >= delayStamina) //회복 타이머가 스태미나 지연시간보다 클때 //2.1초 가 되면 회복시작
                {
                    NowStamina += recoveryStamina * Time.deltaTime;         // 매 초마다 스태미나 회복
                    NowStamina = Mathf.Clamp(NowStamina, 0f, maxStamina);
                }
            }
        } // Animator 컴포넌트의 "walk" 값을 true로
        else
        { // W, S, A, D를 누르지 않았을 때 "걷기" 애니메이션이 활성화 일경우 -> 비활성화
            if (animator.GetBool("walk") || animator.GetBool("run"))
            {
                animator.SetBool("walk", false); // "walk"값 비활성화
                animator.SetBool("run", false); // "run"값 비활성화
                runPow = 1;
            }
            //정지 했을 경우에도 스태미나 회복
            timerStamina += Time.deltaTime;
            if (timerStamina >= delayStamina)
            {
                NowStamina += recoveryStamina * Time.deltaTime;
                NowStamina = Mathf.Clamp(NowStamina, 0f, maxStamina);
            }
            //정지 할 경우 
        }
        if (staminaSlider != null) staminaSlider.value = NowStamina; //스태미나 UI 동기화
        /*
        rigid.linearVelocity와 transform.forward...등 차이점
        transform : 물리적 이동이 아닌 월드 좌표를 강제적으로 이동시킨다. (순간이동)
        RigidBody : 월드좌표로 강제적 이동이 아닌 물리(속도)를 사용해 이동시킨다.
        
        !+ transform과 RigidBody의 값은 Vector3로 이루어져 있어 서로에게 적용이 가능하다 +!

        transform.position = transform.position +/- rigid.linearVelocity <- 현재 좌표에 RigidBody의 물리값 만큼 더하거나 뺸다
        
        rigidbody의 값에 물리적 이동 값을 대입하여 오브젝트가 물리값에 의하여 점차 이동한다
        rigidbody.linearVelocity = new Vector3(X, Y, Z) / new Vector3(transform.position.x, transform.position.y, transform.position.z)
                                 = transform.forward, right, up  (앞, 오른쪽, 위) forward / -forward : (0, 0, 1 : 0, 0, -1)
                                 = -transform.forward, right, up (뒤, 왼쪽, 아래)   right / -right   : (1, 0, 0 : -1, 0, 0)
                                   (오브젝트가 바라보는 방향으로 이동한다              up / -up      : (0, 1, 0 : 0, -1, 0)
         */

        //? -------------------------------------------- 이동 구현 코드--------------------------------------------------------
        // 1인칭 이동 

        if (playerCamera.GetComponent<Cam>().FirstPlayer == true) // Tab키를 눌러 Camera의 값이 1인칭인가? 3인칭인가?
        {
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
            {
                rigid.linearVelocity = new Vector3(0f, rigid.linearVelocity.y, 0f);
                if (!ground) rigid.linearVelocity = Vector3.zero;
            }
            else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                PlayerMovemass(0, 0, movepw.z); // W
            else if (!Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                PlayerMovemass(-movepw.x, 0, 0); // A
            else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                PlayerMovemass(0, 0, -movepw.z); // S
            else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
                PlayerMovemass(movepw.x, 0, 0); // D
            else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                PlayerMovemass(-movepw.x, 0, movepw.z); // WA
            else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
                PlayerMovemass(movepw.x, 0, movepw.z); // WD
            else if (!Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
                PlayerMovemass(-movepw.x, 0, -movepw.z); // SA
            else if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
                PlayerMovemass(movepw.x, 0, -movepw.z); // SD


            /*
             
            rigid.linearVelocity = new Vecto3()와 기본 식의 차이
            
            new Vector3 같은 경우는 x, y, z의 값 3개를 사용하여 만들어야 하며 forward와 right값에는 x, y, z값이 
            전부 들어있기에 개별적으로 값을 만들어야한다 (서로 간의 더하기 및 곱하기가 힘듬)
            
            rigid.linearVelocity = new Vector3 (x, y, z); 이 안되는 이유
            x값과 z값을 개별적으로 넣게 된다면 x값에서는 z값이 0으로 z값에서는 x값이 0으로 반환 되기 때문이다
            또한 new Vector3의 경우 오브젝트가 바라보는 방향으로 값이 적용되는게 아닌 처음 오브젝트가 바라보는 방향으로
            진행되기 때문에 한가지 방향으로만 간다.

            rigid.linearVelocity = (x) + (z) & (y) 의 차이점
            x, y, z값을 개별적으로 넣을수 있고 필요없는 값은 넣지 않아도 된다
            1키 입력일 경우 rigid.linearVelocity = (transform.forward * movepw.z * runPow) + (Vector3.up * rigid.linearVelocity.y);
            transform.foward(0 * movepw * runPow, 0 * movepw * runPow, 1 * movepw * runPow) + (y값은 실시간 도입)

            2키 입력일 경우 rigid.linarVelocity = (transform.forward * movepw.z * runPow) +
                                (transform.right * movepw.x * runPow) + (Vector3.up rigid.linearVelocity.y)
            transform.foward(0 * movepw * runPow, 0 * movepw * runPow, 1 * movepw * runPow)
            transform.right (1 * movepw * runPow, 0 * movepw * runPow, 0 * movepw * runPow) <- !+ 두개의 값은 곱셈이 아닌 덧셈임! +!
            
            덧셈 일경우 : forward(0, 0, 5f & 10f) + right(5f & 10f, 0, 0) = (0 + 5f & 10f, 0 + 0, 5f & 10f + 0) 
                          = (5f & 10f, 0, 5f & 10f) x와 z값에 값이 들어감 
            곱셈 일경우 : forward(0, 0, 5f & 10f) * right(5f & 10f, 0, 0) = ( 0 * 5f * 10f, 0 * 0, 5f & 10f * 0)
                          = (0, 0, 0) x와 z값에 0이 곱해졌기 때문에 값이 사라짐 ( 또한 Vector3와 Vector3 끼리는 곱셈, 나눗셈이 안됨)
             */
        }
        else // 3인칭 이동
        {
            if (Input.GetKey(KeyCode.W)) // w 키 전방
            {
                if (rigid.linearVelocity.z >= 5f) rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, rigid.linearVelocity.y, movepw.z * runPow); //속도 제한

                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))  // 전진 하면서 A 또는 D 가 눌릴때
                    rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, rigid.linearVelocity.y, (movepw.z - 2) * runPow); //! 전진하면서 좌우가 눌리니까 대각이동 속도 감소 해야 함 z = 5 - 2

                if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) // 전진 하면서 A 와 D 가 안눌릴때
                {
                    rigid.linearVelocity = new Vector3(0f, rigid.linearVelocity.y, rigid.linearVelocity.z); //x값 초기화
                    rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, rigid.linearVelocity.y, movepw.z * runPow); // z = 5 -2
                }
            }

            if (Input.GetKey(KeyCode.S)) // s 키 후방
            {
                if (rigid.linearVelocity.z <= -5f) rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, rigid.linearVelocity.y, -movepw.z * runPow); //속도 제한

                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))  //후진 하면서 A 또는 D 가 눌릴때
                    rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, rigid.linearVelocity.y, (-movepw.z + 2) * runPow); //! 후진하면서 좌우가 눌리니까 대각이동 속도 감소 감소해야 함 z = -5 + 2

                if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) //후진 하면서 A 와 D 가 안눌릴때
                {
                    rigid.linearVelocity = new Vector3(0f, rigid.linearVelocity.y, rigid.linearVelocity.z);  // x값 초기화
                    rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, rigid.linearVelocity.y, -movepw.z * runPow); // z = -5 + 2
                }
            }

            if (Input.GetKey(KeyCode.A)) // A 키 좌측
            {
                if (rigid.linearVelocity.x <= -5f) rigid.linearVelocity = new Vector3(-movepw.x * runPow, rigid.linearVelocity.y, rigid.linearVelocity.z); // 속도제한

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) // 좌로 가면서 W 또는 S 가 눌릴때
                    rigid.linearVelocity = new Vector3((-movepw.x + 2) * runPow, rigid.linearVelocity.y, rigid.linearVelocity.z); //! 좌로 가면서 앞뒤가 눌리면 대각이동 속도 감소해야 함 x = -5 + 2

                if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) //좌로 가면서 W 와 S 가 안눌릴때
                {
                    rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, rigid.linearVelocity.y, 0f); //z 값 초기화
                    rigid.linearVelocity = new Vector3(-movepw.x * runPow, rigid.linearVelocity.y, rigid.linearVelocity.z); // -x = 5
                }
            }

            if (Input.GetKey(KeyCode.D)) // D 키 우측
            {

                if (rigid.linearVelocity.x >= 5f) rigid.linearVelocity = new Vector3(movepw.x * runPow, rigid.linearVelocity.y, rigid.linearVelocity.z); // 속도 제한

                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) //우로 가면서 W 또는 S 가 눌릴 때
                    rigid.linearVelocity = new Vector3((movepw.x - 2) * runPow, rigid.linearVelocity.y, rigid.linearVelocity.z); //! 우로 가면서 앞뒤가 눌리면 대각이동 속도 감소해야 함 x = 5 -2 

                if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) //우로 가면서 W 와 S 가 안눌릴때 
                {
                    rigid.linearVelocity = new Vector3(rigid.linearVelocity.x, rigid.linearVelocity.y, 0f);  // z 값 초기화
                    rigid.linearVelocity = new Vector3(movepw.x * runPow, rigid.linearVelocity.y, rigid.linearVelocity.z); // x = 5 
                }
            }
        }

        //?-------------------------------------------여기 까지 이동 구현------------------------------------------------------------------
        //! 플레이어 회전 구현
        Vector3 moveDirection = new Vector3(rigid.linearVelocity.x, 0f, rigid.linearVelocity.z);

        if (moveDirection != Vector3.zero) //움직이고 있을 때만 회전
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection); //회전 방향을 바라보게 끔 회전
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Slerp <- 부드럽게 회전
        }

        //bool isMoving = moveDirection.magnitude > 0.1f; //일정 속도 움직이면 움직이는 상태 on
        //animator.SetBool("isWalk", isMoving);
        EnemyFollowDot();


    }
    void PlayerMovemass(float x, float y, float z)
    {
        if (x == 0) rigid.linearVelocity = (transform.forward * z * runPow) + (Vector3.up * rigid.linearVelocity.y);
        else if (z == 0) rigid.linearVelocity = (transform.right * x * runPow) + (Vector3.up * rigid.linearVelocity.y);
        else rigid.linearVelocity = (transform.forward * z / 1.5f * runPow) +
                (transform.right * x / 1.5f * runPow) + (Vector3.up * rigid.linearVelocity.y);
    }

    void EnemyFollowDot()
    {
        if (DotTimer < 3f) DotTimer += Time.deltaTime;
        else
        {
            if (PlayerDot[0] != Vector3.zero)
            {
                for (int i = PlayerDot.Length - 1; 0 < i; i--)
                {
                    PlayerDot[i] = PlayerDot[i - 1];
                    GetComponent<LineRenderer>().SetPosition(i - 1, PlayerDot[i - 1]);
                }
                if (DotCount == 10)
                {
                    PlayerDot[PlayerDot.Length - 1] = Vector3.zero;
                    DotCount = 9;
                }
            }
            PlayerDot[0] = transform.position;
            DotCount++;
            DotTimer = 0f;
        }
    }
    void OnDrawGizmosSelected()
    {

    }


    void playerDead()
    {
        if (PlayerHp <= 0)
        {
            //animation dead start and return
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Key")
        {
            //Debug.Log("Get Key");
            Destroy(other.gameObject);
            Keycount++;
        }
    }

    void UpdateKeyUI()      //열쇠 획득 코드
    {
        if (KeyCountText != null)
        {
            KeyCountText.text = Keycount + " / " + TotalKeys;
        }


        if (!isGameClear && Keycount >= 5)
        {
            isGameClear = true;
            Debug.Log("Game Clear!");
            Time.timeScale = 0;

            gameClearTMPText.enabled = true;
        }
    }
}
