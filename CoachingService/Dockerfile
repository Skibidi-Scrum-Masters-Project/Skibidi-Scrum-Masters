FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["CoachingService/CoachingService.csproj", "CoachingService/"]
COPY ["Shared/Shared.Models/Shared.Models.csproj", "Shared/Shared.Models/"]

# Restore dependencies
RUN dotnet restore "CoachingService/CoachingService.csproj"

# Copy source code
COPY CoachingService/ CoachingService/
COPY Shared/ Shared/

# Build the application
WORKDIR "/src/CoachingService"
RUN dotnet build "CoachingService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CoachingService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoachingService.dll"]