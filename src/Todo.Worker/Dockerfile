# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Todo.Worker/Todo.Worker.csproj", "src/Todo.Worker/"]
RUN dotnet restore "src/Todo.Worker/Todo.Worker.csproj"

COPY . .
RUN dotnet publish "src/Todo.Worker/Todo.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Todo.Worker.dll"]
