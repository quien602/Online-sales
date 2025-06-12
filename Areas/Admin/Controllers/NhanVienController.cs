using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class NhanVienController : Controller
    {
        // GET: Admin/NhanVien
        public ActionResult Index(int? page)
        {
            List<NhanVien> items = new List<NhanVien>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            // Define pagination parameters
            int pageSize = 10;
            int pageIndex = page ?? 1; // Ensure that page is properly initialized

            // Calculate starting row index for pagination
            int startRowIndex = (pageIndex - 1) * pageSize + 1;
            int endRowIndex = pageIndex * pageSize;

            // Construct the SQL query with pagination
            string sqlQuery = $@"
    SELECT * FROM (
        SELECT *, ROW_NUMBER() OVER (ORDER BY MaNV DESC) AS RowNumber
        FROM NhanVien
    ) AS RowConstrainedResult
    WHERE RowNumber BETWEEN {startRowIndex} AND {endRowIndex}";

            // Execute the query
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Map data from SqlDataReader to NhanVien object
                            NhanVien NhanVien = new NhanVien();
                            NhanVien.MaNV = reader.GetString(reader.GetOrdinal("MaNV"));
                            NhanVien.HoTen = reader["HoTen"].ToString();
                            NhanVien.CCCD = reader["CCCD"].ToString();
                            NhanVien.SoDienThoai = reader["SoDienThoai"].ToString();
                            string hienthiValue = reader["TrangThai"].ToString();
                            if (bool.TryParse(hienthiValue, out bool result))
                            {
                                NhanVien.TrangThai = result;
                            }
                            else
                            {
                                NhanVien.TrangThai = false;
                                Console.WriteLine($"Invalid value for TrangThai: {hienthiValue}");
                            }
                            // Handle Tongtien
                            DateTime NgaySinh;
                            if (DateTime.TryParse(reader["NgaySinh"].ToString(), out NgaySinh))
                            {
                                NhanVien.NgaySinh = NgaySinh;
                            }

                            items.Add(NhanVien);
                        }
                    }
                }
            }

            // Pass the items to the view along with pagination information
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;
            return View(items);
        }
        [HttpPost]
        public ActionResult HienThi(string MaNV)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool isActive = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current HienThi value from the database
                string selectQuery = "SELECT TrangThai FROM NhanVien WHERE MaNV = @MaNV";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaNV", MaNV);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        isActive = (bool)result;
                    }
                }

                // Update the news item in the database
                string updateQuery = "UPDATE NhanVien SET TrangThai = @TrangThai WHERE MaNV = @MaNV";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaNV", MaNV);
                    updateCommand.Parameters.AddWithValue("@TrangThai", !isActive); // Toggle the value
                    updateCommand.ExecuteNonQuery();
                }
            }

            return Json(new { success = true, HienThi = !isActive });
        }
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(NhanVien model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Open connection before executing command
                    string cmd = "INSERT INTO TaiKhoan (MaTaiKhoan, TenDangNhap, MatKhau, HoTen, SoDienThoai, Email) " +
                                   "VALUES (@MaTaiKhoan, @TenDangNhap, @MatKhau, @HoTen, @SoDienThoai, NULL)";
                    string matk = "TK" + GenerateRandomNumberTK();

                    using (SqlCommand command = new SqlCommand(cmd, connection))
                    {
                        command.Parameters.AddWithValue("@MaTaiKhoan", matk);
                        command.Parameters.AddWithValue("@Hoten", model.HoTen);
                        command.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);
                        command.Parameters.AddWithValue("@TenDangNhap", model.TenDangNhap);
                        command.Parameters.AddWithValue("@MatKhau", model.MatKhau);


                        command.ExecuteNonQuery();
                    }
                    string ctQuyenQuery = "INSERT INTO CTQuyen (MaTaiKhoan, MaQuyen) VALUES (@MaTaiKhoan, @MaQuyen)";
                    using (SqlCommand ctQuyenCommand = new SqlCommand(ctQuyenQuery, connection))
                    {
                        ctQuyenCommand.Parameters.AddWithValue("@MaTaiKhoan", matk);
                        ctQuyenCommand.Parameters.AddWithValue("@MaQuyen", "Q003");

                        ctQuyenCommand.ExecuteNonQuery();
                    }
                    string query = "INSERT INTO NhanVien (MaNV, Hoten, CCCD, NgaySinh, DiaChi, SoDienThoai, MaTaiKhoan, MaNQL, TrangThai) VALUES (@MaNV, @Hoten, @CCCD, NULL, @SoDienThoai, @DiaChi, @MaTaiKhoan, NULL, 1)";
                    string manv = "NV" + GenerateRandomNumber();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaNV", manv);
                        command.Parameters.AddWithValue("@Hoten", model.HoTen);
                        command.Parameters.AddWithValue("@CCCD", model.CCCD);
                        command.Parameters.AddWithValue("@DiaChi", model.DiaChi);
                        command.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);
                        command.Parameters.AddWithValue("@MaTaiKhoan", matk);
                        command.Parameters.Add("@NgaySinh", SqlDbType.Date).Value = model.NgaySinh.Date;


                        command.ExecuteNonQuery();
                    }
                    


                }

                return RedirectToAction("Index");
            }
            return View(model);
        }
        private string GenerateRandomNumber()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }
        private string GenerateRandomNumberTK()
        {
            Random random = new Random();
            return random.Next(1000, 9999).ToString();
        }

        public ActionResult Delete(string MaNV)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Xóa bản ghi từ bảng NhanVien trước
                    string deleteNhanVienQuery = "DELETE FROM NhanVien WHERE MaNV = @MaNV";
                    using (SqlCommand nhanVienCommand = new SqlCommand(deleteNhanVienQuery, connection, transaction))
                    {
                        nhanVienCommand.Parameters.AddWithValue("@MaNV", MaNV);
                        int rowsAffected = nhanVienCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Xóa bản ghi từ bảng TaiKhoan
                            string deleteTaiKhoanQuery = "DELETE FROM TaiKhoan WHERE MaTaiKhoan = (SELECT MaTaiKhoan FROM NhanVien WHERE MaNV = @MaNV)";
                            using (SqlCommand taiKhoanCommand = new SqlCommand(deleteTaiKhoanQuery, connection, transaction))
                            {
                                taiKhoanCommand.Parameters.AddWithValue("@MaNV", MaNV);
                                taiKhoanCommand.ExecuteNonQuery();
                            }

                            // Xóa bản ghi từ bảng CTQuyen
                            string deleteCTQuyenQuery = "DELETE FROM CTQuyen WHERE MaTaiKhoan = (SELECT MaTaiKhoan FROM NhanVien WHERE MaNV = @MaNV)";
                            using (SqlCommand ctQuyenCommand = new SqlCommand(deleteCTQuyenQuery, connection, transaction))
                            {
                                ctQuyenCommand.Parameters.AddWithValue("@MaNV", MaNV);
                                ctQuyenCommand.ExecuteNonQuery();
                            }

                            // Commit transaction if all operations succeed
                            transaction.Commit();
                            return Json(new { success = true });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Rollback transaction if an exception occurs
                    transaction.Rollback();
                    return Json(new { success = false, message = ex.Message });
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
                                // Giả sử MaBai là kiểu chuỗi trong cơ sở dữ liệu
                                string MaNV = id.Trim(); // Xóa khoảng trắng ở hai đầu nếu có
                                if (!string.IsNullOrEmpty(MaNV))
                                {
                                    string sqlQuery = "DELETE FROM NhanVien WHERE MaNV LIKE @MaNV";
                                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                                    {
                                        command.Parameters.AddWithValue("@MaNV", MaNV);
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
        // GET: NhanVien/Edit/5
        public ActionResult Edit(string MaNV)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            NhanVien model = new NhanVien();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT NV.*, TK.TenDangNhap, TK.MatKhau " +
                               "FROM NhanVien NV " +
                               "JOIN TaiKhoan TK ON NV.MaTaiKhoan = TK.MaTaiKhoan " +
                               "WHERE NV.MaNV = @MaNV";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaNV", MaNV);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.MaNV = reader["MaNV"].ToString();
                            model.HoTen = reader["HoTen"].ToString();
                            model.CCCD = reader["CCCD"].ToString();
                            model.NgaySinh = reader.GetDateTime(reader.GetOrdinal("NgaySinh"));
                            model.DiaChi = reader["DiaChi"].ToString();
                            model.SoDienThoai = reader["SoDienThoai"].ToString();
                            model.MaTaiKhoan = reader["MaTaiKhoan"].ToString();
                            model.TenDangNhap = reader["TenDangNhap"].ToString();
                            model.MatKhau = reader["MatKhau"].ToString();
                        }
                    }
                }
            }

            return View(model);
        }
        // POST: NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NhanVien model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string query = "UPDATE NhanVien " +
                                           "SET HoTen = @HoTen, CCCD = @CCCD, NgaySinh = @NgaySinh, DiaChi = @DiaChi, SoDienThoai = @SoDienThoai " +
                                           "WHERE MaNV = @MaNV";

                            using (SqlCommand command = new SqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@MaNV", model.MaNV);
                                command.Parameters.AddWithValue("@HoTen", model.HoTen);
                                command.Parameters.AddWithValue("@CCCD", model.CCCD);
                                command.Parameters.Add("@NgaySinh", SqlDbType.Date).Value = model.NgaySinh.Date;
                                command.Parameters.AddWithValue("@DiaChi", model.DiaChi);
                                command.Parameters.AddWithValue("@SoDienThoai", model.SoDienThoai);

                                command.ExecuteNonQuery();
                            }

                            string updateTaiKhoanQuery = "UPDATE TaiKhoan " +
                                                         "SET TenDangNhap = @TenDangNhap, MatKhau = @MatKhau " +
                                                         "WHERE MaTaiKhoan = @MaTaiKhoan";

                            using (SqlCommand command = new SqlCommand(updateTaiKhoanQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@MaTaiKhoan", model.MaTaiKhoan);
                                command.Parameters.AddWithValue("@TenDangNhap", model.TenDangNhap);
                                command.Parameters.AddWithValue("@MatKhau", model.MatKhau);

                                command.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            return RedirectToAction("Index");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            // Handle the exception (e.g., log the error)
                            ViewBag.Error = "An error occurred while updating the record: " + ex.Message;
                        }
                    }
                }
            }

            return View(model);
        }


    }
}