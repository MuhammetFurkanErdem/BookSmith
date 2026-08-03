# BookSmith - Sprint History & Roadmap

---

## 📌 Completed Sprints

### Sprint 0 - Solution Structure & Initial Architecture
**Status:** ✅ Completed  
**Goal:** Establish clean architecture solution structure and core projects.
- Created `BookSmith.sln` (.NET 8).
- Set up project layers: `BookSmith.Core`, `BookSmith.Services`, `BookSmith.Infrastructure`, `BookSmith.UI`, `BookSmith.Tests`.
- Configured `.gitignore` and base repository structure.
- **Commit:** `1912cff`, `580970a`

---

### Sprint 1 - UI Shell & MVVM Plumbing
**Status:** ✅ Completed  
**Goal:** Create modern dark-themed UI layout shell and base MVVM command infrastructure.
- Designed `MainWindow` dark theme shell.
- Created `MainView` user control.
- Added `RelayCommand` and `AsyncRelayCommand`.
- **Commit:** `580970a`

---

### Sprint 2 - PDF File Selection
**Status:** ✅ Completed  
**Goal:** Allow user to browse and select a PDF file from UI.
- Integrated OpenFileDialog in `MainViewModel`.
- Added selected file path property and UI binding.
- **Commit:** `fee3bb0`

---

### Sprint 3 - Read PDF Metadata
**Status:** ✅ Completed  
**Goal:** Display basic metadata after a PDF is selected.
- Integrated official `UglyToad.PdfPig` library.
- Extracted PDF page count, file name, and file size.
- Added metadata info card panel in `MainView`.
- **Commit:** `5fe8323`

---

### Sprint 4 - Refactor PDF Reading into Service Layer
**Status:** ✅ Completed  
**Goal:** Decouple PDF reading from presentation layer into `BookSmith.Services`.
- Created `PdfMetadata` model and `IPdfReader` interface in domain layer.
- Implemented `PdfPigReader` service in `BookSmith.Services`.
- Refactored `MainViewModel` to use `IPdfReader`.
- **Commit:** `de23246`

---

### Sprint 5 - Introduce Dependency Injection
**Status:** ✅ Completed  
**Goal:** Configure IoC container for service registration and View/ViewModel resolution.
- Configured Microsoft.Extensions.DependencyInjection in `App.xaml.cs`.
- Registered `IPdfReader`, `MainViewModel`, and `MainWindow`.
- **Commit:** `cfd7351`

---

### Sprint 6 - PDF First Page Preview Feature
**Status:** ✅ Completed  
**Goal:** Display the text content of the first page of the selected PDF as an initial preview.
- Added `ReadFirstPageText` to `IPdfReader` and `PdfPigReader`.
- Updated `MainViewModel` and UI to render the first page preview text.
- **Commit:** `eca025d`

---

### Sprint 7 - Basic Text Cleaning Pipeline Infrastructure
**Status:** ✅ Completed  
**Goal:** Establish core text cleaning abstractions and models.
- Created `ITextCleaner` interface in domain layer.
- Created `TextCleaningResult` model.
- Created `TextCleaner` skeleton implementation in `BookSmith.Services`.
- **Commit:** `363a3c0`

---

### Sprint 8 - Basic Text Normalization
**Status:** ✅ Completed  
**Goal:** Normalize line endings and whitespace handling.
- Implemented CR/LF line ending normalization (`\r\n` -> `\n`).
- Added trim and whitespace cleaning logic in `TextCleaner`.
- **Commit:** `2529d77`

---

### Sprint 9 - Soft Hyphen Character Removal
**Status:** ✅ Completed  
**Goal:** Strip soft hyphen (`\u00AD`) characters that disrupt text reading.
- Integrated `\u00AD` removal rule in `TextCleaner.Clean()`.
- Added unit tests for soft hyphen stripping.
- **Commit:** `020caf6`

---

