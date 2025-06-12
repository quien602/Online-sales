using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class BaiVietController : Controller
    {
        // GET: Admin/BaiViet
        public ActionResult Index(int? page)
        {
            List<BAIVIET> items = new List<BAIVIET>();

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
            SELECT *, ROW_NUMBER() OVER (ORDER BY MaBai) AS RowNumber
            FROM BAIVIET
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
                            // Map data from SqlDataReader to News object
                            BAIVIET news = new BAIVIET();
                            news.MaBai = reader.GetString(reader.GetOrdinal("MaBai"));
                            news.TenBai = reader["TenBai"].ToString();
                            news.Mieuta = reader["Mieuta"].ToString();
                            news.ChiTiet = reader["ChiTiet"].ToString();
                            string hienthiValue = reader["Hienthi"].ToString();
                            if (bool.TryParse(hienthiValue, out bool result))
                            {
                                news.Hienthi = result;
                            }
                            else
                            {
                                // Handle the case where the string cannot be parsed as a boolean
                                // For example, set a default value
                                news.Hienthi = false; // Or true, depending on your requirements
                                                      // You could also log the invalid value for further investigation
                                Console.WriteLine($"Invalid value for Hienthi: {hienthiValue}");
                            }
                            news.HinhDD = reader["HinhDD"].ToString();

                            // Map other properties similarly

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
        public ActionResult HienThi(string MaBai)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool isActive = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current HienThi value from the database
                string selectQuery = "SELECT HienThi FROM BAIVIET WHERE MaBai = @MaBai";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaBai", MaBai);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        isActive = (bool)result;
                    }
                }

                // Update the news item in the database
                string updateQuery = "UPDATE BAIVIET SET HienThi = @HienThi WHERE MaBai = @MaBai";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaBai", MaBai);
                    updateCommand.Parameters.AddWithValue("@HienThi", !isActive); // Toggle the value
                    updateCommand.ExecuteNonQuery();
                }
            }

            return Json(new { success = true, HienThi = !isActive });
        }
        public ActionResult Add()
        {
            // Assuming you have a connection string named "ConnectionString" in your web.config
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            // Your SQL query to retrieve ProductCategories
            string query = "SELECT MaDM, TenDM FROM DANHMUC";

            List<SelectListItem> selectListItems = new List<SelectListItem>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string MaDM = reader["MaDM"].ToString();
                            string TenDM = reader["TenDM"].ToString();

                            SelectListItem item = new SelectListItem
                            {
                                Value = MaDM,
                                Text = TenDM
                            };
                            selectListItems.Add(item);
                        }
                    }
                }
            }

            // Create a SelectList from selectListItems
            SelectList selectList = new SelectList(selectListItems, "Value", "Text");

            // Add the SelectList to ViewBag.DanhMuc
            ViewBag.DanhMuc = selectList;

            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Add(BAIVIET model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Open connection before executing command
                                       // Encode the potentially dangerous input from the model

                    string query = "INSERT INTO BAIVIET (MaBai, TenBai, Mieuta, ChiTiet, HinhDD, Alias, MaDM) VALUES (@MaBai, @TenBai, @Mieuta, @ChiTiet, @HinhDD, @Alias, @MaDM)";
                    string maBai = "BV" + GenerateRandomNumber();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaBai", maBai);
                        command.Parameters.AddWithValue("@TenBai", model.TenBai);
                        command.Parameters.AddWithValue("@Mieuta", model.Mieuta);
                        command.Parameters.AddWithValue("@ChiTiet", model.ChiTiet); // Use the encoded value
                        command.Parameters.AddWithValue("@HinhDD", model.HinhDD);
                        model.Alias = Webbanhangonline.Models.Commons.Filter.FilterChar(model.TenBai);
                        command.Parameters.AddWithValue("@Alias", model.Alias);
                        command.Parameters.AddWithValue("@MaDM", model.MaDM);

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
            int randomNumber = random.Next(00000, 99999); // Sinh số ngẫu nhiên từ 10000 đến 99999
            return randomNumber.ToString();
        }

        public ActionResult Edit(string MaBai)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            BAIVIET item = new BAIVIET();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM BAIVIET WHERE MaBai = @MaBai";

                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@MaBai", MaBai);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item.MaBai = MaBai;
                        item.TenBai = reader["TenBai"].ToString();
                        item.Mieuta = reader["Mieuta"].ToString();
                        item.HinhDD = reader["HinhDD"].ToString();
                        item.ChiTiet = reader["ChiTiet"].ToString();
                        item.MaDM = reader["MaDM"].ToString();
                        if (!reader.IsDBNull(reader.GetOrdinal("Hienthi")))
                        {
                            string hienthiValue = reader["Hienthi"].ToString();
                            bool hienthiBool;
                            if (bool.TryParse(hienthiValue, out hienthiBool))
                            {
                                item.Hienthi = hienthiBool;
                            }
                            else
                            {
                                // Handle the case where the string cannot be parsed to a boolean.
                                // You may want to provide a default value or handle the error accordingly.
                            }
                        }
                        else
                        {
                            // Handle DBNull case if necessary
                        }

                    }
                    else
                    {
                        return HttpNotFound();
                    }
                }
            }
            // Your SQL query to retrieve ProductCategories
            string query = "SELECT MaDM, TenDM FROM DANHMUC";

            List<SelectListItem> selectListItems = new List<SelectListItem>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string MaDM = reader["MaDM"].ToString();
                            string TenDM = reader["TenDM"].ToString();

                            SelectListItem item1 = new SelectListItem
                            {
                                Value = MaDM,
                                Text = TenDM
                            };
                            selectListItems.Add(item1);
                        }
                    }
                }
            }

            // Create a SelectList from selectListItems
            SelectList selectList = new SelectList(selectListItems, "Value", "Text");

            // Add the SelectList to ViewBag.DanhMuc
            ViewBag.DanhMuc = selectList;
            return View(item);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BAIVIET model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch MaDM from the DANHMUC table based on some condition (e.g., DanhMucId)
                    string fetchMaDMQuery = "SELECT MaBai FROM BAIVIET WHERE MaBai = @MaBai";
                    SqlCommand fetchMaDMCommand = new SqlCommand(fetchMaDMQuery, connection);
                    fetchMaDMCommand.Parameters.AddWithValue("@MaBai", model.MaBai); // Assuming DanhMucId is the identifier to fetch MaDM

                    // Execute scalar to fetch MaDM
                    string maDM = fetchMaDMCommand.ExecuteScalar() as string;

                    // Check if maDM is not null or empty before proceeding
                    if (!string.IsNullOrEmpty(maDM))
                    {
                        // Update BAIVIET table using MaDM
                        string sqlQuery = "UPDATE BAIVIET SET TenBai = @TenDM, Mieuta = @Mieuta, HinhDD = @HinhDD, ChiTiet = @ChiTiet, Alias = @Alias WHERE MaBai = @MaBai";
                        SqlCommand command = new SqlCommand(sqlQuery, connection);
                        command.Parameters.AddWithValue("@MaBai", model.MaBai);
                        command.Parameters.AddWithValue("@TenDM", model.TenBai);
                        command.Parameters.AddWithValue("@Mieuta", model.Mieuta);
                        command.Parameters.AddWithValue("@HinhDD", model.HinhDD);
                        command.Parameters.AddWithValue("@ChiTiet", model.ChiTiet);
                        model.Alias = Webbanhangonline.Models.Commons.Filter.FilterChar(model.TenBai);
                        command.Parameters.AddWithValue("@Alias", model.Alias);

                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        // Handle the case where MaDM is null or empty from the DANHMUC table
                    }
                }

                return RedirectToAction("Index");
            }

            return View(model);
        }
        public ActionResult Delete(string MaBai)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "DELETE FROM BAIVIET WHERE MaBai = @MaBai";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaBai", MaBai);

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
                                // Giả sử MaBai là kiểu chuỗi trong cơ sở dữ liệu
                                string MaBai = id.Trim(); // Xóa khoảng trắng ở hai đầu nếu có
                                if (!string.IsNullOrEmpty(MaBai))
                                {
                                    string sqlQuery = "DELETE FROM BAIVIET WHERE MaBai LIKE @MaBai";
                                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                                    {
                                        command.Parameters.AddWithValue("@MaBai", MaBai);
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


    }
}