FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем из папки Kdbapp файл csproj в папку Kdbapp внутри контейнера
COPY Kdbapp/Kdbapp.csproj Kdbapp/
RUN dotnet restore "Kdbapp/Kdbapp.csproj"

# Копируем всё остальное
COPY . .
WORKDIR /src/Kdbapp
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5237
ENTRYPOINT ["dotnet", "Kdbapp.dll"]