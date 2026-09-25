# Multi-stage Dockerfile for Quillow .NET 10 Web API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and project files for layer caching
COPY src/Quillow.Core/Quillow.Core.csproj src/Quillow.Core/
COPY src/Quillow.Infrastructure/Quillow.Infrastructure.csproj src/Quillow.Infrastructure/
COPY src/Quillow.Services/Quillow.Services.csproj src/Quillow.Services/
COPY src/Quillow.API/Quillow.API.csproj src/Quillow.API/

# Restore dependencies
RUN dotnet restore src/Quillow.API/Quillow.API.csproj

# Copy remaining source code
COPY src/ src/

# Publish Release
WORKDIR /app/src/Quillow.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Ensure uploads directory exists
RUN mkdir -p /app/wwwroot/uploads

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Quillow.API.dll"]
