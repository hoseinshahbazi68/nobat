FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Nobat.API/Nobat.API.csproj", "src/Nobat.API/"]
COPY ["src/Nobat.Application/Nobat.Application.csproj", "src/Nobat.Application/"]
COPY ["src/Nobat.Domain/Nobat.Domain.csproj", "src/Nobat.Domain/"]
COPY ["src/Nobat.Infrastructure/Nobat.Infrastructure.csproj", "src/Nobat.Infrastructure/"]
RUN dotnet restore "src/Nobat.API/Nobat.API.csproj"
COPY . .
WORKDIR "/src/src/Nobat.API"
RUN dotnet build "Nobat.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Nobat.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Nobat.API.dll"]

