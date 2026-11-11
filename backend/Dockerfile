FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution, props, and project files for caching restore layer
COPY src/NaryBE/*.slnx ./
COPY src/NaryBE/Directory.Packages.props ./
COPY src/NaryBE/Core/*.csproj ./Core/
COPY src/NaryBE/Infrastructure/*.csproj ./Infrastructure/
COPY src/NaryBE/NaryBE/*.csproj ./NaryBE/

RUN dotnet restore NaryBE.slnx

# Copy source and publish
COPY src/NaryBE/. .
WORKDIR /app/NaryBE
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "NaryBE.dll"]