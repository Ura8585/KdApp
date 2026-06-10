FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта из подпапки Kdbapp
COPY Kdbapp/*.csproj ./Kdbapp/
# Восстанавливаем пакеты, указывая просто на папку (универсальный поиск csproj)
RUN dotnet restore "Kdbapp/"

# Копируем все остальные исходники
COPY . .
WORKDIR "/src/Kdbapp"
# Собираем проект, также указывая на папку
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5237

ENTRYPOINT ["dotnet", "Kdbapp.dll"]
