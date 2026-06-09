# HonoursProject
The following project is part of my **2026 Honours Project** for the **BSC (HONS) SOFTWARE DEVELOPMENT (GRADUATE APPRENTICESHIP)** programme at **Edinburgh Napier University**.

# Project Abstract
This project investigates whether gamified digital learning environments can enhance student engagement and improve retention of a subject matter compared with traditional instructional materials. Gamification has become increasingly prominent in many areas of technology and education, however existing research presents mixed findings, with many studies reporting short-term motivational benefits but limited evidence of sustained engagement or long-term learning gains. To address these gaps this study develops a prototype web-based application that incorporates selected game mechanics and compares its effectiveness with a non-gamified control condition using standard documentation or presentation materials. Two participant groups of equal size will engage with either the gamified or non-gamified version of the learning material and complete periodic assessments and quizzes designed to measure both engagement and retention of the subject materials over time. 

By combining a purpose-built digital application with repeated testing, this study aims to provide clearer insight into whether gamification meaningfully supports student engagement and improved retention in educational contexts.

# Application Overview
The application contained within this REPO uses a **Three-Tiered Architecture** with a **Model-View-Controler (MVC)** pattern and implementing single purpose **Services** on the **Application Layer**. 

The application is expected to follow the below design structure

**Presentation Layer**
- View(s)
- Controller(s)
**Application Layer**
- Service(s)
**Data Layer**
- Model(s)
- Database(s)

This architectural design would adhere to the following typical flow:
- **USER** submits a request via interaction with a **VIEW**
- **CONTROLLER** receives the request from the **VIEW** interaction
- **CONTROLLER** calls the appropriate **SERVICE**
- **SERVICE** executes business logic
- **SERVICE** business logic interacts with a **MODEL**
- **MODEL** sends a request to the **DATABASE**
- **DATABASE** receives the request from the **MODEL**
- **DATABASE** completes the request and send the result back to the **MODEL**
- **MODEL** returns the resulting data back to the **SERVICE**
- **SERVICE** uses returned data in business logic where applicable
- **SERVICE** returns processed data back to the **CONTROLLER**
- **CONTROLLER** returns the processed data to the **VIEW**
- **VIEW** renders the result of the processed data to the **USER**

## Layers and Components
The **Presentation Layer** consists of the following component(s):

**View** components are the front-end of the application and are responsible for the rendering of data provided via **Controller** components and will render the data into visual components for user interaction. The View sends user interactions, such as clicking buttons, through to a controller component.

**Controller** components serve as an intermediary for the **Service** components and the **View** components by receiving user interaction from the view, processing the input and then calling the appropriate service that contains the relevant business logic for the operation being requested. The controller components are responsible for returning the resulted output of the called service to the view to allow the correct rendering of visual data and changes from the interaction.

The **Application Layer** consists of the following component(s):

**Service** components contain business logic and how data can be processed using the data structure defined in the **Model** components. Service components implement business logic using functions that are called by **Controller** components to process data based on user interaction.

The **Data Layer** consists of the following component(s):

**Model** components contain the definitions of the data structure that the application uses, containing the connections for what data that the application has access to from the **Database** components. The model components are the core of the application and define the data that can be accessed and processed.

**Database** components store data used by the application in structured tables that can be accessd via the **Services** and **Model** components in order to pass and manipulate data to the other components of the application.

## Design Drawbacks
The implemented design does pose potential drawbacks including; the separation of concerns can lead to tight coupling between **Controllers** with both **Views** and **Services** creating the potential for changes cascading across components if design is not well implemented.

Another drawback is the potenial of **Controllers** becoming inflated and unmanageable if they are designed to handle multiple responsibilities, violating the **SRP** software principle. The inclusion of **Services** in the design helps to mitigate this risk by limiting the resonsibilities of the controller to calling appropriate services and assigning each service a single responsibility.

This architectural design requires a well thought out designa nd implementation to mitigate risks and avoid potential issues, placing a strong focus on the design and planning of the system to ensure correct data and functionality are captured as expected for the intended application purposes.

## Design Justification

The key aspect of this architecture is the focus on *Separation of Concerns* and the use of key components. The high level diagram provided within this REPO (Assets/Architecture_Flow_Diagram.png) visually demonstrates the design and flow.

By separating the core functionality and interactive systems of the applciation across multiple components we increase scalability and maintainability of the application as well as allowing for modularity and customisation of each individual component. Through this we also reserve a separation of concerns between components which is the key advantage of the chosen architecture and parttern. This provides each component a clearly defined role and allows for them to be designed for their specific intended uses rather than being responsible for multiple purposes or business logic and creating tight coupling. This seperation also allows for easier testability of each component as they may be tested independently to be performing their individual tasks and purposes. Additionally, the reusability of components such as **Controllers** and **Services** allow for components to be utilised by other components in the application, such as **Views** and **Controllers** respectively, without creating duplicate components that would violate the **DRY** principle of software design.

The **Service** components within this design are introduced to create an extra layer of separation of concerns. This additional component allows for the **Controller** components to remain focused on handling requests and routing and for **Model** components to remain focused on data definition and persistence and allowing the **Service** components contain the business logic. This separation reduces coupling between **Controller** and **Model** components and adheres to the **Single Responsibility Principle (SRP)** in turn helping to improve maintainability, testability and scalability of the application. 

This architectural design is popular for web based applications due to the structure of components allowing for a clear defined separation of concerns and responsibilities.

## Languages and Strucutre
This design utilises organized directories that reflect the layered design. The core file structure of the application will include directories for **Views** for presentation, **Controllers** and **Services** for request handling and business logic, **Models** for persistence definitions, and a **Data** directory for infrastructure components such as the **Database** connections. 

This design will also utilise the following key languages: 
- HTML  
- CSS 
- C# 
- TypeScript

The application will also utilise **MySQL** for data storage using SQL queries executed through repository components in the **Data layer**, which are invoked by **Services**. Connection strings are defined in an *appsettings.json* configuration file, which uses JSON format to manage database access and application settings within the Data directory. This structure continues to enforce the separation of concerns.  

The diagram below provides an initial visual representation of the proposed file structure using the expected file type extensions and examples of files that may exist in each directory. 

Project Root
|-- Assets
|  |--Application_Assets
|  |  |-- Logo.png
|  |
|  |-- Documentation_Assets
|     |-- Architecture_Flow_Diagram.png
|
|-- Controllers
|  |-- HomeController.cs
|  |-- ProfileController.cs
|
|-- Models
|  |-- User.cs
|  |-- Lesson.cs
|
|-- Repositories
|  |-- DatabaseService.cs
|
|-- Services
|  |-- LoginService.cs
|  |-- LogoutService.cs
|
|-- Views
|  |-- index.html
|  |-- styles.css
|
|-- appsettings.json