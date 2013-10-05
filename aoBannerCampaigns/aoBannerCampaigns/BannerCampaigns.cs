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
        CPCSBaseClass cs;           // JK - this is only used within your routines, so consider declaring it within the routine to prevent memory leak. (but this will work)
        // JK - consider using "CPBaseClass cp" instead of "CPBaseClass cpBaseClass"
        public override object Execute(CPBaseClass cpBaseClass)
        {
            try
            {
                // JK - consider removing this - cpBaseClass can never be null. If somehow it is null, it will throw an error the first time you use it and that would be fine.
                if (cpBaseClass == null)
                {
                    return string.Empty;
                }
                // JK - in the old api this was necessary because there were two calls (init and getcontent). In the new api there is only execute() so you do not have to store cp in the class, just pass it as an argument to subrountines that need it.
                this.cpBaseClass = cpBaseClass;
                // JK - I commented this out just to emphasize that is it not necessary.
                // JK - init() and getContent() were publics in the old system that have no value here. They are not needed. 
                // JK - cs is only used inside your getContent routine, so declare it there and initialize it there. (but this will work)
                CPCSBaseClass cs = cpBaseClass.CSNew();
                // JK - not needed - init(cs);
                // JK - not needed - optionString = cpBaseClass.Doc.GetText("");
                return getContent( /* optionString */);
            }
            catch (Exception ex)
            {
                cpBaseClass.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }
        }

        // JK - not needed - private void init(CPCSBaseClass cs)
        // JK - not needed - {
        // JK - not needed -     try
        // JK - not needed -     {
        // JK - not needed -         this.cs = cs;
        // JK - not needed -     }
        // JK - not needed -     catch (Exception ex)
        // JK - not needed -     {
        // JK - not needed -         cpBaseClass.Site.ErrorReport(ex, "Unexpected trap");
        // JK - not needed -     }
        // JK - not needed - }
        //
        // since getContent(coptionString) is no longer needed, you could put this code
        // right into execute(). Also, the optionString argument is not needed
        //
        private string getContent(/*string optionString*/)
        {
            string stringValue = string.Empty;
            string html = "starting getContent()";
            if (cpBaseClass == null || cs == null)
            {
                return string.Empty;
            }
            try
            {
                // JK - the second argument of getText() is defaultValue, which is returned by getText() if Campaign is missing
                // JK - calling cp.doc.getInteger("campaign") does the same thing as cp.utils.encodeInteger(p.doc.getText("campaign"))
                campaignID = cpBaseClass.Utils.EncodeInteger(cpBaseClass.Doc.GetText("Campaign" /*,  optionString */));
                if (campaignID == 0)
                {
                    // JK - the argument here should be "CampaignID", not "Campaign"
                    // JK - again, you can use cp.doc.getInteger()
                    campaignID = cpBaseClass.Utils.EncodeInteger(cpBaseClass.Doc.GetText("Campaign", optionString));
                }

                //============================================================================================
                //                 Process Banners         
                // JK - if bannerID is not 0, then someone clicked on a banner and you will redirect to the link
                //============================================================================================

                bannerID = cpBaseClass.Doc.GetInteger("BannerID");
                if (bannerID != 0)
                {
                    // JK - should be cpBaseClass.Db.EncodeSQLNumber(bannerId), not cpBaseClass.Utils.EncodeInteger(bannerID)
                    if (cs.Open("Banners", string.Format("(ID = {0})", cpBaseClass.Utils.EncodeInteger(bannerID))))
                    {
                        // JK - this should be cs.GetInteger( "Clicks" )
                        clicks = cpBaseClass.Doc.GetInteger("Clicks");
                        cs.SetField("Clicks", (clicks + 1).ToString() );
                        // JK - this should be cs.GetText( "Link" )
                        encodedLink = cpBaseClass.Doc.GetText("Link");
                        nonEncodedLink = cpBaseClass.Utils.DecodeResponseVariable(encodedLink);
                        cs.Close();
                        // JK - the old code exited here with a redirect to nonEncodedLink, cpBaseClass.Response.Redirect(nonEncodedLink);
                        // JK - if this redirect is taken, you can exit the routine any way you choose.
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
                        bannerCriteria = getBannerCriteria( campaignID,  false);
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

                }
                if (cs.Open( "Banners", bannerCriteria, "LastViewDate", true, string.Empty, 10, 1))
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
                    //copy = cs.getcs("");
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
                    if (cs.Insert("Banner Viewings"))
                    {
                        cs.SetField("BannerID", bannerID.ToString());
                    }
                }
                cs.Close();

            }
            catch (Exception ex)
            {
                cpBaseClass.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }
            return html;
        }

        private string getBannerCriteria(long campaignID, bool andCriteria)
        {
            string stream, innerStream, criteria;
            stream = innerStream = criteria = string.Empty;

            // JK - dont need to check the global cs object either.
            if (cpBaseClass == null || cs == null)
            {
                return string.Empty;
            }
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
                        // JK should be cs.getInteger( "bannerid" ).toString()
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
                cpBaseClass.Site.ErrorReport(ex, "Unexpected trap");
                return string.Empty;
            }

            return stream;

        }

        private string optionString, link, align, sQLNow, qS, bannerName;
        private string bannerCriteria, hiddenResponse, encodedLink, nonEncodedLink, copy;
        private long height, width, bannerID, campaignID, viewingsMax;
        private long clicks, clicksMax, viewings;
        private bool newWindow, isLinkAuthoring;
        private DateTime dateExpires;
        private const string contentNameBannerRules = "Banner Campaign Rules";
    


    }
}
