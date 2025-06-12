using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webbanhangonline.Models.EF;
using Webbanhangonline.Models;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class LoaiSPController : Controller
    {
        // GET: Admin/LoaiSP
        public ActionResult Index()
        {
            List<LoaiSanPham> LoaiSanPhams = new List<LoaiSanPham>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM LoaiSanPham";

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LoaiSanPham LoaiSanPham = new LoaiSanPham();
                        LoaiSanPham.MaLSP = reader["MaLSP"].ToString();
                        LoaiSanPham.TenLSP = reader["TenLSP"].ToString();
                        LoaiSanPham.BieuTuong = reader["BieuTuong"].ToString();
                        // Thêm các trường dữ liệu khác tương ứng

                        LoaiSanPhams.Add(LoaiSanPham);
                    }
                }
            }
            return View(LoaiSanPhams);
        }
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Add(LoaiSanPham model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Open connection before executing command

                    string query = "INSERT INTO LoaiSanPham (MaLSP, TenLSP, MoTa, BieuTuong, Alias, GhiChu) VALUES (@MaLSP, @TenLSP, @MoTa, @BieuTuong, @Alias, NULL)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaLSP", model.MaLSP);
                        command.Parameters.AddWithValue("@TenLSP", model.TenLSP);
                        command.Parameters.AddWithValue("@MoTa", model.MoTa);
                        command.Parameters.AddWithValue("@BieuTuong", model.BieuTuong);
                        model.Alias = Webbanhangonline.Models.Commons.Filter.FilterChar(model.Alias);
                        command.Parameters.AddWithValue("@Alias", model.Alias);
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            return View(model);
        }
        public ActionResult Edit(string MaLSP)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            LoaiSanPham item = new LoaiSanPham();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM LoaiSanPham WHERE MaLSP = @MaLSP";

                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@MaLSP", MaLSP);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item.MaLSP = MaLSP;
                        item.TenLSP = reader["TenLSP"].ToString();
                        item.MoTa = reader["MoTa"].ToString();
                        item.BieuTuong = reader["BieuTuong"].ToString();
                        item.Alias = reader["Alias"].ToString();
                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
            }

            return View(item);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LoaiSanPham model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = "UPDATE LoaiSanPham SET TenLSP = @TenLSP, MoTa = @MoTa, BieuTuong = @BieuTuong, Alias = @Alias WHERE MaLSP = @MaLSP";

                    SqlCommand command = new SqlCommand(sqlQuery, connection);
                    command.Parameters.AddWithValue("@MaLSP", model.MaLSP);
                    command.Parameters.AddWithValue("@TenLSP", model.TenLSP);
                    command.Parameters.AddWithValue("@MoTa", model.MoTa);
                    command.Parameters.AddWithValue("@BieuTuong", model.BieuTuong);
                    command.Parameters.AddWithValue("@Alias", model.Alias);

                    command.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }
        public ActionResult Delete(string MaLSP)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "DELETE FROM LoaiSanPham WHERE MaLSP = @MaLSP";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaLSP", MaLSP);

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
                                string MaLSP = id.Trim(); // Xóa khoảng trắng ở hai đầu nếu có
                                if (!string.IsNullOrEmpty(MaLSP))
                                {
                                    string sqlQuery = "DELETE FROM LoaiSanPham WHERE MaLSP LIKE @MaLSP";
                                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                                    {
                                        command.Parameters.AddWithValue("@MaLSP", MaLSP);
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

    }
}