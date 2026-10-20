# Shooong Unity Contributions

Unity/XR 드론 비행 팀 프로젝트 Shooong에서 오성영이 담당한 클라이언트 코드 발췌본입니다.

[프로젝트 설명과 시연 영상](https://ohsungyoung-game.netlify.app/#shooong)

![Shooong](https://ohsungyoung-game.netlify.app/assets/shooong-poster.jpg)

## 본인 담당 범위

- Unity 개발 2인·백엔드 1인 팀에서 Unity 클라이언트 개발 담당
- 드론 비행 입력, Rigidbody 기반 이동, 호버·자세 안정화
- 컨트롤러·핸드 입력, 체크포인트·비행 상태·HUD
- 백엔드 인증·비행 세션·텔레메트리 연동 클라이언트
- Codex를 구현 보조 도구로 활용하고 Unity에서 동작 확인 및 조정

사용자가 Cesium을 제외한 해당 클라이언트 코드를 본인 담당 범위로 확인했습니다. Cesium 공간 구성, 지도·지형 데이터, 외부 에셋과 백엔드 서버 코드는 공개본에서 제외했습니다.

## 먼저 볼 코드

| 기능 | 코드 |
| --- | --- |
| 비행 물리와 안정화 | [DroneMovement.cs](Assets/02_Scripts/Drone/DroneMovement.cs) |
| XR 입력을 공통 비행 입력으로 변환 | [VRDroneInputReader.cs](Assets/02_Scripts/VR/VRDroneInputReader.cs) |
| 핸드 입력 | [VRHandTracker.cs](Assets/02_Scripts/VR/VRHandTracker.cs) |
| 체크포인트 진행 | [CheckpointCourse.cs](Assets/02_Scripts/Checkpoint/CheckpointCourse.cs) |
| 비행 상태 전송 | [DronePoseBatchSender.cs](Assets/02_Scripts/Network/Telemetry/DronePoseBatchSender.cs) |
| 클라이언트 API 설정 | [ApiConfigSO.cs](Assets/02_Scripts/Network/Core/ApiConfigSO.cs) |

컨트롤러와 핸드 입력을 공통 비행 입력으로 전달하고, 물리 프레임에서 추력·기울기·회전·감쇠를 적용합니다. 체크포인트와 UI, 네트워크 전송은 별도 코드에서 처리합니다.

## 공개용 변경과 포함 범위

`Assets/02_Scripts`의 본인 담당 C# 코드를 원래 경로로 발췌했습니다. 공개본에서 테스트 계정 기본값은 비웠고 실제 서버 주소는 예시 주소로 바꿨습니다. 원본 팀 저장소는 수정하지 않았습니다.

Scene·Prefab, Inspector 연결, 패키지·프로젝트 설정과 외부 에셋이 생략되어 있어 이 저장소만으로 원본 프로젝트 전체가 실행되지는 않습니다. 검토한 원본 환경은 Unity 6000.4.4f1, OpenXR, XR Interaction Toolkit 3.4.1, Input System, URP이며 공개 소스에는 Cesium 코드가 포함되지 않습니다.

## 검토 기록

원본 소스의 테스트 로그인 기본값과 서버 주소를 공개본에서 치환하고, 인증정보 패턴과 Cesium 참조 여부를 다시 확인했습니다. 비행 입력·Rigidbody 제어 코드를 읽고 설명 범위를 확인했습니다. 이번 코드 발췌본의 Unity 컴파일·기기 재시험은 수행하지 않았습니다. 자세한 파일 목록과 변경 사유는 [PUBLICATION_MANIFEST.json](PUBLICATION_MANIFEST.json)에 기록했습니다.

## 이용 범위

개인 담당 코드의 열람과 기술 평가를 위한 포트폴리오입니다. 팀 프로젝트 전체에 대한 권리를 주장하거나 별도 오픈소스 라이선스를 부여하지 않습니다. 외부 라이브러리·에셋의 권리는 원 저작자에게 있습니다.
