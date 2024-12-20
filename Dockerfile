# Use the ASP.NET base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 7002

# Use the .NET SDK image for build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IdentityProviderMicroservice/IdentityProviderMicroservice.csproj", "IdentityProviderMicroservice/"]
RUN dotnet restore "./IdentityProviderMicroservice/./IdentityProviderMicroservice.csproj"
COPY . .
WORKDIR "/src/IdentityProviderMicroservice"
RUN dotnet build "./IdentityProviderMicroservice.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IdentityProviderMicroservice.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use the ASP.NET base image again for the final stage
FROM base AS final
WORKDIR /app

# Install iputils-ping (requires root access)
USER root
RUN apt-get update && apt-get install -y iputils-ping

# Copy the published app
COPY --from=publish /app/publish .

# Copy the certificate into the container
COPY ["certs/Phonebook.pfx", "/https/Phonebook.pfx"]

# Set environment variables for Kestrel to use the certificate
ENV ASPNETCORE_Kestrel__Endpoints__Https__Url=https://+:7002
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/Phonebook.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPassword=luka004leho1
# Set the entry point for the container
ENTRYPOINT ["dotnet", "IdentityProviderMicroservice.dll"]
