# Базовый образ для runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Создание директории для DataProtection-Keys
RUN mkdir -p /root/.aspnet/DataProtection-Keys \
    && chmod -R 700 /root/.aspnet/DataProtection-Keys

# Сборка проекта
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["INAPI/INAPI.csproj", "INAPI/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Storage/Storage.csproj", "Storage/"]
RUN dotnet restore "INAPI/INAPI.csproj"
COPY . . 
WORKDIR "/src/INAPI"
RUN dotnet build "INAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Публикация проекта
FROM build AS publish
RUN dotnet publish "INAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "INAPI.dll"]