using System;
using FS.DI;
using FS.Extends;
using FSS.Application.Clients.Client;
using FSS.Application.Clients.Client.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FSS.Service.Controllers
{
    public class BaseController : ControllerBase
    {
        protected IHttpContextAccessor HttpContextAccessor { get; }

        /// <summary>
        /// 客户端请求IP
        /// </summary>
        protected ClientDTO Client { get; }

        public BaseController(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
            Client = new ClientDTO
            {
                ClientIp   = HttpContextAccessor.HttpContext.Request.Headers["ClientIp"].ToString().Split(',')[0].Trim(),
                ClientName = HttpContextAccessor.HttpContext.Request.Headers["ClientName"],
                Id         = HttpContextAccessor.HttpContext.Request.Headers["ClientId"].ToString().ConvertType(0L),
                Jobs       = HttpContextAccessor.HttpContext.Request.Headers["ClientJobs"].ToString().Split(','),
                ActivateAt = DateTime.Now
            };

            // 更新客户端的使用时间
            IocManager.Instance.Resolve<ClientApp>().UpdateClient(Client);
        }
    }
}