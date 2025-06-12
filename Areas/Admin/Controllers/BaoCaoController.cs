using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OfficeOpenXml;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class BaoCaoController : Controller
    {
        // GET: Admin/BaoCao
        public ActionResult Index()
        {
            var dt = GetInventoryReport();
            var report = new List<InventoryReportViewModel>();

            foreach (DataRow row in dt.Rows)
            {
                var item = new InventoryReportViewModel
                {
                    MaSanPham = row["MaSP"].ToString(),
                    TenSanPham = row["TenSP"].ToString(),
                    NgayNhap = row["NgayNhap"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["NgayNhap"]),
                    NgayXuat = row["NgayXuat"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["NgayXuat"]),
                    SoLuongNhap = row["SoLuongNhap"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoLuongNhap"]),
                    SoLuongXuat = row["SoLuongXuat"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoLuongXuat"])
                };
                report.Add(item);
            }

            // Group by MaSanPham and calculate totals
            var groupedReport = report.GroupBy(r => new { r.MaSanPham, r.TenSanPham })
                                      .Select(g => new GroupedInventoryReportViewModel
                                      {
                                          MaSanPham = g.Key.MaSanPham,
                                          TenSanPham = g.Key.TenSanPham,
                                          Entries = g.ToList(),
                                          TotalSoLuongNhap = g.Sum(x => x.SoLuongNhap),
                                          TotalSoLuongXuat = g.Sum(x => x.SoLuongXuat)
                                      }).ToList();

            return View(groupedReport);
        }


        public DataTable GetInventoryReport()
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            string query = @"
SELECT sp.MaSP, sp.TenSP, pn.NgayNhap, px.NgayXuat, ctpn.SoLuong AS SoLuongNhap, ctpx.SoLuong AS SoLuongXuat, pn.TrangThai AS StatusNhap, px.TrangThai AS StatusXuat
FROM SanPham sp
LEFT JOIN CTPhieuNhap ctpn ON sp.MaSP = ctpn.MaSP
LEFT JOIN PhieuNhap pn ON ctpn.MaPN = pn.MaPN
LEFT JOIN CTPhieuXuat ctpx ON sp.MaSP = ctpx.MaSP
LEFT JOIN PhieuXuat px ON ctpx.MaPX = px.MaPX
ORDER BY sp.MaSP, pn.NgayNhap DESC, px.NgayXuat DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }


        public ActionResult GetInventoryReportTQ()
        {
            List<InventoryReport> inventoryReports = new List<InventoryReport>();
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT 
                sp.MaSP, 
                sp.TenSP, 
                sp.SoLuong AS 'Tồn kho đầu kì', 
                COALESCE(SUM(ctpn.SoLuong), 0) AS 'Nhập kho', 
                COALESCE(SUM(ctpx.SoLuong), 0) AS 'Xuất kho', 
                sp.SoLuongCK AS 'Tồn kho cuối kì', 
                sp.SoLuongCK * sp.GiaGoc AS 'SumofDoanhSoTonKho', 
                COALESCE(SUM(ctpx.SoLuong * ctpx.GiaBan), 0) AS 'SumofDoanhThuBanra',
                COALESCE(SUM(cthd.SoLuong), 0) AS 'CountofProInvoice'
            FROM 
                SanPham sp 
                LEFT JOIN CTPhieuNhap ctpn ON ctpn.MaSP = sp.MaSP 
                LEFT JOIN CTPhieuXuat ctpx ON ctpx.MaSP = sp.MaSP 
                LEFT JOIN CTHoaDon cthd ON cthd.MaSP = sp.MaSP
            GROUP BY 
                sp.MaSP, 
                sp.TenSP, 
                sp.SoLuong, 
                sp.SoLuongCK, 
                sp.GiaGoc;
        ";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    InventoryReport report = new InventoryReport
                    {
                        MaSP = reader["MaSP"].ToString(),
                        TenSP = reader["TenSP"].ToString(),
                        TonKhoDauKi = reader.IsDBNull(reader.GetOrdinal("Tồn kho đầu kì")) ? 0 : reader.GetInt32(reader.GetOrdinal("Tồn kho đầu kì")),
                        NhapKho = reader.IsDBNull(reader.GetOrdinal("Nhập kho")) ? 0 : reader.GetInt32(reader.GetOrdinal("Nhập kho")),
                        XuatKho = reader.IsDBNull(reader.GetOrdinal("Xuất kho")) ? 0 : reader.GetInt32(reader.GetOrdinal("Xuất kho")),
                        TonKhoCuoiKi = reader.IsDBNull(reader.GetOrdinal("Tồn kho cuối kì")) ? 0 : reader.GetInt32(reader.GetOrdinal("Tồn kho cuối kì")),
                        SumofDoanhSoTonKho = reader.IsDBNull(reader.GetOrdinal("SumofDoanhSoTonKho")) ? 0 : reader.GetDecimal(reader.GetOrdinal("SumofDoanhSoTonKho")),
                        SumofDoanhThuBanra = reader.IsDBNull(reader.GetOrdinal("SumofDoanhThuBanra")) ? 0 : reader.GetDecimal(reader.GetOrdinal("SumofDoanhThuBanra")),
                        CountofProInvoice = reader.IsDBNull(reader.GetOrdinal("CountofProInvoice")) ? 0 : reader.GetInt32(reader.GetOrdinal("CountofProInvoice"))
                    };

                    inventoryReports.Add(report);
                }

                reader.Close();
            }

            return View(inventoryReports); // Assuming you have a corresponding view
        }
        [HttpPost]
        public ActionResult XuatExcelBaoCaoXNT()
        {
            var dt = GetInventoryReport();
            var report = new List<InventoryReportViewModel>();

            foreach (DataRow row in dt.Rows)
            {
                var item = new InventoryReportViewModel
                {
                    MaSanPham = row["MaSP"].ToString(),
                    TenSanPham = row["TenSP"].ToString(),
                    NgayNhap = row["NgayNhap"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["NgayNhap"]),
                    NgayXuat = row["NgayXuat"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["NgayXuat"]),
                    SoLuongNhap = row["SoLuongNhap"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoLuongNhap"]),
                    SoLuongXuat = row["SoLuongXuat"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoLuongXuat"])
                };
                report.Add(item);
            }

            var groupedReport = report.GroupBy(r => new { r.MaSanPham, r.TenSanPham })
                                      .Select(g => new GroupedInventoryReportViewModel
                                      {
                                          MaSanPham = g.Key.MaSanPham,
                                          TenSanPham = g.Key.TenSanPham,
                                          Entries = g.ToList(),
                                          TotalSoLuongNhap = g.Sum(x => x.SoLuongNhap),
                                          TotalSoLuongXuat = g.Sum(x => x.SoLuongXuat)
                                      }).ToList();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("BaoCaoXuatNhapTon");

                worksheet.Cells["A1"].Value = "Mã sản phẩm";
                worksheet.Cells["B1"].Value = "Tên sản phẩm";
                worksheet.Cells["C1"].Value = "Ngày nhập";
                worksheet.Cells["D1"].Value = "Ngày xuất";
                worksheet.Cells["E1"].Value = "Số lượng nhập";
                worksheet.Cells["F1"].Value = "Số lượng xuất";

                var row = 2;
                foreach (var group in groupedReport)
                {
                    foreach (var item in group.Entries)
                    {
                        worksheet.Cells[row, 1].Value = item.MaSanPham;
                        worksheet.Cells[row, 2].Value = item.TenSanPham;
                        worksheet.Cells[row, 3].Value = item.NgayNhap?.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 4].Value = item.NgayXuat?.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 5].Value = item.SoLuongNhap;
                        worksheet.Cells[row, 6].Value = item.SoLuongXuat;
                        row++;
                    }
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"BaoCaoXuatNhapTon-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }
            public ActionResult DoPhuSanPham(string filter)
        {
            var model = LayDuLieuBaoCao(filter);
            return View(model);
        }

        // POST: BaoCao/XuatExcel
        [HttpPost]
        public ActionResult XuatExcel(string filter)
        {
            var model = LayDuLieuBaoCao(filter);

            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("BaoCaoDoPhuSanPham");

                worksheet.Cells["A1"].Value = "Mã sản phẩm";
                worksheet.Cells["B1"].Value = "Tên sản phẩm";
                worksheet.Cells["C1"].Value = "Tổng số lượng khách hàng đặt";
                worksheet.Cells["D1"].Value = "Tổng số lượng khách hàng";
                worksheet.Cells["E1"].Value = "Tổng doanh thu";

                var row = 2;
                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = item.MaSP;
                    worksheet.Cells[row, 2].Value = item.TenSP;
                    worksheet.Cells[row, 3].Value = item.TongSoLuongKhachHangDat;
                    worksheet.Cells[row, 4].Value = item.TongSoLuongKhachHang;
                    worksheet.Cells[row, 5].Value = item.TongDoanhThu;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"BaoCaoDoPhuSanPham-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        private List<DoPhuSanPhamViewModel> LayDuLieuBaoCao(string filter)
        {
            var model = new List<DoPhuSanPhamViewModel>();
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        sp.MaSP, 
                        sp.TenSP, 
                        ISNULL(SUM(dhct.SoLuong), 0) AS TongSoLuongKhachHangDat, 
                        COUNT(DISTINCT dh.MaHD) AS TongSoLuongKhachHang,
                        ISNULL(SUM(dhct.SoLuong * dhct.Gia), 0) AS TongDoanhThu
                    FROM 
                        SanPham sp
                    LEFT JOIN 
                        CTHOADON dhct ON sp.MaSP = dhct.MaSP
                    LEFT JOIN 
                        HoaDon dh ON dhct.MaHD = dh.MaHD
                    GROUP BY 
                        sp.MaSP, 
                        sp.TenSP";

                if (filter == "coDon")
                {
                    query += " HAVING COUNT(DISTINCT dh.MaHD) > 0";
                }
                else if (filter == "chuaCoDon")
                {
                    query += " HAVING COUNT(DISTINCT dh.MaHD) = 0";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new DoPhuSanPhamViewModel
                            {
                                MaSP = reader["MaSP"].ToString(),
                                TenSP = reader["TenSP"].ToString(),
                                TongSoLuongKhachHangDat = int.Parse(reader["TongSoLuongKhachHangDat"].ToString()),
                                TongSoLuongKhachHang = int.Parse(reader["TongSoLuongKhachHang"].ToString()),
                                TongDoanhThu = decimal.Parse(reader["TongDoanhThu"].ToString())
                            };
                            model.Add(item);
                        }
                    }
                }
            }

            return model;
        }


        //Test excel 
        [HttpPost]
        public ActionResult XuatExcel1()
        {
            var model = LayDuLieuBaoCaoGT();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("BaoCaoDoPhuSanPham");

                worksheet.Cells["A1"].Value = "Mã sản phẩm";
                worksheet.Cells["B1"].Value = "Tên sản phẩm";
                worksheet.Cells["C1"].Value = "Tồn kho đầu kì";
                worksheet.Cells["D1"].Value = "Nhập kho";
                worksheet.Cells["E1"].Value = "Xuất kho";
                worksheet.Cells["F1"].Value = "Tồn kho cuối kì";
                worksheet.Cells["G1"].Value = "Sum of Doanh So Ton Kho";
                worksheet.Cells["H1"].Value = "Sum of Doanh Thu Ban Ra";
                worksheet.Cells["I1"].Value = "Đếm Số Lượng Sản Phẩm Được Xuất Bán";

                var row = 2;
                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = item.MaSP;
                    worksheet.Cells[row, 2].Value = item.TenSP;
                    worksheet.Cells[row, 3].Value = item.TonKhoDauKi;
                    worksheet.Cells[row, 4].Value = item.NhapKho;
                    worksheet.Cells[row, 5].Value = item.XuatKho;
                    worksheet.Cells[row, 6].Value = item.TonKhoCuoiKi;
                    worksheet.Cells[row, 7].Value = item.SumofDoanhSoTonKho;
                    worksheet.Cells[row, 8].Value = item.SumofDoanhThuBanra;
                    worksheet.Cells[row, 9].Value = item.CountofProInvoice;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"BaoCaoTonKhoSanPham-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        private List<InventoryReport> LayDuLieuBaoCaoGT()
        {
            var model = new List<InventoryReport>();
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                sp.MaSP, 
                sp.TenSP, 
                sp.SoLuong AS 'Tồn kho đầu kì', 
                COALESCE(SUM(ctpn.SoLuong), 0) AS 'Nhập kho', 
                COALESCE(SUM(ctpx.SoLuong), 0) AS 'Xuất kho', 
                sp.SoLuongCK AS 'Tồn kho cuối kì', 
                sp.SoLuongCK * sp.GiaGoc AS 'SumofDoanhSoTonKho', 
                COALESCE(SUM(ctpx.SoLuong * ctpx.GiaBan), 0) AS 'SumofDoanhThuBanra',
                COALESCE(SUM(cthd.SoLuong), 0) AS 'CountofProInvoice'
            FROM 
                SanPham sp 
                LEFT JOIN CTPhieuNhap ctpn ON ctpn.MaSP = sp.MaSP 
                LEFT JOIN CTPhieuXuat ctpx ON ctpx.MaSP = sp.MaSP 
                LEFT JOIN CTHoaDon cthd ON cthd.MaSP = sp.MaSP
            GROUP BY 
                sp.MaSP, 
                sp.TenSP, 
                sp.SoLuong, 
                sp.SoLuongCK, 
                sp.GiaGoc;";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new InventoryReport
                            {
                                MaSP = reader["MaSP"].ToString(),
                                TenSP = reader["TenSP"].ToString(),
                                TonKhoDauKi = reader.IsDBNull(reader.GetOrdinal("Tồn kho đầu kì")) ? 0 : reader.GetInt32(reader.GetOrdinal("Tồn kho đầu kì")),
                                NhapKho = reader.IsDBNull(reader.GetOrdinal("Nhập kho")) ? 0 : reader.GetInt32(reader.GetOrdinal("Nhập kho")),
                                XuatKho = reader.IsDBNull(reader.GetOrdinal("Xuất kho")) ? 0 : reader.GetInt32(reader.GetOrdinal("Xuất kho")),
                                TonKhoCuoiKi = reader.IsDBNull(reader.GetOrdinal("Tồn kho cuối kì")) ? 0 : reader.GetInt32(reader.GetOrdinal("Tồn kho cuối kì")),
                                SumofDoanhSoTonKho = reader.IsDBNull(reader.GetOrdinal("SumofDoanhSoTonKho")) ? 0 : reader.GetDecimal(reader.GetOrdinal("SumofDoanhSoTonKho")),
                                SumofDoanhThuBanra = reader.IsDBNull(reader.GetOrdinal("SumofDoanhThuBanra")) ? 0 : reader.GetDecimal(reader.GetOrdinal("SumofDoanhThuBanra")),
                                CountofProInvoice = reader.IsDBNull(reader.GetOrdinal("CountofProInvoice")) ? 0 : reader.GetInt32(reader.GetOrdinal("CountofProInvoice"))
                            };
                            model.Add(item);
                        }
                    }
                }
            }

            return model;
        }

        //LocBaoCao
        public ActionResult statusFilter(string status)
        {
            // Gọi phương thức GetInventoryReport với tham số status để lấy dữ liệu
            var dt = GetInventoryReport(status);

            // Chuyển đổi dữ liệu từ DataTable sang các đối tượng ViewModel
            var report = new List<InventoryReportViewModel>();

            foreach (DataRow row in dt.Rows)
            {
                var item = new InventoryReportViewModel
                {
                    MaSanPham = row["MaSP"].ToString(),
                    TenSanPham = row["TenSP"].ToString(),
                    NgayNhap = row["NgayNhap"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["NgayNhap"]),
                    NgayXuat = row["NgayXuat"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["NgayXuat"]),
                    SoLuongNhap = row["SoLuongNhap"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoLuongNhap"]),
                    SoLuongXuat = row["SoLuongXuat"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoLuongXuat"])
                };
                report.Add(item);
            }

            // Nhóm theo Mã Sản phẩm và tính tổng
            var groupedReport = report.GroupBy(r => new { r.MaSanPham, r.TenSanPham })
                                      .Select(g => new GroupedInventoryReportViewModel
                                      {
                                          MaSanPham = g.Key.MaSanPham,
                                          TenSanPham = g.Key.TenSanPham,
                                          Entries = g.ToList(),
                                          TotalSoLuongNhap = g.Sum(x => x.SoLuongNhap),
                                          TotalSoLuongXuat = g.Sum(x => x.SoLuongXuat)
                                      }).ToList();

            // Trả về view với dữ liệu đã nhóm và tính tổng
            return View("Index", groupedReport);
        }

        // Phương thức để lấy dữ liệu báo cáo nhập xuất kho từ database
        private DataTable GetInventoryReport(string status)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            string query = @"
            SELECT sp.MaSP, sp.TenSP, pn.NgayNhap, px.NgayXuat, ctpn.SoLuong AS SoLuongNhap, ctpx.SoLuong AS SoLuongXuat, pn.TrangThai AS StatusNhap, px.TrangThai AS StatusXuat
            FROM SanPham sp
            LEFT JOIN CTPhieuNhap ctpn ON sp.MaSP = ctpn.MaSP
            LEFT JOIN PhieuNhap pn ON ctpn.MaPN = pn.MaPN
            LEFT JOIN CTPhieuXuat ctpx ON sp.MaSP = ctpx.MaSP
            LEFT JOIN PhieuXuat px ON ctpx.MaPX = px.MaPX
            WHERE (@status = '' OR (@status = '1' AND (pn.TrangThai = 1 OR px.TrangThai = 1))
                               OR (@status = 'False' AND (pn.TrangThai IS NULL OR pn.TrangThai = 0 OR px.TrangThai IS NULL OR px.TrangThai = 0))
                               OR (@status = 'null' AND (pn.TrangThai IS NULL OR px.TrangThai IS NULL)))
            ORDER BY sp.MaSP, pn.NgayNhap DESC, px.NgayXuat DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@status", status);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }


        }
        //Doanh thu bán hàng
        public ActionResult DoanhThuBanHang()
        {
            var model = LayDuLieuDoanhThuBanHang();
            return View(model);
        }

        // POST: BaoCao/XuatExcelDoanhThu
        [HttpPost]
        public ActionResult XuatExcelDoanhThu()
        {
            var model = LayDuLieuDoanhThuBanHang();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("BaoCaoDoanhThuBanHang");

                worksheet.Cells["A1"].Value = "Mã sản phẩm";
                worksheet.Cells["B1"].Value = "Tên sản phẩm";
                worksheet.Cells["C1"].Value = "Số lượng bán";
                worksheet.Cells["D1"].Value = "Doanh thu";

                var row = 2;
                foreach (var item in model)
                {
                    worksheet.Cells[row, 1].Value = item.MaSP;
                    worksheet.Cells[row, 2].Value = item.TenSP;
                    worksheet.Cells[row, 3].Value = item.SoLuongBan;
                    worksheet.Cells[row, 4].Value = item.DoanhThu;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"BaoCaoDoanhThuBanHang-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        private List<DoanhThuBanHangViewModel> LayDuLieuDoanhThuBanHang()
        {
            var model = new List<DoanhThuBanHangViewModel>();
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        sp.MaSP, 
                        sp.TenSP, 
                        SUM(dhct.SoLuong) AS SoLuongBan, 
                        SUM(dhct.SoLuong * dhct.Gia) AS DoanhThu
                    FROM 
                        SanPham sp
                    LEFT JOIN 
                        CTHOADON dhct ON sp.MaSP = dhct.MaSP
                    LEFT JOIN 
                        HoaDon dh ON dhct.MaHD = dh.MaHD
                    GROUP BY 
                        sp.MaSP, 
                        sp.TenSP";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new DoanhThuBanHangViewModel
                            {
                                MaSP = reader["MaSP"].ToString(),
                                TenSP = reader["TenSP"].ToString(),
                                SoLuongBan = reader["SoLuongBan"] != DBNull.Value ? int.Parse(reader["SoLuongBan"].ToString()) : 0,
                                DoanhThu = reader["DoanhThu"] != DBNull.Value ? decimal.Parse(reader["DoanhThu"].ToString()) : 0
                            };
                            model.Add(item);
                        }
                    }
                }
            }

            return model;
        }
    }


}