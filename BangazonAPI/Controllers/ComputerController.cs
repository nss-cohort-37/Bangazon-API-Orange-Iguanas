using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComputerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ComputerController(IConfiguration config)
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

        [HttpGet]
        public async Task<IActionResult> Get(
                  [FromQuery] string available)
        {
            if (available != "true" && available != "false")
            {
                var computers = GetAllComputers(available);
                return Ok(computers);
            }
            else if (available == "true")
            {
                var computers = GetAllAvailableComputers(available);
                return Ok(computers);
            }
            else
            {
                var computers = GetAllUnAvailableComputers(available);
                return Ok(computers);

            }
        }
        private List<Computer> GetAllAvailableComputers([FromQuery] string available)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                     SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model, e.ComputerId
                    FROM Computer c
                    LEFT JOIN Employee e
                    ON e.ComputerId = c.Id";


                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Computer> computers = new List<Computer>();

                    while (reader.Read())
                    {
                        if (reader.IsDBNull(reader.GetOrdinal("ComputerId")))
                        {
                            // Chain another if statement that makes sure ONLY computers with null decomission dates are displaying
                            if (reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                            {

                                Computer computer = new Computer
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                    Make = reader.GetString(reader.GetOrdinal("Make")),
                                    Model = reader.GetString(reader.GetOrdinal("Model"))

                                };

                            //Don't need this shit

                            //if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                            //{
                            //    computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                            //}
                            //else
                            //{
                            //    computer.DecomissionDate = null;
                            //}

                            computers.Add(computer);

                            }
                        }
                    }
                    reader.Close();

                    return computers;
                }
            }
        }
        private List<Computer> GetAllUnAvailableComputers([FromQuery] string available)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                     SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model, e.ComputerId
                    FROM Computer c
                    LEFT JOIN Employee e
                    ON e.ComputerId = c.Id";

                    // INNER JOIN Employee e
                    // ^^^ Don't use this since it doesn't capture the decomissioned computers with no employee assigned


                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Computer> computers = new List<Computer>();

                    while (reader.Read())
                    {
                        // Use an IF statement to get all computers currently assigned to an employee OR!!!! to get all computers with NO employee assigned AND with a decomission date
                        if (!reader.IsDBNull(reader.GetOrdinal("ComputerId")) || reader.IsDBNull(reader.GetOrdinal("ComputerId")) && !reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            Computer computer = new Computer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                                Make = reader.GetString(reader.GetOrdinal("Make")),
                                Model = reader.GetString(reader.GetOrdinal("Model"))
                            };


                            // Don't need this shit
                            //if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                            //{
                            //    computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                            //}
                            //else
                            //{
                            //    computer.DecomissionDate = null;
                            //}

                            computers.Add(computer);
                        }

                    }
                    reader.Close();

                    return computers;
                }
            }
        }
        private List<Computer> GetAllComputers([FromQuery] string available)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model
                    FROM Computer c ";


                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Computer> computers = new List<Computer>();

                    while (reader.Read())
                    {
                        Computer computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model"))

                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                        else
                        {
                            computer.DecomissionDate = null;
                        }
                        computers.Add(computer);
                    }
                    reader.Close();

                    return computers;
                }
            }
        }

        [HttpGet("{id}", Name = "GetComputer")]
        public async Task<IActionResult> Get([FromRoute] int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT c.Id, c.PurchaseDate, c.DecomissionDate, c.Make, c.Model
                    FROM Computer c 
                    WHERE c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", Id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Computer computer = null;

                    if (reader.Read())
                    {
                        computer = new Computer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            PurchaseDate = reader.GetDateTime(reader.GetOrdinal("PurchaseDate")),
                            Make = reader.GetString(reader.GetOrdinal("Make")),
                            Model = reader.GetString(reader.GetOrdinal("Model"))

                        };
                        if (!reader.IsDBNull(reader.GetOrdinal("DecomissionDate")))
                        {
                            computer.DecomissionDate = reader.GetDateTime(reader.GetOrdinal("DecomissionDate"));
                        }
                        else
                        {
                            computer.DecomissionDate = null;
                        }
                    }
                    reader.Close();

                    return Ok(computer);
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Computer computer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = @"INSERT INTO Computer (PurchaseDate, Make, Model)
                                        OUTPUT INSERTED.Id
                                        VALUES (@purchaseDate, @make, @model)";
                    cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));
                    //cmd.Parameters.Add(new SqlParameter("@decomissionDate", computer.DecomissionDate));
                    cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                    cmd.Parameters.Add(new SqlParameter("@model", computer.Model));

                    int newId = (int)cmd.ExecuteScalar();
                    computer.Id = newId;
                    return CreatedAtRoute("GetComputer", new { id = newId }, computer);
                }
            }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Computer computer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Computer
                                            SET PurchaseDate = @purchaseDate, DecomissionDate = @decomissionDate, Make = @make, Model = @model
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@purchaseDate", computer.PurchaseDate));

                        if (computer.DecomissionDate == null)
                        {

                            cmd.Parameters.Add(new SqlParameter("@decomissionDate", DBNull.Value));
                        }
                        else
                        {
                            cmd.Parameters.Add(new SqlParameter("@decomissionDate", computer.DecomissionDate));
                        }

                        cmd.Parameters.Add(new SqlParameter("@make", computer.Make));
                        cmd.Parameters.Add(new SqlParameter("@model", computer.Model));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Computer WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!ComputerExists(id))
                {
                    return NotFound();
                }
                else if (EmployeeIsAssigned(id))
                {
                    return new StatusCodeResult(StatusCodes.Status403Forbidden);
                }
                else
                {
                    throw;
                }
            }
        }

        private bool ComputerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, PurchaseDate, DecomissionDate, Make, Model
                        FROM Computer
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

        private bool EmployeeIsAssigned(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, PurchaseDate, DecomissionDate, Make, Model
                        FROM Computer
                        WHERE Id = @id AND Id IN (SELECT ComputerId FROM Employee)";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
