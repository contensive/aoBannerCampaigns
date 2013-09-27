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
            campaignID = Convert.ToInt32(cpBaseClass.Doc.GetText("Campaign", optionString));
            if (campaignID == 0)
            {
                campaignID = Convert.ToInt32(cpBaseClass.Doc.GetText("Campaign", optionString));
            }
            bool ok ;
            bannerID = cpBaseClass.Doc.GetInteger("BannerID");
            if (bannerID != 0)
            {
                ok = cs.Open("Banners", string.Format("(ID = {0})", Convert.ToInt32(bannerID)));
                if (ok)
                {
                    clicks = cpBaseClass.Doc.GetInteger("Clicks");
                    //cpBaseClass.Doc.SetProperty(   Need to know about Main.SetCS(CSBanners, "Clicks", Clicks + 1)
                    encodedLink = cpBaseClass.Doc.GetText("Link");
                    nonEncodedLink = 
                }
            }

            //============================================================================================
            //                 Process Banners         
            //============================================================================================

            return "output";
        }

        private string optionString, link, methodName, align, sQLNow, qS, bannerName, idCriteria;
        private string bannerCriteria, hiddenResponse, encodedLink, nonEncodedLink, copy, sql;
        private long cS, height, width, csBanners, bannerID, campaignID;
        private long clicks, clicksMax, viewings, viewingsMax, contentID, csBanner;
        private bool newWindow, isLinkAuthoring;
        private DateTime dateExpires;


    }
}
