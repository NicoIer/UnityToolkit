using System;
using System.Collections.Generic;

namespace UnityToolkit
{
    public interface IModel
    {
        void Init();
    }

    public class ModelCenter
    {
        private readonly Dictionary<Type, IModel> _models = new Dictionary<Type, IModel>();

        public void Register<T>() where T : IModel, new()
        {
            T model = new T();
            Register(model);
        }

        public void Register<T>(T model) where T : IModel
        {
            var type = typeof(T);
            if (_models.ContainsKey(type))
            {
                throw new ArgumentException($"ModelCenter.Register<{type}>() failed, model already registered");
                return;
            }

            _models[type] = model;
            model.Init();
        }

        public TModel Get<TModel>() where TModel : IModel
        {
            var type = typeof(TModel);
            if (_models.TryGetValue(type, out var model))
            {
                return (TModel)model;
            }

            return default(TModel);
        }
    }
}