using System;
using System.Collections.Generic;
using System.Text;
using Contensive.BaseClasses;

namespace test
{
    //
    // 1) Change the namespace to the collection name
    // 2) Change this class name to the addon name
    // 3) Create a Contensive Addon record with the namespace apCollectionName.ad
    // 3) add reference to CPBase.DLL, typically installed in c:\program files\kma\contensive\
    //
    public class test : Contensive.BaseClasses.AddonBaseClass
    {
        //
        // execute method is the only public
        //
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp)
        {
            string s = "";
            string peopleList = "";
            CPCSBaseClass cs = cp.CSNew();
            CPBlockBaseClass myBlock = cp.BlockNew();

            string forminput = cp.Doc.GetText("nameOfThis");
            //
            s += "<h1>" + forminput + "</h1>";
            s += "<div id=\"target\"><input type=text name=nameOfThis value=\"\"></div>";
            s += "<div><input type=submit name=button value=Push></div>";
            s = cp.Html.Form(s);
            //
            myBlock.OpenLayout("testlayout");
            //myBlock.SetInner("#target", "This wiped out the input box!!");
            s += myBlock.GetHtml();
            //
            cs.Open("people");
            do
            {
                peopleList += cp.Html.li(cs.GetText("name"));
                cs.GoNext();
            } while (cs.OK());
            cs.Close();
            peopleList = cp.Html.ul(peopleList);
            s += peopleList;
            return s;

        }
    }
}
/// do
/// ()