using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Validation;

namespace Xpand.ExpressApp.SystemModule {
    public interface IModelClassEnableFKViolations {
        [Category("eXpand")]
        [Description("Does not allow delete of an object that has referenced objects")]
        bool EnableFKViolations { get; set; }
    }
    [ModelInterfaceImplementor(typeof(IModelClassEnableFKViolations), "ModelClass")]
    public interface IModelViewEnableFKViolations : IModelClassEnableFKViolations {
    }

    public class FKViolationViewController : ViewController<ObjectView>, IModelExtender {
        void IModelExtender.ExtendModelInterfaces(ModelInterfaceExtenders extenders) {
            extenders.Add<IModelClass, IModelClassEnableFKViolations>();
            extenders.Add<IModelObjectView, IModelViewEnableFKViolations>();
        }

        protected override void OnActivated() {
            base.OnActivated();
            if (((IModelViewEnableFKViolations)View.Model).EnableFKViolations)
                ObjectSpace.ObjectDeleting += ObjectSpace_OnObjectDeleting;
        }
        protected override void OnDeactivated() {
            base.OnDeactivated();
            if (((IModelViewEnableFKViolations)View.Model).EnableFKViolations)
                ObjectSpace.ObjectDeleting -= ObjectSpace_OnObjectDeleting;
        }
        private void ObjectSpace_OnObjectDeleting(object sender, ObjectsManipulatingEventArgs e) {
            foreach (var o in e.Objects) {
                var count = ((XPObjectSpace)ObjectSpace).Session.CollectReferencingObjects(o).Count;
                if (count > 0) {
                    var result = new RuleSetValidationResult();
                    var messageTemplate = "Cannot be deleted " + count + " references found";
                    result.AddResult(new RuleSetValidationResultItem(o, ContextIdentifier.Delete, null,
                                                                     new RuleValidationResult(null, this, ValidationState.Invalid,
                                                                                              messageTemplate)));
                    throw new ValidationException(messageTemplate, result);
                }
            }
        }
    }
}