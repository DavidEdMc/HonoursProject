# Author: David Edward McIntyre
# Project: HonoursProject
The following project is part of my **2026 Honours Project** for the **BSC (HONS) SOFTWARE DEVELOPMENT (GRADUATE APPRENTICESHIP)** programme at **Edinburgh Napier University**.

## Running the Application

To run HonoursProject without installing anything:

1. Go to the Releases page.
2. Download the latest ZIP file.
3. Extract it.
4. Open the "publish" folder.
5. Run HonoursProject.exe.

The app will launch automatically in your browser.

Your first launch of the application will require you to register an account via the login/registration form options, complete the registration and then log in to your personal session of the application using the created credentials. NOTE: All passwords are securely stored and are not visible on the Database

NOTE: As this is a university project, the exe file may be be flagged by features such as Windows Defender as an "unrecognized app" this is due to it simply being a prototype applicaiton and not an official exe release file that would be recognized. Please ignore this warning if it appears.

# Project Abstract
This project evaluates whether a gamified web-based learning environment improves student engagement and information retention compared with traditional materials. It implements a prototype application that uses selected game mechanics and compares outcomes and data gathered from two matched participant groups (gamified vs. non-gamified). The repository contains the prototype application files with the deliverable releases located on https://github.com/DavidEdMc/HonoursProject/releases

# Application Overview
This prototype application consists of a gamified learning system that uses a collection of curated lessons. Each lesson is separated into two parts, a static learning page that provides the subject material in a quick easy to read format, and an interactive quiz to test the users knowledge of the learning page.

Each quiz consists of 10 questions of various question types:
- Multiple Choice (Choose one)
- Select ALL Correct statements/Facts (Choose multiple)
- Drag and Drop
- Order items
- Fill in the Blank

The quiz part loops through all 10 questions in a random order until ALL questions have been correctly answered. If a user answers incorrectly on one or more questions, after completing the final question of the lesson all incorrectly answered questions will be looped through as a "Retry" until answered correctly. Incorrect answers affect the score provided on the Completion page. After the initial lessons users will access the Exam which consists of 20 questions utilising information and questions from the previous lessons. Try and complete every lesson with a perfect score!

After completing a quiz a user is provided with a Completion page that provides some statistics on how they performed including Time Taken, number of mistakes made, and the accuracy of their answers. In addition to the learning cycle, each answer provides the user with XP to increase their Level and each run increases their total Score, increasing their position on the Dashboard Leaderboard. 

On the Dashboard users can view their number of completed lessons, any unlocked achievements, and how many days streak they have used the application. Clicking the Lessons or Achievements card will direct the user to the associated page.

The Achievements page provides a list of unlockable achievements/badges in the application to encourage users to attempt perfect lesson, all lessons, maintaining streaks, gaining XP and more. Can you get all Achievements?

# Application Design Overview
The application contained within this REPO uses a **Three-Tiered Architecture** with a **Model-View-Controller (MVC)** pattern and implementing the use of **Services** on the **Application Layer** for application logic. 

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

**Database** components store data used by the application in structured tables that can be accessed via the **Services** and **Model** components in order to pass and manipulate data to the other components of the application.

## Languages and Structure
This design utilises organized directories that reflect the layered design. The core file structure of the application will include directories for **Views** for presentation, **Controllers** and **Services** for request handling and business logic, **Models** for persistence definitions, and a **Data** directory for infrastructure components such as the **Database** connections. 

This design will also utilise the following key languages: 
- HTML  
- CSS 
- C# 
- JavaScript

The application will also utilise **MySQL** for data storage using SQL queries executed through repository components in the **Data layer**, which are invoked by **Services**. Connection strings are defined in an *appsettings.json* configuration file, which uses JSON format to manage database access and application settings within the Data directory. This structure continues to enforce the separation of concerns.  

The diagram below provides an visual representation of the file structure using the expected file type extensions and examples of files that may exist in each directory.

```text
Project Root
|-- Controllers
|  |-- AccountController.cs
|  |-- LessonController.cs
|
|-- Models
|  |-- User.cs
|  |-- Lesson.cs
|
|-- Services
|  |-- UserService.cs
|  |-- LessonService.cs
|
|-- Views
|  |-- Account
|  |   |-- loginPage.cshtml
|  |
|  |-- Dashboard
|  |   |-- DashboardPage.cshtml
|  |
|  |-- Home
|  |   |-- index.cshtml
|  |
|  |-- Lesson
|  |   |-- LessonRunner.cshtml
|  |
|  |-- Shared
|      |-- _Layout.cshtml
|
|-- wwwroot
|  |-- assets
|  |   |-- audio
|  |   |    |-- correct.mp3
|  |   |
|  |   |-- images
|  |   |    |-- audio.png
|  |   |
|  |   |-- profile_avatar
|  |        |--default.png
|  |   
|  |-- css
|  |    |-- Common
|  |    |    |--Buttons.css
|  |    |--Home.css
|  |
|  |-- js
|       |-- site.js
|
|-- appsettings.json
|-- README.md
```
