using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stratis.Bitcoin.Utilities;
using Stratis.Bitcoin.Utilities.JsonErrors;
using Stratis.Bitcoin.Utilities.ModelStateErrors;
using x42.Features.xServer.Models;

namespace x42.Features.xServer.Controllers
{
    /// <summary>Controller providing operations for the xServer network.</summary>
    [ApiController]
    [ApiVersion("1")]
    [Route("api/[controller]")]
    public class xServerController : Controller
    {
        /// <summary>Instance logger.</summary>
        private readonly ILogger logger;

        public xServerController(ILoggerFactory loggerFactory)
        {
            Guard.NotNull(loggerFactory, nameof(loggerFactory));

            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
        }

        /// <summary>
        /// Retrieves the xServer stats
        /// </summary>
        /// <returns>Returns the stats of the xServer network.</returns>
        [Route("getxserverstats")]
        [HttpGet]
        public IActionResult GetxServerStats()
        {
            if (!this.ModelState.IsValid)
            {
                return ModelStateErrors.BuildErrorResponse(this.ModelState);
            }

            try
            {
                //TODO: Get Server Stats
                var serverStats = new GetxServerStatsResult()
                {
                    Connected = 0
                };

                return this.Json(serverStats);
            }
            catch (Exception e)
            {
                this.logger.LogError("Exception occurred: {0}", e.ToString());
                return ErrorHelpers.BuildErrorResponse(HttpStatusCode.BadRequest, e.Message, e.ToString());
            }
        }
    }
}