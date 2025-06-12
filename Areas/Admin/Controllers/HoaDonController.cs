using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class HoaDonController : Controller
    {
        // GET: Admin/HoaDon
        private string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

        // GET: Admin/HoaDon
        public ActionResult Index(int? page)
        {
            return View(LayDanhSachHoaDon(page));
        }

        public ActionResult LocTheoThang(int? page, int? month, int? year)
        {
            return View("Index", LayDanhSachHoaDon(page, month, year));
        }

        public ActionResult TimKiemTheoMa(int? page, string searchMaHD)
        {
            return View("Index", LayDanhSachHoaDon(page, searchMaHD: searchMaHD));
        }

        private List<HoaDon> LayDanhSachHoaDon(int? page, int? month = null, int? year = null, string searchMaHD = null)
        {
            List<HoaDon> items = new List<HoaDon>();

            // Define pagination parameters
            int pageSize = 10;
            int pageIndex = page ?? 1; // Ensure that page is properly initialized

            // Calculate starting row index for pagination
            int startRowIndex = (pageIndex - 1) * pageSize + 1;
            int endRowIndex = pageIndex * pageSize;

            // Construct the SQL query with pagination and filtering
            string sqlQuery = $@"
        SELECT * FROM (
            SELECT *, ROW_NUMBER() OVER (ORDER BY NgayTao DESC) AS RowNumber
            FROM HoaDon
            WHERE 1 = 1";

            if (!string.IsNullOrEmpty(searchMaHD))
            {
                sqlQuery += $" AND MaHD LIKE '%{searchMaHD}%'";
            }

            if (month.HasValue && year.HasValue)
            {
                sqlQuery += $" AND MONTH(NgayTao) = {month.Value} AND YEAR(NgayTao) = {year.Value}";
            }

            sqlQuery += $@"
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
                            // Map data from SqlDataReader to HoaDon object
                            HoaDon hoaDon = new HoaDon
                            {
                                MaHD = reader["MaHD"].ToString(),
                                TenKhachHang = reader["TenKhachHang"].ToString(),
                                HinhThucTT = Convert.ToInt32(reader["HinhThucTT"]),
                                SoDienThoai = reader["SoDienThoai"].ToString(),
                                ThanhTien = reader.IsDBNull(reader.GetOrdinal("ThanhTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ThanhTien")),
                                NgayTao = reader.IsDBNull(reader.GetOrdinal("NgayTao")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("NgayTao"))
                            };
                            items.Add(hoaDon);
                        }
                    }
                }
            }

            // Pass the items to the view along with pagination information
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;
            return items;
        }

        [HttpPost]
        public ActionResult UpdateTT(string id, int trangthai)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE HoaDon SET HinhThucTT = @HinhThucTT WHERE MaHD = @MaHD";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@HinhThucTT", trangthai);
                        command.Parameters.AddWithValue("@MaHD", id);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new { message = "Success", Success = true });
                        }
                        else
                        {
                            return Json(new { message = "No rows affected", Success = false });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = "Error: " + ex.Message, Success = false });
            }
        }

        public ActionResult ViewOrder(string MaHD)
        {
            HoaDon item = GetOrderById(MaHD);
            return View(item);
        }

        public ActionResult Partial_SanPhamCTHD(string MaHD)
        {
            List<CTHOADON> items = GetOrderDetailsByOrderId(MaHD);
            return PartialView(items);
        }

        // Helper method to retrieve order by MaHD using ADO.NET
        private HoaDon GetOrderById(string MaHD)
        {
            HoaDon order = null;
            string query = "SELECT * FROM HoaDon WHERE MaHD = @MaHD";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaHD", MaHD);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = new HoaDon
                            {
                                MaHD = reader["MaHD"].ToString(),
                                TenKhachHang = reader["TenKhachHang"].ToString(),
                                SoDienThoai = reader["SoDienThoai"].ToString(),
                                DiaChi = reader["DiaChi"].ToString(),
                                ThanhTien = reader.IsDBNull(reader.GetOrdinal("ThanhTien")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ThanhTien")),
                                NgayTao = reader.IsDBNull(reader.GetOrdinal("NgayTao")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("NgayTao")),
                                Email = reader["Email"].ToString(),
                                HinhThucTT = Convert.ToInt32(reader["HinhThucTT"])
                            };
                        }
                    }
                }
            }

            return order;
        }

        // Helper method to retrieve order details by MaHD using ADO.NET
        private List<CTHOADON> GetOrderDetailsByOrderId(string MaHD)
        {
            List<CTHOADON> orderDetails = new List<CTHOADON>();
            string query = @"
        SELECT ct.MaSP, SP.TenSP, ct.SoLuong, SP.MoTa, ct.Gia 
        FROM CTHOADON ct 
        JOIN SanPham SP ON SP.MaSP = ct.MaSP 
        WHERE ct.MaHD = @MaHD";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@MaHD", MaHD);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CTHOADON orderDetail = new CTHOADON
                            {
                                MaSP = reader["MaSP"].ToString(),
                                TenSP = reader["TenSP"].ToString(),
                                SoLuong = Convert.ToInt32(reader["SoLuong"]),
                                MoTa = reader["MoTa"].ToString(),
                                Gia = Convert.ToDecimal(reader["Gia"])
                            };
                            orderDetails.Add(orderDetail);
                        }
                    }
                }
            }

            return orderDetails;
        }

        public ActionResult PrintInvoice(string MaHD)
        {
            // Fetch the order and customer data from the database
            HoaDon order = GetOrderById(MaHD);
            List<CTHOADON> orderDetails = GetOrderDetailsByOrderId(MaHD);
            decimal taxRate = 0.093m; // 9.3%
            decimal subtotal = order.ThanhTien;
            decimal taxAmount = subtotal * taxRate;
            var model = new InvoiceViewModel
            {
                InvoiceNumber = order.MaHD,
                InvoiceDate = DateTime.Now.ToString("MM/dd/yyyy"),
                CustomerName = order.TenKhachHang,
                CustomerAddress = order.DiaChi,
                CustomerPhone = order.SoDienThoai,
                CustomerEmail = order.Email,
                OrderId = order.MaHD,
                PaymentDueDate = order.NgayTao.ToString("MM/dd/yyyy"),
                AccountNumber = "123456789",
                Items = orderDetails,
                Subtotal = order.ThanhTien,
                Tax = taxAmount, // Format tax as currency string
                Shipping = "Shipping on delivery",
                Total = subtotal + taxAmount // Total = Subtotal + Tax
            };

            return View("PrintInvoice", model); // assuming your view is named InvoicePrint.cshtml
        }


    }
}