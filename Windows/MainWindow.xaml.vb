Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Windows.Threading
Imports Microsoft.Win32

Public Class MainWindow
#Region "変数"
    Private _searchIndex As Integer = 0
    Private _builtPrj As New List(Of Project)
    Private _builtRs As New Dictionary(Of Project, BuildResult)
#End Region

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Task.Run(Sub()
                     If WindowData.ListProjects.Count = 0 Then
                         MsgBox(W001)
                     End If
                 End Sub)
    End Sub

    Private Sub ReloadProjects(sender As Object, e As RoutedEventArgs)
        WindowData.LoadProjects(Nothing)
    End Sub

    Private Sub Project_SelectionChanged(sender As ListView, e As SelectionChangedEventArgs)
        WindowData.ProjectCountCur = 0
        WindowData.ProjectCountMax = GetRunnedProjects(lvProjects.SelectedItems, WindowData.ListProjects).Count
    End Sub

    Private Sub Log_DoubleCLick(sender As Object, e As MouseButtonEventArgs) Handles lvLog.MouseDoubleClick, lvLogError.MouseDoubleClick
        Dim logObj As LogObj = CType(CType(e.OriginalSource, FrameworkElement).DataContext, LogObj)
        LogDetails.ShowWindow(logObj.RunnedCmd, logObj.Details2, logObj.LogError, logObj.Path)
    End Sub

    Private Sub Project_DoubleClick(sender As Object, e As MouseButtonEventArgs)
        Dim item As Project = CType(CType(e.OriginalSource, FrameworkElement).DataContext, Project)
        If item Is Nothing Then
            MsgBox(E005)
            Return
        End If
        Dim slns As List(Of Project) = GetRunnedProjects({item}, WindowData.ListProjects)
        If slns.Count <> 1 Then
            MsgBox(E009)
            Return
        End If
        Process.Start(IIf(Directory.Exists(item.Path), item.Path, Directory.GetParent(item.Path).FullName))
    End Sub

    Private Sub CopyTo(sender As Object, e As RoutedEventArgs)
        For Each sln As Project In GetRunnedProjects(lvProjects.SelectedItems, WindowData.ListProjects)
            'buildDir
            Dim buildDirPath As String = $"{Directory.GetParent(sln.Path).FullName}\{WindowData.SelectedBuildMode.Dir}"
            If Directory.Exists(buildDirPath) Then
                'copy
                Dim ignoredExt = WindowData.SelectedIgnoredTypes.ToLower
                For Each f As String In Directory.GetFiles(buildDirPath)
                    Dim fileInfo As New FileInfo(f)
                    If Not ignoredExt.Contains(fileInfo.Extension.ToLower) Then
                        File.Copy(f, $"{WindowData.SelectedCopyPath}\{fileInfo.Name}", True)
                    End If
                Next
            End If
        Next
    End Sub

    Private Sub Build(sender As Object, e As RoutedEventArgs)
        'check
        If WindowData.SelectedFramework Is Nothing Then
            MsgBox(E001)
            Return
        End If
        If WindowData.SelectedFramework.Value.All(Function(x) Not Directory.Exists(x)) Then
            MsgBox(E002)
            Return
        End If
        If String.IsNullOrEmpty(WindowData.SelectedSourcePath) Then
            MsgBox(E003)
            Return
        End If
        If Not Directory.Exists(WindowData.SelectedSourcePath) Then
            MsgBox(E004)
            Return
        End If
        Dim selectedProjects As List(Of Project) = GetRunnedProjects(lvProjects.SelectedItems, WindowData.ListProjects)
        If selectedProjects.Count = 0 Then
            MsgBox(E005)
            Return
        End If
        If WindowData.SelectedBuildMode Is Nothing Then
            MsgBox(E006)
            Return
        End If
        If WindowData.IsCopy And String.IsNullOrEmpty(WindowData.SelectedCopyPath) Then
            MsgBox(E007)
            Return
        End If
        If WindowData.IsCopy And Not Directory.Exists(WindowData.SelectedCopyPath) Then
            MsgBox(E008)
            Return
        End If
        If Not WindowData.IsSync AndAlso MsgBox(W003, MsgBoxStyle.OkCancel) = MsgBoxResult.Cancel Then
            Return
        End If

        'lock window
        Me.IsEnabled = False
        _builtPrj.Clear()
        _builtRs.Clear()

        'clear log
        WindowData.Log = New ObservableCollection(Of LogObj)
        WindowData.LogError = New ObservableCollection(Of LogObj)
        WindowData.ProjectCountCur = 0

        'start
        Dim allTasks As List(Of Task) = Nothing
        If WindowData.IsSync Then
            For Each prj As Project In selectedProjects
                If allTasks Is Nothing Then
                    allTasks = New List(Of Task)({Task.Run(Sub() BuildStart(prj))})
                Else
                    allTasks = New List(Of Task)({Task.WhenAll(allTasks.ToArray).ContinueWith(Sub() BuildStart(prj))})
                End If
            Next
        Else
            allTasks = New List(Of Task)
            For Each prj As Project In selectedProjects
                allTasks.Add(Task.Run(Sub() BuildStart(prj)))
            Next
        End If

        'sync
        Task.WhenAll(allTasks.ToArray).ContinueWith(Sub()
                                                        Dispatcher.Invoke(Sub()
                                                                              'ok
                                                                              If lvLogError.Items.IsEmpty Then
                                                                                  MsgBox(I001)
                                                                              Else
                                                                                  MsgBox(I002)
                                                                              End If

                                                                              'unlock window
                                                                              Me.IsEnabled = True
                                                                          End Sub)
                                                    End Sub)
    End Sub

    Private Sub BuildStart(prj As Project)
        If File.Exists(prj.Path) AndAlso prj.Path.ToLower.EndsWith(".sln") Then
            Dim rs = Build(prj)
            Dispatcher.BeginInvoke(Sub()
                                       If rs.ResultCd = 0 Then
                                           WindowData.Log.Add(rs.Log)
                                           lvLog.ScrollIntoView(rs.Log)
                                       Else
                                           WindowData.LogError.Add(rs.Log)
                                           lvLogError.ScrollIntoView(rs.Log)
                                       End If
                                       WindowData.ProjectCountCur += 1
                                   End Sub)
        ElseIf Directory.Exists(prj.Path) AndAlso prj.Path.ToLower.EndsWith("ws") Then
            Dim name = prj.Name.ToLower.Substring(0, prj.Name.Length - 2)
            Dim refProjs = {
                WindowData.ListProjects.FirstOrDefault(Function(x) x.IsExists AndAlso x.Path.ToLower.EndsWith($"{name}wsi.sln")),
                WindowData.ListProjects.FirstOrDefault(Function(x) x.IsExists AndAlso x.Path.ToLower.EndsWith($"{name}bl.sln")),
                WindowData.ListProjects.FirstOrDefault(Function(x) x.IsExists AndAlso x.Path.ToLower.EndsWith($"{name}cl.sln"))
                }
            Dim binFolder = $"{prj.Path}\Bin"
            Dim startTime = DateTime.Now
            Dim exitcode = 0
            Dim logDetails = ""
            Dim errDetails = ""
            Const sep = "
