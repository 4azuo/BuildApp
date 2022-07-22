Imports System.IO

Public Class LoadByText
    Public Shared Function ShowWindow(text As String) As LoadByText
        Dim newWindow As New LoadByText
        newWindow.WindowData.Text = text
        newWindow.ShowDialog()
        newWindow.loader.Focus()
        Return newWindow
    End Function

    Private Sub Ok(sender As Object, e As RoutedEventArgs)
        WindowData.ReturnValue = True
        Me.Close()
    End Sub
End Class
