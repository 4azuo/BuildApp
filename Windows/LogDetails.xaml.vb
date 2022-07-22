Imports System.IO

Public Class LogDetails
    Public Shared Function ShowWindow(iCmd As String, iDetails As String, iErrors As String, Optional iDirPath As String = "") As LogDetails
        Dim newWindow As New LogDetails

        newWindow.WindowData.Cmd = iCmd
        newWindow.WindowData.Log = iDetails
        newWindow.WindowData.LogError = iErrors
        newWindow.WindowData.DirPath = iDirPath

        newWindow.Show()
        Return newWindow
    End Function

    Private Sub OpenBuildDir(sender As Object, e As RoutedEventArgs)
        If String.IsNullOrEmpty(WindowData.DirPath) Then Return
        Process.Start(IIf(Directory.Exists(WindowData.DirPath), WindowData.DirPath, Directory.GetParent(WindowData.DirPath).FullName))
    End Sub
End Class
