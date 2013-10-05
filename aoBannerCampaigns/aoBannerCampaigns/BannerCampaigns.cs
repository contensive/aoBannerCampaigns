using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contensive.BaseClasses;

namespace aoBannerCampaigns
{
    public class BannerCampaigns : AddonBaseClass
    {
        public override object Execute(CPBaseClass cp)
        {
            try
            {
                return getContent(cp);
            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }
        }

        private string getContent(CPBaseClass cp)
        {
            string stringValue = string.Empty;
            string html = "starting getContent()";

            try
            {
                CPCSBaseClass cs = cp.CSNew();
                campaignID = cp.Doc.GetInteger("Campaign");
                if (campaignID == 0)
                {
                    campaignID = cp.Doc.GetInteger("CampaignID");
                }
                //============================================================================================
                //                 Process Banners         
                //============================================================================================
                bannerID = cp.Doc.GetInteger("BannerID");
                if (bannerID != 0)
                {
                    if (cs.Open("Banners", string.Format("(ID = {0})", cp.Db.EncodeSQLNumber(bannerID))))
                    {
                        clicks = cs.GetInteger("Clicks");
                        cs.SetField("Clicks", (clicks + 1).ToString());
                        encodedLink = cs.GetText("Link");
                        nonEncodedLink = cp.Utils.DecodeResponseVariable(encodedLink);
                    }
                    cs.Close();
                }
                //==========================================================================
                // get next banner
                //==========================================================================
                sQLNow = cp.Db.EncodeSQLDate(DateTime.Now);
                isLinkAuthoring = cp.User.IsEditingAnything;
                if (campaignID != 0)
                {
                    if (isLinkAuthoring)
                    {
                        bannerCriteria = getBannerCriteria(cp, cs, campaignID, false);
                    }
                    else
                    {
                        // Campaign Name given no authoring - select a banner from the campaign
                        bannerCriteria = string.Format("((ClickMax is null) IR (Click<ClickMax)) " +
                            "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" +
                            "AND((DateExpires is null)OR(DateExpires>'{0}')) '{1}'", sQLNow, getBannerCriteria(cp, cs, campaignID, true));
                    }
                }
                else
                {
                    //  no Campaign given, get any banner from any campaign
                    bannerCriteria = string.Format("((ClicksMax is null)OR(Clicks<ClicksMax))" +
                                                   "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" +
                                                   "AND((DateExpires is null)OR(DateExpires>'{0}'))", sQLNow);

                }
                if (cs.Open("Banners", bannerCriteria, "LastViewDate", true, string.Empty, 10, 1))
                {
                    bannerID = cs.GetInteger("ID");
                    link = cs.GetText("Link");
                    newWindow = cs.GetBoolean("NewWindow");
                    height = cs.GetInteger("Height");
                    width = cs.GetInteger("Width");
                    align = cs.GetText("Align");
                    clicks = cs.GetInteger("Clicks");
                    clicksMax = cs.GetInteger("ClicksMax");
                    viewings = cs.GetInteger("Viewings");
                    viewingsMax = cs.GetInteger("ViewingsMax");
                    dateExpires = cs.GetDate("DateExpires");
                    qS = cp.Request.QueryString;

                    if (isLinkAuthoring)
                    {
                        stringValue += string.Format("<div>{0}</div>", cs.GetEditLink(true));
                    }

                    if (!string.IsNullOrEmpty(link))
                    {
                        qS = cp.Utils.ModifyQueryString(qS, "bannerid", bannerID.ToString(), true);
                        html += string.Format("<a href='{0}'?'{1}'", cp.Request.Page, qS);
                        if (newWindow)
                        {
                            html += "target='_blank'";
                        }
                        html += "/>";
                    }

                    // ----- Add banner
                    nonEncodedLink = cp.Request.Protocol + cp.Site.FilePath + cs.GetText("ImageFilename");
                    encodedLink = cp.Utils.EncodeHTML(nonEncodedLink);
                    html += string.Format("<IMG border='0' src='{0}'", encodedLink);
                    if (width != 0)
                    {
                        html += string.Format("width='{0}'", width);
                    }
                    if (height != 0)
                    {
                        html += string.Format("height='{0}'", height);
                    }
                    if (!string.IsNullOrEmpty(align))
                    {
                        html += string.Format("align='{1}'", align);
                    }

                    if (!string.IsNullOrEmpty(link))
                    {
                        html += "</a>";
                    }

                    html = string.Format("alt='{0}'", align);


                    if (!string.IsNullOrEmpty(bannerName))
                    {
                        if (clicksMax != 0 && clicks > clicksMax)
                        {
                            hiddenResponse = string.Format("[Banner hidden (Clicks Max Met): {0}]", bannerName);
                        }
                        else if (viewings != 0 && viewings > viewingsMax)
                        {
                            hiddenResponse = string.Format("[Banner hidden (Viewings Max Met): {0}]", bannerName);
                        }
                        else if (dateExpires < DateTime.Now)
                        {
                            hiddenResponse = string.Format("[Banner hidden (Expired): {0}]", bannerName);
                        }
                    }
                }

                if (isLinkAuthoring)
                {
                    // authoring - show hiddenresponse and do not count viewings or lastviewdate
                    html += hiddenResponse;
                    html += string.Format("<div>{0}nbsp;Add a banner</div>", cs.GetAddLink("Banners,campaign=" + campaignID));
                }
                else
                {
                    // not authoring and banner found - count viewing and update lastviewdate
                    // this means in LinkAuthoring, banner will not rotate
                    //  Also add a viewing record for the banner so we can track individual viewing dates
                    cs.SetField("Viewings", (viewings + 1).ToString());
                    cs.SetField("LastViewDate", DateTime.Now.ToString());
                    if (cs.Insert("Banner Viewings"))
                    {
                        cs.SetField("BannerID", bannerID.ToString());
                    }
                }
                cs.Close();

            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }
            return html;
        }

        private string getBannerCriteria(CPBaseClass cp, CPCSBaseClass cs, long campaignID, bool andCriteria)
        {
            string stream, innerStream, criteria;
            stream = innerStream = criteria = string.Empty;
            try
            {
                if (campaignID != 0)
                {
                    criteria = string.Format("BannerCampaignID = {0}", cp.Db.EncodeSQLNumber(campaignID));
                }
                if (andCriteria)
                {
                    stream = "AND";
                }
                if (cs.Open(contentNameBannerRules, criteria, "BannerID"))
                {
                    do
                    {
                        if (innerStream != "")
                        {
                            innerStream += ",";
                        }
                        innerStream += cs.GetInteger("BannerID").ToString();
                        cs.GoNext();

                    } while (cs.OK());
                }
                else
                {
                    innerStream = "0";
                }

                cs.Close();

                stream += string.Format("(ID IN('{0})')", innerStream);

            }
            catch (Exception ex)
            {
                cp.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }

            return stream;

        }

        private string link, align, sQLNow, qS, bannerName;
        private string bannerCriteria, hiddenResponse, encodedLink, nonEncodedLink;
        private long height, width, bannerID, campaignID, viewingsMax;
        private long clicks, clicksMax, viewings;
        private bool newWindow, isLinkAuthoring;
        private DateTime dateExpires;
        private const string contentNameBannerRules = "Banner Campaign Rules";



    }
}
