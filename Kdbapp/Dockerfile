FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Сначала только csproj + restore — отдельный слой для кэша NuGet
COPY Kdbapp/Kdbapp.csproj Kdbapp/
RUN dotnet restore "Kdbapp/Kdbapp.csproj"

# Затем остальной код и публикация
COPY . .
WORKDIR /src/Kdbapp
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5237
ENTRYPOINT ["dotnet", "Kdbapp.dll"]
