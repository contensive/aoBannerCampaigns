using System;
using Contensive.BaseClasses;

namespace Contensive.Addons.aoBannerCampaigns {
    public class BannerCampaigns : AddonBaseClass {


        //private string link;
        //private string align;
        //private string sQLNow;
        //private string qS;
        //private string bannerName;
        //private string bannerCriteria;
        //private string hiddenResponse;
        //private string encodedLink;
        //private string nonEncodedLink;
        //private int height;
        //private int width;
        //private int bannerID;
        //private int campaignID;
        //private int viewingsMax;
        //private int clicks;
        //private int clicksMax;
        //private int viewings;
        //private bool newWindow;
        //private bool isLinkAuthoring;
        private DateTime dateExpires;
        private const string contentNameBannerRules = "Banner Campaign Rules";

        public override object Execute(CPBaseClass cp) {
            string html = "";
            try {
                DateTime dateMinValue = new DateTime(1990, 1, 1);
                string bannerTitle;
                bool exitWithoutBanner = false;
                CPCSBaseClass cs = cp.CSNew();
                int campaignID = cp.Doc.GetInteger("Campaign");
                if (campaignID == 0) {
                    campaignID = cp.Doc.GetInteger("CampaignID");
                }


                //if the campaginid is still 0, then use the guid of the instanceguid of the banner campaign
                string campaignGuid = "";
                if (campaignID == 0) {
                    campaignGuid = cp.Doc.GetText("instanceId");
                    if (!string.IsNullOrEmpty(campaignGuid)) {
                        if (cs.Open("Banner Campaigns", "ccguid=" + cp.Db.EncodeSQLText(campaignGuid))) {
                            campaignID = cs.GetInteger("id");
                        }
                        else {
                            //create a settings record for this instance
                            if (cs.Insert("Banner Campaigns")) {
                                cs.SetField("ccguid", campaignGuid);
                                cs.Save();
                                campaignID = cs.GetInteger("id");
                            }
                        }
                    }
                }


                //============================================================================================
                //                 Process Banners         
                //============================================================================================
                int bannerID = cp.Doc.GetInteger("BannerID");
                string nonEncodedLink = "";
                string encodedLink = "";
                if (bannerID != 0) {
                    if (cs.Open("Banners", string.Format("(ID = {0})", cp.Db.EncodeSQLNumber(bannerID)))) {
                        int clicks = cs.GetInteger("Clicks");
                        cs.SetField("Clicks", (clicks + 1).ToString());
                        encodedLink = cs.GetText("Link");
                        nonEncodedLink = cp.Utils.DecodeResponseVariable(encodedLink);
                        cp.Response.Redirect(nonEncodedLink);
                        exitWithoutBanner = true;
                    }
                    cs.Close();
                }
                if (!exitWithoutBanner) {
                    //==========================================================================
                    // get next banner
                    //==========================================================================
                    string sQLNow = cp.Db.EncodeSQLDate(DateTime.Now);
                    bool isLinkAuthoring = cp.User.IsEditingAnything;
                    string bannerCriteria = "";
                    if (campaignID != 0) {
                        if (isLinkAuthoring) {
                            bannerCriteria = getBannerCriteria(cp, cs, campaignID, false);
                        }
                        else {
                            // Campaign Name given no authoring - select a banner from the campaign
                            bannerCriteria = string.Format("((ClicksMax is null)OR(Clicks<ClicksMax)) " +
                                "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" +
                                "AND((DateExpires is null)OR(DateExpires>{0})){1}", sQLNow, getBannerCriteria(cp, cs, campaignID, true));
                        }
                    }
                    else {
                        //  no Campaign given, get any banner from any campaign
                        bannerCriteria = string.Format("((ClicksMax is null)OR(Clicks<ClicksMax))" +
                                                       "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" +
                                                       "AND((DateExpires is null)OR(DateExpires>{0}))", sQLNow);

                    }
                    string hiddenResponse = "";
                    if (cs.Open("Banners", bannerCriteria, "LastViewDate", true, string.Empty, 10, 1)) {
                        bannerID = cs.GetInteger("ID");
                        dateExpires = cs.GetDate("DateExpires");
                        bannerTitle = cs.GetText("caption");
                        string link = cs.GetText("Link");
                        bool newWindow = cs.GetBoolean("NewWindow");
                        //
                        if (!string.IsNullOrEmpty(link)) {
                            string qS = cp.Doc.RefreshQueryString;
                            qS = cp.Utils.ModifyQueryString(qS, "bannerid", bannerID.ToString(), true);
                            html += string.Format("<a href=\"{0}?{1}\"", cp.Request.Page, qS);
                            if (newWindow) {
                                html += " target=\"_blank\"";
                            }
                            html += ">";
                        }

                        // ----- Add banner
                        nonEncodedLink = cp.Request.Protocol + cp.Request.Host + cp.Site.FilePath + cs.GetText("ImageFilename");
                        encodedLink = cp.Utils.EncodeHTML(nonEncodedLink);
                        html += string.Format("<IMG border=\"0\" src=\"{0}\"", encodedLink);
                        int width = cs.GetInteger("Width");
                        if (width != 0) {
                            html += string.Format(" width=\"{0}\"", width);
                        }
                        int height = cs.GetInteger("Height");
                        if (height != 0) {
                            html += string.Format(" height=\"{0}\"", height);
                        }
                        string align = cs.GetText("Align");
                        if (!string.IsNullOrEmpty(align)) {
                            html += string.Format(" align=\"{0}\"", align);
                        }

                        html += string.Format(" alt=\"{0}\" title=\"{0}\">", cp.Utils.EncodeHTML(bannerTitle));

                        if (!string.IsNullOrEmpty(link)) {
                            html += "</a>";
                        }

                        string bannerName = cs.GetText("name");
                        int viewings = cs.GetInteger("Viewings");
                        if (!string.IsNullOrEmpty(bannerName)) {
                            int clicks = cs.GetInteger("Clicks");
                            int clicksMax = cs.GetInteger("ClicksMax");
                            int viewingsMax = cs.GetInteger("ViewingsMax");
                            if (clicksMax != 0 && clicks > clicksMax) {
                                hiddenResponse = string.Format("<div style=\"clear:both;\">[Administrator, Clicks Max have been met for this banner: {0}]</div>", bannerName);
                            } else if (viewingsMax != 0 && viewings > viewingsMax) {
                                hiddenResponse = string.Format("<div style=\"clear:both;\">[Administrator, Viewings Max have been met for this banner: {0}]</div>", bannerName);
                            } else if ((dateExpires > dateMinValue) && (dateExpires < DateTime.Now)) {
                                hiddenResponse = string.Format("<div style=\"clear:both;\">[Administrator, this banner has expired: {0}]</div>", bannerName);
                            }
                        }
                        if (!isLinkAuthoring) {
                            // not authoring and banner found - count viewing and update lastviewdate
                            // this means in LinkAuthoring, banner will not rotate
                            //  Also add a viewing record for the banner so we can track individual viewing dates
                            cs.SetField("Viewings", (viewings + 1).ToString());
                            cs.SetField("LastViewDate", DateTime.Now.ToString());
                            if (cs.Insert("Banner Viewings")) {
                                cs.SetField("BannerID", bannerID.ToString());
                            }
                        }
                    }
                    cs.Close();
                    //
                    // administrator - add a banner
                    //
                    if (isLinkAuthoring) {
                        // authoring - show hiddenresponse and do not count viewings or lastviewdate
                        html += hiddenResponse;
                        string addLink = cp.Content.GetAddLink("banners", "campaign=" + campaignID, false, true);
                        html += string.Format("<div style=\"clear:both;\">{0}</div>", addLink);
                    }
                    html = cp.Content.GetEditWrapper(html, "banners", bannerID);
                }

            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex, "Unexpected trap in execute");
                return string.Empty;
            }
            return html;
        }

        private string getBannerCriteria(CPBaseClass cp, CPCSBaseClass cs, long campaignID, bool andCriteria) {
            string stream, innerStream, criteria;
            stream = innerStream = criteria = string.Empty;
            try {
                if (campaignID != 0) {
                    criteria = string.Format("BannerCampaignID = {0}", cp.Db.EncodeSQLNumber(campaignID));
                }
                if (andCriteria) {
                    stream = "AND";
                }
                if (cs.Open(contentNameBannerRules, criteria, "BannerID")) {
                    do {
                        if (innerStream != "") {
                            innerStream += ",";
                        }
                        innerStream += cs.GetInteger("BannerID").ToString();
                        cs.GoNext();

                    } while (cs.OK());
                }
                else {
                    innerStream = "0";
                }

                cs.Close();

                stream += string.Format("(ID IN({0}))", innerStream);

            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex, "Unexpected trap in getBannerCriteria");
                return string.Empty;
            }
            return stream;

        }
    }
}