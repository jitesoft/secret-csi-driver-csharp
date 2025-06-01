FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CsiSecretProvider/CsiSecretProvider.csproj", "CsiSecretProvider/"]
RUN dotnet restore "CsiSecretProvider/CsiSecretProvider.csproj"
COPY . .
WORKDIR "/src/CsiSecretProvider"
RUN dotnet build "CsiSecretProvider.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CsiSecretProvider.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CsiSecretProvider.dll"]
