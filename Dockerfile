# Use the official .NET 9 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["SampleProject/SampleProject.csproj", "SampleProject/"]
COPY ["SampleProject.Application/SampleProject.Application.csproj", "SampleProject.Application/"]
COPY ["SampleProject.Domain/SampleProject.Domain.csproj", "SampleProject.Domain/"]
COPY ["SampleProject.Infrastructure/SampleProject.Infrastructure.csproj", "SampleProject.Infrastructure/"]
COPY ["SampleProject.Persistence/SampleProject.Persistence.csproj", "SampleProject.Persistence/"]
COPY ["Common.Shared/Common.Shared.csproj", "Common.Shared/"]
COPY ["Common.Options/Common.Options.csproj", "Common.Options/"]

# Restore dependencies
RUN dotnet restore "SampleProject/SampleProject.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/SampleProject"
RUN dotnet build "SampleProject.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "SampleProject.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/Logs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "SampleProject.dll"]
