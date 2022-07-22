Public Class CpuModeEnum
    Public Shared ReadOnly DefaultCpuModes As CpuModeEnum() = {
        New CpuModeEnum("x64", "/p:PlatformTarget=x64"),
        New CpuModeEnum("x86", "/p:PlatformTarget=x86")
        }

#Region "Constructor"
    Public ReadOnly Property Name As String
    Public ReadOnly Property CmdMode As String

    Public Sub New(iName As String, iCmdMode As String)
        Name = iName
        CmdMode = iCmdMode
    End Sub
#End Region
End Class