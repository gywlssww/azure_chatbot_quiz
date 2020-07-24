using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    public class Card
    {
        public static HeroCard GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = "조교 딩과 복습퀴즈를 시작합니다.",
                Subtitle = " 60점 이상인 경우에만 출석으로 인정됩니다.",
                Text = " 출석할 학생의 이름을 입력하세요",
                Images = new List<CardImage> { new CardImage("https://github.com/gywlssww/azure_chatbot_quiz/blob/master/teacher.png?raw=true") },
            };

            return heroCard;
        }
    }
}
