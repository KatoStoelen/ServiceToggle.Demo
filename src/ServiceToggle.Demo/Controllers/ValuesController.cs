using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceToggle.Demo.Services;

namespace ServiceToggle.Demo.Controllers
{
    [Route("api/value")]
    public class ValuesController : Controller
    {
        private readonly IValueService _valueService;

        public ValuesController(IValueService valueService)
        {
            _valueService = valueService;
        }

        [HttpGet]
        public string Get()
        {
            return _valueService.GetValue();
        }
    }
}
