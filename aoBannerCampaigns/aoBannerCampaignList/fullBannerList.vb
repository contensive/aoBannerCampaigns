Namespace Contensive.Addons.aoBannerCampaigns
    '
    Public Class fullBannerListClass
        Inherits BaseClasses.AddonBaseClass
        '
        Public Overrides Function Execute(ByVal CP As BaseClasses.CPBaseClass) As Object
            Dim s As String = ""
            '
            Try
                Dim cs As BaseClasses.CPCSBaseClass = CP.CSNew()
                Dim campaignID As Integer = CP.Utils.EncodeInteger(CP.Doc.GetInteger("Campaign"))
                Dim criteria As String = ""
                Dim bannerID As Integer = 0
                Dim cs2 As BaseClasses.CPCSBaseClass = CP.CSNew()
                Dim cid As Integer = 0
                Dim imageFileName As String = ""
                Dim link As String = ""
                Dim inS As String = ""
                '
                If campaignID <> 0 Then
                    criteria = "BannerCampaignID=" & campaignID
                End If
                '
                If cs.Open("Banner Campaign Rules", criteria) Then
                    Do While cs.OK()
                        bannerID = cs.GetInteger("bannerID")
                        '
                        If cs2.Open("Banners", "(id=" & bannerID & ") and ((DateExpires>=" & CP.Db.EncodeSQLDate(Date.Now) & ") or (DateExpires is null))") Then
                            imageFileName = cs2.GetText("imageFileName")
                            link = cs2.GetText("link")
                            '
                            If link <> "" Then
                                link = "http://" & CP.Site.DomainPrimary & "/bannerClickHandler?bannerID=" & bannerID
                            End If
                            '
                            If link <> "" Then
                                inS += "<a href=""" & link & """>"
                            End If
                            '
                            inS += "<img src=""http://" & CP.Site.DomainPrimary & CP.Site.FilePath & imageFileName & """ />"
                            '
                            If link <> "" Then
                                inS += "</a>"
                            End If
                            '
                            s += inS & "<br /><br />"
                            inS = ""
                            '
                            End If
                        cs2.Close()
                        '
                        cs.GoNext()
                    Loop
                    '
                End If
                cs.Close()
            Catch ex As Exception
                Try
                    CP.Site.ErrorReport(ex, "error in Contensive.Addons.aoBannerCampaigns.fullBannerListClass.execute")
                Catch errObj As Exception
                End Try
            End Try
            '
            Return s
        End Function
        '
    End Class
    '
    Public Class bannerClickHandlerClass
        Inherits BaseClasses.AddonBaseClass
        '
        Public Overrides Function Execute(ByVal CP As BaseClasses.CPBaseClass) As Object
            Dim s As String = ""
            '
            Try
                Dim bannerID As Integer = CP.Utils.EncodeInteger(CP.Doc.GetInteger("bannerID"))
                Dim cs As BaseClasses.CPCSBaseClass = CP.CSNew()
                Dim link As String = ""
                Dim clicks As Integer = 0
                '
                If bannerID <> 0 Then
                    If cs.Open("Banners", "id=" & bannerID) Then
                        link = cs.GetText("link")
                        clicks = cs.GetInteger("Clicks") + 1
                        '
                        cs.SetField("Clicks", clicks.ToString)
                        '
                        If link <> "" Then
                            If Not link.Contains("http") Then
                                link = "http://" & link
                            End If
                            '
                            CP.Response.Redirect(link)
                        Else
                            CP.Response.Redirect("http://" & CP.Site.DomainPrimary)
                        End If
                    End If
                    cs.Close()
                End If
                cs.Close()
            Catch ex As Exception
                Try
                    CP.Site.ErrorReport(ex, "error in Contensive.Addons.aoBannerCampaigns.bannerClickHandlerClass.execute")
                Catch errObj As Exception
                End Try
            End Try
            '
            Return s
        End Function
        '
    End Class
    '
End Namespace