Imports System
Imports Xunit

Namespace FreeSql.Tests.VB
    Public Class UnitTest1
        <Fact>
        Sub TestSub()

            REM VB.net 表达式解析兼容性测试
            Dim id As Integer = 100
            Dim List1 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.Id = 100).ToList()
            Dim List2 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.Id = id).ToList()
            Dim List11 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.IdNullable = 100).ToList()
            Dim List22 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.IdNullable = id).ToList()
            Dim idNullable As Integer? = 100
            Dim List222 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.IdNullable = idNullable).ToList()
            Dim List3 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.Title = "xxx").ToList()
            Dim title As String = "xxx"
            Dim List4 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.Title = title).ToList()
            Dim List5 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.Title <> "xxx").ToList()
            Dim List6 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.Title <> title).ToList()

            Dim List7 = g.sqlserver.Select(Of Testvb).ToList(Function(a) New With {a, a.Id, a.Title})
            Dim List8 = g.sqlserver.Select(Of Testvb).Where(Function(a) a.IsDeleted).ToList()


            g.sqlserver.Delete(Of Testvb2).Where("1=1").ExecuteAffrows()
            g.sqlserver.Delete(Of Testvb).Where("1=1").ExecuteAffrows()
            g.sqlserver.Insert(New Testvb With {.Id = 1, .Title = "title1"}).ExecuteAffrows()
            g.sqlserver.Insert(New Testvb With {.Id = 2, .Title = "title2"}).ExecuteAffrows()
            g.sqlserver.Insert(New Testvb With {.Id = 3, .Title = "title3"}).ExecuteAffrows()
            g.sqlserver.Insert(New Testvb2 With {.Id = 1, .TestvbId = 1, .Context = "Context11"}).ExecuteAffrows()
            g.sqlserver.Insert(New Testvb2 With {.Id = 2, .TestvbId = 1, .Context = "Context12"}).ExecuteAffrows()
            g.sqlserver.Insert(New Testvb2 With {.Id = 3, .TestvbId = 1, .Context = "Context13"}).ExecuteAffrows()
            g.sqlserver.Insert(New Testvb2 With {.Id = 4, .TestvbId = 2, .Context = "Context21"}).ExecuteAffrows()
            g.sqlserver.Insert(New Testvb2 With {.Id = 5, .TestvbId = 2, .Context = "Context22"}).ExecuteAffrows()
            g.sqlserver.Insert(New Testvb2 With {.Id = 6, .TestvbId = 3, .Context = "Context31"}).ExecuteAffrows()

            Dim List9 = g.sqlserver.Select(Of Testvb).IncludeMany(Function(a) a.Testvb2s).ToList()


        End Sub
    End Class
End Namespace

Class Testvb
    Property Id As Integer
    Property Title As String
    Property IsDeleted As Boolean
    Property IdNullable As Integer?

    Property Testvb2s As List(Of Testvb2)
End Class

Class Testvb2
    Property Id As Integer
    Property TestvbId As Integer
    Property Testvb As Testvb
    Property Context As String
End Class