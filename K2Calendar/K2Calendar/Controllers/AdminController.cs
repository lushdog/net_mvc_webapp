using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using K2Calendar.Models;
using System.Data.SqlClient;
using System.Web.Security;

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
            UserInfoSummaryModel userSummary;
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.
                                ConnectionStrings["ApplicationServices"].ConnectionString))
            {
                conn.Open();
                using (SqlCommand command = new SqlCommand(@";WITH Results_CTE AS
                                                                (
                                                                    SELECT
                                                                        aspnet_Users.UserName, aspnet_Users.UserId, 
                                                                        aspnet_Membership.LoweredEmail, aspnet_Membership.LastLoginDate,
                                                                        UserInfoModels.FirstName, UserInfoModels.LastName, UserInfoModels.Id,
                                                                        ROW_NUMBER() OVER (ORDER BY aspnet_Users.UserName) AS RowNum
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
                        reader.Read();
                        userSummary = new UserInfoSummaryModel { FirstName = reader["FirstName"].ToString(), 
                                                                LastName = reader["LastName"].ToString(), 
                                                                Username = reader["UserName"].ToString(), 
                                                                Email = reader["LoweredEmail"].ToString(),
                                                                MembershipId = reader["UserId"].ToString(),
                                                                UserId = Convert.ToInt32(reader["Id"]),
                                                                LastLogin = DateTime.Parse(reader["LastLoginDate"].ToString())
                                                                };
                        userSummary.Roles = Roles.GetRolesForUser(userSummary.Username);
                    }
                }
            }            
            return View();
        }
    }
}
