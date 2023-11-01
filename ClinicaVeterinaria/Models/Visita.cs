using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClinicaVeterinaria.Models
{
    public class Visita
    {
        [Display(Name = "Id Visita")]
        public int IdVisita { get; set; }
        [Display(Name = "Data Visita")]
        public DateTime DataVisita { get; set; }
        public string Esame { get; set; }
        [Display (Name = "Descrizione Visita")]
        public string DescrizioneVisita { get; set; }
        public Paziente Paziente { get; set; }
        public LoginUtente Medico { get; set; }
        public string DataVisitaString { get; set; }
    }
}