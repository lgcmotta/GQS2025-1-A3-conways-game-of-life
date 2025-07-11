global using Conways.GameOfLife.API.Diagnostics;
global using Conways.GameOfLife.API.Features.CreateBoard;
global using Conways.GameOfLife.API.Features.FinalGeneration;
global using Conways.GameOfLife.API.Features.NextGeneration;
global using Conways.GameOfLife.API.Features.NextGenerations;
global using Conways.GameOfLife.Domain.Exceptions;
global using Conways.GameOfLife.Domain;
global using Conways.GameOfLife.Infrastructure;
global using Conways.GameOfLife.Infrastructure.Factories;
global using Conways.GameOfLife.Infrastructure.Persistence;
global using Conways.GameOfLife.IntegrationTests.Extensions;
global using Conways.GameOfLife.IntegrationTests.Factories;
global using FluentAssertions;
global using FluentValidation;
global using HashidsNet;
global using MediatR;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using System.Net.Http.Json;
global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Mime;
global using System.Text.Json;
global using Testcontainers.PostgreSql;