using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClinicaVeterinaria.Models
{
    public class Paziente
    {
        public int IdPaziente { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        [Display(Name = "Data di Registrazione")]
        public DateTime DataRegistrazione { get; set; }

        [Required]
        [Display(Name = "Colore")]
        public string ColoreMantello { get; set; }

        [Required]
        [Display(Name = "Data di nascita")]
        public DateTime DataNascita { get; set; }

        public bool Microchip { get; set; }

        [Display(Name = "N. Chip")]
        public int NChip { get; set; }

        [Display(Name = "Nome Proprietario")]
        public string NomeProprietario { get; set; }

        [Display(Name = "Cognome Proprietario")]
        public string CognomeProprietario { get; set; }

        [Required]
        [Display(Name = "Tipologia")]
        public Tipologia Tipologia { get; set; }

        public List<Visita> listaVisite = new List<Visita>();

        [Display(Name = "Foto")]
        public string FotoPaziente { get; set; }

        public HttpPostedFileBase Photo { get; set; }

        public string DataRegistrazioneString { get; set; }
        public string DataNascitaString { get; set; }
    }
}