# **Overview**   
This repository contains the source code for the ECommerceSystem. It is a structured software solution, likely built with .NET given the C\# project files, designed to handle e-commerce operations such as product management and order processing.

# **Project Structure**   
The codebase is divided into three primary projects:

* **ECommerce.Api**: The main web API for the system. It contains API endpoints for operations, specifically structured with an OrdersController.cs and a ProductsController.cs to handle order and product requests respectively. It also includes standard API configuration files like appsettings.json and Program.cs.  
* **ECommerce.Processor**: A background processing service or function app. It includes an OrderProcessorFunction.cs, which is likely responsible for asynchronously handling or fulfilling orders submitted through the API. It includes specific configuration files like host.json and serviceDependencies.json.  
* **ECommerce.Shared**: A shared library containing common data models and logic used by both the API and the Processor. It includes class files for OrderRequest.cs and Product.cs.

# **Key Configuration Files**   
At the root level, the project includes several essential files for setup and deployment:

* **ECommerceSystem.sln**: The overarching solution file to open and build all interconnected projects together.  
* **docker-compose.yml**: Indicates that the system is containerized and can be spun up using Docker Compose.  
* **config.json**: A general configuration file for system-wide settings.  
* **.gitignore and .gitattributes**: Standard version control configuration files.

# **Getting Started**   
Since the repository includes a docker-compose.yml file, the easiest way to run the application is likely through Docker. Alternatively, you can open the ECommerceSystem.sln file in your preferred C\# IDE (such as Visual Studio or Rider) to build and run the API and Processor projects locally.
