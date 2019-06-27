using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BAQFacade.Utils;
using Microsoft.Extensions.Options;
using System.IO;
using RestSharp;
using Microsoft.AspNetCore.Http;

namespace BAQFacade.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EpicorController : ControllerBase
    {

        private readonly IOptions<EpiSettings> _epiSettings;
        private readonly string _user;
        private readonly string _path;
        private readonly string _authBAQ;

        public EpicorController(IOptions<EpiSettings> app)
        {
            _epiSettings = app;
            _user = $"{_epiSettings.Value.User}:{_epiSettings.Value.Password}";
            _path = $"{_epiSettings.Value.Host}/{_epiSettings.Value.Instance}/api/v{_epiSettings.Value.ApiVersion}";
            _authBAQ = _epiSettings.Value.AuthBAQ;
        }


        /// <summary>
        /// Checks if the given user has a valid Token/BAQ Combo
        /// If it does it runs a GET on the given BAQ / Params
        /// </summary>
        /// <param name="BAQID">BAQID</param>
        /// <returns></returns>
        [HttpGet("{BAQID}")]
        public IActionResult Get(string BAQID)
        {
            string msg = "";
            if (EpiUtils.ValidSession(this.Request.Headers["Authorization"].ToString().Replace("Bearer ", ""), BAQID, _path, _user, _authBAQ, out msg))
            {
                var restClient = new RestSharp.RestClient(_path);
                var request = new RestSharp.RestRequest($"BaqSvc/{BAQID}");
                foreach (var p in this.Request.Query)
                {
                    request.Parameters.Add(new RestSharp.Parameter(p.Key, p.Value, RestSharp.ParameterType.QueryString));
                }

                request.AddHeader("Authorization", $"Basic {EpiUtils.Base64Encode(_user)}");
                IRestResponse response = restClient.Execute(request);
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.BadRequest:
                        {
                            return BadRequest(response.Content);
                        }
                    case System.Net.HttpStatusCode.OK:
                    default:
                        {
                            return Ok(response.Content);
                        }
                }
            }
            else
            {
                return Unauthorized(msg);
            }
        }

        /// <summary>
        /// Validates if a user has a valid session , if so it runs a PATCH on the given BAQ / Params and returns the results
        /// </summary>
        /// <param name="BAQID"></param>
        /// <returns></returns>
        [HttpPatch("{BAQID}")]
        public IActionResult Patch(string BAQID)
        {
            string msg = "";
            if (EpiUtils.ValidSession(this.Request.Headers["Authorization"].ToString().Replace("Bearer ", ""), BAQID, _path, _user, _authBAQ, out msg))
            {
                var restClient = new RestSharp.RestClient(_path);
                var request = new RestSharp.RestRequest($"BaqSvc/{BAQID}");
                foreach (var p in this.Request.Query)
                {
                    request.Parameters.Add(new RestSharp.Parameter(p.Key, p.Value, RestSharp.ParameterType.QueryString));
                }

                request.AddHeader("Authorization", $"Basic {EpiUtils.Base64Encode(_user)}");
                

                using (StreamReader sr = new StreamReader(this.Request.Body))
                {
                    request.AddJsonBody(sr.ReadToEnd());
                }
                IRestResponse response = restClient.Execute(request, Method.PATCH);
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.BadRequest:
                        {
                            return BadRequest(response.Content);
                        }
                    case System.Net.HttpStatusCode.InternalServerError:
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, response.Content);
                        }
                    case System.Net.HttpStatusCode.OK:
                    default:
                        {
                            return Ok(response.Content);
                        }
                }
            }
            else
            {
                return Unauthorized(msg);
            }
        }
    }
}
