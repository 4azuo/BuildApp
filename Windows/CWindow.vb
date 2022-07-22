Public MustInherit Class CWindow(Of T As {CWindowData, New})
    Inherits Window

    Public ReadOnly Property WindowData As New T

    Public Sub New()
        Width = 1024
        Height = 768
        WindowState = WindowState.Normal
        WindowStartupLocation = WindowStartupLocation.CenterScreen
    End Sub

    Private Sub Window_Loaded(sender As Object, e As EventArgs) Handles Me.Loaded
        Me.DataContext = WindowData
        WindowData.InitWindow(Me)
    End Sub
End Class
