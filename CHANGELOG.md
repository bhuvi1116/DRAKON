# Changelog

## v5.0

Added:
- consolidated DRAKON Studio 5.0 release
- visual-first editor shell inspired by Visual Studio 2022
- DRAKON Auto-layout command in editor UI
- layout summary for spine/silhouette diagnostics
- release notes and packaging for v5.0

Improved:
- profile summaries and build pipeline hints
- editor title and version badges

## v0.27

Добавлено:
- `DrakonLayoutEngine` для auto-layout по шампуру и силуэту
- `DrakonLayoutOptions`, `DrakonLayoutReport`, `DrakonLayoutIssue`
- unit tests для базового layout профиля ДРАКОН
- документация `docs/editor-drakon-layout-engine-v0.md`

Изменено:
- visual-first профиль теперь имеет отдельный слой компоновки под правила ДРАКОН
- подготовлена база для следующего подключения layout прямо к canvas

Ограничения:
- авто-компоновка пока не подключена к UI-команде редактора


## v0.19

Добавлено:
- автоматическая привязка узлов к сетке после drag-and-drop на canvas
- документация `docs/editor-canvas-snap-v0.md`

Изменено:
- завершение drag теперь фиксирует итоговую позицию через ближайшую точку сетки
- history summary и canvas summary отражают привязку к сетке

## v0.17

Added:
- Avalonia canvas control for diagram rendering
- real grid rendering on canvas
- visual node selection by pointer press
- canvas selection status in editor UI

Improved:
- synchronized node selection between canvas and side panels

## v0.17

Added:
- Avalonia canvas control for diagram rendering
- real grid rendering on canvas
- visual node selection by pointer press
- canvas selection status in editor UI

Improved:
- synchronized node selection between canvas and side panels


## v0.16

Добавлено:
- `GridRenderService` как основа для визуальной отрисовки сетки canvas
- модель `GridLine` и `GridLineOrientation`
- unit tests для расчета линий сетки
- документация `docs/editor-grid-visual-foundation-v0.md`

Изменено:
- архитектура визуального слоя подготовлена к подключению сетки в Avalonia canvas

Ограничения:
- линии сетки пока не подключены к реальной отрисовке в canvas

## v0.15

Добавлено:
- `NodeDragService` как основа для drag-and-drop перемещения узлов
- unit tests для drag-update и grid snapping
- документация `docs/editor-dragdrop-foundation-v0.md`

Изменено:
- архитектура редактора подготовлена к подключению pointer events Avalonia

Ограничения:
- drag-and-drop пока не подключен к реальному canvas-контролу

## v0.14

Добавлено:
- редактирование координат выбранного узла в UI редактора
- привязка выбранного узла к сетке с настраиваемым шагом
- unit tests для перемещения узла и snap-to-grid
- документация `docs/editor-layout-and-grid-v0.md`

Изменено:
- `NodeItemViewModel` теперь показывает положение узла
- command-based Undo/Redo подключен к операциям изменения положения

Ограничения:
- перемещение пока выполняется через панель свойств, а не drag-and-drop на canvas
- визуальная сетка еще не отрисовывается

## v0.12

Добавлено:
- минимальные операции редактирования графа в редакторе: добавление и удаление узлов, изменение текста узла, добавление и удаление связей
- `DiagramEditService`
- модели представления узлов и связей для UI
- unit tests для редактирования графа
- документация `docs/editor-graph-editing-v0.md`

Изменено:
- snapshot-история Undo/Redo теперь подключена к операциям изменения графа
- главное окно редактора расширено списками узлов и связей и панелью редактирования

Ограничения:
- редактирование пока выполняется через списки, а не через полноценный canvas workflow
- история все еще snapshot-based


## v1.0

Added:
- open/save/save as workflow for `.drakon.json` files in the editor
- `DiagramFileService` for loading and saving diagram documents
- editor unit test for file persistence round-trip
- documentation for editor file workflow

Improved:
- the editor now keeps track of the current project file path
- template-created projects can be reopened and resaved as normal files

## v0.9

Added:
- template-based project creation directly in the desktop editor
- `TemplateBootstrapService` for UI bootstrap and save flow
- editor unit tests for template bootstrap
- documentation for editor template workflow

Improved:
- template creation now works both from CLI and from the Avalonia UI
- the editor can immediately load the generated diagram after creation

## v0.8

Added:
- CLI command `new` for project bootstrap from templates
- template catalog for hello-world, minimal, simple-branch and max-of-two
- format version compatibility warning in validator
- documentation for templates and versioning
- unit tests for template catalog, CLI bootstrap and version checks

Improved:
- project bootstrap no longer requires manual copying from samples

## v0.7

