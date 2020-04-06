using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueReportController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RevenueReportController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }


        // Get all productTypes from the database
        [HttpGet]
        public async Task<IActionResult> GET()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT prodType.[Name], prodType.Id, SUM(p.Price) as TotalRevenue
                                        FROM OrderProduct orderProd
                                        LEFT JOIN Product p
                                        ON p.Id = orderProd.ProductId
                                        FULL OUTER JOIN ProductType prodType
                                        ON p.ProductTypeId = prodType.Id
                                        GROUP BY prodType.[Name], prodType.Id
                                        ";

                    SqlDataReader reader = cmd.ExecuteReader();
                    var revenueReports = new List<RevenueReport>();

                    RevenueReport report = null;

                    while (reader.Read())
                    {
                        report = new RevenueReport()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            ProductType = reader.GetString(reader.GetOrdinal("Name"))
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("TotalRevenue")))
                        {
                            report.TotalRevenue = reader.GetDecimal(reader.GetOrdinal("TotalRevenue"));
                        }
                        else
                        {
                            report.TotalRevenue = 0;
                        }

                        revenueReports.Add(report);
                    }
                    reader.Close();

                    return Ok(revenueReports);
                }
            }
        }
    }
}