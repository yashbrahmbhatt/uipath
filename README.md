[![Build Changed Projects](https://github.com/yashbrahmbhatt/uipath/actions/workflows/build-projects.yaml/badge.svg)](https://github.com/yashbrahmbhatt/uipath/actions/workflows/build-projects.yaml)

# Yash's UiPath Repo

This is a single **monorepo** containing all of Yash's UiPath-related automation components. It consolidates reusable libraries, production-ready automation projects, and supporting .NET packages into a unified structure to simplify development, testing, and deployment.

---

## 🧱 Project Structure

### 🔹 Automation Libraries
Reusable libraries of workflows and components, designed to be shared across automation solutions. These promote consistency, reduce duplication, and support faster delivery.

### 🔹 Automation Projects
End-to-end automation processes that orchestrate business logic using the shared libraries. These are intended for deployment to UiPath Orchestrator and are suitable for unattended or attended execution.

### 🔹 .NET Libraries
Supporting .NET class libraries used to define custom activities, typed configuration models, and integrations with external systems. These libraries are distributed as versioned packages and consumed by UiPath solutions.

### 🔹 CI/CD Workflows
Automated pipelines that:
- Detect which projects have changed
- Selectively build only those projects
- Generate semantic versions automatically
- Package and publish .NET libraries as NuGet packages
- Tag and release versioned artifacts

This approach ensures minimal build time, consistent release management, and full traceability of changes across automation components.

---

## 🎯 Goals

- **Reusability**: Centralize logic and configuration to support reuse across projects.
- **Extensibility**: Extend UiPath with custom .NET components and external integrations.
- **Scalability**: Enable enterprise-scale automation through modular design.
- **Automation-First**: Use intelligent CI/CD pipelines for fast, reliable delivery of changes.

---

## 🛠️ Built With

- [UiPath Studio](https://www.uipath.com/product/studio)
- [.NET 6+](https://dotnet.microsoft.com/)
- [GitHub Actions](https://github.com/features/actions)

---

## 📄 License


This project is licensed under the MIT License. a
