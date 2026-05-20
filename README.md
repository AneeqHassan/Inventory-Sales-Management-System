# 🛒 Superstore Inventory & Sales Management System

## Overview
A scalable, secure backend enterprise application developed in C# using the **ASP.NET Core MVC** framework. This system is designed to handle complex inventory tracking and seamless sales processing for a superstore environment. 

A major focus of this project is on **System Security and Architecture**, implementing strict system boundaries, cryptographic password hashing, and token-based state management to protect sensitive operational data.

## 🛠️ Technical Stack
* **Framework:** ASP.NET Core MVC (C#)
* **Authentication & Security:** JSON Web Tokens (JWT), BCrypt, Role-Based Access Control (RBAC)
* **Architecture:** Enterprise MVC pattern, detailed system modeling (UML)

## 🚀 Key Features

### 1. Secure Access & Identity Management
* **JWT Authentication:** Implements secure stateless authentication utilizing JSON Web Tokens to manage user sessions safely across API endpoints.
* **Cryptographic Security:** Utilizes BCrypt for robust password hashing, ensuring credentials are never stored in plaintext.
* **Role-Based Access Control (RBAC):** Enforces the principle of least privilege by restricting access to inventory and sales endpoints based on strict user roles (e.g., Admin, Cashier, Manager).

### 2. Inventory & Sales Processing
* **Centralized Inventory Management:** Provides full CRUD (Create, Read, Update, Delete) capabilities for managing thousands of product SKUs.
* **Real-Time Sales Processing:** Handles transactional data efficiently, updating inventory counts dynamically as sales are processed.

### 3. Comprehensive System Architecture
The application was built following strict software engineering principles and system modeling techniques. The repository includes documentation for:
* **Entity-Relationship Diagrams (ERD):** Mapping the complex relationships between products, sales, users, and roles.
* **Use Case & Sequence Diagrams:** Defining clear system boundaries and mapping the exact flow of data during sales transactions and inventory updates.

## 💻 Setup & Installation
1. Clone the repository to your local machine.
2. Ensure the .NET Core SDK is installed.
3. Update the `appsettings.json` with your database connection string and JWT Secret Key.
4. Run `dotnet ef database update` to apply Entity Framework migrations.
5. Execute `dotnet run` to launch the application locally.
