// MVVM

using System;
using UnityEngine;

namespace Nico
{
    public interface IView<TViewModel>
    {
        public TViewModel Model { get; set; }
    }
    
    public interface IViewModel<TView> : ICanGetModel where TView : class
    {
        public TView View { get; set; }
        public void OnBind(TView view);
        public void OnUnbind(TView view);
    }
    

    public interface IModel
    {
        public void OnRegister();
        public void OnSave();
    }

    public interface ICanGetModel
    {
        public TModel GetModel<TModel>() where TModel : class, IModel, new() => ModelManager.Get<TModel>();
    }
}