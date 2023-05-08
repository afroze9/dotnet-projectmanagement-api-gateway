FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/ProjectManagement.ApiGateway/ProjectManagement.ApiGateway.csproj", "src/ProjectManagement.ApiGateway/nuget.config", "src/ProjectManagement.ApiGateway/"]
RUN dotnet restore "src/ProjectManagement.ApiGateway/ProjectManagement.ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/ProjectManagement.ApiGateway"
RUN dotnet build "ProjectManagement.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProjectManagement.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectManagement.ApiGateway.dll"]
