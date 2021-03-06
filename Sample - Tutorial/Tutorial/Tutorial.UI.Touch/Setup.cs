﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.Dialog.Touch;
using Cirrious.MvvmCross.Touch.Interfaces;
using Cirrious.MvvmCross.Binding.Binders;
using Cirrious.MvvmCross.Touch.Platform;
using Tutorial.Core;
using Tutorial.Core.Converters;
using Tutorial.Core.ViewModels;
using Tutorial.Core.ViewModels.Lessons;
using Tutorial.UI.Touch.Views;
using Tutorial.UI.Touch.Views.Lessons;

namespace Tutorial.UI.Touch
{
    public class Setup
        : MvxTouchDialogBindingSetup
    {
        public Setup(MvxApplicationDelegate applicationDelegate, IMvxTouchViewPresenter presenter)
            : base(applicationDelegate, presenter)
        {
        }

        #region Overrides of MvxBaseSetup

        protected override MvxApplication CreateApp()
        {
            var app = new App();
            return app;
        }

        protected override IEnumerable<Type> ValueConverterHolders
        {
            get { return new[] { typeof(Converters) }; }
        }

        #endregion
    }
}
