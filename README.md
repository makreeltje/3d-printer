# 3D Printer Cost Calculator

An application to estimate the full cost of 3D printing projects by parsing GCODE files, calculating material, power, and machine costs. Designed for professionals and hobbyists alike — intended to be self-hosted.

---

## 📚 Project Documentation

Documentation is maintained as versioned specs in AsciiDoc format:

🔗 **Live Specs**: [https://makreeltje.github.io/3d-printer/](https://makreeltje.github.io/3d-printer/)

---

## 🛠️ Setup Instructions

### Backend (.NET)

```bash
git clone https://github.com/makreeltje/3d-printer.git
cd 3d-printer
dotnet build
dotnet run
```

### Frontend (Angular)

```bash
cd ClientApp
npm install
npm start
```

The full dev server runs with:
```bash
dotnet run
```

---

## 🚧 Feature Tracking

All development tasks and milestones are tracked in our GitHub Project:

🔗 [Project Board](https://github.com/makreeltje/3d-printer/projects)

Each feature/bug has an issue with a structured checklist and category label (`frontend`, `backend`, `analytics`, etc.).

Labels are automatically maintained and used to update our live checklist issue.

---

## 🤖 Automation

We use GitHub Actions to:

- Generate and publish AsciiDoc specs to GitHub Pages
- Track progress in a central setup checklist
- Tag and organize issues automatically via `gh` CLI

---

## 🧪 Testing

We use:

- `xUnit`, `Moq`, `FluentAssertions` for backend
- `Karma`, `Jasmine` for Angular

Run backend tests:
```bash
dotnet test
```

---

## 🧩 Contributing

1. Fork this repository
2. Use a branch like `feature/my-feature-name`
3. Follow the [commit message style](docs/contributing.md)
4. Create a Pull Request to `develop`

---

## 📄 License

[MIT License](LICENSE)