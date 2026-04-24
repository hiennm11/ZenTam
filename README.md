# ZenTâm - Hệ Sinh Thái Thuật Số / Phong Thủy Minimalist

> **ZenTâm** là hệ sinh thái Thuật số/Phong thủy Minimalist — không rườm rà bùa chú. User chat ngôn ngữ tự nhiên → AI bóc tách intent → .NET Rule Engine tính toán → Trả về kết quả Cát/Hung rõ ràng.

## 🎯 Project Overview

```
User Message (Natural Language)
         ↓
    AI Intent Parser
    (LiteLLM + Ollama)
         ↓
   .NET Rule Engine
   (Pure Math + Lookup)
         ↓
  Cát/Hung Result
```

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| Backend | .NET 10 Web API (Vertical Slice Architecture) |
| AI Router | LiteLLM + Ollama (Qwen2/Phi-3 local) + Redis Cache |
| Orchestrator | n8n |
| Database | Entity Framework Core + SQLite/PostgreSQL |
| Frontend | Telegram Bot (MVP) |

## 📁 Project Structure

```
ZenTam/
├── src/
│   └── ZenTam.Api/
│       ├── Common/               # Shared utilities
│       │   ├── Caching/          # Redis/Memory cache abstraction
│       │   ├── Domain/           # DTOs, Enums, Domain models
│       │   ├── Exceptions/       # Custom exceptions
│       │   ├── Lunar/           # AmLichCalculator (JDN, Solar2Lunar)
│       │   └── Rules/           # ISpiritualRule, RuleResolver, RuleResult
│       ├── Features/            # Vertical Slice modules
│       │   ├── Clients/         # ClientProfile CRUD + RelatedPersons
│       │   ├── EvaluateSpiritualAction/  # Rule engine for Năm/Hàng
│       │   └── ParseAndEvaluate/        # LLM Intent + Evaluation
│       └── Infrastructure/       # EF Core DbContext, Entities, Seeders
├── tests/
│   └── ZenTam.Api.UnitTests/    # Unit tests
└── docs/
    └── ZenTam_Master_Roadmap.md # Master project roadmap
```

## ✅ Current Status

### Phase 0 — Core Engine (DONE ✅)
- [`AmLichCalculator`](src/ZenTam.Api/Common/Lunar/AmLichCalculator.cs) - JDN, Solar2Lunar
- [`RuleResolver`](src/ZenTam.Api/Common/Rules/RuleResolver.cs) - Tầng Năm rule engine
- LLM Intent Parser (Regex + SLM)

### Phase 1 — Data Foundation (DONE ✅)
- `ClientProfile` Entity + CRUD API
- `ClientRelatedPerson` Entity + Management API
- `ConsultationSession` Entity for history
- Redis Cache for LLM Intent

### Phase 2 — Tầng Tháng + Gánh Mệnh (IN PROGRESS 🏗️)
- CanChi Engine (GetCanChiNam, GetCanChiThang, GetCanChiNgay)
- Tầng Tháng Rules (Nguyệt Kỵ, Tam Nuông, Xung Tuổi...)
- Gánh Mệnh Logic

### Phase 3 — Tầng Ngày + Find Good Days (PENDING)
- Thập Nhị Trực, Nhị Thập Bát Tú
- Hoàng/Hắc Đạo, Sát Chủ, Thọ Tử
- `POST /api/evaluate/find-good-days`

## 🧮 Core Engine Components

### AmLichCalculator
| Method | Description |
|--------|-------------|
| `JulianDayNumber(d, m, y)` | Foundation for all calculations |
| `Solar2Lunar(DateTime)` | Solar → Lunar date conversion |
| `Convert(DateTime)` | Returns `LunarDateContext` |
| `GetLunarYear(DateTime)` | Get lunar year |

### Tầng Năm Rules
| Rule | Description |
|------|-------------|
| `KimLauRule` | Kim Lâu (quý nhân) |
| `HoangOcRule` | Hoang Ốc (hạn chế) |
| `TamTaiRule` | Tam Tai (xung khắc) |
| `ThaiTueRule` | Thai Tü (tai ách) |

## 🔌 API Endpoints

### Client Management
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/clients` | Create client profile |
| `GET` | `/api/clients?phone={phone}` | Search by phone |
| `GET` | `/api/clients/{id}` | Get full profile |
| `POST` | `/api/clients/{id}/related` | Add related person |
| `DELETE` | `/api/clients/{id}/related/{rid}` | Delete related person |

### Evaluation
| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/chat` | Parse message + evaluate |
| `POST` | `/api/evaluate/action` | Evaluate action by clientId |

## 📊 Feature Expansion Roadmap

### Tier 1 — HIGH VALUE, LOW EFFORT
- [ ] **Smart Calendar API** `/api/calendar/month` - 30 days with score & flags
- [ ] **Detailed Hoàng Đạo Hours** - 12 hours (Tý→Hợi) + top 3 good hours
- [ ] **Xông Đất / Xuất Hành** - New ActionCode + Bát Trạch directions
- [ ] **Compatibility Check** - Spouse/partner matching

### Tier 2 — HIGH VALUE, MEDIUM EFFORT
- [ ] **Tứ Trụ Bát Tự Basic** - 4 pillars + ngũ hành + deficiency/excess
- [ ] **Annual Stars (Sao Chiếu Mệnh)** - Thái Tuế, Thiên Đức, Lộc Tồn...
- [ ] **House Direction / Phong Thủy** - Bát Trạch 8 directions × 8 mệnh
- [ ] **Baby Naming** - Ngũ hành suggestions + KHang HY + tone

### Tier 3 — COMPLEX / STRATEGIC
- [ ] **Complete Tử Vi Chart** - 14 main stars + 108 sub stars + Đại/Tiểu Hạn
- [ ] **AI Natural Language Explanation** - Rule Engine → LLM interpretation
- [ ] **Streak & Reminder System** - n8n Cron push + Streak tracker
- [ ] **PDF Export** - "Phiếu Tư Vấn Thuật Số" via QuestPDF

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- Ollama (for local LLM)
- Redis (optional, falls back to Memory Cache)

### Build & Run
```bash
cd src/ZenTam.Api
dotnet restore
dotnet run
```

### Run Tests
```bash
dotnet test
```

## 📅 Timeline

```
Phase 0-1: Data Foundation ✅ (Tuần 1-2)
Phase 2:   Tầng Tháng + Gánh Mệnh 🏗️ (Tuần 3-4)
Phase 3:   Tầng Ngày + Find Good Days (Tuần 5-6)
Phase 4:   AI Luận Giải + Lịch Tháng (Tuần 7-8)
Phase 5:   Tứ Trụ + Sao Chiếu Mệnh (Tuần 9-10)
Phase 6:   Tử Vi + Đặt Tên + PDF (Tháng 3-4)
```

## 📚 Documentation

- [ZenTam Master Roadmap](docs/ZenTam_Master_Roadmap.md) - Full project planning
- [Workspace Standard](docs/WORKSPACE_STANDARD.md) - Development guidelines
- [Phase 1 Data Blueprint](docs/ai/task/zentam-phase1-data/02_Blueprint.md) - Phase 1 implementation details