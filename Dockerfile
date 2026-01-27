# Use the ASP.NET Core runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 443

# Use the SDK image to build and publish the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ifmisIdentity/ifmisIdentity.csproj", "ifmisIdentity/"]
RUN dotnet restore "ifmisIdentity/ifmisIdentity.csproj"
COPY . .
WORKDIR "/src/ifmisIdentity"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Final stage: copy published files to runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ifmisIdentity.dll"]
