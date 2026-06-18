FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY Kdbapp/Kdbapp.csproj .
RUN dotnet restore
COPY Kdbapp/ .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
RUN mkdir -p /app/wwwroot/uploads/prints
RUN chmod -R 777 /app/wwwroot
EXPOSE 5237
ENTRYPOINT ["dotnet", "Kdbapp.dll"]