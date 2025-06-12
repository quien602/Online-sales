using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class SanPhamController : Controller
    {
        // GET: Admin/SanPham
        public ActionResult Index(int? page)
        {
            List<SanPham> items = new List<SanPham>();

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            // Define pagination parameters
            int pageSize = 10;
            int pageIndex = page ?? 1; // Ensure that page is properly initialized

            // Calculate starting row index for pagination
            int startRowIndex = (pageIndex - 1) * pageSize + 1;
            int endRowIndex = pageIndex * pageSize;
            // Construct the SQL query with pagination
            string sqlQuery = $@"
SELECT SanPham.*, LoaiSanPham.TenLSP
FROM SanPham
INNER JOIN LoaiSanPham ON SanPham.MaLSP = LoaiSanPham.MaLSP
ORDER BY SanPham.MaSP
OFFSET {startRowIndex - 1} ROWS
FETCH NEXT {pageSize} ROWS ONLY";

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
                            // Map data from SqlDataReader to News object
                            SanPham news = new SanPham();
                            news.MaSP = reader.GetString(reader.GetOrdinal("MaSP"));
                            news.TenSP = reader["TenSP"].ToString();
                            news.MoTa = reader["MoTa"].ToString();
                            news.ChiTiet = reader["ChiTiet"].ToString();
                            news.HinhAnh = reader["HinhAnh"].ToString();
                            int soLuong;
                            if (int.TryParse(reader["SoLuong"].ToString(), out soLuong))
                            {
                                news.SoLuong = soLuong;
                            }
                            else
                            {
                                // Xử lý khi giá trị không hợp lệ
                            }
                            news.MaLSP = reader["TenLSP"].ToString();
                            decimal giaGoc;
                            if (decimal.TryParse(reader["GiaGoc"].ToString(), out giaGoc))
                            {
                                news.GiaGoc = giaGoc;
                            }
                            else
                            {
                                giaGoc = 0;
                            }
                            decimal GiaBan;
                            if (decimal.TryParse(reader["GiaBan"].ToString(), out GiaBan))
                            {
                                news.GiaBan = GiaBan;
                            }
                            else
                            {
                                giaGoc = 0;
                            }
                            decimal GiaKM;
                            if (decimal.TryParse(reader["GiaKM"].ToString(), out GiaKM))
                            {
                                news.GiaKM = GiaKM;
                            }
                            else
                            {
                                giaGoc = 0;
                            }
                            string hienthiValue = reader["Hienthi"].ToString();
                            if (bool.TryParse(hienthiValue, out bool result))
                            {
                                news.HienThi = result;
                            }
                            else
                            {
                                news.HienThi = false;
                                Console.WriteLine($"Invalid value for Hienthi: {hienthiValue}");
                            }
                            string hienthiKM = reader["KhuyenMai"].ToString();
                            if (bool.TryParse(hienthiKM, out bool kq))
                            {
                                news.KhuyenMai = kq;
                            }
                            else
                            {
                                news.KhuyenMai = false;
                                Console.WriteLine($"Invalid value for KhuyenMai: {hienthiKM}");
                            }
                            string hienthihome = reader["Home"].ToString();
                            if (bool.TryParse(hienthihome, out bool Home))
                            {
                                news.Home = Home;
                            }
                            else
                            {
                                news.Home = false;
                                Console.WriteLine($"Invalid value for Home: {hienthihome}");
                            }
                            string hienthisphot = reader["SPHot"].ToString();
                            if (bool.TryParse(hienthisphot, out bool SPHot))
                            {
                                news.SPHot = SPHot;
                            }
                            else
                            {
                                news.SPHot = false;
                                Console.WriteLine($"Invalid value for Home: {hienthisphot}");
                            }
                            DateTime ngayTao;
                            if (reader["NgayTao"] != DBNull.Value) // Check for DBNull before conversion
                            {
                                if (DateTime.TryParse(reader["NgayTao"].ToString(), out ngayTao))
                                {
                                    // Conversion successful, assign the value to your variable
                                    news.NgayTao = ngayTao;
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }

                            DateTime ngaycapnhat;
                            if (reader["NgayCapNhat"] != DBNull.Value) // Check for DBNull before conversion
                            {
                                if (DateTime.TryParse(reader["NgayCapNhat"].ToString(), out ngaycapnhat))
                                {
                                    // Conversion successful, assign the value to your variable
                                    news.NgayCapNhat = ngaycapnhat;
                                }
                                else
                                {
                                }
                            }
                            else
                            {
                            }

                            items.Add(news);
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
        public ActionResult KhuyenMai(string MaSP)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool isActive = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current HienThi value from the database
                string selectQuery = "SELECT KhuyenMai FROM SanPham WHERE MaSP = @MaSP";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaSP", MaSP);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        isActive = (bool)result;
                    }
                }

                // Update the news item in the database
                string updateQuery = "UPDATE SanPham SET KhuyenMai = @KhuyenMai WHERE MaSP = @MaSP";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaSP", MaSP);
                    updateCommand.Parameters.AddWithValue("@KhuyenMai", !isActive); // Toggle the value
                    updateCommand.ExecuteNonQuery();
                }
            }

            return Json(new { success = true, KhuyenMai = !isActive });
        }
        [HttpPost]
        public ActionResult HienThi(string MaSP)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool isActive = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current HienThi value from the database
                string selectQuery = "SELECT HienThi FROM SanPham WHERE MaSP = @MaSP";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaSP", MaSP);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        isActive = (bool)result;
                    }
                }

                // Update the news item in the database
                string updateQuery = "UPDATE SanPham SET HienThi = @HienThi WHERE MaSP = @MaSP";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaSP", MaSP);
                    updateCommand.Parameters.AddWithValue("@HienThi", !isActive); // Toggle the value
                    updateCommand.ExecuteNonQuery();
                }
            }

            return Json(new { success = true, HienThi = !isActive });
        }
        [HttpPost]
        public ActionResult HomeActive(string MaSP)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool isActive = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current HienThi value from the database
                string selectQuery = "SELECT Home FROM SanPham WHERE MaSP = @MaSP";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaSP", MaSP);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        isActive = (bool)result;
                    }
                }

                // Update the news item in the database
                string updateQuery = "UPDATE SanPham SET Home = @Home WHERE MaSP = @MaSP";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaSP", MaSP);
                    updateCommand.Parameters.AddWithValue("@Home", !isActive); // Toggle the value
                    updateCommand.ExecuteNonQuery();
                }
            }

            return Json(new { success = true, HomeActive = !isActive });
        }
        [HttpPost]
        public ActionResult SPHot(string MaSP)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool isActive = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current HienThi value from the database
                string selectQuery = "SELECT SPHot FROM SanPham WHERE MaSP = @MaSP";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaSP", MaSP);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        isActive = (bool)result;
                    }
                }

                // Update the news item in the database
                string updateQuery = "UPDATE SanPham SET SPHot = @SPHot WHERE MaSP = @MaSP";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaSP", MaSP);
                    updateCommand.Parameters.AddWithValue("@SPHot", !isActive); // Toggle the value
                    updateCommand.ExecuteNonQuery();
                }
            }

            return Json(new { success = true, SPHot = !isActive });
        }
        public ActionResult Add()
        {
            // Assuming you have a connection string named "ConnectionString" in your web.config
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            // Your SQL queries to retrieve ProductCategories and Warehouses
            string queryLoaiSanPham = "SELECT MaLSP, TenLSP FROM LoaiSanPham";
            string queryKho = "SELECT MaKho, TenKho FROM Kho";

            // Lists to hold SelectListItem objects
            List<SelectListItem> productCategoryItems = new List<SelectListItem>();
            List<SelectListItem> warehouseItems = new List<SelectListItem>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Query to retrieve Product Categories
                using (SqlCommand command = new SqlCommand(queryLoaiSanPham, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string MaLSP = reader["MaLSP"].ToString();
                            string TenLSP = reader["TenLSP"].ToString();

                            SelectListItem item = new SelectListItem
                            {
                                Value = MaLSP,
                                Text = TenLSP
                            };
                            productCategoryItems.Add(item);
                        }
                    }
                    connection.Close();
                }

                // Query to retrieve Warehouses
                using (SqlCommand command = new SqlCommand(queryKho, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string MaKho = reader["MaKho"].ToString();
                            string TenKho = reader["TenKho"].ToString();

                            SelectListItem item = new SelectListItem
                            {
                                Value = MaKho,
                                Text = TenKho
                            };
                            warehouseItems.Add(item);
                        }
                    }
                }
            }

            // Create SelectLists from selectListItems
            SelectList productCategorySelectList = new SelectList(productCategoryItems, "Value", "Text");
            SelectList warehouseSelectList = new SelectList(warehouseItems, "Value", "Text");

            // Add the SelectLists to ViewBag
            ViewBag.LoaiSanPham = productCategorySelectList;
            ViewBag.Kho = warehouseSelectList;

            return View();

        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Add(SanPham model)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            model.NgayTao = DateTime.Now; // Set NgayTao to current date and time
            model.NgayCapNhat = DateTime.Now; // Set NgayCapNhat to current date and time
            model.NguoiTao = "admin"; // Set NguoiTao to "admin"

            // Retrieve HinhAnh from form data (this will be set by the selected radio button)
            model.HinhAnh = Request.Form["HinhAnh"];
            string masp = "SP" + GenerateRandomNumber();

            string sqlQuery = @"
        INSERT INTO SanPham (MaSP, TenSP, MoTa, ChiTiet, HinhAnh, GiaGoc, GiaBan, GiaKM, SoLuong, HienThi, KhuyenMai, Home, SPHot, NgayTao, NgayCapNhat, NguoiTao, Alias, MaLSP, MaKho, SoLuongCK, SoLuongTam)
        VALUES (@MaSP, @TenSP, @MoTa, @ChiTiet, @HinhAnh, @GiaGoc, @GiaBan, @GiaKM, @SoLuong, @HienThi, @KhuyenMai, @Home, @SPHot, @NgayTao, @NgayCapNhat, @NguoiTao, @Alias, @MaLSP, @MaKho, 0, @SoLuongTam)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaSP", masp);
                    command.Parameters.AddWithValue("@TenSP", model.TenSP);
                    command.Parameters.AddWithValue("@MoTa", model.MoTa);
                    command.Parameters.AddWithValue("@ChiTiet", model.ChiTiet);
                    command.Parameters.AddWithValue("@HinhAnh", model.HinhAnh); // Use the selected image URL
                    command.Parameters.AddWithValue("@GiaGoc", model.GiaGoc);
                    command.Parameters.AddWithValue("@GiaBan", model.GiaBan);
                    command.Parameters.AddWithValue("@GiaKM", model.GiaKM);
                    command.Parameters.AddWithValue("@SoLuong", model.SoLuong);
                    command.Parameters.AddWithValue("@HienThi", model.HienThi);
                    command.Parameters.AddWithValue("@KhuyenMai", model.KhuyenMai);
                    command.Parameters.AddWithValue("@Home", model.Home);
                    command.Parameters.AddWithValue("@SPHot", model.SPHot);
                    command.Parameters.AddWithValue("@NgayTao", model.NgayTao); // Use current date and time for NgayTao
                    command.Parameters.AddWithValue("@NgayCapNhat", model.NgayCapNhat); // Use current date and time for NgayCapNhat
                    command.Parameters.AddWithValue("@NguoiTao", model.NguoiTao); // Use "admin" for NguoiTao
                    model.Alias = Webbanhangonline.Models.Commons.Filter.FilterChar(model.Alias);
                    command.Parameters.AddWithValue("@Alias", model.Alias);
                    command.Parameters.AddWithValue("@MaLSP", model.MaLSP);
                    command.Parameters.AddWithValue("@MaKho", model.MaKho);
                    command.Parameters.AddWithValue("@SoLuongTam", model.SoLuong);


                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        private string GenerateRandomNumber()
        {
            Random random = new Random();
            int randomNumber = random.Next(00000, 99999); // Sinh số ngẫu nhiên từ 10000 đến 99999
            return randomNumber.ToString();
        }
        public ActionResult Edit(string MaSP)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            SanPham item = new SanPham();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM SanPham WHERE MaSP = @MaSP";

                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@MaSP", MaSP);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item.MaSP = reader["MaSP"].ToString();
                        item.TenSP = reader["TenSP"].ToString();
                        item.MoTa = reader["MoTa"].ToString();
                        item.HinhAnh = reader["HinhAnh"].ToString();
                        item.ChiTiet = reader["ChiTiet"].ToString();
                        item.MaLSP = reader["MaLSP"].ToString();
                        item.Alias = reader["Alias"].ToString();
                        if (!reader.IsDBNull(reader.GetOrdinal("HienThi")))
                        {
                            string hienthiValue = reader["HienThi"].ToString();
                            if (bool.TryParse(hienthiValue, out bool hienthiBool))
                            {
                                item.HienThi = hienthiBool;
                            }
                        }

                        // Chuyển đổi và gán giá trị cho GiaGoc
                        if (!reader.IsDBNull(reader.GetOrdinal("GiaGoc")))
                        {
                            if (decimal.TryParse(reader["GiaGoc"].ToString(), out decimal giaGoc))
                            {
                                item.GiaGoc = giaGoc;
                            }
                        }

                        // Chuyển đổi và gán giá trị cho GiaBan
                        if (!reader.IsDBNull(reader.GetOrdinal("GiaBan")))
                        {
                            if (decimal.TryParse(reader["GiaBan"].ToString(), out decimal giaBan))
                            {
                                item.GiaBan = giaBan;
                            }
                        }

                        // Chuyển đổi và gán giá trị cho GiaKM
                        if (!reader.IsDBNull(reader.GetOrdinal("GiaKM")))
                        {
                            if (decimal.TryParse(reader["GiaKM"].ToString(), out decimal giaKM))
                            {
                                item.GiaKM = giaKM;
                            }
                        }

                        // Chuyển đổi và gán giá trị cho SoLuong
                        if (!reader.IsDBNull(reader.GetOrdinal("SoLuong")))
                        {
                            if (int.TryParse(reader["SoLuong"].ToString(), out int soLuong))
                            {
                                item.SoLuong = soLuong;
                            }
                        }

                        // Chuyển đổi và gán giá trị cho NgayTao
                        if (!reader.IsDBNull(reader.GetOrdinal("NgayTao")))
                        {
                            if (DateTime.TryParse(reader["NgayTao"].ToString(), out DateTime ngayTao))
                            {
                                item.NgayTao = ngayTao;
                            }
                        }

                        // Chuyển đổi và gán giá trị cho NgayCapNhat
                        if (!reader.IsDBNull(reader.GetOrdinal("NgayCapNhat")))
                        {
                            if (DateTime.TryParse(reader["NgayCapNhat"].ToString(), out DateTime ngayCapNhat))
                            {
                                item.NgayCapNhat = ngayCapNhat;
                            }
                        }

                        // Chuyển đổi và gán giá trị cho SPHot
                        if (!reader.IsDBNull(reader.GetOrdinal("SPHot")))
                        {
                            string sphotValue = reader["SPHot"].ToString();
                            if (bool.TryParse(sphotValue, out bool sphotBool))
                            {
                                item.SPHot = sphotBool;
                            }
                        }

                        // Chuyển đổi và gán giá trị cho Home
                        if (!reader.IsDBNull(reader.GetOrdinal("Home")))
                        {
                            string homeValue = reader["Home"].ToString();
                            if (bool.TryParse(homeValue, out bool homeBool))
                            {
                                item.Home = homeBool;
                            }
                        }

                        // Chuyển đổi và gán giá trị cho KhuyenMai
                        if (!reader.IsDBNull(reader.GetOrdinal("KhuyenMai")))
                        {
                            string khuyenmaiValue = reader["KhuyenMai"].ToString();
                            if (bool.TryParse(khuyenmaiValue, out bool khuyenmaiBool))
                            {
                                item.KhuyenMai = khuyenmaiBool;
                            }
                        }


                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
            }
            string connString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            // Lists to hold SelectListItem objects
            List<SelectListItem> selectListLoaiSanPham = new List<SelectListItem>();
            List<SelectListItem> selectListKho = new List<SelectListItem>();

            using (SqlConnection conn1 = new SqlConnection(connString))
            {
                // Query to retrieve Product Categories
                string queryLoaiSanPham = "SELECT MaLSP, TenLSP FROM LoaiSanPham";
                SqlCommand commandLoaiSanPham = new SqlCommand(queryLoaiSanPham, conn1);

                conn1.Open();
                using (SqlDataReader reader = commandLoaiSanPham.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string MaLSP = reader["MaLSP"].ToString();
                        string TenLSP = reader["TenLSP"].ToString();

                        SelectListItem itemLoaiSanPham = new SelectListItem
                        {
                            Value = MaLSP,
                            Text = TenLSP
                        };
                        selectListLoaiSanPham.Add(itemLoaiSanPham);
                    }
                }
                conn1.Close();
            }
            string connString1 = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection conn2 = new SqlConnection(connString1))
            {
                // Query to retrieve Warehouses
                string queryKho = "SELECT MaKho, TenKho FROM Kho";
                SqlCommand commandKho = new SqlCommand(queryKho, conn2);

                conn2.Open();
                using (SqlDataReader reader = commandKho.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string MaKho = reader["MaKho"].ToString();
                        string TenKho = reader["TenKho"].ToString();

                        SelectListItem itemKho = new SelectListItem
                        {
                            Value = MaKho,
                            Text = TenKho
                        };
                        selectListKho.Add(itemKho);
                    }
                }
                conn2.Close();
            }

            // Create SelectLists from selectListItems
            SelectList selectListLoaiSanPhamFinal = new SelectList(selectListLoaiSanPham, "Value", "Text");
            SelectList selectListKhoFinal = new SelectList(selectListKho, "Value", "Text");

            // Add the SelectLists to ViewBag
            ViewBag.LoaiSanPham = selectListLoaiSanPhamFinal;
            ViewBag.Kho = selectListKhoFinal;

            return View(item);


        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SanPham model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch MaSP from the SanPham table based on the condition (e.g., SanPham)
                    string fetchMaSPQuery = "SELECT MaSP FROM SanPham WHERE MaSP = @MaSP";
                    SqlCommand fetchMaSPCommand = new SqlCommand(fetchMaSPQuery, connection);
                    fetchMaSPCommand.Parameters.AddWithValue("@MaSP", model.MaSP);
                    model.NgayCapNhat = DateTime.Now; // Set NgayCapNhat to current date and time

                    // Execute scalar to fetch MaSP
                    string maSP = fetchMaSPCommand.ExecuteScalar() as string;

                    // Check if maSP is not null or empty before proceeding
                    if (!string.IsNullOrEmpty(maSP))
                    {
                        // Update SanPham table using MaSP
                        string sqlQuery = @"
                UPDATE SanPham 
                SET 
                    TenSP = @TenSP, 
                    MoTa = @MoTa, 
                    ChiTiet = @ChiTiet, 
                    HinhAnh = @HinhAnh, 
                    GiaGoc = @GiaGoc, 
                    GiaBan = @GiaBan, 
                    GiaKM = @GiaKM, 
                    SoLuong = @SoLuong, 
                    HienThi = @HienThi, 
                    KhuyenMai = @KhuyenMai, 
                    Home = @Home, 
                    SPHot = @SPHot,
                    Alias = @Alias,
                    NgayCapNhat = @NgayCapNhat
                WHERE MaSP = @MaSP";

                        SqlCommand command = new SqlCommand(sqlQuery, connection);
                        command.Parameters.AddWithValue("@MaSP", model.MaSP);
                        command.Parameters.AddWithValue("@TenSP", model.TenSP);
                        command.Parameters.AddWithValue("@MoTa", model.MoTa);
                        command.Parameters.AddWithValue("@ChiTiet", model.ChiTiet);
                        command.Parameters.AddWithValue("@HinhAnh", model.HinhAnh); // Use the selected image URL
                        command.Parameters.AddWithValue("@GiaGoc", model.GiaGoc);
                        command.Parameters.AddWithValue("@GiaBan", model.GiaBan);
                        command.Parameters.AddWithValue("@GiaKM", model.GiaKM);
                        command.Parameters.AddWithValue("@SoLuong", model.SoLuong);
                        command.Parameters.AddWithValue("@HienThi", model.HienThi);
                        command.Parameters.AddWithValue("@KhuyenMai", model.KhuyenMai);
                        command.Parameters.AddWithValue("@Home", model.Home);
                        command.Parameters.AddWithValue("@SPHot", model.SPHot);
                        model.Alias = Webbanhangonline.Models.Commons.Filter.FilterChar(model.Alias);
                        command.Parameters.AddWithValue("@Alias", model.Alias); 
                        command.Parameters.AddWithValue("@NgayCapNhat", model.NgayCapNhat); // Use current date and time for NgayCapNhat
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        // Handle the case where MaSP is null or empty from the SanPham table
                        ModelState.AddModelError(string.Empty, "The product ID does not exist.");
                        return View(model);
                    }
                }

                return RedirectToAction("Index");
            }

            return View(model);

        }
        public ActionResult Delete(string MaSP)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "DELETE FROM SanPham WHERE MaSP = @MaSP";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaSP", MaSP);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true });
                    }
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
                                // Giả sử MaSP là kiểu chuỗi trong cơ sở dữ liệu
                                string MaSP = id.Trim(); // Xóa khoảng trắng ở hai đầu nếu có
                                if (!string.IsNullOrEmpty(MaSP))
                                {
                                    string sqlQuery = "DELETE FROM SanPham WHERE MaSP LIKE @MaSP";
                                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                                    {
                                        command.Parameters.AddWithValue("@MaSP", MaSP);
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
        public ActionResult ExportTemplate()
        {
            var csv = new StringBuilder();
            string header = "MaSP,TenSP,MoTa,ChiTiet,HinhAnh,GiaGoc,GiaBan,GiaKM,SoLuong,HienThi,KhuyenMai,Home,SPHot,DacDiem,NgayTao,NgayCapNhat,NguoiTao,SLTruyCap,Alias,MaLSP,MaKho,SoLuongCK,SoLuongTam";
            csv.AppendLine(header);
            byte[] buffer = Encoding.UTF8.GetBytes(csv.ToString());
            return File(buffer, "text/csv", "SanPhamTemplate.csv");
        }
        [HttpPost]
        public JsonResult UploadTemplate(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                var sanPhamList = new List<SanPham>();

                try
                {
                    using (var reader = new StreamReader(file.InputStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var values = line.Split(',');

                            // Skip the header row
                            if (values[0] == "MaSP") continue;

                            try
                            {
                                var sanPham = new SanPham
                                {
                                    MaSP = GenerateNewMaSP(),
                                    TenSP = values[1],
                                    MoTa = values[2],
                                    ChiTiet = values[3],
                                    HinhAnh = values[4],
                                    GiaGoc = decimal.Parse(values[5]),
                                    GiaBan = decimal.Parse(values[6]),
                                    GiaKM = decimal.Parse(values[7]),
                                    SoLuong = int.Parse(values[8]),
                                    HienThi = bool.Parse(values[9]),
                                    KhuyenMai = bool.Parse(values[10]),
                                    Home = bool.Parse(values[11]),
                                    SPHot = bool.Parse(values[12]),
                                    DacDiem = !string.IsNullOrEmpty(values[13]) ? bool.Parse(values[13]) : false,
                                    NgayTao = DateTime.Parse(values[14]),
                                    NgayCapNhat = DateTime.Parse(values[15]),
                                    NguoiTao = values[16],
                                    SLTruyCap = !string.IsNullOrEmpty(values[17]) ? int.Parse(values[17]) : 0,
                                    Alias = values[18],
                                    MaLSP = values[19],
                                    MaKho = values[20],
                                    SoLuongCK = !string.IsNullOrEmpty(values[21]) ? int.Parse(values[21]) : 0,
                                    SoLuongTam = !string.IsNullOrEmpty(values[22]) ? int.Parse(values[22]) : 0
                                };

                                sanPhamList.Add(sanPham);
                            }
                            catch (Exception ex)
                            {
                                // Log the exception or handle it as needed
                                // Skip the current row if there is an error
                                Console.WriteLine($"Error parsing row: {ex.Message}");
                                continue;
                            }
                        }
                    }

                    using (var connection = new SqlConnection("Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;"))
                    {
                        connection.Open();

                        foreach (var sanPham in sanPhamList)
                        {
                            try
                            {
                                // Check if MaSP already exists
                                bool maSPExists;
                                do
                                {
                                    maSPExists = false;
                                    var checkQuery = "SELECT COUNT(*) FROM SanPham WHERE MaSP = @MaSP";
                                    using (var checkCommand = new SqlCommand(checkQuery, connection))
                                    {
                                        checkCommand.Parameters.AddWithValue("@MaSP", sanPham.MaSP);
                                        var count = (int)checkCommand.ExecuteScalar();
                                        if (count > 0)
                                        {
                                            maSPExists = true;
                                            sanPham.MaSP = GenerateNewMaSP(); // Generate new MaSP if it already exists
                                        }
                                    }
                                } while (maSPExists);

                                var query = @"INSERT INTO SanPham 
                            (MaSP, TenSP, MoTa, ChiTiet, HinhAnh, GiaGoc, GiaBan, GiaKM, SoLuong, HienThi, KhuyenMai, Home, SPHot, DacDiem, NgayTao, NgayCapNhat, NguoiTao, SLTruyCap, Alias, MaLSP, MaKho, SoLuongCK, SoLuongTam)
                            VALUES (@MaSP, @TenSP, @MoTa, @ChiTiet, @HinhAnh, @GiaGoc, @GiaBan, @GiaKM, @SoLuong, @HienThi, @KhuyenMai, @Home, @SPHot, @DacDiem, @NgayTao, @NgayCapNhat, @NguoiTao, @SLTruyCap, @Alias, @MaLSP, @MaKho, @SoLuongCK, @SoLuongTam)";

                                using (var command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@MaSP", sanPham.MaSP);
                                    command.Parameters.AddWithValue("@TenSP", sanPham.TenSP);
                                    command.Parameters.AddWithValue("@MoTa", sanPham.MoTa);
                                    command.Parameters.AddWithValue("@ChiTiet", sanPham.ChiTiet);
                                    command.Parameters.AddWithValue("@HinhAnh", sanPham.HinhAnh);
                                    command.Parameters.AddWithValue("@GiaGoc", sanPham.GiaGoc);
                                    command.Parameters.AddWithValue("@GiaBan", sanPham.GiaBan);
                                    command.Parameters.AddWithValue("@GiaKM", sanPham.GiaKM);
                                    command.Parameters.AddWithValue("@SoLuong", sanPham.SoLuong);
                                    command.Parameters.AddWithValue("@HienThi", sanPham.HienThi);
                                    command.Parameters.AddWithValue("@KhuyenMai", sanPham.KhuyenMai);
                                    command.Parameters.AddWithValue("@Home", sanPham.Home);
                                    command.Parameters.AddWithValue("@SPHot", sanPham.SPHot);
                                    command.Parameters.AddWithValue("@NgayTao", sanPham.NgayTao);
                                    command.Parameters.AddWithValue("@NgayCapNhat", sanPham.NgayCapNhat);
                                    command.Parameters.AddWithValue("@NguoiTao", sanPham.NguoiTao);
                                    command.Parameters.AddWithValue("@Alias", sanPham.Alias);
                                    command.Parameters.AddWithValue("@MaLSP", sanPham.MaLSP);
                                    command.Parameters.AddWithValue("@MaKho", sanPham.MaKho);
                                    command.Parameters.AddWithValue("@SLTruyCap", sanPham.SLTruyCap == 0 ? (object)DBNull.Value : sanPham.SLTruyCap);
                                    command.Parameters.AddWithValue("@DacDiem", sanPham.DacDiem == false ? (object)DBNull.Value : sanPham.DacDiem);
                                    command.Parameters.AddWithValue("@SoLuongTam", sanPham.SoLuong);
                                    command.Parameters.AddWithValue("@SoLuongCK", sanPham.SoLuongCK == 0 ? (object)DBNull.Value : sanPham.SoLuongCK);



                                    command.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log exception details here
                                Console.WriteLine($"Error inserting row with MaSP: {sanPham.MaSP}. Exception: {ex.Message}");
                                continue;
                            }
                        }
                    }

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    // Log the exception as needed
                    return Json(new { success = false, message = "An error occurred while processing the file. " + ex.Message });
                }
            }

            return Json(new { success = false, message = "No file uploaded or file is empty." });
        }

        private string GenerateNewMaSP()
        {
            string prefix = "SP";
            string newMaSP = "";

            while (true)
            {
                // Tạo một số ngẫu nhiên từ 10000 đến 99999
                Random rand = new Random();
                int randomNumber = rand.Next(10000, 99999);

                // Kết hợp tiền tố và số ngẫu nhiên để tạo mã mới
                newMaSP = prefix + randomNumber.ToString();

                // Kiểm tra xem mã sản phẩm mới đã tồn tại trong cơ sở dữ liệu hay chưa
                // Nếu chưa tồn tại, thoát khỏi vòng lặp
                if (!MaSPExists(newMaSP))
                {
                    break;
                }
            }

            return newMaSP;
        }

        private bool MaSPExists(string maSP)
        {
            // Thực hiện kiểm tra trong cơ sở dữ liệu xem mã sản phẩm đã tồn tại hay chưa
            // Trả về true nếu mã đã tồn tại, ngược lại trả về false
            // Ví dụ:
            // return dbContext.SanPhams.Any(sp => sp.MaSP == maSP);

            // Trong trường hợp không có cơ sở dữ liệu, bạn có thể tạm thời trả về giá trị ngẫu nhiên cho mục đích thử nghiệm
            Random rand = new Random();
            return rand.Next(0, 2) == 1; // Giả sử có 50% khả năng mã đã tồn tại
        }
        //Search
        public ActionResult Search(string tenSanPham, int? page)
        {
            int pageIndex = page ?? 1;
            int startRowIndex = (pageIndex - 1) * pageSize + 1;

            List<SanPham> items = GetSanPhamListByName(tenSanPham, startRowIndex, pageSize);

            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;
            ViewBag.SearchQuery = tenSanPham;
            return View("Index", items);
        }

        public ActionResult Filter(string maLoaiSP, int? page)
        {
            int pageIndex = page ?? 1;
            int startRowIndex = (pageIndex - 1) * pageSize + 1;

            List<SanPham> items = GetSanPhamListByCategory(maLoaiSP, startRowIndex, pageSize);

            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageIndex;
            ViewBag.SelectedCategory = maLoaiSP;
            return View("Index", items);
        }

        private List<SanPham> GetSanPhamList(int startRowIndex, int pageSize)
        {
            string sqlQuery = $@"
SELECT SanPham.*, LoaiSanPham.TenLSP
FROM SanPham
INNER JOIN LoaiSanPham ON SanPham.MaLSP = LoaiSanPham.MaLSP
ORDER BY SanPham.MaSP
OFFSET {startRowIndex - 1} ROWS
FETCH NEXT {pageSize} ROWS ONLY";

            return ExecuteSanPhamQuery(sqlQuery);
        }
        private readonly string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
        private const int pageSize = 10;

        private List<SanPham> GetSanPhamListByName(string tenSanPham, int startRowIndex, int pageSize)
        {
            string sqlQuery = $@"
SELECT SanPham.*, LoaiSanPham.TenLSP
FROM SanPham
INNER JOIN LoaiSanPham ON SanPham.MaLSP = LoaiSanPham.MaLSP
WHERE SanPham.TenSP LIKE '%' + @TenSP + '%'
ORDER BY SanPham.MaSP
OFFSET {startRowIndex - 1} ROWS
FETCH NEXT {pageSize} ROWS ONLY";

            return ExecuteSanPhamQuery(sqlQuery, new SqlParameter("@TenSP", tenSanPham));
        }

        private List<SanPham> GetSanPhamListByCategory(string maLoaiSP, int startRowIndex, int pageSize)
        {
            string sqlQuery = $@"
SELECT SanPham.*, LoaiSanPham.TenLSP
FROM SanPham
INNER JOIN LoaiSanPham ON SanPham.MaLSP = LoaiSanPham.MaLSP
WHERE SanPham.MaLSP = @MaLSP
ORDER BY SanPham.MaSP
OFFSET {startRowIndex - 1} ROWS
FETCH NEXT {pageSize} ROWS ONLY";

            return ExecuteSanPhamQuery(sqlQuery, new SqlParameter("@MaLSP", maLoaiSP));
        }

        private List<SanPham> ExecuteSanPhamQuery(string sqlQuery, params SqlParameter[] parameters)
        {
            List<SanPham> items = new List<SanPham>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SanPham news = new SanPham();
                            news.MaSP = reader.GetString(reader.GetOrdinal("MaSP"));
                            news.TenSP = reader["TenSP"].ToString();
                            news.MoTa = reader["MoTa"].ToString();
                            news.ChiTiet = reader["ChiTiet"].ToString();
                            news.HinhAnh = reader["HinhAnh"].ToString();
                            if (int.TryParse(reader["SoLuong"].ToString(), out int soLuong))
                            {
                                news.SoLuong = soLuong;
                            }
                            news.MaLSP = reader["TenLSP"].ToString();
                            if (decimal.TryParse(reader["GiaGoc"].ToString(), out decimal giaGoc))
                            {
                                news.GiaGoc = giaGoc;
                            }
                            if (decimal.TryParse(reader["GiaBan"].ToString(), out decimal giaBan))
                            {
                                news.GiaBan = giaBan;
                            }
                            if (decimal.TryParse(reader["GiaKM"].ToString(), out decimal giaKM))
                            {
                                news.GiaKM = giaKM;
                            }
                            if (bool.TryParse(reader["Hienthi"].ToString(), out bool hienThi))
                            {
                                news.HienThi = hienThi;
                            }
                            if (bool.TryParse(reader["KhuyenMai"].ToString(), out bool khuyenMai))
                            {
                                news.KhuyenMai = khuyenMai;
                            }
                            if (bool.TryParse(reader["Home"].ToString(), out bool home))
                            {
                                news.Home = home;
                            }
                            if (bool.TryParse(reader["SPHot"].ToString(), out bool spHot))
                            {
                                news.SPHot = spHot;
                            }
                            if (DateTime.TryParse(reader["NgayTao"].ToString(), out DateTime ngayTao))
                            {
                                news.NgayTao = ngayTao;
                            }
                            if (DateTime.TryParse(reader["NgayCapNhat"].ToString(), out DateTime ngayCapNhat))
                            {
                                news.NgayCapNhat = ngayCapNhat;
                            }
                            items.Add(news);
                        }
                    }
                }
            }

            return items;
        }
    }
}