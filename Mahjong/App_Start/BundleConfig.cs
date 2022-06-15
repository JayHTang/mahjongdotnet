using System.Web;
using System.Web.Optimization;

namespace Mahjong
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/styles").Include(
                      "~/Content/css/bootstrap.css",
                      "~/Content/css/site.css",
                      "~/Content/PagedList.css"));

            BundleTable.EnableOptimizations = true;

        }
    }

    public class RewriteUrlTransform : IItemTransform
    {
        CssRewriteUrlTransform tran = new CssRewriteUrlTransform();
        public string Process(string includedVirtualPath, string input)
        {
            var input1 = tran.Process(includedVirtualPath, input);
            var basePath = VirtualPathUtility.ToAbsolute("~");
            input1 = input1.Replace("url(/Content", string.Format("url({0}/Content", basePath));
            return input1;
        }
    }
}
