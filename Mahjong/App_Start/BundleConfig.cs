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

            bundles.Add(new ScriptBundle("~/bundles/jquery-ui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/styles").Include(
                      "~/Content/css/bootstrap.css",
                      "~/Content/css/site.css",
                      "~/Content/css/bootstrap-datepicker3.standalone.css",
                      "~/Content/PagedList.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/jquery-ui").Include(
                      "~/Content/themes/base/jquery-ui.min.css"));

            bundles.Add(new ScriptBundle("~/bundles/highcharts").Include(
                      "~/Scripts/highcharts/7.1.2/highcharts.js",
                      "~/Scripts/highcharts/7.1.2/highcharts-more.js",
                      "~/Scripts/highcharts/7.1.2/modules/data.js",
                      "~/Scripts/highcharts/7.1.2/modules/exporting.js",
                      "~/Scripts/highcharts/7.1.2/modules/drilldown.js"));

            bundles.Add(new ScriptBundle("~/bundles/datepicker").Include(
                      "~/Scripts/bootstrap-datepicker.js"));

            bundles.Add(new StyleBundle("~/Content/gijgo/combined/gijgo").Include(
                      "~/Content/gijgo/combined/gijgo.css"));

            bundles.Add(new ScriptBundle("~/bundles/timepicker").Include(
                      "~/Scripts/gijgo/combined/gijgo.js"/*,
                      "~/Scripts/gijgo/modular/core.js",
                      "~/Scripts/gijgo/modular/datepicker.js",
                      "~/Scripts/gijgo/modular/timepicker.js"*/));

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
