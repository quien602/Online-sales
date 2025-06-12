using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Webbanhangonline.Areas.Admin.Controllers
{

    public class HomeAdminController : Controller
    {

        // GET: Admin/HomeAdmin
        private string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

        // GET: Admin/HomeAdmin
        public ActionResult Index()
        {
            ViewBag.TotalRevenue = GetTotalRevenueFormatted();
            ViewBag.TotalProfit = GetTotalProfitFormatted();
            ViewBag.TotalOrdersCount = GetTotalOrdersCount();
            ViewBag.TotalProductsCount = GetTotalProductsCount();
            ViewBag.TotalCategoryCount = GetTotalCategory();

            ViewBag.OrderCountJanuary = GetOrdersCountForMonth(1);
            ViewBag.OrderCountFebruary = GetOrdersCountForMonth(2);
            ViewBag.OrderCountMarch = GetOrdersCountForMonth(3);
            ViewBag.OrderCountApril = GetOrdersCountForMonth(4);
            ViewBag.OrderCountMay = GetOrdersCountForMonth(5);
            ViewBag.OrderCountJune = GetOrdersCountForMonth(6);
            ViewBag.OrderCountJuly = GetOrdersCountForMonth(7);
            ViewBag.OrderCountAugust = GetOrdersCountForMonth(8);
            ViewBag.OrderCountSeptember = GetOrdersCountForMonth(9);
            ViewBag.OrderCountOctober = GetOrdersCountForMonth(10);
            ViewBag.OrderCountNovember = GetOrdersCountForMonth(11);
            ViewBag.OrderCountDecember = GetOrdersCountForMonth(12);

            ViewBag.CategoryCounts = GetCategoryCounts();

            ViewBag.TopSellingProducts = GetTop3BestSellingProducts();

            HttpCookie cookie = Request.Cookies["User"];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
            {
                return RedirectToAction("Index", "DangNhap");
            }
            var username = Request.Cookies["User"]?.Value;
            // Lấy thông tin người dùng từ cookie
            ViewBag.Username = cookie.Value;
            return View();
        }
        private string GetUsernameForCurrentUser()
        {
            string username = "";
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    string currentUserId = User.Identity.Name;

                    // Query database to get username for current user
                    string query = "SELECT TenDangNhap FROM TaiKhoan WHERE TenDangNhap = @Username";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Username", currentUserId);

                        connection.Open();
                        username = (string)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle or log exception
                Console.WriteLine("Error fetching username: " + ex.Message);
            }

            return username;
        }

        private string GetTotalRevenueFormatted()
        {
            decimal totalRevenue = 0;
            string query = "SELECT SUM(ThanhTien) FROM HoaDon";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    totalRevenue = Convert.ToDecimal(result);
                }
            }

            return totalRevenue.ToString("#,##0");
        }

        private string GetTotalProfitFormatted()
        {
            decimal totalProfit = 0;
            string query = @"
                SELECT SUM(od.SoLuong * GiaBan) AS TotalSell, SUM(od.SoLuong * GiaGoc) AS TotalBuy
                FROM HoaDon o
                JOIN CTHOADON od ON o.MaHD = od.MaHD
                JOIN SanPham p ON od.MaSP = p.MaSP
                GROUP BY CAST(o.NgayTao AS date)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    decimal totalSell = Convert.ToDecimal(reader["TotalSell"]);
                    decimal totalBuy = Convert.ToDecimal(reader["TotalBuy"]);
                    totalProfit += (totalSell - totalBuy);
                }
                reader.Close();
            }

            return totalProfit.ToString("#,##0");
        }

        private int GetTotalOrdersCount()
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM HoaDon";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    count = (int)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception
                Console.WriteLine("Error fetching total orders count: " + ex.Message);
                // Optionally rethrow the exception or return a default value
            }

            return count;
        }


        private int GetTotalProductsCount()
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM SanPham";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                count = (int)command.ExecuteScalar();
            }

            return count;
        }

        private int GetTotalCategory()
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM LoaiSanPham";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                count = (int)command.ExecuteScalar();
            }

            return count;
        }

        private Dictionary<string, int> GetCategoryCounts()
        {
            Dictionary<string, int> categoryCounts = new Dictionary<string, int>();

            string query = "SELECT pc.TenLSP, COUNT(p.MaSP) AS ProductCount " +
                           "FROM LoaiSanPham pc " +
                           "LEFT JOIN SanPham p ON pc.MaLSP = p.MaLSP " +
                           "GROUP BY pc.TenLSP";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string categoryName = reader["TenLSP"].ToString();
                    int productCount = Convert.ToInt32(reader["ProductCount"]);
                    categoryCounts.Add(categoryName, productCount);
                }
                reader.Close();
            }

            return categoryCounts;
        }

        private List<(string Title, int SoldQuantity)> GetTop3BestSellingProducts()
        {
            List<(string Title, int SoldQuantity)> topSellingProducts = new List<(string Title, int SoldQuantity)>();

            string query = @"
                SELECT TOP 3 p.TenSP, SUM(od.SoLuong) AS SoldQuantity
                FROM CTHOADON od
                JOIN SanPham p ON od.MaSP = p.MaSP
                GROUP BY p.TenSP
                ORDER BY SoldQuantity DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    string productName = reader["TenSP"].ToString();
                    int soldQuantity = Convert.ToInt32(reader["SoldQuantity"]);
                    topSellingProducts.Add((productName, soldQuantity));
                }
                reader.Close();
            }

            return topSellingProducts;
        }

        private int GetOrdersCountForMonth(int month)
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM HoaDon WHERE MONTH(NgayTao) = @Month";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Month", month);
                connection.Open();
                count = (int)command.ExecuteScalar();
            }

            return count;
        }
    }
}