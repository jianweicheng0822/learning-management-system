# Multi-stage build: SDK image for building, slim ASP.NET image for running

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj first for layer caching — only re-restores when dependencies change
COPY ["Learning Management System/Learning Management System.csproj", "Learning Management System/"]
RUN dotnet restore "Learning Management System/Learning Management System.csproj"

# Copy source code and publish a self-contained release build
COPY . .
RUN dotnet publish "Learning Management System/Learning Management System.csproj" \
    -c Release -o /app/publish --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80

# Force production config and listen on port 80
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Copy only the published output from the build stage
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Learning Management System.dll"]
