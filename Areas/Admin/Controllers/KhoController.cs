using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class KhoController : Controller
    {
        // GET: Admin/Kho
        public ActionResult Index()
        {
            List<Kho> Khos = new List<Kho>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM Kho";

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Kho kho = new Kho();
                        kho.MaKho = reader["MaKho"].ToString();
                        kho.TenKho = reader["TenKho"].ToString();
                        kho.Diachi = reader["Diachi"].ToString();
                        // Thêm các trường dữ liệu khác tương ứng

                        Khos.Add(kho);
                    }
                }
            }
            return View(Khos);
        }
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Kho model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Open connection before executing command

                    string query = "INSERT INTO Kho (MaKho, TenKho, Diachi) VALUES (@MaKho, @TenKho, @Diachi)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaKho", model.MaKho);
                        command.Parameters.AddWithValue("@TenKho", model.TenKho);
                        command.Parameters.AddWithValue("@Diachi", model.Diachi);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            return View(model);
        }
        public ActionResult Delete(string MaKho)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "DELETE FROM Kho WHERE MaKho = @MaKho";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaKho", MaKho);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true });
                    }
                }
            }

            return Json(new { success = false });
        }
        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids)) // Check if ids is not empty
                {
                    string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        var idArray = ids.Split(',');
                        if (idArray != null && idArray.Any())
                        {
                            foreach (var id in idArray)
                            {
                                // Giả sử MaLSP là kiểu chuỗi trong cơ sở dữ liệu
                                string MaKho = id.Trim(); // Xóa khoảng trắng ở hai đầu nếu có
                                if (!string.IsNullOrEmpty(MaKho))
                                {
                                    string sqlQuery = "DELETE FROM Kho WHERE MaKho LIKE @MaKho";
                                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                                    {
                                        command.Parameters.AddWithValue("@MaKho", MaKho);
                                        command.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "No IDs provided." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for further investigation
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "An error occurred while processing your request." });
            }
        }

        public ActionResult Edit(string MaKho)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            Kho item = new Kho();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM Kho WHERE MaKho = @MaKho";

                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@MaKho", MaKho);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item.MaKho = MaKho;
                        item.TenKho = reader["TenKho"].ToString();
                        item.Diachi = reader["Diachi"].ToString();
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Kho model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = "UPDATE Kho SET TenKho = @TenKho, Diachi = @Diachi WHERE MaKho = @MaKho";

                    SqlCommand command = new SqlCommand(sqlQuery, connection);
                    command.Parameters.AddWithValue("@MaKho", model.MaKho);
                    command.Parameters.AddWithValue("@TenKho", model.TenKho);
                    command.Parameters.AddWithValue("@Diachi", model.Diachi);

                    command.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }
    }
}