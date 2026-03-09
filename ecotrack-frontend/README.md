# EcoTrack B2B

[Frontend Live Demo](https://eco-track-b2-b.vercel.app)

## Stack & Technologies

- Frontend: React (Vite, TypeScript, TailwindCSS, i18n)
- Backend: ASP.NET Core Web API (C#)
- Database: PostgreSQL (Neon.tech)
- Authentication: JWT, email verification (SendGrid)
- Deployment: Render (backend), Vercel (frontend), Neon (database)

## Unit Testing

- Backend: xUnit tests for API endpoints, authentication, and business logic

## Project Structure

- EcoTrack.WebApi/ - Backend API
- ecotrack-frontend/ - Frontend app
- EcoTrack.Core/ - Domain models and logic
- EcoTrack.Application/ - Application layer (DTOs, services)
- EcoTrack.Infrastructure/ - Persistence, email, migrations
- EcoTrack.Tests/ - Unit tests

## About EcoTrack

EcoTrack is a B2B platform for tracking and reporting company emissions. Users can register, verify their email, log in, and manage emission entries. The app supports Slovak and English, and is optimized for both desktop and mobile devices.

