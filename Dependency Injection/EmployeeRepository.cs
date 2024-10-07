using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmployeeManagementAPI.Dependency_Injection
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly EmployeeContext employeeContext;

        public EmployeeRepository(EmployeeContext context)
        {
            employeeContext = context;
        }

        public async Task<Employee> GetEmployeeByIdAsync(int id)
        {
            return await employeeContext.Employees.FindAsync(id);
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await employeeContext.Employees.ToListAsync();
        }

        public async Task AddEmployeeAsync(Employee employee)
        {
            await employeeContext.Employees.AddAsync(employee);
            await employeeContext.SaveChangesAsync();
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            employeeContext.Employees.Update(employee);
            await employeeContext.SaveChangesAsync();
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await employeeContext.Employees.FindAsync(id);
            if (employee != null)
            {
                employeeContext.Employees.Remove(employee);
                await employeeContext.SaveChangesAsync();
            }
        }

        // New method to get employee by username 
        public async Task<Employee> GetEmployeeByUserNameAndPasswordAsync(string userName, string password)
        {
            return await employeeContext.Employees
                .FirstOrDefaultAsync(e => e.userName == userName && e.Password == password);
        }
    }
}
