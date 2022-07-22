Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Text.RegularExpressions

Public Class GitDiffsData
    Inherits CWindowData

    ''' <summary>
    ''' Lists
    ''' </summary>
    Public Property Branches As ObservableCollection(Of Branch)

    ''' <summary>
    ''' Window Values
    ''' </summary>
    <NotifyMethod("LoadBranches")>
    Public Property RepositoryPath As String = "../../../"
    Public Property SelectedBranchA As Branch = Nothing
    Public Property SelectedBranchB As Branch = Nothing
    Public Property AllDiffs As String = ""
    Public Property AllAffectedProjects As String = ""
    Public Property ReturnValue As Boolean = False

    ''' <summary>
    ''' OnLoad
    ''' </summary>
    Public Overrides Sub OnLoad()
        LoadBranches(Nothing)
    End Sub

    ''' <summary>
    ''' Load all branches in selected repository
    ''' </summary>
    Public Sub LoadBranches(propName)
        Const GET_ALL_BRANCHES As String = "git branch -a"

        'clear
        Branches = New ObservableCollection(Of Branch)
        SelectedBranchA = Nothing
        SelectedBranchB = Nothing
        AllDiffs = ""
        AllAffectedProjects = ""
        If Not Directory.Exists(RepositoryPath) Then Return

        'create processer
        Dim processer As Process = CreateProcesser("cmd.exe", Nothing, True, True)
        processer.Start()

        'get all branches
        Using sw As StreamWriter = processer.StandardInput
            sw.WriteLine($"cd ""{RepositoryPath}""")
            sw.WriteLine(GET_ALL_BRANCHES)
        End Using

        'rs
        Dim output As String = processer.StandardOutput.ReadToEnd()
        Dim err As String = processer.StandardError.ReadToEnd()

        'set data
        processer.WaitForExit()
        If processer.ExitCode = 0 Then
            Dim lines As String() = GetCmdOutputs(GET_ALL_BRANCHES, output) _
                                        .Where(Function(x) Not x.Contains("HEAD ->")) _
                                        .Select(Of String)(Function(x) IIf(x.StartsWith("* "), x.Replace("* ", ""), x)) _
                                        .ToArray
            For Each l As String In lines
                Branches.Add(New Branch(l, l))
            Next
        Else
            LogDetails.ShowWindow(GET_ALL_BRANCHES, output, err)
            Return
        End If
    End Sub
End Class
