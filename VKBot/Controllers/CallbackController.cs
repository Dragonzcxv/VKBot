using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using System.Threading;
using VkNet.Model.Keyboard;
using VkNet.Model.Attachments;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace VKBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallbackController:ControllerBase
    {
        ///<summary>
        ///Конфигурация приложения
        ///</summary>
        private readonly IConfiguration _configuration;

        private readonly IVkApi _vkApi;

        private MessageKeyboard keyboard;
        private UploadServerInfo uploadServer;

        public CallbackController(IVkApi vkApi,IConfiguration configuration)
        {
            _vkApi = vkApi;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Callback([FromBody]Updates updates)
        {
            var msg = Message.FromJson(new VkResponse(updates.Object));
      
            switch (updates.Type)
            {
                case "confirmation":
                    return Ok(_configuration["Config:Confirmation"]);
                case "group_join":
                    {
                        //SendTextMessage(msg.PeerId.Value, "случился бан");
                        keyboard = KeyboardBuild();
                        _vkApi.Messages.Send(new MessagesSendParams
                        {
                            RandomId = new DateTime().Millisecond,
                            PeerId = msg.PeerId.Value,
                            Message = "Ты посмотри кто к нам колёса катит",
                            Keyboard = keyboard
                        });
                        break;
                    }
                case "message_new":
                    {
                        if (msg.Geo != null)
                        {
                            SendTextMessage(msg.PeerId.Value, WeatherFunction(msg.Geo.Coordinates.Latitude,msg.Geo.Coordinates.Longitude));
                        }
                        break;
                    }
            }
            return Ok("ok");
        }

        private string WeatherFunction(double lat, double lon)
        {
            string cityName;
            string descriptionWeather;
            int tempWeather;
            int tempFeelsWeather;;
            string adress = $"http://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&units=metric&lang=ru&appid={_configuration["Config:WetherAPI"]}";
            using (WebClient client = new WebClient())
            {
                var JData = client.DownloadString(adress);
                var data = JObject.Parse(JData);
                cityName = (string)data["name"];
                descriptionWeather = (string)data["weather"][0]["description"];
                tempWeather = (int)data["main"]["temp"];
                tempFeelsWeather = (int)data["main"]["feels_like"];
                Console.WriteLine($"В общем в городе {cityName} сейчас {tempWeather} по цельсию, но чувствовать ты будешь {tempFeelsWeather}. В целом, в твоей дыре {descriptionWeather}. Пис тебе!");
            }
            return $"В общем в городе {cityName} сейчас {tempWeather} по цельсию, но чувствовать ты будешь {tempFeelsWeather}. В целом, в твоей дыре {descriptionWeather}. Пис тебе!";
        }

        private void SetActivityMessages(string groupId, long peerId, int second, bool isVoice = false)
        {
            if (isVoice)
                _vkApi.Messages.SetActivity(groupId, MessageActivityType.AudioMessage, peerId);
            else
                _vkApi.Messages.SetActivity(groupId, MessageActivityType.Typing, peerId);
            Thread.Sleep(second * 1000);
        }

        private void SendTextMessage(long? userId, string message)
        {
            _vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = new DateTime().Millisecond,
                PeerId = userId,
                Message = message
            });
        }

        private void SendVoiceMessage(long? userId, string pathAudioMessage)
        {
            uploadServer = _vkApi.Docs.GetMessagesUploadServer(userId, DocMessageType.AudioMessage);
            var wc = new WebClient();
            var doc = Encoding.UTF8.GetString(wc.UploadFile(uploadServer.UploadUrl, pathAudioMessage));

            var audioMessage = new List<MediaAttachment>
            {
                _vkApi.Docs.Save(doc, "message", "test")[0].Instance
            };

            _vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = new DateTime().Millisecond,
                PeerId = userId,
                Attachments = audioMessage
            });
           // http://api.openweathermap.org/data/2.5/weather?lat=54.1960900&lon=37.6182200&units=metric&lang=ru&appid=0e8751055ef3bd151909c6423d520232

        }

        private MessageKeyboard KeyboardBuild()
        {
            List<List<MessageKeyboardButton>> buttons = new List<List<MessageKeyboardButton>>
            {
                new List<MessageKeyboardButton>
                {
                    new MessageKeyboardButton
                    {
                        Action = new MessageKeyboardButtonAction
                        {
                            Type = KeyboardButtonActionType.Location
                        }
                    }
                }
            };
            return new MessageKeyboard { Buttons = buttons };


        }

        

    }

    
}
