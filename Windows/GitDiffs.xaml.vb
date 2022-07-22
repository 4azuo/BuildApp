Imports System.IO
Imports System.Text.RegularExpressions

Public Class GitDiffs
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Task.Run(Sub()
                     If WindowData.Branches.Count = 0 Then
                         MsgBox(W002)
                     End If
                 End Sub)
    End Sub

    Public Shared Function ShowWindow() As GitDiffs
        Dim newWindow As New GitDiffs
        newWindow.ShowDialog()
        Return newWindow
    End Function

    Private Sub BranchChanged(sender As Object, e As SelectionChangedEventArgs)
        WindowData.AllDiffs = ""
        WindowData.AllAffectedProjects = ""

        If WindowData.SelectedBranchA Is Nothing OrElse WindowData.SelectedBranchB Is Nothing OrElse WindowData.SelectedBranchA?.Equals(WindowData.SelectedBranchB) Then Return

        'lock
        Me.IsEnabled = False

        LoadAllDiff()
        If Not String.IsNullOrEmpty(WindowData.AllDiffs) Then LoadAllAffectedProjects()

        'unlock
        Me.IsEnabled = True
    End Sub

    Private Sub LoadAllDiff()
        Const GET_ALL_AFFECTED_PROJECTS As String = "git diff --name-only {0}..{1}"
        Dim runnedCmd As String

        'create processer
        Dim processer As Process = CreateProcesser("cmd.exe", Nothing, True, True)
        processer.Start()

        'get all branches
        Using sw As StreamWriter = processer.StandardInput
            sw.WriteLine($"cd ""{WindowData.RepositoryPath}""")
            runnedCmd = String.Format(GET_ALL_AFFECTED_PROJECTS, WindowData.SelectedBranchA.FullName, WindowData.SelectedBranchB.FullName)
            sw.WriteLine(runnedCmd)
        End Using

        'rs
        Dim output As String = processer.StandardOutput.ReadToEnd()
        Dim err As String = processer.StandardError.ReadToEnd()

        'set AllDiffs
        processer.WaitForExit()
        If processer.ExitCode = 0 Then
            If Not String.IsNullOrEmpty(output) Then
                'set AllDiffs
                WindowData.AllDiffs = ConvertRawUtf8ToJapanese(GetCmdOutput(runnedCmd, output))
            End If
        Else
            LogDetails.ShowWindow(runnedCmd, output, err)
            Return
        End If
    End Sub

    Private Sub LoadAllAffectedProjects()
        Dim rs As New List(Of String)
        Dim repositoryDir As String = New DirectoryInfo($"{Directory.GetCurrentDirectory}\{WindowData.RepositoryPath}").FullName
        For Each line As String In WindowData.AllDiffs.Split(vbLf)
            Dim s As String() = line.Split("/"c)
            If s.Length < 2 Then Continue For
            Dim tmp As String = ""
            For i = 0 To s.Length - 2
                tmp += s(i) + "\"
                If Not Directory.Exists($"{repositoryDir}{tmp}") Then Continue For
                For Each sln As String In GetAllProjects($"{repositoryDir}{tmp}")
                    rs.Add(sln)
                Next
            Next
        Next
        rs.RemoveAll(Function(x) x Is Nothing)
        WindowData.AllAffectedProjects = String.Join(vbLf, rs.Distinct.ToArray)
    End Sub

    Private Sub BtnSelect_Click(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrEmpty(WindowData.AllDiffs) OrElse String.IsNullOrEmpty(WindowData.AllAffectedProjects) Then
            MsgBox(E010)
            Return
        End If
        WindowData.ReturnValue = True
        Me.Close()
    End Sub
End Class
