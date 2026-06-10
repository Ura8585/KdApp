FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Создаем и переходим в папку бэкенда внутри контейнера
WORKDIR /src/Kdbapp

# Копируем .csproj файл из локальной папки Kdbapp
COPY Kdbapp/*.csproj ./
RUN dotnet restore

# Копируем остальные исходники бэкенда
COPY Kdbapp/ ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5237

ENTRYPOINT ["dotnet", "Kdbapp.dll"]
