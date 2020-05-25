Imports System
Imports FreeSql.DataAnnotations
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

            BaseEntity.Initialization(g.sqlserver, Nothing)
            Dim cowR As CowRecord = New CowRecord
            cowR.Id = 1
            cowR.Lact = 1
            cowR.VetCount = 1
            cowR.Save()

            cowR.VetCount += 1
            cowR.Update()



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

<Index("uk_Primary", "Id,Lact", True)>
Public Class CowRecord
    Inherits BaseEntity(Of CowRecord)
    Private _Id As Integer
    Private _Lact As Integer
    Private _Pen As Integer
    Private _BDAT As Date?
    Private _FDAT As Date?
    Private _DDAT As Date?
    Private _EDAT As Date?
    Private _ARDAT As Date?
    Private _MKDAT As Date?
    Private _BFDAT As Date?
    Private _USDAT As Date?
    Private _RC As Integer
    Private _DMLK1 As Integer
    Private _VetCount As Integer

    <Column(IsPrimary:=True)>
    Public Property Id As Integer
        Get
            Return _Id
        End Get
        Set(value As Integer)
            _Id = value
        End Set
    End Property


    <Column(IsPrimary:=True)>
    Public Property Lact As Integer
        Get
            Return _Lact
        End Get
        Set(value As Integer)
            _Lact = value
        End Set
    End Property

    Public Property VetCount As Integer
        Get
            Return _VetCount
        End Get
        Set(value As Integer)
            _VetCount = value
        End Set
    End Property
End Class
