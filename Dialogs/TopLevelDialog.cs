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

        
        // Define a "done" response for the company selection prompt.
        private const string DoneOption = "done";

        // Define value names for values tracked inside the dialogs.
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

        
        private static async Task<DialogTurnResult> UidStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[QuizResult] = stepContext.Options as List<string> ?? new List<string>();
            var isloop = stepContext.Values[QuizResult] as List<string>;
            
            if (isloop.Count != 0)
            {
                stepContext.Values[UserInfo] = new UserProfile();
                var userProfile2 = (UserProfile)stepContext.Values[UserInfo];
                userProfile2.name = isloop[0];
               

                return await stepContext.NextAsync(isloop, cancellationToken);
            }
            

            // Create an object in which to collect the user's information within the dialog.
            stepContext.Values[UserInfo] = new UserProfile();
            stepContext.Values[Answer] = new List<string>();
            stepContext.Values[Questions] = new List<string>();
            stepContext.Values[Examples] = new List<string>();
            

            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.name= (string)stepContext.Result;//이름 저장
            var promptOptions = new PromptOptions { Prompt = MessageFactory.Text("학번 9자리를 입력하세요") };

            // Ask the user to enter their name.
            return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> AttendenceStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) //학번 입력 후 출석률 확인 페이지
        {
            stepContext.Values[QuizResult] = stepContext.Options as List<string> ?? new List<string>();
            var isloop = stepContext.Values[QuizResult] as List<string>;
            if (isloop.Count != 0)
            {
                return await stepContext.NextAsync(isloop, cancellationToken); ;
            }
            // Set the user's name to what they entered in response to the name prompt.
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            userProfile.unum = ((string)stepContext.Result);//학번 저장

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

                stepContext.Values[Answer] = answer;
            
            //example 형성 후 질문

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(question),
                RetryPrompt = MessageFactory.Text("보기에서 정답을 고르세요!"),
                Choices = ChoiceFactory.ToChoices(example.Split('/')),
            };

            // Prompt the user for a choice.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);


        }

        private async Task<DialogTurnResult> ChkQuizStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Set the user's company selection to what they entered in the review-selection dialog.
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            var quizResultlist = stepContext.Values[QuizResult] as List<string>;
            var answerlist = (string)stepContext.Values[Answer];
            var choice = (FoundChoice)stepContext.Result;
            await stepContext.Context.SendActivityAsync(choice.Value == answerlist ? "정답입니다." : "오답입니다.");
            quizResultlist.Add(choice.Value == answerlist ? "O" : "X");

            if (quizResultlist.Count == 6)
            {

                // Thank them for participating.
                int score = 0;
                for (int i = 1; i < quizResultlist.Count; i++) {
                    if (String.Compare(quizResultlist[i], "O") == 0) score+=20;
                }
                string scoreStr = $"{((UserProfile)stepContext.Values[UserInfo]).name}님의 퀴즈 결과 {score}점 입니다.\r\n " +
                    $"1번 문제: {quizResultlist[1]}\r\n2번 문제: {quizResultlist[2]}\r\n3번 문제: {quizResultlist[3]}\r\n4번 문제: {quizResultlist[4]}\r\n5번 문제: {quizResultlist[5]}";
                if (score >= 60)
                {
                    await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text(scoreStr+"\r\n축하드립니다. 오늘 수업을 이수하였습니다."),
                    cancellationToken);
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
                
                // Exit the dialog, returning the collected user information.
                return await stepContext.EndDialogAsync(stepContext.Values[UserInfo], cancellationToken);
            }
            else {
                return await stepContext.ReplaceDialogAsync(nameof(TopLevelDialog), quizResultlist, cancellationToken);
            }
        }
    }
}
