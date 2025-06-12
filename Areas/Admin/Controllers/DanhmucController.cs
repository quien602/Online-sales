using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webbanhangonline.Models;
using Webbanhangonline.Models.EF;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class DanhmucController : Controller
    {
        // GET: Admin/Danhmuc

        public ActionResult Index()
        {
            List<DANHMUC> danhMucs = new List<DANHMUC>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM DANHMUC";

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DANHMUC danhMuc = new DANHMUC();
                        danhMuc.MaDM = reader["MaDM"].ToString();
                        danhMuc.TenDM = reader["TenDM"].ToString();
                        danhMuc.ViTri = Convert.ToInt32(reader["ViTri"]);
                        // Thêm các trường dữ liệu khác tương ứng

                        danhMucs.Add(danhMuc);
                    }
                }
            }

            return View(danhMucs);
        }
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(DANHMUC model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Open connection before executing command

                    string query = "INSERT INTO DANHMUC (MaDM, TenDM, MieuTa, ViTri, HienThi, Alias) VALUES (@MaDM, @TenDM, @MieuTa, @ViTri, 0, @Alias)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaDM", model.MaDM);
                        command.Parameters.AddWithValue("@TenDM", model.TenDM);
                        command.Parameters.AddWithValue("@MieuTa", model.MieuTa);
                        model.Alias = Webbanhangonline.Models.Commons.Filter.FilterChar(model.TenDM);
                        command.Parameters.AddWithValue("@Alias", model.Alias);
                        command.Parameters.AddWithValue("@ViTri", model.ViTri);

                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(string MaDM)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            DANHMUC item = new DANHMUC();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM DANHMUC WHERE MaDM = @MaDM";

                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@MaDM", MaDM);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item.MaDM = MaDM;
                        item.TenDM = reader["TenDM"].ToString();
                        item.MieuTa = reader["MieuTa"].ToString();
                        item.ViTri = Convert.ToInt32(reader["ViTri"]);
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
        public ActionResult Edit(DANHMUC model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = "UPDATE DANHMUC SET TenDM = @TenDM, MieuTa = @MieuTa, ViTri = @ViTri, Alias = @Alias WHERE MaDM = @MaDM";

                    SqlCommand command = new SqlCommand(sqlQuery, connection);
                    command.Parameters.AddWithValue("@MaDM", model.MaDM);
                    command.Parameters.AddWithValue("@TenDM", model.TenDM);
                    command.Parameters.AddWithValue("@MieuTa", model.MieuTa);
                    model.Alias = Webbanhangonline.Models.Commons.Filter.FilterChar(model.TenDM);
                    command.Parameters.AddWithValue("@Alias", model.Alias);
                    command.Parameters.AddWithValue("@ViTri", model.ViTri);

                    command.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }
        public ActionResult Delete(string MaDM)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "DELETE FROM DANHMUC WHERE MaDM = @MaDM";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaDM", MaDM);

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