// Common libraries
global using Common.Shared;
global using Common.Options;

// Core framework
global using MediatR;
global using FluentValidation;
global using AutoMapper;

// ASP.NET Core basics
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.Filters;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.OpenApi.Models;
global using Microsoft.EntityFrameworkCore;

// System basics
global using System.Text;
global using System.Text.Json;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;

// Logging
global using Serilog;
global using Serilog.Events;
global using Serilog.Formatting.Json;

// Health checks
global using Microsoft.Extensions.Diagnostics.HealthChecks;

// Project interfaces
global using SampleProject.Application.Interfaces;
global using SampleProject.Domain.Common;
global using SampleProject.Domain.Responses;

// Aliases for commonly used types
global using Result = SampleProject.Domain.Common.Result;
