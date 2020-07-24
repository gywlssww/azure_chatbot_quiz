# 온라인 강의 - 복습 퀴즈 & 학생 정보 관리 챗봇

순차적, 루프 등의 복잡한 대화 흐름과 Azure 서버 상의 postgresql database에 저장된 데이터를 바탕으로 강의 관련 복습 퀴즈를 진행하고 성적에 따라 학생 별로 출결 현황을 저장한다.

## 사전 요구 사항

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```
- Azure Database for Postgresql 계정

## 실행 방법

1.직접 챗봇 실행
- Clone the repository

    ```bash
    git clone https://github.com/Microsoft/botbuilder-samples.git
    ```

- Run the bot from Visual Studio:

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/csharp_dotnetcore/43.complex-dialog` folder
  - Select `ComplexDialogBot.csproj` file
  - Press `F5` to run the project

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the latest Bot Framework Emulator from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

2. 조교 딩 이미지가 담긴 카드 메세지가 출력되면 정상적으로 작동이 시작된 것
(안내에 따라)
   (1) 이름 입력
   (2) 학번 입력
   (3) 현재까지 출석률 조회
   (4) 퀴즈 시작 - 문제에 대해 답이라 생각하는 항목 클릭
                 - 정답 확인 후 <위키 백과로 이동> 버튼을 이용해 관련 사이트로 이동 가능.
   (5) 퀴즈 종료 후 문항 별 채점 결과와 최종 점수 확인.
   (6) 데이터 베이스 상의 변경된 출석률 확인 후 프로그램 종료.
