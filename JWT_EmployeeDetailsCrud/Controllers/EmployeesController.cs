using JWT_EmployeeDetailsCrud.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly IConfiguration _configuration;
    //private readonly string _connectionString;

    private const string _connectionString = "Server=(LocalDB)\\MSSQLLocalDB;Database=YourDatabaseName;Trusted_Connection=True;MultipleActiveResultSets=true;";


    public EmployeesController(IConfiguration configuration)
    {
        _configuration = configuration;
        //_connectionString = _configuration.GetConnectionString("Server = (LocalDB)\\MSSQLLocalDB; Database = Empls1; Trusted_Connection = True; MultipleActiveResultSets = true;");
    }

    private string GenerateJwtToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationMinutes"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    //[AllowAnonymous]
    //[HttpPost("login")]
    //public IActionResult Login([FromBody] LoginModel model)
    [HttpPost]
    [Route("authenticate")]

    public IActionResult Authenticate(LoginModel model)
    {
        // Add authentication logic here (e.g., validate username and password)

        // Assuming authentication is successful, generate token
        var token = GenerateJwtToken(model.Username);

        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetAllEmployees()
    {
        List<Employee> employees = new List<Employee>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand("SELECT * FROM Employees", connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            ContactNumber = reader["ContactNumber"].ToString()
                        };
                        employees.Add(employee);
                    }
                }
            }
        }

        return Ok(employees);
    }

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetEmployeeById(int id)
    {
        Employee employee = null;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand("SELECT * FROM Employees WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        employee = new Employee
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            ContactNumber = reader["ContactNumber"].ToString()
                        };
                    }
                }
            }
        }

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    [Authorize]
    [HttpPost]
    public IActionResult AddEmployee([FromBody] Employee employee)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand("INSERT INTO Employees (Name, ContactNumber) VALUES (@Name, @ContactNumber); SELECT SCOPE_IDENTITY();", connection))
            {
                command.Parameters.AddWithValue("@Name", employee.Name);
                command.Parameters.AddWithValue("@ContactNumber", employee.ContactNumber);

                int newEmployeeId = Convert.ToInt32(command.ExecuteScalar());

                employee.Id = newEmployeeId;
            }
        }

        return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, employee);
    }

    [Authorize]
    [HttpPut("{id}")]
    public IActionResult UpdateEmployee(int id, [FromBody] Employee updatedEmployee)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand("UPDATE Employees SET Name = @Name, ContactNumber = @ContactNumber WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Name", updatedEmployee.Name);
                command.Parameters.AddWithValue("@ContactNumber", updatedEmployee.ContactNumber);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound();
                }
            }
        }

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public IActionResult DeleteEmployee(int id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (SqlCommand command = new SqlCommand("DELETE FROM Employees WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound();
                }
            }
        }

        return NoContent();
    }
}
