using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ClinicaVeterinaria.Models
{
    public class Ricovero
    {
        [Display(Name = "Id Ricovero")]
        public int IdRicovero { get; set; }
        [Display(Name = "Data Ricovero")]
        public DateTime DataRicovero { get; set; }
        public string DataRicoveroString { get; set; }
        public Paziente Paziente { get; set; }
    }
}