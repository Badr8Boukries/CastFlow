using System;

namespace CastFlow.Api.Dtos.Response
{
    public enum AuthenticatedUserType { Talent, Admin } 

    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public long? UserId { get; set; }
        public AuthenticatedUserType? UserType { get; set; } 
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }

        public AuthResponseDto() { }
        public AuthResponseDto(bool isSuccess, string message) { IsSuccess = isSuccess; Message = message; }
        public AuthResponseDto(string message, string token, long userId, AuthenticatedUserType userType, string firstName, string lastName, string email)
        { IsSuccess = true; Message = message; Token = token; UserId = userId; UserType = userType; FirstName = firstName; LastName = lastName; Email = email; }
    }
}