# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY Frontend/FitLifeFitness.csproj Frontend/
COPY Shared/ Shared/
RUN dotnet restore Frontend/FitLifeFitness.csproj

# Copy everything else and build
COPY Frontend/ Frontend/
WORKDIR /app/Frontend
RUN dotnet publish -c Release -o out

# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/Frontend/out .

# Expose port 8080
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "FitLifeFitness.dll"]