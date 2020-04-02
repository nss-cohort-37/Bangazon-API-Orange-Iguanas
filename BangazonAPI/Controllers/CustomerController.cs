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
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
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
        public async Task<IActionResult> Get([FromQuery] string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Id, FirstName, LastName, CreatedDate, Active, Address, City, State, Email, Phone 
                                        FROM Customer
                                        WHERE 1 = 1";

                    if (q != null)
                    {
                        cmd.CommandText += " AND FirstName Like @q ";
                        cmd.CommandText += " OR LastName Like @q";
                        cmd.Parameters.Add(new SqlParameter("@q", "%" + q + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Customer> customers = new List<Customer>();

                    while (reader.Read())
                    {
                        if (reader.GetBoolean(reader.GetOrdinal("Active")) == true)
                        {
                            Customer customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                City = reader.GetString(reader.GetOrdinal("City")),
                                State = reader.GetString(reader.GetOrdinal("State")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone"))
                            };

                            customers.Add(customer);
                        }
                    }

                    reader.Close();

                    return Ok(customers);
                }
            }
        }

        [HttpGet("{id}", Name = "GetCustomer")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromQuery] string include)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT c.Id, c.FirstName, c.LastName, c.CreatedDate, c.Active, c.Address, c.City, c.State, c.Email, c.Phone";

                    if (include == "products")
                    {
                        cmd.CommandText += ", p.Id, p.ProductTypeId, p.CustomerId, p.Price, p.Description, p.Title, p.DateAdded";
                    }

                    cmd.CommandText += @" FROM Customer c ";

                    if (include == "products")
                    {
                        cmd.CommandText += "LEFT JOIN Product p ON p.CustomerId = c.Id ";
                    }

                    cmd.CommandText += " WHERE c.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Customer customer = null;

                    while (reader.Read())
                    {
                        if (customer == null)
                        {
                            customer = new Customer
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                City = reader.GetString(reader.GetOrdinal("City")),
                                State = reader.GetString(reader.GetOrdinal("State")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                Products = new List<Product>()
                            };
                        }

                        if (include == "products")
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal("CustomerId")))
                            {
                                customer.Products.Add(new Product()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                    Description = reader.GetString(reader.GetOrdinal("Description")),
                                    Title = reader.GetString(reader.GetOrdinal("Title")),
                                    DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded"))
                                });
                            }
                        }
                    }
                    reader.Close();

                    return Ok(customer);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Customer customer)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Customer (FirstName, LastName, CreatedDate, Active, Address, City, State, Email, Phone)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstName, @lastName, @createdDate, @active, @address, @city, @state, @email, @phone)";

                    cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastName", customer.LastName));
                    cmd.Parameters.Add(new SqlParameter("@createdDate", DateTime.Now));
                    cmd.Parameters.Add(new SqlParameter("@active", customer.Active));
                    cmd.Parameters.Add(new SqlParameter("@address", customer.Address));
                    cmd.Parameters.Add(new SqlParameter("@city", customer.City));
                    cmd.Parameters.Add(new SqlParameter("@state", customer.State));
                    cmd.Parameters.Add(new SqlParameter("@email", customer.Email));
                    cmd.Parameters.Add(new SqlParameter("@phone", customer.Phone));


                    int newId = (int)cmd.ExecuteScalar();
                    customer.Id = newId;
                    return CreatedAtRoute("GetCustomer", new { id = newId }, customer);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Customer customer)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Customer
                                            SET FirstName = @firstName, LastName = @lastName, CreatedDate = @createdDate, Active = @active, Address = @address, City = @city, State = @state, Email = @email, Phone = @phone
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@firstName", customer.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastName", customer.LastName));
                        cmd.Parameters.Add(new SqlParameter("@createdDate", customer.CreatedDate));
                        cmd.Parameters.Add(new SqlParameter("@active", customer.Active));
                        cmd.Parameters.Add(new SqlParameter("@address", customer.Address));
                        cmd.Parameters.Add(new SqlParameter("@city", customer.City));
                        cmd.Parameters.Add(new SqlParameter("@state", customer.State));
                        cmd.Parameters.Add(new SqlParameter("@email", customer.Email));
                        cmd.Parameters.Add(new SqlParameter("@phone", customer.Phone));


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
                if (!CustomerExists(id))
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
                        cmd.CommandText = @"UPDATE Customer
                                            SET Active = @active
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));
                        cmd.Parameters.Add(new SqlParameter("@active", false));

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
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CustomerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, CreatedDate, Active, Address, City, State, Email, Phone 
                        FROM Customer
                        WHERE Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}