====================================================================================================
====================================================================================================
====================================================================================================
====================================================================================================
====================================================================================================
"

            For Each p As Project In refProjs
                Dim rs = Build(p)
                Copy(binFolder, rs.BuildDir, WindowData.SelectedIgnoredTypes.ToLower)
                exitcode += rs.ResultCd
                logDetails += If(String.IsNullOrEmpty(rs.Log.Details2), "", $"{rs.Log.Details2}{sep}")
                errDetails += If(String.IsNullOrEmpty(rs.Log.LogError), "", $"{rs.Log.LogError}{sep}")
                For Each ref As String In GetReferingProjects(p)
                    File.Copy(ref, $"{binFolder}\{Path.GetFileName(ref)}", True)
                Next
            Next

            Dim endTime = DateTime.Now
            Dispatcher.BeginInvoke(Sub()
                                       If exitcode = 0 Then
                                           Dim newlog = New LogObj(prj.Path, prj.Name, prj.Path, $"＝REBUILD：{prj.Name}（{endTime - startTime:mm\:ss}）", logDetails, errDetails)
                                           WindowData.Log.Add(newlog)
                                           lvLog.ScrollIntoView(newlog)
                                       Else
                                           Dim newlog = New LogObj(prj.Path, prj.Name, prj.Path, $"＝ERROR：{prj.Name}（{endTime - startTime:mm\:ss}）", logDetails, errDetails)
                                           WindowData.LogError.Add(newlog)
                                           lvLogError.ScrollIntoView(newlog)
                                       End If
                                       WindowData.ProjectCountCur += 1
                                   End Sub)
        End If
    End Sub

    Private Function Build(prj As Project) As BuildResult
        SyncLock prj
            'built project?
            If _builtPrj.Contains(prj) Then Return _builtRs(prj)

            Dim startTime As DateTime = DateTime.Now
            Dim exitcode As Integer = -1
            Dim log As LogObj = Nothing

            'buildDir
            Dim buildDirPath As String = $"{Directory.GetParent(prj.Path).FullName}\{WindowData.SelectedBuildMode.Dir}"

            'clear
            If Directory.Exists(buildDirPath) Then
                My.Computer.FileSystem.DeleteDirectory(buildDirPath, FileIO.DeleteDirectoryOption.DeleteAllContents)
            End If

            'build
            Dim executer As String = $"{WindowData.SelectedFramework.Value.FirstOrDefault(Function(x) Directory.Exists(x))}\MSBuild.exe"
            Dim params As String = $" ""{prj.Path}"" {WindowData.SelectedBuildMode.CmdMode} {WindowData.SelectedCpuMode.CmdMode} -t:rebuild"
            Dim processer As Process = CreateProcesser(executer, params)
            processer.Start()

            Dim logDetails As String = processer.StandardOutput.ReadToEnd()
            Dim errDetails As String = processer.StandardError.ReadToEnd()

            'log
            processer.WaitForExit()
            Dim endTime As DateTime = DateTime.Now
            Dim benchmark = endTime - startTime
            exitcode = processer.ExitCode
            If exitcode = 0 Then
                'copy to
                If WindowData.IsCopy Then
                    Copy(WindowData.SelectedCopyPath, buildDirPath, WindowData.SelectedIgnoredTypes.ToLower)
                End If

                'log
                log = New LogObj($"{executer}{params}", prj.Name, prj.Path, $"＝REBUILD：{prj.Name}（{benchmark:mm\:ss}）", logDetails, errDetails)
            Else
                'error
                log = New LogObj($"{executer}{params}", prj.Name, prj.Path, $"＝ERROR：{prj.Name}（{benchmark:mm\:ss}）", logDetails, errDetails)
            End If

            processer.Close()
            Dim rs = New BuildResult(exitcode, benchmark, buildDirPath, log)
            _builtPrj.Add(prj)
            _builtRs(prj) = rs
            Return rs
        End SyncLock
    End Function

    Private Sub SelectByGitDiffs(sender As Object, e As RoutedEventArgs)
        Dim newWindow As GitDiffs = GitDiffs.ShowWindow()
        If newWindow.WindowData.ReturnValue Then
            WindowData.LoadProjects(Nothing)
            WindowData.ListProjects = New ObservableCollection(Of Project)(WindowData.ListProjects.Where(Function(x) x.IsExists AndAlso newWindow.WindowData.AllAffectedProjects.Contains(x.Path)))
            For Each sln As Project In WindowData.ListProjects
                lvProjects.SelectedItems.Add(sln)
            Next
        End If
    End Sub

    Private Sub SelectByGitBranch(sender As Object, e As RoutedEventArgs)
        Dim newWindow As GitBranch = GitBranch.ShowWindow()
        If newWindow.WindowData.ReturnValue Then
            WindowData.LoadProjects(Nothing)
            WindowData.ListProjects = New ObservableCollection(Of Project)(WindowData.ListProjects.Where(Function(x) x.IsExists AndAlso newWindow.WindowData.AllAffectedProjects.Contains(x.Path)))
            For Each sln As Project In WindowData.ListProjects
                lvProjects.SelectedItems.Add(sln)
            Next
        End If
    End Sub

    Private Sub SelectedProjectsClear(sender As Object, e As RoutedEventArgs)
        lvProjects.SelectedItems.Clear()
        WindowData.ProjectCountCur = 0
        WindowData.ProjectCountMax = 0
    End Sub

    Private Sub ProjectSearchInput(sender As Object, e As TextChangedEventArgs)
        _searchIndex = ProjectSearch() * 1000
    End Sub

    Private Sub ProjectSearchInput_KeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Enter Then
            _searchIndex += IIf(Keyboard.IsKeyDown(Key.LeftShift) OrElse Keyboard.IsKeyDown(Key.RightShift), -1, 1)
            ProjectSearch(_searchIndex)
        End If
    End Sub

    Private Function ProjectSearch(Optional sIndex As Integer = 0) As Integer
        'highligh
        Dim cond As String = WindowData.SearchText.ToLower
        For Each sln As Project In WindowData.ListProjects
            sln.Mode = IIf(sln.IsExists AndAlso Not String.IsNullOrEmpty(cond) AndAlso sln.Name.ToLower.Contains(cond), 1, 0)
        Next

        'focus
        Dim searchingList = WindowData.ListProjects.Where(Function(x) x.Mode = 1).ToList
        If searchingList.Count > 0 Then
            Dim focusItem = searchingList(sIndex Mod searchingList.Count)
            If focusItem IsNot Nothing Then
                focusItem.Mode = 2
                lvProjects.ScrollIntoView(focusItem)
            End If
        End If

        Return searchingList.Count
    End Function

    Private Sub ClearScreen(sender As Object, e As RoutedEventArgs)
        WindowData.LoadProjects(Nothing)
        WindowData.Log = New ObservableCollection(Of LogObj)
        WindowData.LogError = New ObservableCollection(Of LogObj)
    End Sub

    Private Sub Save(sender As Object, e As RoutedEventArgs)
        Dim saveFileDialog As New SaveFileDialog

        saveFileDialog.Filter = "Builds-List (*.bdl)|*.bdl"
        saveFileDialog.FilterIndex = 1
        saveFileDialog.RestoreDirectory = True

        If saveFileDialog.ShowDialog Then
            File.WriteAllText(saveFileDialog.FileName, String.Join(vbNewLine, lvProjects.SelectedItems.Cast(Of Project).Select(Function(x) x.Path)))
        End If
    End Sub

    Private Sub Load(sender As Object, e As RoutedEventArgs)
        Dim openFileDialog As New OpenFileDialog

        openFileDialog.Filter = "Builds-List (*.bdl)|*.bdl"
        openFileDialog.FilterIndex = 1
        openFileDialog.RestoreDirectory = True
        openFileDialog.Multiselect = True

        If openFileDialog.ShowDialog Then
            If MsgBox(C001, MsgBoxStyle.YesNo) = MsgBoxResult.Yes Then lvProjects.SelectedItems.Clear()
            For Each f As String In openFileDialog.FileNames
                Dim loadedData As String() = File.ReadAllText(f).Split({vbNewLine}, StringSplitOptions.RemoveEmptyEntries).Select(Function(x) Trim(x)).ToArray
                For Each sln As Project In WindowData.ListProjects
                    If Not String.IsNullOrEmpty(sln.Path) AndAlso loadedData.Contains(sln.Path) Then
                        lvProjects.SelectedItems.Add(sln)
                    End If
                Next
            Next
        End If
    End Sub

    Private Sub LoadByText_Click(sender As Object, e As RoutedEventArgs)
        Dim newWindow As LoadByText = LoadByText.ShowWindow(String.Join(vbNewLine, lvProjects.SelectedItems.Cast(Of Project).Select(Function(x) x.Path)))
        If newWindow.WindowData.ReturnValue Then
            lvProjects.SelectedItems.Clear()
            Dim newData As String() = newWindow.WindowData.Text.Split({vbNewLine}, StringSplitOptions.RemoveEmptyEntries).Select(Function(x) Trim(x)).ToArray
            For Each sln As Project In WindowData.ListProjects
                If Not String.IsNullOrEmpty(sln.Path) AndAlso newData.Contains(sln.Path) Then
                    lvProjects.SelectedItems.Add(sln)
                End If
            Next
        End If
    End Sub
End Class
