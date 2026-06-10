FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем ВСЁ, что есть (самый надёжный способ для начала)
COPY . .

# Восстанавливаем
RUN dotnet restore "Kdbapp/Kdbapp.csproj"

# Публикуем
WORKDIR /src/Kdbapp
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5237
ENTRYPOINT ["dotnet", "Kdbapp.dll"]