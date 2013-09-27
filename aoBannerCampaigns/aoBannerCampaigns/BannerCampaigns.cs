using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contensive.BaseClasses;

namespace aoBannerCampaigns
{
    public class BannerCampaigns : AddonBaseClass
    {
        public override object Execute(CPBaseClass cpBaseClass)
        {
            methodName = "Execute";
            CPCSBaseClass cs = cpBaseClass.CSNew();
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
                    bannerCriteria = getBannerCriteria(cpBaseClass, cs, campaignID, false);
                }
                else
                {
                    // Campaign Name given no authoring - select a banner from the campaign
                    bannerCriteria = string.Format("((ClickMax is null) IR (Click<ClickMax)) " +
                        "AND((ViewingsMax is null)OR(Viewings<ViewingsMax))" +
                        "AND((DateExpires is null)OR(DateExpires>'{0}')) '{1}'", sQLNow, getBannerCriteria(cpBaseClass, cs, campaignID, true));

                }
            }


            return "output";
        }

        private string getBannerCriteria(CPBaseClass cp, CPCSBaseClass csObj, long campaignID, bool andCriteria)
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
                bool ok = csObj.Open(contentNameBannerRules, criteria, "BannerId");
                if (ok)
                {
                    if (innerStream != "")
                    {
                        innerStream += ",";
                    }
                    innerStream += cp.Doc.GetText("BannerID");
                }
                innerStream = "0";
                csObj.Close();

                stream += string.Format("(ID IN('{0})')");

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
        private bool newWindow, isLinkAuthoring;
        private DateTime dateExpires;
        private const string contentNameBannerRules = "Banner Campaign Rules";



    }
}
