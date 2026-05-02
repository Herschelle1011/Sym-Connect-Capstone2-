# SYM-Connect

A web-based Youth Center Management System built with ASP.NET Core MVC, designed to streamline member management, activity scheduling, participation tracking, and administrative operations for youth organizations.

## 📋 Overview

SYM-Connect centralizes the core operations of a youth center — from managing members and groups to scheduling activities, tracking attendance, and running elections — all through a clean, role-based web interface.

## 🛠️ Tech Stack

- **Framework:** ASP.NET Core MVC (.NET 8)
- **Language:** C#
- **Frontend:** Razor Views, HTML, CSS, JavaScript
- **Database:** Entity Framework Core
- **IDE:** Visual Studio

## 🗂️ Project Structure

## 📌 Features

- **Master Files** — Manage Members, Groups, Roles, and Facilities
- **Members & Groups** — Member list, group management, role assignments, membership applications
- **Scheduling & Reservations** — Calendar view, activity scheduling, facility reservation, conflict checker
- **Activities** — Create activities, manage approval workflows, collect feedback and evaluations
- **Attendance** — Record and view attendance history
- **Participation Tracking** — Points summary and engagement records
- **Announcements** — Create, schedule, and view announcements
- **Voting System** — Create elections, cast votes, view results
- **Documents & Media** — File uploads, repository, and media gallery
- **Reports** — Weekly summaries, attendance, participation, and activity reports
- **Settings** — Profile settings, user management, system configuration

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2022 or later
- SQL Server (or configure your preferred provider in `appsettings.json`)

### Installation

1. Clone the repository:
```bash
   git clone https://github.com/Herschelle1011/Sym-Connect-Capstone2-.git
```
2. Open `Sym-Connect (Capstone2).sln` in Visual Studio.
3. Update the connection string in `appsettings.json`.
4. Run database migrations:
```bash
   dotnet ef database update
```
5. Run the project:
```bash
   dotnet run
```

## 👥 Team

Capstone 2 Project — [Don Bosco Youth Center]

## 📄 License

This project is for academic purposes only.
