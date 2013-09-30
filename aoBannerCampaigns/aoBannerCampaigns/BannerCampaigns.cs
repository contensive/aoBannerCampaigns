using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contensive.BaseClasses;

namespace aoBannerCampaigns
{
    public class BannerCampaigns : AddonBaseClass
    {
        CPBaseClass cpBaseClass;
        CPCSBaseClass cs;
        public override object Execute(CPBaseClass cpBaseClass)
        {
            try
            {
                cpBaseClass = cpBaseClass;
                CPCSBaseClass cs = cpBaseClass.CSNew();
                init(cs);
                optionString = string.Empty;
                return getContent(optionString);
            }
            catch (Exception ex)
            {
                // write the line which is written in catch block in existing files.
                return ex.Message;
            }
        }

        private void init(CPCSBaseClass cs)
        {
            try
            {
                this.cs = cs;
            }
            catch (Exception ex)
            {
            }

        }

        private string getContent(string optionString)
        {
            methodName = "Execute";
            string stringValue = string.Empty;
            string html = string.Empty;
            try
            {
                campaignID = cpBaseClass.Utils.EncodeInteger(cpBaseClass.Doc.GetText("Campaign", optionString));
                if (campaignID == 0)
                {
                    campaignID = cpBaseClass.Utils.EncodeInteger(cpBaseClass.Doc.GetText("Campaign", optionString));
                }

                //============================================================================================
                //                 Process Banners         
                //============================================================================================

                bool ok;
                bannerID = cpBaseClass.Doc.GetInteger("BannerID");
                if (bannerID != 0)
                {
                    ok = cs.Open("Banners", string.Format("(ID = {0})", cpBaseClass.Utils.EncodeInteger(bannerID)));
                    if (ok)
                    {
                        clicks = cpBaseClass.Doc.GetInteger("Clicks");
                        cs.SetField("Clicks", (clicks + 1).ToString());
                        encodedLink = cpBaseClass.Doc.GetText("Link");
                        nonEncodedLink = cpBaseClass.Utils.DecodeResponseVariable(encodedLink);
                        cs.Close();
                    }
                    cs.Close();
                }


                //==========================================================================
                // get next banner
                //==========================================================================

                sQLNow = cpBaseClass.Db.EncodeSQLDate(DateTime.Now);
                isLinkAuthoring = cpBaseClass.User.IsEditingAnything;

                if (campaignID != 0)
                {
                    if (isLinkAuthoring)
                    {
                        bannerCriteria = getBannerCriteria(campaignID, false);
                    }
                    else
                    {
                        // Campaign Name given no authoring - select a banner from the campaign
                        bannerCriteria = string.Format("((ClickMax is null) IR (Click<ClickMax)) " +
                            "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" +
                            "AND((DateExpires is null)OR(DateExpires>'{0}')) '{1}'", sQLNow, getBannerCriteria(campaignID, true));

                    }
                }
                else
                {
                    //  no Campaign given, get any banner from any campaign

                    bannerCriteria = string.Format("((ClicksMax is null)OR(Clicks<ClicksMax))" +
                                                   "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" +
                                                   "AND((DateExpires is null)OR(DateExpires>'{0}'))", sQLNow);
                    bannerCriteria = string.Format("((ClicksMax is null)OR(Clicks<ClicksMax))" +
                                                   "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" +
                                                   "AND((DateExpires is null)OR(DateExpires> '{0}'))'{1}'",
                                                   sQLNow, getBannerCriteria(campaignID, true));

                }
                ok = cs.Open("Banners", bannerCriteria, "LastViewDate", false, string.Empty, 10, 1);
                if (ok)
                {
                    //if (campaignID != 0)
                    //{
                    //    // To ask corresponding method of GetAdminHintWrapper in new API
                    //    stringValue = "Main.GetAdminHintWrapper(There are no banners available within the given campaign.)";
                    //}
                    //else
                    //{
                    //    stringValue = "Main.GetAdminHintWrapper(" +
                    //                  "No campaign was selected so all banners will display, but no banner was found." +
                    //                  "<ul><li>To add a banner, turn on advanced edit and click Add a Banner" +
                    //                  "<li>To add a campaign, turn on advanced edit and click Add a Campaign" +
                    //                  "<li>To select a campaign to display here, turn on advanced edit and click the" +
                    //                  "icon above this text to edit setting for this instance.</ul>)";
                    //}

                }
                else
                {
                    bannerID = cs.GetInteger("ID");
                    link = cs.GetText("Link");
                    newWindow = cs.GetBoolean("NewWindow");
                    height = cs.GetInteger("Height");
                    width = cs.GetInteger("Width");
                    align = cs.GetText("Align");
                    clicks = cs.GetInteger("Clicks");
                    clicksMax = cs.GetInteger("ClicksMax");
                    viewings = cs.GetInteger("ViewingsMax");
                    dateExpires = cs.GetDate("DateExpires");
                    qS = cpBaseClass.Request.QueryString;

                    if (isLinkAuthoring)
                    {
                        stringValue += string.Format("<div>{0}</div>", cs.GetEditLink(true));
                    }

                    if (true)
                    {
                        if (!string.IsNullOrEmpty(link))
                        {
                            qS = cpBaseClass.Utils.ModifyQueryString(qS, "bannerid", bannerID.ToString(), true);
                            html += string.Format("<a href='{0}'?'{1}'", cpBaseClass.Request.Page, qS);
                            if (newWindow)
                            {
                                html += "target='_blank'";
                            }
                            html += "/>";
                        }

                        // ----- Add banner

                        nonEncodedLink = cpBaseClass.Request.Protocol + cpBaseClass.Site.FilePath + cs.GetText("ImageFilename");
                        encodedLink = cpBaseClass.Utils.EncodeHTML(nonEncodedLink);
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
                    }
                    else
                    {
                        html += string.Format("<div class='BannerText'>{0}</div>", copy);
                    }

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
                    csOk = cs.Insert("Banner Viewings");
                    if (csOk)
                    {
                        cs.SetField("BannerID", bannerID.ToString());
                    }
                }
                cs.Close();

            }
            catch (Exception ex)
            {
                // Place the code which is written at this place in existing file.
                return string.Empty;
            }


            return html;
        }

        private string getBannerCriteria(long campaignID, bool andCriteria)
        {
            string stream, innerStream, criteria;
            stream = innerStream = criteria = string.Empty;
            try
            {
                if (campaignID != 0)
                {
                    criteria = string.Format("BannerCampaignID = {0}", cpBaseClass.Db.EncodeSQLNumber(campaignID));
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
                        innerStream += cpBaseClass.Doc.GetText("BannerID");
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
                stream = string.Empty;
            }

            return stream;

        }

        private string optionString, link, methodName, align, sQLNow, qS, bannerName, idCriteria;
        private string bannerCriteria, hiddenResponse, encodedLink, nonEncodedLink, copy, sql;
        private long cS, height, width, csBanners, bannerID, campaignID;
        private long clicks, clicksMax, viewings, viewingsMax, contentID, csBanner;
        private bool newWindow, isLinkAuthoring, csOk;
        private DateTime dateExpires;
        private string result;
        private const string contentNameBannerRules = "Banner Campaign Rules";



    }
}
