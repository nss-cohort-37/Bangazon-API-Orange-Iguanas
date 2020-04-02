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
    public class UserPaymentTypeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UserPaymentTypeController(IConfiguration config)
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


        // Get all userPaymentTypes from the database
        [HttpGet]
        public async Task<IActionResult> GET()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT Id, AcctNumber, Active, CustomerId, PaymentTypeId
                                        FROM UserPaymentType
                                        ";

                    SqlDataReader reader = cmd.ExecuteReader();
                    var userPaymentTypes = new List<UserPaymentType>();

                    while (reader.Read())
                    {
                        var userPaymentType = new UserPaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            AcctNumber = reader.GetString(reader.GetOrdinal("AcctNumber")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"))
                        };

                        userPaymentTypes.Add(userPaymentType);
                    }
                    reader.Close();

                    return Ok(userPaymentTypes);
                }
            }
        }


        // Get a single userPaymentType by Id from database
        [HttpGet("{id}", Name = "GetUserPaymentType")]
        public async Task<IActionResult> GET([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, AcctNumber, Active, CustomerId, PaymentTypeId
                        FROM UserPaymentType
                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    UserPaymentType userPaymentType = null;

                    if (reader.Read())
                    {
                        userPaymentType = new UserPaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            AcctNumber = reader.GetString(reader.GetOrdinal("AcctNumber")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"))
                        };
                        reader.Close();

                        return Ok(userPaymentType);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }
        }


        // Create productTypes and add them to database
        [HttpPost]
        public async Task<IActionResult> POST([FromBody] UserPaymentType userPaymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        INSERT 
                                        INTO UserPaymentType (AcctNumber, Active, CustomerId, PaymentTypeId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@acctNumber, @active, @customerId, @paymentTypeId)";

                    cmd.Parameters.Add(new SqlParameter("@acctNumber", userPaymentType.AcctNumber));
                    cmd.Parameters.Add(new SqlParameter("@active", userPaymentType.Active));
                    cmd.Parameters.Add(new SqlParameter("@customerId", userPaymentType.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@paymentTypeId", userPaymentType.PaymentTypeId));

                    int newId = (int)cmd.ExecuteScalar();
                    userPaymentType.Id = newId;
                    return CreatedAtRoute("GetUserPaymentType", new { id = newId }, userPaymentType);
                }
            }
        }


        // Update single productType by id from database
        [HttpPut("{id}")]
        public async Task<IActionResult> PUT([FromRoute] int id, [FromBody] UserPaymentType userPaymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                            UPDATE UserPaymentType
                                            SET 
                                            AcctNumber = @acctNumber,
                                            Active = @active,
                                            CustomerId = @customerId,
                                            PaymentTypeId = @paymentTypeId
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@acctNumber", userPaymentType.AcctNumber));
                        cmd.Parameters.Add(new SqlParameter("@active", userPaymentType.Active));
                        cmd.Parameters.Add(new SqlParameter("@customerId", userPaymentType.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@paymentTypeId", userPaymentType.PaymentTypeId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows were effected");
                    }
                }
            }
            catch (Exception)
            {
                if (!UserPaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        //Delete productType by id from database
        [HttpDelete("{id}")]
        public async Task<IActionResult> DELETE([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE 
                                            FROM UserPaymentType
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows were affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!UserPaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // Check to see if productType exists by id in database
        private bool UserPaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, AcctNumber, Active, CustomerId, PaymentTypeId
                        FROM UserPaymentType
                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}