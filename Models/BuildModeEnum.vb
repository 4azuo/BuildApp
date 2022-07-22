Public Class BuildModeEnum
    Public Shared ReadOnly DefaultBuildModes As BuildModeEnum() = {
        New BuildModeEnum("Debug", "/property:Configuration=Debug", "Bin\Debug"),
        New BuildModeEnum("Release", "/property:Configuration=Release", "Bin\Release")
        }

#Region "Constructor"
    Public ReadOnly Property Name As String
    Public ReadOnly Property CmdMode As String
    Public ReadOnly Property Dir As String

    Public Sub New(iName As String, iCmdMode As String, iDir As String)
        Name = iName
        CmdMode = iCmdMode
        Dir = iDir
    End Sub
#End Region
End Class