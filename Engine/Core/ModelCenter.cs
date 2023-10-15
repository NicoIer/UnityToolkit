using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityToolkit
{
    public abstract class ModelEvent<T> where T : ModelEvent<T>, IModel
    {
        private event Action<T> OnEvent = _ => { };

        public ICommand Register(Action<T> onEvent)
        {
            this.OnEvent += onEvent;
            return new EventUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T> onEvent)
        {
            OnEvent -= onEvent;
        }

        public void Trigger()
        {
            OnEvent(this as T);
        }
    }

    public interface IModel
    {
        // void Init();
        // void Save();
    }

    public class ModelCenter
    {
        private readonly Dictionary<Type, IModel> _models = new Dictionary<Type, IModel>();

        // private readonly Dictionary<Type, BindableProperty<IModel>> _bindable =
        //     new Dictionary<Type, BindableProperty<IModel>>();

        public T Register<T>() where T : IModel, new()
        {
            T model = new T();
            return Register(model);
        }

        public T Register<T>(T model) where T : IModel
        {
            var type = typeof(T);
            if (_models.ContainsKey(type))
            {
                throw new ArgumentException($"ModelCenter.Register<{type}>() failed, model already registered");
            }

            _models[type] = model;
            // model.Init();
            return Get<T>();
        }

        public TModel Get<TModel>() where TModel : IModel
        {
            var type = typeof(TModel);
            if (_models.TryGetValue(type, out var model))
            {
                return (TModel)model;
            }

            throw new KeyNotFoundException($"please register model<{nameof(TModel)}> first");
        }

        // public BindableProperty<TModel> GetBind<TModel>() where TModel : IModel
        // {
        //     var type = typeof(TModel);
        //     if (_bindable.TryGetValue(type, out BindableProperty<IModel> bind))
        //     {
        //         return bind as BindableProperty<TModel>;
        //     }
        //     
        //     TModel model = Get<TModel>();
        //     BindableProperty<TModel> bindableProperty = new BindableProperty<TModel>(model);
        //     _bindable.Add(type, bindableProperty as BindableProperty<IModel>);
        //     return bindableProperty;
        // }

        // public void Save<TModel>() where TModel : IModel
        // {
        //     TModel model = Get<TModel>();
        //     // model.Save();
        // }
        //
        // public void SaveAll()
        // {
        //     foreach (var model in _models.Values)
        //     {
        //         model.Save();
        //     }
        // }
    }
}