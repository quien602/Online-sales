using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class TaiKhoanController : Controller
    {
        // GET: Admin/TaiKhoan
        private string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

        // Method to get all user accounts
        public ActionResult Index(int? page)
        {
            List<TaiKhoan> taiKhoans = new List<TaiKhoan>();
            int pageSize = 10;
            int pageIndex = page ?? 1; // Ensure that page is properly initialized

            // Calculate starting row index for pagination
            int startRowIndex = (pageIndex - 1) * pageSize;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT * FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY MaTaiKhoan) AS RowNum, * 
                FROM TaiKhoan
            ) AS RowConstrainedResult
            WHERE RowNum > @StartRowIndex AND RowNum <= @EndRowIndex
            ORDER BY RowNum";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartRowIndex", startRowIndex);
                    command.Parameters.AddWithValue("@EndRowIndex", startRowIndex + pageSize);

                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TaiKhoan taiKhoan = new TaiKhoan
                            {
                                MaTaiKhoan = reader["MaTaiKhoan"].ToString(),
                                TenDangNhap = reader["TenDangNhap"].ToString(),
                                MatKhau = reader["MatKhau"].ToString(),
                                HoTen = reader["HoTen"].ToString(),
                                SoDienThoai = reader["SoDienThoai"].ToString(),
                                Email = reader["Email"].ToString()
                            };
                            taiKhoans.Add(taiKhoan);
                        }
                    }
                }
            }

            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;
            return View(taiKhoans);
        }


        // Method to create a new user account
        public ActionResult Create()
        {
            ViewBag.MaQuyen = new SelectList(GetQuyenList(), "MaQuyen", "TenQuyen"); // Populate the dropdown with roles
            return View();
        }

        // POST: TaiKhoan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TaiKhoan taiKhoan, string selectedMaQuyen)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Insert into TaiKhoan
                            string query = "INSERT INTO TaiKhoan (MaTaiKhoan, TenDangNhap, MatKhau, HoTen, SoDienThoai, Email) " +
                                           "VALUES (@MaTaiKhoan, @TenDangNhap, @MatKhau, @HoTen, @SoDienThoai, @Email)";
                            string maTaiKhoan = "TK" + GenerateRandomNumber();

                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                                command.Parameters.AddWithValue("@TenDangNhap", taiKhoan.TenDangNhap);
                                command.Parameters.AddWithValue("@MatKhau", taiKhoan.MatKhau);
                                command.Parameters.AddWithValue("@HoTen", taiKhoan.HoTen);
                                command.Parameters.AddWithValue("@SoDienThoai", taiKhoan.SoDienThoai);
                                command.Parameters.AddWithValue("@Email", taiKhoan.Email);

                                command.ExecuteNonQuery();
                            }

                            // Insert into CTQuyen
                            string ctQuyenQuery = "INSERT INTO CTQuyen (MaTaiKhoan, MaQuyen) VALUES (@MaTaiKhoan, @MaQuyen)";
                            using (SqlCommand ctQuyenCommand = new SqlCommand(ctQuyenQuery, connection, transaction))
                            {
                                ctQuyenCommand.Parameters.AddWithValue("@MaTaiKhoan", maTaiKhoan);
                                ctQuyenCommand.Parameters.AddWithValue("@MaQuyen", selectedMaQuyen);

                                ctQuyenCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            ViewBag.Message = "Account and permissions created successfully.";
                            return RedirectToAction("Index");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ViewBag.Error = "Failed to create account. Error: " + ex.Message;
                        }
                    }
                }
            }
            ViewBag.MaQuyen = new SelectList(GetQuyenList(), "MaQuyen", "TenQuyen"); // Repopulate the dropdown if creation failed
            return View(taiKhoan);
        }

        private List<Quyen> GetQuyenList()
        {
            List<Quyen> list = new List<Quyen>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT MaQuyen, TenQuyen FROM Quyen";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(new Quyen
                        {
                            MaQuyen = reader["MaQuyen"].ToString(),
                            TenQuyen = reader["TenQuyen"].ToString()
                        });
                    }
                }
            }
            return list;
        }


        private string GenerateRandomNumber()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        //Edit
        public ActionResult Edit(string MaTaiKhoan)
        {
            if (string.IsNullOrEmpty(MaTaiKhoan))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TaiKhoan taiKhoan = null;
            string selectedMaQuyen = "";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT tk.MaTaiKhoan, tk.TenDangNhap, tk.MatKhau, tk.HoTen, tk.SoDienThoai, tk.Email, ctq.MaQuyen " +
                               "FROM TaiKhoan tk " +
                               "LEFT JOIN CTQuyen ctq ON tk.MaTaiKhoan = ctq.MaTaiKhoan " +
                               "WHERE tk.MaTaiKhoan = @MaTaiKhoan";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaTaiKhoan", MaTaiKhoan);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            taiKhoan = new TaiKhoan
                            {
                                MaTaiKhoan = reader["MaTaiKhoan"].ToString(),
                                TenDangNhap = reader["TenDangNhap"].ToString(),
                                MatKhau = reader["MatKhau"].ToString(),
                                HoTen = reader["HoTen"].ToString(),
                                SoDienThoai = reader["SoDienThoai"].ToString(),
                                Email = reader["Email"].ToString()
                            };
                            selectedMaQuyen = reader["MaQuyen"].ToString();
                        }
                    }
                }
            }

            if (taiKhoan == null)
            {
                return HttpNotFound();
            }

            ViewBag.MaQuyen = new SelectList(GetQuyenList(), "MaQuyen", "TenQuyen", selectedMaQuyen);
            return View(taiKhoan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TaiKhoan taiKhoan, string selectedMaQuyen)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Update TaiKhoan
                            string query = "UPDATE TaiKhoan SET TenDangNhap = @TenDangNhap, MatKhau = @MatKhau, HoTen = @HoTen, SoDienThoai = @SoDienThoai, Email = @Email " +
                                           "WHERE MaTaiKhoan = @MaTaiKhoan";

                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@MaTaiKhoan", taiKhoan.MaTaiKhoan);
                                command.Parameters.AddWithValue("@TenDangNhap", taiKhoan.TenDangNhap);
                                command.Parameters.AddWithValue("@MatKhau", taiKhoan.MatKhau);
                                command.Parameters.AddWithValue("@HoTen", taiKhoan.HoTen);
                                command.Parameters.AddWithValue("@SoDienThoai", taiKhoan.SoDienThoai);
                                command.Parameters.AddWithValue("@Email", taiKhoan.Email);

                                command.ExecuteNonQuery();
                            }

                            // Update CTQuyen
                            string ctQuyenQuery = "IF EXISTS (SELECT 1 FROM CTQuyen WHERE MaTaiKhoan = @MaTaiKhoan) " +
                                                  "UPDATE CTQuyen SET MaQuyen = @MaQuyen WHERE MaTaiKhoan = @MaTaiKhoan " +
                                                  "ELSE " +
                                                  "INSERT INTO CTQuyen (MaTaiKhoan, MaQuyen) VALUES (@MaTaiKhoan, @MaQuyen)";

                            using (SqlCommand ctQuyenCommand = new SqlCommand(ctQuyenQuery, connection, transaction))
                            {
                                ctQuyenCommand.Parameters.AddWithValue("@MaTaiKhoan", taiKhoan.MaTaiKhoan);
                                ctQuyenCommand.Parameters.AddWithValue("@MaQuyen", selectedMaQuyen);

                                ctQuyenCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            ViewBag.Message = "Account and permissions updated successfully.";
                            return RedirectToAction("Index");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            ViewBag.Error = "Failed to update account. Error: " + ex.Message;
                        }
                    }
                }
            }
            ViewBag.MaQuyen = new SelectList(GetQuyenList(), "MaQuyen", "TenQuyen", selectedMaQuyen); // Repopulate the dropdown if update failed
            return View(taiKhoan);
        }
        public ActionResult Delete(string MaTaiKhoan)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Use a transaction to ensure both deletions happen together
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // First, delete the associated records in the CTQuyen table
                        string deleteCTQuyenQuery = "DELETE FROM CTQuyen WHERE MaTaiKhoan = @MaTaiKhoan";
                        using (SqlCommand command = new SqlCommand(deleteCTQuyenQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@MaTaiKhoan", MaTaiKhoan);
                            command.ExecuteNonQuery();
                        }

                        // Then, delete the record in the TaiKhoan table
                        string deleteTaiKhoanQuery = "DELETE FROM TaiKhoan WHERE MaTaiKhoan = @MaTaiKhoan";
                        using (SqlCommand command = new SqlCommand(deleteTaiKhoanQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@MaTaiKhoan", MaTaiKhoan);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                                return Json(new { success = true });
                            }
                        }

                        transaction.Rollback();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // Log the exception (not shown here)
                    }
                }
            }

            return Json(new { success = false });
        }

    }
}