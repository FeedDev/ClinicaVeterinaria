using ClinicaVeterinaria.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using static System.Net.Mime.MediaTypeNames;

namespace ClinicaVeterinaria.Controllers
{
    [Authorize]
    public class ClinicaVeterinariaController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            List<Paziente> ListaPazienti = new List<Paziente>();
            try
            {
                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Paziente INNER JOIN Tipologia ON Paziente.IdTipologia = Tipologia.IdTipologia", sqlConnection);

                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        Paziente p = new Paziente();
                        p.IdPaziente = Convert.ToInt32(sqlDataReader["IdPaziente"]);
                        p.Nome = sqlDataReader["Nome"].ToString();
                        p.DataRegistrazione = Convert.ToDateTime(sqlDataReader["DataRegistrazione"]);
                        p.Tipologia = new Tipologia();
                        p.Tipologia.NomeTipologia = sqlDataReader["NomeTipologia"].ToString();
                        p.DataNascita = Convert.ToDateTime(sqlDataReader["DataNascita"]);
                        p.ColoreMantello = sqlDataReader["ColoreMantello"].ToString();
                        p.Microchip = Convert.ToBoolean(sqlDataReader["Microchip"]);
                        p.NChip = Convert.ToInt32(sqlDataReader["NChip"]);
                        p.NomeProprietario = sqlDataReader["NomeProprietario"].ToString();
                        p.CognomeProprietario = sqlDataReader["CognomeProprietario"].ToString();
                        p.FotoPaziente = sqlDataReader["FotoPaziente"].ToString();

                        ListaPazienti.Add(p);
                    }
                }

                sqlConnection.Close();
                ViewBag.ListaTipologie = DashboardTipologie();
                return View(ListaPazienti);

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        public List<Tipologia> DashboardTipologie()
        {

            SqlConnection sqlConnection = ConnessioneDB();
            sqlConnection.Open();
            List<Tipologia> listaTipologie = new List<Tipologia>();

            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Tipologia", sqlConnection);

            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

            if (sqlDataReader.HasRows)
            {
                while (sqlDataReader.Read())
                {
                    Tipologia t = new Tipologia();

                    t.IdTipologia = Convert.ToInt32(sqlDataReader["IdTipologia"]);
                    t.NomeTipologia = sqlDataReader["NomeTipologia"].ToString();

                    listaTipologie.Add(t);
                }
            }

            sqlConnection.Close();
            return listaTipologie;
        }

        public static SqlConnection ConnessioneDB()
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["ConnessioneDB"].ConnectionString;
            SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            return sqlConnection;
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard");

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(LoginUtente l)
        {
            try
            {
                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Login", sqlConnection);

                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        if (l.Username == sqlDataReader["Username"].ToString() && l.Password == sqlDataReader["Password"].ToString())
                        {
                            FormsAuthentication.SetAuthCookie(l.Username, false);

                            HttpCookie cookiePropID = new HttpCookie("USER_COOKIE");
                            cookiePropID.Values["ID"] = sqlDataReader["IdLogin"].ToString();
                            Response.Cookies.Add(cookiePropID);

                            TempData["Successo"] = $"Benvenuto {l.Username}";

                            sqlConnection.Close();
                            return Redirect(FormsAuthentication.DefaultUrl);

                        }
                        else
                        {
                            ViewBag.Errore = "Username o password errate";
                        }
                    }
                }

                sqlConnection.Close();

            }
            catch
            {
                return View();
            }

            return View();
        }

        public ActionResult SignOut()
        {
            FormsAuthentication.SignOut();
            return Redirect(FormsAuthentication.LoginUrl);

        }

        public static List<SelectListItem> DropDownTipologie()
        {
            SqlConnection sqlConnection = ConnessioneDB();
            sqlConnection.Open();

            SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Tipologia", sqlConnection);

            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

            List<SelectListItem> listaTipologia = new List<SelectListItem>();
            if (sqlDataReader.HasRows)
            {
                while (sqlDataReader.Read())
                {
                    SelectListItem t = new SelectListItem
                    {
                        Text = sqlDataReader["NomeTipologia"].ToString(),
                        Value = sqlDataReader["IdTipologia"].ToString(),
                    };

                    listaTipologia.Add(t);
                }
            }

            sqlConnection.Close();

            return listaTipologia;
        }

        [HttpGet]
        public ActionResult CreatePaziente()
        {
            List<SelectListItem> listaTipologie = DropDownTipologie();

            ViewBag.Tipologia = listaTipologie;
            return View();
        }

        [HttpPost]
        public ActionResult CreatePaziente(Paziente p)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SqlConnection sqlConnection = ConnessioneDB();
                    sqlConnection.Open();

                    SqlCommand sqlCommand = new SqlCommand("INSERT INTO Paziente VALUES(@nome, @dataregistrazione, @idtipologia, @colore, @datanascita, @microchip, @nchip, @nomeprop, @cognomeprop, @foto)", sqlConnection);

                    sqlCommand.Parameters.AddWithValue("nome", p.Nome);
                    sqlCommand.Parameters.AddWithValue("dataregistrazione", p.DataRegistrazione);
                    sqlCommand.Parameters.AddWithValue("idtipologia", p.Tipologia.IdTipologia);
                    sqlCommand.Parameters.AddWithValue("colore", p.ColoreMantello);
                    sqlCommand.Parameters.AddWithValue("datanascita", p.DataNascita);

                    if (p.NChip > 0)
                    {
                        p.Microchip = true;
                    }
                    else
                    {
                        p.Microchip = false;
                    }

                    sqlCommand.Parameters.AddWithValue("nchip", p.NChip);
                    sqlCommand.Parameters.AddWithValue("microchip", p.Microchip);

                    if (p.NomeProprietario != null && p.CognomeProprietario != null)
                    {
                        sqlCommand.Parameters.AddWithValue("nomeprop", p.NomeProprietario);
                        sqlCommand.Parameters.AddWithValue("cognomeprop", p.CognomeProprietario);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("nomeprop", DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("cognomeprop", DBNull.Value);
                    }

                    //INSERIMENTO FILE NELLA CARTELLA IMG
                    if (p.Photo != null)
                    {
                        if (p.Photo.ContentLength > 0)
                        {
                            string fileName = p.Photo.FileName;
                            string path = Server.MapPath("~/Content/img");
                            p.Photo.SaveAs($"{path}/{fileName}");
                            sqlCommand.Parameters.AddWithValue("foto", p.Photo.FileName);
                        }
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("foto", "default.png");
                    }

                    int righeInserite = sqlCommand.ExecuteNonQuery();


                    if (righeInserite > 0)
                    {
                        TempData["Successo"] = "Animale aggiunto correttamente";
                    }
                    sqlConnection.Close();
                    CreateRicovero();

                    return RedirectToAction("Dashboard");
                }
                else
                {
                    TempData["Errore"] = "Non hai inserito i campi correttamente";
                    return RedirectToAction("Dashboard");
                }
            }
            catch (Exception ex)
            {
                TempData["Errore"] = ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        public void CreateRicovero()
        {
            SqlConnection sqlConnection = ConnessioneDB();
            sqlConnection.Open();

            SqlCommand sqlCommand = new SqlCommand("SELECT TOP 1 * FROM Paziente ORDER BY IdPaziente DESC ", sqlConnection);

            SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
            Paziente p = new Paziente();
            if (sqlDataReader.HasRows)
            {
                while (sqlDataReader.Read())
                {
                    p.IdPaziente = Convert.ToInt32(sqlDataReader["IdPaziente"]);
                    p.DataRegistrazione = Convert.ToDateTime(sqlDataReader["DataRegistrazione"]);
                }
            }

            sqlConnection.Close();

            sqlConnection.Open();

            sqlCommand = new SqlCommand("INSERT INTO Ricovero VALUES(@dataricovero, @idpaziente) ", sqlConnection);
            sqlCommand.Parameters.AddWithValue("dataricovero", p.DataRegistrazione);
            sqlCommand.Parameters.AddWithValue("idpaziente", p.IdPaziente);

            sqlCommand.ExecuteNonQuery();

            sqlConnection.Close();

        }

        [HttpGet]
        public ActionResult EditPaziente(int id)
        {
            try
            {
                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Paziente Where IdPaziente = @idpaziente", sqlConnection);

                sqlCommand.Parameters.AddWithValue("idpaziente", id);

                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                Paziente paziente = new Paziente();
                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        paziente.IdPaziente = Convert.ToInt32(sqlDataReader["IdPaziente"]);
                        paziente.DataRegistrazione = Convert.ToDateTime(sqlDataReader["DataRegistrazione"]);
                        paziente.Tipologia = new Tipologia();
                        paziente.Tipologia.IdTipologia = Convert.ToInt32(sqlDataReader["IdTipologia"]);
                        paziente.ColoreMantello = sqlDataReader["ColoreMantello"].ToString();
                        paziente.DataNascita = Convert.ToDateTime(sqlDataReader["DataNascita"]);
                        paziente.Microchip = Convert.ToBoolean(sqlDataReader["Microchip"]);
                        paziente.NChip = Convert.ToInt32(sqlDataReader["NChip"]);
                        paziente.Nome = sqlDataReader["Nome"].ToString();
                        paziente.NomeProprietario = sqlDataReader["NomeProprietario"].ToString();
                        paziente.CognomeProprietario = sqlDataReader["CognomeProprietario"].ToString();
                        paziente.FotoPaziente = sqlDataReader["FotoPaziente"].ToString();
                    }
                }
                else
                {
                    return RedirectToAction("Dashboard");
                }

                return View(paziente);

            }
            catch (Exception ex)
            {
                TempData["Errore"] = ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        [HttpPost]
        public ActionResult EditPaziente(Paziente p)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SqlConnection sqlConnection = ConnessioneDB();
                    sqlConnection.Open();

                    SqlCommand sqlCommand = new SqlCommand("UPDATE Paziente SET Nome=@nome, NomeProprietario=@nomeprop, CognomeProprietario=@cognomeprop, FotoPaziente=@foto WHERE IdPaziente=@idpaziente", sqlConnection);

                    sqlCommand.Parameters.AddWithValue("idpaziente", p.IdPaziente);

                    sqlCommand.Parameters.AddWithValue("nome", p.Nome);
                    if (p.NomeProprietario != null && p.CognomeProprietario != null)
                    {
                        sqlCommand.Parameters.AddWithValue("nomeprop", p.NomeProprietario);
                        sqlCommand.Parameters.AddWithValue("cognomeprop", p.CognomeProprietario);
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("nomeprop", DBNull.Value);
                        sqlCommand.Parameters.AddWithValue("cognomeprop", DBNull.Value);
                    }

                    //INSERIMENTO FILE NELLA CARTELLA IMG
                    if (p.Photo != null)
                    {
                        if (p.Photo.ContentLength > 0)
                        {
                            string fileName = p.Photo.FileName;
                            string path = Server.MapPath("~/Content/img");
                            p.Photo.SaveAs($"{path}/{fileName}");


                            sqlCommand.Parameters.AddWithValue("foto", p.Photo.FileName);
                        }
                    }
                    else
                    {
                        sqlCommand.Parameters.AddWithValue("foto", p.FotoPaziente);
                    }

                    int righeInserite = sqlCommand.ExecuteNonQuery();

                    if (righeInserite > 0)
                    {
                        TempData["Successo"] = "Animale aggiunto correttamente";
                    }

                    sqlConnection.Close();
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ViewBag.Errore = "Non hai inserito i campi correttamente";
                    return View(p);
                }
            }
            catch (Exception ex)
            {
                TempData["Errore"] = ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult DettagliPaziente(int id)
        {
            try
            {
                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Paziente INNER JOIN Tipologia ON Tipologia.IdTipologia = Paziente.IdTipologia WHERE Paziente.IdPaziente=@idpaziente", sqlConnection);

                sqlCommand.Parameters.AddWithValue("idpaziente", id);

                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                Paziente p = new Paziente();
                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        p.IdPaziente = Convert.ToInt32(sqlDataReader["IdPaziente"]);
                        p.Nome = sqlDataReader["Nome"].ToString();
                        p.DataRegistrazione = Convert.ToDateTime(sqlDataReader["DataRegistrazione"]);
                        p.Tipologia = new Tipologia();
                        p.Tipologia.NomeTipologia = sqlDataReader["NomeTipologia"].ToString();
                        p.DataNascita = Convert.ToDateTime(sqlDataReader["DataNascita"]);
                        p.ColoreMantello = sqlDataReader["ColoreMantello"].ToString();
                        p.Microchip = Convert.ToBoolean(sqlDataReader["Microchip"]);
                        p.NChip = Convert.ToInt32(sqlDataReader["NChip"]);
                        p.NomeProprietario = sqlDataReader["NomeProprietario"].ToString();
                        p.CognomeProprietario = sqlDataReader["CognomeProprietario"].ToString();
                        p.FotoPaziente = sqlDataReader["FotoPaziente"].ToString();
                    }
                }

                sqlConnection.Close();
                sqlConnection.Open();

                sqlCommand = new SqlCommand("SELECT * FROM Visita INNER JOIN Login ON Login.IdLogin = Visita.IdLogin WHERE IdPaziente=@idpaziente", sqlConnection);

                sqlCommand.Parameters.AddWithValue("idpaziente", id);

                sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        Visita visita = new Visita();
                        visita.DescrizioneVisita = sqlDataReader["DescrizioneVisita"].ToString();
                        visita.DataVisita = Convert.ToDateTime(sqlDataReader["DataVisita"]);
                        visita.IdVisita = Convert.ToInt32(sqlDataReader["IdVisita"]);
                        visita.Paziente = new Paziente();
                        visita.Paziente.IdPaziente = Convert.ToInt32(sqlDataReader["IdPaziente"]);
                        visita.Medico = new LoginUtente();
                        visita.Medico.Username = sqlDataReader["Username"].ToString();
                        visita.Esame = sqlDataReader["Esame"].ToString();
                        p.listaVisite.Add(visita);
                    }
                }

                sqlConnection.Close();
                return View(p);
            }
            catch (Exception ex)
            {
                ViewBag.Errore = ex.Message;
                return RedirectToAction("Dashboard");
            }
        }

        [HttpGet]
        public ActionResult CreateVisita()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateVisita(Visita v, int idPaziente)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SqlConnection sqlConnection = ConnessioneDB();
                    sqlConnection.Open();

                    SqlCommand sqlCommand = new SqlCommand("INSERT INTO Visita VALUES(@datavisita, @esame, @descrizione, @idpaziente, @idlogin)", sqlConnection);

                    sqlCommand.Parameters.AddWithValue("idpaziente", idPaziente);
                    sqlCommand.Parameters.AddWithValue("datavisita", DateTime.Now);
                    sqlCommand.Parameters.AddWithValue("esame", v.Esame);
                    sqlCommand.Parameters.AddWithValue("descrizione", v.DescrizioneVisita);
                    sqlCommand.Parameters.AddWithValue("idlogin", Convert.ToInt32(Request.Cookies["USER_COOKIE"]["ID"]));

                    int righeInserite = sqlCommand.ExecuteNonQuery();

                    if (righeInserite > 0)
                    {
                        TempData["Successo"] = "Animale aggiunto correttamente";
                    }

                    sqlConnection.Close();
                    return RedirectToAction("DettagliPaziente", new { id = idPaziente });
                }
                else
                {
                    ViewBag.Errore = "Non hai inserito i campi correttamente";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Errore = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public ActionResult DeleteVisita(int id)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SqlConnection sqlConnection = ConnessioneDB();
                    sqlConnection.Open();
                    SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Visita INNER JOIN Paziente ON Visita.IdPaziente = Paziente.IdPaziente WHERE IdVisita=@idvisita", sqlConnection);

                    sqlCommand.Parameters.AddWithValue("idvisita", id);

                    SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                    Visita visita = new Visita();

                    if (sqlDataReader.HasRows)
                    {
                        while (sqlDataReader.Read())
                        {
                            visita.DescrizioneVisita = sqlDataReader["DescrizioneVisita"].ToString();
                            visita.DataVisita = Convert.ToDateTime(sqlDataReader["DataVisita"]);
                            visita.IdVisita = Convert.ToInt32(sqlDataReader["IdVisita"]);
                            visita.Paziente = new Paziente();
                            visita.Paziente.IdPaziente = Convert.ToInt32(sqlDataReader["IdPaziente"]);
                            visita.Paziente.Nome = sqlDataReader["Nome"].ToString();
                            visita.Medico = new LoginUtente();
                            visita.Medico.IdLogin = Convert.ToInt32(sqlDataReader["IdLogin"]);
                            visita.Esame = sqlDataReader["Esame"].ToString();
                        }
                    }

                    sqlConnection.Close();
                    return View(visita);
                }
                else
                {
                    ViewBag.Errore = "Non hai inserito i campi correttamente";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Errore = ex.Message;
                return View();
            }
        }

        [HttpPost]
        [ActionName("DeleteVisita")]
        public ActionResult ConfirmDeleteVisita(int id, int IdPaziente)
        {
            try
            {

                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("DELETE Visita WHERE IdVisita=@idvisita", sqlConnection);
                sqlCommand.Parameters.AddWithValue("idvisita", id);

                int righeInserite = sqlCommand.ExecuteNonQuery();

                if (righeInserite > 0)
                {
                    TempData["Successo"] = "Animale aggiunto correttamente";
                }

                sqlConnection.Close();
                return RedirectToAction("DettagliPaziente", new { id = IdPaziente });

            }
            catch (Exception ex)
            {
                ViewBag.Errore = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public JsonResult CreateTipologie(string NomeTipologia)
        {
            SqlConnection sqlConnection = ConnessioneDB();
            sqlConnection.Open();

            SqlCommand sqlCommand = new SqlCommand("INSERT INTO Tipologia VALUES(@nometipologia)", sqlConnection);

            sqlCommand.Parameters.AddWithValue("nometipologia", NomeTipologia);

            Tipologia t = new Tipologia();
            int righeInserite = sqlCommand.ExecuteNonQuery();

            if (righeInserite > 0)
            {
                t.NomeTipologia = NomeTipologia;
                TempData["Successo"] = "Tipologia aggiunta correttamente";
            }

            sqlConnection.Close();
            return Json(t, JsonRequestBehavior.AllowGet);
        }


        /*[HttpGet]
        public ActionResult DeleteTipologie(int id)
        {
            try
            {
                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Tipologia WHERE IdTipologia=@idtipologia", sqlConnection);
                sqlCommand.Parameters.AddWithValue("idtipologia", id);

                Tipologia t = new Tipologia();
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        t.IdTipologia = Convert.ToInt32(sqlDataReader["IdTipologia"]);
                        t.NomeTipologia = sqlDataReader["NomeTipologia"].ToString();
                    }
                }

                sqlConnection.Close();
                return View(t);

            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }*/

        [HttpGet]
        public JsonResult DeleteTipologia(int IdTipologia)
        {
            try
            {
                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("DELETE Tipologia WHERE IdTipologia=@idtipologia", sqlConnection);
                sqlCommand.Parameters.AddWithValue("idtipologia", IdTipologia);

                sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
                return Json("ok", JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                return Json("ko", JsonRequestBehavior.AllowGet);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult RicercaAnimali()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult RicoveriAttivi()
        {
            List<Tipologia> ListaTipologie = new List<Tipologia>();
            try
            {
                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Tipologia", sqlConnection);

                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        Tipologia t = new Tipologia();
                        t.IdTipologia = Convert.ToInt32(sqlDataReader["IdTipologia"]);
                        t.NomeTipologia = sqlDataReader["NomeTipologia"].ToString();


                        ListaTipologie.Add(t);
                    }
                }
                sqlConnection.Close();
                return View(ListaTipologie);

            }
            catch (Exception ex)
            {
                ViewBag.Err = ex.Message;
                return View();
            }
        }
        [AllowAnonymous]
        [HttpPost]
        public JsonResult FiltroRicoveriAttivi(string[] tipologie)
        {
                List<Ricovero> listRicovero = new List<Ricovero>();

                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Tipologia", sqlConnection);

                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                Tipologia t = new Tipologia();
                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        if(tipologie != null)
                        {
                            foreach (string tip in tipologie)
                            {
                                if (tip == sqlDataReader["NomeTipologia"].ToString())
                                {
                                    SqlConnection sqlConnection2 = ConnessioneDB();
                                    sqlConnection2.Open();

                                    SqlCommand sqlCommand2 = new SqlCommand("SELECT * FROM Ricovero INNER JOIN Paziente ON Ricovero.IdPaziente = Paziente.IdPaziente WHERE Paziente.IdTipologia=@idtipologia", sqlConnection2);

                                    int id = Convert.ToInt32(sqlDataReader["IdTipologia"]);

                                    sqlCommand2.Parameters.AddWithValue("idtipologia", id);

                                    SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();


                                    if (sqlDataReader2.HasRows)
                                    {
                                        while (sqlDataReader2.Read())
                                        {
                                            Ricovero r = new Ricovero();
                                            r.Paziente = new Paziente();
                                            r.Paziente.Nome = sqlDataReader2["Nome"].ToString();
                                            r.Paziente.Tipologia = new Tipologia();
                                            r.Paziente.IdPaziente = Convert.ToInt32(sqlDataReader2["IdPaziente"]);
                                            r.Paziente.Tipologia.NomeTipologia = sqlDataReader["NomeTipologia"].ToString();
                                            r.Paziente.FotoPaziente = sqlDataReader2["FotoPaziente"].ToString();
                                            r.DataRicovero = Convert.ToDateTime(sqlDataReader2["DataRicovero"]);
                                            r.DataRicoveroString = r.DataRicovero.Day + "/" + r.DataRicovero.Month + "/" + r.DataRicovero.Year;

                                            listRicovero.Add(r);
                                        }
                                    }

                                    sqlConnection2.Close();
                                }
                            }
                        }
                    }
                }

                sqlConnection.Close();
                return Json(listRicovero,JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AllowAnonymous]
        public JsonResult RicercaForChip(int NChip)
        {
            if (NChip > 0)
            {
                SqlConnection sqlConnection = ConnessioneDB();
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT * FROM Paziente INNER JOIN Tipologia ON Tipologia.IdTipologia = Paziente.IdTipologia WHERE Paziente.NChip=@nchip", sqlConnection);

                sqlCommand.Parameters.AddWithValue("nchip", NChip);

                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                Paziente p = new Paziente();
                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        p.IdPaziente = Convert.ToInt32(sqlDataReader["IdPaziente"]);
                        p.Nome = sqlDataReader["Nome"].ToString();
                        p.DataRegistrazione = Convert.ToDateTime(sqlDataReader["DataRegistrazione"]);
                        p.Tipologia = new Tipologia();
                        p.Tipologia.NomeTipologia = sqlDataReader["NomeTipologia"].ToString();
                        p.DataNascita = Convert.ToDateTime(sqlDataReader["DataNascita"]);
                        p.ColoreMantello = sqlDataReader["ColoreMantello"].ToString();
                        p.Microchip = Convert.ToBoolean(sqlDataReader["Microchip"]);
                        p.NChip = Convert.ToInt32(sqlDataReader["NChip"]);
                        p.NomeProprietario = sqlDataReader["NomeProprietario"].ToString();
                        p.CognomeProprietario = sqlDataReader["CognomeProprietario"].ToString();
                        p.FotoPaziente = sqlDataReader["FotoPaziente"].ToString();
                        p.DataRegistrazioneString = p.DataRegistrazione.Day + "/" + p.DataRegistrazione.Month + "/" + p.DataRegistrazione.Year;
                        p.DataNascitaString = p.DataNascita.Day + "/" + p.DataNascita.Month + "/" + p.DataNascita.Year;
                    }

                }
                else
                {
                    return Json("ko", JsonRequestBehavior.AllowGet);
                }

                sqlConnection.Close();
                sqlConnection.Open();

                sqlCommand = new SqlCommand("SELECT * FROM Visita INNER JOIN Login ON Login.IdLogin = Visita.IdLogin WHERE IdPaziente=@idpaziente", sqlConnection);

                sqlCommand.Parameters.AddWithValue("idpaziente", p.IdPaziente);

                sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        Visita visita = new Visita();
                        visita.DescrizioneVisita = sqlDataReader["DescrizioneVisita"].ToString();
                        visita.DataVisita = Convert.ToDateTime(sqlDataReader["DataVisita"]);
                        visita.IdVisita = Convert.ToInt32(sqlDataReader["IdVisita"]);
                        visita.Paziente = new Paziente();
                        visita.Paziente.IdPaziente = Convert.ToInt32(sqlDataReader["IdPaziente"]);
                        visita.Medico = new LoginUtente();
                        visita.Medico.Username = sqlDataReader["Username"].ToString();
                        visita.Esame = sqlDataReader["Esame"].ToString();
                        visita.DataVisitaString = visita.DataVisita.Day + "/" + visita.DataVisita.Month + "/" + visita.DataVisita.Year;
                        p.listaVisite.Add(visita);
                    }
                }

                sqlConnection.Close();
                return Json(p, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("ko", JsonRequestBehavior.AllowGet);
            }
        }
            
    }
}
