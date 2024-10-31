# Green Gains Backend 💪🪴

[[TOC]]

## Introduction

This is the Backend for the Greengains Project, in our Project for the Master.

It includes Endpoints to receive Data from Sensors and Endpoints for the Frontend

## Prerequisites

> [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) - `v8.0.10`
>
> Microsoft Visual Studio 2022 - `v17.11.5`

## Getting Started

1. Clone Repo

```console
git clone https://github.com/MasterprojektGreenGains/Backend
```

2. change to directory

```console
cd Backend
```

3. Run build

```console
dotnet run build
```

### Setting Up the appsettings Connectionstrings

To connect to any Database the API looks into the Configurations. The easiest way to define the ConnectionString is inside the `appsettings.json` or `appsettings.Development.json`. The Default ConnectionString for a TimescaleDB/PostgreSQL Connection is already inlcuded but it should be in the format of:

```json
{
  ...

  "ConnectionStrings": {
    "GreenGainsDb": *your ConnectionString here*
  }

  ...
}
```

the default for our purpose and the one needed to connect to the Docker instance mentioned at [Starting the TimescaleDB Docker Instance](#starting-the-timescaledb-docker-instance) is

`"GreenGainsDb": "Server=127.0.0.1;Port=5432;Database=greengains;User Id=postgres;Password=GreenGains;"`

### Starting the TimescaleDB Docker Instance

For Local Testing Purposes you can run a TimescaleDB Instance in Docker

Use the following command

```console
docker run -d --name GreenGainsDb -p 5432:5432 -e POSTGRES_PASSWORD=GreenGains timescale/timescaledb-ha:pg16
```

after this the docker instance should run and you should be able to apply the migrations to the database with

```console
dotnet ef update database
```

## Running the Application

## API Endpoints

## Testing

## Deployment

## Contributing

## License
