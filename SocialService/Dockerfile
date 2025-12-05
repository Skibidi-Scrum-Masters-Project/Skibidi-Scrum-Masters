FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["SocialService/SocialService.csproj", "SocialService/"]
COPY ["Shared/Shared.Models/Shared.Models.csproj", "Shared/Shared.Models/"]

# Restore dependencies
RUN dotnet restore "SocialService/SocialService.csproj"

# Copy source code
COPY SocialService/ SocialService/
COPY Shared/ Shared/

# Build the application
WORKDIR "/src/SocialService"
RUN dotnet build "SocialService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SocialService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SocialService.dll"]