### Sprint 10 - Wrapped Line Merge in TextCleaner
**Status:** ✅ Completed  
**Goal:** Rejoin text lines broken across line endings within paragraphs while preserving sentence endings and dialogue lines.
- Implemented `MergeWrappedLines` in `TextCleaner`.
- Added smart checks for sentence punctuation (`.`, `?`, `!`, `:`) and dialogue starters (`-`, `—`, `"`, `“`).
- Added unit tests verifying line merging behavior.
- **Commit:** `80c3654`

---

### Sprint 11 - Header and Footer Removal
**Status:** ✅ Completed  
**Goal:** Detect and remove repetitive running headers and footers across page boundaries.
- Added `RemoveHeadersAndFooters(IReadOnlyList<string> pages)` to `ITextCleaner` and `TextCleaner`.
- Frequency-based analysis (detects line patterns appearing on 3+ pages).
- **Commit:** `8bbf134`

---

### Sprint 12 - Full PDF Text Extraction (`IPdfReader.ReadAllPages`)
**Status:** ✅ Completed  
**Goal:** Implement full multi-page PDF text extraction in `IPdfReader` and `PdfPigReader`.
- Added `IReadOnlyList<string> ReadAllPages(string filePath)` signature to `IPdfReader`.
- Implemented `ReadAllPages` in `PdfPigReader` with null/empty/missing file validations.
- Added `PdfPigReaderTests` unit test suite (6 passing tests).
- **Commit:** `e8e91f6`

---

## 🚀 Upcoming Sprints (Roadmap)

### Sprint 13 - Book Pipeline Coordination Service (`IBookPipeline`) 👈 **NEXT STEP**
**Goal:** Create a unified `BookPipeline` service coordinating PDF reading, header/footer removal, line merging, and text normalization.
- **Tasks:**
  - Define `IBookPipeline` interface and `BookPipeline` implementation in `BookSmith.Services`.
  - Wire `IPdfReader.ReadAllPages` ➔ `ITextCleaner.RemoveHeadersAndFooters` ➔ `ITextCleaner.Clean`.
  - Return detailed `BookProcessingResult` containing cleaned text and statistics (page count, original vs cleaned char count, reduction ratio).
  - Add unit tests for `BookPipeline`.

---

### Sprint 14 - Pipeline UI Integration & Progress Reporting
**Goal:** Connect `IBookPipeline` to `MainViewModel` and `MainView.xaml`.
- **Tasks:**
  - Add `CleanBookCommand` to `MainViewModel`.
  - Add UI progress bar & status indicators for asynchronous processing.
  - Render cleaned text result and statistics summary card in `MainView`.

---

### Sprint 15 - Advanced Dialogue & TTS Formatting (ElevenReader Optimization)
**Goal:** Format text specifically for TTS engines like ElevenReader.
- **Tasks:**
  - Standardize dialogue quotes (`“...”`, `«...»`, `—`).
  - Add paragraph breaks and natural pause formatting for character speech.
  - Add configurable cleaning rules in `TextCleaner`.

---

### Sprint 16 - EPUB Exporter Service (`IEpubExporter`)
**Goal:** Package cleaned text into standard EPUB format.
- **Tasks:**
  - Implement `EpubExporter` in `BookSmith.Services/Export`.
  - Support basic chapter detection/splitting and EPUB metadata embedding.
  - Add unit tests for EPUB generation.

---

### Sprint 17 - Persistent Settings & Configuration (`AppSettings`)
**Goal:** Allow users to toggle cleaning options via UI and persist settings across sessions.
- **Tasks:**
  - Create `SettingsViewModel` and `SettingsView`.
  - Implement persistent storage in `BookSmith.Infrastructure/Settings/AppSettings.cs`.

---

### Sprint 18 - Export UI Integration & File Save Dialogs
**Goal:** Provide "Export to EPUB" and "Export to TXT" save dialogs and completion notifications.
- **Tasks:**
  - Add SaveFileDialog logic to UI.
  - Implement completion toast/dialog notifications.
