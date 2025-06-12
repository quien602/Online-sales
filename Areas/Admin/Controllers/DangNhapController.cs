using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Webbanhangonline.Areas.Admin.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

        // GET: Admin/DangNhap
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Check()
        {
            string username = HttpContext.Request.Form["username"];
            string password = HttpContext.Request.Form["password"];

            string connectionString = @"Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"
        SELECT tk.TenDangNhap, STRING_AGG(q.MaQuyen, ',') AS Roles
        FROM TaiKhoan tk 
        JOIN CTQuyen ctq ON ctq.MaTaiKhoan = tk.MaTaiKhoan 
        JOIN Quyen q ON q.MaQuyen = ctq.MaQuyen
        WHERE tk.TenDangNhap = @username
        AND tk.MatKhau = @password
        GROUP BY tk.TenDangNhap";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                conn.Open();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    // Login successful, set cookies
                    string roles = dt.Rows[0]["Roles"].ToString();
                    HttpCookie userCookie = new HttpCookie("User", username);
                    HttpCookie rolesCookie = new HttpCookie("Roles", roles);

                    userCookie.Expires = DateTime.Now.AddHours(1);
                    rolesCookie.Expires = DateTime.Now.AddHours(1);

                    Response.Cookies.Add(userCookie);
                    Response.Cookies.Add(rolesCookie);

                    return RedirectToAction("Index", "HomeAdmin");
                }
                else
                {
                    // Login failed
                    ViewBag.ErrorMessage = "Invalid username or password.";
                    return View("Index");
                }
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Session.Abandon(); // Nếu có sử dụng session
            return RedirectToAction("Index", "HomeAdmin");
        }


        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerifyPhoneNumber(string phoneNumber)
        {
            string connectionString = "Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT COUNT(*) FROM TaiKhoan WHERE SoDienThoai = @phoneNumber";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);

                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                conn.Close();

                if (count == 1)
                {
                    TempData["SoDienThoai"] = phoneNumber;
                    return RedirectToAction("ResetPassword");
                }
                else
                {
                    ViewBag.ErrorMessage = "Invalid phone number.";
                    return View("ForgotPassword");
                }
            }
        }

        public ActionResult ResetPassword()
        {
            if (TempData["PhoneNumber"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(string newPassword)
        {
            string phoneNumber = TempData["PhoneNumber"] as string;

            if (string.IsNullOrEmpty(phoneNumber))
            {
                return RedirectToAction("ForgotPassword");
            }

            string connectionString = "Data Source=DESKTOP-0O6CEBG;Initial Catalog=Quanlybanhangonline;Integrated Security=True;";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "UPDATE TaiKhoan SET MatKhau = @newPassword WHERE SoDienThoai = @phoneNumber";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@newPassword", newPassword);
                cmd.Parameters.AddWithValue("@phoneNumber", phoneNumber);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return RedirectToAction("Index", "DangNhap");
        }
    }
}
