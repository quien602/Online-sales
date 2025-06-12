using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class QuyenController : Controller
    {
        // GET: Admin/Quyen
        private string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

        // Method to get all permissions
        public ActionResult Index()
        {
            List<Quyen> quyens = new List<Quyen>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT * FROM Quyen";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Quyen quyen = new Quyen
                            {
                                MaQuyen = reader["MaQuyen"].ToString(),
                                TenQuyen = reader["TenQuyen"].ToString()
                            };
                            quyens.Add(quyen);
                        }
                    }
                }
            }

            return View(quyens);
        }
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Quyen model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Open connection before executing command

                    string query = "INSERT INTO Quyen (MaQuyen, TenQuyen) VALUES (@MaDM, @TenDM)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaDM", model.MaQuyen);
                        command.Parameters.AddWithValue("@TenDM", model.TenQuyen);
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            return View(model);
        }



        public ActionResult Delete(string MaQuyen)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "DELETE FROM Quyen WHERE MaQuyen = @MaQuyen";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaQuyen", MaQuyen);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true });
                    }
                }
            }

            return Json(new { success = false });
        }


    }
}