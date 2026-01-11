using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed;   
    float hAxis, vAxis;
    Vector3 moveVec;
    Animator ani;
    bool Run;
    public float rotSpeed;

    void Awake()
    {
        ani = GetComponentInChildren<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hAxis = Input.GetAxisRaw("Horizontal"); //수직이동
        vAxis = Input.GetAxisRaw("Vertical");   //수평이동
        Run = Input.GetButton("Run");           //leftshift 누를때 달리기

        moveVec = new Vector3(hAxis, 0, vAxis).normalized;  //대각이동할경우 거리가 더 김 normalized => 방향값을 1로 보정해줌        

        if(Run){    //달릴때 속도 ++
            transform.position += moveVec * speed * 1.5f * Time.deltaTime;
        }else{
            transform.position += moveVec * speed * Time.deltaTime;
        }

        
        
        transform.LookAt(transform.position + moveVec);

        ani.SetBool("isWalk", moveVec != Vector3.zero);
        ani.SetBool("isRun", Run);

    }

    private void FixedUpdate()
    {
        if (moveVec != Vector3.zero){
            transform.forward = Vector3.Lerp(transform.forward, moveVec, rotSpeed * Time.deltaTime);
        }
    }
}
