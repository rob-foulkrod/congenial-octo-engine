# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Todo.Web/Todo.Web.csproj", "src/Todo.Web/"]
RUN dotnet restore "src/Todo.Web/Todo.Web.csproj"

COPY . .
RUN dotnet publish "src/Todo.Web/Todo.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Azure Container Apps routes ingress to the container target port.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Todo.Web.dll"]
