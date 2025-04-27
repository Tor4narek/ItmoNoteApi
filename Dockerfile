# Базовый образ для runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

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

# Установка переменных окружения для JWT
ENV JWT_SECRET_KEY=${JWT_SECRET_KEY}
ENV JWT_ISSUER=${JWT_ISSUER}
ENV JWT_AUDIENCE=${JWT_AUDIENCE}
ENV JWT_EXPIRY_IN_MINUTES=${JWT_EXPIRY_IN_MINUTES}
ENV TELEGRAM_BOT_TOKEN=${TELEGRAM_BOT_TOKEN}
ENV MARKDOWN_PROD_PATH=${MARKDOWN_PROD_PATH}
ENV LOGGING__LOGLEVEL__DEFAULT=${LOGGING__LOGLEVEL__DEFAULT}
ENV LOGGING__LOGLEVEL__MICROSOFT_ASPNETCORE=${LOGGING__LOGLEVEL__MICROSOFT_ASPNETCORE}
ENV DEFAULT_CONNECTION=${DEFAULT_CONNECTION}
ENV ALLOWEDHOSTS=${ALLOWEDHOSTS}

ENTRYPOINT ["dotnet", "INAPI.dll"]
