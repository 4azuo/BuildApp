Imports System.IO
Imports System.Text.RegularExpressions

Module ProjectHelper
    Public Function GetAllProjects(path As String, Optional opt As SearchOption = SearchOption.TopDirectoryOnly)
        Dim rs As New List(Of String)
        rs.AddRange(Directory.GetFiles(path, "*.sln", opt).Distinct.Where(Function(x) Not x.Contains("SRC\WebService\")))
        rs.AddRange(Directory.GetFiles(path, "*.svc", opt).Distinct.Where(Function(x) x.Contains("SRC\WebService\")).Select(Function(x) Directory.GetParent(x).FullName))
        Return rs.OrderBy(Function(x) GetOrderIndex(x))
    End Function

    Public Function GetRunnedProjects(prjs As IEnumerable, from As IEnumerable(Of Project)) As List(Of Project)
        from = from.Where(Function(x) x.IsExists AndAlso Not String.IsNullOrEmpty(x.Path))
        Dim rs As New List(Of Project)
        If prjs Is Nothing Then Return rs
        For Each prj As Project In prjs
            If prj IsNot Nothing AndAlso Not String.IsNullOrEmpty(prj.Path) Then
                Select Case prj.Name
                    Case "All"
                        rs.AddRange(from)
                    Case "ConvertSqlString"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\ConvertSqlString\")))
                    Case "DbController"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\DbController\")))
                    Case "UserControl"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\UserControl\")))
                    Case "CheckLogicCommon"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\CheckLogicCommon\")))
                    Case "Entity"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\Entity\")))
                    Case "WebServiceInterface"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\WebServiceInterface\")))
                    Case "CheckLogic"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\CheckLogic\")))
                    Case "CommonParts"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\CommonParts\")))
                    Case "Accesser"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\Accesser\")))
                    Case "BusinessLogic"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\BusinessLogic\")))
                    Case "Print"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\Print\")))
                    Case "Batch"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\Batch\")))
                    Case "Form"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\Form\")))
                    Case "YSBatch(EXE)"
                        rs.AddRange(from.Where(Function(x) x.Path.Contains("SRC\YSBatch(EXE)\")))
                    Case "WebService"
                        rs.AddRange(from.Where(Function(x) x.Path.EndsWith("SRC\WebService")))
                    Case Else
                        rs.Add(prj)
                End Select
            End If
        Next
        Return rs.Distinct.OrderBy(Function(x) GetOrderIndex(x.Path)).ToList
    End Function

    Public Function GetOrderIndex(prjPath As String) As Integer
        If prjPath.Contains("SRC\ConvertSqlString\") Then Return 1

        If prjPath.Contains("SRC\DbController\") Then Return 2

        If prjPath.Contains("SRC\UserControl\") Then Return 3

        If prjPath.Contains("SRC\CheckLogicCommon\") Then Return 4

        If prjPath.Contains("SRC\Entity\") Then Return 5

        If prjPath.Contains("SRC\WebServiceInterface\") Then Return 6

        If prjPath.Contains("SRC\CheckLogic\") Then Return 7

        If prjPath.Contains("SRC\CommonParts\") Then Return 9

        If prjPath.Contains("SRC\Accesser\") Then Return 10

        If prjPath.Contains("SRC\BusinessLogic\") Then Return 11

        If prjPath.Contains("SRC\Print\") Then Return 12

        If prjPath.Contains("SRC\Batch\") Then Return 13

        If prjPath.Contains("SRC\Form\") Then Return 14

        If prjPath.Contains("SRC\YSBatch(EXE)\") Then Return 15

        If prjPath.EndsWith("SRC\WebService") Then Return 16

        Return 99
    End Function

    Public Function GetReferingProjects(prj As Project) As List(Of String)
        Dim rs As New List(Of String)
        If prj Is Nothing Then Return rs
        Dim prjFile = Regex.Replace(prj.Path, "\.sln", ".vbproj", RegexOptions.IgnoreCase)
        If Not File.Exists(prjFile) Then Return rs

        Dim allText = File.ReadAllText(prjFile)
        Dim groupRegex As New Regex("<ItemGroup>(.*?)</ItemGroup>", RegexOptions.Singleline)
        For Each m1 As Match In groupRegex.Matches(allText)
            Dim group = m1.Groups(1).Value

            Dim refRegex As New Regex("<Reference(.*?)</Reference>", RegexOptions.Singleline)
            For Each m2 As Match In refRegex.Matches(group)
                Dim ref = m2.Groups(1).Value

                Dim hintRegex As New Regex("<HintPath>(.*?)</HintPath>", RegexOptions.Singleline)
                For Each m3 As Match In hintRegex.Matches(ref)
                    Dim hint = m3.Groups(1).Value

                    If File.Exists(hint) Then rs.Add(hint)
                Next
            Next
        Next
        Return rs
    End Function
End Module
