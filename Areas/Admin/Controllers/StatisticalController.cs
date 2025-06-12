using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class StatisticalController : Controller
    {
        // GET: Admin/Statistical
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult GetStatistical(string fromDate, string toDate)
        {
            List<object> result = new List<object>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Build SQL query
                string query = @"
           SELECT 
    CONVERT(date, o.NgayTao) AS CreateDate, 
    SUM(od.SoLuong * p.GiaGoc) AS TotalBuy, 
    SUM(od.SoLuong * od.Gia) AS TotalSell
FROM HoaDon o
INNER JOIN CTHOADON od ON o.MaHD = od.MaHD
INNER JOIN SanPham p ON od.MaSP = p.MaSP
WHERE 1=1 ";

                // Add conditions based on fromDate and toDate parameters
                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(fromDate))
                {
                    DateTime startDate = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    query += "AND CONVERT(date, o.NgayTao) >= @StartDate ";
                    parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date) { Value = startDate });
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    query += "AND CONVERT(date, o.NgayTao) <= @EndDate ";
                    parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date) { Value = endDate });
                }

                query += @"
            GROUP BY CONVERT(date, o.NgayTao)
            ORDER BY o.NgayTao";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddRange(parameters.ToArray());

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime date = reader.GetDateTime(0);
                    decimal totalBuy = reader.GetDecimal(1);
                    decimal totalSell = reader.GetDecimal(2);

                    result.Add(new
                    {
                        Date = date,
                        Doanhthu = totalSell,
                        LoiNhuan = totalSell - totalBuy
                    });
                }

                reader.Close();
            }

            // Calculate totalDoanhThu
            decimal totalDoanhThu = result.Sum(x => (decimal)x.GetType().GetProperty("Doanhthu").GetValue(x, null));

            // Returning JSON result
            return Json(new { Date = result, TotalDoanhThu = totalDoanhThu }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetLineChartData(string fromDate, string toDate)
        {
            List<object> result = new List<object>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Build SQL query
                string query = @"
            SELECT 
    CONVERT(date, o.NgayTao) AS CreateDate, 
    SUM(od.SoLuong * p.GiaGoc) AS TotalBuy, 
    SUM(od.SoLuong * od.Gia) AS TotalSell
FROM HoaDon o
INNER JOIN CTHOADON od ON o.MaHD = od.MaHD
INNER JOIN SanPham p ON od.MaSP = p.MaSP
WHERE 1=1 ";

                // Add conditions based on fromDate and toDate parameters
                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(fromDate))
                {
                    DateTime startDate = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    query += "AND CONVERT(date, o.NgayTao) >= @StartDate ";
                    parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date) { Value = startDate });
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    DateTime endDate = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    query += "AND CONVERT(date, o.NgayTao) <= @EndDate ";
                    parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date) { Value = endDate });
                }

                query += @"
            GROUP BY CONVERT(date, o.NgayTao)
            ORDER BY o.NgayTao";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddRange(parameters.ToArray());

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    DateTime date = reader.GetDateTime(0);
                    decimal totalBuy = reader.GetDecimal(1);
                    decimal totalSell = reader.GetDecimal(2);

                    result.Add(new
                    {
                        Label = date.ToString("dd/MM/yyyy"),
                        DoanhThu = totalSell,
                        LoiNhuan = totalSell - totalBuy
                    });
                }

                reader.Close();
            }

            // Returning JSON result
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}