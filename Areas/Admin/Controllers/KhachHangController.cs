using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class KhachHangController : Controller
    {
        // GET: Admin/KhachHang
        public ActionResult Index(int? page)
        {
            List<KhachHang> items = new List<KhachHang>();

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
        SELECT *, ROW_NUMBER() OVER (ORDER BY MaKH DESC) AS RowNumber
        FROM KhachHang
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
                            // Map data from SqlDataReader to KhachHang object
                            KhachHang KhachHang = new KhachHang();
                            KhachHang.MaKH = reader.GetString(reader.GetOrdinal("MaKH"));
                            KhachHang.HoTen = reader["HoTen"].ToString();
                            KhachHang.CCCD = reader["CCCD"].ToString();
                            KhachHang.SoDienThoai = reader["SoDienThoai"].ToString();
                            string hienthiValue = reader["TrangThai"].ToString();
                            if (bool.TryParse(hienthiValue, out bool result))
                            {
                                KhachHang.TrangThai = result;
                            }
                            else
                            {
                                KhachHang.TrangThai = false;
                                Console.WriteLine($"Invalid value for TrangThai: {hienthiValue}");
                            }
                            // Handle Tongtien
                            DateTime NgaySinh;
                            if (DateTime.TryParse(reader["NgaySinh"].ToString(), out NgaySinh))
                            {
                                KhachHang.NgaySinh = NgaySinh;
                            }

                            items.Add(KhachHang);
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
        public ActionResult HienThi(string MaKH)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool isActive = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current HienThi value from the database
                string selectQuery = "SELECT TrangThai FROM KhachHang WHERE MaKH = @MaKH";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaKH", MaKH);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        isActive = (bool)result;
                    }
                }

                // Update the news item in the database
                string updateQuery = "UPDATE KhachHang SET TrangThai = @TrangThai WHERE MaKH = @MaKH";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaKH", MaKH);
                    updateCommand.Parameters.AddWithValue("@TrangThai", !isActive); // Toggle the value
                    updateCommand.ExecuteNonQuery();
                }
            }

            return Json(new { success = true, HienThi = !isActive });
        }
        public ActionResult Delete(string MaKH)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Xóa bản ghi từ bảng KhachHang trước
                    string deleteKhachHangQuery = "DELETE FROM KhachHang WHERE MaKH = @MaKH";
                    using (SqlCommand KhachHangCommand = new SqlCommand(deleteKhachHangQuery, connection, transaction))
                    {
                        KhachHangCommand.Parameters.AddWithValue("@MaKH", MaKH);
                        int rowsAffected = KhachHangCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Xóa bản ghi từ bảng TaiKhoan
                            string deleteTaiKhoanQuery = "DELETE FROM TaiKhoan WHERE MaTaiKhoan = (SELECT MaTaiKhoan FROM KhachHang WHERE MaKH = @MaKH)";
                            using (SqlCommand taiKhoanCommand = new SqlCommand(deleteTaiKhoanQuery, connection, transaction))
                            {
                                taiKhoanCommand.Parameters.AddWithValue("@MaKH", MaKH);
                                taiKhoanCommand.ExecuteNonQuery();
                            }

                            // Xóa bản ghi từ bảng CTQuyen
                            string deleteCTQuyenQuery = "DELETE FROM CTQuyen WHERE MaTaiKhoan = (SELECT MaTaiKhoan FROM KhachHang WHERE MaKH = @MaKH)";
                            using (SqlCommand ctQuyenCommand = new SqlCommand(deleteCTQuyenQuery, connection, transaction))
                            {
                                ctQuyenCommand.Parameters.AddWithValue("@MaKH", MaKH);
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
        public ActionResult Details(string maKH)
        {
            if (string.IsNullOrEmpty(maKH))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            KhachHang khachHang = GetKhachHangById(maKH); // Hàm GetKhachHangById để lấy thông tin khách hàng từ cơ sở dữ liệu

            if (khachHang == null)
            {
                return HttpNotFound();
            }

            return View(khachHang);
        }
        private KhachHang GetKhachHangById(string maKH)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            KhachHang khachHang = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM KhachHang WHERE MaKH = @MaKH";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaKH", maKH);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            khachHang = new KhachHang
                            {
                                MaKH = !reader.IsDBNull(reader.GetOrdinal("MaKH")) ? reader.GetString(reader.GetOrdinal("MaKH")) : string.Empty,
                                HoTen = !reader.IsDBNull(reader.GetOrdinal("HoTen")) ? reader.GetString(reader.GetOrdinal("HoTen")) : string.Empty,
                                CCCD = !reader.IsDBNull(reader.GetOrdinal("CCCD")) ? reader.GetString(reader.GetOrdinal("CCCD")) : string.Empty,
                                SoDienThoai = !reader.IsDBNull(reader.GetOrdinal("SoDienThoai")) ? reader.GetString(reader.GetOrdinal("SoDienThoai")) : string.Empty,
                                NgaySinh = !reader.IsDBNull(reader.GetOrdinal("NgaySinh")) ? reader.GetDateTime(reader.GetOrdinal("NgaySinh")) : DateTime.MinValue,
                                TrangThai = !reader.IsDBNull(reader.GetOrdinal("TrangThai")) && reader.GetBoolean(reader.GetOrdinal("TrangThai")),

                            };
                        }
                    }
                }
            }

            return khachHang;
        }

    }
}