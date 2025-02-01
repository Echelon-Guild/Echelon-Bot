using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EchelonBot.Services
{
    public class CalendarService
    {
        private readonly IConfiguration _config;

        public CalendarService(IConfiguration config)
        {
            _config = config;
        }

        public async Task ConnectAsync()
        {
            throw new NotImplementedException();
        }

        
    }
}
