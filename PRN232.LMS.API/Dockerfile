# syntax=docker/dockerfile:1
# ─────────────────────────────────────────────────────────────
# Stage 1: Build
# ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first (better Docker layer caching)
COPY ["PRN232.LMS.API/PRN232.LMS.API.csproj", "PRN232.LMS.API/"]
COPY ["PRN232.LMS.Services/PRN232.LMS.Services.csproj", "PRN232.LMS.Services/"]
COPY ["PRN232.LMS.Repositories/PRN232.LMS.Repositories.csproj", "PRN232.LMS.Repositories/"]

RUN dotnet restore "PRN232.LMS.API/PRN232.LMS.API.csproj"

# Copy all source
COPY . .

WORKDIR /src/PRN232.LMS.API
RUN dotnet build "PRN232.LMS.API.csproj" -c Release -o /app/build

# ─────────────────────────────────────────────────────────────
# Stage 2: Publish
# ─────────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "PRN232.LMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ─────────────────────────────────────────────────────────────
# Stage 3: Runtime (smaller image)
# ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthchecks if needed
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "PRN232.LMS.API.dll"]
