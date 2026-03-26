FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/CosmaAPI/CosmaAPI.csproj", "src/CosmaAPI/"]
RUN dotnet restore "src/CosmaAPI/CosmaAPI.csproj"

COPY . .
WORKDIR /src/src/CosmaAPI
RUN dotnet publish "CosmaAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CosmaAPI.dll"]