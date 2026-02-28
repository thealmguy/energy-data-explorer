# Project Scope

The scope of this project is to create a simple web application that allows a user to examine their energy data, including year on year, month on month, week on week, and day on day comparisons. The app will show visualisations of the user's energy data, allowing them to drill down and explore their usage.

The app will also use api data to show alongside gas and electricity usage, weather - including temperature, humidity and wind speed, the number of hours of daylight.

# Third party data sources
For energy, the data will be sourced using the Octopus Energy API, which provides access to energy usage data for customers of Octopus Energy. The API allows users to retrieve their energy usage data, including historical data and real-time data.

For weather data, the app will use the open-meteo API (https://open-meteo.com/en/docs) which provides a variety of weather data, including temperature, humidity, wind speed, and hours of daylight. The API allows users to retrieve weather data for specific locations and time periods. The API allows current and historical weather data.

# Application usage
When the app starts, tghe user will be prompted to enter their Octopus Energy API key and select the location for which they want to view weather data. The app will then retrieve the user's energy usage data and weather data for the selected location.

The app will display the energy usage data and weather data in a visual format, allowing the user to easily compare their energy usage with the weather conditions. The user will be able to drill down into the data to see more detailed information about their energy usage and weather conditions. Some visualisations for example:

- A line chart showing energy usage over time, with weather data overlaid on the same chart.
- A bar chart comparing energy usage for different time periods (e.g. year on year, month on month) with weather data for the same periods.
- A scatter plot showing the relationship between energy usage and weather conditions (e.g. temperature vs energy usage).

# Tech Stack
The app will be .net 10 based. It will use Blazor for the frontend, and the backend will be built using ASP.NET Core. The app will use the Octopus Energy API and the open-meteo API to retrieve energy usage data and weather data, respectively. The app will also use a charting library such as Chart.js or D3.js or Google Charts to create visualisations of the data.

Use .net aspire to create the project structure, and follow best practices for organizing the code and separating concerns. The frontend and backend will be developed as separate projects within the same solution, allowing for clear separation of concerns and easier maintenance.

## Application Architecture
The application will follow a typical client-server architecture, with the frontend built using Blazor and the backend built using ASP.NET Core. The frontend will be responsible for displaying the user interface and handling user interactions, while the backend will be responsible for retrieving data from the APIs and processing it for display.

The backend will have two main components: a data retrieval component that interacts with the Octopus Energy API and the open-meteo API to retrieve energy usage data and weather data, respectively, and a data processing component that processes the retrieved data for display in the frontend. The frontend will have components for displaying the energy usage data and weather data in visual formats, as well as components for handling user interactions such as selecting time periods and drilling down into the data.

For the first iteration of this application, all operations will be performed in memory with no persistent storage. The application will retrieve data from the APIs and process it for display without storing any data in a database or other persistent storage mechanism. However, prefectching data may be split out in the future so allow for this in the architecture.

Initially, a front end blazor project and a backend ASP.NET Core Web API project will be created. The frontend will make HTTP requests to the backend to retrieve energy usage data and weather data, which will then be displayed in the user interface.

## Testing strategy
The project will use a combination of unit testing and integration testing to ensure that the code is working correctly. Unit tests will be written for individual components of the application, while integration tests will be used to test the interactions between different components. The tests will be run as part of the CI/CD pipeline to ensure that any changes to the code do not break existing functionality. Interfaces will be used to allow for mocking of the API calls in unit tests, allowing for testing of the data processing components without relying on actual API calls.

## Technical standards
SOLID principles will be followed in the design and implementation of the application. The code will be written in a clean and maintainable way, with a focus on readability and simplicity. The application will also follow best practices for security, such as using secure authentication methods for accessing the APIs and protecting sensitive data.

# CI/CD
The project will use Azure DevOps Pipelines for continuous integration and continuous deployment (CI/CD). The pipeline will be triggered on every push to the main branch, and will run a series of tests to ensure that the code is working correctly. If the tests pass, the pipeline will then deploy the application to Azure App Service.

The pipeline will deploy an App Service to an existing App Service Plan on a basic SKU. The pipeline will also set up Application Insights for monitoring the application, and will configure alerts to notify the development team of any issues or errors that occur in the application. The pipeline will follow best practice in having idempotent deployment steps, ensuring that the same pipeline can be run multiple times without causing issues or errors in the deployment process.

# Operations and monitoring
The application will be deployed to Azure App Service on a basic SKU. Application Insights will be used for monitoring the application, including tracking performance metrics, logging errors, and monitoring user interactions. Alerts will be set up to notify the development team of any issues or errors that occur in the application. Regular maintenance and updates will be performed to ensure that the application remains secure and up-to-date with the latest technologies and best practices.