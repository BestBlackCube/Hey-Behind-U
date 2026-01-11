using UnityEngine;

public class Cam : MonoBehaviour
{
    public Transform target;
    public bool FirstPlayer = false;
    [SerializeField] float mouseSensitivity = 200f; //마우스 감도
    [SerializeField] float xRotation = 0f;
    [SerializeField] float yRotation = 0f;

    // Update is called once per frame
    void Start()
    {
        SetThridView();
        //  transform.position = new Vector3(0, 3.5f, -5);
        //  transform.Rotate(new Vector3(15, 0, 0));
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) FirstPlayer = !FirstPlayer;

        if (!FirstPlayer)
        {
            SetThridView();     //3인칭 뷰
            Cursor.lockState = CursorLockMode.None; //? 커서 화면 고정해제
            Cursor.visible = true;                  //? 커서 보이게
        }
        else
        {                          //1인칭 뷰
            SetFirstView();
            handleFirstRotation();
            Cursor.lockState = CursorLockMode.Locked; //? 커서 화면 고정
            Cursor.visible = false;                   //? 커서 안보이게
        }
        //transform.position = new Vector3(target.transform.position.x, 
        //target.transform.position.y + 3.5f, target.transform.position.z - 5);
    }

    void SetFirstView()     //! 1인칭 시점
    {
        transform.position = target.position + target.forward * 0.1f + new Vector3(0, 1.6f, 0); //1인칭 위치 설정
        transform.rotation = target.rotation;       //1인칭 회전 설정
    }

    void SetThridView()     //! 3인칭
    {
        transform.position = target.position + new Vector3(0, 3.5f, -5);        //3인칭 위치 설정
        transform.rotation = Quaternion.Euler(15, 0, 0);        //3인칭 x 축 15도 아래로 바라보게 설정
    }

    void handleFirstRotation()          //TODO 1인칭 마우스 회전 구현
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;    //마우스 입력 처리 right + / left -
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;    //마우스 입력 처리 Up + / Down -

        xRotation -= mouseY;        //마우스 위로 가면 위로 / 아래로 내리면 아래로 가게끔
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); //고개가 꺾이지 않게 제한 / 360도 불가능

        yRotation += mouseX; //! 좌우 회전값

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);   //카메라 x, y축 회전 적용
        target.rotation = Quaternion.Euler(0f, yRotation, 0f);      //플레이어 몸체 회전
    }

}

