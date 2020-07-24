// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using Npgsql;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.BotBuilderSamples
{
    public class ReviewSelectionDialog : ComponentDialog
    {
       
        // Define a "done" response for the company selection prompt.
        private const string DoneOption = "done";

        // Define value names for values tracked inside the dialogs.
        private const string AnswerSelected = "value-companiesSelected";
        private const string Correct = "value-correct";

        // Define the company choices for the company selection prompt.
        private readonly string[] _companyOptions = new string[]
        {
            "임계구역", "상호배제", "페이징", "세마포어",
        };

        // Define value names for values tracked inside the dialogs.
        private const string UserInfo = "value-userInfo";

        //db정보 가져오기
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

       
        public ReviewSelectionDialog()
            : base(nameof(ReviewSelectionDialog))
        {
            

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                    SelectionStepAsync,
                    LoopStepAsync,
                }));

            InitialDialogId = nameof(WaterfallDialog);


            

        }

        private async Task<DialogTurnResult> SelectionStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("start review 넘어왔어."));

            stepContext.Values[UserInfo] = new UserProfile();
            var userProfile = (UserProfile)stepContext.Values[UserInfo];
            var result = (string)stepContext.Options;
            int qnum=0; //qnum에 해당하는 문제를 불러오기 

            if (Int32.Parse(result) >= 0 && Int32.Parse(result) <= 10)
            {
                qnum = Int32.Parse(result);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("next ro loopdialog" + result + "넘어왔어."));
            }
            else {
                userProfile.name = (string)stepContext.Result;//이름 저장
                stepContext.Values[AnswerSelected] = new List<string>();
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("reviewdialog시작했고" + userProfile.name + "넘어왔어."));
            }

            // Continue using the same selection list, if any, from the previous iteration of this dialog.
            //var list = stepContext.Options as List<string> ?? new List<string>();
            //stepContext.Values[AnswerSelected] = list;
            //stepContext.Values[Correct] = 0;
                                   
            string question = "";
            string examplelist = "";
            // db 작업
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                string commandstr = String.Format(
                   "SELECT question, list FROM quiz WHERE qid =  {0}; ", qnum);

                var command = new NpgsqlCommand(commandstr, conn);

                var reader = command.ExecuteReader();
               
                while (reader.Read())
                {
                    question = reader.GetString(1);
                    examplelist = reader.GetString(2);
                    stepContext.Values[Correct] = reader.GetString(3);
                }


            }
            // Create a prompt message.
            
            // Create the list of options to choose from.
            string[] examples = examplelist.Split('/');
            var options = examples.ToList();
            //var options = _companyOptions.ToList();
            //options.Add(DoneOption);
          

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text(question),
                RetryPrompt = MessageFactory.Text("Please choose an option from the list."),
                Choices = ChoiceFactory.ToChoices(examples),
            };
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("[hj] question:" + question ));

            // Prompt the user for a choice.
            return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> LoopStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken)
        {
            var answer = (string)stepContext.Values[Correct];
            var choice = (FoundChoice)stepContext.Result;
            var picked = choice.Value;
            string quizresult = answer == picked ? "정답입니다." : "오답입니다.";
            var list = stepContext.Values[AnswerSelected] as List<string>;
            list.Add(quizresult);
            // Retrieve their selection list, the choice they made, and whether they chose to finish.
            /*var list = stepContext.Values[AnswerSelected] as List<string>;
            var choice = (FoundChoice)stepContext.Result;
            var done = choice.Value == DoneOption;
            
            if (!done) //done 을 선택한 경우
            {
                // If they chose a company, add it to the list.
                list.Add(choice.Value);
                await stepContext.Context.SendActivityAsync(choice.Value + "를 선택하셨습니다.");
            }
            */
            if (list.Count==4) //반복 종료
            {
                // If they're done, exit and return their list.
                //list 에 유저 정보를 준다.
                return await stepContext.EndDialogAsync(list, cancellationToken);
            }
            else // 반복
            {
                var qnum = list.Count.ToString();
                // Otherwise, repeat this dialog, passing in the list from this iteration.
                return await stepContext.ReplaceDialogAsync(nameof(ReviewSelectionDialog), qnum, cancellationToken);
            }
            


        }
    }
}
