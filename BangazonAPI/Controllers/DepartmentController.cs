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
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
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


        // Get all departments from the database
        [HttpGet]
        public async Task<IActionResult> GET()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT Id, Name, Budget
                                        FROM Department
                                        ";

                    SqlDataReader reader = cmd.ExecuteReader();
                    var departments = new List<Department>();

                    while (reader.Read())
                    {
                        var department = new Department
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                            Employees = new List<Employee>()
                        };

                        departments.Add(department);
                    }
                    reader.Close();

                    return Ok(departments);
                }
            }
        }


        // Get a single department by Id from database
        [HttpGet("{id}", Name = "GetDepartment")]
        public async Task<IActionResult> GET(
            [FromRoute] int id,
            [FromQuery] string include
            )
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT d.Id, d.Name, d.Budget, e.Id, e.FirstName, e.LastName, e.DepartmentId, e.Email, e.IsSupervisor, e.ComputerId
                        FROM Department d
                        LEFT JOIN Employee e
                        ON d.Id = e.DepartmentId
                        WHERE d.Id = @id
                       ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Department department = null;

                    while (reader.Read())
                    {

                        if (department == null)
                        {
                            department = new Department
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Budget = reader.GetInt32(reader.GetOrdinal("Budget")),
                                Employees = new List<Employee>()
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("DepartmentId")))
                        {
                            if (include == "employees")
                            {
                                department.Employees.Add(new Employee()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                    DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                    Email = reader.GetString(reader.GetOrdinal("Email")),
                                    ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId")),
                                    IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                                });
                            }
                        }

                    }
                    reader.Close();

                    return Ok(department);
                }
            }
        }


        // Create departments and add them to database
        [HttpPost]
        public async Task<IActionResult> POST([FromBody] Department department)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        INSERT 
                                        INTO Department (Name, Budget)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @budget)
                                        ";

                    cmd.Parameters.Add(new SqlParameter("@name", department.Name));
                    cmd.Parameters.Add(new SqlParameter("@budget", department.Budget));

                    int newId = (int)cmd.ExecuteScalar();
                    department.Id = newId;
                    return CreatedAtRoute("GetDepartment", new { id = newId }, department);
                }
            }
        }


        // Update single department by id from database
        [HttpPut("{id}")]
        public async Task<IActionResult> PUT([FromRoute] int id, [FromBody] Department department)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                            UPDATE Department
                                            SET 
                                            Name = @name,
                                            Budget = @budget
                                            WHERE Id = @id
                                            ";

                        cmd.Parameters.Add(new SqlParameter("@name", department.Name));
                        cmd.Parameters.Add(new SqlParameter("@budget", department.Budget));
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
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        //Delete department by id from database
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
                        cmd.CommandText = @"
                                            DELETE 
                                            FROM Department
                                            WHERE Id = @id
                                            ";

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
                if (!DepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // Check to see if department exists by id in database
        private bool DepartmentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Budget
                        FROM Department
                        WHERE Id = @id
                        ";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}