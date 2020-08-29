using System.Collections.Generic;
using System.Threading.Tasks;
using EndDeviceService.Models;
using EndDeviceService.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EndDeviceService.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TargetsController : ControllerBase
    {
        private MessageTranService _messageTranService;

        public TargetsController(MessageTranService messageTranService)
        {
            _messageTranService = messageTranService;
        }

        /// <summary>
        /// 获取目标位置
        /// </summary>        
        [HttpGet]
        public ActionResult<Position> Get()
        {
            return Ok(_messageTranService.position);
        }
    }
}