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

### Sprint 21 - Automatic Front-Matter & Publisher Credit Filter (`IFrontMatterFilter`)
**Status:** ✅ Completed  
**Goal:** Automatically detect and strip publisher copyright details, ISBNs, translator notes, and printing credits from front matter pages.
- Created `IFrontMatterFilter` interface in `BookSmith.Core/Interfaces`.
- Implemented `FrontMatterFilter` in `BookSmith.Services/Cleaning` with Turkish + international keyword detection (3+ hits → front matter page).
- Scans only first 8 pages; never touches main content.
- Short pages (< 200 chars) at the start are also automatically removed.
- Integrated as Step 1 in `BookPipeline.Process()` before header/footer removal.
- Added `RemoveFrontMatter` toggle to `AppSettings`, `ImportViewModel`, and `ImportView.xaml` (green checkbox).
- Created `FrontMatterFilterTests` unit test suite (71 total passing tests).

### Sprint 22 - Chapter Structure & Table of Contents Detection (`IChapterDetector`)
**Status:** ✅ Completed  
**Goal:** Detect chapter headings (`BÖLÜM 1`, `CHAPTER I`, Roman numerals `I`, `II`, `III`) and structure them in EPUB table of contents.
- Created `IChapterDetector` interface in `BookSmith.Core/Interfaces` and `ChapterInfo` domain model.
- Implemented `ChapterDetector` in `BookSmith.Services/Cleaning` with regex pattern recognition for Turkish, English, Roman numerals, and ordinals.
- Integrated chapter detection as Step 4 in `BookPipeline.Process()`.
- Updated `EpubExporter` to generate multi-chapter EPUB files (`chapter1.xhtml`, `chapter2.xhtml`, etc.) and a full `toc.ncx` Table of Contents map.
- Added collapsible chapter navigator sidebar to `EditorView.xaml` with instant click-to-scroll navigation.
- Created `ChapterDetectorTests` unit test suite (78 total passing tests).

---

## Phase 3: Export Customization & User Presets

### Sprint 23 - Advanced EPUB Styling & Cover Image Support
**Status:** ✅ Completed  
**Goal:** Allow users to upload custom book cover images and customize EPUB font sizes/margins.
- Added `CoverImagePath`, `FontSizePt`, and `LineHeight` to `EpubExportOptions`.
- Updated `EpubExporter` to package cover image (`OEBPS/cover.jpg|png`), render `cover.xhtml`, update `content.opf` (cover meta, manifest, spine), and add Cover navPoint in `toc.ncx`.
- Applied user typography preferences (font size & line height) to generated `OEBPS/style.css`.
- Added EPUB customization panel to `EditorView.xaml` with Cover Image file picker, Author metadata input, Font Size selector, and "⚙ EPUB Settings" toggle button.
- Created `EpubExporterTests` unit test suite (81 total passing tests).

### Sprint 24 - Cleaning Presets & Batch Processing Engine (`IBatchProcessor`)
**Status:** ✅ Completed  
**Goal:** Save custom cleaning rule presets and batch-process multiple PDF books in sequence.
- Created `CleaningPreset` domain model, `IPresetManager` interface, and `JsonPresetManager` implementation.
- Included 3 built-in presets ("Default Rules", "ElevenReader Fiction", "Academic Clean") and persistence for user-created custom presets in `%AppData%/BookSmith/presets.json`.
- Created `BatchItem` model, `IBatchProcessor` interface, and `BatchProcessor` implementation for sequential multi-file execution with progress reporting.
- Updated `ImportView.xaml` with Preset selection dropdown ("Default", "ElevenReader Fiction", "Academic Clean"), "+ Save Preset" button, and multi-file selection support.
- Updated `MainViewModel` to execute `IBatchProcessor` when multiple files are queued.
- Created `PresetManagerTests` and `BatchProcessorTests` unit test suites (87 total passing tests).

### Sprint 25 - Performance Virtualization & v2.0 Final Polish
**Status:** ✅ Completed  
**Goal:** Optimize text rendering for 1000+ page books and deliver final UI polish.
- Implemented zero-allocation non-blocking background debounced counter calculation (`UpdateCounters()`) in `EditorViewModel` for instant UI responsiveness on 1,000,000+ character books.
- Capped Undo/Redo history stack to 10 snapshots to prevent LOH (Large Object Heap) memory overhead during heavy text edits.
- Added visual tooltips with keyboard shortcuts (`Ctrl+F`, `Ctrl+Z`, `Ctrl+Y`) to `EditorView.xaml` quick-action buttons.
- Created `PerformanceTests` verifying sub-500ms cleaning and chapter detection on simulated 1000-page book text (88 total passing unit tests).

