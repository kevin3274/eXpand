﻿using System;
using System.Collections;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Validation;
using DevExpress.ExpressApp.Validation.AllContextsView;
using DevExpress.Persistent.Validation;

namespace Xpand.ExpressApp.Validation {
    public class ActionExecuteValidationController : ObjectViewController {
        public event EventHandler<CustomGetAggregatedObjectsToValidateEventArgs> CustomGetAggregatedObjectsToValidate;
        public event EventHandler<NeedToValidateObjectEventArgs> NeedToValidateObject;
        public event EventHandler<ContextValidatingEventArgs> ContextValidating;

        protected virtual void OnContextValidating(ContextValidatingEventArgs args) {
            if (ContextValidating != null) {
                ContextValidating(this, args);
            }
        }

        private void CustomizeDeleteValidationException(ValidationCompletedEventArgs args) {
            args.Exception.MessageHeader = ValidationExceptionLocalizer.GetExceptionMessage(ValidationExceptionId.DeleteErrorMessageHeader);
            args.Exception.ObjectHeaderFormat = ValidationExceptionLocalizer.GetExceptionMessage(ValidationExceptionId.DeleteErrorMessageObjectFormat);
        }

        protected override void OnDeactivated() {
            base.OnDeactivated();
            foreach (var controller in Frame.Controllers) {
                foreach (var action in controller.Actions) {
                    action.Executed -= ActionOnExecuted;
                }
            }
        }

        protected override void OnActivated() {
            base.OnActivated();
            foreach (var controller in Frame.Controllers) {
                foreach (var action in controller.Actions) {
                    action.Executed += ActionOnExecuted;
                }
            }
        }

        void ActionOnExecuted(object sender, ActionBaseEventArgs actionBaseEventArgs) {
            if (View.ObjectTypeInfo.Type != typeof(ValidationResults)) {
                ValidationTargetObjectSelector deleteSelector = new ActionExecuteContextTargetObjectSelector();
                SubscribeSelectorEvents(deleteSelector);
                var selectedObjects = ((SimpleActionExecuteEventArgs)actionBaseEventArgs).SelectedObjects;
                var context = actionBaseEventArgs.Action.Id;
                var contextValidatingEventArgs = new ContextValidatingEventArgs(context, new ArrayList(selectedObjects));
                OnContextValidating(contextValidatingEventArgs);
                if (CanAccessDeletedObjects(context))
                    Validator.RuleSet.ValidateAll(contextValidatingEventArgs.TargetObjects, context, CustomizeDeleteValidationException);
            }
        }

        bool CanAccessDeletedObjects(string context) {
            return !(!ObjectSpace.IsDeletionDefferedType(View.ObjectTypeInfo.Type) && context == "Delete");
        }

        private void OnSelectorCustomGetAggregatedObjectsToValidate(object sender, CustomGetAggregatedObjectsToValidateEventArgs args) {
            if (CustomGetAggregatedObjectsToValidate != null) {
                CustomGetAggregatedObjectsToValidate(this, args);
            }
        }

        private void OnSelectorNeedToValidateObject(object sender, NeedToValidateObjectEventArgs args) {
            if (NeedToValidateObject != null) {
                NeedToValidateObject(this, args);
            }
        }

        private void SubscribeSelectorEvents(ValidationTargetObjectSelector selector) {
            selector.CustomNeedToValidateObject += OnSelectorNeedToValidateObject;
            selector.CustomGetAggregatedObjectsToValidate += OnSelectorCustomGetAggregatedObjectsToValidate;
        }

    }
    public class ActionExecuteContextTargetObjectSelector : ValidationTargetObjectSelector {

        protected override bool NeedToValidateObject(IObjectSpace objectSpace, object targetObject) {
            return true;
        }
    }

}
