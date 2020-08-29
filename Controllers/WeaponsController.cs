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
    public class WeaponsController : ControllerBase
    {
        private MessageTranService _messageTranService;

        public WeaponsController(MessageTranService messageTranService)
        {
            _messageTranService = messageTranService;
        }

        /// <summary>
        /// 向SMP发送外挂数据
        /// </summary>        
        [HttpPost]
        public ActionResult Post([FromBody] Weapon weapon)
        {
            _messageTranService.MessageTranToSMP(weapon);
            return Ok();
        }

        /// <summary>
        /// 获取外挂信息
        /// </summary>        
        [HttpGet]
        public ActionResult<List<Weapon>> Get()
        {
            return Ok(_messageTranService.weaponList);
        }

    }
}