VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "Banners"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit
'
Const ContentNameBannerCampaigns = "Banner Campaigns"
Const ContentNameBannerRules = "Banner Campaign Rules"
Const ContentNameBanners = "Banners"
'Const ContentNameImages = "Banner Images"
'Const ContentNameText = "Banner Text"
'
Private Main As Object
Private Csv As Object

Public Function Execute(CsvObject As Object, MainObject As Object, OptionString As String, FilterInput As String) As String
    On Error GoTo ErrorTrap
    
    Set Csv = CsvObject
    
    Call Init(MainObject)
    
    Execute = GetContent(OptionString)
    
    Exit Function
ErrorTrap:
    Call HandleError("BannerCampaignClass", "Execute", Err.Number, Err.Source, Err.Description, True, False)
    End Function
'
Public Sub Init(MainObject As Object)
    Set Main = MainObject
    Exit Sub
    '
ErrorTrap:
    Call HandleError("BannerCampaignClass", "Init", Err.Number, Err.Source, Err.Description, True, False)
    End Sub
'
Public Function GetContent(OptionString As String) As String
    On Error GoTo ErrorTrap
    '
    Dim CS As Long
    Dim Link As String
    Dim NewWindow As Boolean
    Dim Height As Long
    Dim Width As Long
    Dim MethodName As String
    Dim Align As String
    Dim SQLNow As String
    Dim CSBanners As Long
    Dim BannerID As Long
    Dim QS As String
    Dim BannerName As String
    Dim CampaignID As Long
    Dim BannerCriteria As String
    '
    Dim Clicks As Long
    Dim ClicksMax As Long
    Dim Viewings As Long
    Dim ViewingsMax As Long
    Dim DateExpires As Date
    Dim HiddenResponse As String
    Dim IsLinkAuthoring As Boolean
    Dim EncodedLink As String
    Dim NonEncodedLink As String
    '
    Dim Copy As String
    'Dim ContentID As Long
    Dim SQL As String
    Dim IDCriteria As String
    '
    Dim CSBanner As Long
    '
    MethodName = "GetContent"
    '
    CampaignID = kmaEncodeInteger(Main.GetAggrOption("Campaign", OptionString))
    If CampaignID = 0 Then
        ' compatible with a previous build
        CampaignID = kmaEncodeInteger(Main.GetAggrOption("CampaignID", OptionString))
    End If
    '
    '==========================================================================
    '   Process Banners
    '==========================================================================
    '
    BannerID = Main.GetStreamInteger("BannerID")
    If BannerID <> 0 Then
        CSBanners = Main.OpenCSContent("Banners", "(ID=" & KmaEncodeSQLNumber(BannerID) & ")")
        If Main.IsCSOK(CSBanners) Then
            Clicks = Main.GetCSInteger(CSBanners, "Clicks")
            Call Main.SetCS(CSBanners, "Clicks", Clicks + 1)
            EncodedLink = Main.GetCSText(CSBanners, "Link")
            NonEncodedLink = kmaDecodeResponseVariable(EncodedLink)
            Call Main.CloseCS(CSBanners)
            Call Main.Redirect(NonEncodedLink)
            Exit Function
        End If
        Call Main.CloseCS(CSBanners)
    End If
    ' 
    '==========================================================================
    ' get next banner
    '==========================================================================
    '
    SQLNow = KmaEncodeSQLDate(Now)
    IsLinkAuthoring = Main.IsLinkAuthoring("Banner Campaigns")
    '
    If CampaignID <> 0 Then
        If IsLinkAuthoring Then
            '
            ' Campaign given + Link Authoring - return banner regardless of expiration
            '
            BannerCriteria = GetBannerCriteria(CampaignID, False)
        Else
            '
            ' Campaign Name given no authoring - select a banner from the campaign
            '
            BannerCriteria = "" _
                & "((ClicksMax is null)OR(Clicks<ClicksMax))" _
                & "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" _
                & "AND((DateExpires is null)OR(DateExpires>" & SQLNow & "))" _
                & GetBannerCriteria(CampaignID, True)
        End If
    Else
        '
        ' no Campaign given, get any banner from any campaign
        '
        BannerCriteria = "" _
            & "((ClicksMax is null)OR(Clicks<ClicksMax))" _
            & "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" _
            & "AND((DateExpires is null)OR(DateExpires>" & SQLNow & "))"
        'BannerCriteria = "" _
        '    & "((ClicksMax is null)OR(Clicks<ClicksMax))" _
        '    & "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" _
        '    & "AND((DateExpires is null)OR(DateExpires>" & SQLNow & "))" _
        '    & GetBannerCriteria(CampaignID, True)
    End If
    '
    CS = Main.OpenCSContent("Banners", BannerCriteria, "LastViewDate", , , , , 1)
    If Not Main.IsCSOK(CS) Then
        '
        ' ----- No banner found
        '
        If CampaignID <> 0 Then
            GetContent = Main.GetAdminHintWrapper("There are no banners available within the given campaign.")
        Else
            GetContent = Main.GetAdminHintWrapper("No campaign was selected so all banners will display, but no banner was found." _
                & "<ul>" _
                & "<li>To add a banner, turn on advanced edit and click Add a Banner" _
                & "<li>To add a campaign, turn on advanced edit and click Add a Campaign" _
                & "<li>To select a campaign to display here, turn on advanced edit and click the icon above this text to edit setting for this instance." _
                & "</ul>")
            'GetContent = Main.GetAdminHintWrapper("There was no campaign set in the Add-on. To select a campaign, turn on Advanced Edit, and hover over the settings icon at the top of the Add-on on the page.")
        End If
    Else
        '
        ' ----- Read and buffer Database values
        '
        BannerID = Main.GetCSInteger(CS, "ID")
        Link = Main.GetCSText(CS, "Link")
        NewWindow = Main.GetCSBoolean(CS, "NewWindow")
        Height = Main.GetCSInteger(CS, "Height")
        Width = Main.GetCSInteger(CS, "Width")
        Align = Main.GetCSText(CS, "Align")
        Clicks = Main.GetCSInteger(CS, "Clicks")
        ClicksMax = Main.GetCSInteger(CS, "ClicksMax")
        Viewings = Main.GetCSInteger(CS, "Viewings")
        ViewingsMax = Main.GetCSInteger(CS, "ViewingsMax")
        DateExpires = Main.GetCSDate(CS, "DateExpires")
        QS = Main.RefreshQueryString
        'Copy = Main.GetCSTextFile(CS, "CopyFileName")
        'ContentID = Main.GetCSInteger(CS, "ContentControlID")
        '
        ' ----- Authoring Link
        '
        If IsLinkAuthoring Then
            ' added div to put the edit above the image
            GetContent = GetContent & "<div>" & Main.GetCSRecordEditLink(CS) & "</div>"
        End If
        '
        If True Then
        'If ContentID = Main.GetContentID(ContentNameImages) Then
            ' 
            ' ----- Start Anchor if needed
            '
            If Link <> "" Then
                QS = ModifyQueryString(QS, "bannerid", CStr(BannerID), True)
                GetContent = GetContent & "<A href=""" & Main.ServerPage & "?" & QS & """ "
                If NewWindow Then
                    GetContent = GetContent & " target=""_blank"""
                End If
                GetContent = GetContent & ">"
            End If
            '
            ' ----- Add banner
            '
            NonEncodedLink = Main.ServerProtocol & Main.ServerHost & Main.serverFilePath & Main.GetCSText(CS, "ImageFilename")
            EncodedLink = kmaEncodeURL(NonEncodedLink)
            '
            GetContent = GetContent & "<IMG border=""0"" src=""" & EncodedLink & """"
            If (Width <> 0) Then
                GetContent = GetContent & " width=""" & CStr(Width) & """ "
            End If
            If (Height <> 0) Then
                GetContent = GetContent & " height=""" & CStr(Height) & """ "
            End If
            If Align <> "" Then
                GetContent = GetContent & " align=""" & Align & """ "
            End If
            GetContent = GetContent & " alt=""" & Main.GetCSText(CS, "Caption") & """>"
            '
            ' ----- End Anchor if needed
            '
            If Link <> "" Then
                GetContent = GetContent & "</A>"
            End If
        Else
            GetContent = GetContent & "<div class=""BannerText"">" & Copy & "</div>"
        End If
        '
        ' ----- Content Manager Message
        '
        If BannerName <> "" Then
            If (ClicksMax <> 0) And (Clicks >= ClicksMax) Then
                HiddenResponse = "[Banner hidden (Clicks Max Met): " & BannerName & "]"
            ElseIf (ViewingsMax <> 0) And (Viewings >= ViewingsMax) Then
                HiddenResponse = "[Banner hidden (Viewings Max Met): " & BannerName & "]"
            ElseIf (DateExpires <> CDate(0)) And (DateExpires < Now) Then
                HiddenResponse = "[Banner hidden (Expired): " & BannerName & "]"
            End If
        End If
    End If
    If IsLinkAuthoring Then
        '
        ' authoring - show hiddenresponse and do not count viewings or lastviewdate
        '
        GetContent = GetContent & HiddenResponse
        'GetContent = GetContent & "<div>" & Main.GetRecordAddLink("Banner Campaigns", "") & "&nbsp;Add a campaign</div>"
        GetContent = GetContent & "<div>" & Main.GetRecordAddLink("Banners", "campaign=" & CampaignID) & "&nbsp;Add a banner</div>"
        'If CampaignID <> 0 Then
        '    GetContent = GetContent & "<div>Add a banner to this campaign&nbsp;" & Main.GetRecordAddLink("Banners", "CampaignID=" & CampaignID) & "</div>"
        'End If
    ElseIf Main.IsCSOK(CS) Then
        '
        ' not authoring and banner found - count viewing and update lastviewdate
        ' this means in LinkAuthoring, banner will not rotate
        '   Also add a viewing record for the banner so we can track individual viewing dates
        '
        Call Main.SetCS(CS, "Viewings", Viewings + 1)
        Call Main.SetCS(CS, "LastViewDate", Now())
        
        CSBanner = Main.InsertCSContent("Banner Viewings")
        If Main.CSOK(CSBanner) Then
            Call Main.SetCS(CSBanner, "BannerID", BannerID)
        End If
        Call Main.CloseCS(CSBanner)
        
    End If
    '
    ' ----- Done
    '
    Call Main.CloseCS(CS)
    '
    ' ----- add Help
    '
    GetContent = Main.GetHelpLink(42, "Controlling the Banner Subsystem", "The banner subsystem produces linked rotating banners. Turn on link authoring to edit individual banners. To add or edit other banners go to the Admin site, typically under the Site Content menu.") & GetContent
    '
    Exit Function
ErrorTrap:
    Call HandleError("BannerCampaignClass", "GetContent", Err.Number, Err.Source, Err.Description, True, False)
    End Functionan
'
Private Function GetBannerCriteria(CampaignID As Long, AndCriteria As Boolean) As String
    On Error GoTo ErrorTrap
    '
    Dim CS As Long
    Dim Stream As String
    Dim InnerStream As String
    Dim Criteria As String
    '
    If CampaignID <> 0 Then
        Criteria = "BannerCampaignID=" & Main.EncodeSQLNumber(CampaignID)
    End If
    '
    If AndCriteria Then
        Stream = "AND"
    End If
    '
    CS = Main.OpenCSContent(ContentNameBannerRules, Criteria, , , , , "BannerID")
    If Main.CSOK(CS) Then
        Do While Main.CSOK(CS)
            If InnerStream <> "" Then
                InnerStream = InnerStream & ","
            End If
            InnerStream = InnerStream & Main.GetCSText(CS, "BannerID")
            Call Main.NextCSRecord(CS)
        Loop
    Else
        InnerStream = "0"
    End If
    Call Main.CloseCS(CS)
    ' 
    Stream = Stream & "(ID IN(" & InnerStream & "))"
    '
    GetBannerCriteria = Stream
    '
    Exit Function
ErrorTrap:
    Call HandleError("BannerCampaignClass", "GetBannerCriteria", Err.Number, Err.Source, Err.Description, True, False)
    End Function
