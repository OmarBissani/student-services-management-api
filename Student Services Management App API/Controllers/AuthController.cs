﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Student_Services_Management_App_API.DAL;
using Student_Services_Management_App_API.Dtos;
using Student_Services_Management_App_API.Models;

namespace Student_Services_Management_App_API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly DatabaseContext dbContext;

    public AuthController(DatabaseContext context)
    {
        dbContext = context;
    }

    [HttpPost("students")]
    public async Task<ActionResult<Student>> SignInAsStudent([FromBody] SignInDto signIn)
    {
        var student = AuthenticateStudent(signIn);

        if (student == null)
            return NotFound();

        var token = GenerateToken(student);
        return Ok(token);
    }

    [HttpPost("admins")]
    public async Task<ActionResult<Student>> SignInAsAdmin([FromBody] SignInDto signIn)
    {
        var admin = AuthenticateAdmin(signIn);

        if (admin == null)
            return NotFound();

        var token = GenerateToken(admin);

        return Ok();
    }

    private static string GenerateToken(Student student)
    {
        var claims = new[]
        {
            new Claim("FirstName", student.FirstName),
            new Claim("LastName", student.LastName),
            new Claim("Email", student.Email),
            new Claim("StudentId", student.StudentId.ToString()),
            new Claim("Gender", student.Gender.ToString()),
            new Claim("IsDorms", student.IsDorms.ToString()),
            new Claim(ClaimTypes.Role, "Student")
        };

        return GenerateToken(claims);
    }

    private static string GenerateToken(Admin admin)
    {
        var claims = new[]
        {
            new Claim("FirstName", admin.FirstName),
            new Claim("LastName", admin.LastName),
            new Claim("Email", admin.Email),
            new Claim(ClaimTypes.Role, "Admin")
        };

        return GenerateToken(claims);
    }

    private static string GenerateToken(Claim[] claims)
    {
        var jwtKey = Environment.GetEnvironmentVariable("ASPNETCORE_JWTKEY");
        var jwtIssuer = Environment.GetEnvironmentVariable("ASPNETCORE_JWTISSUER");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            jwtIssuer,
            jwtIssuer,
            claims,
            expires: DateTime.Now.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private Student? AuthenticateStudent(SignInDto signIn)
    {
        var student = DataAccessLayer.GetStudentByEmail(dbContext, signIn.Email);
        if (student != null &&
            BCrypt.Net.BCrypt.Verify(signIn.Password, student.HashedPassword))
            return student;

        return null;
    }

    private Admin? AuthenticateAdmin(SignInDto signIn)
    {
        var admin = DataAccessLayer.GetAdminByEmail(dbContext, signIn.Email);
        if (admin != null && BCrypt.Net.BCrypt.Verify(signIn.Password, admin.HashedPassword))
            return admin;

        return null;
    }
}