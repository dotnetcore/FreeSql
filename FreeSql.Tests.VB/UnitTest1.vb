Imports System
Imports Xunit

Namespace FreeSql.Tests.VB
    Public Class UnitTest1
        <Fact>
        Sub TestSub()

            REM VB.net 表达式解析兼容性测试
            Dim List1 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.Id = 100).ToList()
            Dim List2 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.Title = "xxx").ToList()
            Dim List3 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.Title <> "xxx").ToList()

            Dim List4 = g.sqlserver.Select(Of Testvb).ToList(Function(a) New With {a, a.Id, a.Title})

        End Sub
    End Class
End Namespace

Class Testvb
    Property Id As Integer
    Property Title As String
End Class
