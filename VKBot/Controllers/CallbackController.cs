﻿using Microsoft.AspNetCore.Mvc;
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
using System.Collections.Generic;
using VKBot.Models;
using System.Threading.Tasks;

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

        private UploadServerInfo uploadServer;





        public CallbackController(IVkApi vkApi,IConfiguration configuration)
        {
            _vkApi = vkApi;
            _configuration = configuration;
        }





        [HttpPost]
        public IActionResult Callback([FromBody]Updates updates)
        {
            if(updates.Type == "confirmation")
                return Ok(_configuration["Config:Confirmation"]);

            Task.Factory.StartNew(() =>
            {
                var msg = Message.FromJson(new VkResponse(updates.Object));
                switch (updates.Type)
                {
                    case "group_join":
                        {
                            _vkApi.Messages.Send(new MessagesSendParams
                            {
                                RandomId = new DateTime().Millisecond,
                                PeerId = msg.UserId.Value,
                                Message = "Ты посмотри кто к нам колёса катит\nЯ могу рассказать, какая погода сегодня, ты только намекни, местоположением например",
                                Keyboard = KeyboardBuild()
                            });
                            break;
                        }
                    case "message_new":
                        {
                            if (msg.Geo != null)
                            {
                                uploadServer = _vkApi.Photo.GetMessagesUploadServer(msg.PeerId.Value);
                                WeatherAPI.UpdateWeather(msg.Geo.Coordinates.Latitude, msg.Geo.Coordinates.Longitude, msg.Geo.Place.City, uploadServer);
                                SetActivityMessages(updates.GroupId.ToString(), msg.PeerId.Value, 5);
                                SendTextMessage(msg.PeerId.Value, WeatherAPI.GetTextWeather(), _vkApi.Photo.SaveMessagesPhoto(WeatherAPI.iconResponse));
                            }
                            else
                                SendTextMessage(msg.PeerId.Value, "Братишка мой альтернативно одарённый...\n" +
                                                                  "Я погодный бот, я могу только сказать погоду и посадить на нож\n" +
                                                                  "Лучше бы тебе отправить поскорее координаты, не то выбор будет не велик");

                            break;
                        }
                }
            });
            return Ok("ok");
        }

        
        
        
        /// <summary>
        /// Состояние бота "набирает текст\записывает аудио"
        /// </summary>
        /// <param name="groupId">Id бота</param>
        /// <param name="peerId">Id пользователя, с кем происходит диалог</param>
        /// <param name="second">Продолжительность в секундах</param>
        /// <param name="isVoice">Голосовое сообщение да\нет</param>
        private void SetActivityMessages(string groupId, long peerId, int second, bool isVoice = false)
        {
            if (isVoice)
                _vkApi.Messages.SetActivity(groupId, MessageActivityType.AudioMessage, peerId);
            else
                _vkApi.Messages.SetActivity(groupId, MessageActivityType.Typing, peerId);
            Thread.Sleep(second * 1000);
        }





        private void SendTextMessage(long? userId, string message, IReadOnlyCollection<Photo> photos = null)
        {
            _vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = new DateTime().Millisecond,
                PeerId = userId,
                Message = message,
                Attachments = photos
            });
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
