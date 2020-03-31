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
                //case "group_join":
                //    {
                //        //SendTextMessage(msg.PeerId.Value, "случился бан");
                //        keyboard = KeyboardBuild();
                //        _vkApi.Messages.Send(new MessagesSendParams
                //        {
                //            RandomId = new DateTime().Millisecond,
                //            PeerId = msg.PeerId.Value,
                //            Message = "Ты посмотри кто к нам колёса катит",
                //            Keyboard = keyboard
                //        });
                //        break;
                //    }
                default:
                    SendTextMessage(msg.PeerId.Value, updates.Type+" fef");
                    break;
            }
            return Ok("ok");
        }

        private void SetActivityMessages(string groupId, long peerId, int second, bool isVoice = false)
        {
            if (isVoice)
                _vkApi.Messages.SetActivity(groupId, MessageActivityType.AudioMessage, peerId);
            else
                _vkApi.Messages.SetActivity(groupId, MessageActivityType.Typing, peerId);
            Thread.Sleep(second * 1000);
        }

        private void SendTextMessage(long? peerId, string message)
        {
            _vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = new DateTime().Millisecond,
                PeerId = peerId,
                Message = message
            });
        }

        private void SendVoiceMessage(long? peerId, string pathAudioMessage)
        {
            uploadServer = _vkApi.Docs.GetMessagesUploadServer(peerId, DocMessageType.AudioMessage);
            var wc = new WebClient();
            var doc = Encoding.UTF8.GetString(wc.UploadFile(uploadServer.UploadUrl, pathAudioMessage));

            var audioMessage = new List<MediaAttachment>
            {
                _vkApi.Docs.Save(doc, "message", "test")[0].Instance
            };

            _vkApi.Messages.Send(new MessagesSendParams
            {
                RandomId = new DateTime().Millisecond,
                PeerId = peerId,
                Attachments = audioMessage
            });

        }

        private MessageKeyboard KeyboardBuild()
        {
            KeyboardBuilder builder = new KeyboardBuilder();
            builder.Clear();
            builder.AddButton("Быкman, какая погода на сегодня?", "extra", KeyboardButtonColor.Primary);
            builder.AddButton("Быкman, нужно трек качнуть из моих аудио", "extra", KeyboardButtonColor.Positive);
            builder.AddButton("Быкman, мемос хочу", "extra", KeyboardButtonColor.Negative);
            return builder.Build();
        }

        

    }

    
}
