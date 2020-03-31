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

        private KeyboardBuilder keyBuilder;
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
                case "message_new":
                    {
                        SendTextMessage(msg.PeerId.Value, "случился бан");
                        break;
                    }
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

    }

    
}
