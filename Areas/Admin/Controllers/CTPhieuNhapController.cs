using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class CTPhieuNhapController : Controller
    {
        // GET: Admin/CTPhieuNhap
        public ActionResult Index()
        {
            List<CTPhieuNhap> danhMucs = new List<CTPhieuNhap>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlQuery = "SELECT * FROM CTPhieuNhap";

                SqlCommand command = new SqlCommand(sqlQuery, connection);

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CTPhieuNhap danhMuc = new CTPhieuNhap();
                        danhMuc.MaPN = reader["MaPN"].ToString();
                        danhMuc.MaSP = reader["MaSP"].ToString();
                        danhMuc.SoLuong = Convert.ToInt32(reader["SoLuong"]);
                        // Thêm các trường dữ liệu khác tương ứng

                        danhMucs.Add(danhMuc);
                    }
                }
            }

            return View(danhMucs);
        }
    }
}