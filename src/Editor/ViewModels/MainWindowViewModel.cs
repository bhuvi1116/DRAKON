using System.Collections.ObjectModel;
using System.Windows.Input;
using DrakonNx.Build.Model;
using DrakonNx.Build.Services;
using DrakonNx.CodeGen.C;
using DrakonNx.Core.Layout;
using DrakonNx.Core.Model;
using DrakonNx.Core.Services;
using DrakonNx.Editor.Commands;
using DrakonNx.Editor.Models;
using DrakonNx.Editor.Services;
using DrakonNx.Editor.Services.History;
using DrakonNx.Serialization.Json;
using DrakonNx.Validation.Services;

namespace DrakonNx.Editor.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private readonly DiagramValidator _validator = new();
    private readonly DiagramJsonSerializer _serializer = new();
    private readonly CodeGenerator _codeGenerator = new();
    private readonly GeneratedProjectExporter _exporter = new();
    private readonly CMakeBuildService _buildService = new();
    private readonly TemplateBootstrapService _templateBootstrapService = new();
    private readonly DiagramFileService _diagramFileService = new();
    private readonly CommandHistoryService _historyService = new();
    private readonly DiagramEditService _diagramEditService = new();
    private readonly NodeDragService _nodeDragService = new();
    private readonly DrakonLayoutEngine _layoutEngine = new();

    private DragSession? _dragSession;
    private string? _dragNodeId;
    private (double X, double Y)? _dragPreviewPosition;

    private DiagramDocument _document;
    private string _documentName = string.Empty;
    private int _nodeCount;
    private int _connectionCount;
    private string _validationSummary = string.Empty;
    private string _serializedPreview = string.Empty;
    private string _generatedCPreview = string.Empty;
    private string _generatedCMakePreview = string.Empty;
    private string _exportSummary = string.Empty;
    private string _buildPipelineSummary = string.Empty;
    private string _buildLog = "Сборка еще не запускалась.";
    private string _runLog = "Запуск еще не выполнялся.";
    private string _buildStatus = "Готово к экспорту.";
    private bool _isBusy;
    private string _selectedTemplateName = string.Empty;
    private string _newProjectName = "drakon-project";
    private string _newProjectPath = Path.Combine(Path.GetTempPath(), "drakon-nx-projects", "drakon-project.drakon.json");
    private string _templateCreationSummary = "Выберите шаблон, имя и путь, затем создайте новый проект.";
    private string _currentFilePath = string.Empty;
    private string _openFilePath = string.Empty;
    private string _saveAsFilePath = string.Empty;
    private string _editableDocumentName = string.Empty;
    private string _profileSummary = string.Empty;
    private string _historySummary = "История изменений пуста.";
    private bool _isDirty;
    private NodeItemViewModel? _selectedNode;
    private ConnectionItemViewModel? _selectedConnection;
    private string _selectedNodeText = string.Empty;
    private string _selectedNodeX = string.Empty;
    private string _selectedNodeY = string.Empty;
    private string _gridSize = "20";
    private NodeKind _newNodeKind = NodeKind.Action;
    private string _newNodeText = "новое действие";
    private NodeItemViewModel? _connectionFromNode;
    private NodeItemViewModel? _connectionToNode;
    private PortKind _selectedFromPort = PortKind.Out;
    private PortKind _selectedToPort = PortKind.In;
    private string _graphEditSummary = "Редактирование графа еще не выполнялось.";
    private string _canvasSummary = "Canvas работает в visual-first режиме ДРАКОН: силуэт, ветки, шампуры и быстрые связи.";
    private string _layoutSummary = "DRAKON Layout v5.0: авто-компоновка еще не выполнялась.";
    private ExportResult? _lastExportResult;
    private BuildResult? _lastBuildResult;

    public MainWindowViewModel()
    {
        _document = DiagramFactory.CreateDrakonSilhouetteSpecSample();

        AvailableTemplates = new ObservableCollection<string>(_templateBootstrapService.GetTemplateNames());
        AvailableNodeKinds = new ObservableCollection<NodeKind>(Enum.GetValues<NodeKind>());
        AvailablePorts = new ObservableCollection<PortKind>(Enum.GetValues<PortKind>());
        Nodes = new ObservableCollection<NodeItemViewModel>();
        Connections = new ObservableCollection<ConnectionItemViewModel>();
        _selectedTemplateName = AvailableTemplates.Contains("drakon-silhouette-spec") ? "drakon-silhouette-spec" : (AvailableTemplates.FirstOrDefault() ?? "minimal");
        _newProjectName = _document.Name;
        _newProjectPath = Path.Combine(Path.GetTempPath(), "drakon-nx-projects", SanitizePathName(_document.Name) + ".drakon.json");
        _currentFilePath = _newProjectPath;
        _openFilePath = _newProjectPath;
        _saveAsFilePath = _newProjectPath;
        _editableDocumentName = _document.Name;

        ExportCommand = new DelegateCommand(ExportProject, () => !IsBusy);
        BuildAndRunCommand = new DelegateCommand(BuildAndRun, () => !IsBusy);
        RefreshPreviewCommand = new DelegateCommand(RefreshPreviews, () => !IsBusy);
        ApplyAutoLayoutCommand = new DelegateCommand(ApplyAutoLayout, () => !IsBusy);
        CreateFromTemplateCommand = new DelegateCommand(CreateFromTemplate, () => !IsBusy && !string.IsNullOrWhiteSpace(SelectedTemplateName));
        OpenCommand = new DelegateCommand(OpenDocument, () => !IsBusy && !string.IsNullOrWhiteSpace(OpenFilePath));
        SaveCommand = new DelegateCommand(SaveDocument, () => !IsBusy && !string.IsNullOrWhiteSpace(CurrentFilePath));
        SaveAsCommand = new DelegateCommand(SaveDocumentAs, () => !IsBusy && !string.IsNullOrWhiteSpace(SaveAsFilePath));
        ApplyDocumentNameCommand = new DelegateCommand(ApplyDocumentName, () => !IsBusy && !string.IsNullOrWhiteSpace(EditableDocumentName));
        UndoCommand = new DelegateCommand(Undo, () => !IsBusy && _historyService.CanUndo);
        RedoCommand = new DelegateCommand(Redo, () => !IsBusy && _historyService.CanRedo);
        AddNodeCommand = new DelegateCommand(AddNode, () => !IsBusy && !string.IsNullOrWhiteSpace(NewNodeText));
        DeleteSelectedNodeCommand = new DelegateCommand(DeleteSelectedNode, () => !IsBusy && SelectedNode is not null);
        ApplySelectedNodeTextCommand = new DelegateCommand(ApplySelectedNodeText, () => !IsBusy && SelectedNode is not null);
        ApplySelectedNodePositionCommand = new DelegateCommand(ApplySelectedNodePosition, () => !IsBusy && SelectedNode is not null && !string.IsNullOrWhiteSpace(SelectedNodeX) && !string.IsNullOrWhiteSpace(SelectedNodeY));
        SnapSelectedNodeToGridCommand = new DelegateCommand(SnapSelectedNodeToGrid, () => !IsBusy && SelectedNode is not null && !string.IsNullOrWhiteSpace(GridSize));
        AddConnectionCommand = new DelegateCommand(AddConnection, () => !IsBusy && ConnectionFromNode is not null && ConnectionToNode is not null);
        DeleteSelectedConnectionCommand = new DelegateCommand(DeleteSelectedConnection, () => !IsBusy && SelectedConnection is not null);

        SynchronizeDocumentStats();
        RefreshPreviews();
    }

    public ObservableCollection<string> AvailableTemplates { get; }
    public ObservableCollection<NodeKind> AvailableNodeKinds { get; }
    public ObservableCollection<PortKind> AvailablePorts { get; }
    public ObservableCollection<NodeItemViewModel> Nodes { get; }
    public ObservableCollection<ConnectionItemViewModel> Connections { get; }

    public string SelectedTemplateName
    {
        get => _selectedTemplateName;
        set
        {
            if (_selectedTemplateName == value) return;
            _selectedTemplateName = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public string NewProjectName
    {
        get => _newProjectName;
        set
        {
            if (_newProjectName == value) return;
            _newProjectName = value;
            RaisePropertyChanged();
        }
    }

    public string NewProjectPath
    {
        get => _newProjectPath;
        set
        {
            if (_newProjectPath == value) return;
            _newProjectPath = value;
            RaisePropertyChanged();
        }
    }

    public NodeKind NewNodeKind
    {
        get => _newNodeKind;
        set
        {
            if (_newNodeKind == value) return;
            _newNodeKind = value;
            RaisePropertyChanged();
        }
    }

    public string NewNodeText
    {
        get => _newNodeText;
        set
        {
            if (_newNodeText == value) return;
            _newNodeText = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public NodeItemViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (_selectedNode == value) return;
            _selectedNode = value;
            SelectedNodeText = value?.Text ?? string.Empty;
            SelectedNodeX = value is null ? string.Empty : value.X.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            SelectedNodeY = value is null ? string.Empty : value.Y.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public string SelectedNodeText
    {
        get => _selectedNodeText;
        set
        {
            if (_selectedNodeText == value) return;
            _selectedNodeText = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public string SelectedNodeX
    {
        get => _selectedNodeX;
        set
        {
            if (_selectedNodeX == value) return;
            _selectedNodeX = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public string SelectedNodeY
    {
        get => _selectedNodeY;
        set
        {
            if (_selectedNodeY == value) return;
            _selectedNodeY = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public string GridSize
    {
        get => _gridSize;
        set
        {
            if (_gridSize == value) return;
            _gridSize = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public ConnectionItemViewModel? SelectedConnection
    {
        get => _selectedConnection;
        set
        {
            if (_selectedConnection == value) return;
            _selectedConnection = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public NodeItemViewModel? ConnectionFromNode
    {
        get => _connectionFromNode;
        set
        {
            if (_connectionFromNode == value) return;
            _connectionFromNode = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public NodeItemViewModel? ConnectionToNode
    {
        get => _connectionToNode;
        set
        {
            if (_connectionToNode == value) return;
            _connectionToNode = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public PortKind SelectedFromPort
    {
        get => _selectedFromPort;
        set
        {
            if (_selectedFromPort == value) return;
            _selectedFromPort = value;
            RaisePropertyChanged();
        }
    }

    public PortKind SelectedToPort
    {
        get => _selectedToPort;
        set
        {
            if (_selectedToPort == value) return;
            _selectedToPort = value;
            RaisePropertyChanged();
        }
    }

    public string GraphEditSummary
    {
        get => _graphEditSummary;
        private set
        {
            if (_graphEditSummary == value) return;
            _graphEditSummary = value;
            RaisePropertyChanged();
        }
    }

    public string CanvasSummary
    {
        get => _canvasSummary;
        private set
        {
            if (_canvasSummary == value) return;
            _canvasSummary = value;
            RaisePropertyChanged();
        }
    }


    public string LayoutSummary
    {
        get => _layoutSummary;
        private set
        {
            if (_layoutSummary == value) return;
            _layoutSummary = value;
            RaisePropertyChanged();
        }
    }

    public string ProfileSummary
    {
        get => _profileSummary;
        private set
        {
            if (_profileSummary == value) return;
            _profileSummary = value;
            RaisePropertyChanged();
        }
    }

    public double GridSizeValue
    {
        get
        {
            return double.TryParse(GridSize, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value) && value > 0
                ? value
                : 20d;
        }
    }

    public string CurrentFilePath
    {
        get => _currentFilePath;
        private set
        {
            if (_currentFilePath == value) return;
            _currentFilePath = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public string OpenFilePath
    {
        get => _openFilePath;
        set
        {
            if (_openFilePath == value) return;
            _openFilePath = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public string SaveAsFilePath
    {
        get => _saveAsFilePath;
        set
        {
            if (_saveAsFilePath == value) return;
            _saveAsFilePath = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public string EditableDocumentName
    {
        get => _editableDocumentName;
        set
        {
            if (_editableDocumentName == value) return;
            _editableDocumentName = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public string HistorySummary
    {
        get => _historySummary;
        private set
        {
            if (_historySummary == value) return;
            _historySummary = value;
            RaisePropertyChanged();
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            if (_isDirty == value) return;
            _isDirty = value;
            RaisePropertyChanged();
        }
    }

    public string TemplateCreationSummary
    {
        get => _templateCreationSummary;
        private set
        {
            if (_templateCreationSummary == value) return;
            _templateCreationSummary = value;
            RaisePropertyChanged();
        }
    }

    public string DocumentName
    {
        get => _documentName;
        private set
        {
            if (_documentName == value) return;
            _documentName = value;
            RaisePropertyChanged();
        }
    }

    public int NodeCount
    {
        get => _nodeCount;
        private set
        {
            if (_nodeCount == value) return;
            _nodeCount = value;
            RaisePropertyChanged();
        }
    }

    public int ConnectionCount
    {
        get => _connectionCount;
        private set
        {
            if (_connectionCount == value) return;
            _connectionCount = value;
            RaisePropertyChanged();
        }
    }

    public string ValidationSummary
    {
        get => _validationSummary;
        private set
        {
            if (_validationSummary == value) return;
            _validationSummary = value;
            RaisePropertyChanged();
        }
    }

    public string SerializedPreview
    {
        get => _serializedPreview;
        private set
        {
            if (_serializedPreview == value) return;
            _serializedPreview = value;
            RaisePropertyChanged();
        }
    }

    public string GeneratedCPreview
    {
        get => _generatedCPreview;
        private set
        {
            if (_generatedCPreview == value) return;
            _generatedCPreview = value;
            RaisePropertyChanged();
        }
    }

    public string GeneratedCMakePreview
    {
        get => _generatedCMakePreview;
        private set
        {
            if (_generatedCMakePreview == value) return;
            _generatedCMakePreview = value;
            RaisePropertyChanged();
        }
    }

    public string ExportSummary
    {
        get => _exportSummary;
        private set
        {
            if (_exportSummary == value) return;
            _exportSummary = value;
            RaisePropertyChanged();
        }
    }

    public string BuildPipelineSummary
    {
        get => _buildPipelineSummary;
        private set
        {
            if (_buildPipelineSummary == value) return;
            _buildPipelineSummary = value;
            RaisePropertyChanged();
        }
    }

    public string BuildLog
    {
        get => _buildLog;
        private set
        {
            if (_buildLog == value) return;
            _buildLog = value;
            RaisePropertyChanged();
        }
    }

    public string RunLog
    {
        get => _runLog;
        private set
        {
            if (_runLog == value) return;
            _runLog = value;
            RaisePropertyChanged();
        }
    }

    public string BuildStatus
    {
        get => _buildStatus;
        private set
        {
            if (_buildStatus == value) return;
            _buildStatus = value;
            RaisePropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value) return;
            _isBusy = value;
            RaisePropertyChanged();
            RaiseCommandStates();
        }
    }

    public ICommand ExportCommand { get; }
    public ICommand BuildAndRunCommand { get; }
    public ICommand RefreshPreviewCommand { get; }
    public ICommand ApplyAutoLayoutCommand { get; }
    public ICommand CreateFromTemplateCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }
    public ICommand ApplyDocumentNameCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand RedoCommand { get; }
    public ICommand AddNodeCommand { get; }
    public ICommand DeleteSelectedNodeCommand { get; }
    public ICommand ApplySelectedNodeTextCommand { get; }
    public ICommand ApplySelectedNodePositionCommand { get; }
    public ICommand SnapSelectedNodeToGridCommand { get; }
    public ICommand AddConnectionCommand { get; }
    public ICommand DeleteSelectedConnectionCommand { get; }

    private void RefreshPreviews()
    {
        SynchronizeDocumentStats();

        var issues = _validator.Validate(_document);
        ValidationSummary = issues.Count == 0
            ? "Валидация: ошибок нет"
            : $"Валидация: найдено ошибок: {issues.Count}";
        SerializedPreview = _serializer.Serialize(_document);
        GeneratedCPreview = issues.Count == 0
            ? _codeGenerator.Generate(_document)
            : "Код не сгенерирован: диаграмма содержит ошибки.";

        BuildPipelineSummary = string.Join(Environment.NewLine, new[]
        {
            "Шаг 1: создать проект из шаблона и сохранить .drakon.json",
            "Шаг 2: при необходимости выполнить DRAKON Auto-layout v5.0",
            "Шаг 3: экспорт диаграммы в отдельный каталог",
            "Шаг 4: cmake -S <export-dir> -B <export-dir>/build",
            "Шаг 5: cmake --build <export-dir>/build --config Release",
            "Шаг 6: запуск бинарника и сбор stdout/stderr"
        });

        if (_lastExportResult is not null)
        {
            GeneratedCMakePreview = _lastExportResult.CMakeLists;
            ExportSummary = string.Join(Environment.NewLine, new[]
            {
                $"Каталог экспорта: {_lastExportResult.Layout.OutputDirectory}",
                $"main.c: {_lastExportResult.Layout.MainSourcePath}",
                $"CMakeLists.txt: {_lastExportResult.Layout.CMakeListsPath}",
                $"Ожидаемый бинарник: {_lastExportResult.Layout.BinaryPath}"
            });
        }
        else
        {
            GeneratedCMakePreview = "Выполните экспорт, чтобы получить CMakeLists.txt.";
            ExportSummary = "Экспорт еще не выполнялся.";
        }
    }

    private async void CreateFromTemplate()
    {
        await RunBusyActionAsync(async () =>
        {
            var normalizedProjectName = string.IsNullOrWhiteSpace(NewProjectName) ? "drakon-project" : NewProjectName.Trim();
            var normalizedPath = string.IsNullOrWhiteSpace(NewProjectPath)
                ? Path.Combine(Path.GetTempPath(), "drakon-nx-projects", SanitizePathName(normalizedProjectName) + ".drakon.json")
                : NewProjectPath.Trim();

            var result = _templateBootstrapService.CreateAndSave(SelectedTemplateName, normalizedProjectName, normalizedPath);
            _document = result.Document;
            _lastExportResult = null;
            _lastBuildResult = null;
            _historyService.Reset();
            NewProjectName = _document.Name;
            NewProjectPath = result.OutputPath;
            CurrentFilePath = result.OutputPath;
            OpenFilePath = result.OutputPath;
            SaveAsFilePath = result.OutputPath;
            EditableDocumentName = _document.Name;
            IsDirty = false;
            UpdateHistorySummary("История сброшена после создания проекта из шаблона.");

            await UiDispatcher.InvokeAsync(() =>
            {
                TemplateCreationSummary = string.Join(Environment.NewLine, new[]
                {
                    "Новый проект создан из шаблона.",
                    $"Шаблон: {SelectedTemplateName}",
                    $"Имя: {_document.Name}",
                    $"Файл: {result.OutputPath}"
                });
                BuildStatus = "Новый проект загружен в редактор.";
                BuildLog = "Проект создан из шаблона. Экспорт и сборка еще не запускались.";
                RunLog = "Запуск еще не выполнялся.";
            });
        });
    }

    private async void OpenDocument()
    {
        await RunBusyActionAsync(async () =>
        {
            var filePath = OpenFilePath.Trim();
            var document = _diagramFileService.Load(filePath);
            _document = document;
            _lastExportResult = null;
            _lastBuildResult = null;
            _historyService.Reset();
            CurrentFilePath = filePath;
            OpenFilePath = filePath;
            SaveAsFilePath = filePath;
            NewProjectName = document.Name;
            NewProjectPath = filePath;
            EditableDocumentName = document.Name;
            IsDirty = false;
            UpdateHistorySummary("История сброшена после открытия файла.");

            await UiDispatcher.InvokeAsync(() =>
            {
                TemplateCreationSummary = string.Join(Environment.NewLine, new[]
                {
                    "Проект открыт из файла.",
                    $"Имя: {document.Name}",
                    $"Файл: {filePath}"
                });
                BuildStatus = "Проект открыт.";
                BuildLog = "Файл диаграммы успешно загружен.";
                RunLog = "Запуск еще не выполнялся.";
            });
        });
    }

    private async void SaveDocument()
    {
        await RunBusyActionAsync(async () =>
        {
            var filePath = CurrentFilePath.Trim();
            _diagramFileService.Save(_document, filePath);
            IsDirty = false;
            UpdateHistorySummary("Документ сохранен. История Undo/Redo оставлена в памяти.");

            await UiDispatcher.InvokeAsync(() =>
            {
                TemplateCreationSummary = string.Join(Environment.NewLine, new[]
                {
                    "Проект сохранен.",
                    $"Имя: {_document.Name}",
                    $"Файл: {filePath}"
                });
                BuildStatus = "Проект сохранен.";
                BuildLog = "Файл диаграммы записан на диск.";
            });
        });
    }

    private async void SaveDocumentAs()
    {
        await RunBusyActionAsync(async () =>
        {
            var filePath = SaveAsFilePath.Trim();
            _diagramFileService.Save(_document, filePath);
            CurrentFilePath = filePath;
            OpenFilePath = filePath;
            NewProjectPath = filePath;
            IsDirty = false;
            UpdateHistorySummary("Документ сохранен под новым именем. История Undo/Redo оставлена в памяти.");

            await UiDispatcher.InvokeAsync(() =>
            {
                TemplateCreationSummary = string.Join(Environment.NewLine, new[]
                {
                    "Проект сохранен под новым именем.",
                    $"Имя: {_document.Name}",
                    $"Файл: {filePath}"
                });
                BuildStatus = "Проект сохранен как новый файл.";
                BuildLog = "Создан новый файл диаграммы.";
            });
        });
    }

    private async void ApplyDocumentName()
    {
        await RunBusyActionAsync(async () =>
        {
            var trimmed = string.IsNullOrWhiteSpace(EditableDocumentName) ? "Untitled" : EditableDocumentName.Trim();
            if (string.Equals(trimmed, _document.Name, StringComparison.Ordinal))
            {
                UpdateHistorySummary("Имя документа не изменилось.");
                return;
            }

            var before = CreateHistoryState($"Переименование '{_document.Name}'");
            var afterDocument = HistoryStateCloner.CloneDocument(_document);
            afterDocument.Name = trimmed;
            var after = new DocumentHistoryState(afterDocument, CurrentFilePath, OpenFilePath, SaveAsFilePath, true, $"Переименование документа в '{trimmed}'");
            _historyService.Execute(new DelegateHistoryAction(
                after.Description,
                () => RestoreState(before),
                () => RestoreState(after)));
            _lastExportResult = null;
            _lastBuildResult = null;
            NewProjectName = trimmed;
            EditableDocumentName = trimmed;
            IsDirty = true;
            UpdateHistorySummary($"Документ переименован в '{trimmed}'.");

            await UiDispatcher.InvokeAsync(() =>
            {
                TemplateCreationSummary = string.Join(Environment.NewLine, new[]
                {
                    "Имя документа обновлено.",
                    $"Новое имя: {trimmed}"
                });
                BuildStatus = "Документ изменен.";
            });
        });
    }

    private async void Undo()
    {
        await RunBusyActionAsync(async () =>
        {
            var description = _historyService.Undo();
            UpdateHistorySummary($"Undo выполнен: {description}");

            await UiDispatcher.InvokeAsync(() =>
            {
                TemplateCreationSummary = "Последнее изменение отменено.";
                BuildStatus = "Undo выполнен.";
            });
        });
    }

    private async void Redo()
    {
        await RunBusyActionAsync(async () =>
        {
            var description = _historyService.Redo();
            UpdateHistorySummary($"Redo выполнен: {description}");

            await UiDispatcher.InvokeAsync(() =>
            {
                TemplateCreationSummary = "Последнее изменение восстановлено.";
                BuildStatus = "Redo выполнен.";
            });
        });
    }

    private async void AddNode()
    {
        await RunBusyActionAsync(async () =>
        {
            var nodeText = string.IsNullOrWhiteSpace(NewNodeText) ? NewNodeKind.ToString() : NewNodeText.Trim();
            DiagramNode? createdNode = null;
            _historyService.Execute(new DelegateHistoryAction(
                $"Добавление узла {NewNodeKind}",
                () =>
                {
                    if (createdNode is not null)
                    {
                        _diagramEditService.DeleteNode(_document, createdNode.Id);
                        SelectedNode = null;
                    }
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                },
                () =>
                {
                    createdNode ??= _diagramEditService.AddNode(_document, NewNodeKind, nodeText);
                    if (_document.FindNode(createdNode.Id) is null)
                    {
                        _document.Nodes.Add(createdNode);
                    }
                    SelectedNode = null;
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                }));
            var node = createdNode!;
            UpdateHistorySummary($"Добавлен узел {node.Id}.");

            await UiDispatcher.InvokeAsync(() =>
            {
                GraphEditSummary = $"Добавлен узел {node.Kind} [{node.Id}] с текстом '{node.Text}'.";
                BuildStatus = "Граф изменен.";
            });
        });
    }

    private async void DeleteSelectedNode()
    {
        await RunBusyActionAsync(async () =>
        {
            if (SelectedNode is null)
            {
                GraphEditSummary = "Узел для удаления не выбран.";
                return;
            }

            var nodeId = SelectedNode.Id;
            var snapshot = HistoryStateCloner.Clone(CreateHistoryState($"Удаление узла {nodeId}"));
            var removed = false;
            _historyService.Execute(new DelegateHistoryAction(
                $"Удаление узла {nodeId}",
                () =>
                {
                    RestoreState(snapshot);
                },
                () =>
                {
                    removed = _diagramEditService.DeleteNode(_document, nodeId);
                    SelectedNode = null;
                    SelectedConnection = null;
                    ConnectionFromNode = null;
                    ConnectionToNode = null;
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                }));

            if (removed)
            {
                UpdateHistorySummary($"Удален узел {nodeId}.");
            }

            await UiDispatcher.InvokeAsync(() =>
            {
                GraphEditSummary = removed ? $"Удален узел {nodeId} и все связанные связи." : $"Узел {nodeId} не найден.";
                BuildStatus = removed ? "Граф изменен." : "Удаление не выполнено.";
            });
        });
    }

    private async void ApplySelectedNodeText()
    {
        await RunBusyActionAsync(async () =>
        {
            if (SelectedNode is null)
            {
                GraphEditSummary = "Узел не выбран.";
                return;
            }

            var newText = SelectedNodeText ?? string.Empty;
            var currentNode = _document.FindNode(SelectedNode.Id);
            if (currentNode is null)
            {
                GraphEditSummary = "Выбранный узел не найден в документе.";
                return;
            }

            if (string.Equals(currentNode.Text, newText, StringComparison.Ordinal))
            {
                GraphEditSummary = "Текст узла не изменился.";
                return;
            }

            var nodeId = SelectedNode.Id;
            var oldText = currentNode.Text;
            _historyService.Execute(new DelegateHistoryAction(
                $"Изменение текста узла {nodeId}",
                () =>
                {
                    _diagramEditService.UpdateNodeText(_document, nodeId, oldText);
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                },
                () =>
                {
                    _diagramEditService.UpdateNodeText(_document, nodeId, newText);
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                }));
            UpdateHistorySummary($"Обновлен текст узла {nodeId}.");

            await UiDispatcher.InvokeAsync(() =>
            {
                GraphEditSummary = $"Текст узла {SelectedNode.Id} обновлен.";
                BuildStatus = "Граф изменен.";
            });
        });
    }


    private async void ApplySelectedNodePosition()
    {
        await RunBusyActionAsync(async () =>
        {
            if (SelectedNode is null)
            {
                GraphEditSummary = "Узел не выбран.";
                return;
            }

            if (!TryParseCoordinate(SelectedNodeX, out var newX) || !TryParseCoordinate(SelectedNodeY, out var newY))
            {
                GraphEditSummary = "Координаты узла должны быть числами.";
                return;
            }

            var currentNode = _document.FindNode(SelectedNode.Id);
            if (currentNode is null)
            {
                GraphEditSummary = "Выбранный узел не найден в документе.";
                return;
            }

            if (Math.Abs(currentNode.X - newX) < 0.0001 && Math.Abs(currentNode.Y - newY) < 0.0001)
            {
                GraphEditSummary = "Положение узла не изменилось.";
                return;
            }

            var nodeId = SelectedNode.Id;
            var oldX = currentNode.X;
            var oldY = currentNode.Y;
            _historyService.Execute(new DelegateHistoryAction(
                $"Перемещение узла {nodeId}",
                () =>
                {
                    _diagramEditService.UpdateNodePosition(_document, nodeId, oldX, oldY);
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                },
                () =>
                {
                    _diagramEditService.UpdateNodePosition(_document, nodeId, newX, newY);
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                }));
            UpdateHistorySummary($"Положение узла {nodeId} обновлено.");

            await UiDispatcher.InvokeAsync(() =>
            {
                GraphEditSummary = $"Узел {nodeId} перемещен в ({newX:0.##}, {newY:0.##}).";
                BuildStatus = "Граф изменен.";
            });
        });
    }

    private async void SnapSelectedNodeToGrid()
    {
        await RunBusyActionAsync(async () =>
        {
            if (SelectedNode is null)
            {
                GraphEditSummary = "Узел не выбран.";
                return;
            }

            if (!TryParseCoordinate(GridSize, out var gridSize) || gridSize <= 0)
            {
                GraphEditSummary = "Размер сетки должен быть положительным числом.";
                return;
            }

            var currentNode = _document.FindNode(SelectedNode.Id);
            if (currentNode is null)
            {
                GraphEditSummary = "Выбранный узел не найден в документе.";
                return;
            }

            var oldX = currentNode.X;
            var oldY = currentNode.Y;
            var snappedX = Math.Round(currentNode.X / gridSize) * gridSize;
            var snappedY = Math.Round(currentNode.Y / gridSize) * gridSize;

            if (Math.Abs(oldX - snappedX) < 0.0001 && Math.Abs(oldY - snappedY) < 0.0001)
            {
                GraphEditSummary = "Узел уже выровнен по сетке.";
                return;
            }

            var nodeId = SelectedNode.Id;
            _historyService.Execute(new DelegateHistoryAction(
                $"Привязка узла {nodeId} к сетке",
                () =>
                {
                    _diagramEditService.UpdateNodePosition(_document, nodeId, oldX, oldY);
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                },
                () =>
                {
                    _diagramEditService.SnapNodeToGrid(_document, nodeId, gridSize);
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                }));
            UpdateHistorySummary($"Узел {nodeId} выровнен по сетке.");

            await UiDispatcher.InvokeAsync(() =>
            {
                GraphEditSummary = $"Узел {nodeId} привязан к сетке {gridSize:0.##}.";
                BuildStatus = "Граф изменен.";
            });
        });
    }

    private async void AddConnection()
    {
        await RunBusyActionAsync(async () =>
        {
            if (ConnectionFromNode is null || ConnectionToNode is null)
            {
                GraphEditSummary = "Выберите оба узла для связи.";
                return;
            }

            DiagramConnection? createdConnection = null;
            _historyService.Execute(new DelegateHistoryAction(
                $"Добавление связи {ConnectionFromNode.Id}->{ConnectionToNode.Id}",
                () =>
                {
                    if (createdConnection is not null)
                    {
                        _diagramEditService.DeleteConnection(_document, createdConnection.Id);
                    }
                    SelectedConnection = null;
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                },
                () =>
                {
                    createdConnection ??= _diagramEditService.AddConnection(_document, ConnectionFromNode.Id, SelectedFromPort, ConnectionToNode.Id, SelectedToPort);
                    if (_document.Connections.All(c => c.Id != createdConnection.Id))
                    {
                        _document.Connections.Add(createdConnection);
                    }
                    SelectedConnection = null;
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                }));
            var connection = createdConnection!;
            UpdateHistorySummary($"Добавлена связь {connection.Id}.");

            await UiDispatcher.InvokeAsync(() =>
            {
                GraphEditSummary = $"Добавлена связь {connection.Id}: {connection.FromNodeId}.{connection.FromPort} -> {connection.ToNodeId}.{connection.ToPort}.";
                BuildStatus = "Граф изменен.";
            });
        });
    }

    private async void DeleteSelectedConnection()
    {
        await RunBusyActionAsync(async () =>
        {
            if (SelectedConnection is null)
            {
                GraphEditSummary = "Связь для удаления не выбрана.";
                return;
            }

            var connectionId = SelectedConnection.Id;
            var snapshot = HistoryStateCloner.Clone(CreateHistoryState($"Удаление связи {connectionId}"));
            var removed = false;
            _historyService.Execute(new DelegateHistoryAction(
                $"Удаление связи {connectionId}",
                () => RestoreState(snapshot),
                () =>
                {
                    removed = _diagramEditService.DeleteConnection(_document, connectionId);
                    SelectedConnection = null;
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                }));

            if (removed)
            {
                UpdateHistorySummary($"Удалена связь {connectionId}.");
            }

            await UiDispatcher.InvokeAsync(() =>
            {
                GraphEditSummary = removed ? $"Удалена связь {connectionId}." : $"Связь {connectionId} не найдена.";
                BuildStatus = removed ? "Граф изменен." : "Удаление не выполнено.";
            });
        });
    }

    private async void ApplyAutoLayout()
    {
        await RunBusyActionAsync(async () =>
        {
            var before = HistoryStateCloner.Clone(CreateHistoryState($"Auto-layout {_document.Name}"));
            DrakonLayoutReport? report = null;
            _historyService.Execute(new DelegateHistoryAction(
                "DRAKON Auto-layout v5.0",
                () =>
                {
                    RestoreState(before);
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                },
                () =>
                {
                    report = _layoutEngine.Apply(_document, new DrakonLayoutOptions());
                    InvalidateGeneratedArtifacts();
                    IsDirty = true;
                }));

            var effectiveReport = report ?? new DrakonLayoutReport();
            LayoutSummary = string.Join(Environment.NewLine, new[]
            {
                "DRAKON Auto-layout v5.0 выполнен.",
                $"Silhouette: {effectiveReport.AppliedSilhouette}",
                $"Repositioned nodes: {effectiveReport.RepositionedNodes}",
                $"Issues: {effectiveReport.Issues.Count}"
            }.Concat(effectiveReport.Issues.Select(i => $"- {i.Code}: {i.Message}")));
            UpdateHistorySummary("Выполнен DRAKON Auto-layout v5.0.");

            await UiDispatcher.InvokeAsync(() =>
            {
                GraphEditSummary = $"Auto-layout перестроил {effectiveReport.RepositionedNodes} узлов.";
                CanvasSummary = effectiveReport.Issues.Count == 0
                    ? "Canvas: auto-layout завершен, нарушений шампура не найдено."
                    : $"Canvas: auto-layout завершен, замечаний: {effectiveReport.Issues.Count}.";
                BuildStatus = "DRAKON Auto-layout v5.0 выполнен.";
            });
        });
    }

    private async void ExportProject()
    {
        await RunBusyActionAsync(async () =>
        {
            var exportDirectory = Path.Combine(Path.GetTempPath(), "drakon-nx-preview", SanitizePathName(_document.Name));
            var exportResult = _exporter.Export(_document, exportDirectory);
            _lastExportResult = exportResult;

            await UiDispatcher.InvokeAsync(() =>
            {
                GeneratedCMakePreview = exportResult.CMakeLists;
                ExportSummary = string.Join(Environment.NewLine, new[]
                {
                    $"Каталог экспорта: {exportResult.Layout.OutputDirectory}",
                    $"main.c: {exportResult.Layout.MainSourcePath}",
                    $"CMakeLists.txt: {exportResult.Layout.CMakeListsPath}",
                    $"Ожидаемый бинарник: {exportResult.Layout.BinaryPath}"
                });
                BuildStatus = "Экспорт завершен.";
                BuildLog = "Файлы проекта экспортированы. Готово к сборке.";
            });
        });
    }

    private async void BuildAndRun()
    {
        await RunBusyActionAsync(async () =>
        {
            if (_lastExportResult is null)
            {
                _lastExportResult = _exporter.Export(_document, Path.Combine(Path.GetTempPath(), "drakon-nx-preview", SanitizePathName(_document.Name)));
            }

            await UiDispatcher.InvokeAsync(() =>
            {
                BuildStatus = "Выполняется configure/build/run...";
                BuildLog = "Запуск внешних инструментов...";
                RunLog = "Ожидание завершения процесса...";
            });

            var buildResult = await _buildService.ConfigureBuildAndRunAsync(_lastExportResult.Layout);
            _lastBuildResult = buildResult;

            await UiDispatcher.InvokeAsync(() =>
            {
                BuildStatus = buildResult.Succeeded ? "Сборка и запуск выполнены успешно." : $"Ошибка: {buildResult.ErrorMessage ?? "неизвестная ошибка"}";
                BuildLog = ComposeBuildLog(buildResult);
                RunLog = string.IsNullOrWhiteSpace(buildResult.RunLog)
                    ? "Исполняемый файл не запускался или не дал вывода."
                    : buildResult.RunLog;
            });
        });
    }

    private async Task RunBusyActionAsync(Func<Task> action)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await action();
        }
        catch (Exception ex)
        {
            BuildStatus = $"Ошибка: {ex.Message}";
            BuildLog = ex.ToString();
        }
        finally
        {
            IsBusy = false;
            RefreshPreviews();
        }
    }

    private DocumentHistoryState CreateHistoryState(string description) =>
        new(_document, CurrentFilePath, OpenFilePath, SaveAsFilePath, IsDirty, description);

    private void RestoreState(DocumentHistoryState state)
    {
        _document = state.Document;
        CurrentFilePath = state.CurrentFilePath;
        OpenFilePath = state.OpenFilePath;
        SaveAsFilePath = state.SaveAsFilePath;
        NewProjectPath = string.IsNullOrWhiteSpace(state.CurrentFilePath) ? NewProjectPath : state.CurrentFilePath;
        NewProjectName = state.Document.Name;
        EditableDocumentName = state.Document.Name;
        IsDirty = state.IsDirty;
        _lastExportResult = null;
        _lastBuildResult = null;
    }

    private void UpdateHistorySummary(string message)
    {
        HistorySummary = string.Join(Environment.NewLine, new[]
        {
            message,
            $"CanUndo: {_historyService.CanUndo}",
            $"Undo top: {_historyService.CurrentUndoDescription}",
            $"CanRedo: {_historyService.CanRedo}",
            $"Redo top: {_historyService.CurrentRedoDescription}"
        });
    }

    private void InvalidateGeneratedArtifacts()
    {
        _lastExportResult = null;
        _lastBuildResult = null;
    }

    private void RaiseCommandStates()
    {
        foreach (var command in new ICommand[]
                 {
                     ExportCommand, BuildAndRunCommand, RefreshPreviewCommand, ApplyAutoLayoutCommand, CreateFromTemplateCommand,
                     OpenCommand, SaveCommand, SaveAsCommand, ApplyDocumentNameCommand, UndoCommand, RedoCommand,
                     AddNodeCommand, DeleteSelectedNodeCommand, ApplySelectedNodeTextCommand, ApplySelectedNodePositionCommand, SnapSelectedNodeToGridCommand, AddConnectionCommand, DeleteSelectedConnectionCommand
                 })
        {
            if (command is DelegateCommand delegateCommand)
            {
                delegateCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private static bool TryParseCoordinate(string value, out double result) =>
        double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);

    private void SynchronizeDocumentStats()
    {
        DocumentName = _document.Name;
        NodeCount = _document.Nodes.Count;
        ConnectionCount = _document.Connections.Count;
        EditableDocumentName = _document.Name;

        Nodes.Clear();
        foreach (var node in _document.Nodes.OrderBy(n => n.Id, StringComparer.Ordinal))
        {
            Nodes.Add(new NodeItemViewModel(node.Id, node.Kind, node.Text, node.X, node.Y, node.Lane));
        }

        Connections.Clear();
        foreach (var connection in _document.Connections.OrderBy(c => c.Id, StringComparer.Ordinal))
        {
            Connections.Add(new ConnectionItemViewModel(connection.Id, connection.FromNodeId, connection.FromPort, connection.ToNodeId, connection.ToPort));
        }

        SelectedNode = SelectedNode is null ? null : Nodes.FirstOrDefault(n => n.Id == SelectedNode.Id);
        SelectedConnection = SelectedConnection is null ? null : Connections.FirstOrDefault(c => c.Id == SelectedConnection.Id);
        ConnectionFromNode = ConnectionFromNode is null ? null : Nodes.FirstOrDefault(n => n.Id == ConnectionFromNode.Id);
        ConnectionToNode = ConnectionToNode is null ? null : Nodes.FirstOrDefault(n => n.Id == ConnectionToNode.Id);
    }

    public void SelectNodeById(string? nodeId)
    {
        SelectedNode = string.IsNullOrWhiteSpace(nodeId)
            ? null
            : Nodes.FirstOrDefault(n => string.Equals(n.Id, nodeId, StringComparison.Ordinal));

        CanvasSummary = SelectedNode is null
            ? "Canvas: выделение снято."
            : $"Canvas: выбран узел {SelectedNode.Id} ({SelectedNode.Kind}) @ {SelectedNode.PositionDisplay}.";
    }

    public bool BeginCanvasDrag(string nodeId, double pointerX, double pointerY)
    {
        var node = _document.FindNode(nodeId);
        if (node is null)
        {
            return false;
        }

        _dragNodeId = nodeId;
        _dragSession = _nodeDragService.Begin(node.X, node.Y, pointerX, pointerY);
        _dragPreviewPosition = (node.X, node.Y);
        CanvasSummary = $"Canvas: начато перетаскивание узла {nodeId} из ({node.X:0.##}, {node.Y:0.##}).";
        return true;
    }

    public void UpdateCanvasDrag(string nodeId, double pointerX, double pointerY)
    {
        if (_dragSession is null || !string.Equals(_dragNodeId, nodeId, StringComparison.Ordinal))
        {
            return;
        }

        var updated = _nodeDragService.Update(_dragSession, pointerX, pointerY);
        _dragPreviewPosition = updated;

        if (_diagramEditService.UpdateNodePosition(_document, nodeId, updated.X, updated.Y))
        {
            SynchronizeDocumentStats();
            SelectedNode = Nodes.FirstOrDefault(n => string.Equals(n.Id, nodeId, StringComparison.Ordinal));
            CanvasSummary = $"Canvas: узел {nodeId} перемещается в ({updated.X:0.##}, {updated.Y:0.##}).";
            InvalidateGeneratedArtifacts();
        }
    }

    public void CompleteCanvasDrag(string nodeId)
    {
        if (_dragSession is null || !string.Equals(_dragNodeId, nodeId, StringComparison.Ordinal))
        {
            return;
        }

        var node = _document.FindNode(nodeId);
        if (node is null)
        {
            _dragSession = null;
            _dragNodeId = null;
            _dragPreviewPosition = null;
            return;
        }

        var originX = _dragSession.OriginX;
        var originY = _dragSession.OriginY;
        var dragX = node.X;
        var dragY = node.Y;
        var snapped = _nodeDragService.Snap((dragX, dragY), GridSizeValue);
        var finalX = snapped.X;
        var finalY = snapped.Y;

        if (Math.Abs(finalX - dragX) > 0.0001 || Math.Abs(finalY - dragY) > 0.0001)
        {
            _diagramEditService.UpdateNodePosition(_document, nodeId, finalX, finalY);
            SynchronizeDocumentStats();
            SelectedNode = Nodes.FirstOrDefault(n => string.Equals(n.Id, nodeId, StringComparison.Ordinal));
            CanvasSummary = $"Canvas: узел {nodeId} привязан к сетке ({finalX:0.##}, {finalY:0.##}).";
            InvalidateGeneratedArtifacts();
        }

        if (Math.Abs(finalX - originX) < 0.0001 && Math.Abs(finalY - originY) < 0.0001)
        {
            CanvasSummary = $"Canvas: перетаскивание узла {nodeId} завершено без изменения положения.";
            _dragSession = null;
            _dragNodeId = null;
            _dragPreviewPosition = null;
            SynchronizeDocumentStats();
            return;
        }

        _historyService.Execute(new DelegateHistoryAction(
            $"Перетаскивание узла {nodeId} на canvas с привязкой к сетке",
            () =>
            {
                _diagramEditService.UpdateNodePosition(_document, nodeId, originX, originY);
                SynchronizeDocumentStats();
                SelectedNode = Nodes.FirstOrDefault(n => string.Equals(n.Id, nodeId, StringComparison.Ordinal));
                InvalidateGeneratedArtifacts();
                IsDirty = true;
            },
            () =>
            {
                _diagramEditService.UpdateNodePosition(_document, nodeId, finalX, finalY);
                SynchronizeDocumentStats();
                SelectedNode = Nodes.FirstOrDefault(n => string.Equals(n.Id, nodeId, StringComparison.Ordinal));
                InvalidateGeneratedArtifacts();
                IsDirty = true;
            }));

        IsDirty = true;
        UpdateHistorySummary($"Узел {nodeId} перетащен на canvas и привязан к сетке.");
        GraphEditSummary = $"Узел {nodeId} перемещен на canvas в ({finalX:0.##}, {finalY:0.##}) с привязкой к сетке {GridSizeValue:0.##}.";
        CanvasSummary = $"Canvas: перетаскивание узла {nodeId} завершено в ({finalX:0.##}, {finalY:0.##}) после привязки к сетке.";
        _dragSession = null;
        _dragNodeId = null;
        _dragPreviewPosition = null;
        SynchronizeDocumentStats();
    }


    public void SelectConnectionById(string? connectionId)
    {
        SelectedConnection = string.IsNullOrWhiteSpace(connectionId)
            ? null
            : Connections.FirstOrDefault(c => string.Equals(c.Id, connectionId, StringComparison.Ordinal));

        if (SelectedConnection is null)
        {
            CanvasSummary = "Canvas: выделение связи снято.";
            return;
        }

        CanvasSummary = $"Canvas: выбрана связь {SelectedConnection.Id} {SelectedConnection.FromNodeId}.{SelectedConnection.FromPort} -> {SelectedConnection.ToNodeId}.{SelectedConnection.ToPort}.";
    }

    public void CanvasUseNodeAsConnectionSource(string nodeId)
    {
        var node = Nodes.FirstOrDefault(n => string.Equals(n.Id, nodeId, StringComparison.Ordinal));
        if (node is null)
        {
            CanvasSummary = $"Canvas: узел {nodeId} не найден для начала связи.";
            return;
        }

        ConnectionFromNode = node;
        SelectedFromPort = node.Kind == NodeKind.Condition ? PortKind.True : PortKind.Out;
        CanvasSummary = $"Canvas: источник связи установлен на {node.Id}.{SelectedFromPort}. Выберите целевой узел на диаграмме или в списке.";
    }

    public void CanvasCancelPendingConnection(string reason = "Режим создания связи отменен.")
    {
        if (ConnectionFromNode is null)
        {
            CanvasSummary = "Canvas: активного режима создания связи нет.";
            return;
        }

        ConnectionFromNode = null;
        ConnectionToNode = null;
        CanvasSummary = $"Canvas: {reason}";
    }

    public bool CanvasCanConnectToNode(string targetNodeId)
    {
        if (ConnectionFromNode is null)
        {
            return false;
        }

        var target = Nodes.FirstOrDefault(n => string.Equals(n.Id, targetNodeId, StringComparison.Ordinal));
        if (target is null)
        {
            return false;
        }

        if (string.Equals(ConnectionFromNode.Id, target.Id, StringComparison.Ordinal))
        {
            return false;
        }

        return !Connections.Any(c =>
            string.Equals(c.FromNodeId, ConnectionFromNode.Id, StringComparison.Ordinal) &&
            string.Equals(c.ToNodeId, target.Id, StringComparison.Ordinal) &&
            c.FromPort == SelectedFromPort &&
            c.ToPort == PortKind.In);
    }

    public void CanvasConnectFromPendingSourceToNode(string targetNodeId)
    {
        var target = Nodes.FirstOrDefault(n => string.Equals(n.Id, targetNodeId, StringComparison.Ordinal));
        if (target is null)
        {
            CanvasSummary = $"Canvas: целевой узел {targetNodeId} не найден.";
            return;
        }

        if (ConnectionFromNode is null)
        {
            CanvasSummary = "Canvas: сначала выберите источник связи.";
            return;
        }

        if (!CanvasCanConnectToNode(targetNodeId))
        {
            CanvasSummary = string.Equals(ConnectionFromNode.Id, target.Id, StringComparison.Ordinal)
                ? "Canvas: нельзя создать связь узла с самим собой на текущем этапе."
                : $"Canvas: узел {target.Id} недоступен как цель для связи от {ConnectionFromNode.Id}.{SelectedFromPort}.";
            return;
        }

        ConnectionToNode = target;
        SelectedToPort = PortKind.In;
        AddConnection();
        CanvasSummary = $"Canvas: запрошено создание связи {ConnectionFromNode.Id}.{SelectedFromPort} -> {target.Id}.{SelectedToPort}.";
    }

    public void CanvasDeleteSelectedNode()
    {
        DeleteSelectedNode();
    }

    public void CanvasDeleteSelectedConnection()
    {
        DeleteSelectedConnection();
    }

    private static string ComposeBuildLog(BuildResult buildResult)
    {
        return string.Join(Environment.NewLine + Environment.NewLine, new[]
        {
            "[configure]",
            string.IsNullOrWhiteSpace(buildResult.ConfigureLog) ? "нет вывода" : buildResult.ConfigureLog,
            "[build]",
            string.IsNullOrWhiteSpace(buildResult.BuildLog) ? "нет вывода" : buildResult.BuildLog,
            "[binary]",
            buildResult.BinaryPath ?? "исполняемый файл не найден"
        });
    }

    private static string SanitizePathName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "preview" : cleaned;
    }
}
