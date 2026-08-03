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

### Sprint 13 - Book Pipeline Coordination Service (`IBookPipeline`)
**Status:** ✅ Completed  
**Goal:** Create a unified `BookPipeline` service coordinating PDF reading, header/footer removal, line merging, and text normalization.
- Defined `BookProcessingResult` domain model with char counts and reduction ratio calculations.
- Implemented `BookPipeline` service in `BookSmith.Services/Pipeline`.
- Registered `IBookPipeline` service in `App.xaml.cs` IoC container.
- Created `BookPipelineTests` unit test suite (12 total passing tests).

---

### Sprint 14 - Pipeline UI Integration & Progress Reporting
**Status:** ✅ Completed  
**Goal:** Connect `IBookPipeline` to `MainViewModel` and `MainView.xaml`.
- Updated `MainViewModel` with asynchronous `StartCleaningCommand` and `IBookPipeline` dependency injection.
- Added reactive properties: `IsProcessing`, `IsCleaned`, `CleanedText`, `OriginalCharCount`, `CleanedCharCount`, `ReductionPercentage`.
- Updated `MainView.xaml` with Results Statistics Card and Cleaned Text Preview panel.
- Created `MainViewModelTests` unit test suite (18 total passing tests).

---

### Sprint 15 - Advanced Dialogue & TTS Formatting (ElevenReader Optimization)
**Status:** ✅ Completed  
**Goal:** Format text specifically for TTS engines like ElevenReader.
- Added `FormatDialogueForTts` to `ITextCleaner` and `TextCleaner`.
- Implemented quote normalization (`«`, `»` -> `“`, `”`) and leading dialogue dash normalization (`-`, `–` -> `— `).
- Created `DialogueFormattingTests` unit test suite (22 total passing tests).

---

### Sprint 16 - EPUB Exporter Service (`IEpubExporter`)
**Status:** ✅ Completed  
**Goal:** Package cleaned text into standard EPUB format.
- Created `EpubExportOptions` model in `BookSmith.Core`.
- Implemented `EpubExporter` using .NET `ZipArchive` (`mimetype`, `container.xml`, `content.opf`, `toc.ncx`, `style.css`, `chapter1.xhtml`).
- Registered `IEpubExporter` in `App.xaml.cs` IoC container.
- Created `EpubExporterTests` unit test suite (26 total passing tests).

---

### Sprint 17 - Persistent Settings & Configuration (`AppSettings`)
**Status:** ✅ Completed  
**Goal:** Allow users to toggle cleaning options via UI and persist settings across sessions.
- Created `AppSettings` model in `BookSmith.Core/Models`.
- Created `ISettingsService` interface and `JsonSettingsService` in `BookSmith.Infrastructure/Settings`.
- Integrated `ISettingsService` into `MainViewModel` to auto-load and save checkbox options.
- Created `SettingsServiceTests` unit test suite (28 total passing tests).

---

### Sprint 18 - Export UI Integration & File Save Dialogs
**Status:** ✅ Completed  
**Goal:** Provide "Export as EPUB" and "Export as TXT" save dialogs and completion notifications.
- Added `ExportEpubCommand` and `ExportTxtCommand` to `MainViewModel`.
- Added `SaveFileDialog` integration for `.epub` and `.txt` exports with completion status feedback.
- Updated `MainView.xaml` with "Export as EPUB" and "Export as TXT" action buttons.
- Created `MainViewModelExportTests` unit test suite (30 total passing tests).

---

## 🎉 v1.0 Milestone Completed (Sprints 0 - 18)
All v1.0 core sprints are fully implemented, tested, and verified.

---

# 🚀 BookSmith v2.0 Roadmap (Next Generation UX & High-Consistency Cleaning)

## Phase 1: Modern Multi-View Navigation & Interactive Editor

### Sprint 19 - Modern UI Shell & Navigation Architecture (View-Switching)
**Status:** ✅ Completed  
**Goal:** Replace the single crowded view with a clean step-by-step navigation system (`File Selection` ➔ `Processing` ➔ `Editor & Export`).
- Created `INavigationService` and `NavigationService` in `BookSmith.UI/Navigation`.
- Split UI into 3 modular step views: `ImportView`, `ProcessingView`, `EditorView`.
- Redesigned `MainView.xaml` with modern Stepper Header (`1. Select PDF` -> `2. Cleaning` -> `3. Editor & Export`) and ContentControl shell.
- Created `NavigationServiceTests` unit test suite (38 total passing tests).

---

### Sprint 20 - Interactive Full-Screen Editor (`EditorView` & `EditorViewModel`)
**Status:** ✅ Completed  
**Goal:** Provide a dedicated full-text editor allowing live manual edits, search & replace, line removal, and character metrics before exporting.
- Added **Search & Replace** panel (`Ctrl+F`) with case-insensitive matching, match count, Replace single/all.
- Added **Quick-Action Toolbar**: `🔍 Search & Replace`, `✂ Remove Selection`, `↩ Undo`, `↪ Redo`.
- Implemented **Undo/Redo** stack (up to 20 text snapshots) with `Ctrl+Z` / `Ctrl+Y` keyboard shortcuts.
- Added **Remove Selected Lines** feature with live selection tracking via `SelectionChanged`.
- Added **Live Word & Line counters** in stats card (auto-updating on every text change).
- Created `EditorViewModelTests` unit test suite (61 total passing tests).

---

## Phase 2: Enhanced Intelligence & Publisher Filtering

### Sprint 21 - Automatic Front-Matter & Publisher Credit Filter (`IFrontMatterFilter`) 👈 **NEXT STEP**
**Goal:** Automatically detect and strip publisher copyright details, ISBNs, translator notes, and printing credits from front matter pages.
- **Tasks:**
  - Create `IFrontMatterFilter` in `BookSmith.Services`.
  - Add pattern recognition for keywords ("PEGASUS YAYINLARI", "Baskı-Cilt", "Sertifika No", "Bestseller Roman", "Yayın Koordinatörü").
  - Integrate into `BookPipeline`.

### Sprint 22 - Chapter Structure & Table of Contents Detection (`IChapterDetector`)
**Goal:** Detect chapter headings (`BÖLÜM 1`, `CHAPTER I`, Roman numerals `I`, `II`, `III`) and structure them in EPUB table of contents.
- **Tasks:**
  - Create `IChapterDetector` service.
  - Add sidebar chapter list in `EditorView` allowing instant jump navigation to any chapter.
  - Format chapter headings cleanly into EPUB `<h1>` tags and `toc.ncx`.

---

## Phase 3: Export Customization & User Presets

### Sprint 23 - Advanced EPUB Styling & Cover Image Support
**Goal:** Allow users to upload custom book cover images and customize EPUB font sizes/margins.
- **Tasks:**
  - Add cover image selection in UI.
  - Package cover image into EPUB zip archive structure.

### Sprint 24 - Cleaning Presets & Batch Processing Engine (`IBatchProcessor`)
**Goal:** Save custom cleaning rule presets and batch-process multiple PDF books in sequence.
- **Tasks:**
  - Build preset manager (e.g., "ElevenReader Fiction", "Academic Papers").
  - Support multi-file queue processing.

### Sprint 25 - Performance Virtualization & v2.0 Final Polish
**Goal:** Optimize text rendering for 1000+ page books and deliver final UI polish.
- **Tasks:**
  - Implement UI text virtualization for instant rendering of massive books.
  - Perform full end-to-end regression testing.
