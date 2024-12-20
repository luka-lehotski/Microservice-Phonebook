# Use the .NET ASP.NET runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7004

# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
RUN dotnet restore "./ApiGateway/ApiGateway.csproj"
COPY . .
WORKDIR "/src/ApiGateway"
RUN dotnet build "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use the base image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install curl and set up the certificate
USER root
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

COPY ["certs/Phonebook.pfx", "/https/Phonebook.pfx"]

# Set environment variables for Kestrel to use the certificate
ENV ASPNETCORE_Kestrel__Endpoints__Https__Url=https://+:7004
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/Phonebook.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPassword=luka004leho1

ENTRYPOINT ["dotnet", "ApiGateway.dll"]

