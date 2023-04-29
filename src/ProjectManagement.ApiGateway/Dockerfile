FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ProjectManagement.ApiGateway.csproj", "api-gateway/src/ProjectManagement.ApiGateway/"]
RUN dotnet restore "api-gateway/src/ProjectManagement.ApiGateway/ProjectManagement.ApiGateway.csproj"
WORKDIR "/src/api-gateway/src/ProjectManagement.ApiGateway"
COPY . .
RUN dotnet build "ProjectManagement.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProjectManagement.ApiGateway.csproj" -c Release -o /app/publish 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectManagement.ApiGateway.dll"]
