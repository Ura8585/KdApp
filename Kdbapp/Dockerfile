FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл проекта из подпапки Kdbapp
COPY Kdbapp/*.csproj ./Kdbapp/
RUN dotnet restore "Kdbapp/Kdbapp.csproj"

# Копируем все остальные исходники
COPY . .
WORKDIR "/src/Kdbapp"
RUN dotnet publish "Kdbapp.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5237

ENTRYPOINT ["dotnet", "Kdbapp.dll"]
