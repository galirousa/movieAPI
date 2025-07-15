# Use the official .NET 8.0 SDK image as the base image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the project file and restore dependencies
COPY movieapi/*.csproj movieapi/
RUN dotnet restore movieapi/movieapi.csproj

# Copy the rest of the application code
COPY movieapi/ movieapi/

# Build the application
WORKDIR /app/movieapi
RUN dotnet publish -c Release -o out

# Use the official .NET 8.0 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build-env /app/movieapi/out .

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Run the application
ENTRYPOINT ["dotnet", "movieapi.dll"]
