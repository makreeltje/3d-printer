# Contributing to 3D Printer Cost Calculator

We welcome contributions from the community! This project is designed to help hobbyists and professionals calculate the real cost of 3D prints. Whether you're adding a new feature, improving the UI, or updating documentation — you're in the right place.

---

## 🛠 Development Setup

### Backend

- .NET 6.0 or later
- IDE: Visual Studio / Rider / VS Code

### Frontend

- Node.js + npm
- Angular CLI

---

## 📁 Repository Structure

- `/src`: .NET Core backend
- `/ClientApp`: Angular frontend
- `/docs/specs`: Architecture and requirements (AsciiDoc)
- `.github/workflows`: CI/CD automation

---

## 🧪 Testing

Run backend tests:

```bash
dotnet test
```

Frontend testing (Karma + Jasmine):

```bash
cd ClientApp
ng test
```

---

## 🧩 Git Flow & Branching

We use a simplified Git Flow:

- `main`: production
- `develop`: active integration
- `feature/xyz`, `bugfix/abc`: for new work

Use this naming convention:
```text
feature/cost-engine
bugfix/filament-weight
docs/update-readme
```

---

## ✅ Pull Request Checklist

Before submitting a PR:

- [ ] Code builds successfully
- [ ] Relevant tests pass or are added
- [ ] Your branch is up to date with `develop`
- [ ] You’ve added a descriptive title and comments
- [ ] You’ve linked the relevant issue

---

## 🤖 Automation

Our GitHub Actions handle:

- Spec publishing via AsciiDoc
- Labeling and checklist tracking
- CI for backend/frontend (coming soon)

---

## 💬 Questions or Help?

Open a [Discussion](https://github.com/makreeltje/3d-printer/discussions) or ping in the issue.

Thanks for contributing and improving this tool for the maker community!

---

> Built with ❤️ by and for 3D printing enthusiasts.