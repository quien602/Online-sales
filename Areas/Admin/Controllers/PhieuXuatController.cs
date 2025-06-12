using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class PhieuXuatController : Controller
    {
        // GET: Admin/PhieuXuat
        public ActionResult Index(int? page)
        {
            List<PhieuXuat> items = new List<PhieuXuat>();

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
        SELECT *, ROW_NUMBER() OVER (ORDER BY NgayXuat DESC) AS RowNumber
        FROM PhieuXuat
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
                            // Map data from SqlDataReader to PhieuXuat object
                            PhieuXuat PhieuXuat = new PhieuXuat();
                            PhieuXuat.MaPX = reader.GetString(reader.GetOrdinal("MaPX"));
                            PhieuXuat.TenPX = reader["TenPX"].ToString();
                            string hienthiValue = reader["TrangThai"].ToString();
                            if (bool.TryParse(hienthiValue, out bool result))
                            {
                                PhieuXuat.TrangThai = result;
                            }
                            else
                            {
                                PhieuXuat.TrangThai = false;
                                Console.WriteLine($"Invalid value for TrangThai: {hienthiValue}");
                            }
                            // Handle Tongtien
                            decimal tongtien;
                            if (decimal.TryParse(reader["Tongtien"].ToString(), out tongtien))
                            {
                                PhieuXuat.Tongtien = tongtien;
                            }

                            // Handle NgayXuat
                            DateTime NgayXuat;
                            if (DateTime.TryParse(reader["NgayXuat"].ToString(), out NgayXuat))
                            {
                                PhieuXuat.NgayXuat = NgayXuat;
                            }

                            items.Add(PhieuXuat);
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
        public ActionResult Add(PhieuXuat model, List<CTPhieuXuat> ProductDetails)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Generate unique MaPX
                    string MaPX = "PX" + GenerateRandomNumber();

                    // Insert PhieuXuat
                    InsertPhieuXuat(MaPX, model);

                    // Insert CTPhieuXuat if SanPham is not null
                    if (ProductDetails != null && ProductDetails.Count > 0)
                    {
                        InsertCTPhieuXuat(MaPX, ProductDetails);
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




        // Helper method to insert PhieuXuat
        private void InsertPhieuXuat(string MaPX, PhieuXuat model)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string insertPhieuXuat = "INSERT INTO PhieuXuat (MaPX, TenPX, Tongtien, NgayXuat, MaNV, MaKho) VALUES (@MaPX, @TenPX, @Tongtien, @NgayXuat, NULL, NULL)";

                using (SqlCommand cmdPhieuXuat = new SqlCommand(insertPhieuXuat, connection))
                {
                    cmdPhieuXuat.Parameters.AddWithValue("@MaPX", MaPX);
                    cmdPhieuXuat.Parameters.AddWithValue("@TenPX", model.TenPX);
                    cmdPhieuXuat.Parameters.AddWithValue("@Tongtien", model.Tongtien);
                    cmdPhieuXuat.Parameters.AddWithValue("@NgayXuat", model.NgayXuat);
                    cmdPhieuXuat.ExecuteNonQuery();
                }
            }
        }

        // Helper method to insert CTPhieuXuat
        private void InsertCTPhieuXuat(string MaPX, List<CTPhieuXuat> SanPham)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                if (SanPham != null)
                {
                    foreach (var product in SanPham)
                    {
                        // Insert into CTPhieuXuat
                        string insertChiTietPhieuXuat = "INSERT INTO CTPhieuXuat (MaPX, MaSP, SoLuong, GiaBan) VALUES (@MaPX, @MaSP, @SoLuong, @GiaBan)";
                        using (SqlCommand cmdChiTietPhieuXuat = new SqlCommand(insertChiTietPhieuXuat, connection))
                        {
                            cmdChiTietPhieuXuat.Parameters.AddWithValue("@MaPX", MaPX);
                            cmdChiTietPhieuXuat.Parameters.AddWithValue("@MaSP", product.MaSP);
                            cmdChiTietPhieuXuat.Parameters.AddWithValue("@SoLuong", product.SoLuong);
                            cmdChiTietPhieuXuat.Parameters.AddWithValue("@GiaBan", product.GiaBan);
                            cmdChiTietPhieuXuat.ExecuteNonQuery();
                        }

                        // Update SanPham quantity
                        string updateSanPhamQuantity = "UPDATE SanPham SET SoLuong = SoLuong - @SoLuong WHERE MaSP = @MaSP";
                        using (SqlCommand cmdUpdateSanPham = new SqlCommand(updateSanPhamQuantity, connection))
                        {
                            cmdUpdateSanPham.Parameters.AddWithValue("@SoLuong", product.SoLuong);
                            cmdUpdateSanPham.Parameters.AddWithValue("@MaSP", product.MaSP);
                            cmdUpdateSanPham.ExecuteNonQuery();
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
                string query = "SELECT MaSP, TenSP, GiaBan FROM SanPham";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        products.Add(new SanPham
                        {
                            MaSP = reader["MaSP"].ToString(),
                            TenSP = reader["TenSP"].ToString(),
                            GiaBan = Convert.ToDecimal(reader["GiaBan"]) // Corrected column name
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
        SELECT MaSP, TenSP, GiaBan
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
                        GiaBan = Convert.ToDecimal(reader["GiaBan"])
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
        public ActionResult Edit(string MaPX)
        {
            // Lấy thông tin PhieuXuat từ cơ sở dữ liệu
            PhieuXuat model = GetPhieuXuatById(MaPX);
            if (model == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách chi tiết sản phẩm
            List<CTPhieuXuat> productDetails = GetCTPhieuXuatById(MaPX);

            // Truyền thông tin vào view
            ViewBag.ProductDetails = productDetails;
            ViewBag.Products = GetProducts();
            return View(model);
        }

        // Helper method to get PhieuXuat by MaPX
        private PhieuXuat GetPhieuXuatById(string MaPX)
        {
            PhieuXuat PhieuXuat = null;
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM PhieuXuat WHERE MaPX = @MaPX";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@MaPX", MaPX);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            PhieuXuat = new PhieuXuat
                            {
                                MaPX = reader["MaPX"].ToString(),
                                TenPX = reader["TenPX"].ToString(),
                                Tongtien = Convert.ToDecimal(reader["Tongtien"]),
                                NgayXuat = Convert.ToDateTime(reader["NgayXuat"])
                            };
                        }
                    }
                }
            }
            return PhieuXuat;
        }

        // Helper method to get CTPhieuXuat by MaPX
        private List<CTPhieuXuat> GetCTPhieuXuatById(string MaPX)
        {
            List<CTPhieuXuat> productDetails = new List<CTPhieuXuat>();
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM CTPhieuXuat WHERE MaPX = @MaPX";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@MaPX", MaPX);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productDetails.Add(new CTPhieuXuat
                            {
                                MaPX = reader["MaPX"].ToString(),
                                MaSP = reader["MaSP"].ToString(),
                                SoLuong = Convert.ToInt32(reader["SoLuong"]),
                                GiaBan = Convert.ToDecimal(reader["GiaBan"])
                            });
                        }
                    }
                }
            }
            return productDetails;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PhieuXuat model, List<CTPhieuXuat> ProductDetails)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update PhieuXuat
                    UpdatePhieuXuat(model);

                    // Delete existing CTPhieuXuat records
                    DeleteCTPhieuXuat(model.MaPX);

                    // Insert updated CTPhieuXuat records
                    if (ProductDetails != null && ProductDetails.Count > 0)
                    {
                        InsertCTPhieuXuat(model.MaPX, ProductDetails);
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

        // Helper method to update PhieuXuat
        private void UpdatePhieuXuat(PhieuXuat model)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string updatePhieuXuat = "UPDATE PhieuXuat SET TenPX = @TenPX, Tongtien = @Tongtien, NgayXuat = @NgayXuat WHERE MaPX = @MaPX";

                using (SqlCommand cmdPhieuXuat = new SqlCommand(updatePhieuXuat, connection))
                {
                    cmdPhieuXuat.Parameters.AddWithValue("@MaPX", model.MaPX);
                    cmdPhieuXuat.Parameters.AddWithValue("@TenPX", model.TenPX);
                    cmdPhieuXuat.Parameters.AddWithValue("@Tongtien", model.Tongtien);
                    cmdPhieuXuat.Parameters.AddWithValue("@NgayXuat", model.NgayXuat);
                    cmdPhieuXuat.ExecuteNonQuery();
                }
            }
        }

        private void DeleteCTPhieuXuat(string MaPX)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string deleteCTPhieuXuat = "DELETE FROM CTPhieuXuat WHERE MaPX = @MaPX";

                using (SqlCommand cmdDeleteCTPhieuXuat = new SqlCommand(deleteCTPhieuXuat, connection))
                {
                    cmdDeleteCTPhieuXuat.Parameters.AddWithValue("@MaPX", MaPX);
                    cmdDeleteCTPhieuXuat.ExecuteNonQuery();
                }
            }
        }


        public ActionResult Partial_CTPX(string MaPX)
        {
            List<Webbanhangonline.Models.EF.CTPhieuXuat> products = new List<Webbanhangonline.Models.EF.CTPhieuXuat>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            string query = "SELECT MaSP, SoLuong, GiaBan FROM CTPhieuXuat WHERE MaPX = @MaPX";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MaPX", MaPX);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Webbanhangonline.Models.EF.CTPhieuXuat product = new Webbanhangonline.Models.EF.CTPhieuXuat
                    {
                        MaSP = reader["MaSP"].ToString(),
                        SoLuong = Convert.ToInt32(reader["SoLuong"]),
                        GiaBan = Convert.ToDecimal(reader["GiaBan"])
                    };
                    products.Add(product);
                }
            }

            return PartialView(products);
        }

        [HttpPost]
        public ActionResult Delete(string MaPX)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string deleteCTPhieuXuatQuery = "DELETE FROM CTPhieuXuat WHERE MaPX = @MaPX";
                        using (SqlCommand command = new SqlCommand(deleteCTPhieuXuatQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@MaPX", MaPX);
                            command.ExecuteNonQuery();
                        }

                        string deletePhieuXuatQuery = "DELETE FROM PhieuXuat WHERE MaPX = @MaPX";
                        using (SqlCommand command = new SqlCommand(deletePhieuXuatQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@MaPX", MaPX);
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
                                        string MaPX = id.Trim();
                                        if (!string.IsNullOrEmpty(MaPX))
                                        {
                                            // Delete from CTPhieuXuat first
                                            string deleteCTPhieuXuatQuery = "DELETE FROM CTPhieuXuat WHERE MaPX = @MaPX";
                                            using (SqlCommand command = new SqlCommand(deleteCTPhieuXuatQuery, connection, transaction))
                                            {
                                                command.Parameters.AddWithValue("@MaPX", MaPX);
                                                command.ExecuteNonQuery();
                                            }

                                            // Then delete from PhieuXuat
                                            string deletePhieuXuatQuery = "DELETE FROM PhieuXuat WHERE MaPX = @MaPX";
                                            using (SqlCommand command = new SqlCommand(deletePhieuXuatQuery, connection, transaction))
                                            {
                                                command.Parameters.AddWithValue("@MaPX", MaPX);
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

        public ActionResult TrangThai(string MaPX)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool TrangThai = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current TrangThai value from the PhieuXuat table
                string selectQuery = "SELECT TrangThai FROM PhieuXuat WHERE MaPX = @MaPX";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaPX", MaPX);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        TrangThai = Convert.ToBoolean(result);
                    }
                    else
                    {
                        // Xử lý khi giá trị trả về là null hoặc DBNull.Value
                        // Ví dụ: TrangThai = false; // hoặc TrangThai = true; tùy theo logic của bạn
                    }

                }

                // Update the TrangThai in the PhieuXuat table
                string updateQuery = "UPDATE PhieuXuat SET TrangThai = @TrangThai WHERE MaPX = @MaPX";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaPX", MaPX);
                    updateCommand.Parameters.AddWithValue("@TrangThai", !TrangThai); // Toggle the value
                    updateCommand.ExecuteNonQuery();
                }

                // If TrangThai is true (chốt), update the số lượng in the SanPham table
                if (!TrangThai)
                {
                    // Retrieve thông tin sản phẩm từ bảng ChiTietPhieuXuat
                    string selectChiTietQuery = "SELECT MaSP, SoLuong FROM CTPhieuXuat WHERE MaPX = @MaPX";
                    List<(string, int)> chiTietPhieuXuat = new List<(string, int)>();

                    using (SqlCommand selectChiTietCommand = new SqlCommand(selectChiTietQuery, connection))
                    {
                        selectChiTietCommand.Parameters.AddWithValue("@MaPX", MaPX);
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
                        string updateSanPhamQuery = "UPDATE SanPham SET SoLuongTam = SoLuongTam - @SoLuong WHERE MaSP = @MaSP";
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
        [HttpGet]
        public ActionResult See(string MaPX)
        {
            // Lấy thông tin PhieuXuat từ cơ sở dữ liệu
            PhieuXuat model = GetPhieuXuatById(MaPX);
            if (model == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách chi tiết sản phẩm
            List<CTPhieuXuat> productDetails = GetCTPhieuXuatById(MaPX);

            // Truyền thông tin vào view
            ViewBag.ProductDetails = productDetails;
            ViewBag.Products = GetProducts();
            return View(model);
        }

    }
}