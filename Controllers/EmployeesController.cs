using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Dependency_Injection;
using EmployeeManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeesController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // GET: api/employees/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        // GET: api/employees
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            return Ok(employees);
        }

        // POST: api/employees
        [HttpPost]
        public async Task<IActionResult> PostEmployee([FromBody] Employee newEmployee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _employeeRepository.AddEmployeeAsync(newEmployee);
            return CreatedAtAction(nameof(GetEmployee), new { id = newEmployee.Id }, newEmployee);
        }

        // PUT: api/employees/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutEmployee(int id, [FromBody] Employee updatedEmployee)
        {
            // Get the username of the currently logged-in user
            var currentUser = User.Identity.Name; // This is the username from the token

            // Check if the employee exists
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Only allow the logged-in user to update their own details
            if (employee.userName != currentUser)
            {
                return Forbid("You are not authorized to update this employee.");
            }

            // Validate the model state
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update fields based on input
            if (!string.IsNullOrEmpty(updatedEmployee.Name))
            {
                employee.Name = updatedEmployee.Name;
            }

            if (!string.IsNullOrEmpty(updatedEmployee.Department))
            {
                employee.Department = updatedEmployee.Department;
            }

            if (!string.IsNullOrEmpty(updatedEmployee.Position))
            {
                employee.Position = updatedEmployee.Position;
            }

            if (updatedEmployee.Salary >= 0)
            {
                employee.Salary = updatedEmployee.Salary;
            }

            // Save changes to the database
            await _employeeRepository.UpdateEmployeeAsync(employee);
            return NoContent();
        }
        // DELETE: api/employees/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            // Get the username of the currently logged-in user
            var currentUser = User.Identity.Name; // This is the username from the token

            // Check if the employee exists
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Only allow the logged-in user to delete their own profile
            if (employee.userName != currentUser)
            {
                return Forbid("You are not authorized to delete this employee.");
            }

            await _employeeRepository.DeleteEmployeeAsync(id);
            return NoContent();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (loginModel == null || string.IsNullOrEmpty(loginModel.UserName) || string.IsNullOrEmpty(loginModel.Password))
            {
                return BadRequest("Invalid login request");
            }

            // Fetch employee by username and password
            var employee = await _employeeRepository.GetEmployeeByUserNameAndPasswordAsync(loginModel.UserName, loginModel.Password);
            if (employee == null)
            {
                return Unauthorized(); // Return unauthorized if employee not found
            }
            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("6mP87jTr2HnB$2p4&Zy9!qXcR5!w8qG7nF3vLk9Sx8Qy3EwJb4Pz1T7Kx6R2A8Z1B5Tj1Fq9R0v8D5Lq2Ue8C7!p9A0g6H8!m");

            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, employee.userName),
    new Claim("employeeId", employee.Id.ToString())
};

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token) });
        }
    }
}
