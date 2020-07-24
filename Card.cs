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
        public static ThumbnailCard GetThumbnailCard(string str,string answer)
        {
            var imgurl = str == "정답입니다 :)" ? "https://github.com/gywlssww/azure_chatbot_quiz/blob/master/correct.png?raw=true" : "https://github.com/gywlssww/azure_chatbot_quiz/blob/master/wrong.png?raw=true";
            var thumbnailCard = new ThumbnailCard
            {
                Title = str,
                Subtitle = "정답: "+answer,
                Text = "추가 학습을 원하시면 버튼을 클릭하세요",
                Images = new List<CardImage> { new CardImage(imgurl) },
                //추가학습을 위한 위키 검색 페이지
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "위키백과로 이동", value: "https://ko.wikipedia.org/wiki/%ED%8E%98%EC%9D%B4%EC%A7%95") },
            };

            return thumbnailCard;
        }
    }
}
