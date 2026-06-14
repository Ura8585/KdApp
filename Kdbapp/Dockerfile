FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Kdbapp/Kdbapp.csproj Kdbapp/
RUN dotnet restore "Kdbapp/Kdbapp.csproj"

COPY . .
WORKDIR /src/Kdbapp
RUN dotnet publish -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5237
ENTRYPOINT ["dotnet", "Kdbapp.dll"]
