Public Class FrameworkEnum
    Public Shared ReadOnly DefaultFrameworks As FrameworkEnum() = {
        New FrameworkEnum("2.0", {"C:\Windows\Microsoft.NET\Framework\v2.0.50727"}),
        New FrameworkEnum("3.5", {"C:\Windows\Microsoft.NET\Framework\v3.5"}),
        New FrameworkEnum("4.0", {"C:\Windows\Microsoft.NET\Framework\v4.0.30319"}),
        New FrameworkEnum("4.5.2 (VS 2012)", {"C:\Windows\Microsoft.NET\Framework\v4.0.30319"}),
        New FrameworkEnum("4.5.2 (VS 2013)", {"C:\Program Files (x86)\MSBuild\12.0\Bin"}),
        New FrameworkEnum("4.6", {"C:\Program Files (x86)\MSBuild\14.0\Bin"}),
        New FrameworkEnum("4.7", {
                                    "C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin",
                                    "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin",
                                    "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin"
                          }),
        New FrameworkEnum("4.7.2", {
                                    "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin",
                                    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin",
                                    "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"
                          })
        }

#Region "Constructor"
    Public ReadOnly Property Name As String
    Public ReadOnly Property Value As String()

    Public Sub New(iName As String, iValue As String())
        Name = iName
        Value = iValue
    End Sub
#End Region
End Class
