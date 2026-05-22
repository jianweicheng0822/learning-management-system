FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore (cached layer)
COPY ["Learning Management System/Learning Management System.csproj", "Learning Management System/"]
RUN dotnet restore "Learning Management System/Learning Management System.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "Learning Management System/Learning Management System.csproj" \
    -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Learning Management System.dll"]
