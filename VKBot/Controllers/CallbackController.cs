using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;
using System.Threading;

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
                        // Десериализация
                        Console.WriteLine(updates.Type);
                        Console.WriteLine(updates.Object);
                        Console.WriteLine(updates.GroupId);
                        var msg = Message.FromJson(new VkResponse(updates.Object));

                        if(msg.Text.ToLower() == "да")
                        {
                            _vkApi.Messages.SetActivity("193439141", MessageActivityType.Typing, msg.PeerId.Value);
                            

                            _vkApi.Messages.Send(new MessagesSendParams
                            {
                                RandomId = new DateTime().Millisecond,
                                PeerId = msg.PeerId.Value,
                                Message = updates.Type.ToString()+updates.GroupId.ToString()
                            });
                        }
                        break;
                    }
            }
            return Ok("ok");
        }
    }
}
