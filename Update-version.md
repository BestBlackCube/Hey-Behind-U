# Hey, Behind-U
## 기능 설명
다양한 추격자 기능 개발을 통해 C#과 유니티 엔진의 이해도를 쌓아왔고,  
Nav와 Ray를 적극 사용하여 거리, 시야각, 탐지, Ai 기능 및 시스템을 개발하였습니다.

## 추격자
- ### [일반 추격자](https://github.com/BestBlackCube/Hey-Behind-U/blob/fdcaccebe8ad605c490960793abddff8555fcfe4/Hey-Behind-U/Assets/20202776/Kevin%20Iglesias/Characters/Humanoid%20Giant/Enemy_Script.cs#L160-L178)
    - LineRenderer의 정해진 Position값을 기본적으로 따라 움직이며, 플레이어가 탐지 거리에 들어오게  
    된다면 추격 시스템으로 전환되며 플레이어를 쫓아가게 된다.  
    반대로 플레이어가 탐지 거리 밖으로 벗어 나게 된다면 기존의 LineRenderer의 Position값으로 돌아간다.

- ### [시선 추격자](https://github.com/BestBlackCube/Hey-Behind-U/blob/fdcaccebe8ad605c490960793abddff8555fcfe4/Hey-Behind-U/Assets/20202776/Kevin%20Iglesias/Characters/Humanoid%20Giant/Enemy_Script.cs#L180-L215)
    - 플레이어의 시야각 360도 기준으로 적을 바라보는 180도와 바라보지 않는 180도 2개로 구별하여  
    추격자를 바라보고 있을 경우는 추격자가 움직이지 않고 바라보지 않으면 플레이어를 향해 움직인다.
    - 플레이어와 추격자 사이의 벽이 있을 경우 움직이지 않는 버그가 있었지만, foreach, Ray를 사용하여  
    벽이 있을경우 예외 처리를 통해 벽이 있어도 움직이게 된다.

- ### [영구 추격자](https://github.com/BestBlackCube/Hey-Behind-U/blob/fdcaccebe8ad605c490960793abddff8555fcfe4/Hey-Behind-U/Assets/20202776/Kevin%20Iglesias/Characters/Humanoid%20Giant/Enemy_Script.cs#L335-L346)
    - 플레이어는 일정 시간에 발자국을 1-10개를 순서대로 남기게 되고, 이 추격자는 발자국을 토대로  
    플레이어가 어디에 있든 잡을 때 까지 영구적으로 추격 하게 된다.

- ### [돌진 추격자](https://github.com/BestBlackCube/Hey-Behind-U/blob/fdcaccebe8ad605c490960793abddff8555fcfe4/Hey-Behind-U/Assets/20202776/Kevin%20Iglesias/Characters/Humanoid%20Giant/Enemy_Script.cs#L281-L334)
    - LineRenderer의 정해진 Position값을 따라 움직이지만, 시야각 및 탐지 범위에 플레이어가 있다면  
    정지 후 플레이어 방향으로 Ray를 발사하여 벽 또는 100 이상일 경우 아주 빠르게 플레이어에게 돌진한다.
