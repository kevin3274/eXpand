using System;
using System.ComponentModel;
using System.Drawing;
using DevExpress.ExpressApp.Model;

namespace Xpand.ExpressApp.Thumbnail.Web {
    public interface IModelListViewThumbnailWeb : IModelListView {
        IModelThumbnailWeb ThumbnailWeb { get; }
    }

    public interface IModelThumbnailWeb : IModelNode {
        [DefaultValue("inline-block")]
        string DisplayStyle { get; set; }
        bool HideImages { get; set; }
    }
    [ToolboxBitmap(typeof(ThumbnailWebModule))]
    [ToolboxItem(true)]
    public sealed class ThumbnailWebModule : XpandModuleBase {
        public override void ExtendModelInterfaces(ModelInterfaceExtenders extenders) {
            base.ExtendModelInterfaces(extenders);
            extenders.Add<IModelListView, IModelListViewThumbnailWeb>();
        }
    }
}
