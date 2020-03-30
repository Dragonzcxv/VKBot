using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;

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
                        var msg = Message.FromJson(new VkResponse(updates.Object));

                        if(msg.Text.ToLower() == "да")
                        {
                            _vkApi.Messages.SetActivity("193439141", MessageActivityType.Typing, msg.PeerId.Value);
                            //_vkApi.Messages.Send(new MessagesSendParams
                            //{
                            //    RandomId = new DateTime().Millisecond,
                            //    PeerId = msg.PeerId.Value,
                            //    Message = "Ну тогда полезай на жопу "
                            //});
                        }
                        break;
                    }
            }
            return Ok("ok");
        }
    }
}
