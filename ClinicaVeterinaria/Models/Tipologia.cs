using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ClinicaVeterinaria.Models
{
    public class Tipologia
    {
        [Display(Name = "Tipologia")]
        public int IdTipologia { get; set; }

        [Display(Name = "Tipologia")]
        public string NomeTipologia { get; set; }
    }
}