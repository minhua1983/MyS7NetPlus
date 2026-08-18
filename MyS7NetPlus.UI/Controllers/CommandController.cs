using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using MyS7NetPlus.Common.DataAcquisitions;
using MyS7NetPlus.Common.Tools;
using NLog;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace MyS7NetPlus.UI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly Logger _logger = LogManager.GetLogger("WebApiLogger");
        ConcurrentQueue<MyS7Task> _globalSendQueue;

        public CommandController(ConcurrentQueue<MyS7Task> globalSendQueue)
        {
            _globalSendQueue = globalSendQueue;
        }

        [HttpGet("Echo")]
        public IActionResult Echo()
        {
            _logger.Info("CommandController Echo接口被调用");
            return Ok(new { msg = "kestrel is running inside winform!", time = DateTime.Now });
        }

        [HttpGet("WriteAsync")]
        public async Task<IActionResult> WriteAsync()
        {
            _logger.Info("CommandController WriteAsync接口被调用");

            MyS7Task myS7Task = new()
            {
                MyS7TaskType = MyS7TaskType.WriteAsync,
                IpAddress = "192.168.71.50",
                //TaskCompletionSource = new(),
                StartAddress = "DB1.DBD10",
                ValueType = "Single",
                Value = -45.67f
            };

            _ = await MyS7Context.GetMyS7TaskResult(_globalSendQueue, myS7Task, HttpContext.RequestAborted);

            return Ok(new { msg = "kestrel is running inside winform!", time = DateTime.Now });
        }

        [HttpGet("ReadTagsFromMemory")]
        public async Task<IActionResult> ReadTagsFromMemory()
        {
            _logger.Info("CommandController Test1接口被调用");

            MyS7Task myS7Task = new()
            {
                MyS7TaskType = MyS7TaskType.ReadTagsFromMemory,
                IpAddress = "192.168.71.50",
                //TaskCompletionSource = new(),
            };

            var result = (MyDevice)await MyS7Context.GetMyS7TaskResult(_globalSendQueue, myS7Task, HttpContext.RequestAborted);

            return Ok(result);
        }

        [HttpGet("ReadAsync")]
        public async Task<IActionResult> ReadAsync()
        {
            _logger.Info("CommandController ReadAsync接口被调用");

            MyS7Task myS7Task = new()
            {
                MyS7TaskType = MyS7TaskType.ReadAsync,
                IpAddress = "192.168.71.50",
                //TaskCompletionSource = new(),
                StartAddress = "DB1.DBD10",
                ValueType = "Single",
            };

            var result = await MyS7Context.GetMyS7TaskResult(_globalSendQueue, myS7Task, HttpContext.RequestAborted);

            return Ok(new { data = result ,msg = "kestrel is running inside winform!", time = DateTime.Now });
        }
    }

}

