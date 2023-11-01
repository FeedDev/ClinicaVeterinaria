using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicaVeterinaria.Models
{
    public class LoginUtente
    {
        public int IdLogin { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}