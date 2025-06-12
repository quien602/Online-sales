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
    public class PhieuNhapController : Controller
    {
        // GET: Admin/PhieuNhap
        public ActionResult Index(int? page)
        {
            List<PhieuNhap> items = new List<PhieuNhap>();

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
        SELECT *, ROW_NUMBER() OVER (ORDER BY NgayNhap DESC) AS RowNumber
        FROM PhieuNhap
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
                            // Map data from SqlDataReader to PhieuNhap object
                            PhieuNhap phieuNhap = new PhieuNhap();
                            phieuNhap.MaPN = reader.GetString(reader.GetOrdinal("MaPN"));
                            phieuNhap.TenPN = reader["TenPN"].ToString();
                            string hienthiValue = reader["TrangThai"].ToString();
                            if (bool.TryParse(hienthiValue, out bool result))
                            {
                                phieuNhap.TrangThai = result;
                            }
                            else
                            {
                                phieuNhap.TrangThai = false;
                                Console.WriteLine($"Invalid value for TrangThai: {hienthiValue}");
                            }
                            // Handle Tongtien
                            decimal tongtien;
                            if (decimal.TryParse(reader["Tongtien"].ToString(), out tongtien))
                            {
                                phieuNhap.Tongtien = tongtien;
                            }

                            // Handle NgayNhap
                            DateTime ngayNhap;
                            if (DateTime.TryParse(reader["NgayNhap"].ToString(), out ngayNhap))
                            {
                                phieuNhap.NgayNhap = ngayNhap;
                            }

                            items.Add(phieuNhap);
                        }
                    }
                }
            }

            // Pass the items to the view along with pagination information
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;
            return View(items);
        }

        public ActionResult Add()
        {

            ViewBag.Products = GetProducts();
            return View();
        }
        public ActionResult Create()
        {
            ViewBag.Products = GetProducts();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(PhieuNhap model, List<CTPhieuNhap> ProductDetails)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Generate unique MaPN
                    string MaPN = "PN" + GenerateRandomNumber();

                    // Insert PhieuNhap
                    InsertPhieuNhap(MaPN, model);

                    // Insert CTPhieuNhap if SanPham is not null
                    if (ProductDetails != null && ProductDetails.Count > 0)
                    {
                        InsertCTPhieuNhap(MaPN, ProductDetails);
                    }

                    // Redirect to Index action
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the data. " + ex.Message);
                }
            }

            // If ModelState is not valid or an error occurred, populate ViewBag.Products and return to the view
            ViewBag.Products = GetProducts();
            return View(model);
        }




        // Helper method to insert PhieuNhap
        private void InsertPhieuNhap(string MaPN, PhieuNhap model)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string insertPhieuNhap = "INSERT INTO PhieuNhap (MaPN, TenPN, Tongtien, NgayNhap, MaNV, MaKho, TrangThai) VALUES (@MaPN, @TenPN, @Tongtien, @NgayNhap, NULL, NULL, 0)";

                using (SqlCommand cmdPhieuNhap = new SqlCommand(insertPhieuNhap, connection))
                {
                    cmdPhieuNhap.Parameters.AddWithValue("@MaPN", MaPN);
                    cmdPhieuNhap.Parameters.AddWithValue("@TenPN", model.TenPN);
                    cmdPhieuNhap.Parameters.AddWithValue("@Tongtien", model.Tongtien);
                    cmdPhieuNhap.Parameters.AddWithValue("@NgayNhap", model.NgayNhap);
                    cmdPhieuNhap.ExecuteNonQuery();
                }
            }
        }

        // Helper method to insert CTPhieuNhap
        private void InsertCTPhieuNhap(string MaPN, List<CTPhieuNhap> SanPham)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                if (SanPham != null)
                {
                    foreach (var product in SanPham)
                    {                       
                        // Insert into CTPhieuNhap
                        string insertChiTietPhieuNhap = "INSERT INTO CTPhieuNhap (MaPN, MaSP, SoLuong, GiaBan) VALUES (@MaPN, @MaSP, @SoLuong, @GiaBan)";
                        using (SqlCommand cmdChiTietPhieuNhap = new SqlCommand(insertChiTietPhieuNhap, connection))
                        {
                            cmdChiTietPhieuNhap.Parameters.AddWithValue("@MaPN", MaPN);
                            cmdChiTietPhieuNhap.Parameters.AddWithValue("@MaSP", product.MaSP);
                            cmdChiTietPhieuNhap.Parameters.AddWithValue("@SoLuong", product.SoLuong);
                            cmdChiTietPhieuNhap.Parameters.AddWithValue("@GiaBan", product.GiaGoc);
                            cmdChiTietPhieuNhap.ExecuteNonQuery();
                        }
                    }
                }
            }
        }






        private IEnumerable<SanPham> GetProducts()
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            List<SanPham> products = new List<SanPham>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT MaSP, TenSP, GiaGoc FROM SanPham";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        products.Add(new SanPham
                        {
                            MaSP = reader["MaSP"].ToString(),
                            TenSP = reader["TenSP"].ToString(),
                            GiaGoc = Convert.ToDecimal(reader["GiaGoc"]) // Corrected column name
                        });
                    }
                }
            }

            return products;
        }




        private string GenerateRandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(00000, 99999); // Sinh số ngẫu nhiên từ 10000 đến 99999
            return randomNumber.ToString();
        }


        public ActionResult Partial_SanPham()
        {
            List<Webbanhangonline.Models.EF.SanPham> products = new List<Webbanhangonline.Models.EF.SanPham>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            string query = @"
        SELECT MaSP, TenSP, GiaGoc
        FROM SanPham
    ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Webbanhangonline.Models.EF.SanPham product = new Webbanhangonline.Models.EF.SanPham
                    {
                        MaSP = reader["MaSP"].ToString(),
                        TenSP = reader["TenSP"].ToString(),
                        GiaGoc = Convert.ToDecimal(reader["GiaGoc"])
                    };
                    products.Add(product);
                }
            }

            ViewBag.Products = products;
            return PartialView(products);
        }
        public JsonResult GetProductOptions()
        {
            List<SelectListItem> options = new List<SelectListItem>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            string query = @"
    SELECT MaSP, TenSP
    FROM SanPham
  ";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    options.Add(new SelectListItem
                    {
                        Value = reader["MaSP"].ToString(),
                        Text = reader["TenSP"].ToString()
                    });
                }
            }

            return Json(options, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(string MaPN)
        {
            // Lấy thông tin PhieuNhap từ cơ sở dữ liệu
            PhieuNhap model = GetPhieuNhapById(MaPN);
            if (model == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách chi tiết sản phẩm
            List<CTPhieuNhap> productDetails = GetCTPhieuNhapById(MaPN);

            // Truyền thông tin vào view
            ViewBag.ProductDetails = productDetails;
            ViewBag.Products = GetProducts();
            return View(model);
        }

        // Helper method to get PhieuNhap by MaPN
        private PhieuNhap GetPhieuNhapById(string MaPN)
        {
            PhieuNhap phieuNhap = null;
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM PhieuNhap WHERE MaPN = @MaPN";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@MaPN", MaPN);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            phieuNhap = new PhieuNhap
                            {
                                MaPN = reader["MaPN"].ToString(),
                                TenPN = reader["TenPN"].ToString(),
                                Tongtien = Convert.ToDecimal(reader["Tongtien"]),
                                NgayNhap = Convert.ToDateTime(reader["NgayNhap"])
                            };
                        }
                    }
                }
            }
            return phieuNhap;
        }

        // Helper method to get CTPhieuNhap by MaPN
        private List<CTPhieuNhap> GetCTPhieuNhapById(string MaPN)
        {
            List<CTPhieuNhap> productDetails = new List<CTPhieuNhap>();
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM CTPhieuNhap WHERE MaPN = @MaPN";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@MaPN", MaPN);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productDetails.Add(new CTPhieuNhap
                            {
                                MaPN = reader["MaPN"].ToString(),
                                MaSP = reader["MaSP"].ToString(),
                                SoLuong = Convert.ToInt32(reader["SoLuong"]),
                                GiaGoc = Convert.ToDecimal(reader["GiaBan"])
                            });
                        }
                    }
                }
            }
            return productDetails;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PhieuNhap model, List<CTPhieuNhap> ProductDetails)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update PhieuNhap
                    UpdatePhieuNhap(model);

                    // Delete existing CTPhieuNhap records
                    DeleteCTPhieuNhap(model.MaPN);

                    // Insert updated CTPhieuNhap records
                    if (ProductDetails != null && ProductDetails.Count > 0)
                    {
                        InsertCTPhieuNhap(model.MaPN, ProductDetails);
                    }

                    // Redirect to Index action
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the data. " + ex.Message);
                }
            }

            // If ModelState is not valid or an error occurred, populate ViewBag.Products and return to the view
            ViewBag.Products = GetProducts();
            return View(model);
        }

        // Helper method to update PhieuNhap
        private void UpdatePhieuNhap(PhieuNhap model)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string updatePhieuNhap = "UPDATE PhieuNhap SET TenPN = @TenPN, Tongtien = @Tongtien, NgayNhap = @NgayNhap WHERE MaPN = @MaPN";

                using (SqlCommand cmdPhieuNhap = new SqlCommand(updatePhieuNhap, connection))
                {
                    cmdPhieuNhap.Parameters.AddWithValue("@MaPN", model.MaPN);
                    cmdPhieuNhap.Parameters.AddWithValue("@TenPN", model.TenPN);
                    cmdPhieuNhap.Parameters.AddWithValue("@Tongtien", model.Tongtien);
                    cmdPhieuNhap.Parameters.AddWithValue("@NgayNhap", model.NgayNhap);
                    cmdPhieuNhap.ExecuteNonQuery();
                }
            }
        }

        // Helper method to delete CTPhieuNhap by MaPN
        private void DeleteCTPhieuNhap(string MaPN)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string deleteCTPhieuNhap = "DELETE FROM CTPhieuNhap WHERE MaPN = @MaPN";

                using (SqlCommand cmdDeleteCTPhieuNhap = new SqlCommand(deleteCTPhieuNhap, connection))
                {
                    cmdDeleteCTPhieuNhap.Parameters.AddWithValue("@MaPN", MaPN);
                    cmdDeleteCTPhieuNhap.ExecuteNonQuery();
                }
            }
        }


        public ActionResult Partial_CTSanPham(string MaPN)
        {
            List<Webbanhangonline.Models.EF.CTPhieuNhap> products = new List<Webbanhangonline.Models.EF.CTPhieuNhap>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            string query = @"
    SELECT MaSP, SoLuong, GiaBan
    FROM CTPhieuNhap
    WHERE MaPN = @MaPN
";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@MaPN", MaPN); // Thêm tham số cho mã phiếu nhập

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Webbanhangonline.Models.EF.CTPhieuNhap product = new Webbanhangonline.Models.EF.CTPhieuNhap
                    {
                        MaSP = reader["MaSP"].ToString(),
                        SoLuong = Convert.ToInt32(reader["SoLuong"]),
                        GiaGoc = Convert.ToDecimal(reader["GiaBan"])
                    };
                    products.Add(product);
                }
            }

            ViewBag.Products = products;
            return PartialView(products);
        }


        [HttpPost]
        public ActionResult TrangThai(string MaPN)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool TrangThai = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current HienThi value from the database
                string selectQuery = "SELECT TrangThai FROM PhieuNhap WHERE MaPN = @MaPN";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaPN", MaPN);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        TrangThai = (bool)result;
                    }
                }

                // Update the news item in the database
                string updateQuery = "UPDATE PhieuNhap SET TrangThai = @TrangThai WHERE MaPN = @MaPN";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaPN", MaPN);
                    updateCommand.Parameters.AddWithValue("@TrangThai", !TrangThai); // Toggle the value
                    updateCommand.ExecuteNonQuery();
                }

                if (!TrangThai)
                {
                    // Retrieve thông tin sản phẩm từ bảng ChiTietPhieuXuat
                    string selectChiTietQuery = "SELECT MaSP, SoLuong FROM CTPhieuNhap WHERE MaPN = @MaPN";
                    List<(string, int)> chiTietPhieuXuat = new List<(string, int)>();

                    using (SqlCommand selectChiTietCommand = new SqlCommand(selectChiTietQuery, connection))
                    {
                        selectChiTietCommand.Parameters.AddWithValue("@MaPN", MaPN);
                        using (SqlDataReader reader = selectChiTietCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string maSP = reader.GetString(0);
                                int soLuong = reader.GetInt32(1);
                                chiTietPhieuXuat.Add((maSP, soLuong));
                            }
                        }
                    }

                    // Update số lượng in the SanPham table
                    foreach ((string maSP, int soLuong) in chiTietPhieuXuat)
                    {
                        string updateSanPhamQuery = "UPDATE SanPham SET SoLuongTam = SoLuongTam + @SoLuong WHERE MaSP = @MaSP";
                        using (SqlCommand updateSanPhamCommand = new SqlCommand(updateSanPhamQuery, connection))
                        {
                            updateSanPhamCommand.Parameters.AddWithValue("@MaSP", maSP);
                            updateSanPhamCommand.Parameters.AddWithValue("@SoLuong", soLuong);
                            updateSanPhamCommand.ExecuteNonQuery();
                        }

                        string updateCKSanPhamCKQuery = "UPDATE SanPham SET SoLuongCK = SoLuongTam WHERE MaSP = @MaSP";
                        using (SqlCommand updateSanPhamCKQuery = new SqlCommand(updateCKSanPhamCKQuery, connection))
                        {
                            updateSanPhamCKQuery.Parameters.AddWithValue("@MaSP", maSP);
                            updateSanPhamCKQuery.Parameters.AddWithValue("@SoLuong", soLuong);
                            updateSanPhamCKQuery.ExecuteNonQuery();
                        }
                    }
                }
            }

            return Json(new { success = true, TrangThai = !TrangThai });
        }

        [HttpPost]
        public ActionResult Delete(string MaPN)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string deleteCTPhieuNhapQuery = "DELETE FROM CTPhieuNhap WHERE MaPN = @MaPN";
                        using (SqlCommand command = new SqlCommand(deleteCTPhieuNhapQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@MaPN", MaPN);
                            command.ExecuteNonQuery();
                        }

                        string deletePhieuNhapQuery = "DELETE FROM PhieuNhap WHERE MaPN = @MaPN";
                        using (SqlCommand command = new SqlCommand(deletePhieuNhapQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@MaPN", MaPN);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                            }
                        }

                        transaction.Rollback();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // Log the exception for further investigation
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult DeleteAll(string ids)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        using (SqlTransaction transaction = connection.BeginTransaction())
                        {
                            try
                            {
                                var idArray = ids.Split(',');
                                if (idArray != null && idArray.Any())
                                {
                                    foreach (var id in idArray)
                                    {
                                        string MaPN = id.Trim();
                                        if (!string.IsNullOrEmpty(MaPN))
                                        {
                                            // Delete from CTPhieuNhap first
                                            string deleteCTPhieuNhapQuery = "DELETE FROM CTPhieuNhap WHERE MaPN = @MaPN";
                                            using (SqlCommand command = new SqlCommand(deleteCTPhieuNhapQuery, connection, transaction))
                                            {
                                                command.Parameters.AddWithValue("@MaPN", MaPN);
                                                command.ExecuteNonQuery();
                                            }

                                            // Then delete from PhieuNhap
                                            string deletePhieuNhapQuery = "DELETE FROM PhieuNhap WHERE MaPN = @MaPN";
                                            using (SqlCommand command = new SqlCommand(deletePhieuNhapQuery, connection, transaction))
                                            {
                                                command.Parameters.AddWithValue("@MaPN", MaPN);
                                                command.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }

                                transaction.Commit();
                                return Json(new { success = true });
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                // Log the exception for further investigation
                                Console.WriteLine(ex.Message);
                                return Json(new { success = false, message = "An error occurred while processing your request." });
                            }
                        }
                    }
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
        //Xem

        [HttpGet]
        public ActionResult See(string MaPN)
        {
            // Lấy thông tin PhieuNhap từ cơ sở dữ liệu
            PhieuNhap model = GetPhieuNhapById(MaPN);
            if (model == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách chi tiết sản phẩm
            List<CTPhieuNhap> productDetails = GetCTPhieuNhapById(MaPN);

            // Truyền thông tin vào view
            ViewBag.ProductDetails = productDetails;
            ViewBag.Products = GetProducts();
            return View(model);
        }

    }
}