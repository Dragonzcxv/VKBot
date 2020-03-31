using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using System.Threading;
using System.Text;
using System.Net;
using System.Collections.Generic;
using VkNet.Model.Attachments;
using VkNet.Model.Keyboard;
using System.Collections;

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
        
        public CallbackController(IVkApi vkApi,IConfiguration configuration)
        {
            _vkApi = vkApi;
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Callback([FromBody]Updates updates)
        {
            switch (updates.Type)
            {
                case "confirmation":
                    return Ok(_configuration["Config:Confirmation"]);
                case "message_new":
                    {
                        // Десериализаци
                        var msg = Message.FromJson(new VkResponse(updates.Object));
                        //var uploadServer = _vkApi.Docs.GetMessagesUploadServer(msg.PeerId.Value, DocMessageType.AudioMessage);
                        //var wc = new WebClient();
                        //var doc = Encoding.UTF8.GetString(wc.UploadFile(uploadServer.UploadUrl, @"D:\Downloads\Low Roar\2014 – Hávallagata 30 – EP\2.mp3"));
                        //var voiceMes = _vkApi.Docs.Save(doc, "mes", "test")[0].Instance;
                        //var attac = new List<MediaAttachment>
                        //{
                        //    _vkApi.Docs.Save(doc, "mes", "test")[0].Instance
                        //};
                        

                        if (msg.Text.ToLower() == "да")
                        {
                            
                            _vkApi.Messages.SetActivity("193439141", MessageActivityType.Typing, msg.PeerId.Value);
                            Thread.Sleep(5000);
                            KeyboardBuilder keyBuilder = new KeyboardBuilder();
                            keyBuilder.AddButton("Дед", "Доп инфа", KeyboardButtonColor.Primary, "text");
                            MessageKeyboard keyboard = keyBuilder.Build();
                            


                            _vkApi.Messages.Send(new MessagesSendParams
                            {
                                RandomId = new DateTime().Millisecond,
                                Keyboard = keyboard,
                                PeerId = msg.PeerId.Value,             
                                Message = updates.Type
                            });
                        }
                        break;
                    }
            }
            return Ok(updates.Type);
        }

        
        
    }
}
