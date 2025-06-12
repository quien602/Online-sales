using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Webbanhangonline.Models.EF;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class TinTucController : Controller
    {
        // GET: Admin/TinTuc
        public ActionResult Index(int? page)
        {
            List<TINTUC> items = new List<TINTUC>();

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
            SELECT *, ROW_NUMBER() OVER (ORDER BY MaTin) AS RowNumber
            FROM TINTUC
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
                            TINTUC news = new TINTUC();
                            news.MaTin = reader.GetString(reader.GetOrdinal("MaTin"));
                            news.TenTin = reader["TenTin"].ToString();
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
        public ActionResult HienThi(string MaTin)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            bool isActive = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Retrieve the current HienThi value from the database
                string selectQuery = "SELECT HienThi FROM TINTUC WHERE MaTin = @MaTin";
                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@MaTin", MaTin);
                    object result = selectCommand.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        isActive = (bool)result;
                    }
                }

                // Update the news item in the database
                string updateQuery = "UPDATE TINTUC SET HienThi = @HienThi WHERE MaTin = @MaTin";
                using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@MaTin", MaTin);
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
        public ActionResult Add(TINTUC model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open(); // Open connection before executing command
                                       // Encode the potentially dangerous input from the model

                    string query = "INSERT INTO TINTUC (MaTin, TenTin, Mieuta, ChiTiet, HinhDD, Alias, MaDM) VALUES (@MaTin, @TenTin, @Mieuta, @ChiTiet, @HinhDD, NULL, @MaDM)";
                    string MaTin = "TT" + GenerateRandomNumber();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MaTin", MaTin);
                        command.Parameters.AddWithValue("@TenTin", model.TenTin);
                        command.Parameters.AddWithValue("@Mieuta", model.Mieuta);
                        command.Parameters.AddWithValue("@ChiTiet", model.ChiTiet); // Use the encoded value
                        command.Parameters.AddWithValue("@HinhDD", model.HinhDD);
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

        public ActionResult Edit(string MaTin)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            TINTUC item = new TINTUC();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM TINTUC WHERE MaTin = @MaTin";

                SqlCommand command = new SqlCommand(sqlQuery, connection);
                command.Parameters.AddWithValue("@MaTin", MaTin);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        item.MaTin = MaTin;
                        item.TenTin = reader["TenTin"].ToString();
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
        public ActionResult Edit(TINTUC model)
        {
            if (ModelState.IsValid)
            {
                string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Fetch MaDM from the DANHMUC table based on some condition (e.g., DanhMucId)
                    string fetchMaDMQuery = "SELECT MaDM FROM DANHMUC WHERE MaDM = @MaDM";
                    SqlCommand fetchMaDMCommand = new SqlCommand(fetchMaDMQuery, connection);
                    fetchMaDMCommand.Parameters.AddWithValue("@MaDM", model.MaDM); // Assuming DanhMucId is the identifier to fetch MaDM

                    // Execute scalar to fetch MaDM
                    string maDM = fetchMaDMCommand.ExecuteScalar() as string;

                    // Check if maDM is not null or empty before proceeding
                    if (!string.IsNullOrEmpty(maDM))
                    {
                        // Update TINTUC table using MaDM
                        string sqlQuery = "UPDATE TINTUC SET TenTin = @TenDM, Mieuta = @Mieuta, HinhDD = @HinhDD, ChiTiet = @ChiTiet, Alias = NULL, MaDM = @MaDM WHERE MaTin = @MaTin";
                        SqlCommand command = new SqlCommand(sqlQuery, connection);
                        command.Parameters.AddWithValue("@MaTin", model.MaTin);
                        command.Parameters.AddWithValue("@TenDM", model.TenTin);
                        command.Parameters.AddWithValue("@Mieuta", model.Mieuta);
                        command.Parameters.AddWithValue("@HinhDD", model.HinhDD);
                        command.Parameters.AddWithValue("@ChiTiet", model.ChiTiet);
                        command.Parameters.AddWithValue("@MaDM", maDM); // Use the fetched MaDM value

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
        public ActionResult Delete(string MaTin)
        {
            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "DELETE FROM TINTUC WHERE MaTin = @MaTin";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@MaTin", MaTin);

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
                                string MaTin = id.Trim(); // Xóa khoảng trắng ở hai đầu nếu có
                                if (!string.IsNullOrEmpty(MaTin))
                                {
                                    string sqlQuery = "DELETE FROM TINTUC WHERE MaTin LIKE @MaTin";
                                    using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                                    {
                                        command.Parameters.AddWithValue("@MaTin", MaTin);
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