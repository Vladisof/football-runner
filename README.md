## What I Did

### 1. 미션 하나 추가로 넣기
> 미션 개수를 2에서 3으로 수정헀습니다.               
> PR: https://github.com/G-Nck/DoubleMeUnityTest/pull/1
``` cs
    public void CheckMissionsCount()
    {
        while (missions.Count < missionCount)
            AddMission();
    }
```

### 2. 캐릭터 Accessory 아이템 하나 넣기
> Assets/Bundles/Characters/Cat/character.prefab의 캐릭터 컴포넌트에 엑세서리 리스트에 하트 엑세서리를 추가했습니다.                   
> 예시 이미지 에서 보여준 그대로 추가했습니다.              
> PR: https://github.com/G-Nck/DoubleMeUnityTest/pull/2              
### 3. 슬라이딩이나 점프 성공 시
#### 3-1. 캐릭터 스피드가 점점 빨라지고 장애물에 닿을 시 원래 스피드로 돌아온다.
> 튜토리얼에서 작동되는 회피 로직을 참고하여 로직을 구현했습니다.                       
> 기본 트랙 속도 + 보너스 속도 = 최종속도 로 계산하여 최대속도에 제한을 받습니다.                 
> 콤보당 보너스 속도는 Track Manager 컴포넌트의 BonusSpeedEachCombo로 조절할 수 있습니다.                 
     
> 장애물에 닿을 시 원래 스피드로 돌아오는 기능은 샘플에 기본적으로 내장되어 있기 때문에,              
> 해당 부분에 관련하여 추가적으로 작업한 건 없습니다.              
> 장애물에 닿을 시 콤보는 리셋됩니다.              
> 예시 이미지에 나온 슬라이드 장애물과 점프 장애물 이외의 장애물도 점프 혹은 슬라이드의 방법으로 회피에 성공할 시 콤보가 누적됩니다.              
> 추가적인 기능으로, GameState 컴포넌트로 콤보 누적을 설정할 수 있습니다.              
> ComboOnlyOneEachObstacle : 하나의 장애물 라인당 한번의 콤보만 쌓입니다.              
> ComboOnlyAllLaneObstacle : 전체 라인 장애물만 회피했을 때 콤보가 쌓입니다.              
> ComboOnlySlideAndJump : 슬라이드 혹은 점프로 회피했을 때만 콤보가 쌓입니다.                        
> 더 상세한 설명은 https://github.com/G-Nck/DoubleMeUnityTest/pull/3

#### 3-2. 콤보 UI가 화면에 생성이 되고 콤보 숫자가 점점 올라간다.
> 콤보가 쌓이기 시작할 시 화면 중앙 하단에 표시되며,              
> GameState 컴포넌트의 MinComboForDisplay로 생략할 콤보 수를 지정할 수 있습니다.              

### 4. 스코어 배수에 따라 물고기 코인도 배수만큼 늘어난다.                    
> PR: https://github.com/G-Nck/DoubleMeUnityTest/pull/4

### 5. 장애물 하나 새로 만들어서 런타임 중에 생성되게 만들기.
> '쓰레기 봉투' 장애물을 만들었습니다.              
> 모델은 게임 내에 장식품을 가져다 사용했고, 애니메이션은 간단하게 제작했습니다.              
> 사운드는 구글링하여 쓰레기 봉투 관련 효과음을 찾아 적용했습니다.                         
> PR: https://github.com/G-Nck/DoubleMeUnityTest/pull/5

### 6. PowerUpItem 하나 프리펩으로 만들어서 상점에 넣고, 게임이 시작되면 인게임에서 생성이 되거나 사용하기
> '보호막' 아이템을 구현했습니다.              
> 무적 아이템과 달리 지속시간이 1분 정도로 긴 대신, 한번만 장애물을 무시합니다.              
> 보호막 관련 파티클 이펙트는 샘플 내 메터리얼 및 텍스쳐를 이용해 직접 만들었습니다.              
> 사운드는 가지고 있던 에셋에서 가져와 적용했습니다.                             
> PR: https://github.com/G-Nck/DoubleMeUnityTest/pull/7

# 어려웠던 부분
> 3번의 회피 기능을 구현하는데 예상보다 많은 시간을 들였습니다.              
> 참고할 수 있는 기능인 튜토리얼의 회피 감지 로직이 이미 존재했지만,              
> 해당 로직이 버그를 포함하고 있었기 때문에 발견하기까지에 많은 시간이 걸렸습니다.              

> 회피 로직 자체는 금방 구현했으나, 위에서 말한 버그로 인해 회피 횟수가 트랙 단위 (TrackSegment) 의 마지막 장애물을 지나고 나서야 갑작스럽게 증가하는 현상을 겪었습니다.            
> 버그의 발생 원인은 회피 로직의 현재 트랙 단위의 장애물 위치를 받아오는데, 이때 받아오는 장애물 위치 배열을 정렬되지 않은 상태임에도 불구하고 마치 정렬된 것처럼 사용해서였습니다.
> 따로 Track Segment 프리팹들의 장애물 포지션 배열을 정렬시킨 이후에야 버그가 해결되었습니다.



