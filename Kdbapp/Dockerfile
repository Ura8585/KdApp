FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

# Самое важное — подробный вывод ошибки
RUN dotnet restore "Kdbapp/Kdbapp.csproj" --verbosity detailed

WORKDIR /src/Kdbapp
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5237
ENTRYPOINT ["dotnet", "Kdbapp.dll"]