---

## 🎉 BookSmith v2.0 Release Summary
All 25 Sprints across Phase 1, Phase 2, and Phase 3 have been successfully completed!
- **Core Engine:** PDF extraction (PdfPig), Regex text cleaner, running headers/footers remover, smart dialogue formatter, broken word repair.
- **Intelligence:** Front-matter & publisher credit filter (`IFrontMatterFilter`), Chapter structure & Table of Contents detector (`IChapterDetector`).
- **Exporting:** EPUB 2.0/3.0 multi-chapter exporter with custom cover image packaging, dynamic typography CSS, and TXT exporter.
- **Editor & UI:** 3-Step Navigation Flow (Import -> Processing -> Editor), Search & Replace, Undo/Redo, Live Counters, Chapter Navigator Sidebar, Preset Manager, Batch Queue Processor.

---

## Phase 4: Live Spell Checking & Visual Highlighting (v3.0 Roadmap)

### Sprint 26 - Live Spell Checker & Dictionary Engine (`ISpellChecker`)
**Status:** ✅ Completed  
**Goal:** Build a high-performance Turkish spell checking and word anomaly detection engine.
- Created `MisspelledWord` model and `ISpellChecker` interface in `BookSmith.Core`.
- Implemented `TurkishSpellChecker` in `BookSmith.Services/Cleaning` with a rich Turkish vocabulary dictionary, OCR consonant noise detector, and diacritic-aware Levenshtein edit distance suggestions.
- Registered `ISpellChecker` in `App.xaml.cs` Dependency Injection.
- Created `SpellCheckerTests` unit test suite (92 total passing tests).

### Sprint 27 - WPF Editor Spell Check Overlay & Toggle Switch
**Status:** ✅ Completed  
**Goal:** Highlight misspelled words in the editor, offer right-click suggestions, and add a toggle switch.
- Added `IsSpellCheckEnabled` property and `SpellCheckStatusText` badge (`🔴 Spell Check: ON (X anomalies)` / `⚪ Spell Check: OFF`) to `EditorViewModel`.
- Added `🔴 Spell Check` toggle button to `EditorView.xaml` quick-action toolbar.
- Implemented dynamic right-click ContextMenu in `EditorView.xaml.cs` (`EditorTextBox_ContextMenuOpening`) displaying word correction suggestions.
- Implemented `ApplySuggestionCommand` to perform single-click text replacements.
- Created `EditorSpellCheckTests` unit test suite (94 total passing tests).

---

## Phase 5: LLM-Powered Contextual Text Reconstruction (v3.0 Roadmap)

### Sprint 28 - LLM Service Provider Integration (`IAiReconstructionService`)
**Status:** ✅ Completed  
**Goal:** Connect local (Ollama / Local LLM) or cloud (Gemini / OpenAI API) LLMs to intelligently reconstruct corrupted OCR book paragraphs.
- Created `AiModelConfig` model supporting Ollama, OpenAI, and Gemini LLM providers.
- Created `IAiReconstructionService` interface in `BookSmith.Core`.
- Implemented `OllamaAiService` in `BookSmith.Services/Cleaning` with custom BookSmith AI Prompt Template (fixes OCR encoding bugs `vrdı` -> `vardı`, removes inline author names `ANDRZEJ SAPKOWSKI`, and preserves proper nouns).
- Registered `IAiReconstructionService` in `App.xaml.cs` Dependency Injection.
- Created `AiReconstructionServiceTests` unit test suite (98 total passing tests).

### Sprint 29 - Chunked Async AI Processing Pipeline 👈 **NEXT STEP**
**Goal:** Automatically detect garbage/corrupted paragraph density and send chunks to LLM for asynchronous reconstruction.
- **Tasks:**
  - Build `GarbageCharacterDensity` detector to identify paragraphs with high OCR corruption.
  - Implement chunking engine to send corrupted sections to `IAiReconstructionService`.
  - Integrate AI reconstruction step in `BookPipeline.Process()`.

### Sprint 29 - Chunked Async AI Processing Pipeline
**Goal:** Automatically detect garbage/corrupted paragraph density and send chunks to LLM for asynchronous reconstruction.

### Sprint 30 - AI Correction Assistant & Diff View
**Goal:** Interactive side-by-side comparison (Original vs AI Reconstructed) in the editor with single-click paragraph approval.
- **Editor & UI:** 3-Step Navigation Flow (Import -> Processing -> Editor), Search & Replace, Undo/Redo, Live Counters, Chapter Navigator Sidebar, Preset Manager, Batch Queue Processor.
