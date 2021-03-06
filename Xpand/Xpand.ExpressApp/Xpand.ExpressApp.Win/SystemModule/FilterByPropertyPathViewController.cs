using System.Drawing;
using System.Windows.Forms;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Win.SystemModule;
using DevExpress.Utils.Frames;
using View = DevExpress.ExpressApp.View;
using System.Linq;

namespace Xpand.ExpressApp.Win.SystemModule {
    public class FilterByPropertyPathViewController : ExpressApp.SystemModule.FilterByPropertyPathViewController {
        protected override string GetActiveFilter(IModelListView modelListView) {
            return ((IModelListViewWin)modelListView).ActiveFilterString;
        }

        protected override void SetActiveFilter(IModelListView modelListView, string filter) {
            ((IModelListViewWin)modelListView).ActiveFilterString = filter;
        }

        protected override void AddFilterPanel(string text, object viewSiteControl) {
            Control.ControlCollection controlCollection = ((Control)viewSiteControl).Controls;
            if (string.IsNullOrEmpty(text)) {
                FilterPanel filterPanel = controlCollection.OfType<FilterPanel>().FirstOrDefault();
                if (filterPanel != null) controlCollection.Remove(filterPanel);
            } else {
                var filterPanel = new FilterPanel {
                    BackColor = Color.LightGoldenrodYellow,
                    Dock = DockStyle.Bottom,
                    MaxRows = 25,
                    TabIndex = 0,
                    TabStop = false,
                    MinimumSize = new Size(350, 33),
                    Text = text
                };

                Control.ControlCollection collection = controlCollection;
                collection.Add(filterPanel);
            }
        }

        protected override void SynchronizeInfo(View view) {
            view.SaveModel();
        }

        #region Nested type: FilterPanel
        public class FilterPanel : NotePanel8_1 {
        }
        #endregion
    }
}