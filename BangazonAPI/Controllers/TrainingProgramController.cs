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
    public class TrainingProgramsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TrainingProgramsController(IConfiguration config)
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

        


        // GET all upcomming trainingPrograms from the database !!WORKING

        [HttpGet]
        public async Task<IActionResult> GET()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                                        SELECT Id, Name, StartDate, EndDate, MaxAttendees
                                        FROM TrainingProgram
                                        WHERE 1=1
                                        ";

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        TrainingProgram trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                            Employees = new List<Employee>()
                        };

                        trainingPrograms.Add(trainingProgram);
                    }
                    reader.Close();

                    return Ok(trainingPrograms);
                }
            }
        }


        // GET a single trainingProgram by Id from database !!WORKING

        [HttpGet("{id}", Name = "GetTrainingProgram")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT tp.Id as TrainingProgramID, tp.Name, tp.StartDate, tp.EndDate, tp.MaxAttendees, et.Id as EmployeeTrainingId, et.EmployeeId, et.TrainingProgramId, e.Id AS EmployeeId, e.FirstName, e.LastName, e.DepartmentId, e.Email, e.IsSupervisor, e.ComputerId FROM TrainingProgram tp
                        LEFT JOIN EmployeeTraining et ON et.TrainingProgramId = tp.Id
                        LEFT JOIN Employee e ON et.EmployeeId = e.Id
                        WHERE tp.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    TrainingProgram trainingProgram = null;

                    while (reader.Read())
                    {
                        if (trainingProgram == null)
                        {
                            trainingProgram = new TrainingProgram
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("TrainingProgramID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                                Employees = new List<Employee>()
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))
                        {
                            Employee employeeToAdd = new Employee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                            };

                            if (!reader.IsDBNull(reader.GetOrdinal("EmployeeTrainingId")))
                            {
                                employeeToAdd.Id = reader.GetInt32(reader.GetOrdinal("EmployeeId"));
                            }
                            trainingProgram.Employees.Add(employeeToAdd);
                        }

                    }
                    reader.Close();
                    return Ok(trainingProgram);
                }
            }
        }


        // POST  -- Add training program to database !!WORKING

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @startDate, @endDate, @maxAttendees)";
                    cmd.Parameters.Add(new SqlParameter("@name", trainingProgram.Name));
                    cmd.Parameters.Add(new SqlParameter("@startDate", trainingProgram.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", trainingProgram.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@maxAttendees", trainingProgram.MaxAttendees));

                    int newId = (int)cmd.ExecuteScalar();
                    trainingProgram.Id = newId;
                    return CreatedAtRoute("GetTrainingProgram", new { id = newId }, trainingProgram);
                }
            }
        }

        // POST  -- Add employee to training program !!NOT WORKING

        [HttpPost]
        [Route("{id}/employees")]
        public async Task<IActionResult> Post([FromBody] Employee employee, [FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@employeeId, @trainingProgramId)";
                    cmd.Parameters.Add(new SqlParameter("@employeeId", employee.Id));
                    cmd.Parameters.Add(new SqlParameter("@trainingProgramId", id));

                    cmd.ExecuteNonQuery();
                    return RedirectToRoute("GetTrainingProgram", new { id = id });
                }
            }
        }
        




        // PUT -- Update single trainingProgram by id from database  !!NOT WORKING

        [HttpPut("{id}")]
        public async Task<IActionResult> PUT([FromRoute] int id, [FromBody] TrainingProgram trainingProgram)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
                                            UPDATE TrainingProgram
                                            SET 
                                            Name = @name,
                                            StartDate = @startDate,
                                            EndDate = @endDate,
                                            MaxAttendees = @maxAttendees,
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", trainingProgram.Name));
                        cmd.Parameters.Add(new SqlParameter("@startDate", trainingProgram.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@endDate", trainingProgram.EndDate));
                        cmd.Parameters.Add(new SqlParameter("@startDate", trainingProgram.StartDate));
                        cmd.Parameters.Add(new SqlParameter("@maxAttendees", trainingProgram.MaxAttendees));

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
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // DELETE -- Remove training program from database !!WORKING

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
                                            FROM TrainingProgram
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
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // Check to see if trainingProgram exists by id in database
        private bool TrainingProgramExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

        // DELETE -- Remove employee from trainingProgram
        [HttpDelete("{id}/employees/{employeeId}")]
        [Route("{id}/employees/{employeeId}")]

        public async Task<IActionResult> DeleteEmployeeFromProgram([FromRoute] int id, [FromRoute] int employeeId)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM EmployeeTraining WHERE TrainingProgramId = @id AND EmployeeId = @employeeId";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@employeeId", employeeId));

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
                if (!TrainingProgramExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // Check to see if trainingProgram exists by id in database
        private bool EmployeeTPExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, Budget
                        FROM TrainingProgram
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}