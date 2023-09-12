using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWT_EmployeeDetailsCrud.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
    }

}


//CREATE TABLE Employees (
//    Id INT PRIMARY KEY IDENTITY,
//    Name NVARCHAR(100),
//    ContactNumber NVARCHAR(20)
//);



