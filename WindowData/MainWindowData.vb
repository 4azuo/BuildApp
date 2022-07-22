Imports System.Collections.ObjectModel
Imports System.IO

Public Class MainWindowData
    Inherits CWindowData

    ''' <summary>
    ''' Lists
    ''' </summary>
    Public Property ListFrameworks As FrameworkEnum() = FrameworkEnum.DefaultFrameworks
    Public Property ListBuildModes As BuildModeEnum() = BuildModeEnum.DefaultBuildModes
    Public Property ListCpuModes As CpuModeEnum() = CpuModeEnum.DefaultCpuModes
    Public Property ListProjects As ObservableCollection(Of Project)

    ''' <summary>
    ''' Window Values
    ''' </summary>
    Public Property IsLoading As Boolean = False
    Public Property SelectedFramework As FrameworkEnum = ListFrameworks.FirstOrDefault(Function(x) x.Name = "4.7.2")
    <NotifyMethod("LoadProjects")>
    Public Property SelectedSourcePath As String = "../../../SRC"
    Public Property SelectedBuildMode As BuildModeEnum = ListBuildModes.FirstOrDefault(Function(x) x.Name = "Release")
    Public Property SelectedCpuMode As CpuModeEnum = ListCpuModes.FirstOrDefault(Function(x) x.Name = "x86")
    Public Property SelectedIgnoredTypes As String = ".pdb"
    Public Property SelectedCopyPath As String = "../../../DLL"
    Public Property IsCopy As Boolean = False
    Public Property IsSync As Boolean = True
    Public Property SearchText As String = ""
    Public Property Log As ObservableCollection(Of LogObj)
    Public Property LogError As ObservableCollection(Of LogObj)
    Public Property ProjectCountCur As Integer = 0
    Public Property ProjectCountMax As Integer = 0
    Public ReadOnly Property ShowCount As String
        Get
            Return $"{ProjectCountCur}/{ProjectCountMax}"
        End Get
    End Property
    Public ReadOnly Property WindowTitle As String
        Get
            If Not Directory.Exists(SelectedSourcePath) Then Return ""
            Return $"Builder - {Directory.GetParent(SelectedSourcePath).Name}"
        End Get
    End Property

    ''' <summary>
    ''' OnLoad
    ''' </summary>
    Public Overrides Sub OnLoad()
        LoadProjects(Nothing)
    End Sub

    ''' <summary>
    ''' Load Projects
    ''' </summary>
    Public Sub LoadProjects(propName)
        'reset
        SearchText = ""
        ProjectCountCur = 0
        ProjectCountMax = 0
        ListProjects = New ObservableCollection(Of Project)
        If Not Directory.Exists(SelectedSourcePath) Then Return

        'load custom projects
        ListProjects.Add(New Project("All", "All"))
        ListProjects.Add(New Project("ConvertSqlString", "ConvertSqlString"))
        ListProjects.Add(New Project("DbController", "DbController"))
        ListProjects.Add(New Project("UserControl", "UserControl"))
        ListProjects.Add(New Project("CheckLogicCommon", "CheckLogicCommon"))
        ListProjects.Add(New Project("Entity", "Entity"))
        ListProjects.Add(New Project("WebServiceInterface", "WebServiceInterface"))
        ListProjects.Add(New Project("CheckLogic", "CheckLogic"))
        ListProjects.Add(New Project("CommonParts", "CommonParts"))
        ListProjects.Add(New Project("Accesser", "Accesser"))
        ListProjects.Add(New Project("BusinessLogic", "BusinessLogic"))
        ListProjects.Add(New Project("Print", "Print"))
        ListProjects.Add(New Project("Batch", "Batch"))
        ListProjects.Add(New Project("Form", "Form"))
        ListProjects.Add(New Project("YSBatch(EXE)", "YSBatch(EXE)"))
        ListProjects.Add(New Project("WebService", "WebService"))

        Dim t = New Project("――――――――――――――――――――", Nothing)
        ListProjects.Add(t)
        If ListProjects.Count Mod 2 = 1 Then ListProjects.Add(t)

        'load projects
        LoadAsync(Of Project, String)(ListProjects,
                                      Function()
                                          IsLoading = True
                                          Return GetAllProjects(SelectedSourcePath, SearchOption.AllDirectories)
                                      End Function,
                                      Function(x)
                                          Return New Project(Path.GetFileName(x), x, True)
                                      End Function,
                                      Sub()
                                          IsLoading = False
                                      End Sub)
    End Sub
End Class
