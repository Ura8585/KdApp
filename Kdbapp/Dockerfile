FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and all csproj files first (best layer caching)
COPY *.sln .
COPY Kdbapp/*.csproj Kdbapp/
# If you have other projects (Data, Shared, etc.), copy their .csproj too:
# COPY AnotherProject/*.csproj AnotherProject/

RUN dotnet restore "Kdbapp/Kdbapp.csproj"

# Copy the rest of the source
COPY Kdbapp/ Kdbapp/
# COPY other projects if needed...

WORKDIR /src/Kdbapp
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5237
ENTRYPOINT ["dotnet", "Kdbapp.dll"]