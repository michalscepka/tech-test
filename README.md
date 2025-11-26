# Giacom Tech Test

## Pre-requisites

1. [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) to run the project in IDE
2. [Docker](https://www.docker.com/products/docker-desktop/) to run the database and application in container
3. IDE (e.g. Visual Studio Code, Visual Studio 2022, Jetbrains Rider...)

## Initial run

1. Run the database server in Docker:
     ```sh
     docker compose up db
     ```

2. Open IDE and open the solution

3. Start the application:
    - from IDE:
        - press `Run`
    - from terminal:
        - navigate to `./src/Order.WebAPI`
        - execute this command:
        ```sh
        dotnet run
        ```
    - from Docker:
        - execute this command:
        ```sh
        docker compose up api
        ```

4. Open the web browser

5. Navigate to `http://localhost:8000/swagger/`

6. Use the application