Added:
- CLI tool for validate/generate/build flows
- integration tests for end-to-end pipeline
- documentation for CLI and smoke checks

Improved:
- project is now easier to run in CI without the desktop editor

## v0.6

Added:
- интерактивные команды экспорта generated C-проекта
- интерактивная команда сборки и запуска через CMake
- панель build log и run log в редакторе
- обновленный статус пайплайна в основном окне

Changed:
- MainWindowViewModel теперь управляет export/build/run циклом
- MainWindow.axaml расширен до рабочего build dashboard

## v0.5

Добавлено:
- Проект `Build` для экспорта generated C-проекта.
- `GeneratedProjectExporter`, создающий `main.c` и `CMakeLists.txt`.
- `CMakeBuildService` для вызова `cmake`, сборки и запуска бинарника.
- Unit tests для экспортера сборочного слоя.
- Документация `docs/build-pipeline-v0.md`.
- Предпросмотр `CMakeLists.txt` и пайплайна сборки в редакторе.

Изменено:
- `Editor` теперь ссылается на `Build` и показывает пути exported артефактов.
- `DRAKON-NX.sln` очищен и дополнен новыми проектами `Build` и `Build.UnitTests`.

Ограничения:
- UI пока не содержит интерактивных кнопок запуска внешней сборки.
- Фактический вызов `cmake` зависит от наличия инструментов в системе пользователя.

## v0.4

Добавлено:
- Проект `CodeGen` для генерации C99-кода.
- Минимальный нормализатор диаграммы в `FlowNode` IR.
- Генерация `if/else`, последовательных действий и `main()`.
- Предпросмотр C-кода в редакторе.
- Golden tests для кодогенерации.
- Новые sample-диаграммы: `simple-branch` и `max-of-two`.

Изменено:
- `README.md` и документация расширены разделом про кодогенерацию.
- Solution обновлен новыми проектами и тестами.

Ограничения:
- Генерация пока ориентирована на ациклические диаграммы v0.
- Поддерживаются простые действия: присваивание, `print(...)`, и сырой оператор C.

## v0.11

Добавлено:
- Snapshot-история документа в `Editor`.
- Команды `Undo` и `Redo`.
- `DocumentHistoryService` и `DocumentHistoryState`.
- Изменение имени документа прямо из UI.
- Документация `docs/editor-history-v0.md`.
- Unit tests для истории изменений.

Изменено:
- `MainWindowViewModel` теперь отслеживает состояние Undo/Redo.
- `MainWindow.axaml` расширен блоком истории и кнопками Undo/Redo.

Ограничения:
- История пока snapshot-based.
- Canvas-операции будут подключены к истории на следующем шаге.

## v0.13

Added:
- command-based undo/redo history for graph editing
- delegate history actions and command history service
- tests for command history and state cloning
- documentation for editor command history

Improved:
- graph edit operations now go through explicit undoable actions
- history summary can reflect real undo/redo stack state

## v0.18

Added:
- Pointer-driven drag-and-drop for nodes on Avalonia canvas
- Drag finalization integrated with command-based Undo/Redo
- Canvas drag workflow documentation

Improved:
- Canvas selection now transitions directly into node dragging

## v0.21

Added:
- basic context operations on canvas for nodes and connections
- right-click menu for quick node deletion and connection workflow
- connection hit testing on canvas lines
- documentation for canvas context operations

Improved:
- canvas selection now supports node and connection context actions
- selected connection is highlighted on canvas


## v0.22

Added:
- quick connection mode on canvas with temporary preview line
- pending source node highlight during connection creation
- documentation for canvas quick-connect workflow

Improved:
- left-click on target node can now finish a pending connection directly on the diagram
- canvas render layer now supports temporary connection overlays

## v0.23

Added:
- cancel of quick-connect mode by Escape
- cancel of quick-connect mode by clicking empty canvas area
- visual highlight for valid connection targets during preview
- documentation for connection cancel and target highlight workflow

Improved:
- canvas connection mode now exposes clearer state transitions for quick-connect UX

## v0.24

Added:
- visual-first DRAKON profile and primitive/silhouette layout mode
- new icon kinds: title, branch start, address, question, select, case, loop markers
- silhouette sample template based on the provided DRAKON specification
- lane-aware serialization and visual validation for DRAKON schemes
- spec-aware canvas rendering with dedicated icon shapes and branch guides

Changed:
- editor now starts in a silhouette-oriented visual mode
- C code generation is disabled for the visual DRAKON profile and remains available for the executable subset only


## v0.26

Added:
- Visual Studio 2022 inspired editor shell
- light diagram surface with dark IDE chrome
- bezier-based smoothed connections on canvas
- refined minimalist node rendering for visual-first editing

Improved:
- canvas readability and panel hierarchy
- grid contrast and diagram focus
