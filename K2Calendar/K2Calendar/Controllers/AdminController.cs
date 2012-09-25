using System;
using System.Data.SqlClient;
using System.Web.Mvc;
using System.Web.Security;
using K2Calendar.Models;
using System.Collections.Generic;

namespace K2Calendar.Controllers
{
    public class AdminController : Controller
    {
        AppDbContext dbContext = new AppDbContext();
        private const int PAGE_SIZE = 50;

        //
        // GET: /Admin/
        //
        //public ActionResult Index()
        //{
            //return View();
        //}

        // GET: /Admin/Users/50
        //TODO: sort by parameter
        public ActionResult Users(int pageNumber = 1)
        {
            List<UserInfoSummaryModel> userSummaries = new List<UserInfoSummaryModel>();
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.
                                ConnectionStrings["ApplicationServices"].ConnectionString))
            {
                conn.Open();
                //TODO: if we get over a million rows we should convert to :
                //http://www.4guysfromrolla.com/webtech/042606-1.shtml
                using (SqlCommand command = new SqlCommand(@";WITH Results_CTE AS
                                                                (
                                                                    SELECT
                                                                        aspnet_Users.UserName, aspnet_Users.UserId, 
                                                                        aspnet_Membership.LoweredEmail, aspnet_Membership.LastLoginDate,
                                                                        UserInfoModels.FirstName, UserInfoModels.LastName, UserInfoModels.Id,
                                                                        ROW_NUMBER() OVER (ORDER BY aspnet_Users.UserName) AS RowNum,
                                                                        Count(*) OVER() As TotalRows 
                                                                    FROM aspnet_Users INNER JOIN aspnet_Membership ON aspnet_Users.UserId = aspnet_Membership.UserId 
                                                                    INNER JOIN UserInfoModels ON aspnet_Users.UserId = UserInfoModels.MembershipId 
                                                                )
                                                                SELECT *
                                                                FROM Results_CTE
                                                                WHERE RowNum >= @Offset
                                                                AND RowNum < @Offset + @Limit", conn))
                {
                    command.Parameters.Add(new SqlParameter("@Offset", (pageNumber - 1) * PAGE_SIZE));
                    command.Parameters.Add(new SqlParameter("@Limit", pageNumber * PAGE_SIZE));

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var userSummary = new UserInfoSummaryModel
                            {
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Username = reader["UserName"].ToString(),
                                Email = reader["LoweredEmail"].ToString(),
                                MembershipId = reader["UserId"].ToString(),
                                UserId = Convert.ToInt32(reader["Id"]),
                                LastLogin = DateTime.Parse(reader["LastLoginDate"].ToString())                                
                            };
                            //TODO: could optimize this block
                            userSummary.Roles = Roles.GetRolesForUser(userSummary.Username);
                            userSummaries.Add(userSummary);
                            ViewBag.TotalRows = Convert.ToInt32(reader["TotalRows"]);
                        }                    
                    }
                }
            }
            ViewBag.PageSize = PAGE_SIZE;
            ViewBag.PageNum = pageNumber;
            ViewBag.TotalPages = Math.Ceiling((double)ViewBag.TotalRows / PAGE_SIZE);
            return View(userSummaries);
        }
    }
}
