FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/ProjectManagement.ApiGateway/ProjectManagement.ApiGateway.csproj", "./"]
COPY ["src/ProjectManagement.ApiGateway/nuget.config", "./"]
RUN dotnet restore --configfile nuget.config "ProjectManagement.ApiGateway.csproj"

COPY ["src/ProjectManagement.ApiGateway/*", "./"]
RUN dotnet build -c Release -o /app/build --no-restore "ProjectManagement.ApiGateway.csproj"

FROM build as publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false --no-restore "ProjectManagement.ApiGateway.csproj"

FROM base as final
WORKDIR /app
COPY --from=publish /app/publish .
COPY src/ProjectManagement.ApiGateway/Ocelot /app/Ocelot
ENTRYPOINT ["dotnet", "ProjectManagement.ApiGateway.dll"]
