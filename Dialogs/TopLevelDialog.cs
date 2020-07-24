// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Npgsql;
using System;
using System.Data;
using System.Linq;

namespace Microsoft.BotBuilderSamples
{

    public class TopLevelDialog : ComponentDialog
    {
        //데이터 베이스 정보
        private static string Host = "dbserver502v200722.postgres.database.azure.com";
        private static string User = "ding@dbserver502v200722";
        private static string DBname = "mypgsqldb";
        private static string Password = "Hhj9060!";
        private static string Port = "5432";

        string connString =
               String.Format(
                   "Server={0}; User Id={1}; Database={2}; Port={3}; Password={4};SSLMode=Prefer",
                   Host,
                   User,
                   DBname,
                   Port,
                   Password);

        
        
        private const string DoneOption = "done";

        // 접속한 학생 정보 관리용 변수
        private const string UserInfo = "value-userInfo";
        private const string Questions = "value-questions";
        private const string Examples = "value-examples";
        private const string Answer = "value-answer";
        private const string QuizResult = "value-result";
        public TopLevelDialog()
            : base(nameof(TopLevelDialog))
        {
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                UidStepAsync,
                AttendenceStepAsync,
                DBtaskStepAsync,
                ChkQuizStepAsync,
            }));

            InitialDialogId = nameof(WaterfallDialog);
        }

        //1. 접속한 학생의 학번 입력 - 학번을 바탕으로 학생 정보 업데이트
        private static async Task<DialogTurnResult> UidStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[QuizResult] = stepContext.Options as List<string> ?? new List<string>();
            var isloop = stepContext.Values[QuizResult] as List<string>;
            
            //반복 대화 시 실행-불필요한 작업(학번 입력)의 반복을 피함
            if (isloop.Count != 0)
            {
                stepContext.Values[UserInfo] = new UserProfile();
                var userProfile2 = (UserProfile)stepContext.Values[UserInfo];
                userProfile2.name = isloop[0];
               

                return await stepContext.NextAsync(isloop, cancellationToken);
            }
            

            // 처음 대화 시작 시 실행-접속한 학생 정보 초기화
            stepContext.Values[UserInfo] = new UserProfile();
            stepContext.Values[Answer] = new List<string>();
            stepContext.Values[Questions] = new List<string>();
            stepContext.Values[Examples] = new List<string>();
            

            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.name= (string)stepContext.Result;//이름 저장
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("학번 9자리를 입력하세요") };

            // 처음 대화 시작 시 실행 - 학생의 학번 입력 받기
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        //2. 접속한 학생의 학번을 통해 출석률 조회
        private async Task<DialogTurnResult> AttendenceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
        {
            stepContext.Values[QuizResult] = stepContext.Options as List<string> ?? new List<string>();
            var isloop = stepContext.Values[QuizResult] as List<string>;

            //반복 대화 시 실행-불필요한 작업(출석률 조회)의 반복을 피함
            if (isloop.Count != 0)
            {
                return await stepContext.NextAsync(isloop, cancellationToken); ;
            }

            // 입력받은 학생의 학번을 학생 정보에 저장
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.unum = ((string)stepContext.Result);//학번 저장

            //학번을 바탕으로 데이터 베이스에서 학생 정보(이름,학번,출석률) 조회
            using (var conn = new NpgsqlConnection(connString)) {
                conn.Open();

                string commandstr = String.Format(
                   "SELECT * FROM users WHERE unum =  {0}; ",userProfile.unum);

                var command = new NpgsqlCommand(commandstr, conn);

                var reader = command.ExecuteReader();
                if (reader == null)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("이전 기록이 없습니다."));
                }
                while (reader.Read())
                    {
                        await stepContext.Context.SendActivityAsync(
                            MessageFactory.Text(string.Format(
                                "{0} 님 ({1}) 현재 출석률은 {2}% 입니다.",
                                reader.GetString(1),
                                reader.GetInt32(3).ToString(),
                                reader.GetInt32(4).ToString()
                                )
                            ));
                    userProfile.name = reader.GetString(1);
                    userProfile.attendence = reader.GetInt32(4);
                    }


            }
            
            
            return await stepContext.NextAsync(isloop, cancellationToken);
        }

        //3. 데이터 베이스에서 퀴즈 문제 조회
        private async Task<DialogTurnResult> DBtaskStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var quizResultlist = stepContext.Values[QuizResult] as List<string>;

            // Set the user's age to what they entered in response to the age prompt.

            var userProfile = quizResultlist.Count==0?(UserProfile)stepContext.Values[UserInfo]:new UserProfile();
            var question = "";
            var example = "";
            var answer = " ";

           
            if (quizResultlist.Count == 0)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("퀴즈를 시작합니다 :D !!!"));
                quizResultlist.Add(userProfile.name);
            }
            else { 
                userProfile.name = quizResultlist[0];
            }

            //퀴즈 문제 데이터 베이스에서 문제, 보기, 정답을 한 튜플씩 조회
            using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();

                    string commandstr =String.Format("SELECT * FROM quiz WHERE qid = {0};",quizResultlist.Count);

                    var command = new NpgsqlCommand(commandstr, conn);

                    var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    question = reader.GetString(1);
                    example = reader.GetString(2);
                    answer = reader.GetString(3);
                }
                

                }
            //해당 문제의 정답을 다음 대화 흐름으로 전송하기 위해
                stepContext.Values[Answer] = answer;
            
            //문제의 보기 생성 후 출력
            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(question),
                RetryPrompt = MessageFactory.Text("보기에서 정답을 고르세요!"),
                Choices = ChoiceFactory.ToChoices(example.Split('/')),
            };

            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);


        }

        //4.퀴즈의 정답과 학생의 답을 비교하고 채점한 뒤 다음 문제 출제 작업으로 반복
        private async Task<DialogTurnResult> ChkQuizStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            var quizResultlist = stepContext.Values[QuizResult] as List<string>;
            var answerlist = (string)stepContext.Values[Answer];
            //학생의 답(choice.Value)과 정답(answerlist)을 비교
            var choice = (FoundChoice)stepContext.Result;
            await stepContext.Context.SendActivityAsync(choice.Value == answerlist ? "정답입니다." : "오답입니다.");
            quizResultlist.Add(choice.Value == answerlist ? "O" : "X");

            //퀴즈 데이터 베이스 내 모든 문제를 푼 경우
            if (quizResultlist.Count == 6)
            {

                int score = 0;
                //전체 퀴즈 풀이 점수 계산
                for (int i = 1; i < quizResultlist.Count; i++) {
                    if (String.Compare(quizResultlist[i], "O") == 0) score+=20;
                }
                //문제 당 채점 결과 출력
                string scoreStr = $"{((UserProfile)stepContext.Values[UserInfo]).name}님의 퀴즈 결과 {score}점 입니다.\r\n " +
                    $"1번 문제: {quizResultlist[1]}\r\n2번 문제: {quizResultlist[2]}\r\n3번 문제: {quizResultlist[3]}\r\n4번 문제: {quizResultlist[4]}\r\n5번 문제: {quizResultlist[5]}";
                if (score >= 60)
                {
                    await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(scoreStr+"\r\n축하드립니다. 오늘 수업을 이수하였습니다."),
                    cancellationToken);
                    //60점 이상인 경우 출석으로 인정되어 학생의 출석률 정보 데이터베이스 업데이트
                    using (var conn = new NpgsqlConnection(connString))
                    {
                        conn.Open();

                        using (var command = new NpgsqlCommand("UPDATE users SET attendence = @q WHERE uname = @n;", conn))
                        {
                            command.Parameters.AddWithValue("n", quizResultlist[0]);
                            command.Parameters.AddWithValue("q", 80);

                            int nRows = command.ExecuteNonQuery();
                        }
                    }
                    
                    userProfile.attendence = 80;
                    
                }
                else {
                    await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(scoreStr + "\r\n오늘 수업은 출석으로 인정할 수 없습니다."),
                    cancellationToken);
                    userProfile.attendence = 70;
                }
                
                //대화 흐름 종료
                return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
            }
            //다음 문제 출제를 위해 대화 흐름 반복
            else {
                return await stepContext.ReplaceDialogAsync(nameof(TopLevelDialog), quizResultlist, cancellationToken);
            }
        }
    }
}
